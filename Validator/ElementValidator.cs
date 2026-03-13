using System;
using System.Collections.Generic;
using SharedClasses;

namespace EdmontonDrawingValidator.Validator
{
    /// <summary>
    /// Base class for all DXF element validators
    /// </summary>
    public abstract class ElementValidator
    {
        protected List<ValidationError> _errors = new();

        /// <summary>
        /// Validates an element and returns the result
        /// </summary>
        public abstract ValidationResult Validate(object element);

        /// <summary>
        /// Common method to add validation errors
        /// </summary>
        protected void AddValidationError(
            string elementId,
            string elementType,
            string ruleName,
            string errorMessage,
            ValidationSeverity severity = ValidationSeverity.Error,
            string errorCode = null,
            string layerName = null,
            Dictionary<string, object> context = null)
        {
            var error = new ValidationError
            {
                ElementId = elementId,
                ElementType = elementType,
                RuleName = ruleName,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode ?? ruleName,
                Severity = severity,
                LayerName = layerName,
                TimestampUtc = DateTime.UtcNow,
                AdditionalContext = context ?? new Dictionary<string, object>()
            };

            _errors.Add(error);
        }

        /// <summary>
        /// Returns all collected errors as a ValidationResult
        /// </summary>
        protected ValidationResult GetValidationResult(bool isValid = true)
        {
            var result = new ValidationResult { IsValid = isValid && _errors.Count == 0 };
            result.Errors.AddRange(_errors);
            _errors.Clear();
            return result;
        }

        /// <summary>
        /// Clears all accumulated errors
        /// </summary>
        protected void ClearErrors() => _errors.Clear();
    }
}