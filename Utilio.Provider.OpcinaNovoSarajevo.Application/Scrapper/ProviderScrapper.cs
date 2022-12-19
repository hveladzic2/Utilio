using HtmlAgilityPack;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utilio.Common.Cache.Interfaces;
using Utilio.Common.Logger.Interfaces;
using Utilio.Provider.Common.DataContracts.Response;
using System.Globalization;
using Utilio.Common.Utilities;

namespace Utilio.Provider.OpcinaNovoSarajevo.Application.Scrapper
{
    public class ProviderScrapper : IProviderScrapper
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly ILoggerAdapter _logger;
        private readonly HttpClient _httpClient;
        private List<string> categories { get; set; }
        private string url { get; set; }
        private string dateQuery { get; set; }
        private string loopQuery { get; set; }
        private string contentQuery { get; set; }

        private List<Entry> entries = new List<Entry>();
        public ProviderScrapper (
            ILoggerAdapter logger,
            ICacheProvider cacheProvider,
            HttpClient httpClient)
        {
            _cacheProvider = cacheProvider;
            _logger = logger;
            _httpClient = httpClient;
            
            categories = new List<string>();

            categories.Add(ConfigHelper.GetValue<string>("sveNovosti"));
            categories.Add(ConfigHelper.GetValue<string>("arhivaJavnihRasprava"));
            categories.Add(ConfigHelper.GetValue<string>("aktuelneJavneRasprave"));
            categories.Add(ConfigHelper.GetValue<string>("arhivaKonkursa"));
            categories.Add(ConfigHelper.GetValue<string>("aktuelniKonkursi"));
            categories.Add(ConfigHelper.GetValue<string>("aktuelniJavniPozivi"));
            categories.Add(ConfigHelper.GetValue<string>("arhivaJavnihPoziva"));
            categories.Add(ConfigHelper.GetValue<string>("aktuelneJavneNabavke"));
            categories.Add(ConfigHelper.GetValue<string>("arhivaJavneNabavke"));
            
            url = ConfigHelper.GetValue<string>("NovoSarajevo");
            contentQuery = ConfigHelper.GetValue<string>("contentQuery");
            loopQuery = ConfigHelper.GetValue<string>("loopQuery");
            dateQuery = ConfigHelper.GetValue<string>("dateQuery");
        }

        public async Task<(ICollection<Entry> entries, string referenceIdentifier)> FetchProviderData
            (DateTime fromDate, 
            string lastReferenceIdentifier, 
            string geoLocationServiceUrl
            )
        {
            _logger.LogDebug("Entering FetchProviderData method");

            var entries = GetNovoSarajeviAllNotifications(fromDate);
            var referenceIdentifier = string.Empty; // not used for now

            _logger.LogDebug("End FetchProviderData method");

            return (entries, referenceIdentifier);
        }
        private async Task<string> GetHtmlContent(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:68.0) Gecko/20100101 Firefox/68.0");
            request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.Headers.Add("Accept-Language", "en-us,en;q=0.5");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
        
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseStream = await response.Content.ReadAsStreamAsync();
            
            if (response.Content.Headers.ContentEncoding.Any(x => x.Equals("gzip", StringComparison.InvariantCultureIgnoreCase)))
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }
            else if (response.Content.Headers.ContentEncoding.Any(x => x.Equals("deflate", StringComparison.InvariantCultureIgnoreCase)))
            {
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
            }
        
            using var ms = new MemoryStream();
            await responseStream.CopyToAsync(ms);
        
            var htmlContent = Encoding.UTF8.GetString(ms.ToArray());
            
            return htmlContent;
        }
        private List<Entry> GetNovoSarajeviAllNotifications(DateTime fromDate)
        {
            foreach (string link in categories)
            {
                GetNovoSarajevoAds(fromDate, url + link, link);
            }
            return entries;

        }
        private List<Entry> GetNovoSarajevoAds(DateTime fromDate, string url, string category)
        {
            // HtmlWeb web = new HtmlWeb();
            // HtmlDocument doc = new HtmlDocument();
            // doc = web.Load(url);
            
            var html = GetHtmlContent(url).Result;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            DateTime publishedDate = new DateTime();
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            if (doc.DocumentNode.SelectNodes(loopQuery + "a[@href]") != null)
            {
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes(loopQuery + "a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];


                    if (category.Contains("aktueln"))
                    {
                        foreach (HtmlNode td in doc.DocumentNode.SelectNodes(loopQuery + "td"))
                        {
                            Match match = Regex.Match(td.InnerText, @"\d{2}\.\d{2}\.\d{4}");
                            if (td.InnerText.Contains("Objavljen"))
                            {
                                startDate = DateTime.ParseExact(match.Value + ".", "dd.MM.yyyy.", CultureInfo.InvariantCulture);
                            }
                            else if (td.InnerText.Contains("Završava"))
                            {
                                endDate = DateTime.ParseExact(match.Value + ".", "dd.MM.yyyy.", CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    if (att.Value.Contains("a"))
                    {
                        //HtmlAgilityPack.HtmlDocument doc1 = web.Load(att.Value);
                        
                        var doc1 = new HtmlDocument();
                        doc1.LoadHtml(GetHtmlContent(att.Value).Result);
                        
                        publishedDate = DateTime.ParseExact(Regex.Replace(doc1.DocumentNode.SelectSingleNode(dateQuery).InnerText, @"\t|\n|\r", ""), "dd.MM.yyyy.", CultureInfo.InvariantCulture);

                        if (DateTime.Compare(publishedDate, fromDate) < 0) break;

                        string content = null;
                        if (doc1.DocumentNode.SelectNodes(contentQuery + "p") != null)
                        {
                            foreach (HtmlNode paragraph in doc1.DocumentNode.SelectNodes(contentQuery + "p"))
                            {
                                content += paragraph.InnerText + " ";
                            }
                        }
                        if (doc1.DocumentNode.SelectNodes(contentQuery + "a[@href]") != null)
                        {
                            foreach (HtmlNode docLink in doc1.DocumentNode.SelectNodes(contentQuery + "a[@href]"))
                            {
                                content += docLink.Attributes["href"].Value + ";";
                            }
                        }

                        entries.Add(new Entry
                        {
                            PublishDate = publishedDate,
                            ReferenceStartDate = startDate,
                            ReferenceEndDate = endDate,
                            Title = doc1.DocumentNode.SelectSingleNode(contentQuery + "h2").InnerText,
                            Content = content,
                            SourceUrl = url,
                            Description = category,
                            RawLog = "",
                            AdditionalInformation = "",
                            Regions = new List<int>(),
                            Streets = new List<int>()
                        });
                    }
                }
            }

            return entries;
        }
    }
}
