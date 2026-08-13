using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmailSubscriber.API.Services.AI;

public class GeminiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _fallbackApiKey;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? "";
        _fallbackApiKey = configuration["Gemini:FallbackApiKey"] ?? "";
        _logger = logger;
    }

    public async Task<string> GenerateNewsletterAsync(string category, string pastTopics)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("Gemini API Key is missing. Returning fallback newsletter.");
            return $"<div><h2>{category} Hakkında Yeni Bir Keşif</h2><p>Bu hafta <b>{category}</b> dünyasında yaşanan gelişmeleri sizlerle paylaşıyoruz.</p><ul><li><b>Kaynak 1:</b> Örnek Kaynak</li></ul></div>";
        }

        string systemPrompt;
        
        switch (category.ToLower())
        {
            case "mitoloji":
                systemPrompt = $@"Sen uzman, akıcı ve sade bir dille yazan bir Mitoloji Bülteni yazarı/editörüsün. Görevin, karmaşık mitolojik hikayeleri, karakter ilişkilerini ve efsaneleri modern bir okuyucu için basitleştirerek anlatmaktır. 

ÖZEL TALİMATLAR:
1. Özellikle Yunan mitolojisi, savaş tanrıları (örn. Ares), rüzgar tanrıları veya doğa olaylarının mitolojik kökenleri gibi konuları işlerken karakterlerin soyağaçlarında kaybolma. Ana hikayeye ve sembolizme odaklan.
2. Vurgulama Kuralları: Önemli karakter adlarını, mitolojik eşyaları ve mekanları HTML <b> etiketi kullanarak kalın (bold) font ile yaz. KESİNLİKLE MARKDOWN (**) KULLANMA. Geri kalan metin normal olmalı.
3. Dilin akademik değil, hikaye anlatıcısı tadında ama net ve sade olmalı.
4. Kaynakçaları (URL'leri) KESİNLİKLE `<a href=""URL"">Kaynak Adı</a>` formatında tıklanabilir link yap.

Aşağıdaki şablonu KESİNLİKLE birebir uygula ve saf HTML (<div>, <p>, <h2> vb.) olarak ver, markdown (```html) KULLANMA:

<h2>[Mitolojik Konunun Çarpıcı Başlığı]</h2>

<p><b>Özet:</b> [Konuyu özetleyen iki cümle]</p>
<hr />
<h3>Efsanenin Özü</h3>
<p>[Mitoloik olayın veya karakterin sadeleştirilmiş ana hikayesi]</p>

<h3>Öne Çıkan Figürler ve Semboller</h3>
<ul>
<li><b>[Karakter/Obje Adı]:</b> [Rolü ve anlamı]</li>
<li><b>[Karakter/Obje Adı]:</b> [Rolü ve anlamı]</li>
</ul>

<h3>Bunları Biliyor muydunuz?</h3>
<p>[İşlenen mitolojik figürün günümüzdeki bir yansıması veya az bilinen bir detayı]</p>
<hr />
<h3>Kaynakça</h3>
<ul>
<li><a href=""[Gerçek Kaynak Linki 1]"">[Faydalanılan Kaynak/Yazar Adı]</a></li>
<li><a href=""[Gerçek Kaynak Linki 2]"">[Faydalanılan Kaynak/Yazar Adı]</a></li>
</ul>";
                break;

            case "finans":
                var newsContext = await GetLatestFinanceNewsAsync();

                systemPrompt = $@"Sen uzman, objektif ve sade bir dille yazan bir Finans ve Piyasa Bülteni editörüsün. Görevin, borsa endeksleri, hisse senedi hareketleri ve makroekonomik verileri okuyucuyu yormadan, hap bilgiler şeklinde sunmaktır. Asla yatırım tavsiyesi verme.

Aşağıda bugün piyasalarda olan en güncel ve gerçek haberler verilmiştir. BÜLTENİ KESİNLİKLE BU HABERLERİ BAZ ALARAK VE YORUMLAYARAK YAZ:

{newsContext}

ÖZEL TALİMATLAR:
1. BIST, Nasdaq gibi endekslerdeki hareketleri, kısa vadeli piyasa dinamiklerini veya belirli sektörlerdeki güncel trendleri yorumlarken finansal jargonu minimumda tut. 
2. Vurgulama Kuralları: Yalnızca hisse/şirket kodlarını, endeks isimlerini, yüzdelik değişimleri (örn. %5) ve kilit tarihleri HTML <b> etiketi kullanarak kalın (bold) font ile yaz. KESİNLİKLE MARKDOWN (**) KULLANMA.
3. Yorumların kısa, veriye dayalı ve rasyonel olmalı. Duygusal veya yönlendirici kelimeler (örn. ""uçuşa geçti"", ""çakıldı"") kullanma.
4. Kaynakçaları (URL'leri) KESİNLİKLE `<a href=""URL"">Kaynak Adı</a>` formatında tıklanabilir link yap. Verilen güncel haberlerin linklerini KESİNLİKLE kaynakça bölümünde kullan.

Aşağıdaki şablonu KESİNLİKLE birebir uygula ve saf HTML (<div>, <p>, <h2> vb.) olarak ver, markdown (```html) KULLANMA:

<h2>[Piyasalarda Günün/Haftanın Özeti Başlığı]</h2>

<p><b>Özet:</b> [Piyasanın genel yönünü özetleyen iki cümle]</p>
<hr />
<h3>Piyasa Dinamikleri</h3>
<p>[Günün veya haftanın en çok etkili olan ekonomik olayı veya piyasa hareketinin sade analizi]</p>

<h3>Odaktaki Hareketler</h3>
<ul>
<li><b>[Hisse Kodu / Endeks]:</b> [Değişim oranı ve kısaca nedeni]</li>
<li><b>[Şirket / Sektör Adı]:</b> [Öne çıkan gelişme]</li>
</ul>

<h3>Bunları Biliyor muydunuz?</h3>
<p>[Finansal bir terimin günlük hayattaki karşılığı veya tarihsel bir piyasa istatistiği]</p>
<hr />
<h3>Kaynakça</h3>
<ul>
<li><a href=""[Gerçek Kaynak Linki 1]"">[Haberin alındığı platform/kurum]</a></li>
<li><a href=""[Gerçek Kaynak Linki 2]"">[İkinci Kaynak/Rapor]</a></li>
</ul>";
                break;

            case "bilim":
                systemPrompt = $@"Sen uzman, yenilikçi ve sade bir dille yazan bir Bilim ve Teknoloji Bülteni editörüsün. Görevin, yeni bilimsel keşifleri, yazılım dünyasındaki gelişmeleri veya mühendislik başarılarını herkesin anlayabileceği bir netlikte açıklamaktır.

ÖZEL TALİMATLAR:
1. Karmaşık algoritmaları, yeni nesil teknolojileri (yapay zeka, yazılım mimarileri vb.) veya temel bilimsel keşifleri anlatırken teknik boğuculuktan kaçın. Gerekirse günlük hayattan sade analojiler kullan.
2. Vurgulama Kuralları: Yalnızca bilimsel terimleri, teknoloji/yazılım adlarını, keşfi yapan kurum/kişileri ve yılları HTML <b> etiketi kullanarak kalın (bold) font ile yaz. KESİNLİKLE MARKDOWN (**) KULLANMA.
3. Gelişmenin ""nasıl"" olduğundan çok ""neden önemli"" olduğuna ve gelecekte neyi değiştireceğine odaklan.
4. Kaynakçaları (URL'leri) KESİNLİKLE `<a href=""URL"">Kaynak Adı</a>` formatında tıklanabilir link yap.

Aşağıdaki şablonu KESİNLİKLE birebir uygula ve saf HTML (<div>, <p>, <h2> vb.) olarak ver, markdown (```html) KULLANMA:

<h2>[Keşfin veya Gelişmenin Dikkat Çekici Başlığı]</h2>

<p><b>Özet:</b> [Bu gelişmenin ne olduğunu anlatan iki cümle]</p>
<hr />
<h3>Keşfin / Gelişmenin Detayları</h3>
<p>[Bilimsel veya teknolojik gelişmenin teknik jargon kullanılmadan, net ve sade anlatımı]</p>

<h3>Neden Önemli?</h3>
<ul>
<li><b>[Etkilenecek Alan/Sektör]:</b> [Bu gelişmenin o alana etkisi]</li>
<li><b>[Kilit İnovasyon]:</b> [Öne çıkan teknik fark]</li>
</ul>

<h3>Bunları Biliyor muydunuz?</h3>
<p>[Bu keşfin temelini atan eski bir bilgi veya ilginç bir bilimsel gerçek]</p>
<hr />
<h3>Kaynakça</h3>
<ul>
<li><a href=""[Gerçek Kaynak Linki 1]"">[Keşfin yayınlandığı makale/dergi]</a></li>
<li><a href=""[Gerçek Kaynak Linki 2]"">[Faydalanılan bilimsel kaynak]</a></li>
</ul>";
                break;

            case "politika":
                var politicsContext = await GetLatestPoliticsNewsAsync();
                systemPrompt = $@"Sen uzman, tamamen tarafsız ve sade bir dille yazan bir Politika ve Dış İlişkiler Bülteni editörüsün. Görevin, iç ve dış siyasetteki gelişmeleri, yeni yasaları, diplomatik ilişkileri ve jeopolitik olayları okuyucuya spekülasyondan uzak, net ve anlaşılır bir şekilde sunmaktır.

Aşağıda bugün dünyada ve Türkiye'de olan en güncel ve güvenilir haberler verilmiştir. BÜLTENİ KESİNLİKLE BU HABERLERİ BAZ ALARAK VE YORUMLAYARAK YAZ:

{politicsContext}

ÖZEL TALİMATLAR:
1. Kesinlikle objektif kal. Siyasi kişi, kurum veya ideolojileri överken ya da yererken duygusal, yönlendirici veya sansasyonel kelimeler kullanma. Yalnızca resmi açıklamalara, onaylanmış olaylara ve eylemlerin sonuçlarına odaklan. Bürokratik jargonu basitleştir.
2. Vurgulama Kuralları: Yalnızca <b>siyasi figürlerin/liderlerin adlarını</b>, <b>kurum/örgüt/parti isimlerini</b>, <b>yasa/antlaşma adlarını</b> ve <b>önemli tarihleri</b> HTML <b> etiketi ile kalın (bold) font ile yaz. KESİNLİKLE MARKDOWN (**) KULLANMA. Bütün bir cümleyi asla kalınlaştırma.
3. Haberi bir ""kriz"" veya ""zafer"" diliyle değil; durum tespiti yapan, arka planı aydınlatan saygın bir haber ajansı tonunda kaleme al.
4. Kaynakçaları (URL'leri) KESİNLİKLE `<a href=""URL"">Kaynak Adı</a>` formatında tıklanabilir link yap.

Aşağıdaki şablonu KESİNLİKLE birebir uygula ve saf HTML (<div>, <p>, <h2> vb.) olarak ver, markdown (```html) KULLANMA:

<h2>[Siyasi Gelişmenin Çarpıcı ve Tarafsız Başlığı]</h2>

<p><b>Özet:</b> [Siyasi olayın, kararın veya diplomatik görüşmenin ne olduğunu özetleyen, yorum içermeyen en fazla iki cümle]</p>
<hr />
<h3>Gündemin Odak Noktası</h3>
<p>[Siyasi gelişmenin, meclis kararı veya uluslararası olayın okuyucuyu yormayan, tarafsız ve sade anlatımı. Olayın arka planı ve ne anlama geldiği burada açıklanmalı.]</p>

<h3>Öne Çıkan Detaylar</h3>
<ul>
<li><b>[Siyasi Figür / Kurum Adı]:</b> [Bu aktörün olaydaki rolü veya yaptığı kilit açıklama]</li>
<li><b>[Yasa Tasarısı / Antlaşma / Kavram]:</b> [Alınan kararın veya tartışılan konunun getirdiği en büyük değişiklik]</li>
</ul>

<h3>Bunları Biliyor muydunuz?</h3>
<p>[İşlenen siyasi konunun, antlaşmanın veya diplomatik krizin geçmişteki bir örneği veya bu politik kavramın kökenine dair kısa, ufuk açıcı bir bilgi]</p>
<hr />
<h3>Kaynakça</h3>
<ul>
<li><a href=""[Gerçek Kaynak Linki 1]"">[Açıklamanın yapıldığı resmi kurum, saygın haber ajansı veya rapor]</a></li>
<li><a href=""[Gerçek Kaynak Linki 2]"">[İkinci kaynak]</a></li>
</ul>";
                break;

            default:
                systemPrompt = $@"Sen SUBMAIL için çalışan profesyonel bir içerik üreticisisin. 
Kategori: {category}
Görevin: Bu kategori hakkında daha önce anlatmadığın, çok ilginç ve okuyucuyu bağlayan bir konu seç. Çıktıyı HTML olarak ver.";
                break;
        }

        var fullInstruction = systemPrompt + $"\n\nUYARI: Daha önce işlenen şu konulardan KESİNLİKLE UZAK DUR: {pastTopics}\nLütfen yeni bülteni hemen yukarıdaki şablona göre HTML formatında oluştur.";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = fullInstruction }
                    }
                }
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);

        int maxRetries = 3;
        string currentApiKey = _apiKey;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={currentApiKey}";
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(requestMessage);
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && !string.IsNullOrEmpty(_fallbackApiKey) && currentApiKey != _fallbackApiKey)
                {
                    _logger.LogWarning("Gemini API kota aşımı (429 Too Many Requests). Fallback API anahtarına geçiliyor...");
                    currentApiKey = _fallbackApiKey;
                    attempt--; // Bu denemeyi sayma, yeni anahtarla tekrar dene
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseJson);
                
                var content = jsonDoc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return content?.Replace("```html", "").Replace("```", "").Trim() ?? "";
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries)
                {
                    _logger.LogError(ex, "Gemini isteği başarısız oldu. Maksimum deneme sayısına ulaşıldı.");
                    throw;
                }
                _logger.LogWarning(ex, $"Gemini isteği başarısız oldu (Deneme {attempt}/{maxRetries}). 3 saniye sonra tekrar deneniyor...");
                await Task.Delay(3000);
            }
        }
        
        return string.Empty;
    }

    public async Task<string?> GetImageUrlForTopicAsync(string category, string topic, string htmlContent = "")
    {
        // 1. Önce htmlContent içindeki linkleri (href) bulup og:image çekmeyi deneyelim.
        if (!string.IsNullOrEmpty(htmlContent))
        {
            var hrefMatches = Regex.Matches(htmlContent, @"href=""(http[s]?://[^""]+)""");
            foreach (Match match in hrefMatches.Take(3)) // İlk 3 linke bakalım (hız için)
            {
                var link = match.Groups[1].Value;
                try
                {
                    // Kısa bir timeout ile linke istek atalım
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    var html = await _httpClient.GetStringAsync(link, cts.Token);
                    
                    var ogImageMatch = Regex.Match(html, @"<meta\s+(?:[^>]*?content=""([^""]+)""[^>]*?(?:property|name)=""og:image""|[^>]*?(?:property|name)=""og:image""[^>]*?content=""([^""]+)"")[^>]*?>", RegexOptions.IgnoreCase);
                    if (ogImageMatch.Success)
                    {
                        var imageUrl = !string.IsNullOrEmpty(ogImageMatch.Groups[1].Value) ? ogImageMatch.Groups[1].Value : ogImageMatch.Groups[2].Value;
                        _logger.LogInformation("og:image bulundu: {ImageUrl}", imageUrl);
                        return imageUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Linkten og:image alınamadı: {Link}", link);
                }
            }
        }

        // 2. Eğer og:image bulunamazsa, AI ile İngilizce keyword üret ve Unsplash Random endpoint'ini kullan.
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = $"Bu metni ('{topic}' konusu ve '{category}' kategorisi) okuyup, arka planda görsel aramak için kullanılacak en fazla 3 kelimelik, somut bir İngilizce arama terimi üret. Sadece terimi yaz, aralarına virgül koy. (Örn: stock market red, space star galaxy)" } } }
            }
        };

        string keywords = category.ToLower() switch
        {
            "mitoloji" => "mythology,ancient",
            "bilim" => "science,space",
            "finans" => "finance,money",
            "politika" => "politics,government",
            _ => "abstract"
        };

        int maxRetries = 3;
        string currentApiKey = _apiKey;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={currentApiKey}";
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && !string.IsNullOrEmpty(_fallbackApiKey) && currentApiKey != _fallbackApiKey)
                {
                    _logger.LogWarning("Gemini API kota aşımı (429 Too Many Requests) keyword araması için. Fallback API anahtarına geçiliyor...");
                    currentApiKey = _fallbackApiKey;
                    attempt--;
                    continue;
                }

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(responseJson);
                    
                    var aiKeywords = jsonDoc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString()?.Trim();

                    if (!string.IsNullOrEmpty(aiKeywords))
                    {
                        keywords = aiKeywords.Replace(" ", "").Replace("\n", "").ToLower();
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Görsel keyword'leri alınırken hata oluştu. (Deneme {attempt}/{maxRetries})");
                if (attempt < maxRetries) await Task.Delay(2000);
            }
        }
        
        _logger.LogInformation("Görsel için kullanılacak kelimeler: {Keywords}", keywords);
        // images.unsplash.com/random endpointi yönlendirme (redirect) yaptığı için Gmail gibi bazı e-posta istemcilerinde resmi kırık gösterebilir.
        // Bu yüzden stabil olan ve rastgele stok fotoğraf döndüren alternatif bir servisi kullanıyoruz.
        return $"https://loremflickr.com/800/400/{keywords.Replace(",", ",")}/all";
    }

    private async Task<string> GetLatestFinanceNewsAsync()
    {
        try
        {
            var rssUrl = "https://www.bloomberght.com/rss";
            var response = await _httpClient.GetStringAsync(rssUrl);
            var doc = XDocument.Parse(response);
            
            var items = doc.Descendants("item").Take(3);
            var sb = new StringBuilder();
            sb.AppendLine("GÜNCEL EKONOMİ HABERLERİ:");
            foreach (var item in items)
            {
                var title = item.Element("title")?.Value;
                var description = item.Element("description")?.Value;
                var link = item.Element("link")?.Value;
                sb.AppendLine($"- Başlık: {title}");
                sb.AppendLine($"  Özet: {description}");
                sb.AppendLine($"  Link: {link}");
                sb.AppendLine();
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RSS feed çekilemedi, varsayılan finans istemiyle devam edilecek.");
            return string.Empty;
        }
    }

    private async Task<string> GetLatestPoliticsNewsAsync()
    {
        try
        {
            var rssUrls = new[] 
            { 
                "https://feeds.bbci.co.uk/turkce/rss.xml", 
                "https://feeds.bbci.co.uk/turkce/dunya/rss.xml" 
            };
            
            var feedText = new System.Text.StringBuilder();
            feedText.AppendLine("GÜNCEL POLİTİKA VE DÜNYA HABERLERİ:");

            foreach (var rssUrl in rssUrls)
            {
                var response = await _httpClient.GetStringAsync(rssUrl);
                var doc = XDocument.Parse(response);

                var items = doc.Descendants("item").Take(4);
                foreach (var item in items)
                {
                    var title = item.Element("title")?.Value;
                    var description = item.Element("description")?.Value;
                    var link = item.Element("link")?.Value;

                    if (!string.IsNullOrEmpty(title))
                    {
                        feedText.AppendLine($"- Başlık: {title}");
                        feedText.AppendLine($"  Özet: {description}");
                        feedText.AppendLine($"  Link: {link}");
                        feedText.AppendLine();
                    }
                }
            }

            return feedText.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Politika RSS feed çekilemedi, varsayılan politika istemiyle devam edilecek.");
            return string.Empty;
        }
    }
}
