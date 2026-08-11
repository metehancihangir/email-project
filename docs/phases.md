# 📍 Email Subscriber — Geliştirme Yol Haritası (Roadmap)

> Bu belge, `requirements.md` dosyasındaki tüm gereksinimleri kapsar. Sıfırdan başlayarak her fazı tamamladığında bir sonrakine geçebilirsin. Hiçbir gereksinim atlanmamıştır.

---

## Faz Genel Bakış

| Faz | Konu | Önkoşul |
|-----|------|---------|
| **Faz 0** | Proje Kurulumu & Altyapı | — |
| **Faz 1** | Abonelik Formu (Public) | Faz 0 |
| **Faz 2** | E-posta Kuyruk Altyapısı (IHostedService) | Faz 1 |
| **Faz 3** | Onay & Hoş Geldin E-postası (Double Opt-in) | Faz 2 |
| **Faz 4** | Admin Paneli (Auth + Abone Yönetimi) | Faz 3 |
| **Faz 5** | Toplu Bülten Gönderimi (Newsletter) | Faz 4 |
| **Faz 6** | Analitik (Open/Click Tracking) | Faz 5 |

---

---

# FAZ 0 — Proje Kurulumu & Altyapı

> **Amaç:** Tüm teknoloji yığınını kurmak, projeyi yapılandırmak ve boş ama çalışan bir iskelet oluşturmak.

---

## 0.1 UI/UX Tasarımı

Bu fazda kullanıcıya yönelik ekran yoktur. Ancak tasarım sistemini şimdi kur:

- **Tailwind CSS** kurulumu ve `tailwind.config.js`'de özel renk tokenlarını tanımla:
  ```js
  colors: {
    primary: '#10b981',
    'primary-dark': '#059669',
    'primary-light': '#d1fae5',
    surface: '#ffffff',
    'surface-alt': '#f9fafb',
    text: '#111827',
    'text-muted': '#6b7280',
  }
  ```
- **Google Fonts** — Inter veya Outfit fontunu `index.html`'e ekle.
- **Framer Motion** paketini kur (`npm install framer-motion`).

---

## 0.2 Mimari & Veritabanı Modelleme

### Backend (.NET Web API)
1. `dotnet new webapi -n EmailSubscriber.API` ile proje oluştur.
2. NuGet paketlerini ekle:
   - `Microsoft.EntityFrameworkCore`
   - `Pomelo.EntityFrameworkCore.MySql`
   - `MailKit`
   - `Serilog.AspNetCore`
   - `Microsoft.AspNetCore.Authentication.JwtBearer`
3. Klasör yapısını kur:
   ```
   EmailSubscriber.API/
   ├── Controllers/
   ├── Data/              ← DbContext
   ├── Models/            ← Entity sınıfları
   ├── DTOs/              ← Request/Response nesneleri
   ├── Services/          ← IEmailService, ISubscriberService
   ├── Queue/             ← EmailJob, EmailQueueService, EmailWorker
   ├── Templates/         ← HTML e-posta şablonları
   └── Middleware/        ← Rate limiting, error handler
   ```
4. `appsettings.json` yapısını oluştur (değerleri boş bırak, user secrets kullan):
   ```json
   {
     "ConnectionStrings": { "Default": "" },
     "Smtp": { "Host": "", "Port": 587, "User": "", "Password": "" },
     "Jwt": { "Secret": "", "Issuer": "", "Audience": "" },
     "App": { "BaseUrl": "http://localhost:5173", "ApiBaseUrl": "http://localhost:5000" }
   }
   ```
5. **`dotnet user-secrets`** ile hassas verileri sakla:
   ```
   dotnet user-secrets set "Smtp:Password" "gmail-app-password"
   dotnet user-secrets set "Jwt:Secret" "super-secret-key"
   ```

### Veritabanı
1. MySQL'de `emailsubscriber` veritabanını oluştur.
2. `AppDbContext` sınıfını yaz.
3. **Subscriber Entity** (tüm alanlar baştan eklensin):

   ```csharp
   public class Subscriber
   {
       public int Id { get; set; }
       public string Email { get; set; } = "";
       public string? Name { get; set; }
       public bool IsConfirmed { get; set; } = false;
       public bool IsActive { get; set; } = true;
       public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
       public DateTime? UnsubscribedAt { get; set; }
       public string? ConfirmationToken { get; set; }
       public DateTime? ConfirmationTokenExpiresAt { get; set; }
   }
   ```

4. İlk migration'ı oluştur ve uygula:
   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

### Frontend (React + Vite)
1. `npm create vite@latest email-subscriber-client -- --template react` ile proje kur.
2. Bağımlılıkları kur:
   ```
   npm install tailwindcss @tailwindcss/forms framer-motion axios react-router-dom recharts react-quill
   ```
3. Klasör yapısı:
   ```
   src/
   ├── api/           ← axios instance + endpoint fonksiyonları
   ├── components/    ← yeniden kullanılabilir UI parçaları
   ├── pages/         ← tam sayfa bileşenleri
   ├── hooks/         ← custom hooks
   └── utils/         ← yardımcı fonksiyonlar
   ```
4. Route yapısını kur:
   ```jsx
   <Routes>
     <Route path="/"              element={<SubscribePage />} />
     <Route path="/confirm"       element={<ConfirmPage />} />
     <Route path="/unsubscribe"   element={<UnsubscribePage />} />
     <Route path="/admin/login"   element={<AdminLoginPage />} />
     <Route path="/admin/*"       element={<ProtectedRoute><AdminLayout /></ProtectedRoute>}>
       <Route index              element={<DashboardPage />} />
       <Route path="subscribers" element={<SubscribersPage />} />
       <Route path="newsletter"  element={<NewsletterPage />} />
       <Route path="campaigns"   element={<CampaignsPage />} />
     </Route>
   </Routes>
   ```

---

## 0.3 Kodlama Süreci

### Backend
- `Program.cs`'de şunları kaydet: `DbContext`, `Serilog`, `CORS`, sağlık endpoint'i.

### Frontend
- `src/api/axiosInstance.js`'i kur; `VITE_API_URL` environment variable kullan.

---

## 0.4 Test Stratejisi

| Test | Beklenen Sonuç |
|------|----------------|
| `dotnet run` → `GET /health` | `200 OK` |
| Migration uygulandı mı? | MySQL'de tablo oluştu |
| `npm run dev` | `http://localhost:5173` açılıyor |
| Frontend'den `/health`'e fetch | CORS hatası yok |

---
---

# FAZ 1 — Abonelik Formu (Public Sayfası)

> **Amaç:** Ziyaretçinin e-posta adresi ve opsiyonel isimle abone olabildiği public-facing sayfayı tamamlamak. (req. 3.1, 3.5, 4.1, 4.3, 4.4, 8.1, 8.2, 8.3)

---

## 1.1 UI/UX Tasarımı

### Ekranlar

**Ana Sayfa / Abonelik Sayfası** (`/`)
- **Navbar:** Logo solda, "Admin Girişi" linki sağda.
- **Hero Bölümü:** Büyük başlık, kısa açıklama, 3 fayda maddesi (icon + metin).
- **Form Kartı:**
  - İsim alanı (opsiyonel): `<input type="text" />`
  - E-posta alanı (zorunlu): `<input type="email" required />`
  - "Abone Ol" butonu — emerald green, tam genişlik
- **Kullanıcı Etkileşimleri:**
  - Focus: yeşil border + subtle shadow (CSS transition)
  - Submit: buton disabled + spinner (loading state)
  - Başarı: animasyonlu yeşil toast
  - Hata: animasyonlu kırmızı toast
  - Duplicate: "Bu e-posta adresi zaten kayıtlı." mesajı
- Responsive: Masaüstünde iki sütun, mobilhde tek sütun.
- `aria-label`, `aria-required`, `htmlFor` etiketleri (erişilebilirlik)

**Toast / Alert Bileşeni** (tüm sayfalarda kullanılır)
- Framer Motion: `opacity: 0→1` + `y: -20→0`
- Tip: `success | error | info | warning`
- 4 saniye sonra otomatik kapanır

---

## 1.2 Mimari & Veritabanı

- Mevcut `Subscribers` tablosu yeterli.
- Rate Limiting ekle (`Program.cs`): aynı IP'den 1 dakikada max 3 istek.
- `ISubscriberService` arayüzünü tanımla.

### Güvenlik Katmanı (req. 4.3)

**SQL Injection & XSS Koruması:**
- EF Core parametreli sorgular kullandığı için SQL injection otomatik önlenir — ham SQL string birleştirmesinden kesinlikle kaçın.
- API'den dönen tüm metin alanlarını frontend'de `dangerouslySetInnerHTML` olmadan render et (React varsayılan olarak XSS'e karşı güvenlidir).
- Backend'de `[ApiController]` attribute'u model binding sırasında otomatik input sanitizasyonu sağlar.
- Girdi alanlarına `MaxLength` attribute'ları ekle (ör. `Name` için 100 karakter sınırı).

**HTTPS Zorunluluğu:**
- `Program.cs`'de `app.UseHttpsRedirection();` ekle — HTTP'ye gelen tüm istekleri HTTPS'e yönlendirir.
- Geliştirme ortamında: `dotnet dev-certs https --trust` ile self-signed sertifika kur.
- Production'da: reverse proxy (Nginx, IIS) üzerinden TLS sonlandırma yapılır.

**Honeypot (Bot Koruması — Opsiyonel):**
- Abonelik formuna görünmez (CSS ile `display:none`) bir `honeypot` input alanı ekle:
  ```jsx
  <input type="text" name="website" style={{display:'none'}} tabIndex={-1} />
  ```
- Backend'de bu alan doluysa isteği `400` ile reddet (bot tarafından dolduruldu).
- CAPTCHA (Google reCAPTCHA v3) alternatif olarak eklenebilir; öncelikle honeypot dene.

---

## 1.3 Kodlama Süreci

### Backend — `POST /api/subscribers`

**İş Mantığı:**
1. Request body'den `email` ve `name` al.
2. Sunucu tarafı e-posta format validasyonu; geçersizse `400 Bad Request`.
3. DB'de aynı `email` var mı?
   - `IsConfirmed = true` → `409 Conflict`
   - `IsConfirmed = false` + token geçerli → "Onay e-postası zaten gönderildi."
   - `IsConfirmed = false` + token expire → Yeni token üret, yeniden gönder
4. Yeni kayıt: `IsConfirmed = false`, `IsActive = true`
5. `ConfirmationToken = Guid.NewGuid().ToString("N")`
6. `ConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)`
7. Veritabanına kaydet.
8. Onay e-postasını kuyruğa ekle (Faz 2 hazır olunca güncelle, şimdilik IEmailService.SendAsync).
9. `202 Accepted` dön.

**Hata kodları:** `400` (format), `409` (duplicate), `500` (sunucu)

**SubscribeRequest DTO:**
```csharp
public record SubscribeRequest(
    [Required, EmailAddress] string Email,
    [MaxLength(100)] string? Name
);
```

### Frontend

**Component Hiyerarşisi:**
```
SubscribePage
├── Navbar
├── HeroSection
│   └── FeatureList
└── SubscribeForm
    ├── NameInput
    ├── EmailInput
    ├── SubmitButton (loading state)
    └── Toast
```

**State Yönetimi (`SubscribeForm.jsx`):**
```jsx
const [name, setName] = useState('');
const [email, setEmail] = useState('');
const [loading, setLoading] = useState(false);
const [toast, setToast] = useState(null); // { type, message }

const handleSubmit = async (e) => {
  e.preventDefault();
  if (!isValidEmail(email)) { setToast({ type:'error', message:'Geçersiz e-posta' }); return; }
  setLoading(true);
  try {
    await api.post('/api/subscribers', { email, name });
    setToast({ type: 'success', message: 'Onay e-postanız gönderildi!' });
    setEmail(''); setName('');
  } catch (err) {
    const msg = err.response?.data?.message || 'Bir hata oluştu.';
    setToast({ type: 'error', message: msg });
  } finally { setLoading(false); }
};
```

**`utils/validation.js`:**
```js
export const isValidEmail = (email) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
```

---

## 1.4 Test Stratejisi

| Test | Senaryo | Beklenen |
|------|---------|---------|
| Backend Unit | Geçerli e-posta POST | 202, DB'de kayıt var |
| Backend Unit | Geçersiz format | 400 |
| Backend Unit | Duplicate e-posta | 409 |
| Backend Unit | 4. istek aynı IP'den | 429 |
| Frontend | Boş form gönder | Client-side hata |
| Frontend | Başarılı yanıt | Toast görünür, form temizlenir |
| Frontend | 409 hatası | Uygun mesaj |
| E2E | Tam akış | Toast görünür, DB'de kayıt var |

---
---

# FAZ 2 — E-posta Kuyruk Altyapısı (IHostedService + Channel\<T\>)

> **Amaç:** Kullanıcıyı bloke etmeden e-posta gönderen arka plan kuyruk sistemini kurmak. (req. 4.2)

---

## 2.1 UI/UX Tasarımı

Bu faz arka plan altyapısıdır; yeni ekran yoktur. Etki: form submit sonrası API anında yanıt verir.

---

## 2.2 Mimari

```
API Controller
    ↓ EmailJob kuyruğa yazar
Channel<EmailJob>  (in-memory, .NET built-in)
    ↓ Worker dinler
EmailWorker (IHostedService)
    ↓ SendAsync çağırır
MailKit → Gmail SMTP
```

**EmailJob:**
```csharp
public record EmailJob(string To, string? ToName, string Subject, string HtmlBody);
```

---

## 2.3 Kodlama Süreci

### Backend

**`IEmailQueueService` + `EmailQueueService`:**
```csharp
public interface IEmailQueueService
{
    void Enqueue(EmailJob job);
    IAsyncEnumerable<EmailJob> DequeueAllAsync(CancellationToken ct);
}

public class EmailQueueService : IEmailQueueService
{
    private readonly Channel<EmailJob> _channel = Channel.CreateUnbounded<EmailJob>();

    public void Enqueue(EmailJob job) => _channel.Writer.TryWrite(job);

    public async IAsyncEnumerable<EmailJob> DequeueAllAsync(
        [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(ct))
            yield return job;
    }
}
```

**`EmailWorker` — Retry max 3, exponential backoff:**
```csharp
public class EmailWorker : BackgroundService
{
    private const int MaxRetry = 3;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var job in _queue.DequeueAllAsync(ct))
        {
            for (int attempt = 1; attempt <= MaxRetry; attempt++)
            {
                try
                {
                    await _emailService.SendAsync(job.To, job.ToName, job.Subject, job.HtmlBody);
                    _logger.LogInformation("E-posta gönderildi: {To}", job.To);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Deneme {Attempt}/{Max} başarısız: {To}", attempt, MaxRetry, job.To);
                    if (attempt < MaxRetry)
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                    else
                        _logger.LogError("E-posta gönderilemedi (max retry): {To}", job.To);
                }
            }
        }
    }
}
```

**`IEmailService` (MailKit):**
```csharp
public interface IEmailService
{
    Task SendAsync(string to, string? toName, string subject, string htmlBody);
}
// MailKitEmailService: SmtpClient ile MimeMessage oluştur, appsettings'ten Smtp config oku
```

**Program.cs kayıtları:**
```csharp
builder.Services.AddSingleton<IEmailQueueService, EmailQueueService>();
builder.Services.AddHostedService<EmailWorker>();
builder.Services.AddScoped<IEmailService, MailKitEmailService>();
```

**Faz 1'i güncelle:** `IEmailService.SendAsync()` → `IEmailQueueService.Enqueue()`.

---

## 2.4 Test Stratejisi

| Test | Senaryo | Beklenen |
|------|---------|---------|
| Unit | `Enqueue()` | Worker message aldı |
| Unit | SMTP hatası (mock) | 3 retry, log yazıldı |
| Unit | Exponential backoff | Her retry arası bekleme katlanıyor |
| Integration | Gerçek Gmail | E-posta ulaştı |
| Performans | Form submit | API < 200ms yanıt (e-posta beklenmez) |

---
---

# FAZ 3 — Onay & Hoş Geldin E-postası (Double Opt-in)

> **Amaç:** Token tabanlı onay, 24 saat expiry, hoş geldin e-postası ve unsubscribe akışını tamamlamak. (req. 3.2, 3.3, 3.4)

---

## 3.1 UI/UX Tasarımı

### Ekranlar

**Onay Sayfası** (`/confirm?token=...`)
- Loading: spinner
- Başarılı onay: animasyonlu yeşil checkmark + "Aboneliğiniz onaylandı!" + ana sayfaya dön butonu
- Token expire: kırmızı uyarı + "Yeni Onay E-postası Gönder" butonu (e-posta input'u)
- Merkeze hizalı kart, Framer Motion geçişleri

**Unsubscribe Sayfası** (`/unsubscribe?token=...`)
- Başarılı: gri/turuncu checkmark + "Aboneliğiniz iptal edildi."
- Token hatalı: hata mesajı
- Animasyonlu geçiş

### HTML E-posta Şablonları
- `Templates/ConfirmationEmailTemplate.html`: `{{Name}}`, `{{ConfirmUrl}}`
- `Templates/WelcomeEmailTemplate.html`: `{{Name}}`, `{{UnsubscribeUrl}}`
- Footer'da her zaman "abonelikten çık" linki
- `IEmailTemplateService`: dosyayı okur, `{{Key}}` → `value` replace eder

---

## 3.2 Kodlama Süreci

### Backend

**`GET /api/subscribers/confirm/{token}`**
1. Token ile kayıt ara; yoksa `404`.
2. `IsConfirmed = true` ise `200 OK` (idempotent).
3. `ConfirmationTokenExpiresAt < DateTime.UtcNow` → `410 Gone`.
4. Geçerliyse: `IsConfirmed = true`, token alanlarını null yap, DB güncelle.
5. Hoş geldin e-postasını kuyruğa ekle.
6. `200 OK` dön.

**`POST /api/subscribers/resend-confirmation`**
1. `email` al; kayıt var ve `IsConfirmed = false` mı kontrol et.
2. Yeni token + 24h expiry üret, DB güncelle.
3. Onay e-postasını kuyruğa ekle.
4. `202 Accepted` dön.

**`GET /api/subscribers/unsubscribe/{token}`**
1. Token ile kayıt ara; yoksa `404`.
2. `IsActive = false`, `UnsubscribedAt = DateTime.UtcNow` set et.
3. `200 OK` dön.

### Frontend

**`ConfirmPage.jsx`:**
```jsx
const [status, setStatus] = useState('loading'); // loading | success | expired | error
const token = new URLSearchParams(useLocation().search).get('token');

useEffect(() => {
  api.get(`/api/subscribers/confirm/${token}`)
    .then(() => setStatus('success'))
    .catch(err => setStatus(err.response?.status === 410 ? 'expired' : 'error'));
}, [token]);

return (
  <AnimatePresence>
    {status === 'loading' && <Spinner />}
    {status === 'success' && <SuccessCard />}
    {status === 'expired' && <ExpiredCard onResend={handleResend} />}
  </AnimatePresence>
);
```

---

## 3.4 Test Stratejisi

| Test | Senaryo | Beklenen |
|------|---------|---------|
| Backend Unit | Geçerli token | `IsConfirmed=true`, hoş geldin kuyruğa |
| Backend Unit | Expire token | `410 Gone` |
| Backend Unit | Bilinmeyen token | `404` |
| Backend Unit | Tekrar confirm | `200 OK` (idempotent) |
| Backend Unit | Unsubscribe | `IsActive=false`, `UnsubscribedAt` set |
| Backend Unit | Şablon render | `{{Name}}` doğru replace edildi |
| Frontend | Başarılı confirm URL | Checkmark animasyonu |
| Frontend | Expire URL | "Yeni gönder" ekranı |
| E2E | Tam akış | Kayıt → onay e-postası → tıkla → hoş geldin geldi |

---
---

# FAZ 4 — Admin Paneli (Auth + Abone Yönetimi)

> **Amaç:** JWT admin girişi, abone listesi yönetimi ve istatistik panelini tamamlamak. (req. 3.6, 8.4)

---

## 4.1 UI/UX Tasarımı

### Ekranlar

**Admin Giriş** (`/admin/login`)
- Merkez kart: kullanıcı adı + şifre + "Giriş Yap" butonu
- Hatalı girişte kırmızı alert
- Başarılıda `/admin`'e yönlendir

**Admin Layout** (tüm admin sayfaları)
- Sol Sidebar: Logo, nav linkleri (Dashboard, Aboneler, Bülten, Kampanyalar), çıkış butonu — yeşil aksanlı
- Üst Bar: Sayfa başlığı, admin kullanıcı adı
- Mobilhde hamburger menü

**Dashboard** (`/admin`)
- 4 istatistik kartı: Toplam, Aktif, Onaysız, Bugün eklenen
- Recharts line chart: son 30 günlük büyüme
- Son 5 kampanya tablosu

**Aboneler** (`/admin/subscribers`)
- Arama input'u (debounce 300ms)
- Filtre dropdown'ları: Aktif/Pasif, Onaylı/Onaysız
- Tablo: Id, İsim, E-posta, Onay, Aktif, Kayıt Tarihi, İşlemler
- İşlemler: "Pasife Al" + "Sil" (confirm dialog ile)
- Staggered fade-in (Framer Motion)

---

## 4.2 Mimari & Veritabanı

- Admin credentials: `appsettings.json`'da (kullanıcı adı + BCrypt hash)
- Yeni veritabanı tablosu gerekmez
- JWT: 8 saat geçerli; `[Authorize]` tüm `/api/admin/*` endpoint'lerine
- Büyüme grafiği için GROUP BY sorgusu

---

## 4.3 Kodlama Süreci

### Backend

**`POST /api/admin/login`**
1. `username` + `password` al
2. Stored username karşılaştır
3. BCrypt ile hash doğrula; uyuşmazsa `401`
4. JWT üret, dön

**`GET /api/admin/subscribers`** — `[Authorize]`
- Query: `?search=`, `?isActive=`, `?isConfirmed=`
- EF Core ile `Where()` zinciri; `OrderByDescending(SubscribedAt)`
- Güvenli DTO (token alanları hariç) dön

**`PUT /api/admin/subscribers/{id}/deactivate`** — `[Authorize]`
- `IsActive = false`, `UnsubscribedAt = DateTime.UtcNow`
- `204 No Content`

**`DELETE /api/admin/subscribers/{id}`** — `[Authorize]`
- Kaydı sil; `204 No Content`

**`GET /api/admin/stats`** — `[Authorize]`
```json
{ "total": 42, "active": 38, "unconfirmed": 5, "today": 3 }
```

**`GET /api/admin/subscribers/growth`** — `[Authorize]`
- Son 30 gün, gün bazında kayıt sayısı

### Frontend

**Protected Route:**
```jsx
const ProtectedRoute = ({ children }) => {
  const token = localStorage.getItem('adminToken');
  return token ? children : <Navigate to="/admin/login" />;
};
```

**Auth flow (`AdminLoginPage.jsx`):**
```jsx
const res = await api.post('/api/admin/login', { username, password });
localStorage.setItem('adminToken', res.data.token);
api.defaults.headers.common['Authorization'] = `Bearer ${res.data.token}`;
navigate('/admin');
```

**State yönetimi (`SubscribersPage.jsx`):**
```jsx
const [subscribers, setSubscribers] = useState([]);
const [search, setSearch] = useState('');
const [filters, setFilters] = useState({ isActive: '', isConfirmed: '' });

useEffect(() => {
  const timer = setTimeout(fetchSubscribers, 300);
  return () => clearTimeout(timer);
}, [search, filters]);
```

**Component Hiyerarşisi:**
```
AdminLayout
├── Sidebar → NavLinks
├── TopBar
└── Outlet
    ├── DashboardPage
    │   ├── StatsCards (x4)
    │   └── GrowthChart (Recharts)
    └── SubscribersPage
        ├── SearchBar
        ├── FilterDropdowns
        ├── SubscribersTable
        │   └── SubscriberRow
        └── ConfirmDialog
```

---

## 4.4 Test Stratejisi

| Test | Senaryo | Beklenen |
|------|---------|---------|
| Backend Unit | Yanlış şifre | `401` |
| Backend Unit | Doğru login | JWT token döner |
| Backend Unit | Token'siz admin endpoint | `401` |
| Backend Unit | Filtreli liste | Yalnızca uygun kayıtlar |
| Backend Unit | Deactivate | `IsActive=false` |
| Backend Unit | Stats | Doğru sayılar |
| Frontend | Giriş yapılmadan `/admin` | Login'e yönlendir |
| Frontend | Başarılı login | Dashboard açılır |
| Frontend | Arama | Tablo filtrelendi |
| Frontend | "Sil" butonu | Confirm dialog, satır kaybolur |

---
---

# FAZ 5 — Toplu Bülten Gönderimi (Newsletter)

> **Amaç:** Rich text editör ile bülten oluşturma, asenkron toplu gönderim ve gönderim geçmişini tamamlamak. (req. 3.7, 8.4)

---

## 5.1 UI/UX Tasarımı

**Bülten Oluşturma** (`/admin/newsletter`)
- Konu input (zorunlu)
- Rich text editör (react-quill / tiptap): kalın, italik, link, liste, başlık
- Canlı önizleme paneli (sağda veya altta)
- "Gönder" butonu → disabled + "Gönderiliyor..." state
- Başarıda toast: "Bülten kuyruğa alındı!"

**Kampanya Geçmişi** (`/admin/campaigns`)
- Tablo: Konu, Gönderilme Tarihi, Hedef Sayısı
- Faz 6'da open/click metrikleri eklenecek

---

## 5.2 Mimari & Veritabanı

### Yeni Tablolar

**Campaigns**

| Alan | Tip | Açıklama |
|------|-----|---------|
| Id | INT (PK) | |
| Subject | VARCHAR(255) | |
| HtmlBody | LONGTEXT | |
| SentAt | DATETIME | |
| RecipientCount | INT | |

**CampaignRecipients**

| Alan | Tip | Açıklama |
|------|-----|---------|
| Id | INT (PK) | |
| CampaignId | INT (FK) | |
| SubscriberId | INT (FK) | |
| SentAt | DATETIME | |
| Status | VARCHAR(20) | `sent` / `failed` |

Migration: `dotnet ef migrations add AddCampaignTables`

---

## 5.3 Kodlama Süreci

### Backend

**`POST /api/admin/newsletter`** — `[Authorize]`
1. `subject` + `htmlBody` al; validasyon yap
2. `IsActive=true AND IsConfirmed=true` aboneleri çek
3. `Campaign` kaydı oluştur, DB'ye kaydet
4. Her abone için: HtmlBody'ye unsubscribe linki ekle → `EmailJob` kuyruğa → `CampaignRecipient` kaydı
5. `Campaign.RecipientCount` güncelle
6. `202 Accepted` + `{ campaignId, recipientCount }` dön
7. Worker gönderdikten sonra `CampaignRecipient.Status` günceller: `"sent"` / `"failed"`

**`GET /api/admin/campaigns`** — `[Authorize]`
- `SentAt DESC` sırasıyla tüm kampanyaları dön

### Frontend

**`NewsletterPage.jsx`:**
```jsx
const handleSend = async () => {
  if (!subject || !htmlBody) return;
  setLoading(true);
  try {
    await api.post('/api/admin/newsletter', { subject, htmlBody });
    setToast({ type: 'success', message: 'Bülten kuyruğa alındı!' });
    setSubject(''); setHtmlBody('');
  } finally { setLoading(false); }
};
```

**Component Hiyerarşisi:**
```
NewsletterPage
├── SubjectInput
├── EditorPreviewLayout (flex/grid, iki sütun)
│   ├── RichTextEditor (react-quill)
│   │   └── Toolbar: kalın, italik, başlık, link, sırasız/sıralı liste
│   └── PreviewPanel
│       └── <iframe> veya div[dangerouslySetInnerHTML] ← editördeki HTML'i canlı render eder
└── SendButton (loading state)
```

**Canlı Önizleme Detayı:**
```jsx
// editördeki htmlBody state'i değiştikçe önizleme otomatik güncellenir
<div
  className="preview-panel"
  dangerouslySetInnerHTML={{ __html: htmlBody }}
/>
// NOT: dangerouslySetInnerHTML burada kabul edilebilir çünkü içeriği
// admin kullanıcısı kendisi yazıyor (güvenilir kaynak).
```

---

## 5.4 Test Stratejisi

| Test | Senaryo | Beklenen |
|------|---------|---------|
| Backend Unit | 5 aktif+onaylı abone | 5 EmailJob kuyruğa |
| Backend Unit | Pasif/onaysız aboneler | Gönderilmedi |
| Backend Unit | Campaign kaydı | DB'de, `RecipientCount=5` |
| Backend Unit | Worker gönderim başarısız | Status `failed`, log |
| Frontend | Boş subject | API çağrısı yapılmaz |
| Frontend | Başarılı gönderim | Toast, form temizlendi |
| Frontend | Kampanya listesi | Tablo gösterilir |

---
---

# FAZ 6 — Analitik (Open/Click Tracking)

> **Amaç:** E-posta açılma ve link tıklama takibini eklemek; admin panelinde metrikleri göstermek. (req. 3.8)
> Bu faz "nice to have" niteliğindedir.

---

## 6.1 UI/UX Tasarımı

**Kampanya Listesi güncellemesi** (`/admin/campaigns`)
- Yeni sütunlar: Açılan Sayısı | Açılma Oranı (%) | Tıklanan Sayısı | Tıklanma Oranı (%)
- Opsiyonel: Recharts donut/bar chart

---

## 6.2 Mimari & Veritabanı

**CampaignRecipients'a yeni alanlar:**

| Alan | Tip | Açıklama |
|------|-----|---------|
| OpenedAt | DATETIME, NULL | İlk açılışta set |
| ClickedAt | DATETIME, NULL | İlk tıklamada set |

**Yeni Tablo: TrackedLinks**

| Alan | Tip | Açıklama |
|------|-----|---------|
| Id | INT (PK) | |
| CampaignId | INT (FK) | |
| SubscriberId | INT (FK) | |
| OriginalUrl | VARCHAR(2048) | Hedef URL |
| LinkToken | VARCHAR(64), UNIQUE | Kısa token |
| ClickedAt | DATETIME, NULL | |

Migration: `dotnet ef migrations add AddAnalyticsTables`

---

## 6.3 Kodlama Süreci

### Backend

**`GET /api/track/open/{campaignId}/{subscriberId}`**
1. `CampaignRecipient` bul
2. `OpenedAt` null ise → `DateTime.UtcNow` set et
3. 1x1 şeffaf GIF dön:
   ```csharp
   var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
   return File(pixel, "image/gif");
   ```
4. `Cache-Control: no-cache` header

**`GET /api/track/click/{linkToken}`**
1. `TrackedLinks`'te token ile bul; yoksa `404`
2. `ClickedAt` null ise set et; `CampaignRecipient.ClickedAt` güncelle
3. `302 Redirect` → `OriginalUrl`

**Faz 5'i güncelle — Bülten gönderiminde:**
- Tracking pixel'i HtmlBody sonuna ekle:
  ```html
  <img src="{ApiBaseUrl}/api/track/open/{campaignId}/{subscriberId}" width="1" height="1" style="display:none"/>
  ```
- Her `<a href="...">` linkini wrap et: `TrackedLink` kaydı oluştur, href'i `/api/track/click/{token}` yap

**`GET /api/admin/campaigns/{id}/stats`** — `[Authorize]`
```json
{ "sent": 50, "opened": 20, "openRate": 40.0, "clicked": 8, "clickRate": 16.0 }
```

### Frontend

**`CampaignsPage.jsx`** — Her kampanya için stats endpoint'ini çağır, tabloya ekle.

---

## 6.4 Test Stratejisi

| Test | Senaryo | Beklenen |
|------|---------|---------|
| Backend Unit | Tracking pixel endpoint | 1x1 GIF, `OpenedAt` set |
| Backend Unit | Click redirect | 302, doğru URL, `ClickedAt` set |
| Backend Unit | Aynı abone 2. pixel | `OpenedAt` değişmez |
| Backend Unit | Bilinmeyen linkToken | `404` |
| Backend Unit | Stats endpoint | Doğru oranlar |
| Frontend | Kampanya listesi | Açılma oranları görünür |
| E2E | Gerçek e-posta istemcisi | Pixel yüklenince backend'e istek düştü |

---
---

# FAZ 7 — Deploy & Dokümantasyon (Opsiyonel / Son Adım)

> **Amaç:** Projeyi teslim/paylaşım için hazırlamak. (req. 9 — Açık Sorular)

---

## 7.1 README Hazırlama

Projenin kök klasörüne `README.md` oluştur. Asgari içerik:

- **Proje Açıklaması:** Ne yapıyor, hangi teknolojiler kullanılıyor?
- **Gereksinimler:** .NET SDK sürümü, Node.js, MySQL
- **Kurulum Adımları:**
  1. MySQL'de veritabanını oluştur
  2. `appsettings.json`'a bağlantı string'ini gir (veya user-secrets)
  3. `dotnet ef database update` ile migration uygula
  4. Gmail App Password al ve user-secrets'a ekle
  5. `dotnet run` (backend) + `npm run dev` (frontend)
- **Ortam Değişkenleri Tablosu:** Hangi değerin nereye girileceği
- **Ekran Görüntüleri** (opsiyonel ama etkili)

---

## 7.2 Deploy (Opsiyonel)

Proje sadece localhost'ta çalıştırılacaksa bu bölüm atlanabilir.

**Kolay deploy seçenekleri:**

| Katman | Seçenek | Not |
|--------|---------|-----|
| Backend | Azure App Service | `dotnet publish` + zip deploy |
| Frontend | Vercel / Netlify | `npm run build` → statik dosyalar |
| Veritabanı | PlanetScale / Railway | Ücretsiz MySQL hosting |

**HTTPS Production'da:**
- Azure / Vercel otomatik TLS sağlar.
- VPS'te: Nginx + Certbot (Let's Encrypt) ile ücretsiz SSL sertifikası.

---

## 7.3 Test Stratejisi

| Test | Kontrol |
|------|---------|
| README | Sıfırdan takip edince proje ayağa kalkıyor mu? |
| HTTPS | `http://` istekleri `https://`'e yönleniyor mu? |
| Tüm akış | Abone ol → onay → hoş geldin → admin giriş → bülten gönder → analitik |

---
---

# ✅ Tamamlanma Kontrol Listesi (Requirements ↔ Fazlar)

| Gereksinim | Faz |
|-----------|-----|
| Abonelik formu (e-posta + isim) | Faz 1 |
| Client + server tarafı validasyon | Faz 1 |
| Duplicate e-posta engelleme | Faz 1 |
| Double opt-in (onay linki) | Faz 3 |
| Token 24 saat geçerli | Faz 3 |
| Hoş geldin e-postası | Faz 3 |
| HTML şablonlar + kişiselleştirme | Faz 3 |
| Unsubscribe linki her e-postada | Faz 3, 5 |
| Gmail SMTP + App Password | Faz 2 |
| IEmailService soyutlaması | Faz 2 |
| Serilog loglama | Faz 0 |
| HTTP durum kodları (400, 409, 500) | Faz 1 |
| JWT admin auth | Faz 4 |
| Abone listesi + filtrele + ara | Faz 4 |
| Abone pasife al / sil | Faz 4 |
| İstatistik kartları | Faz 4 |
| Büyüme grafiği (Recharts) | Faz 4 |
| Rate limiting (spam koruması) | Faz 1 |
| **SQL injection & XSS koruması** | **Faz 1** |
| **HTTPS zorunluluğu** | **Faz 1** |
| **Honeypot bot koruması** | **Faz 1** |
| Erişilebilirlik (aria, label) | Faz 1 |
| Responsive tasarım | Faz 1, 4 |
| IHostedService + Channel kuyruk | Faz 2 |
| Retry mekanizması (max 3) | Faz 2 |
| Exponential backoff | Faz 2 |
| Rich text editör ile bülten | Faz 5 |
| Canlı önizleme (newsletter) | Faz 5 |
| Asenkron toplu gönderim | Faz 5 |
| Gönderim geçmişi (kampanya listesi) | Faz 5 |
| Tracking pixel (open tracking) | Faz 6 |
| Click tracking + redirect | Faz 6 |
| Open/click metrikleri (admin) | Faz 6 |
| Clean Minimal tasarım stili | Tüm Fazlar |
| Framer Motion animasyonları | Faz 1, 3, 4 |
| **README & kurulum dokümanı** | **Faz 7** |
| **Deploy (opsiyonel)** | **Faz 7** |

---

> **Başarılar! 🚀** Her fazı tamamladıkça bir sonrakine geç. Sormak istediğin bir şey olursa ilgili fazı belirt.
