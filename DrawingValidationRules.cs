using EdmontonDrawingValidator.Model;
using EdmontonDrawingValidator;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using netDxf.Tables;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.HPRtree;
using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using SharedClasses;
using SharedClasses.Constants;


namespace EdmontonDrawingValidator
{
    public sealed class DrawingValidationRules : MathLib
    {
        private LayerExtractor objLayerExtractor = new LayerExtractor();
        private LineOperations objLineOperation = new LineOperations();
        private NetTopologySuiteUtility objNetTopologySuite = new NetTopologySuiteUtility();

        private Dictionary<string, List<LayerDataWithText>> htLayerData = new Dictionary<string, List<LayerDataWithText>>();
        List<LayerDataWithText> allLayerData = new List<LayerDataWithText>();
        //List<BuildingNamingMapping> buildingFloorMappingData = new List<BuildingNamingMapping>();
        //List<BuildingNameWithProposeAndFloorMap> BuildingFloorNameMap = new List<BuildingNameWithProposeAndFloorMap>();
        Dictionary<string, string> dictLayerDefaultColour = new Dictionary<string, string>();
        private bool IsDxfLayerHasError = false;
        private List<string> lstErrorMessages = new List<string>();
        private string projectType = "";
        string sFileINProcess = "";
        public DrawingValidationRules(string fileInProcess, string Project_Type, Dictionary<string, List<LayerDataWithText>> LayerData, Dictionary<string, string> dictLayerWiseDefaultColour, List<LayerDataWithText> AllLayersData, bool bDxfHasErrorFound, List<string> lstErrorMessageWhileDxfExtract)
        {
            sFileINProcess = fileInProcess;
            projectType = Project_Type;
            htLayerData = LayerData;
            allLayerData = AllLayersData;
            //buildingFloorMappingData = BuildingFloorMappingData;

            IsDxfLayerHasError = bDxfHasErrorFound;
            lstErrorMessages = lstErrorMessageWhileDxfExtract;
            dictLayerDefaultColour = dictLayerWiseDefaultColour;
            //BuildingFloorNameMap = lstBuildingNameMap;

            //if (BuildingFloorNameMap == null) //Ground line missing handle it
            //    BuildingFloorNameMap = new List<BuildingNameWithProposeAndFloorMap>();
        }
         
        public List<DrawingValidateItem> ValidDrawing()
        {
            //initialize();

            List<DrawingValidateItem> lstResult = new List<DrawingValidateItem>();
            List<DrawingValidateItem> lstObjectWithInObject = new List<DrawingValidateItem>();

            DrawingValidateItem objDataExtractionValidation = new DrawingValidateItem();
            DrawingValidateItem objDxfExtractResult = new DrawingValidateItem();

            //validate common reference point
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : ValidateCommonReferencePoint");
            
            DrawingValidateItem objBasementHasNotAllowedAnyRoomOrAccessoryValidation = new DrawingValidateItem();
            objBasementHasNotAllowedAnyRoomOrAccessoryValidation.Name = RuleName.BasementWithRoomAndAccessory;
            objBasementHasNotAllowedAnyRoomOrAccessoryValidation.RuleType = RuleType.Element;
            objBasementHasNotAllowedAnyRoomOrAccessoryValidation.RuleOn = "Basement floor with room and Accessory";

            objDxfExtractResult.Name = RuleName.DXFExtractionError;
            objDxfExtractResult.RuleOn = "DXF file";
            objDxfExtractResult.RuleType = RuleType.Element;

            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : SlabBeamDataValidation");
            //SlabBeamDataValidation(ref lstResult);

            if (IsDxfLayerHasError)
            {
                objDxfExtractResult.IsValid = false;
                foreach (string sMsg in lstErrorMessages)
                {
                    objDxfExtractResult.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = sMsg
                    });
                }
            }
            else
                objDxfExtractResult.IsValid = true;

            lstResult.Add(objDxfExtractResult);

            objDataExtractionValidation.RuleOn = "DXF file data";
            objDataExtractionValidation.RuleType = RuleType.Element;
            objDataExtractionValidation.Name = RuleName.DataExtractionFailed;

            if (allLayerData == null || allLayerData.Count == 0)
            {
                objDataExtractionValidation.IsValid = false;
                objDataExtractionValidation.ErrorElements.Add(new ItemErrorDetails
                {
                    ErrorMessage = ContactMessage
                });
                lstResult.Add(objDataExtractionValidation);

                return lstResult;
            }

            DrawingValidateItem objLayerValidation = new DrawingValidateItem();
            objLayerValidation.Name = RuleName.LayerExistsValidation;
            objLayerValidation.RuleOn = "DxfDrawingLayer";
            objLayerValidation.RuleType = RuleType.Element;

            //DxfLayersName.ProposedWork
            string[] arrLayerName = new string[] { DxfLayersName.FloorPlan, DxfLayersName.Plot, DxfLayersName.Unit, DxfLayersName.RearElevationplan, DxfLayersName.GradeLine, DxfLayersName.Section, DxfLayersName.Side1Elevationplan, DxfLayersName.Side2Elevationplan, DxfLayersName.FrontElevationplan, DxfLayersName.RearElevationplan, DxfLayersName.SitePlan, DxfLayersName.RoofLine, DxfLayersName.MainRoad, DxfLayersName.PrintArea, DxfLayersName.CommonReferencePoint, DxfLayersName.RearMarginLine }; 

            List<string> lstLayerNames = allLayerData.Where(y => !string.IsNullOrWhiteSpace(y.LayerName)).Select(x => x.LayerName.ToLower().Trim()).Distinct().ToList();

            //All layer compulsory exists in drawing
            bool bFound = false;
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Compulsory layer");
            foreach (string layerName in arrLayerName)
            {
                if (allLayerData.Any(x => x.LayerName.ToLower() == layerName.ToLower()) == false)
                {
                    objLayerValidation.IsValid = false;
                    objLayerValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = $"{layerName} layer is missing or not used in drawing"
                    });
                }
            }
             
            return lstResult;
        }

        public void ValidateBulgeValue(ref List<DrawingValidateItem> lstResult)
        {
            DrawingValidateItem objDrawingValidation = new DrawingValidateItem();
            objDrawingValidation.Name = RuleName.PrintAreaValidation;
            objDrawingValidation.RuleOn = "Bulge area";
            objDrawingValidation.RuleType = RuleType.Element;

            List<LayerDataWithText> lstBulgeElements = allLayerData.Where(x => x.HasBulge == true).ToList();
            foreach(LayerDataWithText item in lstBulgeElements)
            {
                foreach(BulgeItem item1 in item.CoordinateWithBulge)
                {
                    if(item1.IsBulgeValue && item1.ItemValue.Bulge > 500)
                    {
                        objDrawingValidation.IsValid = false;
                        objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = "Large bulge not allowed, please make it small or break in part.",
                            ElementPositionCoordinate = item.Coordinates
                        });
                    }
                }
            } 
             
            lstResult.Add(objDrawingValidation);
        }
      
        public DrawingValidateItem ValidatePrintArea()
        {
            DrawingValidateItem objDrawingValidation = new DrawingValidateItem();
            objDrawingValidation.Name = RuleName.PrintAreaValidation;
            objDrawingValidation.RuleOn = "Print area";
            objDrawingValidation.RuleType = RuleType.Element;

            List<LayerDataWithText> lstPrintArea = allLayerData.Where(x => x.LayerName.ToLower() == DxfLayersName.PrintArea).ToList();
            List<LayerDataWithText> lstSitePlan = allLayerData.Where(x => x.LayerName.ToLower() == DxfLayersName.SitePlan).ToList();
            List<LayerDataWithText> lstFloorPlan = allLayerData.Where(x => x.LayerName.ToLower() == DxfLayersName.FloorPlan).ToList(); // 04Mar2025 added floor

            foreach (LayerDataWithText objPrintArea in lstPrintArea)
            {
                if (!string.IsNullOrWhiteSpace(objPrintArea.ColourCode) && objPrintArea.ColourCode.Trim() == DxfLayersName.PrintAreaColourCode) //DxfLayersName.PrintAreaSignatureColour
                    continue;
                
                if (objPrintArea.Coordinates == null || objPrintArea.Coordinates.Count < 3)
                {
                    objDrawingValidation.IsValid = false;
                    objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ElementPositionCoordinate = objPrintArea.Coordinates,
                        ErrorMessage = "_PrintArea is must be a closed polygon"
                    });
                }
                else if (objPrintArea.Coordinates != null && objPrintArea.Coordinates.Count > 2 && !objPrintArea.Coordinates.First().Equals(objPrintArea.Coordinates.Last()))
                {
                    objDrawingValidation.IsValid = false;
                    objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ElementPositionCoordinate = objPrintArea.Coordinates,
                        ErrorMessage = "_PrintArea is must be a closed polygon"
                    });
                }

                string sText = ExtractLayerText(objPrintArea, false, "");
                if (string.IsNullOrWhiteSpace(sText))
                {
                    objDrawingValidation.IsValid = false;
                    objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ElementPositionCoordinate = objPrintArea.Coordinates,
                        ErrorMessage = "_PrintArea associated text is missing or text colour is not same as element colour"
                    });
                }
                else
                {
                    Match mtScale = regexPrintScale.Match(sText.Trim());
                    if (!mtScale.Success || mtScale.Groups.Count != 2)
                    {
                        objDrawingValidation.IsValid = false;
                        objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ElementPositionCoordinate = objPrintArea.Coordinates,
                            ErrorMessage = $"_PrintArea associated text has invalid scale {sText}, it should be like 1:X"
                        });
                    }
                    else
                    {
                        try
                        {
                            int iScale = int.Parse(mtScale.Groups[1].Value);

                            if (iScale > ValidScaleValueRange)
                            {
                                objDrawingValidation.IsValid = false;
                                objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                                {
                                    ElementPositionCoordinate = objPrintArea.Coordinates,
                                    ErrorMessage = $"_PrintArea associated text has scale value greater than {ValidScaleValueRange}"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Exception in print area details " + ex.Message);
                        }
                    }
                }
            }

            List<LayerDataWithText> lstPrintAreaBlock = lstPrintArea.ToList(); // lstPrintArea.Where(x => !string.IsNullOrWhiteSpace(x.ColourCode) && x.ColourCode.Trim() != DxfLayersName.PrintAreaSignatureColour).ToList();
            
            //comment on 24Jan2024
            //List<LayerDataWithText> lstPrintAreaSignature = lstPrintArea.Where(x => !string.IsNullOrWhiteSpace(x.ColourCode) && x.ColourCode.Trim() == DxfLayersName.PrintAreaSignatureColour).ToList();

            //if (lstPrintAreaSignature == null || lstPrintAreaSignature.Count == 0)
            //{
            //    objDrawingValidation.IsValid = false;
            //    objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
            //    {
            //        ErrorMessage = $"_PrintArea: Signature block missing"
            //    });
            //}

            if (lstPrintAreaBlock == null || lstPrintAreaBlock.Count == 0)
            {
                objDrawingValidation.IsValid = false;
                objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                {
                    ErrorMessage = $"_PrintArea missing"
                });
            } 

            if (lstPrintAreaBlock.Count(x => x.HasBulge == true) > 0)
            {
                for (int iCnt = 0; iCnt < lstPrintAreaBlock.Count() - 1; iCnt++)
                {
                    if (lstPrintAreaBlock[iCnt].HasBulge)
                    {
                        LayerDataWithText printarea1 = lstPrintAreaBlock[iCnt].DeepClone();
                        SetAdjustCoordinate(ref printarea1);
                        lstPrintAreaBlock[iCnt] = printarea1;
                    }
                }
            }

            //overlap checking
            for (int iCnt = 0; iCnt < lstPrintAreaBlock.Count() - 1; iCnt++)
            {  
                for (int jCnt = iCnt + 1; jCnt < lstPrintAreaBlock.Count(); jCnt++)
                { 
                    double dIntersectArea = 0, dIntersectRoomArea = 0;
                    if (!IsInPolyUsingAngle(lstPrintAreaBlock[iCnt].Coordinates, lstPrintAreaBlock[jCnt].Coordinates) && 
                        objNetTopologySuite.IsIntersects(lstPrintAreaBlock[iCnt].Coordinates, lstPrintAreaBlock[jCnt].Coordinates, ref dIntersectArea, ref dIntersectRoomArea))
                    {
                        if (dIntersectArea > ErrorAllowScale)
                        {
                            objDrawingValidation.IsValid = false;
                            objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = "Print area crossover with print area",
                                ElementPositionCoordinate = lstPrintAreaBlock[jCnt].Coordinates
                            });
                        }
                    }
                    else if (IsInPolyUsingAngle(lstPrintAreaBlock[iCnt].Coordinates, lstPrintAreaBlock[jCnt].Coordinates) && lstPrintAreaBlock[jCnt].ColourCode != DxfLayersName.PrintAreaColourCode)
                    {
                        objDrawingValidation.IsValid = false;
                        objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = "Print area found within other print area",
                            ElementPositionCoordinate = lstPrintAreaBlock[jCnt].Coordinates
                        });
                    }
                }
            }

            // 04Mar2025 Now check each floor and site plan text within print area
            if (objDrawingValidation.IsValid)
            {
                for (int iCnt = 0; iCnt < lstPrintAreaBlock.Count(); iCnt++)
                {
                    LayerDataWithText printarea1 = lstPrintAreaBlock[iCnt].DeepClone();
                    if (printarea1.HasBulge)
                        SetAdjustCoordinate(ref printarea1);

                    for (int j = 0; j < lstSitePlan.Count(); j++)
                    {
                        if (lstSitePlan[j] == null)
                            continue;

                        if (
                                lstSitePlan[j].TextInfoData != null && lstSitePlan[j].TextInfoData.Count() > 0 &&
                                (lstSitePlan[j].TextInfoData.First().Coordinates != null && IsInPolyUsingAngle(printarea1.Coordinates, lstSitePlan[j].TextInfoData.First().Coordinates)) ||
                                (lstSitePlan[j].TextInfoData.First().TextAlignCoordinates != null && IsInPolyUsingAngle(printarea1.Coordinates, lstSitePlan[j].TextInfoData.First().TextAlignCoordinates))
                            )
                        {
                            lstSitePlan[j] = null;
                        }
                    }

                    for (int j = 0; j < lstFloorPlan.Count(); j++)
                    {
                        if (lstFloorPlan[j] == null)
                            continue;

                        if (
                                lstFloorPlan[j].TextInfoData != null && lstFloorPlan[j].TextInfoData.Count() > 0 &&
                                (lstFloorPlan[j].TextInfoData.First().Coordinates != null && IsInPolyUsingAngle(printarea1.Coordinates, lstFloorPlan[j].TextInfoData.First().Coordinates)) ||
                                (lstFloorPlan[j].TextInfoData.First().TextAlignCoordinates != null && IsInPolyUsingAngle(printarea1.Coordinates, lstFloorPlan[j].TextInfoData.First().TextAlignCoordinates))
                            )
                        {
                            lstFloorPlan[j] = null;
                        }
                    }
                    lstFloorPlan = lstFloorPlan.Where(x => x != null).ToList();
                    lstSitePlan = lstSitePlan.Where(x => x != null).ToList();
                }

                if (lstSitePlan != null && lstSitePlan.Count() > 0)
                {
                    for (int j = 0; j < lstSitePlan.Count(); j++)
                    {
                        objDrawingValidation.IsValid = false;
                        objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = "Site plan is outside print area",
                            ElementPositionCoordinate = lstSitePlan[j].Coordinates
                        });
                    }
                }

                if (lstFloorPlan != null && lstFloorPlan.Count() > 0)
                {
                    for (int j = 0; j < lstFloorPlan.Count(); j++)
                    {
                        objDrawingValidation.IsValid = false;
                        objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = $"Floor plan - {ExtractLayerText(lstFloorPlan[j], false, "")} is outside print area",
                            ElementPositionCoordinate = lstFloorPlan[j].Coordinates
                        });
                    }
                }
            }

            return objDrawingValidation;

        }
        public void ValidateSitePlanOverlap(ref List<DrawingValidateItem> lstResult)
        {
            DrawingValidateItem objSiteplanValidation = new DrawingValidateItem();

            objSiteplanValidation.Name = RuleName.SiteplanAndPlotValidation;
            objSiteplanValidation.RuleOn = "Siteplan with main road, road widening , netplot";
            objSiteplanValidation.RuleType = RuleType.Element;

            List<LayerDataWithText> lstPlot = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.Plot).ToList();
            List<LayerDataWithText> lstMainRoad = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.MainRoad).ToList();

            for (int i = 0; i < lstMainRoad.Count(); i++)
            {
                    LayerDataWithText mainroad = lstMainRoad[i].DeepClone();
                    SetAdjustCoordinate(ref mainroad);
                    lstMainRoad[i] = mainroad;
            }

            for (int i = 0; i < lstMainRoad.Count(); i++)
            {
                string sMainRoadText = ExtractLayerText(lstMainRoad[i], false, "");
                if (string.IsNullOrWhiteSpace(sMainRoadText))
                {
                    objSiteplanValidation.IsValid = false;
                    objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = "Main road associated text is missing",
                        ElementPositionCoordinate = lstMainRoad[i].Coordinates
                    });
                }

                if (lstMainRoad[i].Coordinates.Count == 2)
                {
                    objSiteplanValidation.IsValid = false;
                    objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = "Invalid main road it must be close polygon",
                        ElementPositionCoordinate = lstMainRoad[i].Coordinates
                    });
                }
                
                if (CheckLineTypeIsCenterLine(lstMainRoad[i].LineType))
                {
                    objSiteplanValidation.IsValid = false;
                    objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = "Invalid main road line type found it should not be center",
                        ElementPositionCoordinate = lstMainRoad[i].Coordinates
                    });
                }
            }

            foreach (LayerDataWithText itemPlot in lstPlot)
            {
                LayerDataWithText plot = itemPlot.DeepClone();

                foreach (LayerDataWithText itemMainRoad in lstMainRoad)
                {
                    LayerDataWithText mainroad = itemMainRoad.DeepClone();

                    double dIntersectArea = 0, dIntersectRoomArea = 0;
                    if (!IsInPolyUsingAngle(plot.Coordinates, mainroad.Coordinates) && objNetTopologySuite.IsIntersects(plot.Coordinates, mainroad.Coordinates, ref dIntersectArea, ref dIntersectRoomArea))
                    {
                        if (dIntersectArea > ErrorAllowScale) //ErrorAllowScaleForParallel revert on 27Jun2025 as compare with SMC code
                        {
                            objSiteplanValidation.IsValid = false;
                            objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = "Plot crossover with main road",
                                ElementPositionCoordinate = mainroad.Coordinates
                            });
                        }
                    }
                }
            }
 
            lstResult.Add(objSiteplanValidation);

        }
        public DrawingValidateItem LayerDataNotHaveAnyChildValidation(string itemName, string layerName, List<LayerDataWithText> allLayerData, List<string> lstIgnoreLayer)
        {
            return LayerDataNotHaveAnyChildValidation(itemName, layerName, allLayerData, lstIgnoreLayer, ErrorAllowScale);
        }
        public DrawingValidateItem LayerDataNotHaveAnyChildValidation(string itemName, string layerName, List<LayerDataWithText> allLayerData, List<string> lstIgnoreLayer, double errorAllow)
        {
            DrawingValidateItem objDrawingValidation = new DrawingValidateItem();
            objDrawingValidation.Name = itemName;

            List<LayerDataWithText> lstBuilding = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.Unit).ToList();
            List<LayerInfo> lstBuildingLayer = objLayerExtractor.SetLayerInfo(lstBuilding, DxfLayersName.Unit);

            objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.FloorPlan, DxfLayersName.FloorPlan , ref lstBuildingLayer);
            objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.Section, DxfLayersName.Section, ref lstBuildingLayer);

            //first check layer is exists or not if not then not to process it //16Apr2024
            if (allLayerData.Select(x => x.LayerName).Distinct().ToList().Exists(x => x.ToLower() == layerName.ToLower()) == false)
                return objDrawingValidation;

            Dictionary<string, string> dict = GetDictionaryForAllLayer();
            if (lstIgnoreLayer != null)
            {
                foreach (String s in lstIgnoreLayer)
                {
                    if (dict.ContainsKey(s))
                        dict.Remove(s);
                }
            }

            foreach (LayerInfo itemBuilding in lstBuildingLayer)
            {
                foreach (string sBuiltupKey in itemBuilding.Child.Keys)
                {
                    List<LayerInfo> lstSectionOrFloor = itemBuilding.Child[sBuiltupKey].DeepClone();

                    List<LayerInfo> lstSectionOrFloorChecking = itemBuilding.Child[sBuiltupKey]; // lstSectionOrFloor.DeepClone();
                    
                    objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, layerName, layerName, ref lstSectionOrFloorChecking);
                    bool iFound = false;
                    foreach (LayerInfo itemFloor in itemBuilding.Child[sBuiltupKey])
                    {
                        if (!itemFloor.Child.ContainsKey(layerName))
                            continue;

                        iFound = true;
                    }

                    if (iFound == false)
                        continue; 

                    //now if found so now check it
                    objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, dict, ref lstSectionOrFloor);

                    foreach (LayerInfo itemFloor in lstSectionOrFloor)
                    {
                        if (!itemFloor.Child.ContainsKey(layerName))
                            continue;

                        bool IsTerraceFloor = false;
                        string sTextFloorSectionName = ExtractLayerText(itemFloor, false, "");
                        if (!string.IsNullOrWhiteSpace(sTextFloorSectionName) && regexTerraceFloor.IsMatch(sTextFloorSectionName))
                            IsTerraceFloor = true;

                        List<LayerInfo> layerData = itemFloor.Child[layerName];
                        if (layerData != null && layerData.Count > 0)
                            layerData = layerData.Where(x => x.Data.Coordinates != null && x.Data.Coordinates.Count > 3 && !CheckLineTypeIsCenterLine(x.Data.LineType)).ToList();

                        int iTotal = layerData.Count;

                        for (int i = 0; i < iTotal; i++)
                        {
                            for (int j = 0; j < iTotal; j++)
                            {
                                if (i == j)
                                    continue;

                                if (IsInPolyUsingAngle(layerData[i].Data.Coordinates, layerData[j].Data.Coordinates, errorAllow))
                                {
                                    objDrawingValidation.IsValid = false;
                                    objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                                    {
                                        ElementPositionCoordinate = layerData[j].Data.Coordinates,
                                        ErrorMessage = ExtractLayerText(layerData[j], true, "") + " found within " + layerData[i].Data.LayerName + " " + ExtractLayerText(layerData[i], true, "")
                                    });
                                }

                            }
                        }

                        List<LayerInfo> allDataWithoutLayerData = new List<LayerInfo>();
                        foreach (string skey in itemFloor.Child.Keys)
                        {
                            if (itemFloor.Child.ContainsKey(layerName))
                                continue;

                            List<LayerInfo> lstTempData = itemFloor.Child[skey];
                            if (lstTempData != null && lstTempData.Count > 0)
                                allDataWithoutLayerData.AddRange(lstTempData.Where(x => x.Data.Coordinates != null && x.Data.Coordinates.Count > 3 && !CheckLineTypeIsCenterLine(x.Data.LineType)).ToList());
                        }

                        for (int i = 0; i < iTotal; i++)
                        {
                            string sText = ExtractLayerText(layerData[i], true, "");
                            if (string.IsNullOrWhiteSpace(sText))
                                sText = layerName;

                            List<string> lstLayerNameToCompare = allDataWithoutLayerData.Select(x => x.Data.LayerName.ToLower()).Distinct().ToList();
                            List<LayerInfo> lstCompareList = allDataWithoutLayerData;//.DeepClone();

                        }

                    }
                }
            }

            return objDrawingValidation;
        }
   
        private void AddError(DrawingValidateItem objResult, List<Cordinates> coordinates, string errorMessage)
        {
            objResult.IsValid = false;
            objResult.ErrorElements.Add(new ItemErrorDetails
            {
                ElementPositionCoordinate = coordinates,
                ErrorMessage = errorMessage
            });
        } 
         
        //Parking must be within the floor. It should contain relevant text (CP(C),CP(R), TW(R),TW(C)...) within it. One polygon should contain one text.
        public DrawingValidateItem ValidateChildPolygonWithMasterPolygon(string Name, string parentName, string childName, List<LayerInfo> Master, List<LayerInfo> Child)
        {
            DrawingValidateItem objResult = new DrawingValidateItem();
            objResult.Name = Name;
            objResult.RuleOn = parentName + " and " + childName;
            objResult.RuleType = RuleType.Element;

            foreach (LayerInfo childPolygon in Child)
            {
                bool bFound = false;
                foreach (LayerInfo parentPolygon in Master)
                {
                    if (IsInPolyUsingAngle(parentPolygon.Data.Coordinates, childPolygon.Data.Coordinates))
                    {
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    objResult.IsValid = false;
                    objResult.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = $"{childName} not within {parentName}",
                        ElementPositionCoordinate = childPolygon.Data.Coordinates,
                    });

                    //string logLine = CordinatesToString(childPolygon.Data.Coordinates);
                    //if (objResult.sbInvalidReport.ToString().IndexOf(logLine) == -1)
                    //{
                    //    objResult.sbInvalidReport.AppendLine(logLine);                        
                    //    objResult.sbInvalidReport.AppendLine($"{childName} not in {parentName} {General.InvalidMark}");
                    //}
                }
            }
            //if (objResult.IsValid)
            //{
            //    objResult.sbValidReport.AppendLine($"{childName} with {parentName} {General.ValidMark}");
            //}
            return objResult;
        }
        public void ValidateAllChildWithinParentData(string ReferenceText, string ParentItemName, string ChildItemName, List<LayerInfo> Parents, List<LayerInfo> lstChildsItem, ref DrawingValidateItem objResult)
        {
            if (Parents == null || Parents.Count == 0 || lstChildsItem == null || lstChildsItem.Count == 0)
                return;

            List<LayerInfo> lstChilds = lstChildsItem.DeepClone();

            int iTotalParent = Parents.Count;
            int iTotalChilds = lstChildsItem.Count;

            for (int iParentCnt = 0; iParentCnt < iTotalParent; iParentCnt++)
            {
                LayerInfo objParent = Parents[iParentCnt];

                //issue to ignore Unit BUA door validation (Floor 9-Shop number 4) Error shown seems incorrect - drawing error
                if (objParent.Data.Coordinates.Count < 3 || CheckLineTypeIsCenterLine(objParent.Data.LineType))  ///== DxfLayersName.CenterLine
                    continue;

                //bool bIsFound = false;
                for (int iChildCnt = 0; iChildCnt < iTotalChilds; iChildCnt++)
                {
                    if (lstChilds[iChildCnt] == null)
                        continue;

                    LayerInfo objChild = lstChilds[iChildCnt];

                    if (CheckBothPolygonHasSameCoordinates(objParent.Data.Coordinates, objChild.Data.Coordinates) || IsInPolyUsingAngle(objParent.Data.Coordinates, objChild.Data.Coordinates))
                    {
                        lstChilds[iChildCnt] = null;
                        //bIsFound = true;
                        break;
                    }
                }

            }

            if (lstChilds != null)
                lstChilds = lstChilds.Where(x => x != null).ToList();

            if (lstChilds != null && lstChilds.Count > 0)
            {
                foreach (LayerInfo objChild in lstChilds)
                {
                    objResult.IsValid = false;
                    objResult.ErrorElements.Add(new ItemErrorDetails
                    {
                        ElementPositionCoordinate = objChild.Data.Coordinates,
                        ErrorMessage = $"{ChildItemName} not within {ParentItemName} - Ref. {ReferenceText}"
                    });
                }
            }

        }
        public void ValidateAllChildWithinParent(string ReferenceText, string ParentItemName, string ChildItemName, List<LayerInfo> Parents, List<LayerInfo> lstChilds, ref DrawingValidateItem objResult)
        {
            if (Parents == null || Parents.Count == 0 || lstChilds == null || lstChilds.Count == 0)
                return;

            foreach (LayerInfo objParent in Parents)
            {
                //issue to ignore Unit BUA door validation (Floor 9-Shop number 4) Error shown seems incorrect - drawing error
                if (objParent.Data.Coordinates.Count < 3 || CheckLineTypeIsCenterLine(objParent.Data.LineType))  ///== DxfLayersName.CenterLine
                    continue;

                bool bIsFound = false;
                foreach (LayerInfo objChild in lstChilds)
                {
                    if (CheckBothPolygonHasSameCoordinates(objParent.Data.Coordinates, objChild.Data.Coordinates) || IsInPolyUsingAngle(objParent.Data.Coordinates, objChild.Data.Coordinates))
                    {
                        bIsFound = true;
                        break;
                    }
                }

                if (!bIsFound)
                {
                    objResult.IsValid = false;
                    objResult.ErrorElements.Add(new ItemErrorDetails
                    {
                        ElementPositionCoordinate = objParent.Data.Coordinates,
                        ErrorMessage = $"{ChildItemName} not within {ParentItemName} - Ref. {ReferenceText}"
                    });

                    //string sLine = $"{ChildItemName} not with in {ParentItemName} {General.InvalidMark}";
                    //if (objResult.sbInvalidReport.ToString().IndexOf(sLine) == -1)
                    {
                        //objResult.sbInvalidReport.AppendLine(CordinatesToString(objParent.Data.Coordinates));
                        //objResult.sbInvalidReport.AppendLine(ReferenceText + " " + sLine);
                    }
                }
            }

            //string sLine1 = $"{ParentItemName} is valid with {ChildItemName} {General.ValidMark}";
            //if (objResult.IsValid && objResult.sbValidReport.ToString().IndexOf(sLine1) == -1)
            //    objResult.sbValidReport.AppendLine(sLine1);
        }
        public void ValidateParentWithAllChild(string ReferenceText, string ItemName, string ParentItemName, string ChildItemName, List<LayerInfo> Parents, List<LayerInfo> lstChilds, ref DrawingValidateItem objResult)
        {
            foreach (LayerInfo objChild in lstChilds)
            {
                bool bIsFound = false;
                foreach (LayerInfo objParent in Parents)
                {
                    if (objParent.Data.Coordinates.Count <= 3 || CheckLineTypeIsCenterLine(objParent.Data.LineType))  ///== DxfLayersName.CenterLine
                        continue;

                    if (CheckBothPolygonHasSameCoordinates(objParent.Data.Coordinates, objChild.Data.Coordinates) || IsInPolyUsingAngle(objParent.Data.Coordinates, objChild.Data.Coordinates))
                    {
                        bIsFound = true;
                        break;
                    }
                }

                if (!bIsFound)
                {
                    objResult.IsValid = false;
                    objResult.ErrorElements.Add(new ItemErrorDetails
                    {
                        ElementPositionCoordinate = objChild.Data.Coordinates,
                        ErrorMessage = $"{ChildItemName} not within {ParentItemName}"
                    });

                    //string sLine1 = $"{ChildItemName} not with in {ParentItemName} {General.InvalidMark}";
                    //if (objResult.sbInvalidReport.ToString().IndexOf(sLine1) == -1)
                    //{
                    //    objResult.sbInvalidReport.AppendLine(CordinatesToString(objChild.Data.Coordinates));
                    //    objResult.sbInvalidReport.AppendLine(ReferenceText + " " + sLine1);
                    //}
                }
            }

            //string sLine = $"{ParentItemName} is valid with {ChildItemName} {General.ValidMark}";
            //if (objResult.IsValid && objResult.sbValidReport.ToString().IndexOf(sLine) == -1)
            //    objResult.sbValidReport.AppendLine(sLine);

        }

        public DrawingValidateItem ValidateInternalRoadHasNoOtherPolygon(LayerDataWithText objSitePlan, List<LayerDataWithText> lstInternalRoads)
        {
            //Geometry objSitePolygon = objNetTopologyUtility.ConvertPolygonFromCoordinate(objSitePlan.Coordinates);
            DrawingValidateItem objResult = new DrawingValidateItem();
            objResult.Name = "InternalRoadWithInPlot";
            foreach (LayerDataWithText internalRoad in lstInternalRoads)
            {
                //Geometry objInternalRoad = objNetTopologyUtility.ConvertPolygonFromCoordinate(internalRoad.Coordinates);
                if (!IsInPolyUsingAngle(objSitePlan.Coordinates, internalRoad.Coordinates))   //if (!IsInPolyUsingAngle(objSitePlan.Coordinates, internalRoad.Coordinates))
                {
                    objResult.IsValid = false;
                    objResult.ErrorElements.Add(new ItemErrorDetails
                    {
                        ElementPositionCoordinate = internalRoad.Coordinates,
                        ErrorMessage = $"Internal road not within site plan"

                    });
                    //objResult.sbInvalidReport.AppendLine(CordinatesToString(internalRoad.Coordinates));
                    //objResult.sbInvalidReport.AppendLine($"Internal road not with in site plan {General.InvalidMark}");
                }
            }

            //if(objResult.IsValid)
            //{
            //    objResult.sbValidReport.AppendLine($"Internal road within site plan {General.ValidMark}");
            //}
            return objResult;
        }

        //Road boundries (i.e. any of the coordinates of road should not be in plot/netPlot Polygon.
        //Atleast two coordinates must be on line of the plot/netPlot polygon.
        //Roadwidening should be within plot.
        public DrawingValidateItem ValidatePolygonTouchedWithOtherPolygon(string Name, string parentName, string childName, List<LayerDataWithText> MasterPolygon1, List<LayerDataWithText> MasterPolygon2, List<LayerDataWithText> childPolygon)
        {
            DrawingValidateItem objResult = new DrawingValidateItem();
            objResult.Name = Name;
            foreach (LayerDataWithText child in childPolygon)
            {
                bool bInside = false;
                if (MasterPolygon1 != null && MasterPolygon1.Count > 0)
                {
                    foreach (LayerDataWithText master in MasterPolygon1)
                    {
                        if (FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLines(master, child).Value < General.ErrorAllowScale)
                        {
                            bInside = true;
                            break;
                        } // net plot
                    }
                }

                if (MasterPolygon2 != null && MasterPolygon2.Count > 0)
                {
                    foreach (LayerDataWithText master in MasterPolygon2)
                    {
                        if (FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLines(child, master).Value < General.ErrorAllowScale)
                        {
                            bInside = true;
                            break;
                        }
                    } // plots
                }

                if (!bInside)
                {
                    objResult.IsValid = false;
                    objResult.ErrorElements.Add(new ItemErrorDetails
                    {
                        ElementPositionCoordinate = child.Coordinates,
                        ErrorMessage = $"{childName} should touch {parentName}"
                    });
                    //objResult.sbInvalidReport.AppendLine(CordinatesToString(child.Coordinates));
                    //objResult.sbInvalidReport.AppendLine($"{childName} not touched with {parentName} {General.InvalidMark}");
                }
            }

            //if(objResult.IsValid)
            //{
            //    objResult.sbValidReport.AppendLine($"{childName} touched with {parentName} {General.ValidMark}");
            //}

            return objResult;
        }
        
    
    }
}

