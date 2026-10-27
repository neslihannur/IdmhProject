# IDMH — sunucu

**Şifre, anahtar veya token bu dosyaya yazılmaz.**

| | |
|---|---|
| Sunucu | biora-1 `65.21.182.105`, Coolify → proje **Portfolio** / production |
| Uygulama | Dockerfile (depo kökü, dal `master`), konteyner portu **8080** |
| Adres | https://idmh.neslihannur.com.tr (Plesk DNS: `idmh` A → 65.21.182.105) |
| Veritabanı | Coolify'da ayrı bir PostgreSQL kaynağı; bağlantı `DATABASE_URL` |

## Ortam değişkenleri

| Ad | Açıklama |
|---|---|
| `DATABASE_URL` | PostgreSQL kaynağının "Postgres URL (internal)" değeri |
| `Admin__Email`, `Admin__Password` | İlk yönetici (yalnızca hiç yönetici yokken) |
| `Smtp__Host`, `Smtp__Port`, `Smtp__User`, `Smtp__Password`, `Smtp__FromName` | E-posta gönderimi |
| `Site__NotifyEmail` | Yeni talep bildirimlerinin gideceği adres (varsayılan appsettings.json) |

## Notlar

- Portföy çalışmasıdır: tüm sayfalar `noindex` ve `robots.txt` ile arama motorlarına kapalı; gerçek idmh.com.tr'nin önüne geçmez.
- Fotoğraflar Wikimedia Commons'tan serbest lisanslı; kaynaklar `/gorsel-kaynaklari` sayfasında (`Services/Credits.cs`).
- Panelden yüklenen görseller veritabanında (`Media` tablosu) saklanır, `/media/{id}` ile sunulur.
- Yönetim: `/yonetim` — projeler, yazılar, gelen talepler.
