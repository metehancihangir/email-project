# Faz 3 — UI Revizyonları: Marka Kimliği ve Navigasyon — Teknik Görev Listesi

**Kaynak Doküman:** `update-phases.md` — Faz tanımı §2 "Faz 3 — UI Revizyonları: Marka Kimliği ve Navigasyon", detaylar §3.1 (Talep 1), §3.2 (Talep 2), §3.2.1 (Ek Talep: SUBMAIL adlandırması), §3.4 (Talep 4), kabul kriterleri §4.1, §4.2, §4.4.
**Tahmini Süre:** 1.5–2 gün
**Bağımlılık:** Faz 0 (brand asset'ler repoya eklenmiş, envanterler çıkarılmış olmalı)
**Bu Fazın Kapsamı:** Talep 1 (arka plan), Talep 2 (logo/favicon), Talep 4 (admin geri butonu) ve Ek Talep 3.2.1 ("EmailSubscriber" → "SUBMAIL" adlandırması). Backend/DB değişikliği yok — tamamen frontend + statik asset entegrasyonu. Faz 2 ile paralel yürütülebilir. *(İlişkili: update-phases.md §2 — "Görsel/UX odaklı, backend'e bağımlılığı yok; Faz 2 ile paralel yürütülebilir")*

---

## 3.1 — Talep 1: Arka Plan Görseli Entegrasyonu

- [x] 3.1.1 Faz 0'da `public/assets/background-pattern-tile.svg` altına yerleştirilmiş dosyanın hâlâ doğru konumda olduğunu doğrula. *(İlişkili: §3.1 — "Asset, `public/assets/background-pattern-tile.svg` altına kopyalanır")*
- [x] 3.1.2 `tailwind.config.js` içine `backgroundImage: { 'auth-pattern': "url('/assets/background-pattern-tile.svg')" }` utility tanımını ekle. *(İlişkili: §3.1 — "Tailwind'de custom bir utility tanımlanır" kod örneği)*
- [x] 3.1.3 `LoginPage.jsx` (veya `WelcomeScreen.jsx`) kök container'ına `bg-auth-pattern bg-repeat min-h-screen` class'larını uygula — **`bg-cover` değil `bg-repeat`** kullanıldığından emin ol. *(İlişkili: §3.1 — "Kullanım" ve "`bg-cover` değil `bg-repeat` kullanılmalı")*
- [x] 3.1.4 `AdminLayout.jsx` / `AdminPanelLayout.jsx` container'ına aynı `bg-auth-pattern bg-repeat` class'larını uygula. *(İlişkili: §3.1 — "Kapsam: Kullanıcı karşılama / giriş ekranı, Admin paneli" — orijinal talep v1.1-updates.md madde 1)*
- [x] 3.1.5 Form kartının arka planının opak (`bg-white`) kaldığını ve desenin sadece boş alanlarda göründüğünü doğrula — ek bir overlay/kontrast katmanı **eklenmediğini** teyit et (bu tasarımda gerekli değil). *(İlişkili: §3.1 — "desenin opaklığı zaten form kartının üzerine değil, sadece boş alanlara düştüğünden ekstra overlay gerekmez")*
- [x] 3.1.6 320px'den 1920px+'a kadar farklı breakpoint'lerde (Chrome DevTools responsive mod) deseni görsel olarak kontrol et; SVG karo çözünürlükten bağımsız olduğu için ek mobil varyant **eklenmediğini** doğrula. *(İlişkili: §3.1 — "Responsive: SVG karo ... tüm breakpoint'lerde bozulmadan tekrarlanır; ek bir mobil varyantına gerek yoktur")*

---

## 3.2 — Talep 2: Logo ve Favicon Entegrasyonu

- [x] 3.2.1 `src/assets/` altındaki `logo-icon.svg` ve `wordmark.svg` dosyalarının Faz 0'dan itibaren doğru konumda olduğunu doğrula. *(İlişkili: §3.2 — "Frontend" import örneği)*
- [x] 3.2.2 Ortak `Logo.jsx` component'ini oluştur; `variant="full" | "icon"` prop'una göre `wordmark.svg` veya `logo-icon.svg` render etsin. *(İlişkili: §3.2 — `Logo.jsx` kod örneği)*
  ```jsx
  import wordmarkSvg from '@/assets/wordmark.svg';
  import iconSvg from '@/assets/logo-icon.svg';

  export default function Logo({ variant = 'full', className }) {
    const src = variant === 'icon' ? iconSvg : wordmarkSvg;
    return <img src={src} alt="Submail" className={className} />;
  }
  ```
- [x] 3.2.3 `Header.jsx`/`Navbar.jsx` içindeki mevcut yeşil "E" ikonu + "EmailSubscriber" metnini `<Logo variant="full" />` component'i ile değiştir. *(İlişkili: §3.2 — "Mevcut header'daki 'EmailSubscriber' metni `<Logo variant='full' />` component'i ile değiştirilir")*
- [x] 3.2.4 `AdminNavbar.jsx` içinde de aynı `<Logo variant="full" />` (veya alan darsa `variant="icon"`) component'ini kullan — iki header arasında tutarlılık sağla. *(İlişkili: §3.2 — "hem `Header.jsx`/`Navbar.jsx` hem de `AdminNavbar.jsx` içinde reuse edilir")*
- [x] 3.2.5 Logo'ya `h-8 md:h-10` (veya projenin mevcut navbar yüksekliğiyle Faz 0'da teyit edilen değer) Tailwind class'larını uygula — mobil/masaüstü responsive ölçeklenmeyi sağla. *(İlişkili: §3.2 — "Header'da responsive davranış: `h-8 md:h-10`")*
- [x] 3.2.6 `favicon.ico`, `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png` dosyalarının `public/` kök dizininde olduğunu doğrula (Faz 0'da kopyalanmıştı). *(İlişkili: §3.2 — "Favicon" dosya listesi)*
- [x] 3.2.7 `index.html` `<head>` içine favicon `<link>` etiketlerini ekle:
  ```html
  <link rel="icon" type="image/x-icon" href="/favicon.ico" />
  <link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png" />
  <link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png" />
  <link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png" />
  ```
  *(İlişkili: §3.2 — "`index.html` `<head>` içine ilgili `<link rel='icon'>` etiketleri eklenir")*

---

## 3.3 — Ek Talep 3.2.1: "EmailSubscriber" → "SUBMAIL" Adlandırması

- [x] 3.3.1 Faz 0'da çıkarılan `grep -ri "emailsubscriber"` envanter listesini (bkz. Faz 0 görev 0.5.1) aç ve listedeki her dosyayı tek tek işaretleyerek geç. *(İlişkili: §3.2.1 — "Kod tabanında `grep -ri 'emailsubscriber'` ile taranması önerilir"; Faz 0 §0.5)*
- [x] 3.3.2 `index.html` `<title>` etiketini `SUBMAIL` olarak güncelle. *(İlişkili: §3.2 — "`<title>` etiketi de 'SUBMAIL' olarak güncellenir"; §3.2.1 — "Etkilenen Yerler (Frontend)")*
- [x] 3.3.3 `public/manifest.json` (varsa) içindeki `name` / `short_name` alanlarını `SUBMAIL` olarak güncelle. *(İlişkili: §3.2.1 — "Etkilenen Yerler (Frontend)")*
- [x] 3.3.4 `Templates/Emails/*.html` dosyalarını tara, "EmailSubscriber ekibi" gibi gönderen adı/footer imzası ifadelerini `SUBMAIL ekibi` olarak güncelle. *(İlişkili: §3.2.1 — "Etkilenen Yerler (Backend / İçerik)")*
- [x] 3.3.5 API response'larında veya loglarda sabit metin olarak geçen "EmailSubscriber" referanslarını (varsa) güncelle. *(İlişkili: §3.2.1 — "API response'larında veya loglarda sabit metin olarak geçen 'EmailSubscriber' referansları varsa güncellenmeli")*
- [x] 3.3.6 Tüm değişikliklerden sonra kod tabanında `grep -ri "emailsubscriber"` komutunu tekrar çalıştır ve **sıfır sonuç** döndürdüğünü doğrula. *(İlişkili: §4.2 — "Uygulama genelinde ... `grep -ri 'emailsubscriber'` taraması temiz sonuç veriyor mu")*
- [x] 3.3.7 Yeni wordmark'ın header'da eski logonun yerini birebir aldığını (aynı konum, benzer boyut) görsel olarak doğrula. *(İlişkili: §3.2.1 — Kabul Kriterleri: "Yeni wordmark, header'da eski logonun yerini birebir alacak şekilde ... yerleştirilmeli")*

---

## 3.4 — Talep 4: Admin Login Geri Butonu

- [x] 3.4.1 `AdminLoginPage.jsx` component'inde React Router'ın `useNavigate` hook'unu import et. *(İlişkili: §3.4 — "React Router kullanılıyorsa: `const navigate = useNavigate();`")*
- [x] 3.4.2 Sayfanın sol üst köşesine, mevcut kart/form component'inin dışında (header alanında) `← Kullanıcı Girişine Dön` butonunu ekle: `<button onClick={() => navigate('/login')} className="flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700">`. *(İlişkili: §3.4 — buton kod örneği ve konumlandırma notu)*
- [x] 3.4.3 Buton için basit bir ok/geri ikonu ekle (proje zaten `lucide-react` kullanıyorsa `ArrowLeft`, kullanmıyorsa küçük bir inline SVG ok ikonu) — yeni bir bağımlılık **eklenmediğini** doğrula. *(İlişkili: §3.4 — "yeni bağımlılık gerektirmez, proje zaten React tabanlı olduğundan basit bir SVG ok ikonu da yeterlidir")*
- [x] 3.4.4 Butonun mevcut tasarım diliyle (renk, spacing, font) tutarlı göründüğünü görsel olarak doğrula — ek state yönetimi gerekmediğinden (salt navigasyon) başka bir değişiklik yapılmadığını teyit et. *(İlişkili: §3.4 — "State yönetimi gerekmez; salt navigasyon")*

---

## 3.5 — Çapraz Tarayıcı / Cihaz QA

- [x] 3.5.1 Arka plan görselinin Network sekmesinde `200 OK` ile ve doğru MIME type (`image/svg+xml`) ile yüklendiğini doğrula. *(İlişkili: §4.1 — "Network tab'da 200 OK, doğru MIME type")*
- [x] 3.5.2 Lighthouse Accessibility audit çalıştır; form elemanları ve metnin WCAG AA kontrast oranını (4.5:1) sağladığını doğrula. *(İlişkili: §4.1 — "WCAG AA kontrast oranını (4.5:1) sağlıyor mu")*
- [x] 3.5.3 Lighthouse Performance skorunu kontrol et; SVG karo asset'inin skor düşüşüne neden olmadığını doğrula. *(İlişkili: §4.1 — "Görsel yüklenme süresi Lighthouse Performance skorunu düşürmüyor mu")*
- [x] 3.5.4 Chrome, Firefox, Safari'de tarayıcı sekmesinde favicon'un net göründüğünü doğrula. *(İlişkili: §4.2 — "Tarayıcı sekmesinde favicon net görünüyor mu? Chrome, Firefox, Safari sekme testi")*
- [x] 3.5.5 `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png` dosyalarının bulanıklaşmadan, doğru boyutlarda render edildiğini doğrula. *(İlişkili: §4.2)*
- [x] 3.5.6 Header/Navbar logosunun mobilde (≤375px) ve masaüstünde taşmadan/kırpılmadan göründüğünü doğrula. *(İlişkili: §4.2 — "Header/Navbar logosu mobilde ... taşmadan/kırpılmadan görünüyor mu")*
- [x] 3.5.7 Admin panel navbar'ında logonun tutarlı şekilde göründüğünü doğrula. *(İlişkili: §4.2 — "Admin panel navbar'ında da aynı logo tutarlı şekilde görünüyor mu")*
- [x] 3.5.8 Tarayıcı sekmesi başlığının ("SUBMAIL") tüm sayfalarda (login, admin login, admin panel) doğru göründüğünü doğrula. *(İlişkili: §4.2 — "Tarayıcı sekmesi başlığı ('<title>') 'SUBMAIL' olarak görünüyor mu")*
- [x] 3.5.9 Admin login ekranında geri butonunun görünür ve tıklanabilir olduğunu, tıklandığında `/login` rotasına doğru yönlendirdiğini doğrula. *(İlişkili: §4.4 — "Buton admin login ekranında görünür ve tıklanabilir mi? / Tıklandığında `/login` rotasına doğru yönlendirme yapılıyor mu")*
- [x] 3.5.10 Tarayıcının kendi geri butonuyla (browser back) yeni geri butonu arasında bir tutarsızlık veya navigasyon loop'u oluşmadığını React Router history üzerinden doğrula. *(İlişkili: §4.4 — "Browser'ın kendi geri butonu ile tutarsızlık/loop oluşmuyor mu")*
- [x] 3.5.11 Geri butonunun mevcut tasarım diliyle (renk, spacing, font) tutarlı olduğunu görsel QA ile son kez teyit et. *(İlişkili: §4.4 — "Buton, mevcut tasarım diliyle ... tutarlı mı? Görsel QA")*

---

## 3.6 — Git / PR Hazırlığı ve Teslim

- [x] 3.6.1 `feature/v1.1-ui-branding` (veya proje konvansiyonuna uygun isimde) branch oluştur; Faz 0'da hazırlanan brand asset branch'inden devam et veya rebase et. *(Genel — sprint hazırlığı)*
- [x] 3.6.2 Değişiklikleri mantıksal commit'lere böl: "arka plan entegrasyonu", "logo/favicon entegrasyonu", "SUBMAIL rename", "admin geri butonu" — code review'de her değişikliğin ayrı ayrı incelenebilmesi için. *(Genel)*
- [x] 3.6.3 PR açıklamasına 3.5'teki QA sonuçlarını (Lighthouse skorları, cross-browser ekran görüntüleri, `grep` taraması sıfır sonuç kanıtı) ekle. *(İlişkili: §4.1, §4.2, §4.4 — test kanıtı)*
- [x] 3.6.4 Backend/DB'ye hiçbir değişiklik yapılmadığını PR açıklamasında teyit et — bu faz salt frontend + statik asset'tir. *(İlişkili: §3.1, §3.2, §3.4 — "Backend (.NET Core API): Değişiklik yok", "Veritabanı (MySQL): Değişiklik yok")*
- [x] 3.6.5 Faz 3'ün tamamlandığını ekip kanalında bildir; Faz 2 ile paralel yürütüldüyse iki PR'ın da Faz 4 (entegrasyon/QA) öncesi birleştiğini (merge) teyit et. *(İlişkili: §2 — "Faz 2 ile paralel yürütülebilir"; "Özet: Faz → Talep Eşleşmesi" tablosu — Faz 4'ün Faz 1, 2, 3'e bağımlılığı)*

---

## Faz 3 Çıktıları (Definition of Done)

- [x] Nokta ızgarası arka plan deseni login ve admin panel ekranlarında `bg-repeat` ile doğru görüntüleniyor, kontrast sorunu yok.
- [x] Yeni logo/wordmark header ve admin navbar'da, favicon tüm boyutlarda ve tarayıcılarda doğru görünüyor.
- [x] "EmailSubscriber" ifadesi kod tabanında (frontend, backend, mail template'leri, `<title>`, manifest) hiçbir yerde kalmamış — `grep` taraması temiz.
- [x] Admin login ekranında geri butonu çalışıyor, tasarım diliyle tutarlı.
- [x] Lighthouse Accessibility/Performance skorları düşmemiş.
- [x] Backend/DB'ye dokunulmamış; değişiklikler tamamen frontend + statik asset kapsamında.
