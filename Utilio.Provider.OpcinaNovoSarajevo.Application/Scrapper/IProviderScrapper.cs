using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilio.Provider.Common.DataContracts.Response;

namespace Utilio.Provider.OpcinaNovoSarajevo.Application.Scrapper
{
    public interface IProviderScrapper
    {
        /// <summary>
        /// Fetches data from provider based on provided date
        /// </summary>
        /// <param name="fromDate">From date in UTC</param>
        /// <param name="lastReferenceIdentifier">Last reference identifier returned by this provider service</param>
        /// <param name="geoLocationServiceUrl">An URI to service used to fetch geolocation data</param>
        /// <returns>A tuple of new provider entries and new reference identifier</returns>
        Task<(ICollection<Entry> entries, string referenceIdentifier)> FetchProviderData(DateTime fromDate, string lastReferenceIdentifier, string geoLocationServiceUrl);
        void setData(List<string> data, string url, string loopQuery, string contentQuery, string dateQuery);
    }
}
