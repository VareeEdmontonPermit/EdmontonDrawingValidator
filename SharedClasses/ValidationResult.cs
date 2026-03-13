using System;
using System.Collections.Generic;

namespace SharedClasses
{
    /// <summary>
    /// Represents a validation error for a DXF element
    /// </summary>
    public class ValidationError
    {
        public string ElementId { get; set; }
        public string ElementType { get; set; }
        public string LayerName { get; set; }
        public string RuleName { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public ValidationSeverity Severity { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> AdditionalContext { get; set; } = new();
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    /// <summary>
    /// Represents the result of validation for one or more elements
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new();

        public void AddError(ValidationError error)
        {
            Errors.Add(error);
            IsValid = false;
        }

        public void AddError(string elementId, string elementType, string ruleName, string errorMessage, 
            ValidationSeverity severity = ValidationSeverity.Error, string errorCode = null, string layerName = null)
        {
            AddError(new ValidationError
            {
                ElementId = elementId,
                ElementType = elementType,
                RuleName = ruleName,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode ?? ruleName,
                Severity = severity,
                LayerName = layerName
            });
        }
    }
}