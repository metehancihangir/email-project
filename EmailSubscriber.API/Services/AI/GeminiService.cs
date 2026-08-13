using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmailSubscriber.API.Services.AI;

public class GeminiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? "";
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

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={_apiKey}");
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseJson);
            
            // Gemini JSON format: { "candidates": [ { "content": { "parts": [ { "text": "..." } ] } } ] }
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
            _logger.LogError(ex, "Gemini isteği başarısız oldu.");
            throw;
        }
    }

    public async Task<string?> GetImageUrlForTopicAsync(string category, string topic)
    {
        // Gerçek ve konuyla alakalı bir fotoğraf (AI üretimi DEĞİL) bulmak için, 
        // Gemini'dan konuyu İngilizce 2 kelimelik stok fotoğraf arama etiketine (keyword) çevirmesini istiyoruz.
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = $"'{topic}' konulu bir e-posta bülteni için telifsiz stok fotoğraf arayacağım. Lütfen bana arama yapabileceğim en uygun ve kısa 2 İngilizce kelimeyi virgülle ayırarak ver. Sadece kelimeleri ver, başka hiçbir kelime veya noktalama işareti yazma." } } }
            }
        };

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={_apiKey}");
        requestMessage.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        string keywords = category.ToLower() switch
        {
            "mitoloji" => "mythology,ancient",
            "bilim" => "science,space",
            "finans" => "finance,money",
            _ => "abstract"
        };

        try
        {
            var response = await _httpClient.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);
                var aiKeywords = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString()?.Trim();

                if (!string.IsNullOrEmpty(aiKeywords))
                {
                    keywords = aiKeywords.Replace(" ", "").Replace("\n", "").ToLower();
                }
            }
        }
        catch (Exception ex)
        {
            // Hata olursa varsayılan (fallback) kelimeler kullanılır
            _logger.LogError(ex, "Görsel keyword'leri alınırken hata oluştu.");
        }
        
        // Gerçek fotoğraflar için LoremFlickr kullanıyoruz. 
        // Yapay zekanın bulduğu İngilizce kelimeleri (keywords) kullanarak rastgele ama tam konuyla eşleşen bir fotoğraf getiriyoruz.
        var url = $"https://loremflickr.com/800/400/{keywords}?lock={Random.Shared.Next(1, 10000)}";
        
        return url;
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
}
