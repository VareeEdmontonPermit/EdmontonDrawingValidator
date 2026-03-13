using EdmontonDrawingValidator.Model;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator.Validator
{
    /// <summary>
    /// Manages validation of DXF drawing elements
    /// </summary>
    public class Unit
    {
        private readonly ElementValidator _validator;
        private readonly List<ValidationError> _sessionErrors = new();

        public Unit(ElementValidator validator = null)
        {
            _validator = validator;
        }

        /// <summary>
        /// Validates a single element using the provided or default validator
        /// </summary>
        public ValidationResult ValidateElement(object element, ElementValidator validator = null)
        {
            var v = validator ?? _validator;
            if (v == null)
                throw new ArgumentNullException(nameof(validator), "No validator provided");

            return v.Validate(element);
        }

        /// <summary>
        /// Validates multiple elements and aggregates results
        /// </summary>
        public ValidationResult ValidateElements(List<object> elements, ElementValidator validator = null)
        {
            if (elements == null || elements.Count == 0)
                return new ValidationResult { IsValid = true };

            var combinedResult = new ValidationResult { IsValid = true };

            foreach (var element in elements)
            {
                var result = ValidateElement(element, validator);
                combinedResult.Errors.AddRange(result.Errors);

                if (!result.IsValid)
                    combinedResult.IsValid = false;
            }

            return combinedResult;
        }

        /// <summary>
        /// Validates elements by type using appropriate validators
        /// </summary>
        public ValidationResult ValidateElementsByType(
            List<LayerDataWithText> layers,
            Dictionary<string, ElementValidator> validatorsByType)
        {
            if (layers == null || layers.Count == 0)
                return new ValidationResult { IsValid = true };

            var combinedResult = new ValidationResult { IsValid = true };

            foreach (var layer in layers)
            {
                // Determine element type
                string elementType = DetermineElementType(layer);

                if (validatorsByType.TryGetValue(elementType, out var validator))
                {
                    var result = ValidateElement(layer, validator);
                    combinedResult.Errors.AddRange(result.Errors);

                    if (!result.IsValid)
                        combinedResult.IsValid = false;
                }
            }

            return combinedResult;
        }

        /// <summary>
        /// Gets all errors from the current validation session
        /// </summary>
        public List<ValidationError> GetSessionErrors() => new List<ValidationError>(_sessionErrors);

        /// <summary>
        /// Clears all session errors
        /// </summary>
        public void ClearSessionErrors() => _sessionErrors.Clear();

        /// <summary>
        /// Adds an error to the session
        /// </summary>
        public void AddSessionError(ValidationError error) => _sessionErrors.Add(error);

        /// <summary>
        /// Gets error summary by severity
        /// </summary>
        public Dictionary<ValidationSeverity, int> GetErrorSummary(ValidationResult result)
        {
            return result.Errors
                .GroupBy(e => e.Severity)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        /// <summary>
        /// Gets critical and error-level issues only
        /// </summary>
        public List<ValidationError> GetCriticalIssues(ValidationResult result)
        {
            return result.Errors
                .Where(e => e.Severity == ValidationSeverity.Critical || e.Severity == ValidationSeverity.Error)
                .ToList();
        }

        /// <summary>
        /// Exports validation results to a friendly format
        /// </summary>
        public string ExportValidationReport(ValidationResult result, bool includeContext = false)
        {
            if (result.IsValid)
                return "✓ Validation passed successfully.";

            var report = new System.Text.StringBuilder();
            report.AppendLine($"Validation Report - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Status: {(result.IsValid ? "PASSED" : "FAILED")}");
            report.AppendLine($"Total Errors: {result.Errors.Count}");
            report.AppendLine();

            var summary = GetErrorSummary(result);
            report.AppendLine("Error Summary:");
            foreach (var kvp in summary.OrderByDescending(x => (int)x.Key))
            {
                report.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
            report.AppendLine();

            report.AppendLine("Errors:");
            foreach (var error in result.Errors.OrderByDescending(e => e.Severity))
            {
                report.AppendLine($"  [{error.Severity}] {error.RuleName} - {error.ErrorMessage}");
                report.AppendLine($"    Element: {error.ElementId} | Type: {error.ElementType} | Layer: {error.LayerName}");

                if (includeContext && error.AdditionalContext?.Count > 0)
                {
                    report.AppendLine("    Context:");
                    foreach (var ctx in error.AdditionalContext)
                    {
                        report.AppendLine($"      {ctx.Key}: {ctx.Value}");
                    }
                }
            }

            return report.ToString();
        }

        /// <summary>
        /// Determines the type of DXF element based on its properties
        /// </summary>
        private string DetermineElementType(LayerDataWithText layer)
        {
            if (layer == null)
                return "Unknown";

            if (layer.IsCircle)
                return "Circle";

            if (layer.HasBulge)
                return "ArcLine";

            if (layer.Lines?.Count > 0)
                return "Line";

            if (layer.TextInfoData?.Count > 0)
                return "Text";

            return "Unknown";
        }
    }
}
