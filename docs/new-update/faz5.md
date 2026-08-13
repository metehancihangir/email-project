# Faz 5 — Yayınlama (Release) Görev Listesi

**Kaynak:** `update-phases.md` — Bölüm 2 (Faz 5 tanımı), Bölüm 3 (Detaylı Teknik Uygulama), Bölüm "Özet: Faz → Talep Eşleşmesi"
**Bağımlılık:** Faz 4 (Entegrasyon, QA ve Regresyon Testleri) eksiksiz tamamlanmış ve tüm kritik test senaryoları "geçti" durumunda olmalıdır.
**Tahmini Süre:** 0.5 gün

---

## 1. Ön Kontrol / Release Gate

- [ ] 1.1 Faz 4 kapsamındaki tüm test maddelerinin (bölüm 4.1–4.7) "geçti" olarak işaretlendiğini doğrula; herhangi bir blocker/major bug açık değilse devam et. *(Madde: Özet Faz → Talep Eşleşmesi, Faz 4 → Faz 5 bağımlılığı)*
- [ ] 1.2 Talep 3 (onay kodu tekrar gönderme & rate-limiting) için kritik test senaryolarının (race condition, bypass, yük testi) staging ortamında son bir kez doğrulandığını teyit et. *(Madde: 3.3, 4.3)*
- [ ] 1.3 Release öncesi son bir `grep -ri "emailsubscriber"` taraması yaparak kod tabanında, config dosyalarında ve template'lerde "EmailSubscriber" ifadesi kalmadığını doğrula. *(Madde: 3.2.1, 4.2)*

## 2. Veritabanı Migration'larının Prod'a Uygulanması

- [ ] 2.1 `EmailVerifications` tablosuna `LastCodeRequestedAt` kolonunu ekleyen EF Core migration'ını (`AddLastCodeRequestedAtToEmailVerifications`) prod veritabanı yedeği alındıktan sonra çalıştır. *(Madde: 3.3 Veritabanı)*
- [ ] 2.2 `idx_emailverifications_email_lastrequested` (Email, LastCodeRequestedAt) index'inin prod'da başarıyla oluşturulduğunu doğrula. *(Madde: 3.3 Veritabanı)*
- [ ] 2.3 Migration sonrası var olan kayıtlarda `LastCodeRequestedAt` alanının beklendiği gibi `NULL` kaldığını ve ilk resend talebinde cooldown uygulanmadığını doğrula. *(Madde: 3.3 Veritabanı)*
- [ ] 2.4 Migration'ın rollback planını (staging'de test edilmiş script/komut) hazır bulundur ve prod'da hızlıca geri alınabileceğini teyit et. *(Madde: 4.7, madde 2)*
- [ ] 2.5 Migration uygulama işlemini düşük trafik saatinde planla ve migration süresince servis kesintisi olup olmadığını izle (downtime penceresi varsa önceden duyur). *(Madde: 2 — Faz 5 tanımı)*

## 3. Statik Varlıkların ve Marka Kimliğinin Yayına Alınması

- [ ] 3.1 `background-pattern-tile.svg` (arka plan deseni) dosyasının prod build/CDN içine dahil edildiğini doğrula. *(Madde: 3.1)*
- [ ] 3.2 `logo-icon.svg`, `wordmark.svg` ve favicon setinin (`favicon.ico`, `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png`) prod `public/` dizininde doğru şekilde deploy edildiğini doğrula. *(Madde: 3.2)*
- [ ] 3.3 Prod ortamında `<title>` etiketinin "SUBMAIL" olarak göründüğünü ve `public/manifest.json` içindeki `name`/`short_name` alanlarının güncel olduğunu doğrula. *(Madde: 3.2, 3.2.1)*
- [ ] 3.4 Güncellenmiş e-posta template'lerinin (`WelcomeEmail.html`, `SubscriptionConfirmationEmail.html`) ve gönderen adı/footer imzasının ("SUBMAIL ekibi") prod e-posta gönderim servisine yansıdığını doğrula. *(Madde: 3.2.1, 3.5, 3.6)*

## 4. Backend / API Yayınlama

- [ ] 4.1 `POST /api/auth/resend-code` ve `GET /api/auth/resend-code/status` endpoint'lerinin prod API'de aktif ve erişilebilir olduğunu doğrula. *(Madde: 3.3 Backend)*
- [ ] 4.2 Prod ortamında `429` yanıtlarında `Retry-After` header'ının ve `retryAfterSeconds` body alanının doğru döndüğünü smoke test ile doğrula. *(Madde: 3.3 Rate-Limiting Notu)*
- [ ] 4.3 Admin login geri butonu ile ilgili route değişikliğinin (client-side, backend etkisi yok) prod build'de doğru çalıştığını doğrula. *(Madde: 3.4)*

## 5. Feature Flag Yönetimi

- [ ] 5.1 Varsa, Talep 3 (resend-code & rate-limiting) için kullanılan feature flag'i prod ortamında kaldır/kalıcı olarak aç. *(Madde: 2 — Faz 5 tanımı: "feature flag (varsa) kaldırılması")*
- [ ] 5.2 Feature flag kaldırıldıktan sonra ilgili kod yollarının (fallback/eski davranış) temizlenip temizlenmeyeceğine karar ver; gerekiyorsa teknik borç kaydı aç. *(Madde: 2 — Faz 5 tanımı)*
- [ ] 5.3 Flag kaldırma işleminin canlıda beklenmedik bir davranışa yol açmadığını (ör. resend-code akışının hâlâ doğru çalıştığını) doğrula. *(Madde: 3.3)*

## 6. Deploy Sonrası Doğrulama (Post-Deploy Smoke Test)

- [ ] 6.1 Prod ortamında uçtan uca akışı test et: kayıt/login → onay kodu ekranı → "tekrar deneyin" → e-posta ile yeni kod alma. *(Madde: 3.3, 4.3)*
- [ ] 6.2 Admin login ekranında geri butonunun ve genel admin panel erişiminin prod'da sorunsuz çalıştığını doğrula. *(Madde: 3.4, 4.4)*
- [ ] 6.3 Prod'da giriş ve admin panel ekranlarında arka plan deseni, logo ve favicon'un doğru göründüğünü doğrula. *(Madde: 3.1, 3.2)*
- [ ] 6.4 Gerçek bir e-posta adresine hoş geldin ve abonelik onay e-postası gönderip Gmail/Outlook'ta emoji hizalaması ve zarf ikonunun taşmadığını doğrula. *(Madde: 3.5, 3.6)*
- [ ] 6.5 Deploy sonrası uygulama loglarını ve hata izleme (error tracking) aracını (varsa) en az bir süre izleyerek anormal hata artışı olmadığını doğrula. *(Madde: 2 — Faz 5 tanımı)*

## 7. Sürüm Notu ve İletişim

- [ ] 7.1 v1.1 sürüm notunu (release notes) hazırla; Talep 1–6 ve Ek 3.2.1 (SUBMAIL adlandırması) değişikliklerini madde madde listele. *(Madde: 2 — Faz 5 tanımı: "sürüm notu yayınlanması")*
- [ ] 7.2 Sürüm notunda "EmailSubscriber" isminin "SUBMAIL" olarak değiştiğini ve kullanıcıları/ekipleri etkileyebilecek görsel değişiklikleri (logo, favicon, arka plan) açıkça belirt. *(Madde: 3.2.1)*
- [ ] 7.3 Sürüm notunu ilgili paydaşlarla (ürün, destek, varsa müşteri iletişimi) paylaş ve yayınla. *(Madde: 2 — Faz 5 tanımı)*
- [ ] 7.4 Dahili ekiplere (destek/QA) yeni rate-limit davranışı (429, 2 dakikalık bekleme) hakkında kısa bir bilgilendirme notu ilet. *(Madde: 3.3)*

## 8. Release Kapanışı

- [ ] 8.1 Tüm deploy adımlarının (migration, statik varlıklar, backend, feature flag) prod'da başarıyla tamamlandığını checklist üzerinden teyit et. *(Madde: 2 — Faz 5 tanımı)*
- [ ] 8.2 Release'i proje takip aracında (Jira/Trello/vb.) "tamamlandı" olarak işaretle ve v1.1 sürümünü etiketle (git tag / sürüm numarası). *(Madde: 2 — Faz 5 tanımı)*
- [ ] 8.3 Rollback planının (migration + kod) belirli bir süre (ör. 24–48 saat) erişilebilir ve uygulanabilir durumda tutulduğunu teyit et. *(Madde: 4.7, madde 2)*
