using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class QuizQuestion {
    public string AudioUrl { get; set; }
    public string CorrectAnswer { get; set; }
    public List<string> Options { get; set; }
    public string CoverUrl { get; set; } 
}

public class ITunesService {
    private static readonly HttpClient _httpClient = new HttpClient();
    private static HashSet<long> _playedTrackIds = new HashSet<long>();

    public void ResetPlayedTracks() => _playedTrackIds.Clear();

    public async Task<QuizQuestion?> GetQuestionAsync(string mode, string value) {
        try {
            string url;
            if (mode == "Artist") {
                url = $"https://itunes.apple.com/search?term={Uri.EscapeDataString(value)}&entity=song&attribute=artistTerm&limit=100&country=TR";
            } else {
                url = $"https://itunes.apple.com/search?term={Uri.EscapeDataString(value)}&entity=song&limit=300&country=TR";
            }

            var response = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var results = doc.RootElement.GetProperty("results");
            if (results.GetArrayLength() == 0) return null;

            var allTracks = results.EnumerateArray().ToList();
            var validTracks = new List<JsonElement>();

            foreach (var t in allTracks) {
                if (!t.TryGetProperty("previewUrl", out _)) continue;
                if (_playedTrackIds.Contains(t.GetProperty("trackId").GetInt64())) continue;

                if (mode == "Artist") {
                    string aName = t.GetProperty("artistName").GetString() ?? "";
                    if (aName.Contains(value, StringComparison.OrdinalIgnoreCase)) {
                        validTracks.Add(t);
                    }
                } else { 
                    string gName = t.GetProperty("primaryGenreName").GetString() ?? "";
                    if (gName.Contains(value, StringComparison.OrdinalIgnoreCase)) {
                        validTracks.Add(t);
                    }
                }
            }

            if (validTracks.Count == 0) return null;

            var random = new Random();
            var correctTrack = validTracks[random.Next(validTracks.Count)];
            _playedTrackIds.Add(correctTrack.GetProperty("trackId").GetInt64());

            string GetFormat(JsonElement el) => mode == "Artist" 
                ? el.GetProperty("trackName").GetString()! 
                : $"{el.GetProperty("artistName").GetString()} - {el.GetProperty("trackName").GetString()}";

            string correctTrackName = GetFormat(correctTrack);
            string previewUrl = correctTrack.GetProperty("previewUrl").GetString()!;

            string coverUrl = "";
            if (correctTrack.TryGetProperty("artworkUrl100", out var artworkElement)) {
                coverUrl = artworkElement.GetString()?.Replace("100x100bb", "800x800bb") ?? "";
            }

            var options = allTracks
                .Where(t => t.TryGetProperty("previewUrl", out _))
                .Select(GetFormat)
                .Where(name => name != correctTrackName)
                .Distinct()
                .OrderBy(x => random.Next())
                .Take(3)
                .ToList();
                
            options.Add(correctTrackName);
            return new QuizQuestion { 
                AudioUrl = previewUrl, 
                CorrectAnswer = correctTrackName, 
                Options = options.OrderBy(x => random.Next()).ToList(),
                CoverUrl = coverUrl
            };
        } catch { return null; }
    }

    public async Task<int> GetTrackCountAsync(string mode, string value) {
        try {
            string url = mode == "Artist" 
                ? $"https://itunes.apple.com/search?term={Uri.EscapeDataString(value)}&entity=song&attribute=artistTerm&limit=100&country=TR" 
                : $"https://itunes.apple.com/search?term={Uri.EscapeDataString(value)}&entity=song&limit=200&country=TR";

            var response = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var results = doc.RootElement.GetProperty("results");
            
            int count = 0;
            foreach (var t in results.EnumerateArray()) {
                if (!t.TryGetProperty("previewUrl", out _)) continue;

                if (mode == "Artist") {
                    string aName = t.GetProperty("artistName").GetString() ?? "";
                    if (aName.Contains(value, StringComparison.OrdinalIgnoreCase)) count++;
                } else { 
                    string gName = t.GetProperty("primaryGenreName").GetString() ?? "";
                    if (gName.Contains(value, StringComparison.OrdinalIgnoreCase)) count++;
                }
            }
            return count;
        } catch { return 0; }
    }
}