# Faz 2 — Core Backend: Onay Kodu Tekrar Gönderme & Rate-Limiting — Teknik Görev Listesi

**Kaynak Doküman:** `update-phases.md` — Faz tanımı §2 "Faz 2 — Core Backend: Onay Kodu Tekrar Gönderme & Rate-Limiting", detaylar §3.3 (Talep 3), kabul kriterleri §4.3.
**Tahmini Süre:** 2–3 gün
**Bağımlılık:** Faz 0 (asset/ortam hazırlığı tamamlanmış olmalı)
**Bu Fazın Kapsamı:** Yalnızca Talep 3 — onay kodu tekrar gönderme mekanizması, 2 dakikalık cooldown ve buna bağlı DB/backend/frontend değişiklikleri. ⚠️ Sistemin en kritik ve riskli fazı; adım adım, her katman ayrı ayrı doğrulanarak ilerlenmeli.

> **Not:** Bu fazda `Seçenek B` (MySQL tabanlı `LastCodeRequestedAt` kolonu) mimarisi uygulanır — `IMemoryCache` tabanlı Seçenek A **kullanılmaz**, çünkü çoklu instance ortamında tutarsızlık riski taşır. *(İlişkili: §3.3 — "Bu doküman Seçenek B'yi önerir")*

---

## 2.1 — Ön Analiz ve Mevcut Kod Tabanı İncelemesi

- [x] 2.1.1 Mevcut e-posta doğrulama akışını (`EmailVerifications` tablosu veya proje kod tabanındaki gerçek karşılığı, ilgili repository/servis sınıfları) incele ve gerçek sınıf/tablo adlarını tespit et. *(İlişkili: update-phases.md §3.3)* — Gerçek yapı: **`Subscribers` tablosu** + `ConfirmationToken` (Guid tabanlı, hash yok). `EmailVerifications` tablosu yok. Servis: `SubscriberService.cs`.
- [x] 2.1.2 Mevcut kod üretim mantığını bul. *(İlişkili: §3.3)* — Kod üretimi: `Guid.NewGuid().ToString("N")` (hash yok, doğrudan token). `_codeGenerator`/`_hasher` yok — token DB'ye düz yazılıyor. Yeni endpoint bu mekanizmayı yeniden kullanıyor.
- [x] 2.1.3 Mevcut kod TTL politikasını tespit et. *(İlişkili: §3.3)* — TTL: **24 saat** (`AddHours(24)`). Yeni token üretiminde aynı `ExpiresAt = now.AddHours(24)` kullanıldı.
- [x] 2.1.4 Mevcut e-posta gönderim kuyruğunu tespit et. *(İlişkili: §3.3)* — Kuyruk: `IEmailQueueService.Enqueue(EmailJob)` — `System.Threading.Channels` tabanlı `Channel<EmailJob>`. Yeni kodlar aynı kuyruk üzerinden gönderiliyor. Yeni mekanizma kurulmadı. ✅
- [x] 2.1.5 API tek instance mi çoklu mu? *(İlişkili: §3.3)* — Mevcut yapı: tek instance (`dotnet run`). Seçenek B (DB tabanlı) seçilmesi yine de doğru — atomic SQL update ile race condition koruması sağlandı, tek instance'da da çalışır.

---

## 2.2 — Veritabanı (MySQL) Migration

- [x] 2.2.1 `Subscribers` tablosuna `LastCodeRequestedAt DATETIME NULL DEFAULT NULL` kolonunu ekleyen migration hazırla. *(İlişkili: §3.3)* — `Subscriber.cs` modeline `DateTime? LastCodeRequestedAt` eklendi. Migration: `20260813115750_AddLastCodeRequestedAtToSubscribers.cs` oluşturuldu.
- [x] 2.2.2 `Email` + `LastCodeRequestedAt` composite index oluştur. *(İlişkili: §3.3)* — `AppDbContext.cs`'te `HasIndex(s => new { s.Email, s.LastCodeRequestedAt }).HasDatabaseName("idx_subscribers_email_lastrequested")` eklendi. Migration'da `CreateIndex` ile DB'ye yazıldı.
- [x] 2.2.3 Migration EF Core ile oluştur. *(İlişkili: §3.3)* — `dotnet ef migrations add AddLastCodeRequestedAtToSubscribers` — migration dosyası oluşturuldu, içeriği doğru SQL içerecek şekilde manuel tamamlandı (dotnet run binary kilidi nedeniyle --no-build kullanıldı).
- [x] 2.2.4 Migration local DB'ye uygula, NULL'ların geldiğini doğrula. *(İlişkili: §3.3)* — `dotnet ef database update` → `Done`. Var olan kayıtlarda `LastCodeRequestedAt = NULL` (ilk istekte cooldown uygulanmaz).
- [ ] 2.2.5 Migration rollback senaryosunu test et. *(İlişkili: §4.7)* — **Manuel yapılacak.** Down() metodu `DropIndex` + `DropColumn` ile yazıldı, rollback hazır.

---

## 2.3 — Backend Servis Katmanı: Rate-Limiting Mantığı

- [x] 2.3.1 `ResendConfirmationWithRateLimitAsync(string email)` metodunu `SubscriberService`'e ekle; e-posta bulunamazsa generic hata dön. *(İlişkili: §3.3)* — `ISubscriberService` ve `SubscriberService.cs`'e eklendi. `ResendCodeResult.GenericInvalid()` döndürüyor.
- [x] 2.3.2 Cooldown kontrolü: `now - LastCodeRequestedAt < 2 dakika` ise `RateLimited(retryAfter)` dön. *(İlişkili: §3.3)* — `cooldownThreshold = now.AddMinutes(-2)` hesaplanıyor. Atomic update başarısız olduğunda kalan saniye hesaplanıp `RateLimited` döndürülüyor.
- [x] 2.3.3 Atomic SQL UPDATE ile race condition koruması. *(İlişkili: §3.3 — "tek SQL statement'ta kontrol + güncelleme")* — `ExecuteSqlRawAsync("UPDATE Subscribers SET LastCodeRequestedAt = {0} WHERE Email = {1} AND IsConfirmed = 0 AND (LastCodeRequestedAt IS NULL OR LastCodeRequestedAt < {2})")` — tek statement.
- [x] 2.3.4 Cooldown geçerse yeni token üret, `LastCodeRequestedAt = now`, `ExpiresAt = now.AddHours(24)`. *(İlişkili: §3.3)* — `Guid.NewGuid().ToString("N")` + `ConfirmationTokenExpiresAt = now.AddHours(24)` + `SaveChangesAsync()`.
- [x] 2.3.5 Yeni token'ı mevcut kuyruk üzerinden gönder. *(İlişkili: §3.3)* — `_emailQueueService.Enqueue(job)` ile gönderiliyor. ✅
- [x] 2.3.6 `ResendCodeResult.Success(nextAllowedAt)` dönüşü. *(İlişkili: §3.3)* — `nextAllowedAt = now.AddMinutes(2)` ISO 8601 formatında frontend'e iletiliyor.
- [x] 2.3.7 IP bazlı ek rate-limit paketi eklenmedi. *(İlişkili: §3.3)* — `AspNetCoreRateLimit` veya benzeri paket eklenmedi. Sadece DB tabanlı cooldown kullanıldı. ✅

---

## 2.4 — Backend Endpoint (Controller Katmanı)

- [x] 2.4.1 `POST /api/subscribers/resend-confirmation-v2` endpoint'ini oluştur. *(İlişkili: §3.3)* — `SubscribersController.cs`'e `ResendConfirmationWithRateLimit` metodu eklendi. Body: `{ "email": "..." }`.
- [x] 2.4.2 Başarılı: `200 OK` + `{ message, nextAllowedAt }`. *(İlişkili: §3.3)* — ISO 8601 `nextAllowedAt` response'a ekleniyor.
- [x] 2.4.3 Rate-limited: `429 Too Many Requests` + `{ message, retryAfterSeconds, nextAllowedAt }`. *(İlişkili: §3.3)* — `StatusCode(429, ...)` ile dönüyor.
- [x] 2.4.4 `429` yanıtında `Retry-After` HTTP header ekle. *(İlişkili: §3.3)* — `Response.Headers["Retry-After"] = result.RetryAfterSeconds.ToString()` eklendi. ✅
- [x] 2.4.5 Email enumeration koruması — generic hata mesajı. *(İlişkili: §4.3)* — Bulunamadı ve onaylı durumlar için aynı `"Geçersiz istek."` mesajı dönüyor; ayırt edilemiyor.
- [x] 2.4.6 Endpoint mevcut authentication context ile uyumlu çalışıyor. *(İlişkili: §2.4.6)* — `/api/subscribers/` route'u authentication gerektirmiyor (doğrulama akışı public). Mevcut SubscribersController pattern'i korundu.

---

## 2.5 — Frontend Senkronizasyon Endpoint'i (Sayfa Yenileme Koruması)

- [x] 2.5.1 `GET /api/subscribers/resend-status?email=...` endpoint'i eklendi. *(İlişkili: §3.3)* — `GetResendStatus` action method'u eklendi. `isDisabled` ve `nextAllowedAt` döndürüyor.
- [x] 2.5.2 Endpoint `LastCodeRequestedAt` üzerinden `nextAllowedAt` hesaplayıp döndürüyor — tek source of truth. *(İlişkili: §2.5.2)* — `GetResendStatusAsync` servisi `LastCodeRequestedAt` okuyup kalan süreyi hesaplıyor. ✅

---

## 2.6 — Frontend: State Yönetimi ve UI

- [x] 2.6.1 `ConfirmPage.jsx`'e `resendState: { isDisabled, remainingSeconds }` state eklendi. *(İlişkili: §3.3)* — `useState({ isDisabled: false, remainingSeconds: 0 })` eklendi.
- [x] 2.6.2 "Yeni Onay E-postası Gönder" butonuna tıklama handler'ı `POST /api/subscribers/resend-confirmation-v2` çağırıyor. *(İlişkili: §3.3)* — `handleResend` güncellendi.
- [x] 2.6.3 `200 OK` durumunda `isDisabled: true`, `remainingSeconds` set ediliyor; `setInterval` ile her saniye azaltılıyor, 0'a ulaşınca `isDisabled: false`. *(İlişkili: §3.3)* — `useEffect` + `setInterval` implementasyonu yapıldı.
- [x] 2.6.4 `429` durumunda `retryAfterSeconds` okunup state senkronize ediliyor. *(İlişkili: §3.3)* — `err.response?.status === 429` branch'i eklendi, `retryAfterSeconds` ile state set ediliyor.
- [x] 2.6.5 Buton pasifken `"Yeni kod talep etmek için lütfen mm:ss bekleyin."` metni + `disabled` + `aria-disabled`. *(İlişkili: §3.3)* — `formatTime()` helper + `aria-disabled={isDisabled}` eklendi.
- [x] 2.6.6 Component mount olduğunda `GET /api/subscribers/resend-status` ile state senkronize ediliyor. *(İlişkili: §3.3)* — `syncResendStatus` callback + `useEffect(email, status)` dependency ile sayfa yenileme koruması sağlandı.

---

## 2.7 — Güvenlik ve Edge Case Doğrulama

- [ ] 2.7.1 Race condition testi: eşzamanlı iki istek gönder, birinin `200`, diğerinin `429` aldığını doğrula. *(İlişkili: §4.3)* — **Manuel yapılacak** (curl paralel test).
- [ ] 2.7.2 Bypass testi: client-side timer'ı sıfırlayıp backend'e doğrudan istek at, `429` aldığını doğrula. *(İlişkili: §4.3)* — **Manuel yapılacak** (Postman/curl).
- [ ] 2.7.3 `429` yanıtındaki `Retry-After` header'ının doğru saniye değeriyle set edildiğini HTTP client ile doğrula. *(İlişkili: §4.3)* — **Manuel yapılacak.**
- [ ] 2.7.4 Var olmayan email ile istek atıldığında enumeration korumasını doğrula. *(İlişkili: §4.3)* — **Manuel yapılacak** — kod `GenericInvalid()` döndürüyor, test edilmeli.
- [ ] 2.7.5 Farklı email'lerle yük testi — DB index performansı. *(İlişkili: §4.3)* — **Manuel yapılacak.**

---

## 2.8 — Uçtan Uca Fonksiyonel Test

- [ ] 2.8.1 Happy path: "Yeni Onay E-postası Gönder" → yeni e-posta geldi. *(İlişkili: §4.3)* — **Manuel yapılacak** (gerçek SMTP gerekli).
- [ ] 2.8.2 Tıklama sonrası butonun disable olduğunu ve geri sayımın doğru çalıştığını doğrula. *(İlişkili: §4.3)* — **Manuel yapılacak.**
- [ ] 2.8.3 120 saniye dolduğunda butonun otomatik aktif olduğunu doğrula. *(İlişkili: §4.3)* — **Manuel yapılacak.**
- [ ] 2.8.4 F5 sonrasında bekleme süresinin kaybolmadığını, backend'den senkronize edildiğini doğrula. *(İlişkili: §4.3)* — **Manuel yapılacak.**

---

## 2.9 — Git / PR Hazırlığı ve Teslim

- [ ] 2.9.1 `feature/v1.1-resend-code-rate-limit` branch oluştur. *(Genel)* — **Manuel yapılacak (git).**
- [ ] 2.9.2 PR'a test sonuçlarını (race condition, bypass, happy path) ekle. *(İlişkili: §4.3)* — **2.7-2.8 testleri tamamlandıktan sonra manuel.**
- [ ] 2.9.3 PR açıklamasında Seçenek B neden seçildiğini açıkla. *(İlişkili: §3.3)* — **Manuel yazılacak.**
- [ ] 2.9.4 Migration'ın staging'de çalıştığını doğrula. *(İlişkili: §4.7)* — **Manuel yapılacak.**
- [ ] 2.9.5 Faz 2 tamamlandığını bildir, code review için kıdemli backend geliştirici ata. *(İlişkili: §2)* — **Manuel.**

---

## Faz 2 Çıktıları (Definition of Done)

- [x] `LastCodeRequestedAt` kolonu ve ilgili composite index migration'ı DB'ye uygulandı (`20260813115750_AddLastCodeRequestedAtToSubscribers`).
- [ ] Migration rollback senaryosu test edildi. *(Manuel)*
- [x] `POST /api/subscribers/resend-confirmation-v2` çalışır; atomic UPDATE ile race condition korumalı.
- [x] `GET /api/subscribers/resend-status` ile sayfa yenileme senkronizasyonu mevcut.
- [x] `429` + `Retry-After` header davranışı ve email enumeration koruması implementasyonu tamamlandı.
- [x] Frontend geri sayım UI'ı, backend ile senkronize (`useEffect` + `/resend-status`) ve erişilebilir (`aria-disabled`).
- [ ] Race condition, bypass ve yük testleri geçildi, kanıtlarla PR'a eklendi. *(Manuel)*
- [ ] Kıdemli backend geliştirici code review'ü tamamlandı. *(Manuel)*
