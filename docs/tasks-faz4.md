# Faz 4: Admin Paneli (Auth + Abone Yönetimi) - Teknik Görev Listesi

Bu belge, `phases.md` dosyasındaki Faz 4 gereksinimlerini uygulanabilir ve izlenebilir teknik görevlere ayırmaktadır. Her görevi tamamladıkça yanındaki kutucuğu (`[x]` → `[x]`) işaretleyebilirsiniz.

## 4.1 UI/UX Tasarımı & Frontend Ekranları
### Temel Admin Bileşenleri
- [x] **4.1.1** `AdminLayout.jsx` oluşturulacak. İçerisinde yeşil aksanlı Sol Sidebar (Dashboard, Aboneler, Bülten, Kampanyalar linkleri ve Çıkış butonu) ve Üst Bar yer alacak. Mobil için hamburger menü uyumlu olacak. (Ref: 4.1, 4.3)
- [x] **4.1.2** `ProtectedRoute.jsx` bileşeni oluşturulacak. `localStorage` içinde `adminToken` kontrolü yapıp, yoksa `/admin/login` sayfasına yönlendirecek. (Ref: 4.3)

### Sayfalar
- [x] **4.1.3** `AdminLoginPage.jsx` (Admin Giriş) bileşeni oluşturulacak (`/admin/login`). Kullanıcı adı, şifre formu ve hata durumlarında kırmızı alert gösterimi içerecek. (Ref: 4.1)
- [x] **4.1.4** `DashboardPage.jsx` bileşeni oluşturulacak (`/admin`). (Ref: 4.1)
  - [x] 4 adet istatistik kartı (Toplam, Aktif, Onaysız, Bugün eklenen) barındıracak.
  - [x] Recharts kütüphanesi kullanılarak son 30 günlük büyüme çizgi grafiği (line chart) eklenecek.
  - [x] Son 5 kampanya için tablo yapısı kurulacak.
- [x] **4.1.5** `SubscribersPage.jsx` (Aboneler) bileşeni oluşturulacak (`/admin/subscribers`). (Ref: 4.1, 4.3)
  - [x] Arama input'u (debounce 300ms) ve Filtre Dropdown'ları (Aktif/Pasif, Onaylı/Onaysız) eklenecek.
  - [x] Abone Tablosu (Id, İsim, E-posta, Onay, Aktif, Kayıt Tarihi, İşlemler sütunlarıyla) oluşturulacak.
  - [x] İşlemler menüsünde "Pasife Al" ve "Sil" (Confirm dialog ile) butonları yer alacak.
  - [x] Staggered fade-in (Framer Motion) animasyonları tablo satırlarına uygulanacak.

## 4.2 Mimari & Veritabanı (Backend Hazırlık)
- [x] **4.2.1** `appsettings.json` içerisine Admin kimlik bilgileri (kullanıcı adı ve BCrypt ile hashlenmiş şifre) eklenecek. (Ref: 4.2)
- [x] **4.2.2** JWT Authentication servisi 8 saat geçerli olacak şekilde yapılandırılmış (Faz 0'dan), tüm `/api/admin/*` endpoint'leri için `[Authorize]` attributü kullanılacağı teyit edilecek. (Ref: 4.2)

## 4.3 Kodlama Süreci (Backend API)
- [x] **4.3.1** `POST /api/admin/login` endpoint'i oluşturulacak. (Ref: 4.3)
  - [x] Gönderilen `username` ve `password` değerleri config'deki BCrypt hash ile kıyaslanacak.
  - [x] Uyuşmazlıkta `401 Unauthorized`, başarılı durumda JWT üretilip dönülecek.
- [x] **4.3.2** `GET /api/admin/subscribers` endpoint'i (`[Authorize]`) eklenecek. (Ref: 4.3)
  - [x] `?search=`, `?isActive=`, `?isConfirmed=` query parametreleriyle filtreleme ve EF Core üzerinden `Where` zinciri oluşturulacak.
  - [x] `OrderByDescending(SubscribedAt)` uygulanarak güvenli (token içermeyen) DTO listesi dönülecek.
- [x] **4.3.3** `PUT /api/admin/subscribers/{id}/deactivate` endpoint'i (`[Authorize]`) eklenecek. İlgili kayıt `IsActive = false`, `UnsubscribedAt = DateTime.UtcNow` yapılarak `204 No Content` dönülecek. (Ref: 4.3)
- [x] **4.3.4** `DELETE /api/admin/subscribers/{id}` endpoint'i (`[Authorize]`) eklenecek. Kayıt DB'den silinecek ve `204 No Content` dönülecek. (Ref: 4.3)
- [x] **4.3.5** `GET /api/admin/stats` endpoint'i (`[Authorize]`) eklenecek. Toplam, aktif, onaysız ve bugün eklenen abone sayıları JSON objesi olarak dönülecek. (Ref: 4.3)
- [x] **4.3.6** `GET /api/admin/subscribers/growth` endpoint'i (`[Authorize]`) eklenecek. Son 30 günlük, gün bazında (GROUP BY) yeni kayıt sayıları dönülecek. (Ref: 4.3)

## 4.4 Test Stratejisi
### Backend Unit / Integration Testleri
- [x] **4.4.1** Yanlış şifre ile login denemesinde `401 Unauthorized` döndüğü test edilecek. (Ref: 4.4)
- [x] **4.4.2** Doğru credential'lar ile login olunduğunda JWT token döndüğü test edilecek. (Ref: 4.4)
- [x] **4.4.3** Token gönderilmeden (Unauthorized) admin endpoint'lerine istek atıldığında `401` alındığı test edilecek. (Ref: 4.4)
- [x] **4.4.4** `/subscribers` endpoint'inde filtreleme yapıldığında (örneğin sadece aktifler) yalnızca uygun kayıtların geldiği test edilecek. (Ref: 4.4)
- [x] **4.4.5** Deactivate endpoint'inin bir aboneyi pasife aldığı (`IsActive=false`) test edilecek. (Ref: 4.4)
- [x] **4.4.6** Stats endpoint'inden dönen objenin sayılarının doğru olduğu test edilecek. (Ref: 4.4)

### Frontend Testleri
- [x] **4.4.7** Frontend'de yetkisiz olarak `/admin` rotasına girildiğinde `/admin/login` sayfasına yönlendirildiği doğrulanacak. (Ref: 4.4)
- [x] **4.4.8** Başarılı login sonrasında Dashboard sayfasının (`/admin`) sorunsuz açıldığı görülecek. (Ref: 4.4)
- [x] **4.4.9** Aboneler sayfasındaki arama input'una değer girildiğinde tablonun anlık filtrelendiği test edilecek. (Ref: 4.4)
- [x] **4.4.10** "Sil" butonuna basıldığında onay (Confirm) iletişim kutusunun (dialog) açıldığı ve onay sonrası satırın kaybolduğu doğrulanacak. (Ref: 4.4)
