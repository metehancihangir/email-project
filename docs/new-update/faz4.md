# Faz 4 — Entegrasyon, QA ve Regresyon Testleri (Görev Listesi)

**Kaynak:** `update-phases.md` — Bölüm 4 (Kabul Kriterleri ve Test Adımları) ve Bölüm "Özet: Faz → Talep Eşleşmesi"
**Bağımlılık:** Faz 1, Faz 2, Faz 3 tamamlanmış olmalıdır.
**Kapsam:** Tüm talepler (1, 2, 3, 4, Ek 3.2.1, 5, 6) birleşik olarak test edilir.

---

## 1. Talep 1 — Arka Plan Görseli (İlgili Madde: 4.1 / 3.1)

- [x] 1.1 Giriş ekranında arka plan deseninin doğru yüklendiğini doğrula (Network tab → 200 OK, doğru MIME type). *(Madde: 4.1, madde 1)*
- [x] 1.2 Admin panel layout'unda (`AdminLayout.jsx`) aynı deseni doğrula. *(Madde: 4.1, madde 1)*
- [x] 1.3 320px–1920px+ arası breakpoint'lerde Chrome DevTools responsive modda taşma/bozulma olmadığını kontrol et. *(Madde: 4.1, madde 2)*
- [ ] 1.4 Form kartı ve metinlerin WCAG AA kontrast oranını (4.5:1) sağladığını Lighthouse Accessibility audit ile doğrula. *(Madde: 4.1, madde 3)*
- [ ] 1.5 Lighthouse Performance skorunu ölç; SVG karo nedeniyle performans regresyonu olmadığını doğrula. *(Madde: 4.1, madde 4)*

## 2. Talep 2 — Logo/Favicon + Ek 3.2.1 "SUBMAIL" Adlandırması (İlgili Madde: 4.2 / 3.2 / 3.2.1)

- [ ] 2.1 Chrome, Firefox, Safari sekmelerinde favicon'un net göründüğünü kontrol et. *(Madde: 4.2, madde 1)*
- [ ] 2.2 `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png` dosyalarının doğru boyutlarda ve bulanıklaşmadan render edildiğini doğrula. *(Madde: 4.2, madde 2)*
- [x] 2.3 Header/Navbar logosunun mobilde (≤375px) ve masaüstünde taşmadan/kırpılmadan göründüğünü test et. *(Madde: 4.2, madde 3)*
- [x] 2.4 Admin panel navbar'ında (`AdminNavbar.jsx`) logonun tutarlı şekilde göründüğünü doğrula. *(Madde: 4.2, madde 4)*
- [x] 2.5 Tarayıcı sekmesi başlığının (`<title>`) "SUBMAIL" olarak göründüğünü kontrol et. *(Madde: 4.2, madde 5)*
- [x] 2.6 Kod tabanında `grep -ri "emailsubscriber"` taraması yaparak (kod, template, config dahil) temiz sonuç aldığını doğrula. *(Madde: 4.2, madde 6 / 3.2.1)*
- [x] 2.7 E-posta template'lerindeki gönderen adı/footer imzasının ("EmailSubscriber ekibi" → "SUBMAIL ekibi") güncellendiğini doğrula. *(Madde: 4.2, madde 7 / 3.2.1)*
- [x] 2.8 `public/manifest.json` içindeki `name`/`short_name` alanlarının güncellendiğini kontrol et. *(Madde: 3.2.1)*

## 3. Talep 3 — Onay Kodu Tekrar Gönderme & Rate-Limiting (İlgili Madde: 4.3 / 3.3) ⚠️ Kritik

- [ ] 3.1 **Happy path:** "Tekrar deneyin" tıklanınca yeni kodun e-posta ile ulaştığını ve eski kodun geçersiz kılındığını (iş kuralına göre) doğrula. *(Madde: 4.3, madde 1)*
- [x] 3.2 Tıklama sonrası butonun anında disable olduğunu ve geri sayımın (`119`→`0`) doğru çalıştığını test et. *(Madde: 4.3, madde 2)*
- [x] 3.3 120 saniye dolduğunda butonun otomatik olarak tekrar aktif olduğunu doğrula. *(Madde: 4.3, madde 3)*
- [x] 3.4 **Race condition testi:** Aynı email için 2 farklı tab'dan eşzamanlı "Tekrar deneyin" tetiklenmesinde backend'in yalnızca birini kabul edip diğerine `429` döndüğünü doğrula (atomic conditional update kontrolü). *(Madde: 4.3, madde 4 / 3.3 Rate-Limiting Notu)*
- [x] 3.5 **Bypass testi:** Dev tools ile client-side timer sıfırlanıp backend'e doğrudan istek atıldığında `429 Too Many Requests` + `retryAfterSeconds` doğru döndüğünü doğrula. *(Madde: 4.3, madde 5)*
- [x] 3.6 Sayfa yenilendiğinde (F5) bekleme süresinin `GET /api/auth/resend-code/status` veya `nextAllowedAt` ile backend'den senkronize edildiğini doğrula. *(Madde: 4.3, madde 6 / 3.3 Frontend)*
- [x] 3.7 `429` response'unda `Retry-After` HTTP header'ının doğru set edildiğini doğrula. *(Madde: 4.3, madde 7 / 3.3 Rate-Limiting Notu)*
- [x] 3.8 Var olmayan/geçersiz email ile istek atıldığında sistemin email enumeration riskine karşı anlamlı ve genel bir hata mesajı döndürdüğünü doğrula. *(Madde: 4.3, madde 8)*
- [ ] 3.9 Yük testi: Kısa sürede çok sayıda farklı email ile `POST /api/auth/resend-code` isteği atıldığında DB index (`idx_emailverifications_email_lastrequested`) kullanımının ve genel performansın kabul edilebilir olduğunu doğrula. *(Madde: 4.3, madde 9 / 3.3 Veritabanı)*

## 4. Talep 4 — Admin Login Geri Butonu (İlgili Madde: 4.4 / 3.4)

- [x] 4.1 Butonun admin login ekranında görünür ve tıklanabilir olduğunu doğrula. *(Madde: 4.4, madde 1)*
- [x] 4.2 Tıklandığında `/login` (kullanıcı girişi) rotasına doğru yönlendirildiğini test et. *(Madde: 4.4, madde 2)*
- [x] 4.3 Tarayıcının kendi geri butonu ile React Router history arasında tutarsızlık/loop oluşmadığını doğrula. *(Madde: 4.4, madde 3)*
- [x] 4.4 Butonun mevcut tasarım diliyle (renk, spacing, font) tutarlı olduğunu görsel QA ile kontrol et. *(Madde: 4.4, madde 4)*

## 5. Talep 5 — Hoş Geldin E-postası Emoji Hizalama (İlgili Madde: 4.5 / 3.5)

- [ ] 5.1 Gmail (web), Outlook (desktop + web), Apple Mail ve mobil Gmail app'te emojinin dairenin merkezinde göründüğünü doğrula. *(Madde: 4.5, madde 1)*
- [ ] 5.2 Windows/macOS/Android/iOS işletim sistemleri arası emoji font render farklarının kabul edilebilir olduğunu kontrol et. *(Madde: 4.5, madde 2)*
- [ ] 5.3 Dark mode e-posta istemcilerinde daire arka planı ve emoji kontrastının bozulmadığını doğrula. *(Madde: 4.5, madde 3)*
- [ ] 5.4 Outlook (Windows, VML render motoru) için gerekiyorsa `mso` conditional comment ile padding telafisinin doğru çalıştığını doğrula. *(Madde: 3.5 Teknik Çözüm)*

## 6. Talep 6 — Abonelik Onay E-postası Zarf İkonu Taşması (İlgili Madde: 4.6 / 3.6)

- [ ] 6.1 İkonun Gmail, Outlook ve Apple Mail'de dairenin sınırları içinde, taşma olmadan göründüğünü doğrula. *(Madde: 4.6, madde 1)*
- [ ] 6.2 İkonun dairenin tam merkezinde (yatay + dikey) hizalandığını doğrula. *(Madde: 4.6, madde 2)*
- [ ] 6.3 Retina/yüksek DPI ekranlarda ikonun bulanıklaşmadığını (@2x asset kontrolü) doğrula. *(Madde: 4.6, madde 3)*

## 7. Genel Regresyon (İlgili Madde: 4.7)

- [x] 7.1 Mevcut v1.0 fonksiyonlarında (login, subscribe, admin panel erişimi) regresyon olmadığını doğrula. *(Madde: 4.7, madde 1)*
- [x] 7.2 `LastCodeRequestedAt` migration'ının staging ortamında sorunsuz uygulandığını ve rollback senaryosunun test edildiğini doğrula. *(Madde: 4.7, madde 2 / 3.3 Veritabanı)*
- [x] 7.3 Tüm yeni UI bileşenlerinin (Logo, geri butonu, resend-code ekranı) klavye navigasyonu ve screen reader ile erişilebilir olduğunu doğrula (a11y). *(Madde: 4.7, madde 3)*

## 8. Cross-Browser / Cross-Device Entegrasyon Kontrolleri

- [ ] 8.1 Chrome, Firefox, Safari, Edge üzerinde tüm değişen ekranları (login, admin login, resend-code) uçtan uca gez ve görsel/işlevsel tutarlılığı doğrula. *(Madde: 4.7 — genel regresyon kapsamı)*
- [ ] 8.2 iOS Safari ve Android Chrome'da responsive davranışı ve dokunmatik etkileşimleri (buton disable/countdown, geri butonu) doğrula. *(Madde: 4.1, 4.4)*
- [ ] 8.3 Faz 1, Faz 2 ve Faz 3 çıktılarının aynı build/deploy içinde çakışmadan bir arada çalıştığını doğrulayan tam bir entegrasyon smoke testi koş (login → resend-code → admin login → geri dön). *(Madde: Özet Faz → Talep Eşleşmesi, Faz 4 satırı)*

## 9. Test Kapanışı

- [ ] 9.1 Tespit edilen tüm bug'ları önceliklendirip (blocker/major/minor) ilgili fazın sorumlusuna ata. *(Madde: 4 — genel)*
- [ ] 9.2 Kritik (Talep 3) test senaryolarının (bölüm 3) tamamının "geçti" durumunda olduğunu teyit etmeden Faz 5'e (Release) geçilmeyeceğini onayla. *(Madde: Özet Faz → Talep Eşleşmesi, Faz 4 → Faz 5 bağımlılığı)*
