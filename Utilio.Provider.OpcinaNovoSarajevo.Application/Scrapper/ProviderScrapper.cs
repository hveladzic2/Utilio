using System.Threading.Tasks;
using Utilio.Common.Cache.Interfaces;
using Utilio.Common.Logger.Interfaces;
using Utilio.Provider.Common.DataContracts.Response;
using Utilio.Provider.OpcinaNovoSarajevo.Application.Helpers;

namespace Utilio.Provider.OpcinaNovoSarajevo.Application.Scrapper
{
    public class ProviderScrapper : IProviderScrapper
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly ILoggerAdapter _logger;

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

            var entries = NovoSarajevoHelper.GetNovoSarajevoNews(fromDate);
            var referenceIdentifier = string.Empty; // not used for now

            _logger.LogDebug("End FetchProviderData method");

            return (entries, referenceIdentifier);
        }
    }
}
