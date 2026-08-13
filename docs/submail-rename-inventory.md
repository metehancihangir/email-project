# "EmailSubscriber" => "SUBMAIL" Yeniden Adlandirma Envanteri

Hazirlanis: Faz 0 - 0.5 gorevi kapsaminda otomatik tarama ile cikarilmistir.
Amac: Faz 3 gelistiricisinin kullanmasi icin degistirilmesi gereken tum referanslari listeler.
Onemli Not: Bu listede sadece kullaniciya gorunen UI/icerik referanslari ve konfigurasyon adlari yer almaktadir.
C# namespace (EmailSubscriber.API.*) gibi kod altyapisi, Faz 3te ayri degerlendirilecektir.

---

## 1. Frontend -- Kullaniciya Gorunen Metin

| Dosya | Satir | Mevcut Deger | Degistirilecek Deger |
|-------|-------|--------------|----------------------|
| email-subscriber-client/src/components/Navbar.jsx | 10 | EmailSubscriber (span metni) | Submail (veya Logo.jsx komponenti) |
| email-subscriber-client/index.html | 17 | title: Email Subscriber | title: Submail |

---

## 2. Backend -- Konfigurasyon / JWT / DB Adi

| Dosya | Satir | Mevcut Deger | Not |
|-------|-------|--------------|-----|
| EmailSubscriber.API/appsettings.json | 13 | "Issuer": "EmailSubscriber" | JWT issuer -- Faz 3te "Submail" yapilabilir |
| EmailSubscriber.API/appsettings.json | 14 | "Audience": "EmailSubscriberClient" | JWT audience -- Faz 3te "SubmailClient" yapilabilir |
| EmailSubscriber.API/appsettings.json | 3 | Database=emailsubscriber | DB adi -- opsiyonel (migration gerektirir) |
| EmailSubscriber.API/appsettings.Development.json | 9 | Database=emailsubscriber | DB adi -- yukaridakiyle ayni |
| EmailSubscriber.API/appsettings.Development.json | 12 | JWT Secret icinde "...EmailSubscriberProject..." | Secret string -- opsiyonel |

---

## 3. E-posta Sablonlari -- Gonderen Adi / Footer

| Dosya | Durum |
|-------|-------|
| EmailSubscriber.API/Templates/WelcomeEmailTemplate.html | TEMIZ - "EmailSubscriber" gecmiyor |
| EmailSubscriber.API/Templates/ConfirmationEmailTemplate.html | TEMIZ - "EmailSubscriber" gecmiyor |

---

## 4. C# Namespace / Proje Altyapisi (Faz 3te Ayri Karar Alinacak)

Backend kaynak kodunun tamaminda EmailSubscriber.API.* namespace'i kullanilmaktadir.
Bunlar kullaniciya gorunmeyen teknik referanslardir.
Onerilen yaklasim: Namespace degisikligi proje genelinde kirilma riski tasir, Faz 3te tartisila.

Etkilenen dosyalar (namespace): Program.cs, Services/*.cs, Queue/*.cs, Controllers/*.cs,
Data/*.cs, Models/*.cs, DTOs/*.cs, Middleware/*.cs

---

## Ozet -- Oncelik Sirasi (Faz 3)

YUKSEK ONCELIK (UI-gorunur):
  - Navbar.jsx L10 --> Logo.jsx komponenti ile degistirilecek (Faz 3)
  - index.html <title> --> "Submail" yapilacak

ORTA ONCELIK (Config):
  - JWT Issuer/Audience --> "Submail"/"SubmailClient"

DUSUK ONCELIK / Opsiyonel:
  - DB adi emailsubscriber --> degisiklik migration gerektirir, ertelenebilir
  - C# namespace --> proje genelinde kirilma riski var, ayrica kararlastirilacak
  - E-posta sablonlari --> zaten temiz, degisiklik gerekmez
