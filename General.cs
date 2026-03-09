using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using EdmontonDrawingValidator.Model;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;
using SharedClasses;
using SharedClasses.Constants;


namespace EdmontonDrawingValidator
{
    public class General
    {
        public static bool DebugLogWithDataEnabled = false;

        public static string LogFile = "";
        public static string AppName { get; set; } = "Instance name missing";

        public static readonly string ContactMessage = "Failed to extract data from DWG file, Please contact support@dcpplanning.com.";
        public bool CheckLineTypeIsCenterLine(string sLineType)
        {
            if (!string.IsNullOrWhiteSpace(sLineType) && (sLineType.Trim().ToUpper().Equals(DxfLayersName.CenterLineCode) || sLineType.Trim().ToUpper().Equals(DxfLayersName.CenterLineCode2)))
                return true;

            return false;
        }
        public bool CheckLineTypeIsDashedLine(string sLineType)
        {
            if (!string.IsNullOrWhiteSpace(sLineType) && sLineType.Trim().ToUpper().Equals(DxfLayersName.DashedLine))
                return true;

            return false;
        }
        public static string CreateDrawingFileYesNo { get; set; }
        public static string JsonDataFolder { get; set; }
        public static string InputFolder { get; set; }
        public static string RuleTesterInputFolder { get; set; }
        public static string InputFileExtension { get; set; }

        private static readonly int DefaultNumberOfThreadToStart = 5;

        private static readonly int DefaultSleepMS = 10000;
        public static string NoOfThreadsToStart { get; set; }
        public static string SleepTimeInMS { get; set; }
        public static string RuleTesterBaseAPIUrl
        {
            get; set;
        }

        public static string RulesCheckingStatusUpdateURL
        {
            get; set;
        }

        public static string AddBuildingURL
        {
            get; set;
        }

        private static string errorHtmlDrawingTemplateData = null;
        public static string GetErrorHtmlDrawingTemplateData
        {
            get
            {
                if (string.IsNullOrWhiteSpace(errorHtmlDrawingTemplateData))
                {
                    if (!string.IsNullOrWhiteSpace(errorHtmlDrawingTemplatePath) && File.Exists(errorHtmlDrawingTemplatePath))
                        errorHtmlDrawingTemplateData = File.ReadAllText(errorHtmlDrawingTemplatePath);
                }

                return errorHtmlDrawingTemplateData;
            }
        }

        public static string[] GetAllBuiltupLineList = new string[] { DxfLayersName.CommercialBuiltUpLine, DxfLayersName.ResidentBuiltUpLine, DxfLayersName.IndustrialBuiltUpLine, DxfLayersName.SpecialUseBuiltUpLine };

        public static string errorHtmlDrawingTemplatePath { get; set; }
        public static string svgScale { get; set; }
        //public static string DisclaimerTextPositionValue { get; set; }
        //public static int DisclaimerTextPosition
        //{
        //    get
        //    {
        //        try
        //        {
        //            if (string.IsNullOrWhiteSpace(DisclaimerTextPositionValue) == false)
        //                return int.Parse(DisclaimerTextPositionValue.Trim());
        //        }
        //        catch { }
        //        return 10;
        //    }
        //}
        public bool IsNumeric(String s)
        {
            try
            {
                double d = double.Parse(s);
                return true;
            }
            catch { }
            return false;
        }

        public static int SVGScalingNumber
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(svgScale))
                {
                    try
                    {
                        return int.Parse(svgScale);
                    }
                    catch { }
                }

                return 50;
            }
        }
        public static int WorkerThreadCount
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(NoOfThreadsToStart))
                        return DefaultNumberOfThreadToStart;

                    return int.Parse(NoOfThreadsToStart.Trim());
                }
                catch
                {
                    return DefaultNumberOfThreadToStart;
                }
            }
        }
        public static int SleepTimeMs
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(SleepTimeInMS))
                        return DefaultSleepMS;

                    return int.Parse(SleepTimeInMS.Trim());
                }
                catch
                {
                    return DefaultSleepMS;
                }
            }
        }
        public static bool IsCreateDrawingDataFile
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(CreateDrawingFileYesNo))
                        return false;

                    if (CreateDrawingFileYesNo.ToLower().Trim().Equals("yes"))
                        return true;
                }
                catch
                {

                }
                return false;
            }
        }
        public static string geometryPrecision { get; set; } = "";
        public static double GeometryPrecision
        {
            get
            {
                try
                {
                    return double.Parse(geometryPrecision.Trim());
                }
                catch
                {
                    return double.Parse("10000");
                }
            }
        }

        /// <summary>
        /// Regex(@"typical", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexTypicalFloor = new Regex(@"typical", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"<(body)[^>]*>(.*?)</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexExtractBodyContent = new Regex(@"<(body)[^>]*>(.*?)</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"\\f.*?;", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexClearText = new Regex(@"\\[a-z]+.*?;", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex.Match(sLiftName, @"(\d+)\s*Person", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexPassengerLiftCapacity = new Regex(@"(\d+)\s*Person", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"V\W*e\W*h\W*i\W*c\W*u\W*l\W*a\W*r|c\W*a\W*r", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexVehicularLiftText = new Regex(@"V\W*e\W*h\W*i\W*c\W*u\W*l\W*a\W*r|c\W*a\W*r", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"p\W*a\W*s\W*s\W*e\W*n\W*g\W*e\W*r|c\W*a\W*p\W*a\W*c\W*i\W*t\W*y", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexPassengerLiftText = new Regex(@"p\W*a\W*s\W*s\W*e\W*n\W*g\W*e\W*r|c\W*a\W*p\W*a\W*c\W*i\W*t\W*y|p\W*e\W*r\W*s\W*o\W*n", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"f\W*i\W*r\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexFireLiftText = new Regex(@"f\W*i\W*r\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// "Building {0}";
        /// </summary>
        public static readonly string BuildingText = "Building {0}";

        /// <summary>
        /// "Floor {0}";
        /// </summary>
        public static readonly string FloorText = "Floor {0}";

        /// <summary>
        /// "UNIT {0}";
        /// </summary>
        public static readonly string UnitBuaText = "Unit {0}";

        /// <summary>
        /// "Room {0}";
        /// </summary>
        public static readonly string RoomText = "Room {0}";

        /// <summary>
        /// "Road {0}";
        /// </summary>
        public static readonly string RoadText = "Road {0}";

        /// <summary>
        /// "Common Plot {0}";
        /// </summary>
        public static readonly string CommonPlotText = "Common Plot {0}";

        /// <summary>
        /// "Plot {0}";
        /// </summary>
        public static readonly string PlotText = "Plot {0}";

        /// <summary>
        /// "Proposed work {0}";
        /// </summary>
        public static readonly string ProposedText = "Proposed work {0}";

        /// <summary>
        /// "Basement {0}";
        /// </summary>
        public static readonly string BasementText = "Basement {0}";

        /// <summary>
        /// "Section {0}";
        /// </summary>
        public static readonly string SectionText = "Section {0}";

        /// <summary>
        /// "Floor in section {0}";
        /// </summary>
        public static readonly string FloorInSectionText = "Floor in section {0}";

        /// <summary>
        /// "WithInMarginPropose {0}";
        /// </summary>
        public static readonly string WithInMarginPropose = "WithInMarginPropose {0}";

        /// <summary>
        /// new Regex(@"o\W*(?:h\W*)?w\W*t|o\W*v\W*e\W*r\W*h\W*e\W*a\W*d\W*w\W*a\W*t\W*e\W*r\W*t\W*a\W*n\W*k", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexOverHeadWaterTankOnlyText = new Regex(@"o\W*(?:h\W*)?w\W*t|o\W*v\W*e\W*r\W*h\W*e\W*a\W*d\W*w\W*a\W*t\W*e\W*r\W*t\W*a\W*n\W*k", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"\\\s*(?:pxqc|pxql|pxqr)\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary> 
        public static readonly Regex regexTextOfDataClear = new Regex(@"\\\s*(?:pxqc|pxql|pxqr)\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"  +", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary> 
        public static readonly Regex regexMultipleSpace = new Regex(@"  +", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// \\(?:pxqc|pxql|pxqr) 
        /// Text alignment \pxqc - Center, \pxql - Left, \pxqr - Right
        /// </summary> 
        public static readonly Regex regexRemoveSpecialText = new Regex(@"\s*\\(?:[a-zA-Z0-9]+){1,7};\s*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary> 
        public static readonly Regex regexRemoveSpecialCharacterText = new Regex(@"\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"Tele|TDB|Toilet|TOI|(?:Water|Servant|Store|Security|Electric)\W*room|Security\W*cabin|Entrance\W*Foyer|Foyer.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        public static readonly Regex regexAllowRoomInParkingArea = new Regex(@"Tele|TDB|Toilet|TOI|Meter|(?:Water|Servant|Store|Security|Electric)\W*room|Security\W*cabin|Entrance\W*Foyer|Ent\W*Foyer|Foyer|W\W*C.*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// new Regex(@"^\s*(?:Entrance\W*Foyer|Ent\W*Foyer|Foyer|Entrance\W*lobby|Ent\W*lobby|lobby).*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        public static readonly Regex regexAllowAccessoryInParking = new Regex(@"^\s*(?:Entrance\W*Foyer|Ent\W*Foyer|Foyer|Entrance\W*lobby|Ent\W*lobby|lobby).*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// new Regex(@"(?:P\W*u\W*m\W*p|T\W*r\W*a\W*n\W*s\W*f\W*o\W*r\W*m\W*e\W*r|M\W*e\W*t\W*e\W*r|M\W*T\W*|M\W*|A\W*C\W*Plant)\W*room|Parking\s*Garage", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        //public static readonly Regex regexInvalidAccessoryInPlot = new Regex(@"(?:P\W*u\W*m\W*p|T\W*r\W*a\W*n\W*s\W*f\W*o\W*r\W*m\W*e\W*r|M\W*e\W*t\W*e\W*r|M\W*T\W*|M\W*|A\W*C\W*Plant|Generator|Office)\W*room|Parking\s*Garage", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        //public static readonly Regex regexInvalidAccessoryInPlot = new Regex(@"(?:P\W*u\W*m\W*p|T\W*r\W*a\W*n\W*s\W*f\W*o\W*r\W*m\W*e\W*r|M\W*e\W*t\W*e\W*r|M\W*T\W*|M\W*|A\W*C\W*Plant|servant|security|Generator|storage|office|sec)\W*(?:room|quarter|Office|cabin)?|Parking\s*Garage", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        public static readonly Regex regexInvalidAccessoryInPlot = new Regex(@"(?:P\W*u\W*m\W*p|T\W*r\W*a\W*n\W*s\W*f\W*o\W*r\W*m\W*e\W*r|M\W*e\W*t\W*e\W*r|M\W*T\W*|\b\W*M\W*\b|A\W*C\W*Plant|servant|security|generator|storage|office|sec)\W*(?:room|quarter|Office|cabin)?|Parking\s*Garage", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// new Regex(@"\b(?:solar\s*water\s*(?:heating)?|s\W*w\W*h\W*s|s\W*w\W*h|h\s*e\s*a\s*t)\W*\b|solar", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static Regex regexSolarWaterHeating = new Regex(@"\b(?:solar\s*water\s*(?:heating)?|s\W*w\W*h\W*s|s\W*w\W*h|h\s*e\s*a\s*t)\W*\b|solar", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"^\s*storage\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        public static readonly Regex regexStorageText = new Regex(@"^\s*storage\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        // Regex(@"c\s*a\s*n\s*o\s*p\s*y|f\s*u\s*e\s*l\s*i\s*n\s*g\W*p\s*e\s*d\s*e\s*s\s*t\s*a\s*l", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        ///new Regex(@"c\s*a\s*n\s*o\s*p\s*y|c\s*u\s*r\s*b", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        //public static readonly Regex regexFilterAccessoryInPlot = new Regex(@"c\s*a\s*n\s*o\s*p\s*y|f\s*u\s*e\s*l\s*i\s*n\s*g\W*p\s*e\s*d\s*e\s*s\s*t\s*a\s*l", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        public static readonly Regex regexFilterAccessoryInPlot = new Regex(@"c\s*a\s*n\s*o\s*p\s*y|c\s*u\s*r\s*b", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        // Regex(@"f\s*u\s*e\s*l\s*i\s*n\s*g\W*p\s*e\s*d\s*e\s*s\s*t\s*a\s*l", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// new Regex(@"c\s*u\s*r\s*b", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        //public static readonly Regex regexFilterAccessoryFuelingPedestalInPlot = new Regex(@"f\s*u\s*e\s*l\s*i\s*n\s*g\W*p\s*e\s*d\s*e\s*s\s*t\s*a\s*l", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        public static readonly Regex regexFilterAccessoryFuelingPedestalInPlot = new Regex(@"c\s*u\s*r\s*b", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Regex(@"(?:f\s*u\s*e\s*l\s*i\s*n\s*g|ev)\s*s\s*t\s*a\s*t\s*i\s*o\s*n\s*c\s*a\s*n\s*o\s*p\s*y", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        public static readonly Regex regexFilterAccessoryInSection = new Regex(@"(?:f\s*u\s*e\s*l\s*i\s*n\s*g|ev)\s*s\s*t\s*a\s*t\s*i\s*o\s*n\s*c\s*a\s*n\s*o\s*p\s*y", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// new Regex(@"(^\s*typical\W*(1\W*|1\s*st|first)\W*floor|^\s*(1\W*|1\s*st|first)\W*floor)", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        //public static readonly Regex regexFirstFloor = new Regex(@"(^\s*typical\W*(1\W*|1\s*st|first)\W*floor|^\s*(1\W*|1\s*st|first)\W*floor\W*(1\W*|1\s*st|first))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"\W*(?:typical)?\W*(?:1\W*|1\s*st|first|floor)\W*(?:1\W*|1\s*st|first|floor)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexFirstFloorText = new Regex(@"\W*(?:typical)?\W*(?:1\W*|1\s*st|first|floor)\W*(?:typical)?\W*(?:1\W*|1\s*st|first|floor)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(platform)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexRampPlatform = new Regex(@"(platform)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"t\s*r\s*e\s*e.*?(\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexTree = new Regex(@"^\s*t\s*r\s*e\s*e.*?(\d+).*?$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// new Regex(@"(slop)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexRampSlop = new Regex(@"(slop)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(slop)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexRampPlatformAndSlop = new Regex(@"(platform|slop)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        ///<summary>
        ///(?:car|truck|hadicap|pedestrian|(?:2|two)\W*wheel).*?ramp
        ///</summary>
        public static readonly Regex regexRampValidText = new Regex(@"(?:car|truck|handicap|pedestrian|(?:2|two)\W*wheel).*?ramp", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        ///<summary>
        ///(?:handicap|pedestrian)
        ///</summary>
        public static readonly Regex regexHandicapPedestrianRampValidText = new Regex(@"(?:handicap|pedestrian)", RegexOptions.IgnoreCase | RegexOptions.Singleline);


        /// <summary>
        /// new Regex(@"basement", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexBasementFloor = new Regex(@"basement", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"^\s*(?:(?:first|1|I)\W*basement|basement\W*(?:first|1|I)|basement\W*floor)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexOnlyBasementFirstFloorText = new Regex(@"^\s*(?:(?:first|1|I)\W*(?:st)?\W*basement|basement\W*(?:first|1|I)\W*(?:st)?|basement\W*floor)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //public static readonly Regex regexOnlyBasementFirstFloorText = new Regex(@"^\s*(?:(?:first|1|I)\W*basement|basement\W*(?:first|1|I)|basement\W*floor)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"service", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexServiceFloor = new Regex(@"service", RegexOptions.IgnoreCase | RegexOptions.Singleline);


        /// <summary>
        /// new Regex(@"terrace", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexTerraceFloor = new Regex(@"terrace", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(ground|stilt)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexGroundFloor = new Regex(@"(ground|stilt)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //public static readonly Regex regexGroundFloor = new Regex(@"ground", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(ground|stilt)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexGroundStiltFloor = new Regex(@"(ground|stilt)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// podium
        /// </summary>
        public static readonly Regex regexPodium = new Regex(@"\bp\W*o\W*d\W*i\W*u\W*m\W*\b", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"s\W*t\W*i\W*l\W*t", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexStiltFloor = new Regex(@"s\W*t\W*i\W*l\W*t", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //public static readonly Regex regexGroundFloor = new Regex(@"ground", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"r\W*e\W*f\W*u\W*g\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexRefugeFloor = new Regex(@"r\W*e\W*f\W*u\W*g\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"parking", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexParkingFloorName = new Regex(@"parking", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        ///// <summary>
        ///// new Regex(@"l\W*o\W*f\W*t", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        ///// </summary>
        //public static readonly Regex regexLoft = new Regex(@"l\W*o\W*f\W*t", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"g\W*a\W*r\W*a\W*g\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexGarage = new Regex(@"g\W*a\W*r\W*a\W*g\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"raised\s*COP\s*@\s*(\d*.?(:?\d+)?)\s*(?:meter|mt|m\W*t)\W*height", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexRaisedCOPText = new Regex(@"raised\s*COP\s*@\s*(\d*.?(:?\d+)?)\s*(?:meter|mt|m\W*t)\W*height", RegexOptions.IgnoreCase | RegexOptions.Singleline);


        /// <summary>
        /// *****  INVALID
        /// </summary>
        public const string InvalidMark = "*****  INVALID";

        /// <summary>
        /// *****  VALID
        /// </summary>
        public const string ValidMark = "*****  VALID";

        /// <summary>
        /// @"\s*C\s*P\W+C"
        /// </summary>
        public const string regexCommercialCarParking = @"\s*C\s*P\W+C";

        /// <summary>
        /// @"\s*V\s*P\W+C"
        /// </summary>
        public const string regexCommercialVisitorParking = @"\s*V\s*P\W+C";

        /// <summary>
        /// @"\s*C\s*P\W+R"
        /// </summary>
        public static readonly string regexResidentialCarParking = @"\s*C\s*P\W+R";

        /// <summary>
        /// @"\s*V\s*P\W+R"
        /// </summary>
        public static readonly string regexResidentialVisitorParking = @"\s*V\s*P\W+R";

        /// <summary>
        /// @"\s*V\s*P\W+R"
        /// </summary>
        public static readonly string regexTwoVehicleParking = @"\s*T\s*W|P\W+R";

        /// <summary>
        ///  @"\s*L\s*D\W+C"
        /// </summary>
        public const string regexCommercialLoadingUnloadingParking = @"\s*L\s*D\W+C";
        //public static readonly string regexResidentialLoadingUnloadingParking = @"\s*L\s*D\W+C";

        /// <summary>
        /// new Regex(@"\W+R\s*\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexResidentialParking = new Regex(@"\W+R\s*\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"\W+C\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexCommercialParking = new Regex(@"\W+C\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"\W+I\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexIndustrialParking = new Regex(@"\W+I\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"\W+S\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexSpecialParking = new Regex(@"\W+S\W+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        //public static readonly Regex ParkingNamingRegex = new Regex(@"(?:(?:C|R|T|V)\W*(?:P|W|L\W*D))\W*(?:C|R|I)\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// <summary>
        /// new Regex(@"(?:(?:C|R|T|V|L|U|S)\W*(?:P|W|D))\W*(?:C|R|I|S)\W*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexParkingNaming = new Regex(@"(?:(?:C|R|T|V|L|U|S)\W*(?:P|W|D))\W*(?:C|R|I|S)\W*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:car|(?:two|2)\W*stacks?\W*(?:parking)?|(?:C|R|T|V|L|U|S)\W*(?:P|W|D)\W*(?:C|R|I|S)\W*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexParkingNamingValidation = new Regex(@"(?:(?:two|2)\W*stacks?\W*(?:parking)?|(?:C|R|T|V|L|U|S)\W*(?:P|W|D)\W*(?:C|R|I|S)\W*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //public static readonly Regex regexParkingNamingValidation = new Regex(@"(?:car|(?:two|2)\W*stacks?\W*(?:parking)?|(?:C|R|T|V|L|U|S)\W*(?:P|W|D)\W*(?:C|R|I|S)\W*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:\d+\.?\d*\W*(?:M\W*T|m\W*e\W*t\W*e\W*r|m)\W+(?:[w\W*d|W\W*i\W*d\W*e]\W*){1,}\W*){1,}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        // (?:\d+\.?\d*\W*(?:M\W*T|m\s*e\s*t\s*e\s*r|m)\W+(?:[w\s*d|W\s*i\s*d\s*e]\s*){1,}\W*){1,}
        //public static readonly Regex regexRoadScaleText = new Regex(@"(?:\d+\.?\d*\W*(?:M\W*T|m\W*e\W*t\W*e\W*r|m)\W+(?:[w\W*d|W\W*i\W*d\W*e]\W*){1,}\W*){1,}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //@"(?:\d+\.?\d*\W*(?:M\W*T|m\W*e\W*t\W*e\W*r|m)\W*(?:[w\W*d|W\W*i\W*d\W*e]\W*){1,}\W*){1,}"
        public static readonly Regex regexRoadScaleText = new Regex(@"(?:\d+\.?\d*\W*(?:M\W*T|m\W*e\W*t\W*e\W*r|m)\W*(?:w\W*d|W\W*i\W*d\W*e)\W*){1,}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"demolish", RegexOptions.IgnoreCase);
        /// </summary>
        public static readonly Regex regexDemolishText = new Regex(@"demolish", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"\W*Internal|ODP|NH|SH\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexMainRoadNameText = new Regex(@"\W*Internal|canal|ODP|NH|SH\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        //new Regex(@"(?:\d+\.?\d*\W*(?:MT|meter|m)\W+(?:[wide|W\s*i\s*d\s*e|road|r\s*o\s*a\s*d|a\s*p\s*p\s*r\s*o\s*a\s*c\s*h|Approved|Approve|A\s*p\s*p\s*r\s*o\s*v\s*e\s*d|A\s*p\s*p\s*r\s*o\s*v\s*e|approach|road|r\s*o\s*a\s*d|TP|T\W*p\W*| |\n|internal|main]+\s*){1,}\W*){1,}", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        //public static readonly Regex regexRoadScaleText = new Regex(@"(?:\d+\.?\d*\W*(?:MT|meter|m)\W+(?:[wide|W\s*i\s*d\s*e|road|r\s*o\s*a\s*d|a\s*p\s*p\s*r\s*o\s*a\s*c\s*h|Approved|Approve|A\s*p\s*p\s*r\s*o\s*v\s*e\s*d|A\s*p\s*p\s*r\s*o\s*v\s*e|approach|road|r\s*o\s*a\s*d|TP|T\W*p\W*| |\n|internal|main]+\s*){1,}\W*){1,}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:\d+\.?\d*)\W*(?:MT|M\W*T\W*|meter|m)\W+(?:wide|W\s*i\s*d\s*e)?\s*(?:d\W*r\W*i\W*v\W*e\W*w\W*a\W*y\s*)", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexDriveWayText = new Regex(@"(?:\d+\.?\d*)\W*(?:MT|M\W*T\W*|meter|m)\W+(?:wide|W\s*i\s*d\s*e)?\s*(?:d\W*r\W*i\W*v\W*e\W*w\W*a\W*y\s*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:Telephone|Tele|T\W*D\W*B|Telephone\s*Destribution\s*Board)\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexTelephoneText = new Regex(@"(?:Telephone|Tele|T\W*D\W*B|Telephone\s*Distribution\s*Board)\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:Telephone|Tele|T\W*D\W*B|Telephone\s*Destribution\s*Board)\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexTelephoneText1 = new Regex(@"(?:Telephone|Tele|T\W*D\W*B|Telephone\s*Distribution\s*Board)\W*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:\d+\.?\d*\W*(?:M\W*T\W*|MT|meter|m)\W+(?:[wide|W\s*i\s*d\s*e|passage|p\s*a\s*s\s*s\s*a\s*g\s*e|corridor|c\s*o\s*r\s*r\s*i\s*d\s*o\s*r]+\s*){1,}\W*){1,}", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        // (?:\d+\.?\d*\W*(?:M\W*T\W*|MT|meter|m)\W+(?:[wide|W\s*i\s*d\s*e|passage|p\s*a\s*s\s*s\s*a\s*g\s*e|corridor|c\s*o\s*r\s*r\s*i\s*d\s*o\s*r]+\s*){1,}\W*){1,}"
        public static readonly Regex regexPassageScaleText = new Regex(@"(?:\d+\.?\d*\W*(?:M\W*T\W*|MT|meter|m)\W+(?:(?:wide|W\s*i\s*d\s*e|passage|p\s*a\s*s\s*s\s*a\s*g\s*e|corridor|c\s*o\s*r\s*r\s*i\s*d\s*o\s*r){1,}\s*){1,}\W*){1,}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex regexRoadScaleExtract = new Regex(@"(\d+\.?\d*)\W*(?:M\W*T\W*|MT|meter|m)?", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:\d+\.?\d*\W*(?:M\W*T\W*|MT|meter|m)\W+(?:[wide|W\s*i\s*d\s*e|passage|p\s*a\s*s\s*s\s*a\s*g\s*e|corridor|c\s*o\s*r\s*r\s*i\s*d\s*o\s*r]+\s*){1,}\W*){1,}", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexPassageText = new Regex(@"(passage|p\s*a\s*s\s*s\s*a\s*g\s*e|corridor|c\s*o\s*r\s*r\s*i\s*d\s*o\s*r)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:O\W*T\W*S\W*|v\W*o\W*i\W*d|c\W*u\W*t\W*o\W*u\W*t|d\W*o\W*u\W*b\W*l\W*e\W*h\W*e\W*i\W*g\W*h\W*t)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexOTSText = new Regex(@"(?:O\W*T\W*S\W*|v\W*o\W*i\W*d|c\W*u\W*t\W*o\W*u\W*t|d\W*o\W*u\W*b\W*l\W*e\W*h\W*e\W*i\W*g\W*h\W*t|d\W*u\W*c\W*t)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:O\W*T\W*S\W*|v\W*o\W*i\W*d|c\W*u\W*t\W*o\W*u\W*t)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexDoubleHeightText = new Regex(@"(?:d\W*o\W*u\W*b\W*l\W*e\W*h\W*e\W*i\W*g\W*h\W*t)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:w\W*a\W*t\W*e\W*r\W*(?:r\W*o\W*o\W*m)?|W\W*R)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexWaterRoomText = new Regex(@"(?:w\W*a\W*t\W*e\W*r\W*(?:r\W*o\W*o\W*m)?|W\W*R)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:s\W*e\W*r\W*v\W*a\W*n\W*t(?:r\W*o\W*o\W*m)?|S\W*R)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexServantRoomText = new Regex(@"(?:s\W*e\W*r\W*v\W*a\W*n\W*t\W*(?:r\W*o\W*o\W*m)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:s\W*t\W*o\W*r\W*e\W*(?:r\W*o\W*o\W*m)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexStoreRoomText = new Regex(@"(?:s\W*t\W*o\W*r\W*e\W*(?:r\W*o\W*o\W*m)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:s\W*e\W*c\W*u\W*r\W*i\W*t\W*y\W*(?:r\W*o\W*o\W*m|c\W*a\W*b\W*i\W*n)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexSecurityRoomOrCabinText = new Regex(@"(?:s\W*e\W*c\W*u\W*r\W*i\W*t\W*y\W*(?:r\W*o\W*o\W*m|c\W*a\W*b\W*i\W*n)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"(?:e\W*l\W*e\W*c\W*t\W*r\W*i\W*c|e\W*l\W*e\W*c|e\W*l\W*e)\W*(?:r\W*o\W*o\W*m|c\W*a\W*b\W*i\W*n)?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexElectricRoomText = new Regex(@"(?:e\W*l\W*e\W*c\W*t\W*r\W*i\W*c|e\W*l\W*e\W*c|e\W*l\W*e)\W*(?:r\W*o\W*o\W*m|c\W*a\W*b\W*i\W*n)?", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"F\W*o\W*y\W*e\W*r|E\W*n\W*t\W*r\W*a\W*n\W*c\W*e\W*F\W*o\W*y\W*e\W*r|E\W*n\W*t\W*F\W*o\W*y\W*e\W*r|E\W*n\W*t\W*r\W*a\W*n\W*c\W*e\W*l\W*o\W*b\W*b\W*y|E\W*n\W*t\W*l\W*o\W*b\W*b\W*y|l\W*o\W*b\W*b\W*y", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexEntranceFoyerText = new Regex(@"F\W*o\W*y\W*e\W*r|E\W*n\W*t\W*r\W*a\W*n\W*c\W*e\W*F\W*o\W*y\W*e\W*r|E\W*n\W*t\W*F\W*o\W*y\W*e\W*r|E\W*n\W*t\W*r\W*a\W*n\W*c\W*e\W*l\W*o\W*b\W*b\W*y|E\W*n\W*t\W*l\W*o\W*b\W*b\W*y|l\W*o\W*b\W*b\W*y", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"(?:A\W*c|w\W*a\W*t\W*e\W*r|s\W*e\W*w\W*a\W*g\W*e)\W*plant|w\W*a\W*t\W*e\W*r\W*tank|s\W*t\W*o\W*r\W*a\W*g\W*e|s\W*a\W*f\W*e\W*d\W*e\W*p\W*o\W*s\W*i\W*t\W*v\W*a\W*u\W*l\W*t|g\W*r\W*e\W*y\W*w\W*a\W*t\W*e\W*r\W*t\W*r\W*e\W*a\W*t\W*m\W*e\W*n\W*t\W*p\W*l\W*a\W*n\W*t.*?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexFirstBasementAccessoryNameForBuiltup = new Regex(@"(?:A\W*c|w\W*a\W*t\W*e\W*r|s\W*e\W*w\W*a\W*g\W*e)\W*plant|w\W*a\W*t\W*e\W*r\W*tank|s\W*t\W*o\W*r\W*a\W*g\W*e|s\W*a\W*f\W*e\W*d\W*e\W*p\W*o\W*s\W*i\W*t\W*v\W*a\W*u\W*l\W*t|g\W*r\W*e\W*y\W*w\W*a\W*t\W*e\W*r\W*t\W*r\W*e\W*a\W*t\W*m\W*e\W*n\W*t\W*p\W*l\W*a\W*n\W*t.*?$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"\s*(?:t\W*h\W*e\W*r\W*a\W*p\W*y)\W*(?:d\W*e\W*v\W*i\W*c\W*e|r\W*o\W*o\W*m)?|(?:R\W*a\W*d\W*i\W*a\W*t\W*i\W*o\W*n)\W*(?:t\W*h\W*e\W*r\W*a\W*p\W*y)?\W*(?:d\W*e\W*v\W*i\W*c\W*e|r\W*o\W*o\W*m)?|(?:M\W*R\W*I|X\W*r\W*a\W*y)\W*(?:r\W*o\W*o\W*m)?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexFirstBasementRoomNameForBuiltup = new Regex(@"\s*(?:t\W*h\W*e\W*r\W*a\W*p\W*y)\W*(?:d\W*e\W*v\W*i\W*c\W*e|r\W*o\W*o\W*m)?|(?:R\W*a\W*d\W*i\W*a\W*t\W*i\W*o\W*n)\W*(?:t\W*h\W*e\W*r\W*a\W*p\W*y)?\W*(?:d\W*e\W*v\W*i\W*c\W*e|r\W*o\W*o\W*m)?|(?:M\W*R\W*I|X\W*r\W*a\W*y)\W*(?:r\W*o\W*o\W*m)?", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:T\W*o\W*i\W*(?:l\W*e\W*t)?|W\W*C)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexToiletText = new Regex(@"(?:T\W*o\W*i\W*(?:l\W*e\W*t)?|W\W*C)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:d\W*o\W*o\W*r|M\W*D|m\W*a\W*i\W*n\W*d\W*o\W*o\W*r", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        //public static readonly Regex regexDoorText = new Regex(@"(?:d\W*o\W*o\W*r|M\W*D|m\W*a\W*i\W*n\W*d\W*o\W*o\W*r", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static readonly Regex regexOnlyDoorText = new Regex(@"(?:d(?:oor)?\W*\d*)|e\W*x\W*i\W*t\W*d\W*o\W*o\W*r|e\W*x\W*i\W*t|M\W*D\W*\d*|m\W*a\W*i\W*n\W*d\W*o\W*o\W*r\W*\d*|(?:fire|rolling|shutter)\W*(?:shutter|door)?|(?:F|L|R)\W*(?:D|S)\W*|entry|entrance|main\s*door", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"(?:w\W*i\W*n\W*d\W*o\W*w\W*\d*|w\W*\d*)", RegexOptions.IgnoreCase)
        /// </summary>
        public static readonly Regex regexOnlyWindowText = new Regex(@"(?:w\W*i\W*n\W*d\W*o\W*w\W*\d*|f\W*w\W*\d*|w\W*\d*)", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"(?:v\W*e\W*n\W*t\W*i\W*l\W*a\W*t\W*i\W*o\W*\d*|v\W*\d*)", RegexOptions.IgnoreCase)
        /// </summary>
        public static readonly Regex regexOnlyVentilationText = new Regex(@"(?:v\W*e\W*n\W*t\W*i\W*l\W*a\W*t\W*i\W*o\W*n\W*\d*|v\W*\d*)", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"(?:v\W*o\W*i\W*d)", RegexOptions.IgnoreCase);
        /// </summary> 
        public static readonly Regex regexOnlyVoidText = new Regex(@"(?:v\W*o\W*i\W*d)", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"(?:v\W*o\W*i\W*d\W*\d*", RegexOptions.IgnoreCase)
        /// </summary> 
        public static readonly Regex regexOnlyCutoutText = new Regex(@"(?:c\W*u\W*t\W*o\W*u\W*t)", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"(?:o\W*t\W*s)", RegexOptions.IgnoreCase);
        /// </summary> 
        public static readonly Regex regexOnlyOTSText = new Regex(@"(?:o\W*t\W*s)", RegexOptions.IgnoreCase);
        
        /// <summary>
        /// new Regex(@"(?:(?:ventilation|vent|v)\W*shafts?)", RegexOptions.IgnoreCase);
        /// </summary> 
        //public static readonly Regex regexOnlyOTSVShaftText = new Regex(@"(?:(?:ventilation|vent|v)\W*shafts?)", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"(?:double|doubl|dbl|DBLE|DUBL|dl)\W*(?:height|ht|hgt|hight|high)", RegexOptions.IgnoreCase);
        /// </summary> 
        public static readonly Regex regexOnlyDoubleHeightOTSText = new Regex(@"(?:double|doubl|dbl|DBLE|DUBL|dl)\W*(?:height|ht|hgt|hight|high)", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"(?:stair|s\W*t\W*a\W*i\W*r)?\s*(?:\W*c\W*a\W*s\W*e|c\W*a\W*b\W*i\W*n|l\s*o\s*b\s*b\s*y)", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexStairCaseText = new Regex(@"(?:stair|s\W*t\W*a\W*i\W*r)?\s*(?:\W*c\W*a\W*s\W*e|c\W*a\W*b\W*i\W*n|l\s*o\s*b\s*b\s*y)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"c\W*a\W*b\W*i\W*n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexStairCaseCabinText = new Regex(@"c\W*a\W*b\W*i\W*n", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        ///Existing Staircase - R", "Existing Staircase - NR", "Existing Staircase - Fire
        /// <summary>
        /// Regex(@"Existing\W*Staircase\W+(:?Fire|NR|R)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexExStairValidText = new Regex(@"Existing\W*Staircase\W+(:?Fire|NR|R)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"Existing\W*Staircase\W+Fire", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexExFireStairValidText = new Regex(@"Existing\W*Staircase\W+Fire", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"Existing\W*Staircase\W+NR", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexExNonResidentStairValidText = new Regex(@"Existing\W*Staircase\W+NR", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"Existing\W*Staircase\W+R", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexExResidentStairValidText = new Regex(@"Existing\W*Staircase\W+R", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"l\s*o\s*b\s*b\s*y", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexOnlyLobbyText = new Regex(@"l\s*o\s*b\s*b\s*y", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:window|ventilation|W|v)\W*\d*", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexWindowVentilationText = new Regex(@"(?:window|ventilation|F\s*W|W|v)\W*\d*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:slop|slope|s\W*l\W*o\W*p|s\W*l\W*o\W*p\W*e)", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexSlopText = new Regex(@"(?:slop|slope|s\W*l\W*o\W*p|s\W*l\W*o\W*p\W*e)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:loft|l\W*o\W*f\W*t)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexLoftText = new Regex(@"(?:loft|l\W*o\W*f\W*t)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:pedestrian|p\s*e\s*d\s*e\s*s\s*t\s*r\s*i\s*a\s*n|handicap|h\s*a\s*n\s*d\s*i\s*c\s*a\s*p|slop|slope|s\W*l\W*o\W*p|s\W*l\W*o\W*p\W*e)", RegexOptions.IgnoreCase | RegexOptions.Singleline);  
        /// </summary>
        public static readonly Regex regexIgnoreOtherRampText = new Regex(@"(?:pedestrian|p\s*e\s*d\s*e\s*s\s*t\s*r\s*i\s*a\s*n|handicap|h\s*a\s*n\s*d\s*i\s*c\s*a\s*p|slop|slope|s\W*l\W*o\W*p|s\W*l\W*o\W*p\W*e)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:pedestrian|p\s*e\s*d\s*e\s*s\s*t\s*r\s*i\s*a\s*n)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexPedestrianRampText = new Regex(@"(?:pedestrian|p\s*e\s*d\s*e\s*s\s*t\s*r\s*i\s*a\s*n)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:handicap|h\s*a\s*n\s*d\s*i\s*c\s*a\s*p)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexHandicapRampText = new Regex(@"(?:handicap|h\s*a\s*n\s*d\s*i\s*c\s*a\s*p)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        ///// Regex(@"(?:two|2)\W*wheel|T\W*W)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// <summary>
        /// Regex(@"\s*(?:two|2)\W*wheel\W*(?:parking)?|\bT\W+W\b", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexTwoWheelerRampText = new Regex(@"\s*(?:two|2)\W*wheel\W*(?:parking)?|\bT\W+W\b", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        //public static readonly Regex regexTwoWheelerRampText = new Regex(@"(?:two|2)\W*wheel|T\W*W", RegexOptions.IgnoreCase | RegexOptions.Singleline);


        /// <summary>
        ///  new Regex(@"(?:two|2)\W*stack", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexTwoStackParkingName = new Regex(@"(?:two|2)\W*stack", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:cop|common plot|c\s*o\s*m\s*m\s*o\s*n\s*p\s*l\s*o\s*t)", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexCommonPlotOnlyName = new Regex(@"^\s*(?:c\W*o\W*p|common\W*plot|c\s*o\s*m\s*m\s*o\s*n\s*p\s*l\s*o\s*t)\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"PLINTH", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexPlinthName = new Regex(@"plinth", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"floor", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexFloor = new Regex(@"floor", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"PLINTH", RegexOptions.IgnoreCase | RegexOptions.Singleline)
        /// </summary>
        public static readonly Regex regexMezzanineText = new Regex(@"m\W*e\W*z\W*z\W*a\W*n\W*i\W*n\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:plinth|terrace)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexPlinthOrTerraceText = new Regex(@"(?:plinth|terrace)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:typical|floor|plan)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexTypicalFloorPlanText = new Regex(@"(?:typical|floor|plan)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:typical|basement|floor|plan)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexTypicalBasementFloorPlanText = new Regex(@"(?:typical|basement|floor|plan)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// new Regex(@"(?:typical)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexTypicalOnlyText = new Regex(@"(?:typical)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"t\W*i\W*t\W*l\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexOtherDetailsTitleText = new Regex(@"t\W*i\W*t\W*l\W*e", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"e\W*l\W*e\W*v\W*a\W*t\W*i\W*o\W*n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexOtherDetailsElevationTitleText = new Regex(@"e\W*l\W*e\W*v\W*a\W*t\W*i\W*o\W*n", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"k\W*e\W*y\W*p\W*l\W*a\W*n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexOtherDetailsKeyPlanTitleText = new Regex(@"k\W*e\W*y\W*p\W*l\W*a\W*n", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"after.*?division", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexAfterSitePlanText = new Regex(@"after.*?division", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"before.*?division", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexBeforeSitePlanText = new Regex(@"before.*?division", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"after.*?amalgamation", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexAfterSitePlanAmalgamationText = new Regex(@"after", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"before.*?amalgamation", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexBeforeSitePlanAmalgamationText = new Regex(@"before", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// Regex(@"(\d+)\W*f[\.light ]+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        /// </summary>
        public static readonly Regex regexFlightStairCase = new Regex(@"(\d+)\W*f[\.light ]+", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        ///  new Regex(@"^\s*1\s*:\s*([0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        public static readonly Regex regexPrintScale = new Regex(@"^\s*1\s*:\s*([0-9]+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        ///  new Regex(@"beam$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        public static readonly Regex regexBeam = new Regex(@"beam$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        ///  new Regex(@"beam$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        public static readonly Regex regexSlab = new Regex(@"slab", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        ///  new Regex(@"beam$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        /// </summary>
        public static readonly Regex regexSunk = new Regex(@"sunk", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>
        /// Regex(@"(d+)", RegexOptions.IgnoreCase);
        /// </summary>
        public static Regex regexNumber = new Regex(@"(\d+)", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"existing\W*builtUp", RegexOptions.IgnoreCase);
        /// </summary>
        public static Regex regexExBuiltupText = new Regex(@"existing\W*(builtUp|building)", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"canal", RegexOptions.IgnoreCase);
        /// </summary>
        public static Regex regexCanalText = new Regex(@"canal", RegexOptions.IgnoreCase);

        ///<summary>
        /// \(\s*(?:rear|side\s*(?:1|2)|front)\s*\)
        /// </summary>
        public static Regex regexBalconyText = new Regex(@"\(\s*(?:rear|side\s*(?:1|2)|front)\s*\)", RegexOptions.IgnoreCase);

        ///<summary>
        /// new Regex(@"sky\W*walk", RegexOptions.IgnoreCase);
        /// </summary>
        public static Regex regexSkyWalkText = new Regex(@"sky\W*walk", RegexOptions.IgnoreCase);

        /// <summary>
        /// new Regex(@"exsit", RegexOptions.IgnoreCase);
        /// </summary>
        public static Regex regexExist = new Regex(@"exsit", RegexOptions.IgnoreCase);


        /// <summary>
        /// 20
        /// </summary>
        public static readonly int ValidScaleValueRange = 20;

        public static readonly string NewLine = Environment.NewLine;
        public static string TempBufferString = "";
        //public static readonly int NumberOfDecimalPoint = 4;
        /// <summary>
        /// 4
        /// </summary>
        public static readonly int NumberOfDecimalPoint = 4;

        /// <summary>
        /// 5
        /// </summary>
        public static readonly double PlotSideLengthAllow = 5d;

        /// <summary>
        /// slab
        /// </summary>
        public static readonly string SlabText = "slab";

        /// <summary>
        /// beam
        /// </summary>
        public static readonly string BeamText = "beam";

        
        /// <summary>
        /// sunk
        /// </summary>
        public static readonly string SunkSlabText = "sunk";

        
        //public static readonly string LayerExstructure = "_exstructure";

        //public static string DecimalNumberFormat = "0.0000";
        /// <summary>
        /// 0.0000
        /// </summary>
        public static readonly string DecimalNumberFormat = "0.0000";

        /// <summary>
        /// 0.5
        /// </summary>
        public static readonly double StairCaseLobbyDepthValidDistance = 0.5;

        /// <summary>
        /// 0.5
        /// </summary>
        public static readonly double LiftWallValidWidthValue = 0.5;

        /// <summary>
        /// 0.5
        /// </summary>
        public static readonly double InternalRoadDiffAllowed = 0.5;

        /// <summary>
        /// 1.5
        /// </summary>
        public static readonly double InternalRoadMinWidthAllowed = 1.5;

        /// <summary>
        /// 0.005
        /// </summary>
        public static readonly double ErrorAllowScale = 0.005; //0.0005f
        /// <summary>
        /// 0.0005
        /// </summary>
        public static readonly double ErrorAllowScaleForBoundary = 0.0005; //0.0005f

        /// <summary>
        /// 0.0002
        /// </summary>
        public static readonly double ErrorAllowScaleForDoor = 0.0002; //0.0005f

        /// <summary>
        /// 0.0002
        /// </summary>
        public static readonly double ErrorAllowScaleForIntersection = 0.0001; //0.0005f

        /// <summary>
        /// 0.05
        /// </summary>
        public static readonly double ErrorAllowScaleForParallel = 0.05; //0.0005f

        /// <summary>
        /// 0.05
        /// </summary>
        public static readonly double ErrorAllowScaleForAdjustCoordinate = 0.05; //0.05f

        /// <summary>
        /// 0.00001
        /// </summary>
        public static readonly double ErrorAllowScaleForAlmostZero = 0.00001; //0.0005f

        /// <summary>
        /// 0.00001
        /// </summary>
        public static readonly double ErrorAllowScaleForFrontage = 0.000001; //0.0005f

        /// <summary>
        /// 0.05
        /// </summary>
        public static readonly double BeamMinDistance = 0.05; //0.0005f

        /// <summary>
        /// 0.01
        /// </summary>
        public static readonly double SubPlotDistanceWithRoad = 0.01; //0.0005f

        /// <summary>
        /// 10
        /// </summary>
        public static readonly double CommonPlotSideLengthAllowed = 10;

        /// <summary>
        /// 0.5
        /// </summary>
        public static readonly double ProposeAndFloorAreaDiffAllowedForCompare = 0.5;

        /// <summary>
        /// 1
        /// </summary>
        public static readonly double ProposeWidthDepthMin = 0.90d;

        public double FormatFigureInDecimalPoint(double d)
        {
            return Math.Round(d, NumberOfDecimalPoint);
        }

        public double FormatFigureInDecimalPoint(double d, int DecimalPoint)
        {
            return Math.Round(d, DecimalPoint);
        }
        public double ConvertToDouble(string sValue)
        {
            return double.Parse(sValue, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// d.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture);
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public string FormatFigureToStringInDecimalPoint(double d)
        {
            return d.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// return 3.14 * degreeValue / 180d;
        /// </summary>
        /// <param name="degreeValue"></param>
        /// <returns></returns>
        //public double DegreeToRadian(double degreeValue)
        //{
        //    return 3.14 * degreeValue / 180d;
        //}
        public double TruncateDecimal(double value, int precision)
        {
            double step = (double)Math.Pow(10, precision);
            double tmp = Math.Truncate(step * value);
            return tmp / step;
        }
        public static void MoveFile(string sourceFile, string destinationFile)
        {
            try
            {
                int idx = 0;
                while (File.Exists(destinationFile))
                {
                    destinationFile = Path.GetDirectoryName(destinationFile) + "\\" + Path.GetFileNameWithoutExtension(destinationFile) + "_" + idx++ + Path.GetExtension(destinationFile);
                }

                File.Move(sourceFile, destinationFile);
            }
            catch { }
        }
        public static void DeleteFile(string sourceFile)
        {
            try
            {
                File.Delete(sourceFile);
            }
            catch { }
        }
        public IEnumerable<string> NaturalSort(IEnumerable<string> list)
        {
            int maxLen = list.Select(s => s.Length).Max();
            Func<string, char> PaddingChar = s => char.IsDigit(s[0]) ? ' ' : char.MaxValue;

            return list
                    .Select(s =>
                        new {
                            OrgStr = s,
                            SortStr = Regex.Replace(s, @"(\d+)|(\D+)", m => m.Value.PadLeft(maxLen, PaddingChar(m.Value)))
                        })
                    .OrderBy(x => x.SortStr)
                    .Select(x => x.OrgStr);
        }

        public List<string> SortList(List<string> lstNames)
        {
            if (lstNames == null || lstNames.Count == 0)
                return lstNames;

            return NaturalSort(lstNames).ToList();
        }
        public bool IsSameText(string InputText, string MatchText)
        {
            //return string.Compare( InputText.Trim().ToLower().Equals(MatchText.Trim().ToLower());
            if (string.Compare(InputText.Trim(), MatchText.Trim(), StringComparison.OrdinalIgnoreCase) == 0)
                return true;
            else
                return false;
        }

        public bool IsSameFloorPlanText(string InputText, string MatchText)
        {
            InputText = InputText.Trim().ToUpper().Replace("PLAN", "").Trim();
            MatchText = MatchText.Trim().ToUpper().Replace("PLAN", "").Trim();

            //Find number is same then it consider same 19Apr2023
            if (InputText.Equals(MatchText) || InputText.StartsWith(MatchText) || MatchText.StartsWith(InputText))
            {
                if (regexNumber.IsMatch(MatchText) && regexNumber.IsMatch(InputText))
                {
                    string numberMatch = regexNumber.Match(MatchText).Groups[1].Value;
                    string numberInput = regexNumber.Match(InputText).Groups[1].Value;

                    if (numberMatch == numberInput)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }
            else
                return false; // InputText.Equals(MatchText) || InputText.StartsWith(MatchText) || MatchText.StartsWith(InputText);
        }


        public Dictionary<string, int> GetStingToNumberDictionary()
        {
            Dictionary<string, int> dictStingToNumber = new Dictionary<string, int>();

            dictStingToNumber.Add("twenty", 20);
            dictStingToNumber.Add("thirty", 30);
            dictStingToNumber.Add("forty", 40);
            dictStingToNumber.Add("fifty", 50);
            dictStingToNumber.Add("sixty", 60);
            dictStingToNumber.Add("seventy", 70);
            dictStingToNumber.Add("eighty", 80);
            dictStingToNumber.Add("ninety", 90);

            dictStingToNumber.Add("thirteent?h?", 13);
            dictStingToNumber.Add("fourteent?h?", 14);
            dictStingToNumber.Add("fifteent?h?", 15);
            dictStingToNumber.Add("sixteent?h?", 16);
            dictStingToNumber.Add("seventeent?h?", 17);
            dictStingToNumber.Add("nineteent?h?", 19);

            dictStingToNumber.Add("zero|ground|stilt", 0);
            dictStingToNumber.Add("first", 1);
            dictStingToNumber.Add("second", 2);
            dictStingToNumber.Add("third", 3);
            dictStingToNumber.Add("fourt?h?", 4);
            dictStingToNumber.Add("fifth|five", 5);
            dictStingToNumber.Add("sixt?h?", 6);
            dictStingToNumber.Add("sevent?h?", 7);
            dictStingToNumber.Add("eighteen", 18);
            dictStingToNumber.Add("eight?h?", 8);
            dictStingToNumber.Add("nine?(?:th)?", 9);
            dictStingToNumber.Add("tent?h?", 10);

            dictStingToNumber.Add("eleventh|eleven", 11);
            dictStingToNumber.Add("twelveth|twelfth|twelve", 12);

            return dictStingToNumber;
        }

        public string GetMarginLineTypeByColour(string colourCode)
        {
            //if (colourCode == DxfLayersName.RearMarginLineColour)
            //    return MarginLineType.RearMarginLine;// "Rare margin line"; //pink
            //else if (colourCode == DxfLayersName.FrontMarginLineColour)
            //    return MarginLineType.FrontMarginLine; // "Front margin line"; //red
            //else if (colourCode == DxfLayersName.Side1MarginLineColour)
            //    return MarginLineType.Side1MarginLine; // "Side1 line"; //blue
            //else if (colourCode == DxfLayersName.Side2MarginLineColour)
            //    return MarginLineType.Side2MarginLine;// "Side2 line"; //gray
            //else
               return "";
        }

         
        public string ExtractLayerText(LayerInfo layerInfo, bool all, string defaultText)
        {
            string sLayerText = "";
            if (layerInfo != null && layerInfo.Data != null)
            {
                return ExtractLayerText(layerInfo.Data, all, defaultText);
            }

            return sLayerText;
        }
        public string ExtractLayerText(LayerDataWithText layerInfo, bool all, string defaultText)
        {
            string sLayerText = "";

            if (layerInfo != null && layerInfo.TextInfoData != null && layerInfo.TextInfoData.Count > 0)
            {
                List<string> textInfo = layerInfo.TextInfoData.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Text)).ToList().Select(x => x.Text).ToList();
                if (textInfo != null && textInfo.Count > 0)
                    sLayerText = textInfo.Last();
            }

            if (string.IsNullOrWhiteSpace(sLayerText) && !string.IsNullOrWhiteSpace(defaultText))
                sLayerText = defaultText;

            return sLayerText; //.Replace("," , " ");
        }
        public string ExtractParkingText(LayerDataWithText layerInfo, string defaultText)
        {
            string sLayerText = "";
            string sLayerTextNotMatchWithPattern = "";
            if (layerInfo != null && layerInfo.TextInfoData != null && layerInfo.TextInfoData.Count > 0)
            {
                foreach (LayerTextInfo textInfos in layerInfo.TextInfoData)
                {
                    if (!string.IsNullOrWhiteSpace(textInfos.Text) && General.regexParkingNaming.Match(textInfos.Text).Success)
                        sLayerText += textInfos.Text;
                    else
                        sLayerTextNotMatchWithPattern += textInfos.Text;
                }
            }
            else if (layerInfo != null && layerInfo.TextInfoData != null && layerInfo.TextInfoData.Count == 1)
                sLayerTextNotMatchWithPattern += layerInfo.TextInfoData[0].Text;

            if (string.IsNullOrWhiteSpace(sLayerText) && string.IsNullOrWhiteSpace(sLayerTextNotMatchWithPattern) && !string.IsNullOrWhiteSpace(defaultText))
                sLayerText = defaultText;
            else if (string.IsNullOrWhiteSpace(sLayerText) && !string.IsNullOrWhiteSpace(sLayerTextNotMatchWithPattern))
                sLayerText = sLayerTextNotMatchWithPattern;

            return sLayerText; //.Replace("," , " ");
        }
        public List<LayerInfo> ClearSubLayerWithInLayer(List<LayerInfo> lstInput, ref List<LayerInfo> lstSubPolygon)
        {
            lstSubPolygon = new List<LayerInfo>();
            if (lstInput == null || lstInput.Count == 0)
                return null;

            MathLib objMathLib = new MathLib();
            int iTotal = lstInput.Count;
            for (int i = 0; i < iTotal; i++)
            {
                if (lstInput[i] == null || lstInput[i].Data == null || lstInput[i].Data.Coordinates == null)
                    continue;

                for (int j = 0; j < iTotal; j++)
                {
                    if (i == j || lstInput[j] == null || lstInput[j].Data == null || lstInput[j].Data.Coordinates == null)
                        continue;

                    if (objMathLib.IsInPolyUsingAngle(lstInput[i].Data.Coordinates, lstInput[j].Data.Coordinates) && lstInput[i].Data.ColourCode == lstInput[j].Data.ColourCode)
                    {
                        lstSubPolygon.Add(lstInput[j]);
                        lstInput[j] = null;
                    }
                }
            }

            return lstInput.Where(x => x != null).ToList();
        }

        public int CountSubLayerWithInLayer(List<LayerDataWithText> lstInput)
        {
            if (lstInput == null || lstInput.Count == 0)
                return 0;

            int iTotalSub = 0;
            MathLib objMathLib = new MathLib();
            int iTotal = lstInput.Count;
            for (int i = 0; i < iTotal; i++)
            {
                if (lstInput[i] == null)
                    continue;

                for (int j = 0; j < iTotal - 1; j++)
                {
                    if (lstInput[j] == null || i == j)
                        continue;

                    if (objMathLib.IsInPolyUsingAngle(lstInput[i].Coordinates, lstInput[j].Coordinates))
                    {
                        iTotalSub++;
                        lstInput[j] = null;
                    }
                }
            }

            return iTotalSub;
        }
        public string CoordinatesToString(List<Cordinates> lstCords)
        {
            if (lstCords == null || lstCords.Count == 0)
                return "";

            List<string> lstString = lstCords.Select(x => x.ToString()).ToList();

            return string.Join("\n", lstString);
        }
        public List<LayerInfo> GetAllDataSitePlanWise(List<LayerDataWithText> lstAllInputData, ref LayerExtractor objLayerExtractor)
        {
            List<LayerInfo> lstSitePlanLayer = new List<LayerInfo>();
            if (lstAllInputData == null || lstAllInputData.Count == 0)
                return lstSitePlanLayer;

            List<LayerDataWithText> lstSitePlan = lstAllInputData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.SitePlan).ToList();
            lstSitePlanLayer = objLayerExtractor.SetLayerInfo(lstSitePlan, DxfLayersName.SitePlan);

            objLayerExtractor.ExtractChildLayersForParentLayer(lstAllInputData, GetDictionaryForAllLayer(), ref lstSitePlanLayer);

            return lstSitePlanLayer;
        }
        public List<LayerInfo> GetAllDataBuildingFloorWise(List<LayerDataWithText> lstAllInputData, ref LayerExtractor objLayerExtractor)
        {
            List<LayerInfo> lstBuildingLayer = new List<LayerInfo>();
            if (lstAllInputData == null || lstAllInputData.Count == 0)
                return lstBuildingLayer;

            List<LayerDataWithText> lstBuilding = lstAllInputData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.Building).ToList();
            lstBuildingLayer = objLayerExtractor.SetLayerInfo(lstBuilding, DxfLayersName.Building);

            objLayerExtractor.ExtractChildLayersForParentLayer(lstAllInputData, DxfLayersName.Floor, DxfLayersName.Floor, ref lstBuildingLayer);
            objLayerExtractor.ExtractChildLayersForParentLayer(lstAllInputData, DxfLayersName.Section, DxfLayersName.Section, ref lstBuildingLayer);

            foreach (LayerInfo itemBuilding in lstBuildingLayer)
            {
                if (!itemBuilding.Child.ContainsKey(DxfLayersName.Floor) && !itemBuilding.Child.ContainsKey(DxfLayersName.Section))
                    continue;

                if (itemBuilding.Child.ContainsKey(DxfLayersName.Section))
                {
                    List<LayerInfo> lstSection = itemBuilding.Child[DxfLayersName.Section];
                    objLayerExtractor.ExtractChildLayersForParentLayer(lstAllInputData, GetDictionaryForAllLayer(), ref lstSection);
                }
                else
                {
                    List<LayerInfo> lstFloor = itemBuilding.Child[DxfLayersName.Floor];
                    objLayerExtractor.ExtractChildLayersForParentLayer(lstAllInputData, GetDictionaryForAllLayer(), ref lstFloor);
                }
            }

            return lstBuildingLayer;
        }
        public Dictionary<string, string> GetDictionaryForAllLayer()
        {
            Dictionary<string, string> dictLayerNames = new Dictionary<string, string>();
            foreach (string s in AllLayersName())
            {
                dictLayerNames.Add(s, s);
            }
            return dictLayerNames;
        }
        public List<string> AllLayersName()
        {
            List<string> lstLayersName = new List<string>();
            lstLayersName.Add(DxfLayersName.AccessoryUse);
            lstLayersName.Add(DxfLayersName.ArchProject);
            lstLayersName.Add(DxfLayersName.Beam);
            lstLayersName.Add(DxfLayersName.BasementArea);
            lstLayersName.Add(DxfLayersName.Building);
            lstLayersName.Add(DxfLayersName.Chaja);                     //lstLayersName.Add(DxfLayersName.CommercialFSI);
            lstLayersName.Add(DxfLayersName.CommercialBuiltUpLine);
            lstLayersName.Add(DxfLayersName.CommonPlot);
            lstLayersName.Add(DxfLayersName.Door);
            lstLayersName.Add(DxfLayersName.Exstructure);
            lstLayersName.Add(DxfLayersName.ExBuiltUpLine);
            lstLayersName.Add(DxfLayersName.Floor);
            lstLayersName.Add(DxfLayersName.FloorInSection);
            lstLayersName.Add(DxfLayersName.GroundLevel);
            lstLayersName.Add(DxfLayersName.HighFloodLevel);
            lstLayersName.Add(DxfLayersName.Lift);
            lstLayersName.Add(DxfLayersName.MainRoad);
            lstLayersName.Add(DxfLayersName.MarginLine);
            lstLayersName.Add(DxfLayersName.NetPlot);
            lstLayersName.Add(DxfLayersName.IndividualSubPlot);
            lstLayersName.Add(DxfLayersName.OTS);                       //lstLayersName.Add(DxfLayersName.OWT);
            lstLayersName.Add(DxfLayersName.Parking);
            lstLayersName.Add(DxfLayersName.Plot);                      //lstLayersName.Add(DxfLayersName.ProposedWork);
            lstLayersName.Add(DxfLayersName.Propwork);
            lstLayersName.Add(DxfLayersName.Ramp);                          //lstLayersName.Add(DxfLayersName.ResidentFSI);
            lstLayersName.Add(DxfLayersName.ResidentBuiltUpLine);
            lstLayersName.Add(DxfLayersName.IndustrialBuiltUpLine);            //lstLayersName.Add(DxfLayersName.SpecialFSI);            //lstLayersName.Add(DxfLayersName.SpecialBuiltUpLine);            //lstLayersName.Add(DxfLayersName.SpecialUseFSI);
            lstLayersName.Add(DxfLayersName.SpecialUseBuiltUpLine);
            lstLayersName.Add(DxfLayersName.StairCase);
            lstLayersName.Add(DxfLayersName.Section);
            lstLayersName.Add(DxfLayersName.SectionalItem);
            lstLayersName.Add(DxfLayersName.UnitBUA);
            lstLayersName.Add(DxfLayersName.Void);
            lstLayersName.Add(DxfLayersName.DriveWay);
            lstLayersName.Add(DxfLayersName.Wall);                          //lstLayersName.Add(DxfLayersName.InternalRoad);
            lstLayersName.Add(DxfLayersName.ResidentInternalRoad);
            lstLayersName.Add(DxfLayersName.NonResidentInternalRoad);
            lstLayersName.Add(DxfLayersName.Passage);
            lstLayersName.Add(DxfLayersName.RoadWidening);
            lstLayersName.Add(DxfLayersName.Balcony);
            lstLayersName.Add(DxfLayersName.Room);
            lstLayersName.Add(DxfLayersName.Terrace);
            lstLayersName.Add(DxfLayersName.SitePlan);
            lstLayersName.Add(DxfLayersName.Window);
            lstLayersName.Add(DxfLayersName.RoadCurvature);
            lstLayersName.Add(DxfLayersName.Subdivision);
            lstLayersName.Add(DxfLayersName.RefugeeArea);
            lstLayersName.Add(DxfLayersName.Amalgamation);
            lstLayersName.Add(DxfLayersName.PrintArea);
            lstLayersName.Add(DxfLayersName.WithInMarginLine);
            lstLayersName.Add(DxfLayersName.CommonReferencePoint);
            lstLayersName.Add(DxfLayersName.North);
            lstLayersName.Add(DxfLayersName.OtherDetail);
            lstLayersName.Add(DxfLayersName.NonOwnerPlot);
            lstLayersName.Add(DxfLayersName.Loft);
            lstLayersName.Add(DxfLayersName.WaterBody);
            lstLayersName.Add(DxfLayersName.Connector);
            lstLayersName.Add(DxfLayersName.SkyWalk);
            
            return lstLayersName;
        }

        public List<string> AllLayersNameForDrawing()
        {
            List<string> lstLayersName = new List<string>();
            lstLayersName.Add(DxfLayersName.AccessoryUse);
            lstLayersName.Add(DxfLayersName.ArchProject);
            lstLayersName.Add(DxfLayersName.Beam);
            lstLayersName.Add(DxfLayersName.BasementArea);
            lstLayersName.Add(DxfLayersName.Balcony);
            lstLayersName.Add(DxfLayersName.Building);
            lstLayersName.Add(DxfLayersName.Chaja);
            lstLayersName.Add(DxfLayersName.CommonPlot);
            lstLayersName.Add(DxfLayersName.CommercialBuiltUpLine);
            lstLayersName.Add(DxfLayersName.Door);
            lstLayersName.Add(DxfLayersName.Exstructure);
            lstLayersName.Add(DxfLayersName.ExBuiltUpLine);
            lstLayersName.Add(DxfLayersName.Floor);
            lstLayersName.Add(DxfLayersName.FloorInSection);
            lstLayersName.Add(DxfLayersName.GroundLevel);
            lstLayersName.Add(DxfLayersName.HighFloodLevel);
            lstLayersName.Add(DxfLayersName.IndustrialBuiltUpLine);
            lstLayersName.Add(DxfLayersName.Lift);
            lstLayersName.Add(DxfLayersName.MainRoad);
            lstLayersName.Add(DxfLayersName.MarginLine);
            lstLayersName.Add(DxfLayersName.NetPlot);
            lstLayersName.Add(DxfLayersName.NonResidentInternalRoad);
            lstLayersName.Add(DxfLayersName.OTS);
            lstLayersName.Add(DxfLayersName.OtherDetail);
            lstLayersName.Add(DxfLayersName.Parking);
            lstLayersName.Add(DxfLayersName.Passage);
            lstLayersName.Add(DxfLayersName.Plot);
            lstLayersName.Add(DxfLayersName.IndividualSubPlot);
            lstLayersName.Add(DxfLayersName.Propwork);

            lstLayersName.Add(DxfLayersName.Ramp);
            lstLayersName.Add(DxfLayersName.Room);
            lstLayersName.Add(DxfLayersName.ResidentBuiltUpLine);
            lstLayersName.Add(DxfLayersName.ResidentInternalRoad);
            lstLayersName.Add(DxfLayersName.RoadWidening);

            lstLayersName.Add(DxfLayersName.Sanitation);
            lstLayersName.Add(DxfLayersName.Section);
            lstLayersName.Add(DxfLayersName.SectionalItem);
            lstLayersName.Add(DxfLayersName.SitePlan);
            lstLayersName.Add(DxfLayersName.SpecialUseBuiltUpLine);
            lstLayersName.Add(DxfLayersName.StairCase);
            lstLayersName.Add(DxfLayersName.Terrace);
            lstLayersName.Add(DxfLayersName.Void);
            lstLayersName.Add(DxfLayersName.UnitBUA);
            lstLayersName.Add(DxfLayersName.DriveWay);

            lstLayersName.Add(DxfLayersName.Wall);
            lstLayersName.Add(DxfLayersName.Window);
            lstLayersName.Add(DxfLayersName.RoadCurvature);

            lstLayersName.Add(DxfLayersName.Subdivision);
            lstLayersName.Add(DxfLayersName.RefugeeArea);
            lstLayersName.Add(DxfLayersName.Amalgamation);

            lstLayersName.Add(DxfLayersName.WithInMarginLine);
            lstLayersName.Add(DxfLayersName.PrintArea);
            lstLayersName.Add(DxfLayersName.CommonReferencePoint);
            lstLayersName.Add(DxfLayersName.North);
            lstLayersName.Add(DxfLayersName.NonOwnerPlot);
            lstLayersName.Add(DxfLayersName.Loft);
            lstLayersName.Add(DxfLayersName.WaterBody);
            lstLayersName.Add(DxfLayersName.Connector);
            lstLayersName.Add(DxfLayersName.SkyWalk);

            return lstLayersName;
        }
        public Dictionary<string, List<LayerDataWithText>> LoadAllLayerData(List<LayerDataWithText> lstData)
        {
            Dictionary<string, List<LayerDataWithText>> htLayerData = new Dictionary<string, List<LayerDataWithText>>();
            foreach (string layerName in AllLayersName())
            {
                List<LayerDataWithText> lstLayerData = lstData.Where(x => x.LayerName.IsEquals(layerName)).ToList();
                if (lstLayerData != null && lstLayerData.Count > 0)
                {
                    htLayerData.Add(layerName, lstLayerData);
                }
            }

            return htLayerData;
        } 
        public bool IsTextMatch(LayerDataWithText layerData, string sTextMatch)
        {
            bool bFound = false;
            if (layerData != null && layerData.TextInfoData != null && layerData.TextInfoData.Count > 0)
            {
                foreach (LayerTextInfo textInfoData in layerData.TextInfoData)
                {
                    if (textInfoData.Text.ToLower().Trim() == sTextMatch)
                    {
                        bFound = true;
                        break;
                    }
                }
            }
            return bFound;
        }
        
        public void SetMinMaxValue(double valueOfX, double valueOfY, ref double minValueX, ref double maxValueX, ref double minValueY, ref double maxValueY)
        {
            try
            {
                SetMinMaxValue(valueOfX, ref minValueX, ref maxValueX);

                SetMinMaxValue(valueOfY, ref minValueY, ref maxValueY);
            }
            catch { }
        }
        private void SetMinMaxValue(double value, ref double minValue, ref double maxValue)
        {
            try
            {
                if (maxValue < value && value != double.MinValue && value != double.MaxValue)
                    maxValue = value;

                if (minValue > value && value != double.MinValue && value != double.MaxValue)
                    minValue = value;
            }
            catch { }
        }
        public string CleanLayerName(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
                return "";

            if (layerName.IndexOf("@") > 0)
            {
                layerName = layerName.Substring(0, layerName.IndexOf("@")).Trim();
            }
            return layerName;
        }
        public string GetCoordinatesList(List<Cordinates> lstCords)
        {
            StringBuilder sb = new StringBuilder("");
            foreach (Cordinates cord in lstCords)
                sb.AppendLine(cord.ToString());

            return sb.ToString();
        }

        public string GetLineList(List<CLineSegment> lstLines)
        {
            StringBuilder sb = new StringBuilder("");
            foreach (CLineSegment line in lstLines)
                sb.AppendLine(line.ToString());

            return sb.ToString();
        }

        public List<Cordinates> ClearDuplicateCoordinateFromBottom(List<Cordinates> lstCords)
        {
            if (lstCords == null || lstCords.Count < 2)
                return lstCords;

            lstCords = lstCords.Where(x => x != null).ToList();
            int i = 1;
            int iTotalCount = lstCords.Count;
            if (lstCords.First().Equals(lstCords.Last()))
            {
                while (lstCords != null && lstCords.Count > 1 && i < iTotalCount && lstCords[iTotalCount - i].Equals(lstCords[iTotalCount - (i + 1)]))
                {
                    lstCords[iTotalCount - i] = null;
                    i++;
                }
                lstCords = lstCords.Where(x => x != null).ToList();
            }
            return lstCords;
        }


    } //class 
}