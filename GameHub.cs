using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayerData {
    public string Name { get; set; }
    public int Score { get; set; }
    public int Streak { get; set; } // Bu değer ön yüze gidip efekt seviyesini belirleyecek
    public bool IsOnFire => Streak >= 3; 
    public long FastestTimeMs { get; set; } = long.MaxValue;
    public string FastestTrackName { get; set; } = "";
    public string FastestTrackUrl { get; set; } = "";
    public string FastestCoverUrl { get; set; } = "";
}

public class GameProposal {
    public string Mode { get; set; }
    public string Value { get; set; }
    public long Timestamp { get; set; }
}

public class GameRoom {
    public string RoomCode { get; set; }
    public string HostConnectionId { get; set; }
    public ConcurrentDictionary<string, PlayerData> Players { get; set; } = new();
    public ConcurrentDictionary<string, GameProposal> Proposals { get; set; } = new();
    public ConcurrentDictionary<string, string> Votes { get; set; } = new();
    public GameProposal? ActiveSettings { get; set; }
    public bool IsPlaying { get; set; }
    public long QuestionStartTime { get; set; }
    public long GameStartTime { get; set; }
    public string CurrentCorrectAnswer { get; set; }
    public string CurrentAudioUrl { get; set; }
    public string CurrentCoverUrl { get; set; }
    public HashSet<string> WrongGuessersThisRound { get; set; } = new();
}

public class GameHub : Hub {
    private readonly ITunesService _iTunesService;
    private static readonly ConcurrentDictionary<string, GameRoom> _rooms = new();
    private static readonly ConcurrentDictionary<string, string> _userRooms = new();

    public GameHub(ITunesService iTunesService) { _iTunesService = iTunesService; }

    public async Task CreateRoom(string playerName) {
        string roomCode = GenerateRoomCode();
        var room = new GameRoom { RoomCode = roomCode, HostConnectionId = Context.ConnectionId };
        room.Players[Context.ConnectionId] = new PlayerData { Name = playerName };
        _rooms[roomCode] = room;
        _userRooms[Context.ConnectionId] = roomCode;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        await Clients.Caller.SendAsync("RoomJoined", roomCode, true); 
        await BroadcastLeaderboard(roomCode);
    }

    public async Task JoinRoom(string roomCode, string playerName) {
        roomCode = roomCode.ToUpper();
        if (_rooms.TryGetValue(roomCode, out var room) && !room.IsPlaying) {
            room.Players[Context.ConnectionId] = new PlayerData { Name = playerName };
            _userRooms[Context.ConnectionId] = roomCode;

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Caller.SendAsync("RoomJoined", roomCode, false); 
            await BroadcastLeaderboard(roomCode);
            await Clients.Group(roomCode).SendAsync("GameInfo", $"{playerName} odaya katıldı!");
        } else {
            await Clients.Caller.SendAsync("AnswerResult", false, "Oda bulunamadı veya oyun başladı!");
        }
    }

    private string GenerateRoomCode() {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task SuggestCategory(string mode, string value) {
        if (!_userRooms.TryGetValue(Context.ConnectionId, out var roomCode) || !_rooms.TryGetValue(roomCode, out var room)) return;
        
        int trackCount = await _iTunesService.GetTrackCountAsync(mode, value);
        
        int requiredTracks = room.Players.Count * 10;
        
        if (trackCount < requiredTracks) {
            await Clients.Caller.SendAsync("AnswerResult", false, $"⚠️ HATA: '{value}' için depoda {trackCount} şarkı var. (Odada {room.Players.Count} kişiyiz, en az {requiredTracks} şarkı lazım). Lütfen başka bir şey önerin!");
            return; 
        }

        room.Proposals[Context.ConnectionId] = new GameProposal { Mode = mode, Value = value, Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
        
        await Clients.Group(roomCode).SendAsync("GameInfo", $"✅ {room.Players[Context.ConnectionId].Name} bir kategori önerdi.");
        await Clients.Group(roomCode).SendAsync("ProposalCountUpdated", room.Proposals.Count, room.Players.Count);
    }

    public async Task StartGameByHost() {
        if (!_userRooms.TryGetValue(Context.ConnectionId, out var roomCode) || !_rooms.TryGetValue(roomCode, out var room)) return;
        if (room.HostConnectionId != Context.ConnectionId || room.IsPlaying) return; 

        if (room.Proposals.Count == 0) {
            await Clients.Caller.SendAsync("AnswerResult", false, "Önce kategori önerin!");
            return;
        }

        var distinctProposals = room.Proposals.Values.Select(p => p.Value.Trim().ToLower()).Distinct().ToList();
        if (distinctProposals.Count == 1) {
            room.ActiveSettings = room.Proposals.Values.First();
            await StartGameInternal(room);
        } else {
            var voteOptions = room.Proposals.Values.GroupBy(p => p.Value.Trim().ToLower()).Select(g => g.First()).ToList();
            await Clients.Group(roomCode).SendAsync("StartVoting", voteOptions);
        }
    }

    public async Task SubmitVote(string voteValue) {
        if (!_userRooms.TryGetValue(Context.ConnectionId, out var roomCode) || !_rooms.TryGetValue(roomCode, out var room)) return;
        
        room.Votes[Context.ConnectionId] = voteValue;
        if (room.Votes.Count == room.Players.Count) {
            var winnerValue = room.Votes.Values.GroupBy(v => v).OrderByDescending(g => g.Count())
                .ThenBy(g => room.Proposals.Values.First(p => p.Value.Equals(g.Key, StringComparison.OrdinalIgnoreCase)).Timestamp)
                .First().Key;

            room.ActiveSettings = room.Proposals.Values.First(p => p.Value.Equals(winnerValue, StringComparison.OrdinalIgnoreCase));
            await Clients.Group(roomCode).SendAsync("VotingFinished", room.ActiveSettings.Value);
            await Task.Delay(2000);
            await StartGameInternal(room);
        }
    }

    private async Task StartGameInternal(GameRoom room) {
        room.IsPlaying = true;
        room.GameStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); 
        foreach(var p in room.Players.Values) { 
            p.Score = 0; p.Streak = 0; p.FastestTimeMs = long.MaxValue; 
        } 
        _iTunesService.ResetPlayedTracks();
        await Clients.Group(room.RoomCode).SendAsync("GameStarting", room.ActiveSettings.Mode, room.ActiveSettings.Value);
        await SendNextQuestion(room);
    }

    private async Task SendNextQuestion(GameRoom room) {
        room.WrongGuessersThisRound.Clear();
        var question = await _iTunesService.GetQuestionAsync(room.ActiveSettings.Mode, room.ActiveSettings.Value);
        
        if (question != null) {
            room.CurrentCorrectAnswer = question.CorrectAnswer;
            room.CurrentAudioUrl = question.AudioUrl; 
            room.CurrentCoverUrl = question.CoverUrl; 
            room.QuestionStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); 
            await Clients.Group(room.RoomCode).SendAsync("NewQuestionReceived", question);
        } else {
            await Clients.Group(room.RoomCode).SendAsync("GameInfo", "Şarkı bitti! Lobiye dönülüyor.");
            await Task.Delay(3000);
            ResetLobby(room);
        }
    }

    public async Task SubmitAnswer(string answer) {
        if (!_userRooms.TryGetValue(Context.ConnectionId, out var roomCode) || !_rooms.TryGetValue(roomCode, out var room)) return;
        if (!room.IsPlaying || room.WrongGuessersThisRound.Contains(Context.ConnectionId)) return;

        var player = room.Players[Context.ConnectionId];
        long timeTakenMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - room.QuestionStartTime;
        bool isFast = timeTakenMs <= 2500; 

        if (answer == room.CurrentCorrectAnswer) {
            int points = isFast ? 2 : 1; 
            
            if(timeTakenMs < player.FastestTimeMs) {
                player.FastestTimeMs = timeTakenMs;
                player.FastestTrackName = room.CurrentCorrectAnswer;
                player.FastestTrackUrl = room.CurrentAudioUrl;
                player.FastestCoverUrl = room.CurrentCoverUrl;
            }

            if (player.Streak < 0) player.Streak = 0; 
            player.Streak++; 
            if (player.IsOnFire) points = isFast ? 3 : 2; 

            foreach(var other in room.Players.Values) {
                if(other != player && other.Streak > 0) {
                    other.Streak = 0;
                }
            }

            player.Score += points;
            string speedMsg = isFast ? "(Hızlı Cevap!)" : "";
            await Clients.Group(roomCode).SendAsync("GameInfo", $"✅ {player.Name} bildi! {speedMsg} (+{points})");
            await BroadcastLeaderboard(roomCode, player.Name, true); 

            if (player.Score >= 20) { 
                double totalSeconds = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - room.GameStartTime) / 1000.0;
                string timeString = string.Format("{0:0}:{1:00}", Math.Floor(totalSeconds / 60), totalSeconds % 60);

                await Clients.Group(roomCode).SendAsync("GameFinished", player.Name, timeString, player.FastestTrackName, player.FastestTrackUrl, player.FastestTimeMs, player.FastestCoverUrl);
                ResetLobby(room);
            } else {
                await SendNextQuestion(room);
            }
        } else {
            room.WrongGuessersThisRound.Add(Context.ConnectionId);
            if (player.Streak > 0) player.Streak = 0; 
            player.Streak--;

            if (player.Streak <= -2) {
                player.Score--; player.Streak = 0; 
            }

            await Clients.Caller.SendAsync("AnswerResult", false, "Yanlış!");
            await Clients.OthersInGroup(roomCode).SendAsync("GameInfo", $"❌ {player.Name} yanlış bildi!");
            await BroadcastLeaderboard(roomCode, player.Name, false); 

            if (room.WrongGuessersThisRound.Count >= room.Players.Count) {
                room.WrongGuessersThisRound.Clear();
                await Clients.Group(roomCode).SendAsync("AllGuessedWrong");
            }
        }
    }

    private async Task BroadcastLeaderboard(string roomCode, string actionPlayerName = "", bool? isCorrect = null) {
        if (_rooms.TryGetValue(roomCode, out var room)) {
            var sortedList = room.Players.Values.OrderByDescending(p => p.Score).ToList();
            await Clients.Group(roomCode).SendAsync("LeaderboardUpdated", sortedList, actionPlayerName, isCorrect);
        }
    }

    private void ResetLobby(GameRoom room) {
        room.IsPlaying = false;
        room.Proposals.Clear();
        room.Votes.Clear();
        foreach(var p in room.Players.Values) { p.Score = 0; p.Streak = 0; }
        Clients.Group(room.RoomCode).SendAsync("ResetToLobby");
        BroadcastLeaderboard(room.RoomCode).Wait();
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        if (_userRooms.TryGetValue(Context.ConnectionId, out var roomCode) && _rooms.TryGetValue(roomCode, out var room)) {
            room.Players.TryRemove(Context.ConnectionId, out _);
            room.Votes.TryRemove(Context.ConnectionId, out _);
            _userRooms.TryRemove(Context.ConnectionId, out _);
            
            if (room.Players.Count == 0) {
                _rooms.TryRemove(roomCode, out _); 
            } else {
                if (room.HostConnectionId == Context.ConnectionId) {
                    room.HostConnectionId = room.Players.Keys.First();
                    await Clients.Client(room.HostConnectionId).SendAsync("YouAreNowHost");
                }
                await BroadcastLeaderboard(roomCode);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}