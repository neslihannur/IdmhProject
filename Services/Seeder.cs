using IdmhProject.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdmhProject.Services;

/// İlk açılışta yönetici hesabı, örnek projeler ve yazılar.
public static class Seeder
{
    public static async Task RunAsync(AppDb db, IConfiguration config, ILogger log)
    {
        if (!await db.AdminUsers.AnyAsync() && config["Admin:Email"] is { Length: > 0 } email && config["Admin:Password"] is { Length: > 0 } password)
        {
            var user = new AdminUser { Email = email.Trim().ToLowerInvariant() };
            user.PasswordHash = new PasswordHasher<AdminUser>().HashPassword(user, password);
            db.AdminUsers.Add(user);
            await db.SaveChangesAsync();
            log.LogInformation("Yönetici hesabı açıldı: {Email}", user.Email);
        }

        if (!await db.Projects.AnyAsync())
        {
            var now = DateTime.UtcNow;
            var list = Projects();
            for (var i = 0; i < list.Count; i++) { list[i].SortOrder = i; list[i].CreatedAt = now; }
            db.Projects.AddRange(list);
            await db.SaveChangesAsync();
            log.LogInformation("Örnek projeler eklendi");
        }

        if (!await db.Posts.AnyAsync())
        {
            db.Posts.AddRange(Posts());
            await db.SaveChangesAsync();
            log.LogInformation("Örnek yazılar eklendi");
        }
    }

    static List<Project> Projects() =>
    [
        new()
        {
            Slug = "fatih-bulvari-ofis-binasi", Title = "Fatih Bulvarı Ofis Binası", Discipline = "Mimarlık", Typology = "Ofis",
            Location = "Sultanbeyli, İstanbul", Year = "2024", Area = "2.400 m²", Status = "Tamamlandı", IsFeatured = true,
            Cover = "/img/glass-office.jpg", Gallery = ["/img/glass-office-2.jpg", "/img/louvers.jpg", "/img/campus-night.jpg"],
            Summary = "Kavisli cam cephesiyle bulvarın gürültüsünü içeri almadan ışığı içeri alan, beş katlı bir çalışma yapısı.",
            Body = """
                Bulvar üzerindeki dar ve derin parselde işverenin beklentisi netti: gün boyu doğal ışık alan, esnek bölünebilen ve kiracı değişikliklerine kolayca uyum sağlayan ofis katları.

                Yapının köşesini yumuşatan kavisli cam cephe, hem sokak siluetinde akılda kalan bir iz bırakıyor hem de kat planlarında kolonsuz, kesintisiz bir çalışma alanı sağlıyor. Çift cidarlı cephe sistemi trafik gürültüsünü azaltırken yaz aylarında güneş kazancını sınırlıyor.

                ## Esnek kat planı
                Servis çekirdeği parselin arka kenarına toplandı; böylece her kat tek kiracıya ya da dört ayrı ofise bölünebiliyor. Yükseltilmiş döşeme ve modüler tavan sistemi, bölme değişikliklerinin mekanik tesisata dokunmadan yapılmasına imkân veriyor.

                > İyi bir ofis binası, içindekilerin her sabah fark etmeden sevdiği yapıdır.

                Zemin katta bulvara açılan kafe ve giriş holü, yapıyı mahalleyle buluşturan yarı kamusal bir eşik olarak tasarlandı. Gece aydınlatması, cephenin ritmini abartmadan görünür kılacak şekilde sade tutuldu.
                """,
        },
        new()
        {
            Slug = "kuzguncuk-tugla-ev", Title = "Kuzguncuk Tuğla Ev", Discipline = "Mimarlık", Typology = "Konut",
            Location = "Üsküdar, İstanbul", Year = "2023", Area = "320 m²", Status = "Tamamlandı", IsFeatured = true,
            Cover = "/img/brick-house.jpg", Gallery = ["/img/white-wall.jpg"],
            Summary = "Mahallenin tarihi dokusuna saygı duyan, el yapımı tuğla ve derin pencerelerle kurgulanmış üç katlı bir aile evi.",
            Body = """
                Kuzguncuk'un eğimli sokaklarında, ahşap ve kâgir evlerin arasında yeni bir yapı tasarlamak; yüksek sesle konuşmamayı gerektiriyor. Evin kütlesi komşu yapıların saçak hizasını takip ediyor, cephede ise tek bir malzeme konuşuyor: el yapımı, mat tuğla.

                Derin pencere boşlukları hem iç mekânda oturma nişleri oluşturuyor hem de yaz güneşini doğal bir gölgelikle kesiyor. Üst katta, Boğaz'a bakan tek bir geniş pencere evin en çok kullanılan köşesi oldu.

                ## İçeride sadelik
                İç mekânda beyaz badana, meşe döşeme ve doğal taş tezgâhlar dışında malzeme kullanılmadı. Mutfak ve yemek alanı bahçeye tamamen açılabiliyor; böylece ev yaz aylarında avluyla birlikte tek bir mekâna dönüşüyor.
                """,
        },
        new()
        {
            Slug = "ege-beyaz-ev", Title = "Ege Beyaz Ev", Discipline = "Mimarlık", Typology = "Yazlık konut",
            Location = "Bodrum, Muğla", Year = "2023", Area = "260 m²", Status = "Tamamlandı", IsFeatured = true,
            Cover = "/img/white-house.jpg", Gallery = ["/img/stone-stair.jpg", "/img/stair-shadow.jpg"],
            Summary = "Gölgenin bir yapı malzemesi gibi kullanıldığı, merdivenler ve teraslarla yamaca yerleşen beyaz kütleler.",
            Body = """
                Ege'de yaz, gölge demek. Yamaca kademeli olarak yerleşen beyaz kütleler, her katta bir sonraki katın terasına gölge düşürecek şekilde birbirinin üzerine kaydırıldı.

                Dış merdivenler yapının omurgası: odaları birbirine bağlarken her basamakta manzarayı farklı bir açıdan çerçeveliyor. Kireç sıva ve yerel taş, yapının bakım ihtiyacını azaltırken bölgenin geleneksel yapı diliyle bağ kuruyor.

                ## Doğal serinlik
                Karşılıklı açılan pencereler ve yüksek tavanlar sayesinde ev, en sıcak günlerde bile klimaya ihtiyaç duymadan serin kalabiliyor. Yağmur suyu teras altındaki depoda toplanıp bahçe sulamasında kullanılıyor.
                """,
        },
        new()
        {
            Slug = "avlu-kultur-merkezi", Title = "Avlu Kültür Merkezi", Discipline = "Mimarlık", Typology = "Kamusal yapı",
            Location = "İstanbul", Year = "2022", Area = "5.800 m²", Status = "Yarışma önerisi", IsFeatured = true,
            Cover = "/img/court.jpg", Gallery = ["/img/concrete-steps.jpg", "/img/facade-vertical.jpg", "/img/columns.jpg"],
            Summary = "Kente açılan bir avlu etrafında toplanan kütüphane, atölye ve sergi salonları için ulusal yarışma önerisi.",
            Body = """
                Önerinin çıkış noktası, bir kültür merkezinin kapısının değil avlusunun olması gerektiği fikriydi. Kütüphane, atölyeler ve sergi salonları, gün boyu herkese açık kalan bir iç avlunun çevresinde toplanıyor.

                Brüt beton, yapının hem taşıyıcısı hem de son kat malzemesi. Avluya basamaklanan oturma kademeleri açık hava etkinlikleri için doğal bir amfi oluşturuyor; yaz akşamları sinema, hafta sonları çocuk atölyeleri için kullanılabiliyor.

                ## Işık ve ritim
                Düşey beton lameller kuzey cephesinde yumuşak ve eşit bir ışık sağlarken, güney cephesinde güneşi kesen derin bir doku oluşturuyor. Sütun dizileri, uzun koridorları yönlendirici bir ritimle sakinleştiriyor.
                """,
        },
        new()
        {
            Slug = "nisantasi-daire", Title = "Nişantaşı Daire", Discipline = "İç Mimarlık", Typology = "Konut",
            Location = "Şişli, İstanbul", Year = "2024", Area = "185 m²", Status = "Tamamlandı", IsFeatured = true,
            Cover = "/img/dining.jpg", Gallery = ["/img/kitchen.jpg"],
            Summary = "1960'lardan kalma bir apartman dairesinin, yüksek tavanları ve kemerli duvar nişleriyle yeniden yorumlanması.",
            Body = """
                Daire, yıllar içinde eklenen asma tavanlar ve bölmelerle asıl karakterini kaybetmişti. İlk iş, bütün eklemeleri söküp 3,4 metrelik tavan yüksekliğini ve iki cepheden alınan ışığı yeniden ortaya çıkarmak oldu.

                Salonun uzun duvarı boyunca açılan kemerli nişler, hem kitaplık hem de dolap işlevi görüyor; tek bir sıva rengiyle duvarın parçası gibi okunuyorlar. Zemin, açık tonlu traverten ve meşe ile iki malzemeye indirildi.

                ## Gizli depolama
                Mutfakta tavana kadar uzanan dolaplar, buzdolabı ve kileri tamamen saklıyor. Ada tezgâhı, hem hazırlık alanı hem de günlük kahvaltı masası olarak kullanılıyor.
                """,
        },
        new()
        {
            Slug = "beykoz-villa-ic-mekan", Title = "Beykoz Villa", Discipline = "İç Mimarlık", Typology = "Konut",
            Location = "Beykoz, İstanbul", Year = "2024", Area = "540 m²", Status = "Tamamlandı", IsFeatured = false,
            Cover = "/img/lounge.jpg", Gallery = [],
            Summary = "Geniş bir ailenin günlük yaşamına göre yeniden kurgulanan iki katlı yaşam alanları ve özel tasarım mobilyalar.",
            Body = """
                Villanın ilk katı, sık misafir ağırlayan bir aile için tasarlandı: yemek, oturma ve kütüphane alanları birbirine görsel olarak bağlı, ama kayar panellerle ayrılabiliyor.

                Mobilyaların büyük bölümü atölyemizde tasarlanıp yerel ustalarla üretildi. Doğal taş şömine duvarı, salonun ağırlık merkezi; çevresindeki oturma grubu manzaraya ve ateşe aynı anda bakacak şekilde konumlandı.

                ## Aydınlatma senaryoları
                Her mekân için gündüz, akşam ve misafir senaryoları hazırlandı. Gizli şerit aydınlatmalar tavan derinliğini vurgularken, sarkıt armatürler masa ve oturma gruplarını odak noktası hâline getiriyor.
                """,
        },
        new()
        {
            Slug = "moda-butik-otel", Title = "Moda Butik Otel", Discipline = "İç Mimarlık", Typology = "Konaklama",
            Location = "Kadıköy, İstanbul", Year = "2023", Area = "24 oda", Status = "Tamamlandı", IsFeatured = true,
            Cover = "/img/hotel-room.jpg", Gallery = ["/img/spiral-lamp.jpg"],
            Summary = "Eski bir apartmanın 24 odalı butik otele dönüşümü; geometrik başlıklar ve koyu tonlarla sakin bir konaklama deneyimi.",
            Body = """
                Moda'daki dört katlı apartman, yapısal müdahaleler en aza indirilerek 24 odalı bir butik otele dönüştürüldü. Her katta oda tipleri, mevcut taşıyıcı duvarlara göre farklılaşıyor; bu da her odayı biraz kendine özgü kılıyor.

                Odalarda koyu antrasit duvarlar, keten tekstiller ve geometrik desenli başlıklar kullanıldı. Karartma perdeleri ve akustik paneller, şehrin ortasında sessiz bir uyku sunuyor.

                ## Merdiven holü
                Yapının kalbindeki eliptik merdiven, korunarak otelin simgesi hâline getirildi. Ortadaki boşluğa asılan tek bir aydınlatma elemanı, merdiveni gece boyunca bir fener gibi yakıyor.
                """,
        },
        new()
        {
            Slug = "kadikoy-kahve-evi", Title = "Kadıköy Kahve Evi", Discipline = "İç Mimarlık", Typology = "Yeme-içme",
            Location = "Kadıköy, İstanbul", Year = "2022", Area = "140 m²", Status = "Tamamlandı", IsFeatured = false,
            Cover = "/img/cafe.jpg", Gallery = [],
            Summary = "Gün boyu çalışmaya da uğrayıp geçmeye de uygun, ahşap sedirler ve küre armatürlerle sıcak bir mahalle kahvecisi.",
            Body = """
                Brief kısaydı: hem dizüstü bilgisayarıyla saatlerce oturan hem de beş dakikalık bir espresso için uğrayan misafire aynı özeni gösteren bir mekân.

                Duvar boyunca uzanan ahşap sedir, mekânın kapasitesini artırırken pencere önünü en sevilen köşe yaptı. Ortadaki yuvarlak masalar kolayca yeniden düzenlenebiliyor; akşam etkinliklerinde mekân küçük bir salona dönüşüyor.

                Küre armatürler farklı yüksekliklerde asılarak tavanı hareketlendiriyor, sert yüzeylerin yarattığı yankı akustik tavan panelleriyle dengelendi.
                """,
        },
        new()
        {
            Slug = "atasehir-cephe-yenileme", Title = "Ataşehir Konut Cephe Yenileme", Discipline = "Mimari Revizyon", Typology = "Toplu konut",
            Location = "Ataşehir, İstanbul", Year = "2024", Area = "18.000 m²", Status = "Uygulamada", IsFeatured = false,
            Cover = "/img/housing.jpg", Gallery = ["/img/facades.jpg"],
            Summary = "1990'lardan kalma bir konut sitesinin cephe, yalıtım ve ortak alan revizyonu; sakinler evlerinde yaşamaya devam ederken.",
            Body = """
                Proje, mevcut bir konut sitesinin enerji performansını ve görünümünü iyileştirmek için hazırlandı. En büyük zorluk, 240 dairenin sakinleri evlerinde yaşamaya devam ederken uygulamanın yürütülmesiydi.

                Cephe, havalandırmalı bir giydirme sistemle yenileniyor; mevcut balkon korkulukları kaldırılıp daha şeffaf ve güvenli bir sistemle değiştiriliyor. Isı yalıtımıyla birlikte ısınma giderlerinde belirgin bir düşüş hedefleniyor.

                ## Aşamalı uygulama
                Uygulama blok blok, her blok ortalama altı haftada tamamlanacak şekilde planlandı. Ortak alanlarda giriş holleri, aydınlatma ve yönlendirme tabelaları da revizyonun parçası.
                """,
        },
    ];

    static List<JournalPost> Posts() =>
    [
        new()
        {
            Slug = "isigin-mimarisi", Title = "Işığın mimarisi: gün ışığını tasarımın merkezine almak", Tag = "Tasarım",
            Cover = "/img/columns.jpg", PublishedOn = new DateOnly(2026, 9, 18),
            Excerpt = "Bir mekânı en çok değiştiren malzeme, çoğu zaman bedava olanıdır. Gün ışığını projenin ilk eskizinden itibaren nasıl planladığımızı anlatıyoruz.",
            Body = """
                Bir projeye başlarken ilk sorduğumuz sorulardan biri şu: güneş bu parselde nereden doğup nereye batıyor? Cevap, planın kendisini şekillendiriyor.

                ## Yön, her şeyden önce gelir
                Yaşam alanlarını güney ve batıya, servis mekânlarını kuzeye toplamak; ısıtma ve aydınlatma giderini doğrudan etkiler. Ama işin estetik bir boyutu da var: sabah ışığı alan bir mutfak ile öğleden sonra ışığı alan bir çalışma odası, aynı malzemeyle bambaşka hissettirir.

                ## Gölge de bir malzemedir
                Derin pencere boşlukları, saçaklar ve lameller yalnızca güneşi kesmez; duvarlara gün boyu değişen bir doku çizer. Ege'deki projelerimizde merdivenleri ve terasları, gölgeyi bir sonraki kata taşıyacak şekilde kurguluyoruz.

                > Işık, mekânın hangi saatte nasıl yaşanacağını belirler.

                ## İç mekânda yansıma
                Açık tonlu, mat yüzeyler ışığı derine taşır; parlak yüzeyler ise kamaşmaya yol açabilir. Bu yüzden malzeme seçimini ışık çalışmasıyla birlikte yapıyoruz. Yapay aydınlatma da gün ışığını taklit etmeye değil, akşam saatlerine yeni bir atmosfer kazandırmaya odaklanmalı.
                """,
        },
        new()
        {
            Slug = "ic-mimarlik-sureci", Title = "Bir iç mimarlık projesi nasıl ilerler? Altı adımda süreç", Tag = "Süreç",
            Cover = "/img/sketch.jpg", PublishedOn = new DateOnly(2026, 8, 27),
            Excerpt = "İlk görüşmeden anahtar teslimine kadar bir iç mimarlık projesinde neler oluyor, hangi kararlar ne zaman veriliyor?",
            Body = """
                Müşterilerimizden en sık duyduğumuz soru: "Nereden başlıyoruz?" Projeler birbirinden farklı olsa da yol haritası büyük ölçüde aynı.

                ## 1. Tanışma ve ihtiyaç programı
                Mekânı birlikte geziyor, günlük rutininizi, depolama ihtiyaçlarınızı ve bütçenizi konuşuyoruz. Bu görüşmenin sonunda yazılı bir ihtiyaç programı hazırlıyoruz.

                ## 2. Rölöve ve analiz
                Mevcut durumu milimetrik olarak ölçüyor, tesisat ve taşıyıcı sistemi inceliyoruz. Sürprizlerin çoğu bu aşamada ortaya çıkar.

                ## 3. Konsept
                Plan alternatifleri, malzeme paleti ve atmosfer görselleriyle projenin ruhunu belirliyoruz.

                ## 4. Uygulama projesi
                Detay çizimleri, aydınlatma ve elektrik planları, özel mobilya çizimleri ve metraj listeleri hazırlanıyor.

                ## 5. Teklif ve üretim
                Usta ve tedarikçilerden karşılaştırmalı teklifler alıyor, takvimi netleştiriyoruz.

                ## 6. Uygulama ve teslim
                Şantiyeyi düzenli ziyaretlerle takip ediyor, son kontrolleri yapıp mekânı anahtar teslim ediyoruz.
                """,
        },
        new()
        {
            Slug = "mimari-revizyon-mu-yeniden-tasarim-mi", Title = "Mimari revizyon mu, yeniden tasarım mı?", Tag = "Rehber",
            Cover = "/img/facades.jpg", PublishedOn = new DateOnly(2026, 7, 9),
            Excerpt = "Mevcut bir projede değişiklik ihtiyacı doğduğunda, ne zaman revizyon yeterli olur, ne zaman baştan düşünmek gerekir?",
            Body = """
                Ruhsatı alınmış bir projede kat planını değiştirmek, mevcut bir binanın cephesini yenilemek ya da işlevini dönüştürmek… Bunların hepsi mimari revizyonun konusu.

                ## Revizyon ne zaman yeterli?
                Taşıyıcı sistem ve ana kütle korunuyorsa, değişiklik bölme duvarlar, cephe malzemesi ya da tesisat gibi katmanlarla sınırlıysa revizyon çoğu zaman en hızlı ve ekonomik yoldur.

                ## Ne zaman baştan düşünmeli?
                Yapının kullanım amacı tamamen değişiyorsa, deprem performansı yetersizse ya da imar durumu yeni hakkı tanıyorsa, eldeki projeyi yamamak yerine yeniden tasarlamak uzun vadede daha doğru olabilir.

                > Doğru karar, kâğıt üzerinde değil sahada verilir.

                Her iki durumda da ilk adım aynı: mevcut durumun doğru bir analizi. Rölöve ve yapı incelemesi olmadan verilen kararlar, uygulamada pahalı sürprizlere dönüşebilir.
                """,
        },
        new()
        {
            Slug = "malzeme-durustlugu", Title = "Malzeme dürüstlüğü: beton, ahşap ve taş", Tag = "Malzeme",
            Cover = "/img/concrete-steps.jpg", PublishedOn = new DateOnly(2026, 6, 2),
            Excerpt = "Bir malzemenin kendisi gibi görünmesine izin vermek neden önemli? Az ama doğru malzemeyle çalışmanın yararları.",
            Body = """
                Bir yüzeyi başka bir malzemeymiş gibi göstermek kısa vadede kolay gelir; ama yıllar içinde en hızlı eskiyen yüzeyler genellikle bunlardır.

                ## Brüt beton
                Doğru kalıp ve özenli işçilikle beton, hem taşıyıcı hem de son kat olabilir. Kalıp izleri, yapının nasıl inşa edildiğinin dürüst bir kaydıdır.

                ## Ahşap
                Masif ahşap, dokunulduğunda sıcaklık veren tek yapı malzemesi gibidir. Yağ bazlı bitişlerle zamanla daha da güzelleşir.

                ## Doğal taş
                Yerel taş, hem bölgenin iklimine en uygun malzemedir hem de taşıma maliyetini azaltır. Ege'deki projelerimizde inşaat sırasında çıkan taşı yeniden kullanmaya özen gösteriyoruz.

                Bir projede kullanılan malzeme sayısını azaltmak, mekânı sadeleştirdiği gibi uygulama ve bakım süreçlerini de kolaylaştırır.
                """,
        },
    ];
}
