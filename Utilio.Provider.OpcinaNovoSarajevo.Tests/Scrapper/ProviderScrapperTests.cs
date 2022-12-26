using Utilio.Common.Cache.Interfaces;
using Utilio.Common.Logger.Interfaces;
using Utilio.Common.Utilities;
using Utilio.Provider.OpcinaNovoSarajevo.Application.Scrapper;
using Moq;
using Moq.Protected;
using Microsoft.AspNetCore.Builder;

using System.Net;

namespace Utilio.Provider.OpcinaNovoSarajevo.Tests.Scrapper
{
    public class ProviderScrapperTests
    {
        private readonly ProviderScrapper _providerScrapper;
        private readonly Mock<ICacheProvider> _cacheProvider;
        private readonly Mock<ILoggerAdapter> _logger;
        private readonly Mock<HttpMessageHandler> _msgHandler;
        private readonly Mock<HttpClient> _httpClient;

        public ProviderScrapperTests()
        {

            var builder = WebApplication.CreateBuilder();
            ConfigHelper.Initialize(builder.Configuration.GetSection("ProviderConfiguration"));

            _cacheProvider = new Mock<ICacheProvider>();
            _logger = new Mock<ILoggerAdapter>();
            _msgHandler = new Mock<HttpMessageHandler>();

            _httpClient = new Mock<HttpClient>(_msgHandler.Object);
            _providerScrapper = new ProviderScrapper(_logger.Object, _cacheProvider.Object, _httpClient.Object);
        }


        [Fact]
        public void ProviderScrapper_FetchProviderData_Basic()
        {
            // Arrange
            var testDirectory = Environment.CurrentDirectory;
            var currentDirectory = testDirectory.Substring(0, testDirectory.Length - 17) + "/Scrapper/";
            var homepagePath = Path.Combine(currentDirectory, "homepage.html");
            var newsPath = Path.Combine(currentDirectory, "news.html");
            var publicPurchasePath = Path.Combine(currentDirectory, "publicPurchase.html");
            var publicPurchaseNewsPath = Path.Combine(currentDirectory, "publicPurchaseNews.html");

            string homepageData = File.ReadAllText(homepagePath);
            string newsData = File.ReadAllText(newsPath);
            string publicPurchaseData = File.ReadAllText(publicPurchasePath);
            string publicPurchaseNewsData = File.ReadAllText(publicPurchaseNewsPath);


            string homepageUrl = "https://novosarajevo.ba/o-opcini/sve-novosti/";
            string newsUrl =
                "https://novosarajevo.ba/svecano-otvorene-nove-prostorije-pozajmnog-odjeljenja-biblioteka-sarajevo-u-ulici-paromlinska-35/";
            string publicPurchaseUrl = "https://novosarajevo.ba/javne-nabavke/aktuelne-javne-nabavke/";
            string publicPurchaseNewsUrl = "https://novosarajevo.ba/odluka-o-ponistenju-postupka-nabavka-i-postavljanje-podnih-obloga/";

            var mockedProtected = _msgHandler.Protected();

            mockedProtected.Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = null
            });

            mockedProtected.Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Equals(homepageUrl)),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(homepageData)
            });

            mockedProtected.Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Equals(newsUrl)),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(newsData)
            });

            mockedProtected.Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Equals(publicPurchaseUrl)),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(publicPurchaseData)
            });

            mockedProtected.Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri!.Equals(publicPurchaseNewsUrl)),
                ItExpr.IsAny<CancellationToken>()
            ).ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(publicPurchaseNewsData)
            });

            var fromDate = new DateTime(2022, 12, 15, 8, 20, 0);
            string lastReferenceIdentifier = "string";
            string geoLocationServiceUrl = "string";

            // Act
            var content = _providerScrapper.FetchProviderData(fromDate, lastReferenceIdentifier, geoLocationServiceUrl).Result;

            // Assert
            Assert.NotNull(content.entries);
            Assert.Equal(2, content.entries.Count);
            Assert.Equal("Svečano otvorene nove prostorije pozajmnog odjeljenja Biblioteka Sarajevo u ulici Paromlinska 35", content.entries.ElementAt(0).Title);
            Assert.Equal(new DateTime(2022, 12, 19), content.entries.ElementAt(0).PublishDate);
            Assert.Equal("o-opcini/sve-novosti/", content.entries.ElementAt(0).Description);
            Assert.Equal("javne-nabavke/aktuelne-javne-nabavke/", content.entries.ElementAt(1).Description);
        }
    }
}