# Faz 2: E-posta Kuyruk Altyapısı - Teknik Görev Listesi

Bu belge, `phases.md` dosyasındaki Faz 2 gereksinimlerini uygulanabilir ve izlenebilir teknik görevlere ayırmaktadır. Her görevi tamamladıkça yanındaki kutucuğu (`[x]` → `[x]`) işaretleyebilirsiniz.

## 2.1 UI/UX Tasarımı
- [x] **2.1.1** UI/UX tarafında bir bileşen eklenmeyecek, ancak form submit sonrası API'nin arka plan kuyruğu sayesinde daha hızlı (anında) yanıt verdiği doğrulanacak. (Ref: 2.1)

## 2.2 Mimari
- [x] **2.2.1** E-posta gönderim işlemindeki veri yapısını tutmak için `EmailJob` isimli record/DTO oluşturulacak (`To`, `ToName`, `Subject`, `HtmlBody`). (Ref: 2.2)

## 2.3 Kodlama Süreci
### Backend (Kuyruk Altyapısı)
- [x] **2.3.1** In-memory kuyruk yönetimi için `IEmailQueueService` arayüzü tanımlanacak (`Enqueue`, `DequeueAllAsync`). (Ref: 2.3)
- [x] **2.3.2** `EmailQueueService` sınıfı implemente edilecek (içerisinde asenkron kuyruk için `Channel.CreateUnbounded<EmailJob>()` kullanılacak). (Ref: 2.3)
- [x] **2.3.3** Arka plan görevi olarak çalışacak `EmailWorker` (BackgroundService) sınıfı oluşturulacak:
  - [x] Kuyruktan (`_queue.DequeueAllAsync`) gelen e-posta görevleri sırayla dinlenecek. (Ref: 2.3)
  - [x] Gönderim başarısız olursa maksimum 3 kez tekrar denenecek (MaxRetry = 3). (Ref: 2.3)
  - [x] Başarısız denemeler arasında "Exponential backoff" (katlanarak artan bekleme süresi) uygulanacak. (Ref: 2.3)
  - [x] Her denemede başarılı ve başarısız durumlara göre uygun uyarı/hata (Warning/Error) logları atılacak. (Ref: 2.3)

### Backend (SMTP & Refactoring)
- [x] **2.3.4** Gerçek e-posta gönderimi için `IEmailService` arayüzü tanımlanacak (`SendAsync`). (Ref: 2.3)
- [x] **2.3.5** `MailKitEmailService` sınıfı yazılarak, `MailKit` kütüphanesi üzerinden `appsettings.json` SMTP ayarları ile asıl e-posta gönderimi sağlanacak. (Ref: 2.3)
- [x] **2.3.6** `Program.cs` güncellenerek Dependency Injection (DI) kayıtları eklenecek:
  - [x] `IEmailQueueService` → Singleton olarak eklenecek. (Ref: 2.3)
  - [x] `EmailWorker` → `AddHostedService<EmailWorker>()` olarak eklenecek. (Ref: 2.3)
  - [x] `IEmailService` → Scoped veya Transient olarak eklenecek. (Ref: 2.3)
- [x] **2.3.7** Faz 1'deki `SubscriberService` güncellenerek, kayıt onay simülasyonu yerine `IEmailQueueService.Enqueue` metodu çağrılıp onay e-postası görevi kuyruğa eklenecek. (Ref: 2.3)

## 2.4 Test Stratejisi
### Backend Unit Testleri
- [x] **2.4.1** `Enqueue()` çağrıldığında worker'ın (veya channel'ın) mesajı başarıyla aldığı test edilecek. (Ref: 2.4)
- [x] **2.4.2** SMTP hatası (mocklanarak) oluştuğunda, sistemin e-postayı tam 3 kez göndermeyi denediği (retry) test edilecek. (Ref: 2.4)
- [x] **2.4.3** "Exponential backoff" mantığının doğru çalıştığı ve her başarısız adımda bekleme sürelerinin arttığı doğrulanacak. (Ref: 2.4)

### Integration & Performans Testleri
- [x] **2.4.4** Gerçek (veya test amaçlı) SMTP bilgileri ile entegrasyon testi/manuel test yapılarak e-postanın alıcıya ulaştığı doğrulanacak. (Ref: 2.4)
- [x] **2.4.5** Yeni abone eklenirken API yanıt süresinin < 200ms olduğu, e-posta gönderim gecikmesinin frontend'i (kullanıcıyı) bloklamadığı test edilecek. (Ref: 2.4)
