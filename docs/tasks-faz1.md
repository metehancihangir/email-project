# Faz 1: Abonelik Formu (Public) - Teknik Görev Listesi

Bu belge, `phases.md` dosyasındaki Faz 1 gereksinimlerini uygulanabilir ve izlenebilir teknik görevlere ayırmaktadır. Her görevi tamamladıkça yanındaki kutucuğu (`[x]`) işaretleyebilirsiniz.

## 1.1 UI/UX Tasarımı (Frontend)
- [x] **1.1.1** Navbar bileşeni oluşturulacak (Logo solda, "Admin Girişi" linki sağda). (Ref: 1.1)
- [x] **1.1.2** Hero Bölümü geliştirilecek (Büyük başlık, kısa açıklama, icon ve metin içeren 3 fayda maddesi). (Ref: 1.1)
- [x] **1.1.3** Form Kartı bileşeni geliştirilecek:
  - [x] Opsiyonel İsim alanı (`<input type="text" />`) eklenecek. (Ref: 1.1)
  - [x] Zorunlu E-posta alanı (`<input type="email" required />`) eklenecek. (Ref: 1.1)
  - [x] Tam genişlikte, "emerald green" renkli "Abone Ol" butonu eklenecek. (Ref: 1.1)
- [x] **1.1.4** Form içi kullanıcı etkileşimleri ve durum (state) geçişleri yapılacak:
  - [x] İnputlara focus olunduğunda yeşil border ve subtle shadow (CSS transition ile) gösterilecek. (Ref: 1.1)
  - [x] Form submit edildiğinde buton `disabled` olacak ve loading spinner (yükleniyor ikonu) gösterilecek. (Ref: 1.1)
- [x] **1.1.5** Toast / Alert bileşeni oluşturulacak:
  - [x] Tüm sayfalarda kullanılabilecek, `success`, `error`, `info`, `warning` tiplerini destekleyecek. (Ref: 1.1)
  - [x] Framer Motion kullanılarak `opacity: 0→1` ve `y: -20→0` animasyonu verilecek. (Ref: 1.1)
  - [x] 4 saniye sonra otomatik kapanacak şekilde ayarlanacak. (Ref: 1.1)
- [x] **1.1.6** Toast bildirimlerinin senaryolara entegrasyonu:
  - [x] Başarılı kayıtta animasyonlu yeşil toast çıkacak. (Ref: 1.1)
  - [x] Hata durumunda animasyonlu kırmızı toast çıkacak. (Ref: 1.1)
  - [x] Duplicate (zaten kayıtlı) e-posta durumunda özel uyarı mesajı ("Bu e-posta adresi zaten kayıtlı.") verilecek. (Ref: 1.1)
- [x] **1.1.7** Responsive tasarım: Sayfa düzeni masaüstünde iki sütun, mobilde tek sütun olacak. (Ref: 1.1)
- [x] **1.1.8** Erişilebilirlik (A11y): Form elemanlarına `aria-label`, `aria-required` ve `htmlFor` etiketleri eklenecek. (Ref: 1.1)

## 1.2 Mimari & Veritabanı ve Güvenlik (Backend & Frontend)
- [x] **1.2.1** Rate Limiting eklenecek: `Program.cs`'de aynı IP'den 1 dakikada maksimum 3 isteğe izin verilecek şekilde yapılandırılacak. (Ref: 1.2)
- [x] **1.2.2** `ISubscriberService` arayüzü tanımlanacak ve dependency injection (DI) olarak kaydedilecek. (Ref: 1.2)
- [x] **1.2.3** Model sınıflarına güvenlik ve kısıtlama kuralları (SQL Injection & XSS önlemleri) eklenecek:
  - [x] `Name` alanı için `[MaxLength(100)]` kısıtlaması eklenecek. (Ref: 1.2)
- [x] **1.2.4** HTTPS zorunluluğu: `Program.cs` içerisine `app.UseHttpsRedirection();` eklendiğinden emin olunacak. (Ref: 1.2)
- [x] **1.2.5** Honeypot (Bot Koruması) entegrasyonu:
  - [x] Frontend formuna CSS ile gizlenmiş (`display:none`, `tabIndex={-1}`) `website` isimli bir honeypot input eklenecek. (Ref: 1.2)
  - [x] Backend'de bu alanın dolu gelmesi durumunda bot olarak algılanıp istek `400 Bad Request` ile reddedilecek. (Ref: 1.2)

## 1.3 Kodlama Süreci
### Backend
- [x] **1.3.1** `SubscribeRequest` record/DTO'su oluşturulacak (Email: Required, EmailAddress / Name: MaxLength(100) + Honeypot alanı). (Ref: 1.3)
- [x] **1.3.2** `POST /api/subscribers` endpoint'i yazılacak ve iş mantığı (business logic) implemente edilecek:
  - [x] E-posta format validasyonu (geçersizse `400 Bad Request`). (Ref: 1.3)
  - [x] DB'de kayıt kontrolü yapılacak:
    - [x] Zaten var ve `IsConfirmed = true` ise `409 Conflict`. (Ref: 1.3)
    - [x] `IsConfirmed = false` ve onay token'ı geçerliyse "Onay e-postası zaten gönderildi" uyarısı dönülecek. (Ref: 1.3)
    - [x] `IsConfirmed = false` ve token süresi dolmuşsa yeni token üretilip ilerlenecek. (Ref: 1.3)
  - [x] Yeni abone nesnesi oluşturulup alanları set edilecek (`IsConfirmed = false`, `IsActive = true`). (Ref: 1.3)
  - [x] `ConfirmationToken` üretilecek (`Guid.NewGuid().ToString("N")`). (Ref: 1.3)
  - [x] `ConfirmationTokenExpiresAt` değeri `DateTime.UtcNow.AddHours(24)` olarak set edilecek. (Ref: 1.3)
  - [x] Yeni/güncellenmiş kayıt veritabanına kaydedilecek. (Ref: 1.3)
  - [x] E-posta gönderimi simüle edilecek (`IEmailService.SendAsync` çağrılacak). (Ref: 1.3)
  - [x] İşlem sonucunda `202 Accepted` HTTP status kodu dönülecek. (Ref: 1.3)

### Frontend
- [x] **1.3.3** Frontend Component Hiyerarşisi oluşturulacak (`SubscribePage`, `Navbar`, `HeroSection`, `FeatureList`, `SubscribeForm`). (Ref: 1.3)
- [x] **1.3.4** Form State Yönetimi: `SubscribeForm.jsx` içerisinde `name`, `email`, `loading`, `toast` stateleri (useState) tanımlanacak. (Ref: 1.3)
- [x] **1.3.5** Client-side Validasyon: `utils/validation.js` dosyası oluşturulup içine `isValidEmail` fonksiyonu (regex) yazılacak. (Ref: 1.3)
- [x] **1.3.6** Form Submit (Axios) Entegrasyonu: `handleSubmit` metodunda API isteği (`api.post`) atılacak, hata/başarı durumlarında loading state kapatılacak ve uygun toast gösterilecek. (Ref: 1.3)

## 1.4 Test Stratejisi
### Backend Unit Testleri
- [x] **1.4.1** Geçerli e-posta ile POST isteğinin `202 Accepted` döndüğü ve veritabanına eklendiği test edilecek. (Ref: 1.4)
- [x] **1.4.2** Geçersiz formatta e-posta gönderildiğinde `400 Bad Request` alındığı test edilecek. (Ref: 1.4)
- [x] **1.4.3** Aynı (onaylanmış) e-posta tekrar gönderildiğinde `409 Conflict` alındığı test edilecek. (Ref: 1.4)
- [x] **1.4.4** Aynı IP adresinden limit (3) aşılacak şekilde üst üste istek atıldığında `429 Too Many Requests` döndüğü test edilecek. (Ref: 1.4)

### Frontend & E2E Testleri
- [x] **1.4.5** Boş veya geçersiz formatlı form gönderilmek istendiğinde API'ye istek gitmeden client-side hata (toast) verildiği test edilecek. (Ref: 1.4)
- [x] **1.4.6** Başarılı bir yanıtta toast bileşeninin çıktığı ve form alanlarının (inputların) sıfırlandığı test edilecek. (Ref: 1.4)
- [x] **1.4.7** Zaten kayıtlı uyarısı (409 hatası) döndüğünde, kullanıcıya uygun toast mesajının gösterildiği doğrulanacak. (Ref: 1.4)
- [x] **1.4.8** Tüm akış (Kullanıcı giriş yapar -> Submit eder -> Toast görür -> DB'ye düşer) E2E olarak manuel test edilecek. (Ref: 1.4)
