using FluentValidation;
using Utilio.Common.Validation;
using Utilio.Provider.Common.DataContracts.Request;

namespace Utilio.Provider.OpcinaNovoSarajevo.Api.Validators
{
    public class FetchDataRequestValidator : UtilioAbstractValidator<FetchDataRequest>
    {
        public FetchDataRequestValidator()
        {
            RuleFor(x => x.GeoLocationServiceUri)
                .NotNull()
                .WithMessage("GeoLocation service is not provided");

            RuleFor(x => x.LastReferenceIdentifier)
                .NotNull()
                .WithMessage("LastReferenceIdenitifer is not provided");
        }
    }
}
