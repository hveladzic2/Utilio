using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using Utilio.Provider.Common.DataContracts.Response;
using static System.Net.WebRequestMethods;

namespace Utilio.Provider.OpcinaNovoSarajevo.Application.Helpers
{
    public class NovoSarajevoHelper
    {

        public static List<Entry> GetNovoSarajevoAds(DateTime fromDate)
        {
            string url = "https://novosarajevo.ba/konkursi/aktuelni-konkursi/";
            var entries = new List<Entry>();

            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc = web.Load(url);
            String stringDate;
            DateTime d = new DateTime();
            // extracting all links
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//*[@class='wpv-loop js-wpv-loop']//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att.Value.Contains("a"))
                {
                    HtmlAgilityPack.HtmlDocument doc1 = web.Load(att.Value);
                    stringDate = doc1.DocumentNode.SelectSingleNode("//*[@class='article-date']//span").InnerText;
                    stringDate = Regex.Replace(stringDate, @"\t|\n|\r", "");
                    
                    d = DateTime.ParseExact(stringDate, "dd.MM.yyyy.", CultureInfo.InvariantCulture);
                    // if timestamp is 0 (not specified), return all news
                    if (DateTime.Compare(d, fromDate) < 0) break;
                    string docs = null;
                    foreach (HtmlNode docLink in doc1.DocumentNode.SelectNodes("//*[@class='article-content-inner']//a[@href]"))
                    {
                        docs += docLink.Attributes["href"].Value  + ";";
                    }

                    entries.Add(new Entry
                    {
                        PublishDate = d,
                        Title = doc1.DocumentNode.SelectSingleNode("//*[@class='article-content-inner']//h2").InnerText,
                        Content = docs,
                        SourceUrl = url,
                        Description = "Ads",
                        RawLog = "",
                        AdditionalInformation = "",
                        Regions = new List<int>(),
                        Streets = new List<int>()
                    });
                }
            }

            return entries;
        }
        public static List<Entry> GetNovoSarajevoNews(DateTime fromDate)
        {
            string url = "https://novosarajevo.ba/o-opcini/sve-novosti/";
            var entries = new List<Entry>();

            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc = web.Load(url);
            String stringDate;
            DateTime d = new DateTime();
            // extracting all links
            //int br = 0;
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//*[@class='wpv-loop js-wpv-loop']//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att.Value.Contains("a"))
                {
                    HtmlAgilityPack.HtmlDocument doc1 = web.Load(att.Value);
                    stringDate = doc1.DocumentNode.SelectSingleNode("//*[@class='article-date']//span").InnerText;
                    stringDate = Regex.Replace(stringDate, @"\t|\n|\r", "");

                    d = DateTime.ParseExact(stringDate, "dd.MM.yyyy.", CultureInfo.InvariantCulture);
                    // if timestamp is 0 (not specified), return all news
                    if (DateTime.Compare(d, fromDate) < 0) break;
                    string desc = "";
                    foreach (HtmlNode paragraph in doc1.DocumentNode.SelectNodes("//*[@class='article-content']//p"))
                    {
                        desc += paragraph.InnerText;
                    }
                    entries.Add(new Entry
                    {
                        PublishDate = d,
                        Title = doc1.DocumentNode.SelectSingleNode("//*[@class='article-content-inner']//h2").InnerText,
                        Content = desc,
                        SourceUrl = url,
                        Description = "News",
                        RawLog = "",
                        AdditionalInformation = "",
                        Regions = new List<int>(),
                        Streets = new List<int>()
                    });
                }
            }

            return entries;
        }
    }
}
