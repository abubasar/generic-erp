using Application.Core.Common;
using Application.Core.Exceptions;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Interceptors
{
    public class ValidatorInterceptor : IValidatorInterceptor
    {

        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
        {
            return commonContext;
        }

        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            var failures = result.Errors.Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                var errorMessages = failures.Select(a => new ErrorModel {FieldName=a.PropertyName,Message= a.ErrorMessage } ).Distinct().ToList();
                throw new CustomValidationException(errorMessages);
            }

            return result;
        }


    }
}