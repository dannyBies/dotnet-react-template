using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Runtime.CompilerServices;

namespace Example.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        private readonly ILogger _logger;

        public BaseController(ILogger logger)
        {
            _logger = logger;
        }

        public bool IsValidRequest<T>(IValidator<T> validator, T request, out ModelStateDictionary result, [CallerFilePath] string callerFile = "")
        {
            var validationResult = validator.Validate(request);
            if (validationResult.IsValid)
            {
                result = ModelState;
                return true;
            }

            _logger.LogWarning("Validation errors in: {caller}", callerFile);
            foreach (var validationError in validationResult.Errors)
            {
                _logger.LogWarning("Validation error: {ErrorMessage}", validationError.ErrorMessage);
                ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
            }

            result = ModelState;
            return false;
        }
    }
}
