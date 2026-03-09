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
    public sealed class BuildingPermissionDrawingValidation : MathLib
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
        public BuildingPermissionDrawingValidation(string fileInProcess, string Project_Type, Dictionary<string, List<LayerDataWithText>> LayerData, Dictionary<string, string> dictLayerWiseDefaultColour, List<LayerDataWithText> AllLayersData, bool bDxfHasErrorFound, List<string> lstErrorMessageWhileDxfExtract)
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

            List<DrawingValidateItem> lstLiftText = new List<DrawingValidateItem>();
            List<DrawingValidateItem> lstBuaText = new List<DrawingValidateItem>();

            DrawingValidateItem objLiftDoorValidation = new DrawingValidateItem();
            //DrawingValidateItem objBuaDoorValidation = new DrawingValidateItem();
            //DrawingValidateItem objRoomUnitBuaValidation = new DrawingValidateItem();
            //DrawingValidateItem objUnitBuaRoomValidation = new DrawingValidateItem();
            DrawingValidateItem objAllBuiltupWithUnitBuaValidation = new DrawingValidateItem();
              
             //DrawingValidateItem objResidentBuiltupUnitBuaValidation = new DrawingValidateItem();
             //DrawingValidateItem objCommercialBuiltupUnitBuaValidation = new DrawingValidateItem();
             //DrawingValidateItem objIndustrialBuiltupUnitBuaValidation = new DrawingValidateItem();
             ////DrawingValidateItem objSpecialBuiltupUnitBuaValidation = new DrawingValidateItem();
             //DrawingValidateItem objSpecialUseBuiltupUnitBuaValidation = new DrawingValidateItem();
            DrawingValidateItem objPassageWithBuiltupValidation = new DrawingValidateItem();
            DrawingValidateItem objPassageTextValidation = new DrawingValidateItem();
            DrawingValidateItem objOTSTextValidation = new DrawingValidateItem();
            //DrawingValidateItem objResult = new DrawingValidateItem();
            DrawingValidateItem objWallResult = new DrawingValidateItem();
            DrawingValidateItem objFloorParkingValidation = new DrawingValidateItem();
            DrawingValidateItem objDriveWayTextValidation = new DrawingValidateItem();
            DrawingValidateItem objDxfExtractResult = new DrawingValidateItem();
            //DrawingValidateItem objRampTextValidation = new DrawingValidateItem();
            DrawingValidateItem objDataExtractionValidation = new DrawingValidateItem();
            DrawingValidateItem objAccessoryTextValidation = new DrawingValidateItem();

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
            string[] arrLayerName = new string[] { DxfLayersName.MarginLine, DxfLayersName.Building, DxfLayersName.Floor, DxfLayersName.Propwork, DxfLayersName.Plot, DxfLayersName.NetPlot, DxfLayersName.GroundLevel, DxfLayersName.HighFloodLevel, DxfLayersName.Section, DxfLayersName.SectionalItem, DxfLayersName.FloorInSection, DxfLayersName.SitePlan, DxfLayersName.OtherDetail, DxfLayersName.Room, DxfLayersName.MainRoad, DxfLayersName.PrintArea, DxfLayersName.CommonReferencePoint, DxfLayersName.North }; //,, DxfLayersName.UnitBUA,

             //string[] arrAnyLayerName = new string[] { DxfLayersName.CommercialBuiltUpLine, DxfLayersName.ResidentBuiltUpLine, DxfLayersName.IndustrialBuiltUpLine, DxfLayersName.SpecialUseBuiltUpLine };   
            List<string> lstLayerNames = allLayerData.Where(y => !string.IsNullOrWhiteSpace(y.LayerName)).Select(x => x.LayerName.ToLower().Trim()).Distinct().ToList();

            //All layer compulsory exists in drawing
            bool bFound = false;
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Compulsory layer");
            foreach (string layerName in arrLayerName)
            {
                //Added below condition on 25Nov2023
                if (bFound == false && (layerName.Trim().ToLower().Equals(DxfLayersName.GroundLevel.Trim().ToLower()) || layerName.Trim().ToLower().Equals(DxfLayersName.HighFloodLevel.Trim().ToLower())))
                    bFound = true;
                else if (!lstLayerNames.Contains(layerName))
                {
                    //if (IsMatch(layerName, DxfLayersName.GroundLevel) && lstLayerNames.Contains(DxfLayersName.HighFloodLevel))
                    if (layerName.Trim().ToLower().Equals(DxfLayersName.GroundLevel.Trim().ToLower()) && lstLayerNames.Contains(DxfLayersName.HighFloodLevel))
                    {
                        bFound = true;
                        List<LayerDataWithText> lstData = allLayerData.Where(x => x.LayerName.ToLower() == DxfLayersName.HighFloodLevel.ToLower()).ToList();
                        if (lstData == null || lstData.Count == 0)
                        {
                            objLayerValidation.IsValid = false;
                            objLayerValidation.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = $"{layerName} layer is missing or not used in drawing"
                            });
                        }
                    }
                    else if (layerName.Trim().ToLower().Equals(DxfLayersName.HighFloodLevel.Trim().ToLower()) && lstLayerNames.Contains(DxfLayersName.GroundLevel))  //(IsMatch(layerName, DxfLayersName.HighFloodLevel) && lstLayerNames.Contains(DxfLayersName.GroundLevel))
                    {
                        bFound = true;
                        List<LayerDataWithText> lstData = allLayerData.Where(x => x.LayerName.ToLower() == DxfLayersName.GroundLevel.ToLower()).ToList();
                        if (lstData == null || lstData.Count == 0)
                        {
                            objLayerValidation.IsValid = false;
                            objLayerValidation.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = $"{layerName} layer is missing or not used in drawing"
                            });
                        }
                    }
                    else
                    {
                        objLayerValidation.IsValid = false;
                        objLayerValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = $"{layerName} layer is missing or not used in drawing"
                        });
                    }
                }
                else
                {
                    List<LayerDataWithText> lstData = allLayerData.Where(x => x.LayerName.ToLower() == layerName.ToLower()).ToList();
                    if (lstData == null || lstData.Count == 0)
                    {
                        objLayerValidation.IsValid = false;
                        objLayerValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = $"{layerName} layer is missing or not used in drawing"
                        });
                    }
                }
            }

            if (!bFound)
            {
                objLayerValidation.IsValid = false;
                objLayerValidation.ErrorElements.Add(new ItemErrorDetails
                {
                    ErrorMessage = $"{DxfLayersName.GroundLevel} or {DxfLayersName.HighFloodLevel} layer is missing or not used in drawing"
                });
            }

            // Any layer not allowed exists in drawing
            if (lstLayerNames.Contains(DxfLayersName.IndividualSubPlot))
            {
                objLayerValidation.IsValid = false;
                objLayerValidation.ErrorElements.Add(new ItemErrorDetails
                {
                    ErrorMessage = $"{DxfLayersName.IndividualSubPlot} layer not allowed in {projectType} type permission."
                });
            }


            //Any layer compulsory exists in drawing
            bool bInAnyOneExists = false;
            List<string> allLayerName = new List<string>();
            allLayerName.AddRange(GetAllBuiltupLineList);

            if (projectType == SharedClasses.Constants.ProjectType.BuildingPermissionWithAdditionOrExtension || projectType == SharedClasses.Constants.ProjectType.RevisedWithExistingConstruction) //17Mar2025 revised change
            {
                allLayerName.Add(DxfLayersName.ExBuiltUpLine);
            }

            foreach (string layerName in allLayerName)
            {
                if (lstLayerNames.Contains(layerName))
                {
                    List<LayerDataWithText> lstData = allLayerData.Where(x => x.LayerName.ToLower() == layerName.ToLower()).ToList();
                    if (lstData == null || lstData.Count == 0)
                    {
                        objLayerValidation.IsValid = false;
                        objLayerValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = $"{layerName} layer is missing or not used in drawing"
                        });
                    }
                    else
                    {
                        bInAnyOneExists = true;
                        break;
                    }
                }
            }

            if (true)
            {
                List<LayerDataWithText> lstExStructure = new List<LayerDataWithText>();
                if (htLayerData.ContainsKey(DxfLayersName.Exstructure))
                    lstExStructure = htLayerData[DxfLayersName.Exstructure];

                DrawingValidateItem objExStructureValidation = new DrawingValidateItem();
                objExStructureValidation.Name = RuleName.ExStructureValidation;
                objExStructureValidation.RuleType = RuleType.Text;
                objExStructureValidation.RuleOn = "Layers";
                foreach (LayerDataWithText itemExStructure in lstExStructure)
                {
                    if (string.IsNullOrWhiteSpace(ExtractLayerText(itemExStructure, false, "")))
                    {
                        objExStructureValidation.IsValid = false;
                        objExStructureValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = $"{DxfLayersName.Exstructure} Text is missing or text colour is not same as element colour",
                            ElementPositionCoordinate = itemExStructure.Coordinates
                        });
                    }
                }

                lstResult.Add(objExStructureValidation);
            }

            if (projectType == ProjectType.BuildingPermissionWithAdditionOrExtension || projectType == SharedClasses.Constants.ProjectType.RevisedWithExistingConstruction) //17Mar2025 revised change
            {
                if (htLayerData.Keys.Count(x => x.ToLower() == DxfLayersName.Exstructure) == 0)
                {
                    DrawingValidateItem objExStructureValidation = new DrawingValidateItem();
                    objExStructureValidation.IsValid = false;
                    objExStructureValidation.Name = RuleName.ExStructureValidation;
                    objExStructureValidation.RuleType = RuleType.Text;
                    objExStructureValidation.RuleOn = "Layers";
                    objExStructureValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = $"{DxfLayersName.Exstructure} layer is missing"
                    });
                    lstResult.Add(objExStructureValidation);
                } 

                if (htLayerData.Keys.Count(x => x.ToLower() == DxfLayersName.ExBuiltUpLine) > 0)
                {
                    //check propose work
                    List<LayerDataWithText> lstPropose = new List<LayerDataWithText>();
                    List<LayerDataWithText> lstExStructure = new List<LayerDataWithText>();
                    List<LayerDataWithText> lstBuilding = new List<LayerDataWithText>();

                    if (htLayerData.ContainsKey(DxfLayersName.Propwork))
                        lstPropose = htLayerData[DxfLayersName.Propwork];

                    if (htLayerData.ContainsKey(DxfLayersName.Exstructure))
                        lstExStructure = htLayerData[DxfLayersName.Exstructure];


                    // remove demolise //01Aug2025
                    if(lstExStructure != null && lstExStructure.Count() > 0)
                    {
                        for(int i = lstExStructure.Count - 1; i >= 0; i--)
                        {
                            string sExStructureText = ExtractLayerText(lstExStructure[i], false, "");
                            if (string.IsNullOrWhiteSpace(sExStructureText) || regexDemolishText.IsMatch(sExStructureText))
                            {
                                lstExStructure[i] = null;
                            }
                        }

                        lstExStructure = lstExStructure.Where(x => x != null).ToList();
                    }

                    if (htLayerData.ContainsKey(DxfLayersName.Building))
                        lstBuilding = htLayerData[DxfLayersName.Building];

                    int iTotalPropose = lstPropose.Count;
                    int iTotalExStructure = lstExStructure.Count;
                    int iTotalBuilding = lstBuilding.Count;

                    List<string> lstExStructurePropName = new List<string>();
                    for (int i = 0; i < iTotalPropose; i++)
                    {
                        lstPropose[i].Lines = MakeClosePolyLines(lstPropose[i].Coordinates);
                    }

                    for (int i = 0; i < iTotalExStructure; i++)
                    {
                        lstExStructure[i].Lines = MakeClosePolyLines(lstExStructure[i].Coordinates);
                    }

                    for (int i = 0; i < iTotalBuilding; i++)
                    {
                        lstBuilding[i].Lines = MakeClosePolyLines(lstBuilding[i].Coordinates);
                    }


                    //Find propose which has ex-structure.
                    for (int i = 0; i < iTotalPropose; i++)
                    {
                        LayerDataWithText itemPropose = lstPropose[i];
                        for (int j = 0; j < iTotalExStructure; j++)
                        {
                            LayerDataWithText itemExStruct = lstExStructure[j];
                             
                            List<Double> lstDistances = FindAllDistanceBetweenTwoPolygonUsingCoordinateAndLines(itemPropose, itemExStruct);
                            if (lstDistances != null && lstDistances.Min() == 0)
                            {
                                lstExStructurePropName.Add(ExtractLayerText(itemPropose, false, ""));
                                break;
                            }
                        }
                    }

                    List<LayerInfo> lstBuildingLayer = objLayerExtractor.SetLayerInfo(lstBuilding, DxfLayersName.Building);
                    objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.Floor, DxfLayersName.Floor, ref lstBuildingLayer);

                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    dict.Add(DxfLayersName.ExBuiltUpLine, DxfLayersName.ExBuiltUpLine);

                    foreach (LayerInfo itemBuilding in lstBuildingLayer)
                    {
                        if (!itemBuilding.Child.ContainsKey(DxfLayersName.Floor))
                            continue;

                        List<LayerInfo> lstFloor = itemBuilding.Child[DxfLayersName.Floor];
                        foreach (LayerInfo floor in lstFloor)
                        {
                            objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, dict, ref lstFloor);
                        }
                    }

                    DrawingValidateItem objExStructureValidation = new DrawingValidateItem();
                    objExStructureValidation.IsValid = true;
                    objExStructureValidation.Name = RuleName.ExBuiltupValidation;
                    objExStructureValidation.RuleType = RuleType.Text;
                    objExStructureValidation.RuleOn = "Layers";

                     

                    lstResult.Add(objExStructureValidation);
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

        public void ValidatePlinthBasementFloor(List<DrawingValidateItem> lstResult)
        {
            List<LayerDataWithText> lstBuilding = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.Building).ToList();
            List<LayerInfo> lstBuildingLayer = objLayerExtractor.SetLayerInfo(lstBuilding, DxfLayersName.Building);

            objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.Section, DxfLayersName.Section, ref lstBuildingLayer);
            objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.Floor, DxfLayersName.Floor, ref lstBuildingLayer);
             
            DrawingValidateItem plinthAndBasementFloorChecking = new DrawingValidateItem();
            plinthAndBasementFloorChecking.RuleType = RuleType.Text;
            plinthAndBasementFloorChecking.RuleOn = "Elements";
            plinthAndBasementFloorChecking.Name = RuleName.BasementFloorDataValidation;

            foreach (LayerInfo itemBuilding in lstBuildingLayer)
            {  
                string sBuildingName = ExtractLayerText(itemBuilding, false, "");

                if (!itemBuilding.Child.ContainsKey(DxfLayersName.Section))
                    return;

                List<LayerInfo> lstSection = itemBuilding.Child[DxfLayersName.Section];
                objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.FloorInSection, DxfLayersName.FloorInSection, ref lstSection);

                bool bPlinthSectionFound = false, bBasementSectionFound = false, bBasementFloorFound = false;
                LayerInfo objPlinth = new LayerInfo();
                LayerInfo objBasement = new LayerInfo();
                LayerInfo objBasementFloor = new LayerInfo();
                if (itemBuilding.Child.ContainsKey(DxfLayersName.Section))
                {
                    foreach (LayerInfo section in lstSection)
                    {
                        string sSectionName = ExtractLayerText(section, false, "");
                        if (section.Child.ContainsKey(DxfLayersName.FloorInSection))
                        {
                            List<LayerInfo> lstFloorSection = section.Child[DxfLayersName.FloorInSection];
                            foreach (LayerInfo floorSection in lstFloorSection)
                            {
                                string sFloorInSectionText = ExtractLayerText(floorSection, false, "");

                                if (string.IsNullOrWhiteSpace(sFloorInSectionText))
                                    continue;

                                if (regexPlinthName.IsMatch(sFloorInSectionText))
                                {
                                    bPlinthSectionFound = true;
                                    objPlinth = floorSection;
                                }
                                else if (regexBasementFloor.IsMatch(sFloorInSectionText))
                                {
                                    bBasementSectionFound = true;
                                    objBasement = floorSection;
                                }

                            }
                        }
                    }
                }

                if (itemBuilding.Child.ContainsKey(DxfLayersName.Floor))
                {
                    List<LayerInfo> lstFloors = itemBuilding.Child[DxfLayersName.Floor];
                    foreach (LayerInfo floor in lstFloors)
                    {
                        string sFloorText = ExtractLayerText(floor, false, "");

                        if (string.IsNullOrWhiteSpace(sFloorText))
                            continue;

                        if (regexBasementFloor.IsMatch(sFloorText))
                        {
                            bBasementFloorFound = true;
                            objBasementFloor = floor;
                        }
                    }
                }

                if (bPlinthSectionFound && bBasementSectionFound)
                {
                    plinthAndBasementFloorChecking.IsValid = false;
                    plinthAndBasementFloorChecking.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = $"{sBuildingName} has plinth and basement both found in floor section",
                        ElementPositionCoordinate = objPlinth.Data.Coordinates
                    });
                }

                if (bBasementSectionFound != bBasementFloorFound)
                {
                    if (bBasementSectionFound)
                    {
                        plinthAndBasementFloorChecking.IsValid = false;
                        plinthAndBasementFloorChecking.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = $"{sBuildingName} basement floor missing",
                            ElementPositionCoordinate = objBasement.Data.Coordinates
                        });
                    }
                    else { 
                        plinthAndBasementFloorChecking.IsValid = false;
                        plinthAndBasementFloorChecking.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = $"{sBuildingName} basement floor missing",
                            ElementPositionCoordinate = objBasementFloor.Data.Coordinates
                        });
                    }
                } 
            }

            lstResult.Add(plinthAndBasementFloorChecking);

        }
        public DrawingValidateItem ValidatePrintArea()
        {
            DrawingValidateItem objDrawingValidation = new DrawingValidateItem();
            objDrawingValidation.Name = RuleName.PrintAreaValidation;
            objDrawingValidation.RuleOn = "Print area";
            objDrawingValidation.RuleType = RuleType.Element;

            List<LayerDataWithText> lstPrintArea = allLayerData.Where(x => x.LayerName.ToLower() == DxfLayersName.PrintArea).ToList();
            List<LayerDataWithText> lstSitePlan = allLayerData.Where(x => x.LayerName.ToLower() == DxfLayersName.SitePlan).ToList();
            List<LayerDataWithText> lstFloorPlan = allLayerData.Where(x => x.LayerName.ToLower() == DxfLayersName.Floor).ToList(); // 04Mar2025 added floor

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
            List<LayerDataWithText> lstNetPlot = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.NetPlot).ToList();
            List<LayerDataWithText> lstRoadWidening = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.RoadWidening).ToList();
            List<LayerDataWithText> lstMainRoad = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.MainRoad).ToList();

            //if (lstMainRoad.Count(x => x.HasBulge == true) > 0)
            {
                for (int i = 0; i < lstMainRoad.Count(); i++)
                {
                    //if (lstMainRoad[i].HasBulge)
                    {
                        LayerDataWithText mainroad = lstMainRoad[i].DeepClone();
                        SetAdjustCoordinate(ref mainroad);
                        lstMainRoad[i] = mainroad;
                    }
                }
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
                //if (plot.HasBulge)
                    SetAdjustCoordinate(ref plot);

                foreach (LayerDataWithText itemMainRoad in lstMainRoad)
                {
                    LayerDataWithText mainroad = itemMainRoad.DeepClone();
                    SetAdjustCoordinate(ref mainroad);

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

                foreach (LayerDataWithText itemRoadWidening in lstRoadWidening)
                {
                    LayerDataWithText roadwidening = itemRoadWidening.DeepClone();
                    //if (roadwidening.HasBulge)
                        SetAdjustCoordinate(ref roadwidening);

                    double dIntersectArea = 0, dIntersectRoomArea = 0;
                    if (!IsInPolyUsingAngle(plot.Coordinates, roadwidening.Coordinates) && objNetTopologySuite.IsIntersects(plot.Coordinates, roadwidening.Coordinates, ref dIntersectArea, ref dIntersectRoomArea))
                    {
                        if (dIntersectArea > ErrorAllowScale)
                        {
                            objSiteplanValidation.IsValid = false;
                            objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = "Plot crossover with road widening",
                                ElementPositionCoordinate = roadwidening.Coordinates
                            });
                        }
                    }
                }
            }


            for (int i = 0; i < lstMainRoad.Count(); i++)
            { 
                foreach (LayerDataWithText itemRoadWidening in lstRoadWidening)
                {
                    LayerDataWithText roadwidening = itemRoadWidening.DeepClone();
                    //if (roadwidening.HasBulge)
                        SetAdjustCoordinate(ref roadwidening);

                    double dIntersectArea = 0, dIntersectRoomArea = 0;
                    if (!IsInPolyUsingAngle(lstMainRoad[i].Coordinates, roadwidening.Coordinates) && objNetTopologySuite.IsIntersects(lstMainRoad[i].Coordinates, roadwidening.Coordinates, ref dIntersectArea, ref dIntersectRoomArea))
                    {
                        if (dIntersectArea > ErrorAllowScale)
                        {
                            objSiteplanValidation.IsValid = false;
                            objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = "Main road crossover with road widening",
                                ElementPositionCoordinate = roadwidening.Coordinates
                            });
                        }
                    }
                    else if (IsInPolyUsingAngle(lstMainRoad[i].Coordinates, roadwidening.Coordinates))
                    {
                        objSiteplanValidation.IsValid = false;
                        objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = "Road widening found within Main road",
                            ElementPositionCoordinate = roadwidening.Coordinates
                        });
                    }
                }
            }

            // // 04Apr2025  roadwidening not allowed to touch multiple 
            foreach (LayerDataWithText itemRoadWidening in lstRoadWidening)
            {
                LayerDataWithText roadwidening = itemRoadWidening.DeepClone();
                //if (roadwidening.HasBulge)
                    SetAdjustCoordinate(ref roadwidening);

                int iCountTouchedRoadWidending = 0;
                foreach (LayerDataWithText itemMainRoad in lstMainRoad)
                {

                    LayerDataWithText mainroad = itemMainRoad.DeepClone();
                    //if (mainroad.HasBulge)
                        SetAdjustCoordinate(ref mainroad);

                    List<double> lstDistances = FindAllDistanceBetweenTwoPolygonUsingCoordinateAndLines(roadwidening, mainroad);
                    if (lstDistances != null && lstDistances.Count() > 0 && lstDistances.Min() <= ErrorAllowScale)
                    {
                        iCountTouchedRoadWidending++;
                    }
                }

                if (iCountTouchedRoadWidending > 1)
                {
                    objSiteplanValidation.IsValid = false;
                    objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                    {
                        ErrorMessage = "Road widening touched multiple main roads.",
                        ElementPositionCoordinate = itemRoadWidening.Coordinates
                    });
                }
            }

            foreach (LayerDataWithText itemNetplot in lstNetPlot)
            {
                LayerDataWithText netplot = itemNetplot.DeepClone();
                //if (netplot.HasBulge)
                    SetAdjustCoordinate(ref netplot);

                foreach (LayerDataWithText itemRoadWidening in lstRoadWidening)
                {
                    LayerDataWithText roadwidenning = itemRoadWidening.DeepClone();
                    //if (roadwidenning.HasBulge)
                        SetAdjustCoordinate(ref roadwidenning);

                    double dIntersectArea = 0, dIntersectRoomArea = 0;
                    if (!IsInPolyUsingAngle(netplot.Coordinates, roadwidenning.Coordinates) && objNetTopologySuite.IsIntersects(netplot.Coordinates, roadwidenning.Coordinates, ref dIntersectArea, ref dIntersectRoomArea))
                    {
                        if (dIntersectArea > ErrorAllowScaleForParallel)
                        {
                            objSiteplanValidation.IsValid = false;
                            objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = "Main road crossover with road widening",
                                ElementPositionCoordinate = roadwidenning.Coordinates
                            });
                        }
                    }
                    else if (IsInPolyUsingAngle(netplot.Coordinates, roadwidenning.Coordinates))
                    {
                        objSiteplanValidation.IsValid = false;
                        objSiteplanValidation.ErrorElements.Add(new ItemErrorDetails
                        {
                            ErrorMessage = "Road widening found within net plot",
                            ElementPositionCoordinate = roadwidenning.Coordinates
                        });
                    }
                }
            }

            lstResult.Add(objSiteplanValidation);

        }
        public void _ProposeWorkWithGroundFloorBuiltupAreaCompare(ref List<DrawingValidateItem> lstResult)
        {
            //MathLib objMath = new MathLib();

            List<LayerDataWithText> lstProposeWork = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.Propwork).ToList();

            Dictionary<string, string> dictLayerVsKey = new Dictionary<string, string>();

            dictLayerVsKey.Add(DxfLayersName.CommercialBuiltUpLine, DxfLayersName.CommercialBuiltUpLine);
            dictLayerVsKey.Add(DxfLayersName.ResidentBuiltUpLine, DxfLayersName.ResidentBuiltUpLine);
            dictLayerVsKey.Add(DxfLayersName.SpecialUseBuiltUpLine, DxfLayersName.SpecialUseBuiltUpLine);
            dictLayerVsKey.Add(DxfLayersName.IndustrialBuiltUpLine, DxfLayersName.IndustrialBuiltUpLine);

            List<LayerDataWithText> lstBuilding = objLayerExtractor.ExtractLayersData(allLayerData, DxfLayersName.Building);
            List<LayerInfo> lstBuildingLayer = objLayerExtractor.SetLayerInfo(lstBuilding, DxfLayersName.Building);

            //Building wise floor
            objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.Floor, DxfLayersName.Floor, ref lstBuildingLayer);

            //Dictionary<string, double> dictBuildingProposeArea = new Dictionary<string, double>();
            //foreach (LayerDataWithText proposedWork in lstProposeWork)
            //{
            //    string sProposeText = ExtractLayerText(proposedWork, false, "");
            //    double dProposeArea = objMath.FindAreaByCordinates(proposedWork);
            //    foreach( BuildingNamingMapping item in  buildingFloorMappingData)
            //    {
            //        dictBuildingProposeArea.Add(item.BuildingName, dProposeArea);
            //        foreach(string sBuildingMap in item.BuildingMapping)
            //        {
            //            if(!dictBuildingProposeArea.Keys.Contains(sBuildingMap))
            //                dictBuildingProposeArea.Add(sBuildingMap, dProposeArea); ;
            //        }
            //    }
            //}


            int iBuilding = 1;
            foreach (LayerInfo itemBuilding in lstBuildingLayer)
            {
                string sBuildingText = ExtractLayerText(itemBuilding, false, string.Format(BuildingText, iBuilding));
                iBuilding++;


                string sProposeText = "";
                double dProposeArea = 0d;
                if (string.IsNullOrWhiteSpace(sBuildingText))
                    continue;

                foreach (LayerDataWithText proposedWork in lstProposeWork)
                {
                    sProposeText = ExtractLayerText(proposedWork, false, "");
                    if (!string.IsNullOrWhiteSpace(sProposeText) && sProposeText.IsEquals(sBuildingText))
                    {
                        dProposeArea = FindAreaByCoordinates(proposedWork);
                        break;
                    }
                    
                }

                if (dProposeArea == 0d)
                    continue;

                bool bGroundFloorExists = false, bParkingFloorExists = false;
                foreach (string sFloorKey in itemBuilding.Child.Keys)
                {
                    List<LayerInfo> floorLayerInfo = itemBuilding.Child[sFloorKey];
                    if (floorLayerInfo == null || floorLayerInfo.Count <= 0)
                        continue;

                    //floor wise all data extract
                    for (int i = 0; i < floorLayerInfo.Count; i++)
                    {
                        LayerInfo floorInfo = floorLayerInfo[i];
                        objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, dictLayerVsKey, ref floorInfo);
                    }
                }

                foreach (string sFloorKey in itemBuilding.Child.Keys)
                {
                    List<LayerInfo> floorLayerInfo = itemBuilding.Child[sFloorKey];
                    if (floorLayerInfo == null || floorLayerInfo.Count <= 0)
                        continue;

                    //floor wise all data extract
                    for (int i = 0; i < floorLayerInfo.Count; i++)
                    {
                        LayerInfo floorInfo = floorLayerInfo[i];
                        string sFloorText = ExtractLayerText(floorInfo, false, "Floor" + i);

                        if (regexGroundFloor.IsMatch(sFloorText))
                            bGroundFloorExists = true;
                        else if (regexParkingFloorName.IsMatch(sFloorText))
                            bParkingFloorExists = true;
                    }
                }


                double dTotalGroundFloorArea = 0d;
                //Floor wise
                foreach (string sFloorKey in itemBuilding.Child.Keys)
                {
                    List<LayerInfo> floorLayerInfo = itemBuilding.Child[sFloorKey];
                    if (floorLayerInfo == null || floorLayerInfo.Count <= 0)
                        continue;

                    //floor wise all data extract
                    for (int i = 0; i < floorLayerInfo.Count; i++)
                    {
                        LayerInfo floorInfo = floorLayerInfo[i];

                        string sFloorText = ExtractLayerText(floorInfo, false, "Floor" + i);

                        if (!bGroundFloorExists && !bParkingFloorExists)
                            break;

                        if (bGroundFloorExists)
                        {
                            if (regexGroundFloor.IsMatch(sFloorText) == false)
                                continue;
                        }
                        else if (bParkingFloorExists)
                        {
                            if (regexParkingFloorName.IsMatch(sFloorText) == false)
                                continue;
                        }

                        Dictionary<string, List<LayerInfo>> htData = floorInfo.Child;
                        string[] arrBuiltupKeys = new string[] { DxfLayersName.CommercialBuiltUpLine, DxfLayersName.IndustrialBuiltUpLine, DxfLayersName.ResidentBuiltUpLine, DxfLayersName.SpecialUseBuiltUpLine, DxfLayersName.Parking };

                        foreach (string sBuiltupKey in arrBuiltupKeys)
                        {
                            if (htData.ContainsKey(sBuiltupKey))
                            {
                                List<LayerInfo> lstBuiltup = htData[sBuiltupKey];
                                foreach (LayerInfo item in lstBuiltup)
                                    dTotalGroundFloorArea += FindAreaByCoordinates(item);
                            }
                        }

                        if (dTotalGroundFloorArea != 0d && dProposeArea != 0d && Math.Round(dProposeArea, 1) != Math.Round(dTotalGroundFloorArea, 1))
                        {
                            DrawingValidateItem objResult = new DrawingValidateItem();
                            objResult.RuleType = RuleType.Element;
                            objResult.Name = RuleName.ProposeworkWithGroundfloorBuiltupAreaValidation;
                            objResult.RuleOn = "Propose work Floor builtup";

                            objResult.IsValid = false;
                            objResult.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = $"Building: {sBuildingText} ground floor area {dTotalGroundFloorArea} does not match with {sProposeText} proposed area {dProposeArea}",
                                ElementPositionCoordinate = floorInfo.Data.Coordinates
                            });
                            lstResult.Add(objResult);
                        }
                        else if (dTotalGroundFloorArea == 0d)
                        {
                            DrawingValidateItem objResult = new DrawingValidateItem();
                            objResult.RuleType = RuleType.Element;
                            objResult.Name = RuleName.ProposeworkWithGroundfloorBuiltupAreaValidation;
                            objResult.RuleOn = "Propose work Floor builtup";

                            objResult.IsValid = false;
                            objResult.ErrorElements.Add(new ItemErrorDetails
                            {
                                ErrorMessage = $"Building: {sBuildingText} Ground floor built-up line is missing",
                                ElementPositionCoordinate = floorInfo.Data.Coordinates
                            });
                            lstResult.Add(objResult);
                        }
                    }
                }
            }

            if (lstResult.Count(x => x.Name == RuleName.ProposeworkWithGroundfloorBuiltupAreaValidation) == 0)
            {
                DrawingValidateItem objResult = new DrawingValidateItem();
                objResult.RuleType = RuleType.Element;
                objResult.Name = RuleName.ProposeworkWithGroundfloorBuiltupAreaValidation;
                objResult.RuleOn = "Propose work Floor builtup";
                lstResult.Add(objResult);
            }

        }
        public void AllMezzanineFloorIndexFloorInSection(string sBuildingName, List<LayerInfo> lstFloorInSection, List<LayerInfo> lstMazzanineFloor, ref List<DrawingValidateItem> lstResult)
        {
            DrawingValidateItem objResult = new DrawingValidateItem();
            objResult.RuleType = RuleType.Element;
            objResult.Name = RuleName.MezzanineWithInFloorInSectionDataValidation;
            objResult.RuleOn = "MezzanineWithInFloorInSection";
            lstResult.Add(objResult);
        }
      public DrawingValidateItem LayerDataNotHaveAnyChildValidation(string itemName, string layerName, List<LayerDataWithText> allLayerData, List<string> lstIgnoreLayer)
        {
            return LayerDataNotHaveAnyChildValidation(itemName, layerName, allLayerData, lstIgnoreLayer, ErrorAllowScale);
        }
        public DrawingValidateItem LayerDataNotHaveAnyChildValidation(string itemName, string layerName, List<LayerDataWithText> allLayerData, List<string> lstIgnoreLayer, double errorAllow)
        {
            DrawingValidateItem objDrawingValidation = new DrawingValidateItem();
            objDrawingValidation.Name = itemName;

            // Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + $"LayerDataNotHaveAnyChildValidation start ");
            List<LayerDataWithText> lstBuilding = allLayerData.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.Building).ToList();
            List<LayerInfo> lstBuildingLayer = objLayerExtractor.SetLayerInfo(lstBuilding, DxfLayersName.Building);

            objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.Floor, DxfLayersName.Floor, ref lstBuildingLayer);
            objLayerExtractor.ExtractChildLayersForParentLayer(allLayerData, DxfLayersName.Section, DxfLayersName.Section, ref lstBuildingLayer);

            // Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + $"LayerDataNotHaveAnyChildValidation build vs floor ");

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

            //List<string> lstBuiltupLine = new List<string> { DxfLayersName.CommercialBuiltUpLine, DxfLayersName.ResidentBuiltUpLine, DxfLayersName.SpecialUseBuiltUpLine, DxfLayersName.IndustrialBuiltUpLine };

            // Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + $"LayerDataNotHaveAnyChildValidation dictionary ");
            foreach (LayerInfo itemBuilding in lstBuildingLayer)
            {
                foreach (string sBuiltupKey in itemBuilding.Child.Keys)
                {
                    List<LayerInfo> lstSectionOrFloor = itemBuilding.Child[sBuiltupKey].DeepClone();

                    List<LayerInfo> lstSectionOrFloorChecking = itemBuilding.Child[sBuiltupKey]; // lstSectionOrFloor.DeepClone();

                    //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + $"LayerDataNotHaveAnyChildValidation floor section data ");
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

                    //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + $"LayerDataNotHaveAnyChildValidation floor section data end ");
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
                        //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + $"LayerDataNotHaveAnyChildValidation layer data  " + iTotal);
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
                        //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " : " + $"LayerDataNotHaveAnyChildValidation layer data now layer with other ");

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

                            bool IsCheckConditionForTerrace = false;
                            if (layerData[i].Data.LayerName.Equals(DxfLayersName.StairCase) || layerData[i].Data.LayerName.Equals(DxfLayersName.Lift))
                                IsCheckConditionForTerrace = true;

                            foreach (String s in lstLayerNameToCompare)
                            {
                                bool IsBuiltupLine = false;
                                allDataWithoutLayerData = lstCompareList.Where(x => x.Data.LayerName.Equals(s)).ToList();
                                IsBuiltupLine = GetAllBuiltupLineList.Contains(s);
                                foreach (LayerInfo item in allDataWithoutLayerData)
                                {
                                    double dErrorAllow = errorAllow;
                                    if (item.Data.LayerName.Trim().ToLower() == DxfLayersName.Door)
                                    {
                                        dErrorAllow = ErrorAllowScaleForDoor;
                                    }

                                    if (IsInPolyUsingAngle(layerData[i].Data.Coordinates, item.Data.Coordinates, dErrorAllow))
                                    {
                                        if (IsCheckConditionForTerrace && IsTerraceFloor && IsBuiltupLine)
                                            continue;

                                        objDrawingValidation.IsValid = false;

                                        objDrawingValidation.ErrorElements.Add(new ItemErrorDetails
                                        {
                                            ElementPositionCoordinate = item.Data.Coordinates,
                                            ErrorMessage = item.Data.LayerName + " " + ExtractLayerText(item, true, "") + " found within " + sText
                                        });
                                    }
                                }
                            }
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

