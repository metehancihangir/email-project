# Faz 3: Onay & Hoş Geldin E-postası (Double Opt-in) - Teknik Görev Listesi

Bu belge, `phases.md` dosyasındaki Faz 3 gereksinimlerini uygulanabilir ve izlenebilir teknik görevlere ayırmaktadır. Her görevi tamamladıkça yanındaki kutucuğu (`[x]` → `[x]`) işaretleyebilirsiniz.

## 3.1 UI/UX Tasarımı
### Sayfalar (Frontend)
- [x] **3.1.1** `ConfirmPage.jsx` (Onay Sayfası) bileşeni oluşturulacak (`/confirm?token=...` rotası). (Ref: 3.1)
  - [x] Sayfa açıldığında API isteği yapılana kadar loading spinner gösterilecek. (Ref: 3.1)
  - [x] Başarılı onay durumunda: Animasyonlu yeşil checkmark, "Aboneliğiniz onaylandı!" mesajı ve "Ana Sayfaya Dön" butonu eklenecek. (Ref: 3.1)
  - [x] Token süresi geçmiş (Expired) durumunda: Kırmızı uyarı, e-posta giriş alanı ve "Yeni Onay E-postası Gönder" butonu eklenecek. (Ref: 3.1)
  - [x] Framer Motion geçişleri uygulanarak sayfalar/durumlar arası animasyon sağlanacak. (Ref: 3.1)
- [x] **3.1.2** `UnsubscribePage.jsx` (Abonelikten Çıkış) bileşeni oluşturulacak (`/unsubscribe?token=...` rotası). (Ref: 3.1)
  - [x] Başarılı çıkış durumunda: Gri/turuncu checkmark ve "Aboneliğiniz iptal edildi." mesajı gösterilecek. (Ref: 3.1)
  - [x] Hatalı token durumunda hata mesajı eklenecek. (Ref: 3.1)

### HTML E-posta Şablonları (Backend)
- [x] **3.1.3** `Templates/ConfirmationEmailTemplate.html` şablonu oluşturulacak (İçerisinde `{{Name}}` ve `{{ConfirmUrl}}` değişkenleri barındıracak). (Ref: 3.1)
- [x] **3.1.4** `Templates/WelcomeEmailTemplate.html` şablonu oluşturulacak (İçerisinde `{{Name}}` ve `{{UnsubscribeUrl}}` değişkenleri barındıracak). (Ref: 3.1)
- [x] **3.1.5** `IEmailTemplateService` arayüzü ve servisi yazılarak bu HTML dosyalarının okunması ve `{{Key}}` -> `value` eşleştirmelerinin yapılması (replace) sağlanacak. (Ref: 3.1)

## 3.2 Kodlama Süreci
### Backend
- [x] **3.2.1** `GET /api/subscribers/confirm/{token}` endpoint'i oluşturulacak: (Ref: 3.2)
  - [x] Token geçersizse `404 Not Found` dönülecek. (Ref: 3.2)
  - [x] Token var fakat süresi dolmuşsa (`ConfirmationTokenExpiresAt < DateTime.UtcNow`) `410 Gone` dönülecek. (Ref: 3.2)
  - [x] Zaten onaylanmışsa (`IsConfirmed = true`) `200 OK` (idempotent) dönülecek. (Ref: 3.2)
  - [x] Token geçerliyse: `IsConfirmed = true`, token alanları `null` yapılacak, DB güncellenecek, "Hoş geldin" e-postası kuyruğa (Queue) eklenecek ve `200 OK` dönülecek. (Ref: 3.2)
- [x] **3.2.2** `POST /api/subscribers/resend-confirmation` endpoint'i oluşturulacak: (Ref: 3.2)
  - [x] İstekten e-posta alınacak. Kayıtlı ve `IsConfirmed = false` olduğu doğrulanacak. (Ref: 3.2)
  - [x] Yeni bir token ve 24 saatlik expiry tarihi oluşturulup veritabanı güncellenecek. (Ref: 3.2)
  - [x] Yeni onay e-postası kuyruğa (Queue) eklenecek ve `202 Accepted` dönülecek. (Ref: 3.2)
- [x] **3.2.3** `GET /api/subscribers/unsubscribe/{token}` endpoint'i oluşturulacak: (Ref: 3.2)
  - [x] Token geçersizse `404 Not Found` dönülecek. (Ref: 3.2)
  - [x] Token geçerliyse `IsActive = false` ve `UnsubscribedAt = DateTime.UtcNow` olarak işaretlenecek ve `200 OK` dönülecek. (Ref: 3.2)

### Frontend
- [x] **3.2.4** `ConfirmPage.jsx` içerisinde `useEffect` ile url'den alınan token alınarak `api.get('/api/subscribers/confirm/{token}')` çağrısı yapılacak. Gelen yanıta (status) göre UI güncellenecek (`success`, `expired`, `error`). (Ref: 3.2)

## 3.3 Test Stratejisi
### Backend Unit Testleri
- [x] **3.3.1** Geçerli onay token'ı gönderildiğinde DB'nin güncellendiği (`IsConfirmed=true`) ve Hoş geldin e-postasının kuyruğa eklendiği test edilecek. (Ref: 3.4)
- [x] **3.3.2** Süresi geçmiş (Expired) token gönderildiğinde `410 Gone` döndüğü test edilecek. (Ref: 3.4)
- [x] **3.3.3** Bilinmeyen/geçersiz token durumunda `404 Not Found` döndüğü test edilecek. (Ref: 3.4)
- [x] **3.3.4** Aynı geçerli token tekrar gönderildiğinde hata fırlatmadan `200 OK` (idempotent) döndüğü test edilecek. (Ref: 3.4)
- [x] **3.3.5** Unsubscribe (İptal) endpoint'ine token gönderildiğinde `IsActive=false` olduğu ve `UnsubscribedAt` tarihinin atandığı test edilecek. (Ref: 3.4)
- [x] **3.3.6** `IEmailTemplateService`'in HTML şablonlarındaki `{{Name}}` gibi alanları doğru şekilde replace ettiği test edilecek. (Ref: 3.4)

### Frontend & E2E Testleri
- [x] **3.3.7** Frontend'de başarılı confirm URL'ine girildiğinde animasyonlu Checkmark'ın çıktığı doğrulanacak. (Ref: 3.4)
- [x] **3.3.8** Frontend'de expire olmuş bir URL girildiğinde (410 yanıtı gelince) "Yeni gönder" ekranının (ExpiredCard) çıktığı doğrulanacak. (Ref: 3.4)
- [x] **3.3.9** (Opsiyonel / E2E) Tam akış: Kayıt formundan istek atma -> gelen e-postaya tıklama -> onaylanması -> Hoş geldin e-postasının gelmesi manuel/otomatik test ile doğrulanacak. (Ref: 3.4)
