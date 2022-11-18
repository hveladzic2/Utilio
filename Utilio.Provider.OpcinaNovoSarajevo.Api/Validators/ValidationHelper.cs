namespace Utilio.Provider.OpcinaNovoSarajevo.Api.Validators
{
    public static class ValidationHelper
    {
        public static Utilio.Common.DataContracts.Base.Error ToErrorResponse(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelStateDictionary)
        {
            var errorResponse = new Utilio.Common.DataContracts.Base.Error();

            if (modelStateDictionary.IsValid)
                return errorResponse;

            errorResponse.Errors = modelStateDictionary
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault());

            return errorResponse;
        }
    }

}
