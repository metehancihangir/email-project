# Email Subscriber — v1.1 Uçtan Uca Teknik Uygulama Planı

**Rol:** Full-Stack Yazılım Mimarisi & Sprint Planlama Dokümanı
**Kaynak:** `v1.1-updates.md`
**Mimari Kısıt:** React (Frontend) + ASP.NET Core Web API (Backend) + MySQL (DB) + Tailwind CSS. Yeni harici bağımlılık / altyapı bileşeni (Redis, üçüncü parti servis vb.) eklenmeyecek; mevcut hafif arka plan kuyruğu ve in-process mekanizmalar (`IMemoryCache`, `BackgroundService`) kullanılacaktır.

> **Not:** `requirements.md` bu oturumda context içinde bulunmadığından, bu doküman v1.1-updates.md'nin "Teknik Uyumluluk Notu" bölümünde beyan edilen mimariye dayanılarak hazırlanmıştır. Servis/repository katman adları örnek isimlendirme olup mevcut kod tabanındaki gerçek adlarla eşleştirilmelidir.

---

## 1. Gereksinim ve Mimari Analizi

| Talep | Frontend Etkisi | Backend Etkisi | DB Etkisi | Arka Plan İşi Etkisi |
|---|---|---|---|---|
| 1. Arka plan görseli | Layout/CSS değişikliği (statik asset) | Yok | Yok | Yok |
| 2. Logo/Favicon | Header/Navbar component + `public/` asset değişikliği | Yok (statik dosya servisi mevcutsa) | Yok | Yok |
| 3. Kod tekrar gönderme + rate limit | Yeni UI state (countdown, disabled link) | **Yeni endpoint + rate-limiting mantığı** | **Yeni kolon(lar)** gerekebilir | Mevcut mail gönderim kuyruğu yeniden kullanılır |
| 4. Admin login geri butonu | Basit navigasyon (React Router) | Yok | Yok | Yok |
| 5. Emoji hizalama (mail) | Yok (mail template = ayrı katman) | Mail template dosyası (HTML/inline CSS) | Yok | Yok |
| 6. Zarf ikonu taşması (mail) | Yok | Mail template dosyası (HTML/inline CSS) | Yok | Yok |

**Genel değerlendirme:**
- Talep 1, 2, 4, 5, 6 düşük risklidir; büyük oranda statik varlık ve şablon/CSS revizyonu gerektirir, mimariyi etkilemez.
- Talep 3, sistemin tek "core logic" gerektiren maddesidir: idempotent kod üretimi, e-posta bazlı throttling ve kullanıcıya doğru HTTP durum kodlarıyla (429) geri bildirim gerektirir. Bu, mevcut mimariyle (dış bağımlılık eklemeden) `IMemoryCache` tabanlı bir throttle katmanı ya da doğrudan MySQL üzerinde `LastRequestedAt` kolonu ile çözülebilir. Çoklu instance (yatay ölçekleme) senaryosu varsa `IMemoryCache` süreç-içi olduğundan tutarsızlık riski taşır; bu durumda DB tabanlı yaklaşım (bkz. Faz 2) tercih edilmelidir.
- Mail template düzeltmeleri (5, 6) e-posta istemcileri arası tutarlılık gerektirdiğinden inline CSS + tablo tabanlı (table-based) layout kullanılmalı; flexbox/grid güvenilir değildir (Outlook desteği zayıf).

---

## 2. Fazlandırılmış Modül Planı (Roadmap)

### **Faz 0 — Hazırlık ve Asset Tedariki** (0.5 gün)
- Logo/favicon tasarım varlıklarının (SVG/PNG, çoklu boyut) tedariki.
- Arka plan görselinin (SVG/WebP, optimize edilmiş) tedariki.
- Tasarım tokenlarının (renk, spacing) mevcut Tailwind config ile teyidi.

### **Faz 1 — Hotfix: E-posta Şablon Düzeltmeleri (Bug)** (0.5–1 gün)
- Talep 5 (emoji hizalama) ve Talep 6 (zarf ikonu taşması).
- Bağımsız, düşük riskli, hızlıca prod'a alınabilir. Diğer fazlardan izole çalışılabilir.

### **Faz 2 — Core Backend: Onay Kodu Tekrar Gönderme & Rate-Limiting** (2–3 gün)
- Talep 3. Sistemin en kritik ve riskli parçası; önce backend mantığı ve DB şeması sağlamlaştırılır, sonra frontend entegre edilir.

### **Faz 3 — UI Revizyonları: Marka Kimliği ve Navigasyon** (1.5–2 gün)
- Talep 1 (arka plan), Talep 2 (logo/favicon), Talep 4 (admin geri butonu), Ek Talep 3.2.1 ("EmailSubscriber" → "SUBMAIL" adlandırması).
- Görsel/UX odaklı, backend'e bağımlılığı yok; Faz 2 ile paralel yürütülebilir.
- Tasarım varlıkları (logo, wordmark, arka plan deseni) onaylanmış ve teslim edilmiştir: bkz. `submail-brand-assets.zip`.

### **Faz 4 — Entegrasyon, QA ve Regresyon Testleri** (1 gün)
- Tüm fazların birleşik test edilmesi, e-posta istemcisi testleri, cross-browser/responsive kontrol, rate-limit uç durum testleri.

### **Faz 5 — Yayınlama (Release)** (0.5 gün)
- Migration'ların prod'a uygulanması, feature flag (varsa) kaldırılması, sürüm notu yayınlanması.

**Toplam tahmini süre:** ~6–8 iş günü (tek geliştirici); paralel çalışma ile 4–5 iş gününe indirilebilir.

---

## 3. Detaylı Teknik Uygulama

---

### 3.1 — Talep 1: Marka Kimliği - Arka Plan Görselleri

**Seçilen Tasarım:** Nokta ızgarası (dot-grid) deseni — `#10B981` renginde, %30 opaklıkta, 24x24px tekrarlanabilir (tileable) SVG karo. Mevcut minimal tasarım dilini bozmayan, en düşük riskli ve en performanslı seçenek olduğu için tercih edildi (bkz. `submail-brand-assets/background-pattern-tile.svg`).

**Frontend (React & Tailwind):**
- Etkilenen component'ler: `LoginPage.jsx` (veya `WelcomeScreen.jsx`) ve `AdminLayout.jsx` / `AdminPanelLayout.jsx`.
- Asset, `public/assets/background-pattern-tile.svg` altına kopyalanır (tek bir küçük SVG karo — WebP/PNG'ye gerek yok, SVG tekrarlanan desenler için hem daha küçük dosya boyutu hem de çözünürlükten bağımsız netlik sağlar).
- Tailwind'de custom bir utility tanımlanır:
  ```js
  // tailwind.config.js
  backgroundImage: {
    'auth-pattern': "url('/assets/background-pattern-tile.svg')",
  }
  ```
- Kullanım: `<div className="bg-auth-pattern bg-repeat min-h-screen">` — **`bg-cover` değil `bg-repeat`** kullanılmalı, çünkü desen küçük bir karo olarak tasarlandı ve tekrarlanarak tüm alanı kaplayacak.
- Kontrast garantisi için form kartının arka planı `bg-white` (opak) olarak korunur; desenin opaklığı (%30) zaten form kartının üzerine değil, sadece boş alanlara düştüğünden ekstra overlay gerekmez.
- State yönetimi gerekmiyor (statik stil).
- Responsive: SVG karo çözünürlükten bağımsız olduğundan tüm breakpoint'lerde bozulmadan tekrarlanır; ek bir mobil varyantına gerek yoktur.

**Backend (.NET Core API):** Değişiklik yok. Görsel statik dosya olarak frontend build içinde servis edilir (CDN/static hosting).

**Veritabanı (MySQL):** Değişiklik yok.

**Teslim Edilen Dosyalar:** `background-pattern-tile.svg`, `background-pattern-tile.png` (bkz. `submail-brand-assets.zip`).

---

### 3.2 — Talep 2: Marka Kimliği - Logo

**Seçilen Tasarım:**
- **İkon:** "S" monogramı — `#10B981` renginde yuvarlatılmış kare (rx=24) zemin üzerinde beyaz, kalın "S" harfi + sağ üst köşede küçük bir "bildirim noktası" (abonelik/mail teması). Küçük boyutlarda (favicon 16px) en net okunan seçenek olduğu için tercih edildi.
- **Wordmark (header/navbar):** İkon (34x34, rounded) + yanında "**Sub**" (koyu, `#111827`) + "**mail**" (yeşil, `#10B981`) yazısı, tek satır.
- Kaynak dosyalar: `logo-icon.svg`, `wordmark.svg` (bkz. `submail-brand-assets.zip`).

**Frontend (React & Tailwind):**
- Yeni `Logo.jsx` ortak component'i oluşturulur (`<Logo variant="full" | "icon" className="..." />`), hem `Header.jsx`/`Navbar.jsx` hem de `AdminNavbar.jsx` içinde reuse edilir; `variant="full"` → `wordmark.svg`, `variant="icon"` → `logo-icon.svg` render eder.
  ```jsx
  import wordmarkSvg from '@/assets/wordmark.svg';
  import iconSvg from '@/assets/logo-icon.svg';

  export default function Logo({ variant = 'full', className }) {
    const src = variant === 'icon' ? iconSvg : wordmarkSvg;
    return <img src={src} alt="Submail" className={className} />;
  }
  ```
- Mevcut header'daki **"EmailSubscriber" metni `<Logo variant="full" />` component'i ile değiştirilir** (bkz. ekran görüntüsündeki sol üst köşe — şu an yeşil "E" ikonu + "EmailSubscriber" yazısı; bu, yeni wordmark ile birebir değiştirilecek).
- Favicon: `public/favicon.ico` (16x16 + 32x32 multi-size ICO) + `public/favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png`; `index.html` `<head>` içine ilgili `<link rel="icon">` etiketleri eklenir; `<title>` etiketi de **"SUBMAIL"** olarak güncellenir.
- Header'da responsive davranış: `h-8 md:h-10` gibi Tailwind sınıflarıyla mobil/masaüstü ölçeklenme sağlanır.

**Backend (.NET Core API):** Değişiklik yok.

**Veritabanı (MySQL):** Değişiklik yok.

**Teslim Edilen Dosyalar:** `logo-icon.svg`, `logo-icon-{512,192,32,16}.png`, `favicon.ico`, `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png`, `wordmark.svg`, `wordmark.png` (bkz. `submail-brand-assets.zip`, kullanım talimatları paket içindeki `README.md`'de).

---

### 3.2.1 — Ek Talep: "EmailSubscriber" → "SUBMAIL" Marka Adı Değişikliği

**Kategori:** Yeni Talep (v1.1 sürecinde eklendi) | **Öncelik:** Orta | **İlişkili Madde:** 2 (Logo) ile birlikte uygulanmalı

**Açıklama:** Uygulama genelinde görünen "EmailSubscriber" marka adı "SUBMAIL" olarak değiştirilecek. Bu, salt bir metin değişikliği değil, yukarıdaki logo/wordmark güncellemesiyle birlikte tek bir işte ele alınmalıdır.

**Etkilenen Yerler (Frontend):**
- Header/Navbar component'i (ekran görüntüsündeki sol üst köşe) → `Logo.jsx` (`variant="full"`) ile değiştirilir.
- `index.html` `<title>` etiketi → `SUBMAIL`.
- `public/manifest.json` (varsa) içindeki `name` / `short_name` alanları.
- Admin panel header'ı (`AdminNavbar.jsx`).

**Etkilenen Yerler (Backend / İçerik):**
- E-posta template'lerindeki gönderen adı / footer imzası ("EmailSubscriber ekibi" gibi ifadeler varsa) → `SUBMAIL ekibi` olarak güncellenmeli (`Templates/Emails/*.html` dosyaları taranmalı).
- API response'larında veya loglarda sabit metin olarak geçen "EmailSubscriber" referansları varsa güncellenmeli (kod tabanında `grep -ri "emailsubscriber"` ile taranması önerilir).

**Kabul Kriterleri:**
- Uygulama genelinde (login, admin panel, e-postalar, tarayıcı sekmesi başlığı) "EmailSubscriber" metni hiçbir yerde kalmamalı.
- Yeni wordmark, header'da eski logonun yerini birebir alacak şekilde (aynı konum, benzer boyut) yerleştirilmeli.

---

### 3.3 — Talep 3: Onay Kodu Ekranı - Tekrar Gönderme Mekanizması ⚠️ (Kritik)

Bu, en fazla mimari karar gerektiren maddedir. İki mimari seçenek değerlendirilir:

**Seçenek A — `IMemoryCache` tabanlı (tek instance / monolitik deploy için uygun):**
- Basit, ek bağımlılık gerektirmez, ancak yatay ölçeklenmiş (birden fazla API instance) ortamda instance'lar arasında cache paylaşılmadığından tutarsızlık riski taşır.

**Seçenek B — MySQL tabanlı `LastCodeRequestedAt` kolonu (önerilen, kalıcı ve ölçeklenebilir):**
- Ek bağımlılık gerektirmez (mevcut MySQL kullanılır), instance sayısından bağımsız tutarlı çalışır, mevcut mimariye tam uyumludur. **Bu doküman Seçenek B'yi önerir.**

#### Frontend (React & Tailwind):
- Etkilenen component: `VerificationCodeScreen.jsx` (veya mevcut adıyla `ConfirmCodePage.jsx`).
- Yeni state:
  ```js
  const [resendState, setResendState] = useState({
    isDisabled: false,
    remainingSeconds: 0,
  });
  ```
- `"Kod gelmedi mi? Tekrar deneyin"` linkine tıklanınca:
  1. `POST /api/auth/resend-code` çağrılır (email, session/token context ile).
  2. Başarılı yanıt (`200 OK`) alınırsa `isDisabled: true`, `remainingSeconds: 120` set edilir ve `useEffect` içinde `setInterval` ile her saniye azaltılır; 0'a ulaşınca `isDisabled: false`.
  3. Backend `429 Too Many Requests` dönerse (kullanıcı süre dolmadan client tarafını bypass etmeye çalışırsa — ör. sayfa yenileme), response body'deki `retryAfterSeconds` değeri okunup UI aynı şekilde senkronize edilir. **Bu, client-side timer'ın tek güvenlik katmanı olmamasını, backend'in nihai otorite olmasını sağlar.**
- UI metni: buton pasifken `"Yeni kod talep etmek için lütfen {mm:ss} bekleyin."` gösterilir; buton `disabled` ve `aria-disabled="true"` olarak işaretlenir (erişilebilirlik).
- Sayfa yenilendiğinde/yeniden mount olduğunda bekleme süresinin sıfırlanmaması için, component mount olurken backend'e `GET /api/auth/resend-code/status?email=...` (veya login response'unda dönen `nextAllowedAt` timestamp) ile senkronize olunmalı; **yalnızca client-side state'e güvenilmemeli.**

#### Backend (.NET Core API):

**Yeni/Güncellenen Endpoint:**
```
POST /api/auth/resend-code
Body: { "email": "user@example.com" }
Response 200: { "message": "Yeni kod gönderildi.", "nextAllowedAt": "2026-08-13T10:32:00Z" }
Response 429: { "message": "Lütfen 2 dakika bekleyin.", "retryAfterSeconds": 87 }
```

**Servis Katmanı (`VerificationService` veya eşdeğeri):**
```csharp
public async Task<ResendCodeResult> ResendVerificationCodeAsync(string email)
{
    var pendingVerification = await _repository.GetPendingVerificationByEmailAsync(email);

    if (pendingVerification is null)
        return ResendCodeResult.NotFound();

    var now = DateTime.UtcNow;
    var cooldownWindow = TimeSpan.FromMinutes(2);

    if (pendingVerification.LastCodeRequestedAt.HasValue &&
        now - pendingVerification.LastCodeRequestedAt.Value < cooldownWindow)
    {
        var retryAfter = cooldownWindow - (now - pendingVerification.LastCodeRequestedAt.Value);
        return ResendCodeResult.RateLimited(retryAfter);
    }

    var newCode = _codeGenerator.GenerateCode(); // örn. 6 haneli
    pendingVerification.CodeHash = _hasher.Hash(newCode); // ham kod DB'de tutulmamalı
    pendingVerification.LastCodeRequestedAt = now;
    pendingVerification.ExpiresAt = now.AddMinutes(15); // mevcut kod TTL politikasıyla uyumlu

    await _repository.UpdateAsync(pendingVerification);

    await _emailQueue.EnqueueAsync(new SendVerificationCodeEmailJob(email, newCode)); // mevcut hafif kuyruk

    return ResendCodeResult.Success(now.Add(cooldownWindow));
}
```

**Rate-Limiting Notu:**
- DB-tabanlı kontrol (`LastCodeRequestedAt`) hem race condition'a karşı hem çoklu instance senaryosuna karşı güvenlidir; ancak eşzamanlı çift tıklama (double-submit) riskine karşı `UPDATE ... WHERE LastCodeRequestedAt < @cooldownThreshold` şeklinde **atomic conditional update** (optimistic concurrency) kullanılması önerilir — okuma ve yazma arasında race olmaması için tek SQL statement'ta kontrol + güncelleme yapılmalı.
- Alternatif olarak IP bazlı ek bir rate-limit katmanı (`AspNetCoreRateLimit` paketi gibi harici bir NuGet paketi) **bu fazda önerilmez** çünkü "ek bağımlılık eklenmemeli" kısıtına aykırıdır; e-posta bazlı DB kontrolü yeterli kabul edilmiştir. İleride brute-force/spam koruması için ayrı bir güvenlik fazı önerilebilir (bu dokümanın kapsamı dışında, not düşülmüştür).
- 429 yanıtında `Retry-After` HTTP header'ı da standardına uygun olarak set edilmelidir.

#### Veritabanı (MySQL):

`EmailVerifications` (veya mevcut karşılığı) tablosuna yeni kolon:
```sql
ALTER TABLE EmailVerifications
  ADD COLUMN LastCodeRequestedAt DATETIME NULL DEFAULT NULL AFTER CreatedAt;

-- Sorgu performansı için (email + zaman bazlı kontrol sık yapılacağından)
CREATE INDEX idx_emailverifications_email_lastrequested
  ON EmailVerifications (Email, LastCodeRequestedAt);
```
- Migration, EF Core Migrations üzerinden (`dotnet ef migrations add AddLastCodeRequestedAtToEmailVerifications`) oluşturulmalı ve mevcut migration zincirine eklenmelidir.
- Var olan kayıtlar için `LastCodeRequestedAt` NULL kalabilir (ilk talepte cooldown uygulanmaz).

---

### 3.4 — Talep 4: Yönetici Girişi - Geri Butonu

**Frontend (React & Tailwind):**
- Etkilenen component: `AdminLoginPage.jsx`.
- React Router kullanılıyorsa: `const navigate = useNavigate();` → `<button onClick={() => navigate('/login')} className="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700">← Kullanıcı Girişine Dön</button>`.
- Buton, sayfanın sol üst köşesinde, mevcut kart/form component'inin dışında (header alanında) konumlandırılır; `lucide-react` gibi zaten kullanılıyorsa `ArrowLeft` ikonu ile desteklenebilir (yeni bağımlılık gerektirmez, proje zaten React tabanlı olduğundan basit bir SVG ok ikonu da yeterlidir).
- State yönetimi gerekmez; salt navigasyon.

**Backend (.NET Core API):** Değişiklik yok (route değişikliği yalnızca client-side).

**Veritabanı (MySQL):** Değişiklik yok.

---

### 3.5 — Talep 5: Hoş Geldin E-postası - Emoji Hizalama Hatası

**Değişiklik Yeri:** Mail template dosyası (ör. `Templates/Emails/WelcomeEmail.html`, Razor/HTML tabanlı, backend içinde servis edilen statik template).

**Teknik Çözüm:**
- E-posta istemcileri arası tutarlılık için flexbox yerine **tablo tabanlı ortalama** kullanılmalı:
```html
<table role="presentation" width="64" height="64" cellpadding="0" cellspacing="0"
       style="background-color:#22c55e; border-radius:50%;">
  <tr>
    <td align="center" valign="middle" style="text-align:center; vertical-align:middle; font-size:28px; line-height:1;">
      🎉
    </td>
  </tr>
</table>
```
- `line-height:1` ve `vertical-align:middle` kombinasyonu, emoji'nin font metriklerinden kaynaklanan dikey kaymayı engeller (bazı emoji glyph'leri baseline'dan farklı hizalanır — bu nedenle `line-height` küçük ve sabit tutulmalıdır).
- Outlook (Windows, VML tabanlı render motoru) için gerekirse `mso` conditional comment ile ek padding telafisi yapılabilir.

**Backend (.NET Core API):** Sadece statik HTML template güncellenir; e-posta gönderim servisinde (`IEmailSender`/`SendGridClient` vb.) kod değişikliği gerekmez.

**Veritabanı (MySQL):** Değişiklik yok.

---

### 3.6 — Talep 6: Abonelik Onay E-postası - Zarf İkonu Taşma Hatası

**Değişiklik Yeri:** `Templates/Emails/SubscriptionConfirmationEmail.html`.

**Teknik Çözüm:**
- İkon boyutu, çevresindeki dairenin (`width`/`height`) oranına göre yeniden ölçeklenir (öneri: daire çapının ~%45–50'si):
```html
<table role="presentation" width="64" height="64" cellpadding="0" cellspacing="0"
       style="background-color:#22c55e; border-radius:50%;">
  <tr>
    <td align="center" valign="middle">
      <img src="https://.../envelope-icon.png" width="28" height="28"
           style="display:block; width:28px; height:28px;" alt="Zarf" />
    </td>
  </tr>
</table>
```
- İkonun raster (PNG) yerine mümkünse **inline SVG** kullanılması taşmayı garanti şekilde engeller, ancak Outlook desktop SVG desteği zayıf olduğundan PNG + sabit `width`/`height` (px cinsinden, `%` değil) en güvenli çözümdür.
- `display:block` eklenmesi, `<img>` etiketlerinin bazı istemcilerde varsayılan `inline` davranışından kaynaklanan ekstra boşluk/hizalama sorununu engeller.

**Backend (.NET Core API):** Sadece template güncellenir.

**Veritabanı (MySQL):** Değişiklik yok.

---

## 4. Kabul Kriterleri ve Test Adımları

### 4.1 — Talep 1: Arka Plan Görseli
- [ ] Giriş ve admin panel ekranlarında görsel doğru yükleniyor mu? (Network tab'da 200 OK, doğru MIME type)
- [ ] 320px (mobil) → 1920px+ (masaüstü) arası breakpoint'lerde taşma/bozulma yok mu? (Chrome DevTools responsive test)
- [ ] Form elemanları ve metin, WCAG AA kontrast oranını (4.5:1) sağlıyor mu? (Lighthouse Accessibility audit)
- [ ] Görsel yüklenme süresi Lighthouse Performance skorunu düşürmüyor mu? (WebP + lazy-load kontrolü)

### 4.2 — Talep 2: Logo/Favicon + "SUBMAIL" Adlandırması
- [ ] Tarayıcı sekmesinde favicon net görünüyor mu? (Chrome, Firefox, Safari sekme testi)
- [ ] `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png` doğru boyutlarda ve bulanıklaşmadan render ediliyor mu?
- [ ] Header/Navbar logosu mobilde (≤375px) ve masaüstünde taşmadan/kırpılmadan görünüyor mu?
- [ ] Admin panel navbar'ında da aynı logo tutarlı şekilde görünüyor mu?
- [ ] Tarayıcı sekmesi başlığı (`<title>`) "SUBMAIL" olarak görünüyor mu?
- [ ] Uygulama genelinde ("EmailSubscriber" için) `grep -ri "emailsubscriber"` taraması temiz sonuç veriyor mu (kod, template, config)?
- [ ] E-posta template'lerindeki gönderen adı/footer imzası güncellenmiş mi?

### 4.3 — Talep 3: Onay Kodu Tekrar Gönderme (Kritik — En Detaylı Test Seti)
- [ ] **Happy path:** "Tekrar deneyin" tıklanınca yeni kod e-posta ile ulaşıyor mu ve eski kod geçersiz kılınıyor mu (ya da her ikisi de mi geçerli — iş kuralına göre teyit edilmeli)?
- [ ] Tıklama sonrası buton anında disable oluyor mu ve geri sayım (`119`, `118`, ... `0`) doğru çalışıyor mu?
- [ ] 120 saniye dolduğunda buton otomatik olarak tekrar aktif oluyor mu?
- [ ] **Race condition testi:** Aynı email için 2 farklı tab'dan aynı anda "Tekrar deneyin" tetiklenirse, backend yalnızca birini kabul edip diğerine `429` mü dönüyor? (Atomic conditional update doğrulaması)
- [ ] **Bypass testi:** Client-side timer'ı manuel olarak sıfırlayıp (dev tools ile) backend'e doğrudan istek atıldığında, backend `429 Too Many Requests` + `retryAfterSeconds` doğru dönüyor mu?
- [ ] Sayfa yenilendiğinde (F5) bekleme süresi state'i kayboluyor mu yoksa backend'den senkronize ediliyor mu? (Kritik UX kontrolü)
- [ ] `429` response'unda `Retry-After` header'ı doğru set edilmiş mi?
- [ ] Var olmayan/geçersiz email ile istek atıldığında sistem anlamlı bir hata (`404`/generic mesaj — email enumeration'a karşı dikkatli ol) dönüyor mu?
- [ ] Yük testi: Aynı endpoint'e kısa sürede çok sayıda farklı email ile istek atıldığında sistem performansı (DB index kullanımı) kabul edilebilir mi?

### 4.4 — Talep 4: Admin Login Geri Butonu
- [ ] Buton admin login ekranında görünür ve tıklanabilir mi?
- [ ] Tıklandığında `/login` (kullanıcı giriş) rotasına doğru yönlendirme yapılıyor mu?
- [ ] Browser'ın kendi geri butonu ile tutarsızlık/loop oluşmuyor mu? (React Router history testi)
- [ ] Buton, mevcut tasarım diliyle (renk, spacing, font) tutarlı mı? (Görsel QA)

### 4.5 — Talep 5: Emoji Hizalama
- [ ] Gmail (web), Outlook (desktop + web), Apple Mail, mobil Gmail app'te emoji dairenin merkezinde mi görünüyor?
- [ ] Farklı işletim sistemlerinde (Windows/macOS/Android/iOS) emoji font render farkları göz önünde bulundurularak hizalama kabul edilebilir mi?
- [ ] Dark mode e-posta istemcilerinde daire arka planı ve emoji kontrastı bozulmuyor mu?

### 4.6 — Talep 6: Zarf İkonu Taşma
- [ ] İkon, dairenin sınırları içinde, taşma olmadan görünüyor mu? (Gmail, Outlook, Apple Mail)
- [ ] İkon, dairenin tam merkezinde mi (yatay + dikey)?
- [ ] Farklı DPI/ekran yoğunluklarında (retina vb.) ikon bulanıklaşmıyor mu? (@2x asset kontrolü)

### 4.7 — Genel Regresyon (Faz 4)
- [ ] Mevcut v1.0 fonksiyonlarında (login, subscribe, admin panel erişimi) regresyon var mı?
- [ ] Migration'lar (`LastCodeRequestedAt`) staging ortamında sorunsuz uygulanabiliyor mu, rollback senaryosu test edildi mi?
- [ ] Tüm yeni UI bileşenleri klavye navigasyonu ve screen reader ile erişilebilir mi (a11y)?

---

## Özet: Faz → Talep Eşleşmesi

| Faz | İçerdiği Talepler | Bağımlılık |
|---|---|---|
| Faz 0 | Asset hazırlığı | — |
| Faz 1 | 5, 6 | Faz 0 |
| Faz 2 | 3 | Faz 0 (paralel) |
| Faz 3 | 1, 2, 4, Ek 3.2.1 (yeniden adlandırma) | Faz 0 |
| Faz 4 | Tümü (entegrasyon/QA) | Faz 1, 2, 3 |
| Faz 5 | Release | Faz 4 |
