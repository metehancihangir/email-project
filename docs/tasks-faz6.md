# Faz 6: Analitik (Open/Click Tracking) - Teknik Görev Listesi

Bu belge, `phases.md` dosyasındaki Faz 6 gereksinimlerini uygulanabilir ve izlenebilir teknik görevlere ayırmaktadır. Her görevi tamamladıkça yanındaki kutucuğu (`[ ]` → `[x]`) işaretleyebilirsiniz.

## 6.1 Veritabanı ve Mimari (Backend)
- [x] **6.1.1** `CampaignRecipients` tablosuna (Entity modeline) `OpenedAt` (DateTime, nullable) ve `ClickedAt` (DateTime, nullable) özellikleri eklenecek. (Ref: 6.2)
- [x] **6.1.2** `TrackedLinks` tablosu için Entity oluşturulacak. İçeriğinde `Id`, `CampaignId`, `SubscriberId`, `OriginalUrl`, `LinkToken` (unique, string) ve `ClickedAt` alanları bulunacak. (Ref: 6.2)
- [x] **6.1.3** DbContext içerisine `TrackedLinks` DbSet'i eklenecek, foreign key ilişkileri ve `LinkToken` için unique index konfigürasyonları yapılacak. (Ref: 6.2)
- [x] **6.1.4** Yeni tablo ve güncellemeler için `AddAnalyticsTables` adıyla EF Core Migration oluşturulacak ve veritabanı güncellenecek. (Ref: 6.2)

## 6.2 Backend API Kodlama
- [x] **6.2.1** E-posta açılma (open) takibi için `GET /api/track/open/{campaignId}/{subscriberId}` endpoint'i eklenecek. (Ref: 6.3)
  - [x] İlgili `CampaignRecipient` kaydı bulunacak.
  - [x] `OpenedAt` değeri `null` ise, güncel zaman `DateTime.UtcNow` ile güncellenip kaydedilecek.
  - [x] Yanıt olarak `1x1` boyutunda şeffaf bir GIF dosyası `Cache-Control: no-cache` başlığı (header) ile dönülecek.
- [x] **6.2.2** Tıklama (click) takibi için `GET /api/track/click/{linkToken}` endpoint'i eklenecek. (Ref: 6.3)
  - [x] `TrackedLinks` tablosunda `LinkToken` üzerinden kayıt aranacak; bulunamazsa `404 Not Found` dönülecek.
  - [x] `TrackedLink.ClickedAt` alanı boşsa güncellenecek, aynı zamanda bağlantılı `CampaignRecipient.ClickedAt` alanı da güncellenecek.
  - [x] Ziyaretçi `OriginalUrl` adresine `302 Redirect` (Yönlendirme) ile aktarılacak.
- [x] **6.2.3** Bülten gönderim mantığı (Faz 5 / Newsletter endpoint) güncellenecek. (Ref: 6.3)
  - [x] Oluşturulan HTML gövdesinin (HtmlBody) en sonuna `<img src="{ApiBaseUrl}/api/track/open/{campaignId}/{subscriberId}" width="1" height="1" style="display:none"/>` pikseli (tracking pixel) eklenecek.
  - [x] E-posta gövdesindeki tüm bağlantılar (`<a href="...">`) regex/HtmlAgilityPack ile bulunup, `TrackedLink` kaydı oluşturulacak ve `href` adresleri `/api/track/click/{token}` şekline dönüştürülecek.
- [x] **6.2.4** Kampanya verilerini dönen `GET /api/admin/campaigns` ve/veya `GET /api/admin/campaigns/{id}/stats` endpoint'leri, gönderim (sent), açılma (opened), açılma oranı (openRate), tıklanma (clicked) ve tıklanma oranı (clickRate) metriklerini de döndürecek şekilde geliştirilecek. (Ref: 6.3)

## 6.3 UI/UX Tasarımı & Frontend Ekranları
- [x] **6.3.1** `CampaignsPage.jsx` bileşeni güncellenecek. Analitik verileri için tabloya "Açılan Sayısı", "Açılma Oranı (%)", "Tıklanan Sayısı" ve "Tıklanma Oranı (%)" sütunları eklenecek. (Ref: 6.1, 6.3)
- [x] **6.3.2** (Opsiyonel) Açılma/Tıklanma oranlarını göstermek için UI tarafına grafikler (donut/bar chart) eklenecek. (Ref: 6.1)

## 6.4 Test Stratejisi
### Backend Unit / Integration Testleri
- [x] **6.4.1** Tracking pixel (open) endpoint'ine atılan isteğin 1x1 GIF döndüğü ve ilk istekte `OpenedAt` alanını set ettiği, aynı abonenin ikinci isteğinde ise `OpenedAt` değerinin değişmediği (idempotent) test edilecek. (Ref: 6.4)
- [x] **6.4.2** Tıklama (click redirect) endpoint'ine atılan isteğin 302 döndürdüğü, doğru hedef URL'ye yönlendirdiği ve `ClickedAt` alanını set ettiği test edilecek. (Ref: 6.4)
- [x] **6.4.3** Bilinmeyen/Geçersiz bir `linkToken` ile tıklama endpoint'ine girildiğinde `404 Not Found` döndüğü test edilecek. (Ref: 6.4)
- [x] **6.4.4** Kampanya istatistikleri (stats) endpoint'inin hesaplanan açık ve tıklama oranlarını (sent, opened, clicked) doğru verdiği test edilecek. (Ref: 6.4)

### Frontend & E2E Testleri
- [x] **6.4.5** Frontend tarafında kampanya listesinde analitik verilerin (Açılma oranları, tıklanma sayıları vs.) doğru formatta gösterildiği test edilecek. (Ref: 6.4)
- [x] **6.4.6** Gerçek/Örnek bir e-posta render'ında, HTML içeriğindeki linklerin API formatıyla sarıldığı (wrap edildiği) ve `<img/>` takibinin (tracking pixel) otomatik eklendiği kontrol edilecek. (Ref: 6.4)
