using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations; 

namespace ConstructionApp.Validators
{
    public class DateGreaterThanAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly string _comparisonProperty;

        public DateGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime currentValue)
                return ValidationResult.Success;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                throw new ArgumentException($"Property '{_comparisonProperty}' not found on object.");

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);
            if (comparisonValue is not DateTime comparisonDate)
                return ValidationResult.Success;

            if (currentValue <= comparisonDate)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-dategreaterthan", ErrorMessage ?? "End date must be greater than start date.");
            context.Attributes.Add("data-val-dategreaterthan-other", _comparisonProperty);
        }
    }
}
