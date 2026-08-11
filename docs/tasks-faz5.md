# Faz 5: Toplu Bülten Gönderimi (Newsletter) - Teknik Görev Listesi

Bu belge, `phases.md` dosyasındaki Faz 5 gereksinimlerini uygulanabilir ve izlenebilir teknik görevlere ayırmaktadır. Her görevi tamamladıkça yanındaki kutucuğu (`[ ]` → `[x]`) işaretleyebilirsiniz.

## 5.1 Veritabanı ve Mimari (Backend)
- [x] **5.1.1** `Campaigns` tablosu için Entity oluşturulacak. İçeriğinde `Id`, `Subject`, `HtmlBody`, `SentAt` ve `RecipientCount` özellikleri bulunacak. (Ref: 5.2)
- [x] **5.1.2** `CampaignRecipients` tablosu için Entity oluşturulacak. `Id`, `CampaignId`, `SubscriberId`, `SentAt` ve `Status` (`sent`/`failed`) alanları ile Foreign Key ilişkileri kurulacak. (Ref: 5.2)
- [x] **5.1.3** DbContext içerisine `Campaigns` ve `CampaignRecipients` DbSet'leri eklenecek, mapping konfigürasyonları yapılacak. (Ref: 5.2)
- [x] **5.1.4** Yeni tablolar için `AddCampaignTables` adıyla EF Core Migration oluşturulacak ve veritabanı güncellenecek. (Ref: 5.2)

## 5.2 Backend API Kodlama
- [x] **5.2.1** `POST /api/admin/newsletter` endpoint'i (`[Authorize]`) eklenecek. (Ref: 5.3)
  - [x] Request body'den `subject` ve `htmlBody` alınacak, validasyonları yapılacak.
  - [x] Veritabanından `IsActive=true` VE `IsConfirmed=true` olan tüm aboneler çekilecek.
  - [x] Yeni bir `Campaign` kaydı oluşturulup DB'ye eklenecek.
  - [x] Her bir aktif abone için `HtmlBody` içerisine abonelikten çıkma (unsubscribe) linki enjekte edilecek.
  - [x] Her abone için `EmailJob` arkaplan kuyruğuna (Channel) gönderilecek.
  - [x] İlgili `CampaignRecipient` kayıtları oluşturulacak.
  - [x] `Campaign.RecipientCount` güncellenerek DB'ye kaydedilecek ve `202 Accepted` (campaignId, recipientCount) dönülecek.
- [x] **5.2.2** Email Worker servisi (Mailing Queue) güncellenerek, e-posta gönderimi sonrasında `CampaignRecipient.Status` bilgisini `sent` ya da `failed` olarak veritabanında (Scoped DbContext ile) güncelleyecek. (Ref: 5.3)
- [x] **5.2.3** `GET /api/admin/campaigns` endpoint'i (`[Authorize]`) eklenecek. Tüm kampanyaları `SentAt DESC` (tarihe göre azalan) sırasında dönecek. (Ref: 5.3)

## 5.3 UI/UX Tasarımı & Frontend Ekranları
- [x] **5.3.1** `react-quill` veya eşdeğer Rich Text Editor kütüphanesi projeye (npm) eklenecek. (Ref: 5.1)
- [x] **5.3.2** `NewsletterPage.jsx` (Bülten Oluşturma) bileşeni (`/admin/newsletter`) oluşturulacak. (Ref: 5.1, 5.3)
  - [x] "Konu" (Subject) için zorunlu input eklenecek.
  - [x] Rich text editör (Toolbar: kalın, italik, başlık, link, liste) sayfaya entegre edilecek.
  - [x] Editörün hemen yanına veya altına Canlı Önizleme (Preview Panel) paneli konulacak. (HTML içeriği `dangerouslySetInnerHTML` ile render edilecek).
  - [x] Gönder butonuna tıklandığında yükleniyor (loading) state'i aktif edilecek ve işlem bitene kadar buton devre dışı (disabled) kalacak.
  - [x] Başarılı gönderim sonucunda "Bülten kuyruğa alındı!" şeklinde yeşil bir Toast/Alert mesajı gösterilecek ve form temizlenecek.
- [x] **5.3.3** `CampaignsPage.jsx` (Kampanya Geçmişi) bileşeni (`/admin/campaigns`) oluşturulacak. (Ref: 5.1)
  - [x] Tablo içerisinde "Konu", "Gönderilme Tarihi", ve "Hedef Sayısı (Alıcı Sayısı)" bilgileri gösterilecek.

## 5.4 Test Stratejisi
### Backend Unit / Integration Testleri
- [x] **5.4.1** Sadece Aktif ve Onaylı (`IsActive=true AND IsConfirmed=true`) aboneler için kuyruğa `EmailJob` atıldığı, diğerlerinin atlandığı (ignore) test edilecek. (Ref: 5.4)
- [x] **5.4.2** Doğru girişlerle Newsletter endpoint'inin veritabanında `Campaign` kaydı oluşturduğu ve `RecipientCount` miktarının doğru olduğu test edilecek. (Ref: 5.4)
- [x] **5.4.3** Email Worker içinde gönderim başarısız olduğunda `CampaignRecipient` Status alanının `failed` olarak işaretlendiği test edilecek. (Ref: 5.4)

### Frontend Testleri
- [x] **5.4.4** Konu (`subject`) boşken Newsletter gönder butonunun API çağrısı yapmadığı (frontend validasyonu) test edilecek. (Ref: 5.4)
- [x] **5.4.5** Başarılı bülten gönderiminin ardından Toast mesajının çıktığı ve editör formunun temizlendiği doğrulanacak. (Ref: 5.4)
- [x] **5.4.6** Kampanyalar (Campaigns) sayfasının açıldığı ve API'den gelen verilerin tabloya başarıyla yansıdığı test edilecek. (Ref: 5.4)
