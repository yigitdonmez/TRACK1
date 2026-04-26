# 🏁 TRACK1: Real-Time Multiplayer Music Quiz

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](#)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-orange?logo=signalr&logoColor=white)](#)
[![Vanilla JS](https://img.shields.io/badge/Frontend-Vanilla_JS-F7DF1E?logo=javascript&logoColor=black)](#)

[🇬🇧 English Description](#english-description) | [🇹🇷 Türkçe Açıklama](#türkçe-açıklama)

---

## 🎵 About The Game (Elevator Pitch)
**TRACK1** is a fast-paced, real-time "Party Game" where you test your music knowledge against your friends. Imagine the excitement of a live game show, right in your browser. 

Players join a live lobby using a unique code, collaboratively vote on a music category (e.g., "Rock" or "The Weeknd"), and race against the clock to guess the playing song. With explosive visual feedback for winning streaks, a dynamic scoring system that rewards speed, and a flawless bilingual (EN/TR) interface, TRACK1 is designed to be highly competitive and incredibly fun. 

---

## 🎵 Oyun Hakkında (Asansör Sunumu)
**TRACK1**, arkadaşlarınızla bir araya gelip müzik bilginizi yarıştırdığınız, yüksek tempolu ve gerçek zamanlı bir "Party Game"dir. Bir bilgi yarışmasının canlı heyecanını doğrudan tarayıcınıza taşır.

Oyuncular benzersiz bir kodla canlı lobiye katılır, ortaklaşa bir müzik kategorisi (örn. "Rock" veya "Manga") belirler ve çalan şarkıyı en hızlı tahmin eden olmak için zamana karşı yarışırlar. Üst üste doğru bilenleri ödüllendiren "Alev" (Streak) sistemi, hızı ödüllendiren dinamik puanlaması ve kusursuz çift dilli (TR/EN) arayüzü ile TRACK1, son derece rekabetçi ve eğlenceli olacak şekilde tasarlanmıştır.

---

## 🎥 Demo / Gameplay
> <video src="https://github.com/yigitdonmez/TRACK1/raw/refs/heads/main/record.mp4" width="100%" autoplay loop muted playsinline></video>

---

## 🇬🇧 English Description

### 🏗️ Technical Architecture & Engineering Decisions

#### 1. "Dumb Server, Smart Client" (Event-Driven Logic)
The server acts strictly as an **event dispatcher**. It does not push translated strings; instead, it broadcasts raw events (e.g., `PlayerGuessedEvent`). 
* **Benefit:** This allows the UI to handle localization (`i18n`) independently, significantly reducing server-side complexity and payload size.

#### 2. Solving Asynchronous Race Conditions
A critical issue was identified where "Unlock Board" signals arrived before "Lock Input" due to network jitter. 
* **The Fix:** Implemented a **1.5s dramatic delay** (`Task.Delay`) in the C# backend. This ensures the "Wrong Answer" state is correctly perceived by the user before the UI resets, preventing state desync.

#### 3. Ghost Connection Management
Unexpected disconnections (closing tabs/network drops) can lead to stale game states.
* **The Fix:** Custom `OnDisconnectedAsync` logic performs atomic cleanup, reassigns "Host" privileges instantly, and prevents game-breaking deadlocks by recalculating active player requirements.

### 🛠️ Tech Stack
* **Backend:** C#, ASP.NET Core 8.0, SignalR.
* **Frontend:** HTML5, Modern CSS (Neon Arcade Theme), Vanilla JavaScript.
* **Data Source:** iTunes Search API (Audio previews & Covers).

---

## 🇹🇷 Türkçe Açıklama

### 🏗️ Teknik Mimari ve Mühendislik Kararları

#### 1. "Dilsiz Sunucu, Akıllı İstemci" (Olay Tabanlı Mantık)
Sunucu tamamen bir **olay dağıtıcısı** olarak çalışır. Sunucu üzerinden metin yollamak yerine ham olaylar (`PlayerGuessedEvent`) yayınlanır.
* **Avantaj:** Bu mimari, frontend'in dil çevirilerini (`i18n`) ve görselleştirmeyi bağımsız yönetmesini sağlar, sunucu yükünü minimize eder.

#### 2. Asenkron Yarış Durumu (Race Condition) Çözümü
Ağ dalgalanmaları nedeniyle "Girişleri Aç" sinyalinin "Girişleri Kapat" sinyalinden önce ulaştığı ve oyunun kilitlendiği durumlar için özel bir çözüm geliştirilmiştir.
* **Çözüm:** Backend tarafında **1.5 saniyelik gecikme** (`Task.Delay`) uygulanarak paketlerin doğru sırayla işlenmesi ve tutarlı bir kullanıcı deneyimi (UX) sağlanmıştır.

#### 3. Hayalet Bağlantı (Ghost Connection) Yönetimi
Aniden kopan bağlantıların oyunu dondurmasını engellemek için `OnDisconnectedAsync` metodunda atomik temizlik işlemleri yapılır ve "Host" yetkisi kalan oyunculara anında devredilir.

---

## 🚀 Setup / Kurulum

### Prerequisites / Gereksinimler
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later. / veya daha yenisi.

### Step-by-Step / Adım Adım

**A. Clone the repository / Repoyu indirin:**
    git clone [github.com/yigitdonmez/TRACK1.git](https://github.com/yigitdonmez/TRACK1.git)

**B. Go to project folder / Proje klasörüne girin:**
    cd TRACK1

**C. Restore & Run / Çalıştırın:**
    dotnet run

**D. Play / Oynayın:**
Open your browser and go to localhost address seen on your terminal. You can test this by opening two tabs side-by-side to invite your friends. / Tarayıcınızı açın ve terminalinizde görünen localhost adresine gidin. Arkadaşlarınızı davet etmek için yan yana iki sekme açarak test edebilirsiniz.

---

## 📅 Roadmap / Yol Haritası
- [ ] **Spotify Integration:** 30 saniyelik önizlemelerden tam kütüphane erişimine geçiş.
- [ ] **Global Leaderboard:** Redis kullanarak dünya çapında kalıcı skor tablosu.
- [ ] **Lobby Settings:** Oyuncuların kendi lobi ayarlarını değiştirebileceği alan
- [ ] **Custom Rooms:** Şifreli ve özel lobi kurma desteği.
- [ ] **Language Support:** Dil seçenekleri eklenmesi.

---

## 📄 License / Lisans
Bu proje **MIT Lisansı** ile lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakabilirsiniz.

---

**Developer:** [Salih Yiğit Dönmez](github.com/yigitdonmez)  
**Affiliation:** Istanbul Technical University (ITU) - Computer Engineering