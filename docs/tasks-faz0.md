# ✅ Faz 0 — Görev Listesi: Proje Kurulumu & Altyapı

> Kaynak: [phases.md — FAZ 0](phases.md#faz-0--proje-kurulumu--altyapı)
> Tüm görevler tamamlandığında `[ ]` → `[x]` olarak işaretle.

---

## 🎨 Bölüm A — Tasarım Sistemi
> Kaynak: [phases.md § 0.1 UI/UX Tasarımı](phases.md#01-uiux-tasarımı)

- [x] **A-01** — Frontend projesinde **Tailwind CSS**'i kur ve yapılandır (`tailwind.config.js` oluştur).
- [x] **A-02** — `tailwind.config.js` içinde özel renk token'larını tanımla:
  - `primary: '#10b981'`
  - `primary-dark: '#059669'`
  - `primary-light: '#d1fae5'`
  - `surface: '#ffffff'`
  - `surface-alt: '#f9fafb'`
  - `text: '#111827'`
  - `text-muted: '#6b7280'`
- [x] **A-03** — `index.html`'e **Google Fonts** (Inter veya Outfit) bağlantısını ekle ve `font-family` olarak ayarla.
- [x] **A-04** — **Framer Motion** paketini kur: `npm install framer-motion`.

---

## 🔧 Bölüm B — Backend Kurulumu (.NET Web API)
> Kaynak: [phases.md § 0.2 — Backend (.NET Web API)](phases.md#backend-net-web-api)

- [x] **B-01** — `dotnet new webapi -n EmailSubscriber.API` komutuyla yeni Web API projesi oluştur.
- [x] **B-02** — Aşağıdaki NuGet paketlerini projeye ekle:
  - [x] `Microsoft.EntityFrameworkCore`
  - [x] `Pomelo.EntityFrameworkCore.MySql`
  - [x] `MailKit`
  - [x] `Serilog.AspNetCore`
  - [x] `Microsoft.AspNetCore.Authentication.JwtBearer`
- [x] **B-03** — Proje içinde aşağıdaki klasör yapısını oluştur:
  - [x] `Controllers/`
  - [x] `Data/` ← DbContext buraya gelecek
  - [x] `Models/` ← Entity sınıfları
  - [x] `DTOs/` ← Request/Response nesneleri
  - [x] `Services/` ← IEmailService, ISubscriberService
  - [x] `Queue/` ← EmailJob, EmailQueueService, EmailWorker
  - [x] `Templates/` ← HTML e-posta şablonları
  - [x] `Middleware/` ← Rate limiting, error handler
- [x] **B-04** — `appsettings.json` iskeletini oluştur (değerleri boş bırak, user secrets kullan).
- [x] **B-05** — `dotnet user-secrets init` ile user secrets'ı projeye bağla.
- [x] **B-06** — Hassas verileri user secrets ile sakla (koda asla gömme):
  - [x] `dotnet user-secrets set "Smtp:Password" "<gmail-app-password>"`
  - [x] `dotnet user-secrets set "Jwt:Secret" "<super-secret-key>"`

---

## 🗄️ Bölüm C — Veritabanı Kurulumu (MySQL + EF Core)
> Kaynak: [phases.md § 0.2 — Veritabanı](phases.md#veritabanı)

- [x] **C-01** — MySQL'de `emailsubscriber` adlı veritabanını oluştur.
- [x] **C-02** — `Data/AppDbContext.cs` dosyasını yaz; `DbSet<Subscriber> Subscribers` alanını tanımla.
- [x] **C-03** — `Models/Subscriber.cs` Entity sınıfını aşağıdaki tüm alanlarla oluştur:
  - [x] `Id` (int, PK, auto-increment)
  - [x] `Email` (string, zorunlu)
  - [x] `Name` (string?, opsiyonel)
  - [x] `IsConfirmed` (bool, default: false)
  - [x] `IsActive` (bool, default: true)
  - [x] `SubscribedAt` (DateTime, default: DateTime.UtcNow)
  - [x] `UnsubscribedAt` (DateTime?, null olabilir)
  - [x] `ConfirmationToken` (string?, null olabilir)
  - [x] `ConfirmationTokenExpiresAt` (DateTime?, null olabilir — 24 saat expiry)
- [x] **C-04** — İlk migration'ı oluştur: `dotnet ef migrations add InitialCreate`
- [x] **C-05** — Migration'ı veritabanına uygula: `dotnet ef database update`
- [x] **C-06** — MySQL istemcisinde (MySQL Workbench / DBeaver vb.) `Subscribers` tablosunun tüm sütunlarıyla doğru oluştuğunu doğrula.

---

## ⚛️ Bölüm D — Frontend Kurulumu (React + Vite)
> Kaynak: [phases.md § 0.2 — Frontend (React + Vite)](phases.md#frontend-react--vite)

- [x] **D-01** — Vite + React projesi oluştur: `npm create vite@latest email-subscriber-client -- --template react`
- [x] **D-02** — Proje dizinine gir ve tüm bağımlılıkları tek komutla kur:
  `npm install tailwindcss @tailwindcss/forms framer-motion axios react-router-dom recharts react-quill`
- [x] **D-03** — `src/` altında klasör yapısını oluştur:
  - [x] `src/api/`
  - [x] `src/components/`
  - [x] `src/pages/`
  - [x] `src/hooks/`
  - [x] `src/utils/`
- [x] **D-04** — Boş sayfa dosyalarını oluştur (içleri sonraki fazlarda doldurulacak):
  - [x] `src/pages/SubscribePage.jsx`
  - [x] `src/pages/ConfirmPage.jsx`
  - [x] `src/pages/UnsubscribePage.jsx`
  - [x] `src/pages/admin/AdminLoginPage.jsx`
  - [x] `src/pages/admin/DashboardPage.jsx`
  - [x] `src/pages/admin/SubscribersPage.jsx`
  - [x] `src/pages/admin/NewsletterPage.jsx`
  - [x] `src/pages/admin/CampaignsPage.jsx`
  - [x] `src/components/admin/AdminLayout.jsx`
  - [x] `src/components/admin/ProtectedRoute.jsx`
- [x] **D-05** — `react-router-dom` ile `App.jsx`'e tam route yapısını kur (tüm sayfalar dahil).

---

## 💻 Bölüm E — Temel Kodlama (İskelet)
> Kaynak: [phases.md § 0.3 Kodlama Süreci](phases.md#03-kodlama-süreci)

### Backend — `Program.cs`
- [x] **E-01** — `AppDbContext`'i DI container'a kaydet (MySQL bağlantı string'iyle).
- [x] **E-02** — `Serilog`'u yapılandır ve kaydet (`UseSerilog()`).
- [x] **E-03** — CORS politikasını ekle: React frontend portuna izin ver (`http://localhost:5173`).
- [x] **E-04** — `app.UseHttpsRedirection();` ekle (HTTPS zorunluluğu — req. 4.3).
- [x] **E-05** — Sağlık kontrolü endpoint'i ekle: `app.MapGet("/health", () => Results.Ok("OK"));`

### Frontend — API Katmanı
- [x] **E-06** — `src/api/axiosInstance.js` dosyasını oluştur (`VITE_API_URL` environment variable kullan).
- [x] **E-07** — Proje kökünde `.env` dosyası oluştur: `VITE_API_URL=http://localhost:5000`
- [x] **E-08** — `.env` dosyasını `.gitignore`'a ekle.

---

## 🧪 Bölüm F — Test & Doğrulama
> Kaynak: [phases.md § 0.4 Test Stratejisi](phases.md#04-test-stratejisi)

- [x] **F-01** — Backend'i başlat (`dotnet run`) ve `GET /health` endpoint'ini test et → `200 OK` beklenir.
- [x] **F-02** — MySQL'de migration'ın doğru uygulandığını doğrula → `Subscribers` tablosu tüm sütunlarıyla mevcut olmalı.
- [x] **F-03** — Frontend'i başlat (`npm run dev`) → `http://localhost:5173` hatasız açılmalı.
- [x] **F-04** — Tarayıcı konsolundan (veya Postman'den) frontend → backend `GET /health` isteği at → CORS hatası olmamalı.

---

## 📊 İlerleme Özeti

| Bölüm | Görev Sayısı | Durum |
|-------|-------------|-------|
| A — Tasarım Sistemi | 4 | ✅ Tamamlandı |
| B — Backend Kurulumu | 6 | ✅ Tamamlandı |
| C — Veritabanı | 6 | ✅ Tamamlandı |
| D — Frontend Kurulumu | 5 | ✅ Tamamlandı |
| E — Temel Kodlama | 8 | ✅ Tamamlandı |
| F — Test & Doğrulama | 4 | ✅ Tamamlandı |
| **Toplam** | **33** | ✅ Tamamlandı |

> 🏁 Tüm **F bölümü** tamamlandığında Faz 0 bitti demektir → **[Faz 1](phases.md#faz-1--abonelik-formu-public-sayfası)**'e geç!

