using System.Linq;
using EdmontonDrawingValidator.Model;
using SharedClasses;

namespace EdmontonDrawingValidator.Validator
{
    public class TextValidator : ElementValidator
    {
        public override ValidationResult Validate(object element)
        {
            if (element is not LayerDataWithText textElement)
                return GetValidationResult(false);

            ClearErrors();

            ValidateTextContent(textElement);
            ValidateTextPlacement(textElement);

            return GetValidationResult();
        }

        private void ValidateTextContent(LayerDataWithText textElement)
        {
            if (textElement.TextInfoData == null || textElement.TextInfoData.Count == 0)
            {
                AddValidationError(
                    textElement.LayerName ?? "UNKNOWN",
                    "Text",
                    "TextContent",
                    "No text information found in element",
                    ValidationSeverity.Warning);
                return;
            }

            foreach (var text in textElement.TextInfoData.Where(t => string.IsNullOrWhiteSpace(t.Text)))
            {
                AddValidationError(
                    textElement.LayerName ?? "UNKNOWN",
                    "Text",
                    "TextContent",
                    "Text content is empty or whitespace",
                    ValidationSeverity.Warning);
            }
        }

        private void ValidateTextPlacement(LayerDataWithText textElement)
        {
            if (textElement.TextInfoData == null)
                return;

            foreach (var text in textElement.TextInfoData)
            {
                if (text.Coordinates == null)
                {
                    AddValidationError(
                        textElement.LayerName ?? "UNKNOWN",
                        "Text",
                        "TextPlacement",
                        "Text position is not defined",
                        ValidationSeverity.Error);
                }
            }
        }
    }
}