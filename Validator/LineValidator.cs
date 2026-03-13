using System.Collections.Generic;
using SharedClasses;
using EdmontonDrawingValidator.Model;
using System.Linq;
using System;

namespace EdmontonDrawingValidator.Validator
{
    public class LineValidator : ElementValidator
    {
        /// <summary>
        /// Minimum line length allowed (in drawing units)
        /// </summary>
        private const double MinimumLineLength = 0.001;

        /// <summary>
        /// Maximum line length allowed (in drawing units)
        /// </summary>
        private const double MaximumLineLength = 1000000;

        public override ValidationResult Validate(object element)
        {
            if (element is not LayerDataWithText line)
                return GetValidationResult(false);

            ClearErrors();

            ValidateLineProperties(line);
            ValidateLineLength(line);
            ValidateLineType(line);
            ValidateLayerAssignment(line);
            ValidateLineCoordinates(line);

            return GetValidationResult();
        }

        /// <summary>
        /// Validates basic line properties
        /// </summary>
        private void ValidateLineProperties(LayerDataWithText line)
        {
            if (line == null)
            {
                AddValidationError(
                    "UNKNOWN",
                    "Line",
                    "LineProperties",
                    "Line data is null",
                    ValidationSeverity.Critical);
                return;
            }

            if (string.IsNullOrWhiteSpace(line.LayerName))
            {
                AddValidationError(
                    "UNKNOWN",
                    "Line",
                    "LayerName",
                    "Layer name is not assigned",
                    ValidationSeverity.Error,
                    layerName: line.LayerName);
            }

            if (string.IsNullOrWhiteSpace(line.Command))
            {
                AddValidationError(
                    line.LayerName ?? "UNKNOWN",
                    "Line",
                    "Command",
                    "Command property is not set",
                    ValidationSeverity.Warning,
                    layerName: line.LayerName);
            }
        }

        /// <summary>
        /// Validates line length is within acceptable range
        /// </summary>
        private void ValidateLineLength(LayerDataWithText line)
        {
            if (line?.Lines == null || line.Lines.Count == 0)
            {
                AddValidationError(
                    line?.LayerName ?? "UNKNOWN",
                    "Line",
                    "LineLength",
                    "No line segments found in element",
                    ValidationSeverity.Warning,
                    layerName: line?.LayerName);
                return;
            }

            foreach (var lineSegment in line.Lines)
            {
                if (lineSegment == null)
                    continue;

                double length = lineSegment.Length;

                if (length < MinimumLineLength)
                {
                    AddValidationError(
                        line.LayerName ?? "UNKNOWN",
                        "Line",
                        "LineLengthTooSmall",
                        $"Line length ({length:F6}) is below minimum allowed ({MinimumLineLength})",
                        ValidationSeverity.Warning,
                        errorCode: "LINE_LENGTH_MINIMUM",
                        layerName: line.LayerName,
                        context: new Dictionary<string, object>
                        {
                                { "ActualLength", length },
                                { "MinimumAllowed", MinimumLineLength }
                        });
                }

                if (length > MaximumLineLength)
                {
                    AddValidationError(
                        line.LayerName ?? "UNKNOWN",
                        "Line",
                        "LineLengthTooLarge",
                        $"Line length ({length:F2}) exceeds maximum allowed ({MaximumLineLength})",
                        ValidationSeverity.Error,
                        errorCode: "LINE_LENGTH_MAXIMUM",
                        layerName: line.LayerName,
                        context: new Dictionary<string, object>
                        {
                                { "ActualLength", length },
                                { "MaximumAllowed", MaximumLineLength }
                        });
                }
            }
        }

        /// <summary>
        /// Validates line type is properly assigned
        /// </summary>
        private void ValidateLineType(LayerDataWithText line)
        {
            if (string.IsNullOrWhiteSpace(line.LineType))
            {
                AddValidationError(
                    line.LayerName ?? "UNKNOWN",
                    "Line",
                    "LineType",
                    "Line type is not defined",
                    ValidationSeverity.Warning,
                    layerName: line.LayerName);
                return;
            }

            // Valid DXF line types
            var validLineTypes = new[] { "Continuous", "CONTINUOUS", "Dashed", "DASHED", "Dotted", "DOTTED", "DashDot", "DASHDOT" };

            if (!validLineTypes.Contains(line.LineType, StringComparer.OrdinalIgnoreCase))
            {
                AddValidationError(
                    line.LayerName ?? "UNKNOWN",
                    "Line",
                    "LineTypeInvalid",
                    $"Line type '{line.LineType}' is not recognized",
                    ValidationSeverity.Warning,
                    errorCode: "LINE_TYPE_INVALID",
                    layerName: line.LayerName,
                    context: new Dictionary<string, object>
                    {
                            { "LineType", line.LineType },
                            { "ValidLineTypes", string.Join(", ", validLineTypes) }
                    });
            }
        }

        /// <summary>
        /// Validates layer assignment and color consistency
        /// </summary>
        private void ValidateLayerAssignment(LayerDataWithText line)
        {
            if (line == null)
                return;

            if (string.IsNullOrWhiteSpace(line.LayerName))
            {
                AddValidationError(
                    "UNKNOWN",
                    "Line",
                    "LayerAssignment",
                    "Element is not assigned to any layer",
                    ValidationSeverity.Error);
                return;
            }

            // Validate color code format if present
            if (!string.IsNullOrWhiteSpace(line.ColourCode))
            {
                if (!IsValidColorCode(line.ColourCode))
                {
                    AddValidationError(
                        line.LayerName,
                        "Line",
                        "ColorCode",
                        $"Invalid color code format: {line.ColourCode}",
                        ValidationSeverity.Warning,
                        errorCode: "INVALID_COLOR_CODE",
                        layerName: line.LayerName);
                }
            }
        }

        /// <summary>
        /// Validates line coordinates and bulge data
        /// </summary>
        private void ValidateLineCoordinates(LayerDataWithText line)
        {
            if (line?.Coordinates == null || line.Coordinates.Count == 0)
            {
                if (!line.HasBulge)
                {
                    AddValidationError(
                        line?.LayerName ?? "UNKNOWN",
                        "Line",
                        "Coordinates",
                        "No coordinates found in line element",
                        ValidationSeverity.Error,
                        layerName: line?.LayerName);
                }
                return;
            }

            // Validate each coordinate
            for (int i = 0; i < line.Coordinates.Count; i++)
            {
                var coord = line.Coordinates[i];

                if (coord == null)
                {
                    AddValidationError(
                        line.LayerName ?? "UNKNOWN",
                        "Line",
                        "CoordinateNull",
                        $"Coordinate at index {i} is null",
                        ValidationSeverity.Error,
                        layerName: line.LayerName);
                    continue;
                }

                if (double.IsNaN(coord.X) || double.IsNaN(coord.Y))
                {
                    AddValidationError(
                        line.LayerName ?? "UNKNOWN",
                        "Line",
                        "CoordinateNaN",
                        $"Coordinate at index {i} contains NaN values (X: {coord.X}, Y: {coord.Y})",
                        ValidationSeverity.Error,
                        layerName: line.LayerName);
                }

                if (double.IsInfinity(coord.X) || double.IsInfinity(coord.Y))
                {
                    AddValidationError(
                        line.LayerName ?? "UNKNOWN",
                        "Line",
                        "CoordinateInfinity",
                        $"Coordinate at index {i} contains infinite values (X: {coord.X}, Y: {coord.Y})",
                        ValidationSeverity.Error,
                        layerName: line.LayerName);
                }
            }

            // Validate bulge data if present
            if (line.HasBulge)
            {
                ValidateBulgeData(line);
            }
        }

        /// <summary>
        /// Validates bulge data for curved lines
        /// </summary>
        private void ValidateBulgeData(LayerDataWithText line)
        {
            if (line.CoordinateWithBulge?.Count > 0)
            {
                if (line.OnlyBulgeValue?.Count != line.CoordinateWithBulge.Count)
                {
                    AddValidationError(
                        line.LayerName ?? "UNKNOWN",
                        "Line",
                        "BulgeDataMismatch",
                        $"Bulge value count ({line.OnlyBulgeValue?.Count ?? 0}) does not match coordinate count ({line.CoordinateWithBulge.Count})",
                        ValidationSeverity.Error,
                        layerName: line.LayerName);
                }

                foreach (var bulge in line.OnlyBulgeValue ?? Enumerable.Empty<double>())
                {
                    if (double.IsNaN(bulge) || double.IsInfinity(bulge))
                    {
                        AddValidationError(
                            line.LayerName ?? "UNKNOWN",
                            "Line",
                            "BulgeValueInvalid",
                            $"Invalid bulge value: {bulge}",
                            ValidationSeverity.Error,
                            layerName: line.LayerName);
                    }
                }
            }
        }

        /// <summary>
        /// Validates color code format (e.g., hex or RGB)
        /// </summary>
        private bool IsValidColorCode(string colorCode)
        {
            if (string.IsNullOrWhiteSpace(colorCode))
                return false;

            // Check for hex format (#RRGGBB)
            if (colorCode.StartsWith("#") && colorCode.Length == 7)
            {
                return System.Text.RegularExpressions.Regex.IsMatch(colorCode, @"^#[0-9A-Fa-f]{6}$");
            }

            // Check for numeric color index (0-255)
            if (int.TryParse(colorCode, out int colorIndex))
            {
                return colorIndex >= 0 && colorIndex <= 255;
            }

            return false;
        }
    }
}       