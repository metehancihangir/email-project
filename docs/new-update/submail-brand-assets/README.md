# SUBMAIL — Marka Kimliği Asset Paketi

Bu paket, `update-phases.md` dokümanındaki Faz 3 (Marka Kimliği) için seçilen tasarımların dosyalarını içerir.

## İçerik

### Logo İkonu (S Monogram — Seçenek B)
- `logo-icon.svg` — Vektör kaynak dosya (favicon ve tüm boyutlar bundan türetildi)
- `logo-icon-512.png`, `logo-icon-192.png`, `logo-icon-32.png`, `logo-icon-16.png` — Farklı çözünürlükler
- `favicon.ico` — Çoklu boyutlu (16x16 + 32x32) klasik favicon
- `favicon-16x16.png`, `favicon-32x32.png` — Ayrı PNG favicon boyutları
- `apple-touch-icon.png` (180x180) — iOS ana ekran ikonu için

### Wordmark (İkon + Yazı — Seçenek 2)
- `wordmark.svg` — Vektör kaynak (header/navbar için, transparan arka plan)
- `wordmark.png` — 400x96 (2x/retina) raster versiyon

### Arka Plan Deseni (Nokta Izgarası — Seçenek 1)
- `background-pattern-tile.svg` — 24x24 tekrarlanabilir (tileable) karo
- `background-pattern-tile.png` — Aynı karonun raster versiyonu (48x48, 2x)

## Renk Kodu
- Ana marka rengi: `#10B981` (mevcut sitedeki yeşil ile birebir aynı)
- Metin/koyu ton: `#111827`

## Kullanım — React / Tailwind entegrasyonu

**1. Favicon (`public/index.html`):**
```html
<link rel="icon" type="image/x-icon" href="/favicon.ico" />
<link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png" />
<link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png" />
<link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png" />
<title>SUBMAIL</title>
```
Dosyaları `public/` klasörüne kopyalayın.

**2. Header/Navbar wordmark (`Logo.jsx`):**
```jsx
import wordmark from '@/assets/wordmark.svg';

export default function Logo({ className }) {
  return <img src={wordmark} alt="Submail" className={className} />;
}
```

**3. Arka plan deseni (Tailwind):**
```js
// tailwind.config.js
backgroundImage: {
  'auth-pattern': "url('/assets/background-pattern-tile.svg')",
}
```
```jsx
<div className="bg-auth-pattern bg-repeat min-h-screen">
```
Karo küçük ve tekrarlanabilir olduğu için `bg-repeat` kullanılmalı, `bg-cover` değil.

**4. "EmailSubscriber" → "SUBMAIL" yeniden adlandırması:**
Aşağıdaki yerlerde metin değişikliği yapılmalı:
- Header/Navbar component'i (mevcut "EmailSubscriber" yazısının olduğu yer) → wordmark component'i ile değiştirilecek
- `<title>` etiketi (`index.html`)
- Varsa `manifest.json` içindeki `name`/`short_name` alanları
- Admin panel header'ı
- E-posta template'lerindeki gönderen adı / footer imzası (varsa)
