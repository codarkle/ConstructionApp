using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ConstructionApp.Validators
{
    public class FloatGreaterThanAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly string _comparisonProperty;

        public FloatGreaterThanAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var currentValue = Convert.ToSingle(value);

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
                throw new ArgumentException($"Property '{_comparisonProperty}' not found on object.");

            var comparisonValue = property.GetValue(validationContext.ObjectInstance);
            if (comparisonValue == null)
                return ValidationResult.Success;

            float comparisonFloat = Convert.ToSingle(comparisonValue);

            if (currentValue <= comparisonFloat)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-floatgreaterthan", ErrorMessage ?? "End amount must be greater than start amount.");
            context.Attributes.Add("data-val-floatgreaterthan-other", _comparisonProperty);
        }
    }
}
