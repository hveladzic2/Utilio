using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using Utilio.Provider.Common.DataContracts.Response;
using static System.Net.WebRequestMethods;

namespace Utilio.Provider.OpcinaNovoSarajevo.Application.Helpers
{
    public class NovoSarajevoHelper
    {

        public static List<string> categories = new List<string>() { "o-opcini/sve-novosti/", "javne-rasprave/arhiva-javnih-rasprava/", "javne-rasprave/aktuelne-javne-rasprave/", "konkursi/aktuelni-konkursi/", "konkursi/arhiva-konkursa/", "javni-pozivi/aktuelni-javni-pozivi/", "javni-pozivi/arhiva-javnih-poziva/", "javne-nabavke/aktuelne-javne-nabavke/", "javne-nabavke/arhiva-javne-nabavke/"};
        public static List<Entry> entries = new List<Entry>();

        public static List<Entry> GetNovoSarajeviAllNotifications(DateTime fromDate) 
        {
            string url = "https://novosarajevo.ba/";
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
            DateTime publishedDate = new DateTime();
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();

            if (doc.DocumentNode.SelectNodes("//*[@class='wpv-loop js-wpv-loop']//a[@href]") != null)
            {
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//*[@class='wpv-loop js-wpv-loop']//a[@href]"))
                {
                    HtmlAttribute att = link.Attributes["href"];


                    if (category.Contains("aktueln"))
                    {
                        foreach (HtmlNode td in doc.DocumentNode.SelectNodes("//*[@class='wpv-loop js-wpv-loop']//td"))
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
                        publishedDate = DateTime.ParseExact(Regex.Replace(doc1.DocumentNode.SelectSingleNode("//*[@class='article-date']//span").InnerText, @"\t|\n|\r", ""), "dd.MM.yyyy.", CultureInfo.InvariantCulture);

                        if (DateTime.Compare(publishedDate, fromDate) < 0) break;

                        string docs = null;
                        if (category.Contains("novosti"))
                        {
                            foreach (HtmlNode paragraph in doc1.DocumentNode.SelectNodes("//*[@class='article-content']//p"))
                            {
                                docs += paragraph.InnerText;
                            }
                        }
                        else
                        {
                            foreach (HtmlNode docLink in doc1.DocumentNode.SelectNodes("//*[@class='article-content-inner']//a[@href]"))
                            {
                                docs += docLink.Attributes["href"].Value + ";";
                            }
                        }

                        entries.Add(new Entry
                        {
                            PublishDate = publishedDate,
                            ReferenceStartDate = startDate,
                            ReferenceEndDate = endDate,
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
            }

            return entries;
        }
    }
}
