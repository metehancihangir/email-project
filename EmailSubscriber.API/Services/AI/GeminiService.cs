using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
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
        
        switch (category.ToLower())
        {
            case "mitoloji":
                systemPrompt = $@"Sen uzman, akıcı ve sade bir dille yazan bir Mitoloji Bülteni yazarı/editörüsün. Görevin, karmaşık mitolojik hikayeleri, karakter ilişkilerini ve efsaneleri modern bir okuyucu için basitleştirerek anlatmaktır. 

ÖZEL TALİMATLAR:
1. Özellikle Yunan mitolojisi, savaş tanrıları (örn. Ares), rüzgar tanrıları veya doğa olaylarının mitolojik kökenleri gibi konuları işlerken karakterlerin soyağaçlarında kaybolma. Ana hikayeye ve sembolizme odaklan.
2. Vurgulama Kuralları: Önemli karakter adlarını, mitolojik eşyaları ve mekanları HTML <b> etiketi kullanarak kalın (bold) font ile yaz. KESİNLİKLE MARKDOWN (**) KULLANMA. Geri kalan metin normal olmalı.
3. Dilin akademik değil, hikaye anlatıcısı tadında ama net ve sade olmalı.
4. Kaynakça (URL) verirken KESİNLİKLE uydurma (hallucinated) linkler kullanma. Sadece gerçekliğinden emin olduğun, konuyla ilgili Wikipedia sayfalarının linklerini (örn: https://tr.wikipedia.org/wiki/Zeus) kullan ve `<a href=""URL"">Kaynak Adı</a>` formatında tıklanabilir link yap.

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

        int maxRetries = 7;
        string currentApiKey = _apiKey;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.6-flash:generateContent";
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };
                requestMessage.Headers.Add("x-goog-api-key", currentApiKey);

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
                _logger.LogWarning(ex, $"Gemini isteği başarısız oldu (Deneme {attempt}/{maxRetries}). 10 saniye sonra tekrar deneniyor...");
                await Task.Delay(10000);
            }
        }
        
        return string.Empty;
    }

    public Task<string?> GetImageUrlForTopicAsync(string category, string topic, string htmlContent = "")
    {
        var normalizedCategory = category.ToLowerInvariant().Trim();
        var textToSearch = $"{topic} {htmlContent}".ToLowerInvariant();

        string selectedUrl = normalizedCategory switch
        {
            "mitoloji" => GetMythologyImage(textToSearch),
            "finans" => GetFinanceImage(textToSearch),
            "bilim" => GetScienceImage(textToSearch),
            "politika" => GetPoliticsImage(textToSearch),
            _ => "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?q=80&w=800&auto=format&fit=crop"
        };

        _logger.LogInformation("{Category} bülteni için konuya özel güvenli görsel belirlendi: {Url}", category, selectedUrl);
        return Task.FromResult<string?>(selectedUrl);
    }

    private static string GetMythologyImage(string text)
    {
        if (text.Contains("zeus") || text.Contains("jüpiter") || text.Contains("olimpos") || text.Contains("şimşek"))
            return "https://images.unsplash.com/photo-1564507592333-c60657eea523?q=80&w=800&auto=format&fit=crop"; // Antik Yunan Tapınağı

        if (text.Contains("poseidon") || text.Contains("neptün") || text.Contains("deniz") || text.Contains("okyanus"))
            return "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?q=80&w=800&auto=format&fit=crop"; // Okyanus & Fırtına

        if (text.Contains("athena") || text.Contains("akropolis") || text.Contains("parthenon") || text.Contains("truva"))
            return "https://images.unsplash.com/photo-1555993539-1732b0258235?q=80&w=800&auto=format&fit=crop"; // Parthenon Tapınağı

        if (text.Contains("apollon") || text.Contains("sanat") || text.Contains("müzik") || text.Contains("afrodit"))
            return "https://images.unsplash.com/photo-1576014131341-fe1486fb2475?q=80&w=800&auto=format&fit=crop"; // Klasik Sanat & Heykel

        if (text.Contains("ikarus") || text.Contains("güneş") || text.Contains("kanat") || text.Contains("daidalos"))
            return "https://images.unsplash.com/photo-1518709268805-4e9042af9f23?q=80&w=800&auto=format&fit=crop"; // Gökyüzü & Kanat

        // Ares, Hades, Hermes ve Genel Mitoloji için Klasik Mermer Heykel
        return "https://images.unsplash.com/photo-1579783900882-c0d3dad7b119?q=80&w=800&auto=format&fit=crop";
    }

    private static string GetFinanceImage(string text)
    {
        if (text.Contains("kripto") || text.Contains("bitcoin") || text.Contains("blockchain") || text.Contains("btc"))
            return "https://images.unsplash.com/photo-1518770660439-4636190af475?q=80&w=800&auto=format&fit=crop"; // Kripto & Dijital Varlık

        if (text.Contains("altın") || text.Contains("emtia") || text.Contains("petrol") || text.Contains("gümüş"))
            return "https://images.unsplash.com/photo-1610375461246-83df859d849d?q=80&w=800&auto=format&fit=crop"; // Altın Külçeleri

        if (text.Contains("enflasyon") || text.Contains("faiz") || text.Contains("merkez bankası") || text.Contains("tcmb") || text.Contains("fed") || text.Contains("dolar"))
            return "https://images.unsplash.com/photo-1580519542036-c47de6196ba5?q=80&w=800&auto=format&fit=crop"; // Para & Merkez Bankacılığı

        if (text.Contains("bist") || text.Contains("borsa") || text.Contains("hisse") || text.Contains("nasdaq") || text.Contains("endeks"))
            return "https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?q=80&w=800&auto=format&fit=crop"; // Borsa & Piyasa Grafik Ekranı

        // Genel Finans
        return "https://images.unsplash.com/photo-1590283603385-17ffb3a7f29f?q=80&w=800&auto=format&fit=crop";
    }

    private static string GetScienceImage(string text)
    {
        if (text.Contains("yapay zeka") || text.Contains("robot") || text.Contains("yazılım") || text.Contains("bilgisayar") || text.Contains("algoritma"))
            return "https://images.unsplash.com/photo-1620712943543-bcc4688e7485?q=80&w=800&auto=format&fit=crop"; // Yapay Zeka & Sinir Ağları

        if (text.Contains("kuantum") || text.Contains("fizik") || text.Contains("laboratuvar") || text.Contains("dna") || text.Contains("biyoloji") || text.Contains("tıp") || text.Contains("genetik"))
            return "https://images.unsplash.com/photo-1507413245164-6160d8298b31?q=80&w=800&auto=format&fit=crop"; // Bilimsel Araştırma & Laboratuvar

        // Uzay, James Webb, Gezegenler, Karadelik ve Genel Bilim
        return "https://images.unsplash.com/photo-1451187580459-43490279c0fa?q=80&w=800&auto=format&fit=crop";
    }

    private static string GetPoliticsImage(string text)
    {
        if (text.Contains("meclis") || text.Contains("parlamento") || text.Contains("yasa") || text.Contains("kanun") || text.Contains("hükümet") || text.Contains("seçim"))
            return "https://images.unsplash.com/photo-1540910419892-4a36d2c3266c?q=80&w=800&auto=format&fit=crop"; // Parlamento Binası

        if (text.Contains("lider") || text.Contains("zirve") || text.Contains("başkan") || text.Contains("bakan") || text.Contains("açıklama") || text.Contains("basın"))
            return "https://images.unsplash.com/photo-1577962917302-cd874c4e31d2?q=80&w=800&auto=format&fit=crop"; // Küresel Zirve & Basın Toplantısı

        // Diplomasi, Birleşmiş Milletler, Dış Politika ve Genel Politika
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
            AllowAutoRedirect = false // Otomatik yönlendirmeler ile iç ağa atlamayı engelle
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

            // 0.0.0.0/8 (Current network)
            if (bytes[0] == 0) return false;

            // 10.0.0.0/8 (Private Network)
            if (bytes[0] == 10) return false;

            // 127.0.0.0/8 (Loopback)
            if (bytes[0] == 127) return false;

            // 169.254.0.0/16 (Link-Local & Cloud Metadata e.g. AWS/Azure/GCP 169.254.169.254)
            if (bytes[0] == 169 && bytes[1] == 254) return false;

            // 172.16.0.0/12 (Private Network: 172.16.0.0 - 172.31.255.255)
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false;

            // 192.168.0.0/16 (Private Network)
            if (bytes[0] == 192 && bytes[1] == 168) return false;

            // 224.0.0.0/4 (Multicast)
            if (bytes[0] >= 224 && bytes[0] <= 239) return false;

            // 240.0.0.0/4 (Reserved)
            if (bytes[0] >= 240) return false;
        }

        return true;
    }

    private static async Task<bool> IsSafePublicUrlAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        // Yalnızca HTTP/HTTPS protokollerine izin ver
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

    private async Task<string> GetLatestScienceNewsAsync()
    {
        try
        {
            var rssUrl = "https://evrimagaci.org/rss.xml";
            var response = await _httpClient.GetStringAsync(rssUrl);
            var doc = XDocument.Parse(response);
            
            var items = doc.Descendants("item").Take(3);
            var sb = new StringBuilder();
            sb.AppendLine("GÜNCEL BİLİM HABERLERİ:");
            foreach (var item in items)
            {
                var title = item.Element("title")?.Value;
                var description = item.Element("description")?.Value;
                var link = item.Element("link")?.Value;
                
                // HTML etiketlerini temizleyelim
                if (!string.IsNullOrEmpty(description))
                {
                    description = Regex.Replace(description, "<.*?>", string.Empty);
                }
                
                sb.AppendLine($"- Başlık: {title}");
                sb.AppendLine($"  Özet: {description}");
                sb.AppendLine($"  Link: {link}");
                sb.AppendLine();
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bilim RSS feed çekilemedi, varsayılan bilim istemiyle devam edilecek.");
            return string.Empty;
        }
    }
}
