using System.Collections.Generic;
using SharedClasses;
using EdmontonDrawingValidator.Model;
namespace EdmontonDrawingValidator.Validator
{
    public class CircleValidator : ElementValidator
    {
        public override ValidationResult Validate(object element)
        {
            if (element is not LayerDataWithText circle)
                return GetValidationResult(false);

            ClearErrors();

            ValidateCircleProperties(circle);
            ValidateCircleRadius(circle);
            ValidateCircleCenter(circle);

            return GetValidationResult();
        }

        private void ValidateCircleProperties(LayerDataWithText circle)
        {
            if (!circle.IsCircle)
            {
                AddValidationError(
                    circle.LayerName ?? "UNKNOWN",
                    "Circle",
                    "CircleValidation",
                    "Element marked as circle but IsCircle flag is false",
                    ValidationSeverity.Warning);
            }
        }

        private void ValidateCircleRadius(LayerDataWithText circle)
        {
            if (circle.Radius <= 0)
            {
                AddValidationError(
                    circle.LayerName ?? "UNKNOWN",
                    "Circle",
                    "CircleRadius",
                    $"Invalid circle radius: {circle.Radius}. Radius must be greater than 0.",
                    ValidationSeverity.Error);
            }
        }

        private void ValidateCircleCenter(LayerDataWithText circle)
        {
            if (circle.CenterPoint == null)
            {
                AddValidationError(
                    circle.LayerName ?? "UNKNOWN",
                    "Circle",
                    "CircleCenter",
                    "Circle center point is not defined",
                    ValidationSeverity.Error);
            }
        }
    }
}