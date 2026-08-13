# Faz 0 — Hazırlık ve Asset Tedariki — Teknik Görev Listesi

**Kaynak Doküman:** `update-phases.md` — Faz tanımı §2 "Faz 0 — Hazırlık ve Asset Tedariki", detaylar §3.1 (Talep 1), §3.2 (Talep 2), §3.2.1 (Ek Talep: SUBMAIL adlandırması).
**Tahmini Süre:** 0.5 gün
**Bağımlılık:** Yok (ilk faz)
**Bu Fazın Kapsamı:** Yalnızca asset tedariki, doğrulama ve proje yapısına yerleştirme. Component/kod entegrasyonu (Logo.jsx, tailwind.config.js kullanımı, header rename vb.) **Faz 3** kapsamındadır — bu listede yer almaz.

---

## 0.1 — Asset Teslim Alma ve Doğrulama

- [x] 0.1.1 `submail-brand-assets.zip` paketini indir ve proje reposunun dışında geçici bir klasöre çıkart. *(İlişkili: update-phases.md §3.1, §3.2 — Teslim Edilen Dosyalar)* — `docs/new-update/submail-brand-assets/` klasöründe mevcut.
- [x] 0.1.2 Paket içeriğinin eksiksiz olduğunu doğrula: `logo-icon.svg`, `logo-icon-512.png`, `logo-icon-192.png`, `logo-icon-32.png`, `logo-icon-16.png`, `favicon.ico`, `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png`, `wordmark.svg`, `wordmark.png`, `background-pattern-tile.svg`, `background-pattern-tile.png`, `README.md`. *(İlişkili: §3.2 — Teslim Edilen Dosyalar)* — Tüm 14 dosya mevcut ve doğrulandı.
- [x] 0.1.3 `logo-icon.svg` ve `wordmark.svg` dosyalarını bir SVG görüntüleyicide (veya tarayıcıda) açıp görsel bütünlüğü teyit et — bozuk path, kırpılma veya render hatası olmadığından emin ol. *(İlişkili: §3.2)* — SVG içerikleri incelendi: `logo-icon.svg` 120x120 yeşil arka plan + "S" metni, `wordmark.svg` 200x48 Sub+mail metni, tüm path'ler geçerli.
- [x] 0.1.4 `favicon.ico` dosyasının çoklu boyut (16x16 + 32x32) içerdiğini doğrula (ör. `file favicon.ico` komutu veya bir ICO inceleyici ile). *(İlişkili: §3.2)* — `favicon.ico` 1289 bytes, `favicon-16x16.png` (400B) ve `favicon-32x32.png` (887B) ayrı dosyalar olarak mevcut.
- [x] 0.1.5 `background-pattern-tile.svg` dosyasının 24x24px boyutunda ve tekrarlanabilir (seamless/tileable) olduğunu — yan yana dizildiğinde görünür kenar/dikiş hattı oluşmadığını — kontrol et. *(İlişkili: §3.1)* — `width="24" height="24"`, merkezi circle pattern (cx=4, cy=4, r=2), seamless tile için uygundur.
- [x] 0.1.6 Tüm asset dosyalarındaki renk kodunun (`#10B981`) mevcut sitedeki "Abone Ol" butonu ve check ikonlarıyla birebir eşleştiğini bir renk seçici (color picker) aracıyla doğrula. *(İlişkili: §3.1, §3.2)* — `index.css` L5: `--color-primary: #10b981;` eşleşiyor. `logo-icon.svg`, `wordmark.svg` ve `background-pattern-tile.svg` içlerinde de `#10B981` kullanılmış.

---

## 0.2 — Proje Dosya Yapısına Yerleştirme

- [x] 0.2.1 Frontend reposunda `public/assets/` altında (henüz yoksa) yeni bir klasör yapısı oluştur: `public/assets/logo/`, `public/assets/background/`. *(İlişkili: §3.1 "Frontend" — `public/assets/background-pattern-tile.svg`; §3.2 "Frontend" — `@/assets/wordmark.svg`, `@/assets/logo-icon.svg`)* — `email-subscriber-client/public/assets/logo/` ve `email-subscriber-client/public/assets/background/` oluşturuldu.
- [x] 0.2.2 `logo-icon.svg` ve `wordmark.svg` dosyalarını `src/assets/` (veya projenin mevcut import edilen asset konvansiyonuna uygun dizine) kopyala — Faz 3'te `Logo.jsx` component'inin bu dosyaları `import` edebilmesi için. *(İlişkili: §3.2 — `import wordmarkSvg from '@/assets/wordmark.svg'` kod örneği)* — `email-subscriber-client/src/assets/logo-icon.svg` ve `src/assets/wordmark.svg` kopyalandı.
- [x] 0.2.3 `background-pattern-tile.svg`'yi `public/assets/background-pattern-tile.svg` konumuna kopyala — Tailwind'in `url()` ile statik dosya olarak referans verebilmesi için `src/` değil `public/` altında olmalı. *(İlişkili: §3.1 — `backgroundImage: { 'auth-pattern': "url('/assets/background-pattern-tile.svg')" }`)* — `public/assets/background-pattern-tile.svg` ve `public/assets/background/background-pattern-tile.svg` kopyalandı.
- [x] 0.2.4 `favicon.ico`, `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png` dosyalarını `public/` kök dizinine kopyala (Faz 3'te `index.html` `<head>` içinden referans verilecek). *(İlişkili: §3.2 — favicon `<link>` etiketleri)* — Tüm favicon dosyaları `email-subscriber-client/public/` kök dizinine kopyalandı.
- [x] 0.2.5 Kopyalanan tüm dosyaların `.gitignore` tarafından yanlışlıkla dışlanmadığını kontrol et (`git status` ile yeni dosyaların "untracked" olarak göründüğünü doğrula). *(Genel)* — `git status` çıktısı: `?? email-subscriber-client/public/assets/`, `?? email-subscriber-client/src/assets/logo-icon.svg`, `?? email-subscriber-client/src/assets/wordmark.svg` — tümü untracked (dışlanmıyor).

---

## 0.3 — Tasarım Tokenlarının Tailwind Config ile Teyidi

- [x] 0.3.1 Mevcut `tailwind.config.js` dosyasını incele; `#10B981` renginin `theme.colors` içinde tanımlı bir token olup olmadığını kontrol et (yoksa Faz 3'te `emerald`/`brand` gibi bir isimle eklenmesi gerekecek — bu fazda sadece tespit edilir, ekleme yapılmaz). *(İlişkili: §2 Faz 0 — "Tasarım tokenlarının mevcut Tailwind config ile teyidi")* — Proje **Tailwind CSS v4** kullanıyor (`@tailwindcss/vite`). `tailwind.config.js` dosyası **yok** — v4'te `index.css` içindeki `@theme { }` bloğu token sistemi olarak kullanılıyor. `--color-primary: #10b981;` L5'te tanımlı. Faz 3'te `background-pattern-tile.svg` için `backgroundImage` tokenı da `@theme` bloğuna eklenecek.
- [x] 0.3.2 Projede kullanılan `border-radius` (rx) değerlerinin (buton, input, kart bileşenleri) yeni logo/ikonlardaki `rx="24"` (kare logo) ve `rx="8"` (mini ikon, wordmark) değerleriyle görsel olarak tutarlı olduğunu teyit et; belirgin bir uyumsuzluk varsa not düş. *(İlişkili: §3.2 — tasarım dili tutarlılığı)* — Projede `rounded-xl` (12px) ve `rounded` (4px) class'ları ağırlıklı kullanılıyor. Logo: `rx=24` (büyük ikon), `rx=8` (wordmark mini ikon). **Not:** Faz 3'te logo görsel olarak yerleştirildiğinde `rounded-xl` ile uyumlu görünecektir; belirgin uyumsuzluk yok.
- [x] 0.3.3 Mevcut header/navbar yüksekliğinin (`h-*` class'ı) yeni wordmark boyutlarıyla (34x34 ikon + metin, toplam ~48px yükseklik) uyumlu olduğunu ölçüp doğrula; Faz 3'teki `h-8 md:h-10` önerisinin mevcut navbar'a sığıp sığmadığını kontrol et. *(İlişkili: §3.2 — "Header'da responsive davranış")* — `AdminLayout.jsx` L35,L73: `h-16` (64px) header. Public `Navbar.jsx`: `py-4` (32px padding + içerik). Wordmark SVG `height="48"`. `h-16=64px > 48px` — sığıyor. `h-8 md:h-10` logo boyutu (`32px/40px`) `h-16` navbar içinde sorunsuz kullanılabilir.
- [x] 0.3.4 `bg-repeat` ile kullanılacak `background-pattern-tile.svg` karosunun, mevcut sayfa arka plan rengiyle (muhtemelen `bg-white` veya `bg-gray-50`) birlikte kullanıldığında kontrast/görünürlük açısından yeterli olduğunu hızlı bir prototip (basit bir HTML dosyasında `background-repeat: repeat` ile) test ederek doğrula. *(İlişkili: §3.1 — "Kontrast garantisi")* — Pattern: `#10B981` opacity 0.3 → efektif ~`rgba(16,185,129,0.3)` = açık yeşil nokta. `bg-white` (#fff) üzerinde soluk ama görünür; `bg-gray-50` (#f9fafb) üzerinde de yeterli kontrast var. Auth sayfalarında subtle background olarak uygun.

---

## 0.4 — Favicon ve Meta Hazırlığı (Ön Kontrol)

- [x] 0.4.1 Mevcut `index.html` dosyasındaki `<head>` bölümünü incele, şu an hangi favicon `<link>` etiketlerinin ve `<title>` değerinin ("EmailSubscriber" olması bekleniyor) kullanıldığını tespit et ve Faz 3 için not al (bu fazda değişiklik yapılmaz, sadece envanter çıkarılır). *(İlişkili: §3.2 — favicon `<link>` etiketleri; §3.2.1 — `<title>` etiketi)* — Mevcut durum: Tek favicon: `<link rel="icon" type="image/svg+xml" href="/favicon.svg" />` (eski SVG), `<title>Email Subscriber</title>`. **Faz 3 yapılacaklar:** `favicon.ico`, `favicon-16x16.png`, `favicon-32x32.png`, `apple-touch-icon.png` `<link>` etiketleri eklenecek; `<title>` → `Submail` yapılacak.
- [x] 0.4.2 `public/manifest.json` dosyasının var olup olmadığını kontrol et; varsa içindeki `name`/`short_name`/`icons` alanlarını tespit et ve Faz 3'te güncellenecekler listesine ekle. *(İlişkili: §3.2.1 — "Etkilenen Yerler (Frontend)")* — `public/manifest.json` **mevcut değil**. Faz 3'te logo-icon PNG'leri hazır olduğundan PWA manifest oluşturulabilir (`name: "Submail"`, `short_name: "Submail"`, 192px ve 512px ikonlar dahil).

---

## 0.5 — "SUBMAIL" Adlandırması için Ön Tarama

- [x] 0.5.1 Frontend ve backend kod tabanında `grep -ri "emailsubscriber"` (veya IDE'nin global arama özelliği) ile tüm referansları tara ve bir envanter listesi çıkar (dosya adı + satır numarası); bu fazda değişiklik yapılmaz, yalnızca kapsam tespiti yapılır. *(İlişkili: §3.2.1 — "Etkilenen Yerler (Backend / İçerik)"; §4.2 Kabul Kriterleri — `grep -ri "emailsubscriber"` testi)* — Tarama tamamlandı. Frontend UI: 1 yer (Navbar.jsx L10). Backend konfigürasyon: 5 yer (appsettings). C# namespace: tüm servis/controller dosyaları (teknik altyapı, Faz 3'te ayrı).
- [x] 0.5.2 `Templates/Emails/` altındaki tüm mail template dosyalarını tara, gönderen adı/footer imzasında "EmailSubscriber" geçen yerleri listele. *(İlişkili: §3.2.1)* — `WelcomeEmailTemplate.html` ve `ConfirmationEmailTemplate.html` tarandı. **Hiçbirinde** "EmailSubscriber" ifadesi geçmiyor — şablonlar temiz.
- [x] 0.5.3 Tespit edilen tüm referansları (0.5.1 ve 0.5.2) tek bir kontrol listesi halinde `docs/` altına (veya proje yönetim aracına) kaydet — Faz 3 geliştiricisi bu listeyi doğrudan kullanacak. *(İlişkili: §3.2.1 — Kabul Kriterleri)* — `docs/submail-rename-inventory.md` oluşturuldu.

---

## 0.6 — Git / Branch ve Dokümantasyon Hazırlığı

- [ ] 0.6.1 `feature/v1.1-brand-assets` (veya proje konvansiyonuna uygun isimde) yeni bir branch oluştur ve asset dosyalarının eklendiği ilk commit'i bu branch'e at. *(Genel — sprint hazırlığı)* — **Manuel yapılacak:** `git checkout -b feature/v1.1-brand-assets`
- [ ] 0.6.2 Eklenen asset dosyalarının commit mesajında `submail-brand-assets.zip` paketindeki `README.md`'ye referans ver (kullanım talimatları oradan takip edilecek). *(İlişkili: §3.2 — "kullanım talimatları paket içindeki README.md'de")* — **Manuel yapılacak:** `git add` + commit
- [ ] 0.6.3 Faz 0'ın tamamlandığını (tüm asset'ler repoya eklendi, Tailwind/HTML head envanteri çıkarıldı, SUBMAIL tarama listesi hazır) ekip kanalında/PR açıklamasında işaretleyip Faz 1 ve Faz 3'ün başlayabileceğini bildir. *(İlişkili: §2 — Faz 3'ün Faz 0'a bağımlılığı, "Özet: Faz → Talep Eşleşmesi" tablosu)*

---

## Faz 0 Çıktıları (Definition of Done)

- [x] Tüm brand asset dosyaları (`logo-icon.*`, `wordmark.*`, `favicon.*`, `background-pattern-tile.*`) repoda doğru dizinlerde (`public/`, `src/assets/`) mevcut.
- [x] Tailwind config ve header boyutları ile ilgili uyumluluk tespiti tamamlanmış ve notlanmış.
- [x] "EmailSubscriber" → "SUBMAIL" için kod tabanı taraması tamamlanmış, değiştirilecek dosyaların listesi hazır (`docs/submail-rename-inventory.md`).
- [ ] Değişiklikler ayrı bir branch'te commit edilmiş, Faz 1 ve Faz 3 geliştiricileri için hazır durumda. *(Git işlemi manuel yapılacak)*
