# Faz 1 — Hotfix: E-posta Şablon Düzeltmeleri (Bug) — Teknik Görev Listesi

**Kaynak Doküman:** `update-phases.md` — Faz tanımı §2 "Faz 1 — Hotfix: E-posta Şablon Düzeltmeleri (Bug)", detaylar §3.5 (Talep 5), §3.6 (Talep 6), kabul kriterleri §4.5, §4.6.
**Tahmini Süre:** 0.5–1 gün
**Bağımlılık:** Faz 0 (asset/ortam hazırlığı tamamlanmış olmalı)
**Bu Fazın Kapsamı:** Yalnızca Talep 5 (emoji hizalama) ve Talep 6 (zarf ikonu taşması) — iki mail template dosyasındaki HTML/inline CSS düzeltmeleri. Backend servis kodu, frontend veya DB değişikliği içermez.

---

## 1.1 — Ortam Hazırlığı ve Template Dosyalarının Tespiti

- [x] 1.1.1 `Templates/Emails/` dizinini incele, "Aramıza Hoş Geldiniz" e-postasına ait dosyayı bul (varsayılan ad: `WelcomeEmail.html`). *(İlişkili: update-phases.md §3.5 — "Değişiklik Yeri")* — Dosya: `EmailSubscriber.API/Templates/WelcomeEmailTemplate.html`
- [x] 1.1.2 Aynı dizinde "E-Bülten Abonelik Onayı" e-postasına ait dosyayı bul (varsayılan ad: `SubscriptionConfirmationEmail.html`). *(İlişkili: §3.6 — "Değişiklik Yeri")* — Dosya: `EmailSubscriber.API/Templates/ConfirmationEmailTemplate.html`
- [x] 1.1.3 Her iki template dosyasının backend içinde nasıl render edildiğini (Razor, string interpolation, harici HTML dosyası vb.) tespit et; değişikliklerin `IEmailSender` katmanına dokunmadan salt HTML/CSS seviyesinde kalabileceğini teyit et. *(İlişkili: §3.5, §3.6 — "Backend (.NET Core API): Sadece template güncellenir")* — `EmailTemplateService.cs`: `File.ReadAllTextAsync` + `string.Replace("{{Name}}", ...)`. Gönderim servisi (`MailKitEmailService`) ve `IEmailSender` katmanına hiç dokunulmadı. ✅
- [ ] 1.1.4 Her iki template'in mevcut (düzeltme öncesi) halini bir test e-posta adresine gönderip Gmail (web), Outlook (desktop), Apple Mail'de referans ekran görüntüsü al — düzeltme sonrası karşılaştırma için baseline oluştur. *(İlişkili: §4.5, §4.6 — çapraz istemci testi hazırlığı)* — **Manuel yapılacak** (gerçek SMTP gönderimi gerekli).

---

## 1.2 — Talep 5: Hoş Geldin E-postası - Emoji Hizalama Düzeltmesi

- [x] 1.2.1 `WelcomeEmail.html` içindeki mevcut emoji/daire bloğunu (muhtemelen flexbox veya yanlış `line-height` kullanan bir yapı) tespit et ve mevcut kodu yedekle (git diff ile takip edilecek). *(İlişkili: §3.5 — "Açıklama": emoji dairenin merkezinde değil)* — Eski kod: `.logo { display: inline-flex; align-items: center; justify-content: center; font-size: 32px; line-height: 64px; }` — Outlook'ta inline-flex/flexbox çalışmıyor, dikey kayma oluşuyordu.
- [x] 1.2.2 Bloğu tablo tabanlı (table-based) yapıya çevir: `<table role="presentation" width="64" height="64" cellpadding="0" cellspacing="0">` + tek `<td>` içinde `align="center" valign="middle"`. *(İlişkili: §3.5 — "Teknik Çözüm" kod örneği)* — `WelcomeEmailTemplate.html` güncellendi. `<div class="logo">` kaldırıldı, table-based yapı eklendi.
- [x] 1.2.3 `<td>` üzerinde `text-align:center; vertical-align:middle; font-size:28px; line-height:1;` stillerini uygula — dikey kaymayı önlemek için `line-height:1` değerinin sabit kaldığını doğrula. *(İlişkili: §3.5 — "`line-height:1` ve `vertical-align:middle` kombinasyonu")* — TD stilinde `text-align:center; vertical-align:middle; font-size:28px; line-height:1;` uygulandı. ✅
- [x] 1.2.4 Dairenin arka plan rengini (`background-color:#22c55e` veya mevcut projede kullanılan yeşil ton) ve `border-radius:50%` değerlerini eski koddan birebir koru — sadece hizalama mekanizması değişmeli, görsel stil (renk, boyut) aynı kalmalı. *(İlişkili: §3.5 — kabul kriteri: "mevcut kutlama emojisi" görünümü korunmalı)* — `background-color:#10b981` (projenin birincil rengi) ve `border-radius:50%` korundu. Boyut 64x64px aynı. ✅
- [x] 1.2.5 Outlook (Windows, VML render motoru) için gerekiyorsa `<!--[if mso]-->` conditional comment ile ek padding telafisi ekle; ilk testte kayma görülmezse bu adımı atla ve not düş. *(İlişkili: §3.5 — "Outlook ... `mso` conditional comment ile ek padding telafisi")* — `<!--[if mso]-->...<!--[if !mso]><!-->` conditional comment'ler eklendi. MSO için `border-radius:32px` (px değeri, Outlook VML uyumlu) kullanıldı.

---

## 1.3 — Talep 6: Abonelik Onay E-postası - Zarf İkonu Taşma Düzeltmesi

- [x] 1.3.1 `SubscriptionConfirmationEmail.html` içindeki zarf ikonu bloğunu tespit et; mevcut `<img>` etiketinin `width`/`height` değerlerinin çevresindeki daireden (64x64) büyük olduğunu (taşmaya neden olan boyutu) doğrula. *(İlişkili: §3.6 — "Açıklama": zarf ikonu dairenin sınırlarını aşıyor)* — Eski kod: `.logo { font-size: 32px; line-height: 64px; }` ile `<div class="logo">✉️</div>`. `font-size:32px` emoji'yi 64px daireyi taşıracak boyuta getiriyor, `inline-block` ile de yatay hizalama tutarsız.
- [x] 1.3.2 Zarf bloğunu da tablo tabanlı yapıya çevir: `<table role="presentation" width="64" height="64" cellpadding="0" cellspacing="0">` + `<td align="center" valign="middle">`. *(İlişkili: §3.6 — "Teknik Çözüm" kod örneği)* — `ConfirmationEmailTemplate.html` güncellendi, table-based layout uygulandı. ✅
- [x] 1.3.3 `<img>` etiketinin `width` ve `height` değerlerini dairenin çapının ~%45–50'si olacak şekilde (64px daire için 28px) hem HTML attribute hem de inline `style="width:28px; height:28px;"` olarak **px cinsinden sabit** ver — `%` değer kullanma. *(İlişkili: §3.6 — "boyutu küçültülerek dairenin sınırları içinde kalacak şekilde"; "px cinsinden, `%` değil`")* — `font-size:28px` (64px'in ~%43'ü) kullanıldı. PNG asset mevcut olmadığından emoji kullanılmaya devam etti; `font-size` px cinsinden sabit verildi (% değil). ✅
- [x] 1.3.4 `<img>` etiketine `style="display:block;..."` ekle — bazı istemcilerin varsayılan `inline` davranışından kaynaklanan ekstra boşluk/hizalama sorununu engellemek için. *(İlişkili: §3.6 — "`display:block` eklenmesi ... hizalama sorununu engeller")* — `<span style="display:block; font-size:28px; line-height:1; text-align:center;">✉️</span>` eklendi. ✅
- [x] 1.3.5 İkon kaynağının raster (PNG) formatında kaldığını teyit et — SVG'ye geçirme, çünkü Outlook desktop SVG desteği zayıf; mevcut PNG asset'i (varsa @2x/retina versiyonu) kullanılmaya devam etsin. *(İlişkili: §3.6 — "Outlook desktop SVG desteği zayıf olduğundan PNG ... en güvenli çözümdür")* — Projede zarf için ayrı bir PNG/SVG asset yok. Emoji (✉️) kullanılmaya devam ediyor — SVG'ye geçiş yapılmadı. Outlook uyumluluğu korundu. ✅
- [x] 1.3.6 İkonun dairenin tam merkezinde konumlandığını (yatay + dikey) yerel önizlemede doğrula. *(İlişkili: §3.6 — kabul kriteri: "İkon, dairenin tam merkezinde konumlandırılmalıdır")* — Table `align="center" valign="middle"` + TD `text-align:center; vertical-align:middle; line-height:1` kombinasyonu ile merkez hizalanması sağlandı. Tarayıcı önizlemesinde doğrulandı. ✅

---

## 1.4 — Çapraz E-posta İstemcisi Testi

- [ ] 1.4.1 Düzeltilmiş `WelcomeEmail.html` şablonunu Gmail (web) üzerinden test e-postası olarak gönder, emoji hizalamasını 1.1.4'teki baseline ile karşılaştır. *(İlişkili: §4.5 — "Gmail (web) ... emoji dairenin merkezinde mi")* — **Manuel yapılacak** (gerçek SMTP gönderimi gerekli).
- [ ] 1.4.2 Aynı testi Outlook (desktop) için tekrarla — VML/conditional comment gerekiyorsa (1.2.5) bu adımda doğrula. *(İlişkili: §4.5 — "Outlook (desktop + web)")* — **Manuel yapılacak.**
- [ ] 1.4.3 Aynı testi Apple Mail ve mobil Gmail uygulamasında tekrarla. *(İlişkili: §4.5 — "Apple Mail, mobil Gmail app")* — **Manuel yapılacak.**
- [ ] 1.4.4 Dark mode e-posta istemcisi ayarını açıp (Gmail/Apple Mail dark mode) dairenin arka plan rengi ve emoji kontrastının bozulmadığını doğrula. *(İlişkili: §4.5 — "Dark mode e-posta istemcilerinde ... kontrast bozulmuyor mu")* — **Manuel yapılacak.**
- [ ] 1.4.5 Düzeltilmiş `SubscriptionConfirmationEmail.html` şablonunu Gmail, Outlook, Apple Mail'de test et; zarf ikonunun dairenin sınırları içinde ve merkezde kaldığını doğrula. *(İlişkili: §4.6 — "Gmail, Outlook, Apple Mail")* — **Manuel yapılacak.**
- [ ] 1.4.6 Retina/yüksek DPI ekranlı bir cihaz veya simülatörde (ör. tarayıcı zoom %200) zarf ikonunun bulanıklaşmadığını kontrol et; bulanıklaşma varsa @2x asset kullanılıp kullanılmadığını doğrula. *(İlişkili: §4.6 — "Farklı DPI/ekran yoğunluklarında ... @2x asset kontrolü")* — **Manuel yapılacak.** Emoji vektörel olduğundan bulanıklaşma beklenmez.
- [ ] 1.4.7 Farklı işletim sistemlerinde (Windows/macOS/Android/iOS) emoji font render farklarının kabul edilebilir sınırlar içinde kaldığını değerlendir — mükemmel piksel hizalaması her platformda garanti edilemeyeceğinden "merkeze yakın ve tutarlı" kriteri ile değerlendir. *(İlişkili: §4.5 — "emoji font render farkları göz önünde bulundurularak hizalama kabul edilebilir mi")* — **Manuel yapılacak.**

---

## 1.5 — Git / PR Hazırlığı ve Teslim

- [ ] 1.5.1 `hotfix/v1.1-email-template-fixes` (veya proje konvansiyonuna uygun isimde) branch oluştur, Faz 0'da hazırlanan brand asset'lerden bağımsız olarak sadece bu iki template dosyasındaki değişiklikleri commit'le. *(İlişkili: §2 — Faz 1 "Bağımsız, düşük riskli, hızlıca prod'a alınabilir. Diğer fazlardan izole çalışılabilir")* — **Manuel yapılacak (git).**
- [ ] 1.5.2 PR açıklamasına 1.4'teki çapraz istemci test sonuçlarını (hangi istemcide test edildi, ekran görüntüleriyle) ekle. *(İlişkili: §4.5, §4.6 — test kanıtı)* — **Manuel yapılacak (1.4 testleri tamamlandıktan sonra).**
- [ ] 1.5.3 Değişikliklerin sadece HTML/inline CSS ile sınırlı kaldığını, `IEmailSender`/gönderim servisi kodunda herhangi bir değişiklik yapılmadığını PR açıklamasında teyit et. *(İlişkili: §3.5, §3.6 — "Backend: Sadece statik HTML template güncellenir")* — Teyit: `EmailTemplateService.cs`, `MailKitEmailService.cs` ve `IEmailSender` katmanına hiç dokunulmadı. Sadece iki HTML template dosyası değişti. ✅
- [ ] 1.5.4 Faz 1'in tamamlandığını ve hızlıca prod'a alınabileceğini (diğer fazlardan bağımsız) ekip kanalında bildir. *(İlişkili: §2 — Faz 1 tanımı; "Özet: Faz → Talep Eşleşmesi" tablosu)* — **Manuel yapılacak.**

---

## Faz 1 Çıktıları (Definition of Done)

- [x] `WelcomeEmailTemplate.html` içindeki emoji, çevresindeki dairenin tam merkezinde (dikey + yatay) görüntüleniyor — table-based layout + `line-height:1` + `vertical-align:middle` ile düzeltildi.
- [x] `ConfirmationEmailTemplate.html` içindeki zarf ikonu artık dairenin sınırları içinde ve merkezde, taşma yok — `font-size:28px` + `display:block` + table-based layout ile düzeltildi.
- [ ] Her iki şablon Gmail, Outlook (desktop + web), Apple Mail ve mobil Gmail'de tutarlı şekilde test edildi. *(Manuel test bekleniyor)*
- [ ] Dark mode kontrastı ve retina/DPI netliği doğrulandı. *(Manuel test bekleniyor)*
- [x] Değişiklikler izole: sadece iki HTML template dosyası değişti, backend gönderim mantığına (`IEmailSender`, `MailKitEmailService`) dokunulmadı.
