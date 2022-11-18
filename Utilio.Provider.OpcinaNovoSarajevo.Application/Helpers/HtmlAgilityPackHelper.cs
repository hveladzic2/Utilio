using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;


namespace Utilio.Provider.OpcinaNovoSarajevo.Application.Helpers
{
    public static class HtmlAgilityPackHelper
    {
        //https://www.devbridge.com/articles/web-scraping-tutorial-asp-net/

        public static string GetHtmlContent(string url)
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

        public static HtmlNode GetSingleNode(string url, string xpath)
        {
            var html = GetHtmlContent(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            //var nodeArray = htmlDocument.DocumentNode.SelectNodes(xpath);
            var singleNode = htmlDocument.DocumentNode.SelectSingleNode(xpath);

            return singleNode;
        }

        public static List<string> GetLinks(string url, string xpath)
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
        public static string RecursiveHtmlDecode(string str) {
            if (string.IsNullOrWhiteSpace(str)) return str;  
            var tmp = HttpUtility.HtmlDecode(str);
            while (tmp != str)
            {
                str = tmp;
                tmp = HttpUtility.HtmlDecode(str);
            }
            return str; //completely decoded string
        }
    }
}
