using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utilio.Common.DataContracts.Base;
using Utilio.Common.DataContracts.Enumerations;
using Utilio.Provider.OpcinaNovoSarajevo.Api.Validators;
using Utilio.Provider.OpcinaNovoSarajevo.Application.Scrapper;
using Utilio.Provider.Common.DataContracts.Request;
using Utilio.Provider.Common.DataContracts.Response;

namespace Utilio.Provider.OpcinaNovoSarajevo.Api.Controllers
{
    [ApiController]
    [Route("api/provider")]
    public class ProviderController : ControllerBase
    {

        private readonly ILogger<ProviderController> _logger;
        private readonly IProviderScrapper _providerScrapper;
        private readonly IValidator<FetchDataRequest> _validator;

        public ProviderController(
            ILogger<ProviderController> logger,
            IProviderScrapper providerScrapper,
            IValidator<FetchDataRequest> validator)
        {
            _logger = logger;
            _providerScrapper = providerScrapper;
            _validator = validator;
        }

        //[Route("fetch")]
        [HttpPost]
        [Route("fetch", Name = "FetchProviderData")]
        public async Task<FetchDataResponse> FetchProviderData([FromBody] FetchDataRequest request)
        {
            // doing the manual fluent validation invocation since we'll only have one service method
            var validationResult = await _validator.ValidateAsync(request);
           
            if (!validationResult.IsValid)
            {
                validationResult.AddToModelState(this.ModelState);
                return new FetchDataResponse()
                {
                    Error = ValidationHelper.ToErrorResponse(ModelState),
                    Success = false
                }; 
            }

            var result = await _providerScrapper.FetchProviderData(request.FromDateUTC, request.LastReferenceIdentifier, request.GeoLocationServiceUri);

            return new FetchDataResponse()
            {
                Data = result.entries,
                ReferenceIdentifier = result.referenceIdentifier,
                ProviderIdentifier = request.ProviderIdentifier,
                Success = true
            };
        }
    }
}