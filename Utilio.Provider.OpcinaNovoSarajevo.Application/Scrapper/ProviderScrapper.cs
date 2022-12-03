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

namespace Utilio.Provider.OpcinaNovoSarajevo.Application.Scrapper
{
    public class ProviderScrapper : IProviderScrapper
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly ILoggerAdapter _logger;
        private List<string> categories { get; set; }
        private string url { get; set; }

        private string dateQuery { get; set; }
        private string loopQuery { get; set; }
        private string contentQuery { get; set; }

        private List<Entry> entries = new List<Entry>();
        public ProviderScrapper (
            ILoggerAdapter logger,
            ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
            _logger = logger;
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
        private string GetHtmlContent(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; rv:68.0) Gecko/20100101 Firefox/68.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();

            if (response.ContentEncoding?.IndexOf("gzip", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            }
            else if (response.ContentEncoding?.IndexOf("deflate", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);
            }

            using var ms = new MemoryStream();
            responseStream?.CopyTo(ms);

            var htmlContent = Encoding.UTF8.GetString(ms.ToArray());

            return htmlContent;
        }

        private HtmlNode GetSingleNode(string url, string xpath)
        {
            var html = GetHtmlContent(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            //var nodeArray = htmlDocument.DocumentNode.SelectNodes(xpath);
            var singleNode = htmlDocument.DocumentNode.SelectSingleNode(xpath);

            return singleNode;
        }

        private List<string> GetLinks(string url, string xpath)
        {
            List<string> links = new List<string>();
            var html = GetHtmlContent(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var anchorTags = htmlDocument.DocumentNode.SelectNodes("//a[@href]");
            foreach (var node in anchorTags)
            {
                var link = node.GetAttributeValue("href", string.Empty);
                if (Regex.IsMatch(link, @"/vijesti/\d+"))
                {
                    links.Add(node.GetAttributeValue("href", string.Empty));
                }
            }

            return new HashSet<String>(links).ToList();
        }

        // Function to decode html string. Removes all html specifics and returns plain text.
        private string RecursiveHtmlDecode(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var tmp = HttpUtility.HtmlDecode(str);
            while (tmp != str)
            {
                str = tmp;
                tmp = HttpUtility.HtmlDecode(str);
            }
            return str; //completely decoded string
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
            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc = web.Load(url);
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
                        HtmlAgilityPack.HtmlDocument doc1 = web.Load(att.Value);
                        publishedDate = DateTime.ParseExact(Regex.Replace(doc1.DocumentNode.SelectSingleNode(dateQuery).InnerText, @"\t|\n|\r", ""), "dd.MM.yyyy.", CultureInfo.InvariantCulture);

                        if (DateTime.Compare(publishedDate, fromDate) < 0) break;

                        string docs = null;
                        if (category.Contains("novosti") && doc1.DocumentNode.SelectNodes(contentQuery + "p") != null)
                        {
                            foreach (HtmlNode paragraph in doc1.DocumentNode.SelectNodes(contentQuery + "p"))
                            {
                                docs += paragraph.InnerText;
                            }
                        }
                        else if (doc1.DocumentNode.SelectNodes(contentQuery + "a[@href]") != null)
                        {
                            foreach (HtmlNode docLink in doc1.DocumentNode.SelectNodes(contentQuery + "a[@href]"))
                            {
                                docs += docLink.Attributes["href"].Value + ";";
                            }
                        }

                        entries.Add(new Entry
                        {
                            PublishDate = publishedDate,
                            ReferenceStartDate = startDate,
                            ReferenceEndDate = endDate,
                            Title = doc1.DocumentNode.SelectSingleNode(contentQuery + "h2").InnerText,
                            Content = docs,
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

        public void setData(List<string> data, string baseUrl, string loopQ, string contentQ, string dateQ)
        {
            categories = data;
            url = baseUrl;
            contentQuery = contentQ;
            loopQuery = loopQ;
            dateQuery = dateQ;
        }
    }
}
