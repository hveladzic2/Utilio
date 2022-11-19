using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using Utilio.Provider.Common.DataContracts.Response;
using static System.Net.WebRequestMethods;

namespace Utilio.Provider.OpcinaNovoSarajevo.Application.Helpers
{
    public class NovoSarajevoHelper
    {

        public static List<string> categories = new List<string>() {"konkursi/aktuelni-konkursi/", "konkursi/arhiva-konkursa/", "javni-pozivi/aktuelni-javni-pozivi/", "javni-pozivi/arhiva-javnih-poziva/", "javne-nabavke/aktuelne-javne-nabavke/", "javne-nabavke/arhiva-javne-nabavke/"};
        public static List<Entry> entries = new List<Entry>();

        public static List<Entry> GetNovoSarajeviAllNotifications(DateTime fromDate) 
        {
            string url = "https://novosarajevo.ba/";
            GetNovoSarajevoNews(fromDate, url + "o-opcini/sve-novosti/", "o-opcini/sve-novosti/");
            foreach (string link in categories) 
            {
                GetNovoSarajevoAds(fromDate, url + link, link);
            }
            return entries;

        }
        public static List<Entry> GetNovoSarajevoAds(DateTime fromDate, string url, string category)
        {

            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc = web.Load(url);
            String stringDate;
            DateTime d = new DateTime();

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//*[@class='wpv-loop js-wpv-loop']//a[@href]"))
            {
                HtmlAttribute att = link.Attributes["href"];

                if (att.Value.Contains("a"))
                {
                    HtmlAgilityPack.HtmlDocument doc1 = web.Load(att.Value);
                    stringDate = doc1.DocumentNode.SelectSingleNode("//*[@class='article-date']//span").InnerText;
                    stringDate = Regex.Replace(stringDate, @"\t|\n|\r", "");
                    
                    d = DateTime.ParseExact(stringDate, "dd.MM.yyyy.", CultureInfo.InvariantCulture);

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
                        Description = category,
                        RawLog = "",
                        AdditionalInformation = "",
                        Regions = new List<int>(),
                        Streets = new List<int>()
                    });
                }
            }

            return entries;
        }
        public static List<Entry> GetNovoSarajevoNews(DateTime fromDate, string url, string category)
        {

            HtmlWeb web = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc = web.Load(url);
            String stringDate;
            DateTime d = new DateTime();

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
                        Description = category,
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
