using Newtonsoft.Json;
using EdmontonDrawingValidator.Model;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace EdmontonDrawingValidator {
    public class CDataExtractionProcess : LayerExtractor
    {
        public List<LayerDataWithText> ExtractDataFromDXF(string FileNameInProcess, string InputFile, string OutputFolder, bool DoYouWantLayerWiseDataFile, ref List<string> lstLayers, ref Dictionary<string, string> dictLayerDefaultColour, ref bool DxfFileHasError, ref List<string> lstErrorMessages)
        {
            List<LayerCoordinateInfo> lstResult = new List<LayerCoordinateInfo>();
            List<LayerTextInfo> lstTextResult = new List<LayerTextInfo>();  

            //Process the file now
            ExtractLayerCoordinateAndTextData(InputFile, ref dictLayerDefaultColour, ref lstResult, ref lstTextResult, ref DxfFileHasError, ref lstErrorMessages);

            lstTextResult = lstTextResult.Where(x => !string.IsNullOrWhiteSpace(x.Text) && x.Text.Trim() != "").ToList();

            // added on 13Oct2023
            // All Text to title case
            for (int i = 0; i < lstTextResult.Count(); i++)
            {
                LayerTextInfo textItem = lstTextResult[i];

                if (!string.IsNullOrWhiteSpace(lstTextResult[i].Text))
                    lstTextResult[i].Text = lstTextResult[i].Text.ToTitleCase();
            }

            if (DoYouWantLayerWiseDataFile)
            {
                //Write Text data into file
                WriteLayerTextData(lstTextResult, OutputFolder);

                //Write coordinate data into file
                WriteLayerCoordinateData(lstResult, OutputFolder);
            }

            // 15-Jul-2025: Filter out the layers that are not required
            lstResult = lstResult.Where(x => !(x.LayerName.ToLower() == DxfLayersName.OtherDetail && x.IsCircle)).ToList();


            //process bind text with coordinate region
            List<LayerDataWithText> lstLayerWithText = ProcessBindingTextWithCoordinate(lstResult, lstTextResult);

            //remove multiple text 

            //File.WriteAllText(OutputFolder + "\\AllDataInJson.json", NewtonSoft.Json.JsonConvert.SerializeObject(lstLayerWithText, NewtonSoft.Json.Formatting.Indented));

            if (General.DebugLogWithDataEnabled)
            {
                List<string> lstLayerName = lstLayerWithText.Select(x => x.LayerName).Distinct().ToList();
                File.WriteAllText(@"F:\BKPatel\UserData\Testcase11\layername.txt", string.Join('\n', lstLayerName.ToArray()));

                //layer-wise data
                foreach (string slayer in lstLayerName)
                {
                    List<LayerDataWithText> lstTemp = lstLayerWithText.Where(x => x.LayerName.Trim().ToLower() == slayer.ToLower().Trim()).ToList();
                    File.WriteAllText(@"F:\BKPatel\UserData\Testcase11\" + slayer + "_data.txt", JsonConvert.SerializeObject(lstTemp, Formatting.Indented));
                }
            }

            return lstLayerWithText;
        }

        public void WriteLayerTextData(List<LayerTextInfo> lstTextResult, string OutputFolder)
        {
            List<string> lstTextLayers = lstTextResult.Select(x => x.LayerName).Distinct().ToList();
            foreach (String sLayerName in lstTextLayers)
            {
                StringBuilder sb = new StringBuilder("");

                List<LayerTextInfo> lstLayerTextResult = lstTextResult.Where(x => x.LayerName == sLayerName).Distinct().ToList(); ;
                if (lstLayerTextResult != null && lstLayerTextResult.Count > 0)
                {
                    bool bFirst = true;
                    foreach (LayerTextInfo itemResult in lstLayerTextResult)
                    {
                        if (bFirst)
                            sb.AppendLine(itemResult.LayerName);

                        bFirst = false;
                        foreach (Cordinates cord in itemResult.Coordinates)
                            sb.AppendLine($"{cord.X.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)};{cord.Y.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)};{itemResult.Text};{itemResult.ColourCode}");
                    }

                    if (!Directory.Exists(OutputFolder))
                        Directory.CreateDirectory(OutputFolder);

                    File.WriteAllText(OutputFolder + sLayerName + ".txt", sb.ToString());

                } //if layerresult

            } //for each

        }

        public void WriteLayerCoordinateData(List<LayerCoordinateInfo> lstResult, string OutputFolder)
        {
            List<string> lstLayers = lstResult.Select(x => x.LayerName).Distinct().ToList();

            foreach (String sLayerName in lstLayers)
            {
                StringBuilder sb = new StringBuilder("");

                List<LayerCoordinateInfo> lstLayerResult = lstResult.Where(x => x.LayerName == sLayerName).Distinct().ToList(); ;
                if (lstLayerResult != null && lstLayerResult.Count > 0)
                {
                    sb.AppendLine(lstLayerResult[0].LayerName);
                    foreach (LayerCoordinateInfo itemResult in lstLayerResult)
                    {
                        sb.AppendLine($"Command;{itemResult.Command}");
                        sb.AppendLine($"ColourCode;{itemResult.ColourCode}");

                        if (!string.IsNullOrWhiteSpace(itemResult.LineType))
                            sb.AppendLine($"LineType;{itemResult.LineType}");

                        if (!itemResult.HasBulge && !itemResult.IsCircle)
                        {
                            foreach (Cordinates coordinates in itemResult.Coordinates)
                            {
                                sb.AppendLine($"{coordinates.X.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)};{coordinates.Y.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)}");
                            }
                        }
                        else if (itemResult.IsCircle)
                        {
                            foreach (Cordinates coordinates in itemResult.Coordinates)
                            {
                                sb.AppendLine($"{coordinates.X.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)};{coordinates.Y.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)}");
                            }

                            sb.AppendLine($"Center Point : {itemResult.CenterPoint.X.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)},{itemResult.CenterPoint.Y.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)}");
                            sb.AppendLine($"Radius: {itemResult.Radius}");
                            sb.AppendLine($"StartAngle: {itemResult.StartAngle}");
                            sb.AppendLine($"EndAngle: {itemResult.EndAngle}");
                        }
                        else
                        {
                            foreach (BulgeItem coordinates in itemResult.CoordinateWithBulge)
                            {
                                BulgeItemValue itemValue = (BulgeItemValue)coordinates.ItemValue;

                                if (coordinates.IsBulgeValue)
                                {
                                    sb.AppendLine($"Bulge Start: {itemValue.StartPoint.X.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)};{itemValue.StartPoint.Y.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)}");
                                    sb.AppendLine($"Bulge: {(itemValue.Bulge).ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)}");
                                    sb.AppendLine($"Bulge End: {itemValue.EndPoint.X.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)};{itemValue.EndPoint.Y.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)}");
                                }
                                else
                                    sb.AppendLine($"{itemValue.StartPoint.X.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)};{itemValue.StartPoint.Y.ToString(General.DecimalNumberFormat, CultureInfo.InvariantCulture)}");
                            }
                        }
                        sb.AppendLine("");
                    } // for each

                    if (!Directory.Exists(OutputFolder))
                        Directory.CreateDirectory(OutputFolder);

                    File.WriteAllText(OutputFolder + sLayerName + ".csv", sb.ToString());

                } //if layerresult

            } //for each
        }

    }
}