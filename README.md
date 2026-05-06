# 🏁 TRACK1: Real-Time Multiplayer Music Quiz

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](#)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-orange?logo=signalr&logoColor=white)](#)
[![Vanilla JS](https://img.shields.io/badge/Frontend-Vanilla_JS-F7DF1E?logo=javascript&logoColor=black)](#)

---

## 🎵 About The Game
**TRACK1** is a fast-paced, real-time "Party Game" where you test your music knowledge against your friends. Imagine the excitement of a live game show, right in your browser. 

Players join a live lobby using a unique code, collaboratively vote on a music category (e.g., "Rock" or "The Weeknd"), and race against the clock to guess the playing song. With explosive visual feedback for winning streaks, a dynamic scoring system that rewards speed, and a flawless bilingual (EN/TR) interface, TRACK1 is designed to be highly competitive and incredibly fun. 

---

## 🎵 Oyun Hakkında
**TRACK1**, arkadaşlarınızla bir araya gelip müzik bilginizi yarıştırdığınız, yüksek tempolu ve gerçek zamanlı bir "Party Game"dir. Bir bilgi yarışmasının canlı heyecanını doğrudan tarayıcınıza taşır.

Oyuncular benzersiz bir kodla canlı lobiye katılır, ortaklaşa bir müzik kategorisi (örn. "Rock" veya "Manga") belirler ve çalan şarkıyı en hızlı tahmin eden olmak için zamana karşı yarışırlar. Üst üste doğru bilenleri ödüllendiren "Alev" (Streak) sistemi, hızı ödüllendiren dinamik puanlaması ve kusursuz çift dilli (TR/EN) arayüzü ile TRACK1, son derece rekabetçi ve eğlenceli olacak şekilde tasarlanmıştır.

---

## 🎥 Demo / Gameplay


https://github.com/user-attachments/assets/c70939e7-3e0d-4ba0-91ff-96635397f61a


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

**1. Clone the repository / Repoyu indirin:**
    ```git clone github.com/yigitdonmez/TRACK1.git```

**2. Go to project folder / Proje klasörüne girin:**
    ```cd TRACK1```

**3. Restore & Run / Çalıştırın:**
    ```dotnet run```

**D. Play / Oynayın:**
Open your browser and go to localhost address seen on your terminal. You can test this by opening two tabs side-by-side to invite your friends. / Tarayıcınızı açın ve terminalinizde görünen localhost adresine gidin. Arkadaşlarınızı davet etmek için yan yana iki sekme açarak test edebilirsiniz.

---

## 📅 Roadmap / Yol Haritası

- [ ] **Spotify Integration / Spotify Entegrasyonu:** Move beyond 30s previews to full library access. *(30 saniyelik önizlemelerden tam kütüphane erişimine geçiş.)*
- [ ] **Global Leaderboard / Küresel Skor Tablosu:** Persistent worldwide rankings using Redis. *(Redis kullanarak dünya çapında kalıcı skor tablosu oluşturulması.)*
- [ ] **Cloud Deployment / Bulut Dağıtımı (Canlıya Alma):** Containerize with Docker and deploy via Azure App Service or AWS. *(Projeyi Docker ile konteynerize edip Azure veya AWS üzerinden global erişime açmak.)*
- [ ] **CI/CD Pipeline / Sürekli Entegrasyon:** Automated testing and deployment using GitHub Actions. *(GitHub Actions ile koda eklenen her yeni özelliğin sunucuya otomatik olarak güncellenmesi.)*
- [ ] **Lobby Settings / Gelişmiş Lobi Ayarları:** Dedicated configuration area for hosts to modify game rules (e.g., round time, target score). *(Oda kurucularının tur süresi, hedef puan gibi oyun kurallarını değiştirebileceği ayarlar menüsü.)*
- [ ] **Custom Rooms / Özel Odalar:** Password-protected private lobby support. *(Şifreli ve dışarıya kapalı özel lobi kurma desteği.)*
- [ ] **Language Support / Genişletilmiş Dil Desteği:** Expanding i18n support with new language options. *(Mevcut i18n altyapısına yeni dil seçenekleri eklenmesi.)*

---

## 📄 License / Lisans
Bu proje **MIT Lisansı** ile lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakabilirsiniz.

---

**Developer:** [Salih Yiğit Dönmez](https://github.com/yigitdonmez)  
**Affiliation:** Istanbul Technical University (ITU) - Computer Engineering
