using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EmailSubscriber.API.DTOs;
using HtmlAgilityPack;

namespace EmailSubscriber.API.Services.AI;

public class GeminiService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private readonly string _fallbackApiKey;
    private readonly ILogger<GeminiService> _logger;
    private static readonly HttpClient _safeImageHttpClient = CreateSafeImageHttpClient();

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }
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
        
        switch (category.ToLowerInvariant())
        {
            case "mitoloji":
                systemPrompt = $@"Sen uzman, akıcı ve sade bir dille yazan bir Mitoloji Bülteni yazarı/editörüsün. Görevin, karmaşık mitolojik hikayeleri, karakter ilişkilerini ve efsaneleri modern bir okuyucu için basitleştirerek anlatmaktır. 

ÖZEL TALİMATLAR:
1. Özellikle Yunan, Roma, İskandinav veya Mısır mitolojisi işlerken karakterlerin soyağaçlarında kaybolma. Ana hikayeye, sembolizme ve ana karaktere odaklan.
2. Vurgulama Kuralları: Önemli karakter adlarını, mitolojik eşyaları ve mekanları HTML <b> etiketi kullanarak kalın (bold) font ile yaz. KESİNLİKLE MARKDOWN (**) KULLANMA. Geri kalan metin normal olmalı.
3. Dilin akademik değil, hikaye anlatıcısı tadında ama net ve sade olmalı.
4. Kaynakça (URL) verirken KESİNLİKLE uydurma (hallucinated) linkler kullanma. Sadece gerçekliğinden emin olduğun, konuyla ilgili Wikipedia sayfalarının linklerini (örn: https://tr.wikipedia.org/wiki/Zeus) kullan ve `<a href=""URL"">Kaynak Adı</a>` formatında tıklanabilir link yap.

Aşağıdaki şablonu KESİNLİKLE birebir uygula ve saf HTML (<div>, <p>, <h2> vb.) olarak ver, markdown (```html) KULLANMA:

<h2>[Mitolojik Konunun Çarpıcı Başlığı]</h2>

<p><b>Özet:</b> [Konuyu özetleyen iki cümle]</p>
<hr />
<h3>Efsanenin Özü</h3>
<p>[Mitolojik olayın veya karakterin sadeleştirilmiş ana hikayesi]</p>

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
<li><a href=""[Gerçek Wikipedia Linki 1]"">[Wikipedia Makale Adı]</a></li>
<li><a href=""[Gerçek Wikipedia Linki 2]"">[Wikipedia Makale Adı]</a></li>
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
                var scienceContext = await GetLatestScienceNewsAsync();
                systemPrompt = $@"Sen uzman, yenilikçi ve sade bir dille yazan bir Bilim ve Teknoloji Bülteni editörüsün. Görevin, yeni bilimsel keşifleri, yazılım dünyasındaki gelişmeleri veya mühendislik başarılarını herkesin anlayabileceği bir netlikte açıklamaktır.

Aşağıda bugün bilim ve teknoloji dünyasında olan en güncel haberler verilmiştir. BÜLTENİ KESİNLİKLE BU HABERLERİ BAZ ALARAK VE YORUMLAYARAK YAZ:

{scienceContext}

ÖZEL TALİMATLAR:
1. Karmaşık algoritmaları, yeni nesil teknolojileri (yapay zeka, yazılım mimarileri vb.) veya temel bilimsel keşifleri anlatırken teknik boğuculuktan kaçın. Gerekirse günlük hayattan sade analojiler kullan.
2. Vurgulama Kuralları: Yalnızca bilimsel terimleri, teknoloji/yazılım adlarını, keşfi yapan kurum/kişileri ve yılları HTML <b> etiketi kullanarak kalın (bold) font ile yaz. KESİNLİKLE MARKDOWN (**) KULLANMA.
3. Gelişmenin ""nasıl"" olduğundan çok ""neden önemli"" olduğuna ve gelecekte neyi değiştireceğine odaklan.
4. Kaynakçaları (URL'leri) KESİNLİKLE `<a href=""URL"">Kaynak Adı</a>` formatında tıklanabilir link yap. Verilen güncel haberlerin linklerini KESİNLİKLE kaynakça bölümünde kullan ve uydurma (hallucinated) linkler KULLANMA.

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
<li><a href=""[Gerçek Kaynak Linki 1]"">[Haberin alındığı platform/kurum]</a></li>
<li><a href=""[Gerçek Kaynak Linki 2]"">[İkinci Kaynak/Rapor]</a></li>
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

        try
        {
            var generated = await GenerateContentAsync(fullInstruction);
            if (!string.IsNullOrWhiteSpace(generated))
                return generated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini içerik üretimi beklenmeyen bir hata ile sonlandı.");
        }

        _logger.LogWarning("Gemini içeriği üretilemedi, yedek bülten şablonu kullanılıyor.");
        return $"<div><h2>{category} Hakkında Yeni Bir Keşif</h2><p>Bu hafta <b>{category}</b> dünyasında yaşanan gelişmeleri sizlerle paylaşıyoruz.</p><ul><li><b>Kaynak 1:</b> Örnek Kaynak</li></ul></div>";
    }

    private IEnumerable<string> GetModelCandidates()
    {
        var models = new List<string>();

        var primary = _configuration["Gemini:Model"];
        if (!string.IsNullOrWhiteSpace(primary))
            models.Add(primary.Trim());

        var configuredFallbacks = _configuration.GetSection("Gemini:FallbackModels").Get<string[]>();
        if (configuredFallbacks != null)
        {
            foreach (var fallback in configuredFallbacks)
            {
                if (!string.IsNullOrWhiteSpace(fallback))
                    models.Add(fallback.Trim());
            }
        }

        foreach (var fallback in new[] { "gemini-2.5-flash", "gemini-3.5-flash", "gemini-2.5-flash-lite", "gemini-3.6-flash" })
        {
            if (!models.Contains(fallback, StringComparer.OrdinalIgnoreCase))
                models.Add(fallback);
        }

        return models;
    }

    private static object BuildGenerationConfig(string model)
    {
        // thinkingBudget bazı modellerde 400/404'e neden olabiliyor; 
        // basit config kullan.
        return new
        {
            temperature = 0.7,
            maxOutputTokens = 8192
        };
    }

    private static string? ExtractGeminiErrorMessage(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch
        {
            // ignore parse errors
        }

        return null;
    }

    private async Task<string?> GenerateContentAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey) && string.IsNullOrEmpty(_fallbackApiKey))
            return null;

        var apiKeys = new List<string>();
        if (!string.IsNullOrEmpty(_apiKey))
            apiKeys.Add(_apiKey);
        if (!string.IsNullOrEmpty(_fallbackApiKey) && !apiKeys.Contains(_fallbackApiKey))
            apiKeys.Add(_fallbackApiKey);

        foreach (var model in GetModelCandidates())
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = BuildGenerationConfig(model)
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);

            foreach (var apiKey in apiKeys)
            {
                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    try
                    {
                        var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                        {
                            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                        };
                        requestMessage.Headers.Add("x-goog-api-key", apiKey);

                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                        var response = await _httpClient.SendAsync(requestMessage, cts.Token);
                        var responseJson = await response.Content.ReadAsStringAsync(cts.Token);

                        if (response.IsSuccessStatusCode)
                        {
                            using var jsonDoc = JsonDocument.Parse(responseJson);
                            var content = ExtractGeneratedText(jsonDoc);
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                _logger.LogInformation("Gemini içerik üretildi. Model={Model}", model);
                                return content.Replace("```html", "").Replace("```", "").Trim();
                            }

                            _logger.LogWarning("Gemini yanıtında metin bulunamadı. Model={Model}", model);
                            break;
                        }

                        var apiError = ExtractGeminiErrorMessage(responseJson) ?? response.ReasonPhrase;
                        _logger.LogWarning(
                            "Gemini isteği başarısız ({Status}) model={Model} deneme={Attempt}. {Error}",
                            (int)response.StatusCode, model, attempt, apiError);

                        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
                            break;

                        if (response.StatusCode == HttpStatusCode.TooManyRequests)
                            break;

                        if ((int)response.StatusCode >= 500 && attempt < 2)
                        {
                            await Task.Delay(1000 * attempt);
                            continue;
                        }

                        break;
                    }
                    catch (TaskCanceledException ex)
                    {
                        _logger.LogWarning(ex, "Gemini isteği zaman aşımına uğradı. Model={Model}", model);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Gemini isteği başarısız oldu. Model={Model}, deneme={Attempt}", model, attempt);
                        if (attempt < 2)
                            await Task.Delay(1000 * attempt);
                    }
                }
            }
        }

        return null;
    }

    private static string? ExtractGeneratedText(JsonDocument jsonDoc)
    {
        if (!jsonDoc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            return null;

        var first = candidates[0];
        if (!first.TryGetProperty("content", out var content) || !content.TryGetProperty("parts", out var parts))
            return null;

        var sb = new StringBuilder();
        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True)
                continue;

            if (part.TryGetProperty("text", out var textEl))
                sb.Append(textEl.GetString());
        }

        var text = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    public async Task<AIDraftResult> GenerateNewsletterDraftAsync(string category, string? pastTopics = null)
    {
        try
        {
            var rawHtml = await GenerateNewsletterAsync(category, pastTopics ?? "Yok");

            // Güvenlik: Gemini bazen HTML yerine Markdown (**) kullanır.
            var htmlContent = Regex.Replace(rawHtml, @"\*\*(.*?)\*\*", "<b>$1</b>");

            // XSS Koruması
            var sanitizer = new Ganss.Xss.HtmlSanitizer();
            htmlContent = sanitizer.Sanitize(htmlContent);

            // Başlık yakalama
            string extractedTopic = $"{category} Bülteni";
            var match = Regex.Match(htmlContent, @"<h2>(.*?)</h2>", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                extractedTopic = match.Groups[1].Value;
                extractedTopic = Regex.Replace(extractedTopic, "<.*?>", string.Empty).Trim();
            }

            string categoryEmoji = category.ToLowerInvariant() switch
            {
                "mitoloji" => "🏛️",
                "finans" => "📈",
                "bilim" => "🔬",
                "politika" => "🌍",
                _ => "✨"
            };

            string subject = $"SUBMAIL {category} {categoryEmoji}: {extractedTopic} 🌟";

            // Kaynakça linklerini topla
            var refUrls = new List<string>();
            var linkMatches = Regex.Matches(htmlContent, @"href=[""'](https?://[^""']+)[""']", RegexOptions.IgnoreCase);
            foreach (Match m in linkMatches)
            {
                if (m.Success && !refUrls.Contains(m.Groups[1].Value))
                {
                    refUrls.Add(m.Groups[1].Value);
                }
            }

            // Akıllı Görsel Belirleme (Wikipedia veya Kaynakça OpenGraph Scraper)
            string? coverImageUrl = await GetImageUrlForTopicAsync(category, extractedTopic, htmlContent);

            return new AIDraftResult
            {
                Category = category,
                Topic = extractedTopic,
                Subject = subject,
                HtmlBody = htmlContent,
                CoverImageUrl = coverImageUrl,
                ReferenceUrls = refUrls
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Taslak üretimi sırasında beklenmeyen hata oluştu. Yedek şablon döndürülüyor.");

            var fallbackHtml = $"<div><h2>{category} Bülteni</h2><p>Bu hafta <b>{category}</b> dünyasından seçilmiş bir konuyu sizlerle paylaşıyoruz.</p></div>";
            return new AIDraftResult
            {
                Category = category,
                Topic = $"{category} Bülteni",
                Subject = $"SUBMAIL {category}: {category} Bülteni",
                HtmlBody = fallbackHtml,
                CoverImageUrl = await GetImageUrlForTopicAsync(category, $"{category} Bülteni", fallbackHtml),
                ReferenceUrls = new List<string>()
            };
        }
    }

    public async Task<string?> GetImageUrlForTopicAsync(string category, string topic, string htmlContent = "")
    {
        var normalizedCategory = category.ToLowerInvariant().Trim();

        try
        {
            if (normalizedCategory == "mitoloji")
            {
                var wikiImage = await GetMythologyImageFromWikipediaAsync(topic, htmlContent);
                if (!string.IsNullOrEmpty(wikiImage))
                {
                    _logger.LogInformation("Mitoloji için Wikipedia'dan görsel başarıyla alındı: {Url}", wikiImage);
                    return wikiImage;
                }
            }
            else
            {
                // Bilim, Finans, Politika için kaynakça bağlantılarından OpenGraph / twitter:image kazı
                var scrapedImage = await GetArticleImageFromMetadataAsync(htmlContent, topic);
                if (!string.IsNullOrEmpty(scrapedImage))
                {
                    _logger.LogInformation("{Category} için kaynakça bağlantısından görsel başarıyla çekildi: {Url}", category, scrapedImage);
                    return scrapedImage;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Category} için dinamik görsel çekilirken hata oluştu, varsayılana geçiliyor.", category);
        }

        // Yedek Görseller (Güvenilir Yüksek Çözünürlüklü Fallback)
        var textToSearch = $"{topic} {htmlContent}".ToLowerInvariant();
        string fallbackUrl = normalizedCategory switch
        {
            "mitoloji" => GetMythologyFallbackImage(textToSearch),
            "finans" => GetFinanceFallbackImage(textToSearch),
            "bilim" => GetScienceFallbackImage(textToSearch),
            "politika" => GetPoliticsFallbackImage(textToSearch),
            _ => "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?q=80&w=800&auto=format&fit=crop"
        };

        _logger.LogInformation("{Category} için güvenli yedek görsel seçildi: {Url}", category, fallbackUrl);
        return fallbackUrl;
    }

    /// <summary>
    /// Mitoloji bültenleri için Wikipedia REST API kullanarak konuya/karaktere ait orijinal görseli çeker.
    /// </summary>
    private async Task<string?> GetMythologyImageFromWikipediaAsync(string topic, string htmlContent)
    {
        // 1. Önce HTML içerisindeki Wikipedia linklerini tara (örn: https://tr.wikipedia.org/wiki/Zeus)
        var wikiLinkMatches = Regex.Matches(htmlContent, @"https?://(tr|en)\.wikipedia\.org/wiki/([^""'#\s>]+)", RegexOptions.IgnoreCase);
        var candidateTitles = new List<(string Lang, string Title)>();

        foreach (Match match in wikiLinkMatches)
        {
            if (match.Success)
            {
                var lang = match.Groups[1].Value.ToLowerInvariant();
                var pageTitle = Uri.UnescapeDataString(match.Groups[2].Value);
                if (!string.IsNullOrWhiteSpace(pageTitle) && !candidateTitles.Any(c => c.Title.Equals(pageTitle, StringComparison.OrdinalIgnoreCase)))
                {
                    candidateTitles.Add((lang, pageTitle));
                }
            }
        }

        // 2. Link yoksa başlıktan veya konudan figür/karakter adını türet
        if (!candidateTitles.Any())
        {
            var cleanedTopic = Regex.Replace(topic, @"(Efsanesi|Miti|Hikayesi|Hakkında|ve|ile|Tanrısı|Tanrıçası)", "", RegexOptions.IgnoreCase).Trim();
            var words = cleanedTopic.Split(new[] { ' ', ':', '-', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0)
            {
                candidateTitles.Add(("tr", words[0]));
                candidateTitles.Add(("en", words[0]));
            }
        }

        // 3. Wikipedia Summary API'sine istek at
        foreach (var (lang, pageTitle) in candidateTitles)
        {
            try
            {
                var apiUrl = $"https://{lang}.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(pageTitle)}";
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Add("User-Agent", "SubmailBot/1.0 (https://submail.app; contact@submail.app)");

                var response = await _safeImageHttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonStr);
                    var root = doc.RootElement;

                    // Orijinal veya thumbnail görseli kontrol et
                    if (root.TryGetProperty("originalimage", out var originalImage) && originalImage.TryGetProperty("source", out var origSrc))
                    {
                        var src = origSrc.GetString();
                        if (!string.IsNullOrEmpty(src) && await IsSafePublicUrlAsync(src))
                        {
                            return src;
                        }
                    }

                    if (root.TryGetProperty("thumbnail", out var thumbnail) && thumbnail.TryGetProperty("source", out var thumbSrc))
                    {
                        var src = thumbSrc.GetString();
                        if (!string.IsNullOrEmpty(src) && await IsSafePublicUrlAsync(src))
                        {
                            return src;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Wikipedia API isteği başarısız: {Lang} - {Title}", lang, pageTitle);
            }
        }

        return null;
    }

    /// <summary>
    /// Bilim, Finans ve Politika bültenlerinde kaynakça bağlantılarından OpenGraph (og:image / twitter:image) etiketlerini kazır.
    /// </summary>
    private async Task<string?> GetArticleImageFromMetadataAsync(string htmlContent, string topic)
    {
        var linkMatches = Regex.Matches(htmlContent, @"href=[""'](https?://[^""']+)[""']", RegexOptions.IgnoreCase);
        var urlsToCheck = new List<string>();

        foreach (Match match in linkMatches)
        {
            if (match.Success)
            {
                var url = match.Groups[1].Value;
                // Wikipedia ve arama motorları dışındaki haber/kaynak sitelerini listele
                if (!url.Contains("wikipedia.org") && !url.Contains("google.com") && !urlsToCheck.Contains(url))
                {
                    urlsToCheck.Add(url);
                }
            }
        }

        foreach (var url in urlsToCheck.Take(3)) // İlk 3 kaynağı tara
        {
            if (!await IsSafePublicUrlAsync(url))
                continue;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await _safeImageHttpClient.SendAsync(request, cts.Token);
                
                if (!response.IsSuccessStatusCode)
                    continue;

                var pageHtml = await response.Content.ReadAsStringAsync(cts.Token);
                var doc = new HtmlDocument();
                doc.LoadHtml(pageHtml);

                var ogImageNode = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image' or @name='og:image' or @property='og:image:url']") 
                               ?? doc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image' or @name='twitter:image:src']")
                               ?? doc.DocumentNode.SelectSingleNode("//link[@rel='image_src']");

                if (ogImageNode != null)
                {
                    string imageUrl = ogImageNode.GetAttributeValue("content", string.Empty);
                    if (string.IsNullOrWhiteSpace(imageUrl))
                    {
                        imageUrl = ogImageNode.GetAttributeValue("href", string.Empty);
                    }

                    if (!string.IsNullOrWhiteSpace(imageUrl))
                    {
                        imageUrl = imageUrl.Trim();
                        // Göreceli URL ise tam URL'ye çevir
                        if (imageUrl.StartsWith("/") && Uri.TryCreate(new Uri(url), imageUrl, out var combinedUri))
                        {
                            imageUrl = combinedUri.ToString();
                        }

                        if (await IsSafePublicUrlAsync(imageUrl))
                        {
                            return imageUrl;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Kaynakça bağlantısından OpenGraph görseli çekilemedi: {Url}. Hata: {Message}", url, ex.Message);
            }
        }

        return null;
    }

    private static string GetMythologyFallbackImage(string text)
    {
        if (text.Contains("zeus") || text.Contains("jüpiter") || text.Contains("olimpos") || text.Contains("şimşek"))
            return "https://images.unsplash.com/photo-1564507592333-c60657eea523?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("poseidon") || text.Contains("neptün") || text.Contains("deniz") || text.Contains("okyanus"))
            return "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("athena") || text.Contains("akropolis") || text.Contains("parthenon") || text.Contains("truva"))
            return "https://images.unsplash.com/photo-1555993539-1732b0258235?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("apollon") || text.Contains("sanat") || text.Contains("müzik") || text.Contains("afrodit"))
            return "https://images.unsplash.com/photo-1576014131341-fe1486fb2475?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("ikarus") || text.Contains("güneş") || text.Contains("kanat") || text.Contains("daidalos"))
            return "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?q=80&w=800&auto=format&fit=crop";

        return "https://images.unsplash.com/photo-1579783900882-c0d3dad7b119?q=80&w=800&auto=format&fit=crop";
    }

    private static string GetFinanceFallbackImage(string text)
    {
        if (text.Contains("kripto") || text.Contains("bitcoin") || text.Contains("blockchain") || text.Contains("btc"))
            return "https://images.unsplash.com/photo-1518770660439-4636190af475?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("altın") || text.Contains("emtia") || text.Contains("petrol") || text.Contains("gümüş"))
            return "https://images.unsplash.com/photo-1610375461246-83df859d849d?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("enflasyon") || text.Contains("faiz") || text.Contains("merkez bankası") || text.Contains("tcmb") || text.Contains("fed") || text.Contains("dolar"))
            return "https://images.unsplash.com/photo-1580519542036-c47de6196ba5?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("bist") || text.Contains("borsa") || text.Contains("hisse") || text.Contains("nasdaq") || text.Contains("endeks"))
            return "https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?q=80&w=800&auto=format&fit=crop";

        return "https://images.unsplash.com/photo-1590283603385-17ffb3a7f29f?q=80&w=800&auto=format&fit=crop";
    }

    private static string GetScienceFallbackImage(string text)
    {
        if (text.Contains("yapay zeka") || text.Contains("robot") || text.Contains("yazılım") || text.Contains("bilgisayar") || text.Contains("algoritma"))
            return "https://images.unsplash.com/photo-1620712943543-bcc4688e7485?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("kuantum") || text.Contains("fizik") || text.Contains("laboratuvar") || text.Contains("dna") || text.Contains("biyoloji") || text.Contains("tıp") || text.Contains("genetik"))
            return "https://images.unsplash.com/photo-1507413245164-6160d8298b31?q=80&w=800&auto=format&fit=crop";

        return "https://images.unsplash.com/photo-1451187580459-43490279c0fa?q=80&w=800&auto=format&fit=crop";
    }

    private static string GetPoliticsFallbackImage(string text)
    {
        if (text.Contains("meclis") || text.Contains("parlamento") || text.Contains("yasa") || text.Contains("kanun") || text.Contains("hükümet") || text.Contains("seçim"))
            return "https://images.unsplash.com/photo-1540910419892-4a36d2c3266c?q=80&w=800&auto=format&fit=crop";

        if (text.Contains("lider") || text.Contains("zirve") || text.Contains("başkan") || text.Contains("bakan") || text.Contains("açıklama") || text.Contains("basın"))
            return "https://images.unsplash.com/photo-1577962917302-cd874c4e31d2?q=80&w=800&auto=format&fit=crop";

        return "https://images.unsplash.com/photo-1541872703-74c5e44368f9?q=80&w=800&auto=format&fit=crop";
    }

    private static HttpClient CreateSafeImageHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = async (context, cancellationToken) =>
            {
                var host = context.DnsEndPoint.Host;
                var port = context.DnsEndPoint.Port;

                var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
                var safeAddress = addresses.FirstOrDefault(ip => IsSafeIpAddress(ip));
                if (safeAddress == null)
                {
                    throw new InvalidOperationException($"Güvenli olmayan veya iç ağ hedef IP tespit edildi: {host}");
                }

                var socket = new Socket(safeAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    await socket.ConnectAsync(safeAddress, port, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            },
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            AllowAutoRedirect = false
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(8)
        };
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return client;
    }

    public static bool IsSafeIpAddress(IPAddress ip)
    {
        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        if (IPAddress.IsLoopback(ip))
            return false;

        if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal)
            return false;

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();
            if (bytes[0] == 0) return false;
            if (bytes[0] == 10) return false;
            if (bytes[0] == 127) return false;
            if (bytes[0] == 169 && bytes[1] == 254) return false;
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false;
            if (bytes[0] == 192 && bytes[1] == 168) return false;
            if (bytes[0] >= 224 && bytes[0] <= 239) return false;
            if (bytes[0] >= 240) return false;
        }

        return true;
    }

    private static async Task<bool> IsSafePublicUrlAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var host = uri.DnsSafeHost.ToLowerInvariant();
        if (host == "localhost" || host.EndsWith(".localhost") || host == "127.0.0.1" || host == "::1" || host == "169.254.169.254")
            return false;

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host);
            if (addresses == null || addresses.Length == 0)
                return false;

            return addresses.Any(IsSafeIpAddress);
        }
        catch
        {
            return false;
        }
    }

    private async Task AppendRssItemsAsync(StringBuilder sb, IEnumerable<string> rssUrls)
    {
        foreach (var rssUrl in rssUrls)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await _httpClient.GetStringAsync(rssUrl, cts.Token);
                var doc = XDocument.Parse(response);
                var items = doc.Descendants("item").Take(3);
                foreach (var item in items)
                {
                    var title = item.Element("title")?.Value;
                    var description = item.Element("description")?.Value;
                    var link = item.Element("link")?.Value;
                    if (string.IsNullOrEmpty(title))
                        continue;

                    if (!string.IsNullOrEmpty(description))
                        description = Regex.Replace(description, "<.*?>", string.Empty).Trim();

                    sb.AppendLine($"- Başlık: {title}");
                    sb.AppendLine($"  Özet: {description}");
                    sb.AppendLine($"  Link: {link}");
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RSS feed çekilemedi: {Url}", rssUrl);
            }
        }
    }

    private async Task<string> GetLatestFinanceNewsAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("GÜNCEL EKONOMİ VE PİYASA HABERLERİ:");
        await AppendRssItemsAsync(sb, new[]
        {
            "https://www.bloomberght.com/rss",
            "https://www.dunya.com/rss"
        });
        return sb.Length > 40 ? sb.ToString() : string.Empty;
    }

    private async Task<string> GetLatestPoliticsNewsAsync()
    {
        var feedText = new StringBuilder();
        feedText.AppendLine("GÜNCEL POLİTİKA VE DÜNYA HABERLERİ:");
        await AppendRssItemsAsync(feedText, new[]
        {
            "https://feeds.bbci.co.uk/turkce/rss.xml",
            "https://feeds.bbci.co.uk/turkce/dunya/rss.xml",
            "https://www.trthaber.com/manset_articles.rss"
        });
        return feedText.Length > 40 ? feedText.ToString() : string.Empty;
    }

    private async Task<string> GetLatestScienceNewsAsync()
    {
        var sb = new StringBuilder();
        sb.AppendLine("GÜNCEL BİLİM VE TEKNOLOJİ HABERLERİ:");
        await AppendRssItemsAsync(sb, new[]
        {
            "https://evrimagaci.org/rss.xml",
            "https://www.webtekno.com/rss.xml"
        });
        return sb.Length > 40 ? sb.ToString() : string.Empty;
    }
}
