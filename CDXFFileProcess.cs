using EdmontonDrawingValidator.Model;
using log4net;
using log4net.Config;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.HPRtree;
using Newtonsoft;
using Newtonsoft.Json;
using SharedClasses;
using SharedClasses;
using SharedClasses.Constants;
using SharedClasses.PrintDimension;
using SVPAS.LogUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace EdmontonDrawingValidator
{
    public class CDXFFileProcess : MathLib
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected CDataExtractionProcess objProcess = new CDataExtractionProcess();
        protected LayerExtractor objLayerExtractor = new LayerExtractor();
        protected LineOperations objLineOperation = new LineOperations();
        protected bool bDeleteAfterProcess = true;
        protected bool bMoveAfterProcess = true;

        public async Task<int> ProcessStart(string DXFFilePathToProcess)
        {  
            DateTime dtTaskStart = DateTime.Now;
            DxfProcessorInput dxfProcessorInput = new DxfProcessorInput();
 
            StringBuilder sbHeaderProcessTask = new StringBuilder("");
            sbHeaderProcessTask.AppendLine("Task start time " + dtTaskStart.ToString("HH:mm:ss.fffff"));
            Console.WriteLine(DXFFilePathToProcess);
            if (!File.Exists(DXFFilePathToProcess))
                return 0;

            DxfProcessorQueue objDXFFileQueueItem = JsonConvert.DeserializeObject<DxfProcessorQueue>(File.ReadAllText(DXFFilePathToProcess));

            //Check if both report and validation false then just delete file and return
            if (objDXFFileQueueItem.IsValidateDrawing == false && objDXFFileQueueItem.IsCreateReport == false)
            {
                if (File.Exists(DXFFilePathToProcess))
                    File.Delete(DXFFilePathToProcess);

                Console.WriteLine($"request with no drawing validation and no create report.");
                return 0;
            }

            //if (string.IsNullOrWhiteSpace(objDXFFileQueueItem.ProjectType))
            //{
            //    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Project type not found in input json");
            //    return 0;
            //}

  
            string sFileNameInProcess = Path.GetFileNameWithoutExtension(objDXFFileQueueItem.DxfFilePath);
            string DXFFolder = Path.GetDirectoryName(objDXFFileQueueItem.DxfFilePath);
            DXFFolder = Path.Join(DXFFolder, "\\");

            #region variable value setup 

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Folder " + DXFFolder);

            string sFileOutputFolder = DXFFolder + "DrawingData\\";
            string sDataJsonFile = DXFFolder + sFileNameInProcess + "_data.json";
            string sRuleInputJsonFile = DXFFolder + sFileNameInProcess + "_rule.json";
            string sTimingFile = DXFFolder + sFileNameInProcess + "_ReportTiming.txt";
            string sFileLogFile = DXFFolder + sFileNameInProcess + "_log.txt";
            string sRuleTesterInputJsonFile = General.RuleTesterInputFolder + sFileNameInProcess + "_ruleTesterInput.json";
            string InputDXFFilePath = objDXFFileQueueItem.DxfFilePath;
            if (!System.IO.Directory.Exists(sFileOutputFolder))
                System.IO.Directory.CreateDirectory(sFileOutputFolder);

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Log file: " + sFileLogFile);
            General.LogFile = sFileLogFile;

            sDataJsonFile = sDataJsonFile.Replace(@"\\\\", @"\\");
            sFileNameInProcess = sFileNameInProcess.Replace(@"\\\\", @"\\");
            sRuleInputJsonFile = sRuleInputJsonFile.Replace(@"\\\\", @"\\");

            #endregion

            try
            {
                Console.WriteLine($"File in process:{sFileNameInProcess} , Project type:{objDXFFileQueueItem.ProjectType}");
                log.Info($"File in process:{sFileNameInProcess} , Project type:{objDXFFileQueueItem.ProjectType}");

                DateTime dtDataReadStartTime = DateTime.Now;
                DateTime dtDataReadEndTime = DateTime.Now;
                sbHeaderProcessTask.AppendLine("Data read start time " + dtDataReadStartTime.ToString("HH:mm:ss.fffff"));

                List<string> lstLayers = new List<string>();
                bool bDoYouWantLayerDataFile = true;

                List<LayerDataWithText> lstResultWithText = new List<LayerDataWithText>();
                List<LayerDataWithText> lstPrintDetailsBlocks = new List<LayerDataWithText>();

                Dictionary<string, string> dictLayerDefaultColour = new Dictionary<string, string>();
                bool IsDxfFileHasError = false;
                List<string> lstErrorMessages = new List<string>();
                List<string> logTiming = new List<string>();
                if (lstResultWithText == null || lstResultWithText.Count == 0)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Data extracting going on");
                    logTiming.Add($"Data extract start " + DateTime.Now.ToString("HH:mm:ss.fff"));
                    lstResultWithText = objProcess.ExtractDataFromDXF(sFileNameInProcess, InputDXFFilePath, sFileOutputFolder, bDoYouWantLayerDataFile, ref lstLayers, ref dictLayerDefaultColour, ref IsDxfFileHasError, ref lstErrorMessages); // pass only one by one file
                    logTiming.Add($"Data extract end " + DateTime.Now.ToString("HH:mm:ss.fff"));
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Data extracting complete");

                    logTiming.Add($"Data clean start " + DateTime.Now.ToString("HH:mm:ss.fff"));
                    if (IsDxfFileHasError)
                    {
                        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Drawing file has error found");
                        foreach (string smessage in lstErrorMessages)
                            Console.WriteLine(smessage);
                    }

                    List<LayerDataWithText> lstTemp = lstResultWithText.Where(x => !CheckLineTypeIsCenterLine(x.LineType) && x.TextInfoData != null && x.TextInfoData.Count > 1).ToList();
                    if (lstTemp != null && lstTemp.Count > 0)
                    {
                        List<string> lstLayerToProcess = lstTemp.Select(x => x.LayerName).ToList().Distinct().ToList();
                        foreach (string s in lstLayerToProcess)
                        {
                            List<LayerDataWithText> lstLayerItems = lstTemp.Where(x => x.LayerName.IsEquals(s) && x.TextInfoData != null && x.TextInfoData.Count > 1).ToList();
                            List<LayerDataWithText> lstLayerAllItems = lstResultWithText.Where(x => x.LayerName.IsEquals(s) && x.TextInfoData != null && x.TextInfoData.Count == 1).ToList();
                            foreach (LayerDataWithText objItem in lstLayerItems)
                            {
                                foreach (LayerDataWithText obj in lstLayerAllItems)
                                {
                                    if (objItem.TextInfoData.Count(x => x.Text.IsEquals(obj.TextInfoData[0].Text)) == 0)
                                        continue;

                                    if (IsInPolyUsingAngle(objItem.Coordinates, obj.Coordinates))
                                    {
                                        for (int i = 0; i < objItem.TextInfoData.Count; i++)
                                        {
                                            if (objItem.TextInfoData[i].Text.IsEquals(obj.TextInfoData[0].Text))
                                            {
                                                objItem.TextInfoData[i] = null;
                                                objItem.TextInfoData = objItem.TextInfoData.Where(x => x != null).ToList();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //Remove duplicate text done

                    // remove all validation error objects if exists in dxf
                    lstResultWithText = lstResultWithText.Where(x => x.LayerName.ToLower().Trim() != DxfLayersName.ValidateErrorMessage).ToList();

                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Table block begin and reference data extract");

                    File.WriteAllText(sDataJsonFile, JsonConvert.SerializeObject(lstResultWithText, Formatting.Indented));
                }

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Data formatting start");
 
                if (objDXFFileQueueItem.ProjectType == ProjectType.BuildingPermission || objDXFFileQueueItem.ProjectType == ProjectType.BuildingPermissionWithAdditionOrExtension ||
                    objDXFFileQueueItem.ProjectType == ProjectType.RevisedWithExistingConstruction  || objDXFFileQueueItem.ProjectType == ProjectType.RevisedWithoutExistingConstruction) //17Mar2025 revised change
                {
                    //ProcessBuildingAndFloorForRangeNaming(ref lstResultWithText);

                    //comment 09Jul2022
                    ProcessToSwapLineXYCoordinateSort(ref lstResultWithText);

                    //remove duplicate coordinate which are appears in sequence
                    ProcessToClearDuplicateSequenceCoordinate(ref lstResultWithText);
                }

                //clean polygone which has 0 area CLEAR ALL POLYGON WHICH AREA IS 0 
                ProcessToCleanZeroAreaPolygon(ref lstResultWithText);

                //Do close polygon //29Nov2022
                ProcessToClosePolygon(ref lstResultWithText);

                MakeAllBulgePolyToProcess(ref lstResultWithText);

                lstPrintDetailsBlocks = lstResultWithText.Where(x => x.LayerName.IsEquals(DxfLayersName.PrintArea)).ToList();

                dtDataReadEndTime = DateTime.Now;
                sbHeaderProcessTask.AppendLine("Data read end time " + dtDataReadEndTime.ToString("HH:mm:ss.fffff") + ", Sec: " + DateTime.Now.Subtract(dtDataReadStartTime).TotalSeconds + ", MS: " + dtDataReadEndTime.Subtract(dtDataReadStartTime).TotalMilliseconds);
                sbHeaderProcessTask.AppendLine("-----------------------------------------------------------");
                logTiming.Add($"Data clean end " + DateTime.Now.ToString("HH:mm:ss.fff"));

                logTiming.Add($"Report start " + DateTime.Now.ToString("HH:mm:ss.fff"));
                dxfProcessorInput.DataReadStartTime = dtDataReadStartTime;
                dxfProcessorInput.DataReadEndTime = dtDataReadEndTime;

                Dictionary<string, List<LayerDataWithText>> htLayerData = new Dictionary<string, List<LayerDataWithText>>();
                DrawingValidateItem objDrawingNamingValidation = new DrawingValidateItem();

                htLayerData = LoadAllLayerData(lstResultWithText);

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Proposed coverage calculation going on....");
                StringBuilder sbProposedCoverageArea = new StringBuilder("");


                // Find distance between 
                lstResultWithText.ForEach(x => x.LayerName = x.LayerName.ToLower());


                List<LayerInfo> lstMainRoadInfo = new List<LayerInfo>(); // lstResultWithText.Where(x => x.LayerName.ToLower() == DxfLayersName.FrontRoad).ToList();

                List<LayerDataWithText> lstUnits = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.Unit, StringComparison.OrdinalIgnoreCase)).ToList();
                List<LayerDataWithText> lstLot = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.Lot, StringComparison.OrdinalIgnoreCase)).ToList();
                List<LayerDataWithText> lstFrontRoad = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.FrontRoad, StringComparison.OrdinalIgnoreCase)).ToList();
                List<LayerDataWithText> lstSiteplan = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.SitePlan, StringComparison.OrdinalIgnoreCase)).ToList();
                List<LayerDataWithText> lstSiteplanRearMarginLine = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.RearMarginLine, StringComparison.OrdinalIgnoreCase)).ToList();

                List<LayerDataWithText> lstFrontElevationplan = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.FrontElevationplan, StringComparison.OrdinalIgnoreCase)).ToList();
                List<LayerDataWithText> lstRearElevationplan = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.RearElevationplan, StringComparison.OrdinalIgnoreCase)).ToList();
                List<LayerDataWithText> lstSide1Elevationplan = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.Side1Elevationplan, StringComparison.OrdinalIgnoreCase)).ToList();
                List<LayerDataWithText> lstSide2Elevationplan = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.Side2Elevationplan, StringComparison.OrdinalIgnoreCase)).ToList();

                List<LayerInfo> lstSiteplanInfo = objLayerExtractor.SetLayerInfo(lstSiteplan, DxfLayersName.SitePlan);
                
                List<LayerInfo> lstFrontElevationplanInfo = objLayerExtractor.SetLayerInfo(lstFrontElevationplan, DxfLayersName.FrontElevationplan);
                List<LayerInfo> lstRearElevationplanInfo = objLayerExtractor.SetLayerInfo(lstRearElevationplan, DxfLayersName.RearElevationplan);
                List<LayerInfo> lstSide1ElevationplanInfo = objLayerExtractor.SetLayerInfo(lstSide1Elevationplan, DxfLayersName.Side1Elevationplan);
                List<LayerInfo> lstSide2ElevationplanInfo = objLayerExtractor.SetLayerInfo(lstSide2Elevationplan, DxfLayersName.Side2Elevationplan);

                List<LayerInfo> lstFourElevationplanForWallDoorInfo = new List<LayerInfo>();
                if(lstFrontElevationplanInfo.Count() > 0)
                    lstFourElevationplanForWallDoorInfo.AddRange(lstFrontElevationplanInfo);

                if (lstRearElevationplanInfo.Count() > 0)
                    lstFourElevationplanForWallDoorInfo.AddRange(lstRearElevationplanInfo);

                if (lstSide1ElevationplanInfo.Count() > 0)
                    lstFourElevationplanForWallDoorInfo.AddRange(lstSide1ElevationplanInfo);

                if (lstSide2ElevationplanInfo.Count() > 0)
                    lstFourElevationplanForWallDoorInfo.AddRange(lstSide2ElevationplanInfo);

                objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.Unit, DxfLayersName.Unit, ref lstSiteplanInfo);
                objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.Lot, DxfLayersName.Lot, ref lstSiteplanInfo);
                objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.FrontRoad, DxfLayersName.FrontRoad, ref lstSiteplanInfo);
                objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.Garage, DxfLayersName.Garage, ref lstSiteplanInfo);
                objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.Alley, DxfLayersName.Alley, ref lstSiteplanInfo);

                // Extract data
                objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.Wall, DxfLayersName.Wall, ref lstFourElevationplanForWallDoorInfo);
                objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.Door, DxfLayersName.Door, ref lstFourElevationplanForWallDoorInfo);
                objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.Window, DxfLayersName.Window, ref lstFourElevationplanForWallDoorInfo);
                

                if (lstSiteplanInfo.Count > 0)
                {
                    //var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    //if (lstSiteplanInfo.Any(x => x.Child.ContainsKey(DxfLayersName.Alley)))
                    if (lstSiteplanInfo.Any(x => x.Child.Keys.Any(k => string.Equals(k, DxfLayersName.Alley, StringComparison.OrdinalIgnoreCase))))
                        dxfProcessorInput.IsAlleyExist = true;
                }

                SectionPlanData objSelectionPlanData = new SectionPlanData();
                GetSectionalData(lstResultWithText, ref objSelectionPlanData);
                dxfProcessorInput.SectionalData = objSelectionPlanData;

                //LotInformation objLotInformation = new LotInformation();
                //GetLotInformation(lstSiteplanInfo, ref objLotInformation);
                //dxfProcessorInput.LotData = objLotInformation;

                UnitInformation objUnitInformation = new UnitInformation();
                GetUnitInformation(lstSiteplanInfo, ref objUnitInformation);
                dxfProcessorInput.UnitData = objUnitInformation;

                GarageInformation objGarageInformation = new GarageInformation();
                bool IsGarageExists = false;
                GetGarageInformation(lstSiteplanInfo, ref objGarageInformation, ref IsGarageExists);
                dxfProcessorInput.GarageData = objGarageInformation;
                dxfProcessorInput.IsGarageExists = IsGarageExists;

                AccessoryInformation objAccessoryInformation = new AccessoryInformation();
                GetAccessoryInformation(lstSiteplanInfo, ref objAccessoryInformation);
                dxfProcessorInput.AccessoryUnitInfo = objAccessoryInformation;

                FlankingInformation objFlankingInformation = new FlankingInformation();
                SetBackInformation objSetBackData = new SetBackInformation();
                LotInformation objLotInformation = new LotInformation();
                GetSetBack(lstSiteplanInfo, lstSiteplanRearMarginLine, ref objSetBackData,ref objLotInformation,ref objFlankingInformation);
                dxfProcessorInput.SetBackInfo = objSetBackData;
                dxfProcessorInput.LotData = objLotInformation;
                dxfProcessorInput.FlankingInfo = objFlankingInformation;


                //SitePlanExit
                UnitWallDoorWindowInformation objUnitWallDoorWindowInformation = new UnitWallDoorWindowInformation();
                GetWallFromElevationInformation(lstFourElevationplanForWallDoorInfo, ref objUnitWallDoorWindowInformation);
                //dxfProcessorInput.UnitWallDoorWindowInformation = objUnitWallDoorWindowInformation;
                //
                if (objUnitWallDoorWindowInformation != null && objUnitWallDoorWindowInformation.UnitsWall.Count() > 0)
                {
                    string[] arrElevation = new string[] { DxfLayersName.FrontElevationplan, DxfLayersName.RearElevationplan, DxfLayersName.Side1Elevationplan, DxfLayersName.Side2Elevationplan };
                    foreach (string str in arrElevation)
                    {

                        // Calculate Front wall,door and window
                        List<SingleUnitWallDoorWindowInformation> lstFront = objUnitWallDoorWindowInformation.UnitsWall.Where(x => string.Equals(x.LayerName, str)).ToList();

                        foreach (SingleUnit objUnit in dxfProcessorInput.UnitData.Units)
                        {
                            if (str == DxfLayersName.FrontElevationplan)
                            {
                                objUnit.FrontWallArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.WallArea); //Wall
                                objUnit.FrontWindowArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.Window); //Window
                                objUnit.FrontDoorArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.Door); //Door

                                objUnit.FrontWallRatio = ((objUnit.FrontWindowArea + objUnit.FrontDoorArea) * 100) / objUnit.FrontWallArea;
                            }
                            if (str == DxfLayersName.RearElevationplan)
                            {
                                objUnit.RearWallArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.WallArea); //Wall
                                objUnit.RearWindowArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.Window); //Window
                                objUnit.RearDoorArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.Door); //Door

                                objUnit.RearWallRatio = ((objUnit.RearWindowArea + objUnit.RearDoorArea) * 100) / objUnit.RearWallArea;
                            }
                            if (str ==  DxfLayersName.Side1Elevationplan)
                            {
                                objUnit.Side1WallArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.WallArea); //Wall
                                objUnit.Side1WindowArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.Window); //Window
                                objUnit.Side1DoorArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.Door); //Door

                                objUnit.Side1WallRatio = ((objUnit.Side1WindowArea + objUnit.Side1DoorArea) * 100) / objUnit.Side1WallArea;
                            }
                            if (str == DxfLayersName.Side2Elevationplan)
                            {
                                objUnit.Side2WallArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.WallArea); //Wall
                                objUnit.Side2WindowArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.Window); //Window
                                objUnit.Side2DoorArea = lstFront.Where(x => string.Equals(x.Name, objUnit.Name, StringComparison.OrdinalIgnoreCase)).ToList().Sum(x => x.Door); //Door

                                objUnit.Side2WallRatio = ((objUnit.Side2WindowArea + objUnit.Side2DoorArea) * 100) / objUnit.Side2WallArea;
                            }


                        } 
                    }
                }

                logTiming.Add($"Report end " + DateTime.Now.ToString("HH:mm:ss.fff"));


                dxfProcessorInput.SiteCoverageInfo.AccessoryRoomArea = 0;
                if(dxfProcessorInput.IsGarageExists)
                    dxfProcessorInput.SiteCoverageInfo.GarageArea = objGarageInformation.GarageDistanceWithUnit.Sum(x => x.Area);

                dxfProcessorInput.SiteCoverageInfo.TotalUnitArea = dxfProcessorInput.UnitData.Units.Sum(x=>x.Area);

                dxfProcessorInput.SiteCoverageInfo.SiteArea = objLotInformation.Area;
                dxfProcessorInput.SiteCoverageInfo.AccessoryRoomArea = objAccessoryInformation.AccessoryUnit.Sum(x => x.Area);

                File.WriteAllText(sDataJsonFile, JsonConvert.SerializeObject(lstResultWithText, Formatting.Indented));
                File.WriteAllText(sRuleInputJsonFile, JsonConvert.SerializeObject(dxfProcessorInput, Formatting.Indented));
                File.WriteAllText(sTimingFile, string.Join("\r\n", logTiming));
                string sx = "test";



                /*

                                List<Cordinates> lstUnitCords = listDistanceWithPoints.OrderBy(x => x.Distance).Select(x=>x.StartPoint).Take(2).ToList();
                                List<CLineSegment> lineMainRoad = MakeConnectedLines(lstOnLineCoordinate);

                                // Find distance from minimum
                                if( nearestUnit  != null)
                                {
                                    nearestUnit.Lines = MakeClosePolyLines(nearestUnit.Coordinates);
                                    List<CLineSegment> lines = nearestUnit.Lines;
                                    double? distance = null;
                                    foreach (Cordinates cord in lstOnLineCoordinate)
                                    { 
                                        List<double> lstDistance = FindAllDistanceBetweenPolygonAndPointUsingCoordinateAndLines(nearestUnit, cord);
                                        if (lstDistance != null)
                                            Console.WriteLine($"1 Distance: " + lstDistance.Max());
                                    }
                                }
                */

                #region comment

                //foreach (LayerDataWithText unitItem in lstUnit)
                //{
                //    unitItem.Lines = MakeClosePolyLines(unitItem.Coordinates);
                //    List<CLineSegment> lines = unitItem.Lines;
                //    double? distance = null;
                //    CLineSegment line = new CLineSegment();
                //    FindMinimumDistanceCenterLineToPolygonLineSegmentMidPoint(lstOnLineCoordinate, lines, ref distance, ref line);
                //    if(distance != null)
                //        Console.WriteLine($"1 Distance: " + distance);
                //}



                ////Find Unit which closed to main road line
                //if (lstResultWithText.Exists(x => x.LayerName.Equals(DxfLayersName.RearMargin)))
                //{
                //    LayerDataWithText rearLine = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.RearMargin)).First();
                //    rearLine.Lines = MakeConnectedLines(rearLine.Coordinates);
                //    Console.WriteLine("Rear middle : " + rearLine.Lines.First().MidPoint.ToString());

                //    foreach (LayerDataWithText unitItem in lstUnit)
                //    {
                //        Console.WriteLine("Distance: " + ExtractLayerText(unitItem, false, "") + " =>" + FindAllDistanceBetweenPolygonAndPointUsingCoordinateAndLines(unitItem, rearLine.Lines.First().MidPoint).Min().ToString());
                //    }

                //}

                ////Find Unit which closed opposite to main road line

                //foreach (LayerDataWithText unitItem in lstSiteplanUnit)
                //{
                //    foreach (LayerDataWithText lotItem in lstLot)
                //    {
                //        List<double> distance = FindAllDistanceBetweenTwoPolygonUsingCoordinateAndLines(unitItem, lotItem);
                //        Console.WriteLine($"{ExtractLayerText(unitItem, false, "")} and {ExtractLayerText(lotItem, false,"")} minimum={distance.Min()} , maximum={distance.Max()} " );
                //    }
                //}
                #endregion

                sbHeaderProcessTask.AppendLine("");
                sbHeaderProcessTask.AppendLine("-----------------------------------------------------------");
                sbHeaderProcessTask.AppendLine("Total Process Task End time " + DateTime.Now.ToString("HH:mm:ss.fffff") + ", Sec: " + DateTime.Now.Subtract(dtTaskStart).TotalSeconds + ", MS: " + DateTime.Now.Subtract(dtTaskStart).TotalMilliseconds);
                sbHeaderProcessTask.AppendLine("-----------------------------------------------------------");
                sbHeaderProcessTask.AppendLine("");
                sbHeaderProcessTask.AppendLine("");


                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Report generated successfully.");
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : ");

                dxfProcessorInput.ProcessStartTime = dtTaskStart;
                dxfProcessorInput.ProcessEndTime = DateTime.Now;

                logTiming.Add($"Report end " + DateTime.Now.ToString("HH:mm:ss.fff"));

                if (!Debugger.IsAttached)
                {
                    if (File.Exists(DXFFilePathToProcess))
                        File.Delete(DXFFilePathToProcess);
                }

                //Now write rule tester
                RuleTesterCommand ruleCommand = new RuleTesterCommand();
                ruleCommand.SubPlotUse = new List<SubPlotUseDetail>();
                ruleCommand.ObjProject= new Project();
                ruleCommand.RuleJsonFilePath = sRuleInputJsonFile;


                var objRuleTester = new
                {
                    RuleJsonFilePath = sRuleInputJsonFile,
                    ObjProject = new object(),   // empty object
                    SubPlotUse = new object[] { }
                };

                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
                    WriteIndented = true
                };


                string json = System.Text.Json.JsonSerializer.Serialize(objRuleTester, options);

                int iTry = 0;
                bool bDone = false;
                while (iTry < 15)
                {
                    if (File.Exists(sRuleInputJsonFile))
                    {
                        //Console.WriteLine(json);
                        File.WriteAllText(sRuleTesterInputJsonFile, json);
                        bDone = true;
                        break;
                    }
                    iTry++;
                    await Task.Delay(1000);
                }

                // forcefully done...
                if(!bDone)
                    File.WriteAllText(sRuleTesterInputJsonFile, json);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Write("Exception in file process :" + objDXFFileQueueItem.DxfFilePath);
                Console.WriteLine(ex.StackTrace);
            }
            return 1;
        }

        public void GetSetBack(List<LayerInfo> lstSiteplanInfo, List<LayerDataWithText> lstSiteplanRearMarginLine, ref SetBackInformation objSetBackData,ref LotInformation objLotInformation, ref FlankingInformation objFlankingInformation)
        {
            objSetBackData = new SetBackInformation();

            // Find the closes unit to main road
            LayerDataWithText nearestUnit = null;

            List<LayerInfo> lstSiteplanMainRoad = new List<LayerInfo>();
            List<LayerInfo> lstSiteplanUnit = new List<LayerInfo>();
            List<LayerInfo> lstSiteplanLot = new List<LayerInfo>();

            foreach (LayerInfo itemSiteplan in lstSiteplanInfo)
            {
                foreach (string sKey in itemSiteplan.Child.Keys)
                {
                    if (sKey.Equals(DxfLayersName.FrontRoad))
                        lstSiteplanMainRoad = itemSiteplan.Child[sKey];
                    else if (sKey.Equals(DxfLayersName.Unit))
                        lstSiteplanUnit = itemSiteplan.Child[sKey];
                    else if (sKey.Equals(DxfLayersName.Lot))
                        lstSiteplanLot = itemSiteplan.Child[sKey];
                }
            }

            ////Set back
            CLineSegment nearestLine = null;
            LayerDataWithText nearestUnitFromRear = null;
            DistanceWithCordinate nearestDistance = null;
            CLineSegment rearlineOnPlot = null;

            List<DistanceWithCordinate> lstDistance = new List<DistanceWithCordinate>();

            foreach (LayerInfo itemUnit in lstSiteplanUnit)
            {
                Console.WriteLine("Unit: " + ExtractLayerText(itemUnit, false, ""));
                List<CLineSegment> lstSmoothLine = objLineOperation.MergePolyLineSegments(itemUnit.Data.Lines, ref TempBufferString);

                List<CLineSegment> lstMidCords = lstSmoothLine;

                foreach (CLineSegment line in lstMidCords)
                {
                    Cordinates cordMid = line.MidPoint;
                    foreach (LayerDataWithText itemRear in lstSiteplanRearMarginLine)
                    {
                        List<CLineSegment> lstMeargeRearLine = objLineOperation.MergePolyLineSegments(itemRear.Lines, ref TempBufferString);
                        foreach (CLineSegment rearline in lstMeargeRearLine)
                        {
                            DistanceWithCordinate data = objLineOperation.GetPerpendicularDistanceWithCordsFromLineSegment(rearline, cordMid);
                            if (lstDistance.Count() == 0)
                            {
                                nearestLine = line;
                                nearestDistance = data;
                                nearestUnitFromRear = itemUnit.Data;
                                rearlineOnPlot = rearline;
                            }
                            else if (lstDistance.Count() > 0 && lstDistance.Min(x => x.Distance) > data.Distance)
                            {
                                nearestLine = line;
                                nearestDistance = data;
                                nearestUnitFromRear = itemUnit.Data;
                                rearlineOnPlot = rearline;
                            }

                            lstDistance.Add(data);
                        }
                    }
                }
            }

            //Find the side line of lot
            CLineSegment Side1OnPlot = null;
            CLineSegment Side2OnPlot = null;

            // Find line on lot which are overlap
            CLineSegment overlapLotLineWithRearLine = null;
            foreach (LayerInfo lot in lstSiteplanLot)
            {
                List<CLineSegment> lstSmoothLine = objLineOperation.MergePolyLineSegments(lot.Data.Lines, ref TempBufferString);
                foreach (CLineSegment line in lstSmoothLine)
                {
                    if (line.Equals(rearlineOnPlot) || objLineOperation.IsOverlapping(rearlineOnPlot, line))
                    {
                        overlapLotLineWithRearLine = line;
                    }
                }
            }

            foreach (LayerInfo lot in lstSiteplanLot)
            {
                List<CLineSegment> lstSmoothLine = objLineOperation.MergePolyLineSegments(lot.Data.Lines, ref TempBufferString);
                foreach (CLineSegment line in lstSmoothLine)
                {
                    if (line.Equals(overlapLotLineWithRearLine) == false && objLineOperation.IsOverlapping(overlapLotLineWithRearLine, line) == false && line.IsBothLineConnected(overlapLotLineWithRearLine) == true)
                    {
                        if (Side1OnPlot == null)
                            Side1OnPlot = line;
                        else
                            Side2OnPlot = line;
                    }
                }
            }

            //string unitName = ExtractLayerText(itemUnit.Data, false, "");

            DistanceWithCordinate Side1 = null;
            DistanceWithCordinate Side2 = null;

            List<DistanceWithCordinate> lstSide1DistanceUnitWise = new List<DistanceWithCordinate>();
            List<DistanceWithCordinate> lstSide2DistanceUnitWise = new List<DistanceWithCordinate>();

            foreach (LayerInfo unit in lstSiteplanUnit)
            {
                List<CLineSegment> lstUnitMeargeLines = objLineOperation.MergePolyLineSegments(unit.Data.Lines, ErrorAllowScale, ref TempBufferString);

                List<DistanceWithCordinate> lstSite1Distance = new List<DistanceWithCordinate>();
                List<DistanceWithCordinate> lstSite2Distance = new List<DistanceWithCordinate>();

                foreach (CLineSegment line in lstUnitMeargeLines)
                {
                    DistanceWithCordinate d1 = FindMinimumDistanceWithCordsBetweenTwoLines(Side1OnPlot, line);
                    DistanceWithCordinate d2 = FindMinimumDistanceWithCordsBetweenTwoLines(Side2OnPlot, line);

                    if (d1 != null)
                        lstSite1Distance.Add(d1);

                    if (d2 != null)
                        lstSite2Distance.Add(d2);
                }

                lstSide1DistanceUnitWise.Add(lstSite1Distance.Where(d => d.Distance == lstSite1Distance.Min(x => x.Distance)).First());
                lstSide2DistanceUnitWise.Add(lstSite2Distance.Where(d => d.Distance == lstSite2Distance.Min(x => x.Distance)).First());
            }

            objSetBackData.Rear = FormatFigureInDecimalPoint(nearestDistance.Distance);
            objSetBackData.Side1 = FormatFigureInDecimalPoint(lstSide1DistanceUnitWise.Min(x => x.Distance));
            objSetBackData.Side2 = FormatFigureInDecimalPoint(lstSide2DistanceUnitWise.Min(x => x.Distance));

            Console.WriteLine($"setback: " + nearestDistance.Distance);
            Console.WriteLine($"side1: " + lstSide1DistanceUnitWise.Min(x => x.Distance));
            Console.WriteLine($"side2: " + lstSide2DistanceUnitWise.Min(x => x.Distance));

            //Finding front distance
            double? minDistance = null;
            foreach (LayerInfo itemUnit in lstSiteplanUnit)
            {
                foreach (LayerInfo itemRoadInfo in lstSiteplanMainRoad)
                {
                    double? d = FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLines(itemUnit.Data, itemRoadInfo.Data);
                    if (d != null && minDistance == null)
                    {
                        minDistance = d;
                        nearestUnit = itemUnit.Data.DeepClone();
                    }
                    else if (d != null && minDistance != null && minDistance > d.Value)
                    {
                        minDistance = d;
                        nearestUnit = itemUnit.Data.DeepClone();
                    }
                }
            }

            //find main road line and its middle point
            List<Cordinates> lstOnLineCoordinate = new List<Cordinates>();
            foreach (LayerInfo item in lstSiteplanLot)
            {
                foreach (LayerInfo itemRoad in lstSiteplanMainRoad)
                {
                    foreach (CLineSegment lin in itemRoad.Data.Lines)
                    {
                        foreach (Cordinates cord in item.Data.Coordinates)
                        {
                            if (lin.IsPointOnLine(cord, ErrorAllowScale))
                            {
                                lstOnLineCoordinate.Add(cord);
                            }
                        }
                    }
                }
            }

            // find cordinates 
            foreach (LayerInfo itemRoad in lstSiteplanMainRoad)
            {
                foreach (Cordinates cord in itemRoad.Data.Coordinates.DeepClone())
                {
                    bool isFound = false;
                    foreach (Cordinates cord1 in lstOnLineCoordinate)
                    {
                        if (cord.Equals(cord1))
                        {
                            isFound = true;
                            break;
                        }
                    }

                    if (!isFound)
                    {
                        foreach (LayerInfo itemSiteLot in lstSiteplanLot)
                        {
                            foreach (CLineSegment line in itemSiteLot.Data.Lines)
                            {
                                if (line.IsPointOnLine(cord, ErrorAllowScale))
                                {
                                    lstOnLineCoordinate.Add(cord);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            List<Cordinates> lstOutsideCordOfMainRoad = new List<Cordinates>();
            foreach (LayerInfo item in lstSiteplanLot)
            {
                foreach (LayerInfo itemRoad in lstSiteplanMainRoad)
                {
                    foreach (Cordinates cord1 in itemRoad.Data.Coordinates.DeepClone())
                    {
                        if (IsInPolyUsingAngle(item.Data.Coordinates, cord1, ErrorAllowScale) == false)
                            lstOutsideCordOfMainRoad.Add(cord1);
                    }
                }
            }

            List<DistanceWithCordinate> listDistanceWithPoints = new List<DistanceWithCordinate>();

            List<CLineSegment> lineMainRoad = MakeConnectedLines(lstOnLineCoordinate);
            //List<CLineSegment> lineSmoodhMainRoad = objLineOperation.SmoodhPolyLineSegments(lineMainRoad, ref TempBufferString);

            List<CLineSegment> lstSmoothLines = objLineOperation.SmoodhPolyLineSegments(nearestUnit.Lines, ref TempBufferString);

            // avoid cordinate which are touched with the other unit
            foreach (Cordinates cord1 in nearestUnit.Coordinates)
            {
                double? dDistance = FindMiminumDistanceBetweenPointsToPoint(lstOutsideCordOfMainRoad, cord1);
                if (dDistance != null)
                    listDistanceWithPoints.Add(new DistanceWithCordinate { Distance = dDistance.Value, StartPoint = cord1 });
            }

            // Find the cordinate which are not on the lot 
            listDistanceWithPoints = listDistanceWithPoints.OrderBy(x => x.Distance).ToList().Take(2).ToList();

            List<CLineSegment> lineUnit = null;
            if (listDistanceWithPoints.Count() == 2)
            {
                lineUnit = new List<CLineSegment>();
                lineUnit.Add(new CLineSegment { StartPoint = listDistanceWithPoints[0].StartPoint, EndPoint = listDistanceWithPoints[1].StartPoint });
            }

            foreach (DistanceWithCordinate item in listDistanceWithPoints)
            {
                List<DistanceWithCordinate> lstAllDistance = new List<DistanceWithCordinate>();
                foreach (Cordinates cord in lstOnLineCoordinate)
                {
                    double lstTempDistance = item.StartPoint.GetDistanceFrom(cord);
                    lstAllDistance.Add(new DistanceWithCordinate { Distance = lstTempDistance, StartPoint = cord, EndPoint = item.StartPoint });
                }

                if (lineUnit != null)
                {
                    double? minPerpendicularDistance = null;
                    CLineSegment refline = new CLineSegment();
                    FindMinimumDistanceCenterLineToPolygonLineSegmentMidPointPerpendicular(lstOnLineCoordinate, lineUnit, ref minPerpendicularDistance, ref refline);
                    if (minPerpendicularDistance != null)
                        lstAllDistance.Add(new DistanceWithCordinate { Distance = minPerpendicularDistance.Value, StartPoint = refline.StartPoint, EndPoint = item.StartPoint });
                }

                Console.WriteLine("Min. distance " + lstAllDistance.Select(x => x.Distance).Min() + ", Max distance " + lstAllDistance.Select(x => x.Distance).Max());
                objSetBackData.FrontMax = FormatFigureInDecimalPoint(lstAllDistance.Select(x => x.Distance).Max());
                objSetBackData.FrontMin = FormatFigureInDecimalPoint(lstAllDistance.Select(x => x.Distance).Min());
            }

            // Find perpendicular distance from main road line to unit line
            double lotWidth = objLineOperation.GetSortestDistanceBetweenTwoLine(Side1OnPlot, Side2OnPlot);
            objLotInformation = new LotInformation();
            double totalLotArea = 0;
            foreach (LayerInfo lot in lstSiteplanLot)
            {
                //Console.WriteLine("Width: " + GetSideLength(lot.Data.Coordinates[0], lot.Data.Coordinates[1]));
                //Console.WriteLine("Depth: " + GetPerpendicularHeight(lot.Data.Coordinates[0], lot.Data.Coordinates[1], lot.Data.Coordinates[lot.Data.Coordinates.Count - 1]));

                //double minX = lot.Data.Coordinates.Min(p => p.X);
                //double maxX = lot.Data.Coordinates.Max(p => p.X);
                //double minY = lot.Data.Coordinates.Min(p => p.Y);
                //double maxY = lot.Data.Coordinates.Max(p => p.Y);

                //double width = maxX - minX;
                //double depth = maxY - minY;

                double width = overlapLotLineWithRearLine.Length;
                double depth = Side1OnPlot.Length >= Side2OnPlot.Length ? Side1OnPlot.Length : Side2OnPlot.Length;

                totalLotArea = FindAreaByCoordinates(lot.Data);

                if(lotWidth == width)
                    objLotInformation = new LotInformation { Name = ExtractLayerText(lot.Data, false, ""), Width = FormatFigureInDecimalPoint(width), Depth = FormatFigureInDecimalPoint(depth), Area = FormatFigureInDecimalPoint(totalLotArea) };
                else
                    objLotInformation = new LotInformation { Name = ExtractLayerText(lot.Data, false, ""), Width = FormatFigureInDecimalPoint(depth), Depth = FormatFigureInDecimalPoint(width), Area = FormatFigureInDecimalPoint(totalLotArea) };

                Console.WriteLine("Width: " + width);
                Console.WriteLine("Depth: " + depth);
                Console.WriteLine("Area: " + totalLotArea);
            }


            //Call Flanking  information
            GetFlankingInformation(lstSiteplanInfo, ref Side1OnPlot, ref Side1OnPlot, ref objFlankingInformation);
        }

        public void GetAccessoryInformation(List<LayerInfo> lstSiteplanInfo, ref AccessoryInformation objAccessoryInformation)
        {
            List<LayerInfo> lstSiteplanUnit = new List<LayerInfo>();
            List<LayerInfo> lstSiteplanAccessory = new List<LayerInfo>();

            if (lstSiteplanInfo[0].Child.ContainsKey(DxfLayersName.ABLine))
                lstSiteplanAccessory = lstSiteplanInfo[0].Child[DxfLayersName.ABLine];

            foreach (LayerInfo accessory in lstSiteplanAccessory)
            {
                string unitName = ExtractLayerText(accessory.Data, false, "");
                double area = FindAreaByCoordinates(accessory.Data);
                objAccessoryInformation.AccessoryUnit.Add(new SingleAccessoryUnit
                {
                    Name = unitName,
                    Area = area
                });
            }
        }
        public void GetGarageInformation(List<LayerInfo> lstSiteplanInfo, ref GarageInformation objGarageInformation,ref bool IsGarageExists)
        {
            List<LayerInfo> lstSiteplanUnit = new List<LayerInfo>();
            List<LayerInfo> lstSiteplanGarage = new List<LayerInfo>();

            if (lstSiteplanInfo[0].Child.ContainsKey(DxfLayersName.Unit))
                lstSiteplanUnit = lstSiteplanInfo[0].Child[DxfLayersName.Unit];

            if (lstSiteplanInfo[0].Child.ContainsKey(DxfLayersName.Garage))
                lstSiteplanGarage = lstSiteplanInfo[0].Child[DxfLayersName.Garage];

            foreach (LayerInfo unit in lstSiteplanUnit)
            {
                double minDistance = double.MaxValue;
                string unitName = ExtractLayerText(unit.Data, false, "");
                double garageArea = double.MaxValue;
                foreach (LayerInfo garage in lstSiteplanGarage)
                {
                    IsGarageExists = true;
                    double? distance = FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLines(unit.Data, garage.Data);
                    if (distance != null)
                    {
                        if(distance.Value < minDistance || minDistance == double.MaxValue)
                        {
                            minDistance = distance.Value;
                            garageArea = FindAreaByCoordinates(garage.Data.Coordinates);
                        }
                    }
                }

                if(minDistance != double.MaxValue)
                {
                    objGarageInformation.GarageDistanceWithUnit.Add(new SingleUnitWithGarage
                    {
                        Name = unitName,
                        Distance = FormatFigureInDecimalPoint(minDistance),
                        Area = garageArea
                    });
                    Console.WriteLine($"Garage distance from unit name: {unitName}, Distance: " + minDistance);
                }
                
            }
        }
        public void GetWallFromElevationInformation(List<LayerInfo> lstFourElevationplanForWallDoorInfo, ref UnitWallDoorWindowInformation objGarageInformation)
        {
            foreach (LayerInfo elevation in lstFourElevationplanForWallDoorInfo)
            {
                string elevationName = ExtractLayerText(elevation.Data, false,"");
                if (elevation.Child.ContainsKey(DxfLayersName.Wall))
                {
                    List<LayerInfo> lstWall = elevation.Child[DxfLayersName.Wall];
                    if (elevation.Child.ContainsKey(DxfLayersName.Window))
                    {
                        List<LayerInfo> lstWindow = elevation.Child[DxfLayersName.Window];
                        objLayerExtractor.ExtractChildLayersForParentLayer(lstWindow.Select(x => x.Data).ToList(), DxfLayersName.Window, DxfLayersName.Window, ref lstWall);

                    }

                    if (elevation.Child.ContainsKey(DxfLayersName.Door))
                    {
                        List<LayerInfo> lstDoor = elevation.Child[DxfLayersName.Door];
                        objLayerExtractor.ExtractChildLayersForParentLayer(lstDoor.Select(x => x.Data).ToList(), DxfLayersName.Door, DxfLayersName.Door, ref lstWall);
                    }
                     
                    // find area of all widnow and all
                    foreach (LayerInfo wall in lstWall)
                    {
                        string wallName = ExtractLayerText(wall.Data, false, "");
                        double wallArea = FindAreaByCoordinates(wall.Data);

                        List<double> lstDoorArea = new List<double>();
                        List<double> lstWindowArea = new List<double>();

                        if (wall.Child.ContainsKey(DxfLayersName.Door))
                        {
                            foreach(LayerInfo door in wall.Child[DxfLayersName.Door])
                            {
                                //string doorName = ExtractLayerText(door, false, "");
                                lstDoorArea.Add( FindAreaByCoordinates(door.Data));
                            }
                        }
                        if (wall.Child.ContainsKey(DxfLayersName.Window))
                        {
                            foreach (LayerInfo window in wall.Child[DxfLayersName.Window])
                            {
                                //string windowName = ExtractLayerText(window, false, "");
                                lstWindowArea.Add(FindAreaByCoordinates(window.Data));
                            }
                        }


                        objGarageInformation.UnitsWall.Add(new SingleUnitWallDoorWindowInformation
                        {
                            LayerName = elevation.Data.LayerName,
                            ElevationName = elevationName,
                            Name = wallName,
                            WallArea = wallArea,
                            Door = lstDoorArea.Sum(),
                            Window = lstWindowArea.Sum()
                        });
                    }
                }
            }
        }


        public void GetFlankingInformation(List<LayerInfo> lstSiteplanInfo,ref CLineSegment objSide1Line, ref CLineSegment objSide2Line, ref FlankingInformation objFlankingInformation)
        {
            List<LayerInfo> lstSiteplanUnit = new List<LayerInfo>();
            List<LayerInfo> lstSiteplanOtherRoad = new List<LayerInfo>();
            List<LayerInfo> lstSiteplanFrontRoad = new List<LayerInfo>();
            List<LayerInfo> lstSiteplanRearMarginLine = new List<LayerInfo>();

            List<CLineSegment> lstSiteLines = new List<CLineSegment>();
            if(objSide1Line != null)
                lstSiteLines.Add(objSide1Line);
            if (objSide2Line != null)
                lstSiteLines.Add(objSide2Line);

            if (lstSiteplanInfo[0].Child.ContainsKey(DxfLayersName.Unit))
                lstSiteplanUnit = lstSiteplanInfo[0].Child[DxfLayersName.Unit];

            if (lstSiteplanInfo[0].Child.ContainsKey(DxfLayersName.OtherRoad))
                lstSiteplanOtherRoad = lstSiteplanInfo[0].Child[DxfLayersName.OtherRoad];

            if (lstSiteplanInfo[0].Child.ContainsKey(DxfLayersName.RearMarginLine))
                lstSiteplanRearMarginLine = lstSiteplanInfo[0].Child[DxfLayersName.RearMarginLine];

            if (lstSiteplanInfo[0].Child.ContainsKey(DxfLayersName.FrontRoad))
                lstSiteplanFrontRoad = lstSiteplanInfo[0].Child[DxfLayersName.FrontRoad];

            //Check the other road is landning on side not rear or front side
            bool isValidOtherRoad = true;
            if(lstSiteplanRearMarginLine == null || lstSiteplanRearMarginLine.Count() == 0)
                isValidOtherRoad = false;

            foreach (LayerInfo otherRoad in lstSiteplanOtherRoad)
            {
                //Check multiple cord of rear line are laying on other road
                foreach (LayerInfo rearLine in lstSiteplanRearMarginLine)
                {
                    int cnt = 0;
                    foreach(Cordinates cord in  rearLine.Data.Coordinates)
                    {
                        foreach(CLineSegment line in otherRoad.Data.Lines)
                        {
                            if (line.IsPointOnLine(cord, ErrorAllowScale))
                                cnt++;
                        }
                    }

                    if(cnt > 1)
                        isValidOtherRoad = false;
                }

                //Check multiple cord of main road laying on other road
                foreach (LayerInfo frontRoad in lstSiteplanFrontRoad)
                {
                    int cnt = 0;
                    foreach (Cordinates cord in frontRoad.Data.Coordinates)
                    {
                        foreach (CLineSegment line in otherRoad.Data.Lines)
                        {
                            if (line.IsPointOnLine(cord, ErrorAllowScale))
                                cnt++;
                        }
                    }

                    if (cnt > 1)
                        isValidOtherRoad = false;
                }
            }


            if (isValidOtherRoad)
            {
                foreach (LayerInfo otherRoad in lstSiteplanOtherRoad)
                {
                    double minDistance = double.MaxValue;
                    string unitName = "";
                    string RoadName = "";

                    string roadName = ExtractLayerText(otherRoad.Data, false, "");

                    foreach (LayerInfo unit in lstSiteplanUnit)
                    {
                        string tmpName = ExtractLayerText(unit.Data, false, "");

                        double? distance = FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLines(unit.Data, otherRoad.Data);
                        if (distance != null)
                        {
                            if (distance.Value < minDistance || minDistance == double.MaxValue)
                            {
                                minDistance = distance.Value;
                                unitName = tmpName;
                                RoadName = roadName;
                            }
                        }
                    }

                    // Minimum road 
                    if (minDistance != double.MaxValue)
                    {
                        objFlankingInformation.FlankingRoad.Add(new SingleFlankingRoad
                        { 
                            UnitName = unitName,
                            RoadName = RoadName,
                            Distance = FormatFigureInDecimalPoint(minDistance)
                        });
                        Console.WriteLine($"Garage distance from unit name: {unitName}, Distance: " + minDistance);
                    }
                }
            }
            

        }

        public void GetUnitInformation(List<LayerInfo> lstSiteplanInfo, ref UnitInformation objUnitInformation)
        {
            List<LayerInfo> lstSiteplanUnit = new List<LayerInfo>();
            if (lstSiteplanInfo[0].Child.ContainsKey(DxfLayersName.Unit))
                lstSiteplanUnit = lstSiteplanInfo[0].Child[DxfLayersName.Unit];

            double totalUnitArea = 0;
            foreach (LayerInfo unit in lstSiteplanUnit)
            {
                double minX = unit.Data.Coordinates.Min(p => p.X);
                double maxX = unit.Data.Coordinates.Max(p => p.X);
                double minY = unit.Data.Coordinates.Min(p => p.Y);
                double maxY = unit.Data.Coordinates.Max(p => p.Y);

                double width = maxX - minX;
                double depth = maxY - minY;
                string unitName = ExtractLayerText(unit.Data, false, "");

                double unitArea = FindAreaByCoordinates(unit.Data);
                totalUnitArea += unitArea;

                objUnitInformation.Units.Add(new SingleUnit
                {
                    Area = FormatFigureInDecimalPoint(unitArea),
                    Depth = FormatFigureInDecimalPoint(depth),
                    Name = unitName,
                    Width = FormatFigureInDecimalPoint(width)
                });
                Console.WriteLine($"Unit name: {unitName}, Width: " + width + " , Depth: " + depth + " , Area: " + unitArea);
            }

            Console.WriteLine($"Total Unit Area: " + objUnitInformation.Units.Sum(x => x.Area));
        }

        public void GetLotInformation(List<LayerInfo> lstSiteplanLot, ref LotInformation objLotInformation)
        {
            objLotInformation = new LotInformation();
            double totalLotArea = 0;
            foreach (LayerInfo lot in lstSiteplanLot)
            {
                Console.WriteLine("Width: " + GetSideLength(lot.Data.Coordinates[0], lot.Data.Coordinates[1]));
                Console.WriteLine("Depth: " + GetPerpendicularHeight(lot.Data.Coordinates[0], lot.Data.Coordinates[1], lot.Data.Coordinates[lot.Data.Coordinates.Count - 1]));

                double minX = lot.Data.Coordinates.Min(p => p.X);
                double maxX = lot.Data.Coordinates.Max(p => p.X);
                double minY = lot.Data.Coordinates.Min(p => p.Y);
                double maxY = lot.Data.Coordinates.Max(p => p.Y);

                double width = maxX - minX;
                double depth = maxY - minY;

                totalLotArea = FindAreaByCoordinates(lot.Data);

                objLotInformation = new LotInformation { Name = ExtractLayerText(lot.Data, false, ""), Width = FormatFigureInDecimalPoint(width), Depth = FormatFigureInDecimalPoint(depth), Area = FormatFigureInDecimalPoint(totalLotArea) };
                Console.WriteLine("Width: " + width);
                Console.WriteLine("Depth: " + depth);
                Console.WriteLine("Area: " + totalLotArea);
            }
        }

        public void GetSectionalData(List<LayerDataWithText> lstResultWithText, ref SectionPlanData objSelectionPlanData)
        {

            objSelectionPlanData = new SectionPlanData();
            List<LayerDataWithText> lstSectionPlan = lstResultWithText.Where(x => x.LayerName.Equals(DxfLayersName.SectionPlan, StringComparison.OrdinalIgnoreCase)).ToList();
            List<LayerInfo> lstSectionplanInfo = objLayerExtractor.SetLayerInfo(lstSectionPlan, DxfLayersName.SectionPlan);
            List<LayerInfo> lstUnitInfo = new List<LayerInfo>();// lstResultWithText.Where(x => x.LayerName.ToLower() == DxfLayersName.Unit).ToList();

            objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.GradeLine, DxfLayersName.GradeLine, ref lstSectionplanInfo);
            objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.FloorJoist, DxfLayersName.FloorJoist, ref lstSectionplanInfo);
            objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.TopOfWallPlate, DxfLayersName.TopOfWallPlate, ref lstSectionplanInfo);
            objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.RoofLine, DxfLayersName.RoofLine, ref lstSectionplanInfo);
            objLayerExtractor.ExtractChildLayersForParentLayer(lstResultWithText, DxfLayersName.FloorLine, DxfLayersName.FloorLine, ref lstSectionplanInfo);

            foreach (LayerInfo section in lstSectionplanInfo)
            {
                if (section.Child.Keys.Contains(DxfLayersName.FloorJoist, StringComparer.OrdinalIgnoreCase) &&
                    section.Child.Keys.Contains(DxfLayersName.TopOfWallPlate, StringComparer.OrdinalIgnoreCase) &&
                    //section.Child.Keys.Contains(DxfLayersName.FloorLine, StringComparer.OrdinalIgnoreCase) &&
                    section.Child.Keys.Contains(DxfLayersName.RoofLine, StringComparer.OrdinalIgnoreCase) &&
                    section.Child.Keys.Contains(DxfLayersName.GradeLine, StringComparer.OrdinalIgnoreCase))
                {
                    List<LayerInfo> lstFloorJoist = section.Child[DxfLayersName.FloorJoist];
                    List<LayerInfo> lstTopWallPlate = section.Child[DxfLayersName.TopOfWallPlate];
                    List<LayerInfo> lstRoofLine = section.Child[DxfLayersName.RoofLine];
                    List<LayerInfo> lstGradLine = section.Child[DxfLayersName.GradeLine];
                    List<LayerInfo> lstFloorLine = null;
                    List<LayerInfo> lstGarage = null;

                    if (section.Child.Keys.Contains(DxfLayersName.Garage, StringComparer.OrdinalIgnoreCase))
                        lstGarage = section.Child[DxfLayersName.Garage];

                    if (section.Child.Keys.Contains(DxfLayersName.FloorLine, StringComparer.OrdinalIgnoreCase))
                        lstFloorLine = section.Child[DxfLayersName.FloorLine];

                    lstFloorJoist = lstFloorJoist.OrderBy(x => x.Data.Coordinates.Min(x => x.Y)).ToList();

                    int i = 1;
                    for (; i < lstFloorJoist.Count(); i++)
                    {
                        objSelectionPlanData.Height.Add(new SectionPlanFloorData
                        {
                            FloorIndex = i,
                            Height = FormatFigureInDecimalPoint(Math.Abs(lstFloorJoist[i - 1].Data.Coordinates.Max(x => x.Y) - lstFloorJoist[i].Data.Coordinates.Max(x => x.Y)))
                        });
                    }

                    if (section.Child.Keys.Contains(DxfLayersName.TopOfWallPlate, StringComparer.OrdinalIgnoreCase))
                    {
                        objSelectionPlanData.Height.Add(new SectionPlanFloorData
                        {
                            FloorIndex = i,
                            Height = FormatFigureInDecimalPoint(Math.Abs(lstFloorJoist[lstFloorJoist.Count() - 1].Data.Coordinates.Max(x => x.Y) - lstTopWallPlate[0].Data.Coordinates.Max(x => x.Y)))
                        });

                    }

                    // Garage distance from main unit
                    if (lstUnitInfo.Count() == 1 && lstGarage.Count() == 1)
                    {
                        List<double> lsDistances = FindAllDistanceBetweenTwoPolygonUsingCoordinateAndLines(lstUnitInfo[0].Data, lstGarage[0].Data);
                        Console.WriteLine("Garage to Unit distance : " + lsDistances.Min());
                    }

                    //roof height building height
                    if (section.Child.Keys.Contains(DxfLayersName.RoofLine, StringComparer.OrdinalIgnoreCase))
                    {
                        foreach (LayerInfo roofLine in section.Child[DxfLayersName.RoofLine].ToList())
                        {
                            double topCordinates = roofLine.Data.Coordinates.Max(x => x.Y);
                            List<CLineSegment> topLine = roofLine.Data.Lines;

                            List<CLineSegment> topLines = roofLine.Data.Lines;
                            Polygon inputPolygon = NetTopologySuiteUtility.BuildPolygon(topLines);

                            Polygon simplified = NetTopologySuiteUtility.SimplifyToQuadrilateral(inputPolygon);
                            // Print simplified quadrilateral
                            foreach (var coord in simplified.Coordinates)
                            {
                                Console.WriteLine($"Simplified: {coord.X}, {coord.Y}");
                            }


                            topLine = NetTopologySuiteUtility.ToCustomSegments(simplified);


                            //SimplifyToQuadrilateral
                            //topLine = objLineOperation.SmoodhPolyLineSegments(topLine, ref TempBufferString);
                            topLine = topLine.Where(x => x.MaxY == topCordinates).ToList();
                            if (topLine.Count() > 0)
                            {
                                if (lstFloorJoist.Count() > 0)
                                {
                                    double roofDistance = topLine[0].MidPoint.Y - lstFloorJoist[0].Data.Coordinates.Max(y => y.Y);
                                    //objSelectionPlanData.Roof.Add(new SectionPlanFloorData
                                    //{
                                    //    FloorIndex = 0,
                                    //    Height = FormatFigureInDecimalPoint(roofDistance)
                                    //});

                                    objSelectionPlanData.TotalHeight = FormatFigureInDecimalPoint(roofDistance);
                                }
                                else
                                {
                                    double roofDistance = topLine[0].MidPoint.Y - lstTopWallPlate[0].Data.Coordinates.Max(y => y.Y);
                                    //objSelectionPlanData.Roof.Add(new SectionPlanFloorData
                                    //{
                                    //    FloorIndex = 0,
                                    //    Height = FormatFigureInDecimalPoint(roofDistance)
                                    //});
                                    objSelectionPlanData.TotalHeight = FormatFigureInDecimalPoint(roofDistance);
                                }
                            }

                            //Total height
                            //double totalHeight = roofLine.Data.Coordinates.Max(x => x.Y) - lstGradLine[0].Data.Coordinates.Max(x => x.Y);
                            //objSelectionPlanData.TotalHeight = FormatFigureInDecimalPoint(totalHeight);
                        }
                    }
                }
            }

        }

        // Length of a side between two points
        public static double GetSideLength(Cordinates a, Cordinates b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        // Perpendicular distance from point c to line ab
        public static double GetPerpendicularHeight(Cordinates a, Cordinates b, Cordinates c)
        {
            double numerator = Math.Abs((b.Y - a.Y) * c.X - (b.X - a.X) * c.Y + b.X * a.Y - b.Y * a.X);
            double denominator = Math.Sqrt(Math.Pow(b.Y - a.Y, 2) + Math.Pow(b.X - a.X, 2));
            return numerator / denominator;
        }
        public DimensionBlock PrepareBoundary(List<LayerDataWithText> lstInputData, bool IsMakeClose, ref DimensionBlock plotDimension)
        { 
            List<LayerDataWithText> lstPrintArea = lstInputData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.PrintArea).ToList();
            string temp = "";
            foreach (LayerDataWithText item in lstInputData)
            {
                if (item.HasBulge)
                {
                    ProcessBulgeData(item, IsMakeClose, ref plotDimension);
                }
                else
                {
                    List<CLineSegment> allLines = null;
                    if (IsMakeClose)
                        allLines = MakeClosePolyLines(item.Coordinates);
                    else
                        allLines = MakeConnectedLines(item.Coordinates);

                    if (allLines != null)
                        allLines = objLineOperation.MergePolyLineSegments(allLines, ref temp);

                    foreach (CLineSegment line in allLines)
                    {
                        if (line.Length < 1)
                            continue;

                        plotDimension.LineDimensionBlockInfo.Add(new LineDimensionBlock
                        {
                            StartPoint = line.StartPoint,
                            EndPoint = line.EndPoint,
                            LayerName = item.LayerName
                        });
                    }
                }
            }
             
            return plotDimension;
        }
        public void ProcessBulgeData(LayerDataWithText item, bool IsMakeClose, ref DimensionBlock plotDimension)
        {
            string temp = "";
            if (item.CoordinateWithBulge != null)
            {
                List<Cordinates> lstBulgeCord = new List<Cordinates>();

                int iTotal = item.CoordinateWithBulge.Count;

                BulgeItem cordFirst = null;
                BulgeItem cordLast = null;
                for (int iCnt = 0; iCnt < iTotal; iCnt++)
                {
                    BulgeItem bulgeItem = item.CoordinateWithBulge[iCnt];
                    if (IsMakeClose)
                    {
                        if (iCnt == 0)
                            cordFirst = bulgeItem;
                        else if (iCnt == iTotal - 1)
                            cordLast = bulgeItem;
                    }

                    BulgeItemValue bulgeValue = bulgeItem.ItemValue as BulgeItemValue;
                    if (bulgeItem.IsBulgeValue)
                    {
                        if (!bulgeValue.StartPoint.Equals(bulgeValue.EndPoint))
                        {
                            if (bulgeItem.ItemValue.Bulge == 0)
                            {
                                lstBulgeCord.Add(bulgeValue.StartPoint);
                                lstBulgeCord.Add(bulgeValue.EndPoint);
                            }
                            else
                            {
                                if (lstBulgeCord != null)
                                {
                                    if (iCnt > 0 && !item.CoordinateWithBulge[iCnt - 1].IsBulgeValue)
                                        lstBulgeCord.Add(bulgeItem.ItemValue.StartPoint);

                                    if (lstBulgeCord != null && lstBulgeCord.Count > 0)
                                    {
                                        List<CLineSegment> allLines = MakeConnectedLines(lstBulgeCord);
                                        if (allLines != null)
                                            allLines = objLineOperation.MergePolyLineSegments(allLines, ref temp);

                                        foreach (CLineSegment line in allLines)
                                        {
                                            plotDimension.LineDimensionBlockInfo.Add(new LineDimensionBlock
                                            {
                                                StartPoint = line.StartPoint,
                                                EndPoint = line.EndPoint,
                                                LayerName = item.LayerName
                                            });
                                        }
                                    }
                                }

                                lstBulgeCord = new List<Cordinates>();

                                ArcSegment arcmy = new ArcSegment(bulgeItem.ItemValue.StartPoint, bulgeItem.ItemValue.EndPoint, bulgeItem.ItemValue.Bulge);
                                //plotDimension.BulgeDimensionBlockInfo.Add(new BulgeDimensionBlock { StartPoint = bulgeItem.ItemValue.StartPoint, EndPoint = bulgeItem.ItemValue.EndPoint, Bulge = bulgeItem.ItemValue.Bulge });
                                //plotDimension.ArcDimensionBlockInfo.Add(new ArcDimensionBlock { CenterPoint = arcmy.CenterPoint, StartPoint= bulgeItem.ItemValue.StartPoint, EndPoint = bulgeItem.ItemValue.EndPoint, Radius = arcmy.Radius, StartAngle = arcmy.StartAngleDegree, EndAngle = arcmy.EndAngleDegree, Text = arcmy.Length.ToString("0.00", CultureInfo.InvariantCulture) });

                                plotDimension.ArcDimensionBlockInfo.Add(new ArcDimensionBlock { CenterPoint = arcmy.CenterPoint, Radius = arcmy.Radius, StartAngle = arcmy.StartAngleDegree, EndAngle = arcmy.EndAngleDegree, Text = arcmy.Length.ToString("0.00", CultureInfo.InvariantCulture) });
                            }
                        }
                    }
                    else
                    {
                        if (iCnt > 0 && item.CoordinateWithBulge[iCnt - 1].IsBulgeValue)
                            lstBulgeCord.Add(item.CoordinateWithBulge[iCnt - 1].ItemValue.EndPoint);

                        lstBulgeCord.Add(bulgeValue.StartPoint);
                    }

                } // for each bulge 

                if (IsMakeClose)
                {
                    if (cordFirst != null && cordLast != null)
                    {
                        if (!cordFirst.IsBulgeValue && !cordLast.IsBulgeValue)
                        {
                            BulgeItemValue firstBulgeValue = cordFirst.ItemValue as BulgeItemValue;
                            BulgeItemValue lastBulgeValue = cordLast.ItemValue as BulgeItemValue;

                            if (!firstBulgeValue.StartPoint.Equals(lastBulgeValue.StartPoint))
                            {
                                lstBulgeCord.Add(firstBulgeValue.StartPoint);
                            }
                        }
                        else if (!cordFirst.IsBulgeValue && cordLast.IsBulgeValue)
                        {
                            BulgeItemValue firstBulgeValue = cordFirst.ItemValue as BulgeItemValue;
                            BulgeItemValue lastBulgeValue = cordLast.ItemValue as BulgeItemValue;

                            if (!firstBulgeValue.StartPoint.Equals(lastBulgeValue.EndPoint))
                            {
                                lstBulgeCord.Add(firstBulgeValue.StartPoint);
                            }
                        }
                        else if (cordFirst.IsBulgeValue && !cordLast.IsBulgeValue)
                        {
                            BulgeItemValue firstBulgeValue = cordFirst.ItemValue as BulgeItemValue;
                            BulgeItemValue lastBulgeValue = cordLast.ItemValue as BulgeItemValue;

                            if (!firstBulgeValue.StartPoint.Equals(lastBulgeValue.StartPoint))
                            {
                                lstBulgeCord.Add(firstBulgeValue.StartPoint);
                            }
                        }
                        else if (cordFirst.IsBulgeValue && cordLast.IsBulgeValue)
                        {
                            BulgeItemValue firstBulgeValue = cordFirst.ItemValue as BulgeItemValue;
                            BulgeItemValue lastBulgeValue = cordLast.ItemValue as BulgeItemValue;

                            if (!firstBulgeValue.StartPoint.Equals(lastBulgeValue.EndPoint))
                            {
                                lstBulgeCord.Add(firstBulgeValue.StartPoint);
                            }
                        }
                    }
                }

                if (lstBulgeCord != null && lstBulgeCord.Count > 0)
                {
                    List<CLineSegment> allLines = MakeConnectedLines(lstBulgeCord);
                    if (allLines != null)
                        allLines = objLineOperation.MergePolyLineSegments(allLines, ref temp);

                    foreach (CLineSegment line in allLines)
                    {
                        plotDimension.LineDimensionBlockInfo.Add(new LineDimensionBlock
                        {
                            StartPoint = line.StartPoint,
                            EndPoint = line.EndPoint,
                            LayerName = item.LayerName
                        });
                    }
                }
            }
        }
        public void WriteNewReport(string sFile, string content)
        {
            File.WriteAllText(sFile, "");
        }
        public void AppendReport(string sFile, string content)
        {
            File.AppendAllText(sFile, content + Environment.NewLine);
        }
         public void FormatReportData(string sFile)
        {
            if (!File.Exists(sFile))
                return;

            string sData = File.ReadAllText(sFile);
            sData = sData.Replace("\r", "");
            while (sData.IndexOf("\n\n\n") != -1)
            {
                sData = sData.Replace("\n\n\n", "\n\n");
            }

            sData = sData.Replace("\n", "\r\n");

            File.WriteAllText(sFile, sData);
        }
        public void UpdateReportHeaderData(string sFile, string header)
        {
            if (!File.Exists(sFile))
                return;

            string sData = File.ReadAllText(sFile);

            File.WriteAllText(sFile, header + "\r\n\r\n" + sData);
        }
    
        //Following move to layer extractor
        public void ProcessPolygonMergeLineCoordinates(ref List<LayerDataWithText> lstInput)
        {
            int iCounter = lstInput.Count;
            for (int iLayerCnt = 0; iLayerCnt < iCounter; iLayerCnt++)
            {
                LayerDataWithText layer = lstInput[iLayerCnt];
                if (layer.Coordinates.Count < 4)
                    continue;

                List<Cordinates> lstCords = new List<Cordinates>();
                lstCords.AddRange(layer.Coordinates);

                List<CLineSegment> lstLines = MakeConnectedLines(lstCords);

                bool bFound = true;
                while (bFound)
                {
                    int iCount = lstLines.Count;
                    bFound = false;
                    for (int i = 0; i < iCount - 1; i++)
                    {
                        double length = lstLines[i].Length + lstLines[i + 1].Length;
                        CLineSegment lineNew = new CLineSegment { StartPoint = lstLines[i].StartPoint, EndPoint = lstLines[i + 1].EndPoint };
                        if (lstLines[i].IsParallel(lstLines[i + 1], 0.005) && lineNew.IsPointOnLine(lstLines[i + 1].StartPoint, ErrorAllowScale))
                        {
                            lstLines[i + 1].StartPoint = lstLines[i].StartPoint;
                            lstLines[i] = null;
                            bFound = true;
                        }
                    }

                    lstLines = lstLines.Where(x => x != null).ToList();

                    if (bFound == true)
                    {
                        iCount = lstLines.Count;
                        lstCords = new List<Cordinates>();
                        lstCords.Add(lstLines[0].StartPoint);
                        for (int i = 1; i < iCount; i++)
                        {
                            if (lstLines[i - 1].EndPoint.X != lstLines[i].StartPoint.X && lstLines[i - 1].EndPoint.Y != lstLines[i].StartPoint.Y)
                                lstCords.Add(lstLines[i - 1].EndPoint);

                            lstCords.Add(lstLines[i].StartPoint);
                            if (i == iCount - 1)
                                lstCords.Add(lstLines[i].EndPoint);
                        }

                        lstLines = MakeConnectedLines(lstCords);
                        if (lstLines.Count > 3)
                        {
                            lstLines.Add(lstLines[0]);
                            lstLines.Add(lstLines[1]);
                            lstLines.Add(lstLines[2]);
                            lstLines[0] = null;
                            lstLines[1] = null;
                            lstLines[2] = null;
                        }
                        else if (lstLines.Count > 2)
                        {
                            lstLines.Add(lstLines[0]);
                            lstLines.Add(lstLines[1]);
                            lstLines[0] = null;
                            lstLines[1] = null;
                        }
                    }

                    lstLines = lstLines.Where(x => x != null).ToList();
                }

                lstLines = lstLines.Where(x => x != null).ToList();
                int iCount1 = lstLines.Count;
                lstCords = new List<Cordinates>();
                lstCords.Add(lstLines[0].StartPoint);
                for (int i = 1; i < iCount1; i++)
                {
                    if (lstLines[i - 1].EndPoint.X != lstLines[i].StartPoint.X && lstLines[i - 1].EndPoint.Y != lstLines[i].StartPoint.Y)
                        lstCords.Add(lstLines[i - 1].EndPoint);

                    lstCords.Add(lstLines[i].StartPoint);

                    if (i == iCount1 - 1)
                    {
                        if (!(lstLines[i].EndPoint.X == lstLines[0].StartPoint.X && lstLines[i].EndPoint.Y == lstLines[0].StartPoint.Y))
                            lstCords.Add(lstLines[i].EndPoint);
                    }
                }

                lstCords = lstCords.Where(x => x != null).ToList();

                lstInput[iLayerCnt].Coordinates = lstCords;
            }
        }
        public void ProcessToSwapLineXYCoordinateSort(ref List<LayerDataWithText> lstInput)
        {
            List<LayerDataWithText> lstLineData = lstInput.Where(x => x.Coordinates.Count == 2 && !CheckLineTypeIsCenterLine(x.LineType)).ToList();
            foreach (LayerDataWithText layer in lstLineData)
            {
                layer.Lines = ConvertCoordinateToLineWithSortAndSwap(layer.Coordinates);
            }
        }
        public void ProcessToClearDuplicateSequenceCoordinate(ref List<LayerDataWithText> lstInput)
        {
            if (lstInput == null || lstInput.Count == 0)
                return;

            int iTotalLayer = lstInput.Count;
            for (int iCnt = 0; iCnt < iTotalLayer; iCnt++)
            {
                if (lstInput[iCnt].Coordinates.Count > 1)
                {
                    int i = 0;
                    int iCompareWithIndex = 0;
                    while (i < lstInput[iCnt].Coordinates.Count - 1)
                    {
                        i++;
                        if (lstInput[iCnt].Coordinates[i] == null)
                            continue;

                        if (lstInput[iCnt].Coordinates[iCompareWithIndex].Equals(lstInput[iCnt].Coordinates[i]))
                            lstInput[iCnt].Coordinates[i] = null;
                        else
                            iCompareWithIndex = i;
                    }
                }
                lstInput[iCnt].Coordinates = lstInput[iCnt].Coordinates.Where(x => x != null).ToList();

                //now only one point remains to remove it.
                if (lstInput[iCnt].Coordinates.Count == 1 && lstInput[iCnt].IsCircle == false && lstInput[iCnt].HasBulge == false)
                    lstInput[iCnt].Coordinates = null;
            }

            lstInput = lstInput.Where(x => x.Coordinates != null).ToList();
        }

        //Added on 12Jan2024 
        public void ProcessToCommonPlotName(ref List<LayerDataWithText> lstInput)
        {
            if (lstInput == null || lstInput.Count == 0)
                return;

            int iTotalLayer = lstInput.Count;
            int icnt = 1;
            for (int iCnt = 0; iCnt < iTotalLayer; iCnt++)
            {
                if (lstInput[iCnt].LayerName.ToLower().Trim() != DxfLayersName.CommonPlot.ToLower().Trim() || CheckLineTypeIsCenterLine(lstInput[iCnt].LineType) || lstInput[iCnt].Coordinates == null || lstInput[iCnt].Coordinates.Count <= 2)
                    continue;

                string sText = ExtractLayerText(lstInput[iCnt], false, "");
                if (string.IsNullOrWhiteSpace(sText))
                {
                    if (lstInput[iCnt].TextInfoData == null || lstInput[iCnt].TextInfoData.Count() == 0)
                    {
                        lstInput[iCnt].TextInfoData = new List<LayerTextInfo>();
                        LayerTextInfo itemText = new LayerTextInfo();
                        itemText.Text = string.Format(CommonPlotText, icnt);
                        itemText.Coordinates = new List<Cordinates>();
                        itemText.Coordinates.Add(lstInput[iCnt].Coordinates.First());

                        lstInput[iCnt].TextInfoData.Add(itemText);
                    }
                    else
                    {
                        if (lstInput[iCnt].TextInfoData[0] == null || string.IsNullOrEmpty(lstInput[iCnt].TextInfoData[0].Text))
                        {
                            lstInput[iCnt].TextInfoData = new List<LayerTextInfo>();
                            LayerTextInfo itemText = new LayerTextInfo();
                            itemText.Text = string.Format(CommonPlotText, icnt);
                            itemText.Coordinates = new List<Cordinates>();
                            itemText.Coordinates.Add(lstInput[iCnt].Coordinates.First());

                            lstInput[iCnt].TextInfoData.Add(itemText);
                        }
                    }
                    icnt++;

                }
            }
        }


        //Added on 29Nov022
        public void ProcessToClosePolygon(ref List<LayerDataWithText> lstInput)
        {
            if (lstInput == null || lstInput.Count == 0)
                return;

            string[] arrIgnoreLayers = new string[] { DxfLayersName.MarginLine, DxfLayersName.GroundLevel, DxfLayersName.HighFloodLevel };
            int iTotalLayer = lstInput.Count;
            for (int iCnt = 0; iCnt < iTotalLayer; iCnt++)
            {
                if (arrIgnoreLayers.Contains(lstInput[iCnt].LayerName.ToLower().Trim()) || lstInput[iCnt].IsCircle ||
                    CheckLineTypeIsCenterLine(lstInput[iCnt].LineType) || lstInput[iCnt].Command.ToLower().Trim() != DxfLayersName.PolyLine || lstInput[iCnt].Coordinates == null || lstInput[iCnt].Coordinates.Count <= 3)
                    continue;

                LayerDataWithText dataForArea = lstInput[iCnt].DeepClone();
                if (lstInput[iCnt].Coordinates.Count >= 3 && !CheckLineTypeIsCenterLine(lstInput[iCnt].LineType.ToLower().Trim()))
                {
                    if (!lstInput[iCnt].Coordinates[0].Equals(lstInput[iCnt].Coordinates.Last()))
                        lstInput[iCnt].Coordinates.Add(lstInput[iCnt].Coordinates[0]);
                }
            }
        }

        public void MakeAllBulgePolyToProcess(ref List<LayerDataWithText> lstInput)
        {
            if (lstInput == null || lstInput.Count == 0)
                return;
             
            for (int iCnt = 0; iCnt < lstInput.Count; iCnt++)
            {
                if (lstInput[iCnt].HasBulge)
                {
                    LayerDataWithText temp = lstInput[iCnt].DeepClone();
                    SetAdjustCoordinate(ref temp);
                    temp.Lines = MakeClosePolyLines(lstInput[iCnt].Coordinates);
                    lstInput[iCnt] = temp.DeepClone();
                }
                else
                {
                    lstInput[iCnt].Lines =  MakeClosePolyLines(lstInput[iCnt].Coordinates);
                }
            } 
        }

        //added on 05May2022 
        public void ProcessToCleanZeroAreaPolygon(ref List<LayerDataWithText> lstInput)
        {
            if (lstInput == null || lstInput.Count == 0)
                return;

            int iTotalLayer = lstInput.Count;
            for (int iCnt = 0; iCnt < iTotalLayer; iCnt++)
            {
                try
                {     
                    LayerDataWithText dataForArea = lstInput[iCnt].DeepClone();
                    if (lstInput[iCnt].Coordinates.Count > 3 && lstInput[iCnt].Command.ToLower().Trim() == DxfLayersName.PolyLine && FindAreaByCoordinates(dataForArea) == 0)
                    {
                        lstInput[iCnt] = null;
                    }
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                }
            }

            lstInput = lstInput.Where(x => x != null).ToList();
        }
        public List<LayerInfo> ClearSubLayerWithInLayer(ref List<LayerInfo> lstInput)
        {
            if (lstInput == null || lstInput.Count == 0)
                return null;

            int iTotal = lstInput.Count;
            for (int i = 0; i < iTotal; i++)
            {
                if (lstInput[i] == null || lstInput[i].Data == null || lstInput[i].Data.Coordinates == null)
                    continue;

                for (int j = 0; j < iTotal; j++)
                {
                    if (i == j || lstInput[j] == null || lstInput[j].Data == null || lstInput[j].Data.Coordinates == null)
                        continue;

                    if (IsInPolyUsingAngle(lstInput[i].Data.Coordinates, lstInput[j].Data.Coordinates))
                        lstInput[j] = null;
                }
            }

            return lstInput.Where(x => x != null).ToList();
        }
        public List<LayerInfo> FilterOnlyPolygonData(List<LayerInfo> lstLayerInfo, bool FilterWithoutCenterLine = false)
        {
            if (lstLayerInfo == null || lstLayerInfo.Count == 0)
                return lstLayerInfo;

            lstLayerInfo = lstLayerInfo.Where(x => x.Data.Coordinates != null && x.Data.Coordinates.Count > 3).ToList();

            if (FilterWithoutCenterLine)
                lstLayerInfo = lstLayerInfo.Where(x => !CheckLineTypeIsCenterLine(x.Data.LineType)).ToList(); //.Trim().ToUpper() != DxfLayersName.CenterLine

            return lstLayerInfo;
        }
        public List<LayerInfo> FilterOnlyPolyLineData(List<LayerInfo> lstLayerInfo, bool FilterWithoutCenterLine = false)
        {
            if (lstLayerInfo == null || lstLayerInfo.Count == 0)
                return lstLayerInfo;

            lstLayerInfo = lstLayerInfo.Where(x => x.Data.Coordinates != null && x.Data.Coordinates.Count == 2).ToList();

            if (FilterWithoutCenterLine)
                lstLayerInfo = lstLayerInfo.Where(x => !CheckLineTypeIsCenterLine(x.Data.LineType)).ToList(); //.Trim().ToUpper() != DxfLayersName.CenterLine

            return lstLayerInfo;
        }
        public List<LayerInfo> FilterOnlyByLine(List<LayerInfo> lstLayerInfo, string LineType)
        {
            if (lstLayerInfo == null || lstLayerInfo.Count == 0)
                return lstLayerInfo;

            lstLayerInfo = lstLayerInfo.Where(x => x.Data.LineType == LineType).ToList();

            return lstLayerInfo;
        }
        public void RemoveAllPolygonWhichInsideLayer(ref List<LayerInfo> lstParentInfo, ref List<LayerInfo> lstChildLayerInfo)
        {
            if (lstChildLayerInfo == null || lstChildLayerInfo.Count == 0)
                return;

            if (lstParentInfo == null || lstParentInfo.Count == 0)
                return;

            //Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Parent count: " + lstParentInfo.Count + ", Child count: " + lstChildLayerInfo.Count);

            for (int j = 0; j < lstParentInfo.Count; j++)
            {
                for (int iCnt = 0; iCnt < lstChildLayerInfo.Count; iCnt++)
                {
                    if (IsInPolyUsingAngle(lstParentInfo[j].Data.Coordinates, lstChildLayerInfo[iCnt].Data.Coordinates))
                        lstChildLayerInfo[iCnt] = null;
                }
                lstChildLayerInfo = lstChildLayerInfo.Where(x => x != null).ToList();
            }

            lstChildLayerInfo = lstChildLayerInfo.Where(x => x != null).ToList();
        }
        public void RemoveAllWallPolygonWhichTouchedInLayer(ref List<LayerInfo> lstParentInfo, ref List<LayerInfo> lstWallLayerInfo, ref List<LayerInfo> lstWallWithTouch)
        {
            if (lstParentInfo == null || lstParentInfo.Count == 0)
                return;

            lstWallWithTouch = new List<LayerInfo>();
            if (lstWallLayerInfo == null || lstWallLayerInfo.Count == 0)
                return;

            if (lstParentInfo == null || lstParentInfo.Count == 0)
                return;

            bool bFound = false;
            for (int iCnt = 0; iCnt < lstWallLayerInfo.Count; iCnt++)
            {
                List<Cordinates> lstCords = lstWallLayerInfo[iCnt].Data.Coordinates;
                bFound = false;

                for (int j = 0; j < lstParentInfo.Count; j++)
                {
                    if (lstParentInfo[j].Data.Lines == null || lstParentInfo[j].Data.Lines.Count == 0 || lstParentInfo[j].Data.Coordinates == null || lstParentInfo[j].Data.Coordinates.Count == 0)
                        continue;

                    if (lstParentInfo[j].Data.Lines == null || lstParentInfo[j].Data.Lines.Count == 0 && (lstParentInfo[j].Data.Coordinates == null || lstParentInfo[j].Data.Coordinates.Count == 0))
                    {
                        lstParentInfo[j].Data.Lines = MakeClosePolyLines(lstParentInfo[j].Data.Coordinates);
                    }

                    List<CLineSegment> lstParentLines = lstParentInfo[j].Data.Lines;
                    foreach (CLineSegment line in lstParentLines)
                    {
                        foreach (Cordinates cord in lstCords)
                        {
                            if (line.IsPointOnLine(cord, General.ErrorAllowScale))
                            {
                                lstWallWithTouch.Add(lstWallLayerInfo[iCnt]);
                                lstWallLayerInfo[iCnt] = null;
                                bFound = true;
                                break;
                            }
                        }
                        if (bFound) break;
                    }
                    if (bFound) break;
                }
            }

            lstWallLayerInfo = lstWallLayerInfo.Where(x => x != null).ToList();
        }
        public void AddAllPolygonInsideLayer(ref List<LayerInfo> lstParentInfo, List<LayerInfo> lstChildLayerInfo)
        {
            if (lstChildLayerInfo == null || lstChildLayerInfo.Count == 0)
                return;

            if (lstParentInfo == null || lstParentInfo.Count == 0)
                return;

            lstParentInfo.AddRange(lstChildLayerInfo.ToList());
        }

        //Circle to Coordinate and lines layer
        public List<LayerDataWithText> GetCircleToCoordinateLayer(List<LayerDataWithText> lstMarginLineCircle)
        {
            List<LayerDataWithText> lstCircleToLines = new List<LayerDataWithText>();
            if (lstMarginLineCircle != null && lstMarginLineCircle.Count > 0)
            {
                foreach (LayerDataWithText lineCircle in lstMarginLineCircle)
                {
                    ArcSegment arc = new ArcSegment(lineCircle.CenterPoint, lineCircle.Radius, DegreesToRadians(lineCircle.StartAngle), DegreesToRadians(lineCircle.EndAngle));
                    List<CLineSegment> lstLines = arc.GetArcLineSegments();

                    if (lstLines != null && lstLines.Count > 0)
                    {
                        LayerDataWithText objCircleLines = new LayerDataWithText();
                        objCircleLines.TextInfoData = lineCircle.TextInfoData;
                        objCircleLines.Coordinates = lineCircle.Coordinates;
                        objCircleLines.ColourCode = lineCircle.ColourCode;
                        objCircleLines.Command = lineCircle.Command;
                        objCircleLines.HasBulge = lineCircle.HasBulge;
                        objCircleLines.IsCircle = lineCircle.IsCircle;
                        objCircleLines.Radius = lineCircle.Radius;
                        objCircleLines.StartAngle = lineCircle.StartAngle;
                        objCircleLines.EndAngle = lineCircle.EndAngle;
                        objCircleLines.Coordinates = new List<Cordinates>();

                        objCircleLines.Coordinates.Add(lstLines[0].StartPoint);
                        foreach (CLineSegment line in lstLines)
                            objCircleLines.Coordinates.Add(line.EndPoint);

                        lstCircleToLines.Add(objCircleLines);
                    }
                } // foreach
            }

            return lstCircleToLines;
        }

    } // class

} // namespace
