using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilio.Provider.OpcinaNovoSarajevo.Api.Controllers;
using FakeItEasy;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Utilio.Provider.OpcinaNovoSarajevo.Application.Scrapper;
using Utilio.Provider.Common.DataContracts.Request;
using Utilio.Provider.Common.DataContracts.Response;

namespace Utilio.Provider.OpcinaNovoSarajevo.Tests.Controller
{
    public class ProviderControllerTests
    {
        private ProviderController _providerController;
        private ILogger<ProviderController> _logger;
        private IProviderScrapper _providerScrapper;
        private IValidator<FetchDataRequest> _validator;

        public ProviderControllerTests()
        {
            _logger = A.Fake<ILogger<ProviderController>>();
            _providerScrapper = A.Fake<IProviderScrapper>();
            _validator = A.Fake<IValidator<FetchDataRequest>>();
            _providerController = new ProviderController(_logger, _providerScrapper, _validator);
        }


        [Fact]
        public async void ProviderController_Fetch_Basic()
        {
            //Arrange
            var entries = new List<Entry>();
            var entry = new Entry();
            entry.Title = "Test Title";
            entries.Add(entry);
            var referenceIdentifier = "string";
            var data = (entries, referenceIdentifier);
            var request = A.Fake<FetchDataRequest>();
            var validationResult = new FluentValidation.Results.ValidationResult();
            validationResult.Errors = new List<FluentValidation.Results.ValidationFailure>();
            A.CallTo(() => _validator.ValidateAsync(request, default)).Returns(validationResult);
            A.CallTo(() => _providerScrapper.FetchProviderData(request.FromDateUTC, request.LastReferenceIdentifier, request.GeoLocationServiceUri)).Returns(data);

            //Act
            var actionResult = await _providerController.FetchProviderData(request);

            //Assert
            var result = actionResult.Data;

            Assert.NotNull(result);

            Assert.Equal(result.Count, 1);

            Assert.Equal(result.ElementAt(0).Title, "Test Title");

        }



        [Fact]
        public async void ProviderController_Fetch_Basic2()
        {
            //Arrange
            var entries = new List<Entry>();
            var referenceIdentifier = "string";
            var data = (entries, referenceIdentifier);
            var request = A.Fake<FetchDataRequest>();
            var validationResult = new FluentValidation.Results.ValidationResult();
            validationResult.Errors = new List<FluentValidation.Results.ValidationFailure>();
            A.CallTo(() => _validator.ValidateAsync(request, default)).Returns(validationResult);

            //Act
            var actionResult = await _providerController.FetchProviderData(request);

            //Assert
            Assert.NotNull(actionResult);

            var succes = actionResult.Success;
            Assert.True(succes);
        }

    }
}
