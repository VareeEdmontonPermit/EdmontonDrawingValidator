using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
using EdmontonDrawingValidator.Model;
using SharedClasses;
using System.Text;
using Newtonsoft.Json;

namespace EdmontonDrawingValidator
{
    public class LayerExtractor : MathLib
    {
        public void ExtractLayerCoordinateAndTextData(string sInputFilePath, ref Dictionary<string, string> dictLayerDefaulfColour, ref List<LayerCoordinateInfo> lstCordinate, ref List<LayerTextInfo> lstTextInfo, ref bool HasError, ref List<string> lstError)
        {
            lstCordinate = new List<LayerCoordinateInfo>();
            lstTextInfo = new List<LayerTextInfo>();

            List<string> lstBlock = new List<string>();
            List<string> lstBlockRef = new List<string>();
            List<string> lstBlockEntities = new List<string>();
            Dictionary<string, string> dicLayerWiseDefaultColour = new Dictionary<string, string>();
            StringBuilder sbBlock = new StringBuilder();
            string DrawingScaleCode = "";
            try
            {

                List<string> lstLayer = new List<string>();
                using (FileStream fs = File.Open(sInputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                using (StreamReader sr = new StreamReader(bs))
                {
                    bool bEntityFound = false;
                    string line;
                    int iLineCounter = 0;
                    iLineCounter++;
                    bool bBlockData = false, bBlockRefData = false, bBlockEntity = false, bScaleFound = false;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!bScaleFound)
                        {
                            if (IsSameText(line, DxfLayersName.ScaleUnit))
                            {
                                while (true)
                                {
                                    line = sr.ReadLine();
                                    if (IsSameText(line, DxfLayersName.StandardFlagsCodeValue) || IsSameText(line, DxfLayersName.StandardFlagsCode))
                                    {
                                        line = sr.ReadLine();
                                        DrawingScaleCode = line.Trim();
                                        bScaleFound = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!bBlockData && IsSameText(line, DxfLayersName.TableRecord))
                        {
                            string sLayerName = "", LayerDefaultColour = "";
                            while (true)
                            {
                                line = sr.ReadLine();
                                if (line == DxfLayersName.BlockNameCodeValue)
                                {
                                    sLayerName = CleanLayerName(sr.ReadLine()).Trim();
                                    continue;
                                }
                                else if (line == DxfLayersName.LineColourCodeValue2 || line == DxfLayersName.LineColourCodeValue1)
                                {
                                    LayerDefaultColour = sr.ReadLine().Trim();

                                    if (!string.IsNullOrWhiteSpace(sLayerName) && !string.IsNullOrWhiteSpace(LayerDefaultColour))
                                    {
                                        if (!dicLayerWiseDefaultColour.ContainsKey(sLayerName.ToLower().Trim()))
                                        {
                                            int iColourCode = -1;
                                            try
                                            {
                                                iColourCode = Math.Abs(int.Parse(LayerDefaultColour.Trim()));
                                            }
                                            catch { }
                                            if (iColourCode > 0)
                                                dicLayerWiseDefaultColour.Add(sLayerName.ToLower().Trim(), iColourCode + "");
                                            else
                                                dicLayerWiseDefaultColour.Add(sLayerName.ToLower().Trim(), LayerDefaultColour.Trim());
                                        }
                                        sLayerName = ""; LayerDefaultColour = "";
                                        break;
                                    }
                                }
                                else if (line == DxfLayersName.AcdbBlockEndValue)
                                {
                                    sLayerName = ""; LayerDefaultColour = "";
                                    break;
                                }
                            }
                            continue;
                        }
                        else if (!bBlockData && IsSameText(line, DxfLayersName.Entity))
                        {
                            bEntityFound = true;

                            if (sbBlock.Length != 0)
                            {
                                sbBlock.AppendLine(line);
                                lstBlock.Add(sbBlock.ToString());
                            }

                            sbBlock = new StringBuilder("");
                            sbBlock.AppendLine(line);

                            while (true)
                            {
                                line = sr.ReadLine();
                                sbBlock.AppendLine(line);

                                if (!string.IsNullOrWhiteSpace(line) && IsSameText(line, DxfLayersName.Entity))
                                {
                                    bEntityFound = true;
                                    sbBlock = new StringBuilder("");
                                    sbBlock.AppendLine(line);
                                    continue;
                                }
                                else if (!string.IsNullOrWhiteSpace(line) && bEntityFound && IsSameText(line, DxfLayersName.LayerCommandCode))
                                {
                                    string sCommand = sr.ReadLine().Trim();
                                    iLineCounter++;
                                    sbBlock.AppendLine(sCommand);

                                    if (bEntityFound && (StartWith(sCommand, DxfLayersName.PolyLine) || StartWith(sCommand, DxfLayersName.Line)))
                                    {
                                        bBlockData = false; bBlockRefData = false; bBlockEntity = true;
                                        break;
                                    }
                                    else if (bEntityFound && (StartWith(sCommand, DxfLayersName.Text) || StartWith(sCommand, DxfLayersName.MText)))
                                    {
                                        bBlockData = false; bBlockRefData = false; bBlockEntity = true;
                                        break;
                                    }
                                    else if (bEntityFound && StartWith(sCommand, DxfLayersName.Circle))
                                    {
                                        bBlockData = false; bBlockRefData = false; bBlockEntity = true;
                                        break;
                                    }
                                    else if (bEntityFound && IsSameText(sCommand, DxfLayersName.AcdbBlockBegin))
                                    {
                                        bBlockData = true; bBlockRefData = false; bBlockEntity = false;
                                        break;
                                    }
                                    else if (bEntityFound && IsSameText(sCommand, DxfLayersName.AcdbBlockReference))
                                    {
                                        bBlockData = false; bBlockRefData = true; bBlockEntity = false;
                                        break;
                                    }
                                    else
                                    {
                                        bEntityFound = false;
                                        sbBlock = new StringBuilder("");
                                        break;
                                    }
                                }
                            }
                            continue;
                        }
                        else if (bEntityFound)
                        {
                            if (bBlockData)
                            {
                                if (!string.IsNullOrWhiteSpace(line) && (IsSameText(line, DxfLayersName.AcdbBlockEnd) || IsSameText(line, DxfLayersName.EndBlock)))
                                {
                                    if (sbBlock.Length != 0)
                                    {
                                        sbBlock.AppendLine(line);
                                        lstBlock.Add(sbBlock.ToString());
                                    }
                                    sbBlock = new StringBuilder("");
                                    bEntityFound = false;
                                    bBlockData = false;
                                }
                                else
                                    sbBlock.AppendLine(line);
                            }
                            else if (!string.IsNullOrWhiteSpace(line) && bBlockData == false && line == DxfLayersName.AcdbBlockEndValue)
                            {
                                if (sbBlock.Length != 0)
                                {
                                    sbBlock.AppendLine(line);
                                    if (bBlockRefData)
                                        lstBlockRef.Add(sbBlock.ToString());
                                    else if (bBlockEntity)
                                        lstBlockEntities.Add(sbBlock.ToString());
                                }
                                sbBlock = new StringBuilder("");
                                bEntityFound = false;
                                bBlockEntity = false;
                                bBlockRefData = false;
                            }
                            else
                                sbBlock.AppendLine(line);
                        }
                        iLineCounter++;

                    } //while loop
                } // using
            }
            catch { }

            //check scalling
            if (!DrawingScaleCode.Equals("6"))
            {
                HasError = true;
                lstError.Add("Drawing should be in meter unit.");
            }

            List<LayerCoordinateInfo> lstCoordinateBlock = new List<LayerCoordinateInfo>();
            List<LayerCoordinateInfo> lstCoordinateRef = new List<LayerCoordinateInfo>();
            List<LayerTextInfo> lstTextInfoData = new List<LayerTextInfo>();

            if (General.DebugLogWithDataEnabled)
            {
                int iCount = lstBlock.Count;
                string sFilePath = @"F:\BKPatel\UserData\Testcase11\block.txt";
                File.WriteAllText(sFilePath, "");
                foreach (string sBlock in lstBlock)
                {
                    File.AppendAllText(sFilePath, sBlock);
                    File.AppendAllText(sFilePath, "\r\n\r\n----------------\r\n\r\n");
                }
            }

            List<LayerCoordinateInfo> lstNewItemsBlock = new List<LayerCoordinateInfo>();
            List<LayerCoordinateInfo> lstNewItemsBlockItems = new List<LayerCoordinateInfo>();
            List<LayerTextInfo> lstNewTextItemsBlockItem = new List<LayerTextInfo>();
            foreach (string sBlock in lstBlock)
            {
                ProcessBlockBeginDataToExtractElements(sBlock, ref lstNewItemsBlock, ref lstNewItemsBlockItems, ref lstNewTextItemsBlockItem);
            }

            if (General.DebugLogWithDataEnabled)
            {
                string sJsonFilePath = @"F:\BKPatel\UserData\Testcase11\blockItems.json";
                File.WriteAllText(sJsonFilePath, JsonConvert.SerializeObject(lstNewItemsBlockItems, Formatting.Indented));

                string sJsonTextFilePath = @"F:\BKPatel\UserData\Testcase11\blockTextItems.json";
                File.WriteAllText(sJsonTextFilePath, JsonConvert.SerializeObject(lstNewTextItemsBlockItem, Formatting.Indented));

                string sFilePathBlockRef = @"F:\BKPatel\UserData\Testcase11\blockRef.txt";
                File.WriteAllText(sFilePathBlockRef, "");
                foreach (string sblock in lstBlockRef)
                {
                    File.AppendAllText(sFilePathBlockRef, sblock);
                    File.AppendAllText(sFilePathBlockRef, "\r\n\r\n----------------\r\n\r\n");
                }
            }

            List<LayerCoordinateInfo> lstNewItemsBlockRef = new List<LayerCoordinateInfo>();
            foreach (string sBlockRef in lstBlockRef)
            {
                ProcessBlockReferenceData(sBlockRef, ref lstNewItemsBlockRef);
            }

            if (General.DebugLogWithDataEnabled)
            {
                string sBlockRefJsonTextFilePath = @"F:\BKPatel\UserData\Testcase11\blockRefItems.json";
                File.WriteAllText(sBlockRefJsonTextFilePath, JsonConvert.SerializeObject(lstNewItemsBlockRef, Formatting.Indented));
            }

            //Drawing error checking
            if (lstNewItemsBlockRef != null && lstNewItemsBlockRef.Count(x => x.LayerName.Trim().ToLower() == DxfLayersName.Unit) > 0)
            {
                HasError = true;
                lstError.Add($"{DxfLayersName.Unit} has reference element found.");
            }

            if (lstNewItemsBlockRef != null && lstNewItemsBlockRef.Count(x => x.LayerName.Trim().ToLower() == DxfLayersName.FloorPlan) > 0)
            {
                HasError = true;
                lstError.Add($"{DxfLayersName.FloorPlan} has reference element found.");
            }

            //clear ref element
            lstNewItemsBlockRef = lstNewItemsBlockRef.Where(x => x.LayerName.ToLower().Trim() != DxfLayersName.Unit && x.LayerName.ToLower().Trim() != DxfLayersName.FloorPlan).ToList();

            List<LayerCoordinateInfo> lstNewItemsBlockRefElements = new List<LayerCoordinateInfo>();
            List<LayerTextInfo> lstNewItemsBlockRefTextElements = new List<LayerTextInfo>();
            foreach (LayerCoordinateInfo itemBlockRef in lstNewItemsBlockRef)
            {
                List<LayerCoordinateInfo> lstElementsFound = new List<LayerCoordinateInfo>();
                List<LayerTextInfo> lstTextElementsFound = new List<LayerTextInfo>();

                ProcessReferenceToGetBlockElements(itemBlockRef, lstNewItemsBlock.ToList(), lstNewItemsBlockItems.ToList(), lstNewTextItemsBlockItem.ToList(), ref lstElementsFound, ref lstTextElementsFound);

                if (lstElementsFound != null && lstElementsFound.Count > 0)
                {
                    lstNewItemsBlockRefElements.AddRange(lstElementsFound);
                }

                if (lstTextElementsFound != null && lstTextElementsFound.Count > 0)
                {
                    lstNewItemsBlockRefTextElements.AddRange(lstTextElementsFound);
                }
            }

            //Process all data and adjust cordinate
            if (General.DebugLogWithDataEnabled)
            {
                string sFilePathEntities = @"F:\BKPatel\UserData\Testcase11\entities.txt";
                File.WriteAllText(sFilePathEntities, "");
                foreach (string sBlock in lstBlockEntities)
                {
                    File.AppendAllText(sFilePathEntities, sBlock);
                    File.AppendAllText(sFilePathEntities, "\r\n\r\n----------------\r\n\r\n");
                }
            }

            List<LayerCoordinateInfo> lstEntitiesItemsBlock = new List<LayerCoordinateInfo>();
            List<LayerCoordinateInfo> lstEntitiesItemsBlockItems = new List<LayerCoordinateInfo>();
            List<LayerTextInfo> lstEntitiesTextItemsBlockItem = new List<LayerTextInfo>();
            foreach (string sBlock in lstBlockEntities)
            {
                ProcessBlockBeginDataToExtractElements(sBlock, ref lstEntitiesItemsBlock, ref lstEntitiesItemsBlockItems, ref lstEntitiesTextItemsBlockItem);
            }

            if (General.DebugLogWithDataEnabled)
            {
                string sEntitiesFilePath = @"F:\BKPatel\UserData\Testcase11\entitiesElements.json";
                File.WriteAllText(sEntitiesFilePath, JsonConvert.SerializeObject(lstEntitiesItemsBlockItems, Formatting.Indented));

                string sEntitiesTextFilePath = @"F:\BKPatel\UserData\Testcase11\entitiesText.json";
                File.WriteAllText(sEntitiesTextFilePath, JsonConvert.SerializeObject(lstEntitiesTextItemsBlockItem, Formatting.Indented));
            }

            lstCordinate.AddRange(lstNewItemsBlockRefElements);
            lstTextInfo.AddRange(lstNewItemsBlockRefTextElements);

            lstCordinate.AddRange(lstEntitiesItemsBlockItems);
            lstTextInfo.AddRange(lstEntitiesTextItemsBlockItem);

            lstCordinate = lstCordinate.Where(x => x.LayerName.ToLower().Trim().StartsWith("_")).ToList();
            lstTextInfo = lstTextInfo.Where(x => x.LayerName.ToLower().Trim().StartsWith("_")).ToList();

            SetBulgeElementCoordinate(ref lstCordinate);

            //added on 25Jun2022
            SetCircleElementCoordinate(ref lstCordinate);

            // Set default colour 
            foreach (LayerCoordinateInfo item in lstCordinate)
            {
                if (string.IsNullOrWhiteSpace(item.ColourCode) && dicLayerWiseDefaultColour.ContainsKey(item.LayerName.ToLower().Trim()))
                {
                    item.ColourCode = dicLayerWiseDefaultColour[item.LayerName.ToLower().Trim()];
                }
            }

            foreach (LayerTextInfo item in lstTextInfo)
            {
                if (string.IsNullOrWhiteSpace(item.ColourCode) && dicLayerWiseDefaultColour.ContainsKey(item.LayerName.ToLower().Trim()))
                {
                    item.ColourCode = dicLayerWiseDefaultColour[item.LayerName.ToLower().Trim()];
                }
            }

            dictLayerDefaulfColour = dicLayerWiseDefaultColour;

            //return lstResult;
        }

        /// <summary>
        /// InputText string value start with given MatchText or not
        /// </summary>
        /// <param name="sInputText"></param>
        /// <param name="sMatchText"></param>
        /// <returns></returns>
        public bool StartWith(string InputText, string MatchText)
        {
            return InputText.Trim().ToLower().StartsWith(MatchText.Trim().ToLower());
        }

        /// <summary>
        /// Process block begin and block reference data for adjust the x and y value where ref. added.
        /// </summary>
        public void NotUsed_ProcessBlockBeginAndBlockReferenceData(List<LayerCoordinateInfo> lstResult, List<LayerTextInfo> lstTextResult, ref List<LayerCoordinateInfo> lstNewCordinateData, ref List<LayerTextInfo> lstNewTextCordinateData)
        {
            lstNewCordinateData = new List<LayerCoordinateInfo>();
            lstNewTextCordinateData = new List<LayerTextInfo>();

            try
            {
                List<LayerCoordinateInfo> lstData = lstResult.ToList();
                List<LayerTextInfo> lstTextData = lstTextResult.ToList();

                List<LayerCoordinateInfo> lstResultBlock = lstData.Where(x => x.IsBlockBeginEntry == true).ToList();
                List<LayerCoordinateInfo> lstResultBlockRef = lstData.Where(x => x.IsBlockReferenceElement == true).ToList();
                List<LayerCoordinateInfo> lstResultBlockItems = lstData.Where(x => x.IsBlockElement == true && !string.IsNullOrWhiteSpace(x.BlockName)).ToList();

                List<LayerTextInfo> lstTextResultBlockItems = lstTextData.Where(x => !string.IsNullOrWhiteSpace(x.BlockName)).ToList();
                List<LayerTextInfo> lstTextItems = lstTextData.Where(x => string.IsNullOrWhiteSpace(x.BlockName)).ToList();

                List<LayerCoordinateInfo> lstResultItems = lstData.Where(x => x.IsBlockElement == true && string.IsNullOrWhiteSpace(x.BlockName) && !string.IsNullOrWhiteSpace(x.LayerName) && x.LayerName.StartsWith("_")).ToList();

                foreach (LayerCoordinateInfo blockItem in lstResultBlock)
                {
                    List<LayerCoordinateInfo> lstBlockItems = lstResultBlockItems.Where(x => x.BlockName == blockItem.BlockName).ToList();
                    List<LayerTextInfo> lstBlockTextItems = lstTextResultBlockItems.Where(x => x.BlockName == blockItem.BlockName).ToList();
                    List<LayerCoordinateInfo> lstBlockRefItems = lstResultBlockRef.Where(x => x.BlockName == blockItem.BlockName && !string.IsNullOrWhiteSpace(x.LayerName) && x.LayerName.StartsWith("_")).ToList();

                    double beginX = blockItem.BlockCoordinate.X;
                    double beginY = blockItem.BlockCoordinate.Y;

                    if (lstBlockRefItems != null && lstBlockRefItems.Count > 0)
                    {
                        foreach (LayerCoordinateInfo blockRefItem in lstBlockRefItems)
                        {
                            double beginRefX = blockRefItem.BlockReferenceCoordinate.X;
                            double beginRefY = blockRefItem.BlockReferenceCoordinate.Y;

                            double diffX = beginRefX - beginX;
                            double diffY = beginRefY - beginY;

                            //data
                            if (lstBlockItems != null && lstBlockItems.Count > 0)
                            {
                                foreach (LayerCoordinateInfo item in lstBlockItems)
                                {
                                    LayerCoordinateInfo obj = new LayerCoordinateInfo();
                                    obj = item.DeepClone();
                                    obj.LayerName = CleanLayerName(blockRefItem.LayerName);
                                    obj.BlockName = blockRefItem.BlockName;
                                    obj.IsBlockElement = true;

                                    //adjust cordinate 
                                    obj.Coordinates.ForEach(item =>
                                    {
                                        item.X += diffX;
                                        item.Y += diffY;
                                    });

                                    //adjust bulge data
                                    obj.CoordinateWithBulge.ForEach(item =>
                                    {
                                        if (item.IsBulgeValue)
                                        {
                                            item.ItemValue.StartPoint.X += diffX;
                                            item.ItemValue.StartPoint.Y += diffY;
                                            item.ItemValue.EndPoint.X += diffX;
                                            item.ItemValue.EndPoint.Y += diffY;
                                        }
                                        else
                                        {
                                            item.ItemValue.StartPoint.X += diffX;
                                            item.ItemValue.StartPoint.Y += diffY;
                                        }
                                    });

                                    //adjust circle center point
                                    if (obj.IsCircle && obj.CenterPoint != null)
                                    {
                                        obj.CenterPoint.X += diffX;
                                        obj.CenterPoint.Y += diffY;
                                    }

                                    lstResultItems.Add(obj);
                                }
                            }

                            //Text data
                            if (lstBlockTextItems != null && lstBlockTextItems.Count > 0)
                            {
                                foreach (LayerTextInfo item in lstBlockTextItems)
                                {
                                    LayerTextInfo obj = new LayerTextInfo();
                                    obj = item.DeepClone();
                                    obj.LayerName = CleanLayerName(blockRefItem.LayerName);
                                    obj.BlockName = blockRefItem.BlockName;

                                    //adjust cordinate 
                                    obj.Coordinates.ForEach(item =>
                                    {
                                        item.X += diffX;
                                        item.Y += diffY;
                                    });

                                    lstTextItems.Add(obj);
                                }
                            }

                        } // foreach blockref items
                    } // if
                } // if block item

                lstNewCordinateData = lstResultItems;
                lstNewTextCordinateData = lstTextItems;

            } // foreach block item
            catch { }
        }

        /// <summary>
        /// Extract block begin and block refrence data for adjust the x and y value where ref. added.
        /// </summary>
        public void ExtractLayerBlockBeginAndBlockReferenceData(string sInputFilePath, ref List<DXFData> lstBlock, ref List<DXFData> lstBlockRef)
        {
            try
            {
                List<string> lstLayer = new List<string>();
                //Functions objFunction = new Functions();

                lstBlock = new List<DXFData>();
                lstBlockRef = new List<DXFData>();

                long lineNo = 0;
                using (FileStream fs = File.Open(sInputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BufferedStream bs = new BufferedStream(fs))
                using (StreamReader sr = new StreamReader(bs))
                {
                    string line;
                    bool bEntityStartFound = false, bLayerCode = false;
                    string sCommand = "", sLayerName = "", sBlockName = "", X_Value = "", Y_Value = ""; ;

                    while ((line = sr.ReadLine()) != null)
                    {
                        lineNo++;
                        if (IsSameText(line, DxfLayersName.Entity))
                        {
                            sCommand = "";
                            sLayerName = "";
                            bLayerCode = false;
                            bEntityStartFound = true;
                            sBlockName = "";
                            continue;
                        }
                        else if (line == DxfLayersName.AcdbBlockEndValue || IsSameText(line, DxfLayersName.AcdbBlockEnd.Trim().ToLower()))
                        {
                            bEntityStartFound = false;
                            bLayerCode = false;
                            sCommand = "";
                            sBlockName = "";
                            continue;
                        }
                        else if (bEntityStartFound && IsSameText(line, DxfLayersName.EntityStartCode))
                        {
                            sLayerName = CleanLayerName(sr.ReadLine());
                            lineNo++;
                            bLayerCode = true;
                            bEntityStartFound = false;
                            continue;
                        }
                        else if (bLayerCode && IsSameText(line, DxfLayersName.CommandNameCode))
                        {
                            sCommand = sr.ReadLine().ToLower().Trim();
                            lineNo++;
                            if (IsSameText(sCommand, DxfLayersName.Entity))
                            {
                                sCommand = "";
                                sLayerName = "";
                                bLayerCode = false;
                                bEntityStartFound = true;
                                sBlockName = "";
                            }
                            else if (sCommand == DxfLayersName.AcdbBlockEndValue || IsSameText(sCommand, DxfLayersName.AcdbBlockEnd.Trim().ToLower()))
                            {
                                bEntityStartFound = false;
                                bLayerCode = false;
                                sCommand = "";
                                sBlockName = "";
                            }
                            continue;
                        }
                        else if (bLayerCode && (sCommand.Equals(DxfLayersName.AcdbBlockBegin.Trim().ToLower()) || sCommand.Equals(DxfLayersName.AcdbBlockReference.Trim().ToLower())))
                        {
                            while (true)
                            {
                                if (line == DxfLayersName.BlockNameCodeValue && sBlockName == "")
                                {
                                    sBlockName = sr.ReadLine();
                                    lineNo++;
                                }
                                else if (line == DxfLayersName.LayerPolyLineX_ValueCode)  //else if (line.Trim() == DxfLayersName.LayerPolyLineX_ValueCode)
                                {
                                    X_Value = sr.ReadLine().Trim();
                                    lineNo++;
                                }
                                else if (line == DxfLayersName.LayerPolyLineY_ValueCode) //else if (line.Trim() == DxfLayersName.LayerPolyLineY_ValueCode)
                                {
                                    Y_Value = sr.ReadLine().Trim();
                                    lineNo++;
                                    if (sCommand.Equals(DxfLayersName.AcdbBlockReference.Trim().ToLower()))
                                    {
                                        lstBlockRef.Add(new DXFData { Layer = sLayerName, Name = sBlockName, X = X_Value, Y = Y_Value });
                                    }
                                    else if (sCommand.Equals(DxfLayersName.AcdbBlockBegin.Trim().ToLower()))
                                    {
                                        lstBlock.Add(new DXFData { Layer = sLayerName, Name = sBlockName, X = X_Value, Y = Y_Value });
                                    }
                                    bEntityStartFound = false;
                                    bLayerCode = false;
                                    sCommand = "";
                                    break;
                                }
                                else if (bEntityStartFound && IsSameText(line, DxfLayersName.EntityStartCode))
                                {
                                    bEntityStartFound = false;
                                    bLayerCode = false;
                                    sCommand = "";
                                    sBlockName = "";
                                    break;
                                }
                                else if (line == DxfLayersName.AcdbBlockEndValue || IsSameText(line, DxfLayersName.AcdbBlockEnd.Trim().ToLower()))
                                {
                                    bEntityStartFound = false;
                                    bLayerCode = false;
                                    sCommand = "";
                                    sBlockName = "";
                                    break;
                                }

                                lineNo++;
                                line = sr.ReadLine();

                            }
                            continue;
                        }

                    } //while loop true
                } // using (StreamReader sr = new StreamReader(bs))
            }
            catch { }
        }
        public void SetBulgeElementCoordinate(ref List<LayerCoordinateInfo> lstResult)
        {
            if (lstResult == null || lstResult.Count == 0)
                return;

            for (int i = 0; i < lstResult.Count; i++)
            {
                if (lstResult[i].HasBulge)
                {
                    lstResult[i].Coordinates = new List<Cordinates>();

                    LayerCoordinateInfo reAdjustObject = new LayerCoordinateInfo();
                    List<BulgeItem> lstBulgeItems = lstResult[i].CoordinateWithBulge;

                    //handle last element as bulge value added on 21April2022
                    if (lstBulgeItems != null && lstBulgeItems.Count > 0)
                    {
                        if (lstBulgeItems[lstBulgeItems.Count - 1].IsBulgeValue)
                        {
                            BulgeItem temp = new BulgeItem();
                            temp.IsBulgeValue = false;
                            temp.IsCoordinateValue = true;
                            BulgeItemValue item = new BulgeItemValue();
                            item.Bulge = 0;
                            item.StartPoint = (lstBulgeItems[0].ItemValue as BulgeItemValue).StartPoint;
                            temp.ItemValue = item;

                            lstBulgeItems.Add(temp);
                        }
                    }

                    for (int iCnt = 0; iCnt < lstBulgeItems.Count; iCnt++)
                    {
                        if (lstBulgeItems[iCnt] == null)
                            continue;

                        if (lstBulgeItems[iCnt].IsBulgeValue == false)
                            lstResult[i].Coordinates.Add((lstBulgeItems[iCnt].ItemValue as BulgeItemValue).StartPoint);

                        //added on 28May2022
                        if (lstBulgeItems[iCnt].IsBulgeValue && (iCnt + 1 == lstBulgeItems.Count))
                        {
                            lstBulgeItems[iCnt] = null;
                        }
                        else if (lstBulgeItems[iCnt].IsBulgeValue && (iCnt + 1) < lstBulgeItems.Count)
                        {
                            if (lstBulgeItems[iCnt - 1].IsBulgeValue == false && lstBulgeItems[iCnt + 1].IsBulgeValue == false)
                            {
                                lstBulgeItems[iCnt].ItemValue.StartPoint = (Cordinates)lstBulgeItems[iCnt - 1].ItemValue.StartPoint;
                                lstBulgeItems[iCnt].ItemValue.EndPoint = (Cordinates)lstBulgeItems[iCnt + 1].ItemValue.StartPoint;

                                lstResult[i].Coordinates.Add((lstBulgeItems[iCnt + 1].ItemValue as BulgeItemValue).StartPoint);

                                lstBulgeItems[iCnt - 1] = null;

                                //added condition on 15March2022 bulge found next to next
                                if (iCnt + 2 < lstBulgeItems.Count && !lstBulgeItems[iCnt + 2].IsBulgeValue)
                                    lstBulgeItems[iCnt + 1] = null;

                            } // if condition
                        } //

                    } // for loop

                    //lstResult[i].Coordinates.Add(lstResult[i].Coordinates[0]);

                    lstResult[i].CoordinateWithBulge = lstBulgeItems.Where(x => x != null).ToList();
                } // if condition
            } // for loop
        } // class
        public void SetCircleElementCoordinate(ref List<LayerCoordinateInfo> lstResult)
        {
            if (lstResult == null || lstResult.Count == 0)
                return;

            for (int i = 0; i < lstResult.Count; i++)
            {
                if (lstResult[i].IsCircle)
                {
                    lstResult[i].Coordinates = new List<Cordinates>();

                    //Added on 7Jul2022 it was hard code 0 and 359
                    double StartAngle = 0, EndAngle = 359;
                    if (lstResult[i].StartAngle != lstResult[i].EndAngle) //(lstResult[i].StartAngle != 0d || lstResult[i].EndAngle != 0d)
                    {
                        StartAngle = lstResult[i].StartAngle;
                        EndAngle = lstResult[i].EndAngle;
                        if (EndAngle == 360)
                            EndAngle = 359;
                    }

                    ArcSegment temp = new ArcSegment(lstResult[i].CenterPoint, lstResult[i].Radius, DegreesToRadians(StartAngle), DegreesToRadians(EndAngle));

                    lstResult[i].Coordinates = temp.GetArcPoints();
                    lstResult[i].Coordinates.Add(lstResult[i].Coordinates[0]);

                } // if condition
            } // for loop
        } // class
        //club above two function logic into one 
        public void ProcessReferenceToGetBlockElements(LayerCoordinateInfo ReferenceItem, List<LayerCoordinateInfo> ListBlockList, List<LayerCoordinateInfo> BlockItemsList, List<LayerTextInfo> BlockTextItemsList, ref List<LayerCoordinateInfo> ListElements, ref List<LayerTextInfo> ListTextElements)
        {
            LayerCoordinateInfo objBlockBegin = ListBlockList.Where(x => x.BlockName == ReferenceItem.ReferenceBlockName).FirstOrDefault();
            if (objBlockBegin == null)
                return;

            List<LayerCoordinateInfo> lstBlockElements = BlockItemsList.Where(x => x.BlockName == ReferenceItem.ReferenceBlockName).ToList();
            List<LayerTextInfo> lstBlockTextElements = BlockTextItemsList.Where(x => x.BlockName == ReferenceItem.ReferenceBlockName).ToList();

            List<LayerCoordinateInfo> lstBlockElementsWithReferenceCoordinates = new List<LayerCoordinateInfo>();
            List<LayerTextInfo> lstBlockTextElementsWithReferenceCoordinates = new List<LayerTextInfo>();

            //if (objBlockBegin != null)
            //Added below line on 18Jan2024 to avoid exception where ReferenceItem and ReferenceItem BlockReferenceCoordinate is null
            if (objBlockBegin != null && ReferenceItem != null && ReferenceItem.BlockReferenceCoordinate != null)
            {
                double beginX = objBlockBegin.BlockCoordinate.X;
                double beginY = objBlockBegin.BlockCoordinate.Y;

                double beginRefX = ReferenceItem.BlockReferenceCoordinate.X;
                double beginRefY = ReferenceItem.BlockReferenceCoordinate.Y;

                double diffX = beginRefX - beginX;
                double diffY = beginRefY - beginY;

                double rotateAngle = 0d;
                try
                {
                    rotateAngle = ReferenceItem.ReferenceRotationAngle;
                }
                catch { }

                if (lstBlockTextElements != null && lstBlockTextElements.Count > 0)
                {
                    int iTotalItems = lstBlockTextElements.Count();
                    for (int i = 0; i < iTotalItems; i++)
                    {
                        LayerTextInfo item = lstBlockTextElements[i].DeepClone();
                        item.BlockName = ReferenceItem.ReferenceBlockName; // ReferenceItem.BlockName; // modify 17Apr2023
                        item.LayerName = ReferenceItem.LayerName;  // added 17Apr2023;
                        foreach (Cordinates cord in item.Coordinates)
                        {
                            cord.X = diffX + cord.X * ReferenceItem.XScaling;
                            cord.Y = diffY + cord.Y * ReferenceItem.YScaling;
                        }
                        ListTextElements.Add(item);
                    }
                }

                if (lstBlockElements != null && lstBlockElements.Count > 0)
                {
                    List<LayerCoordinateInfo> lstItems = lstBlockElements.Where(x => x.IsBlockElement == true).ToList();
                    //List<LayerCoordinateInfo> lstReferenceItems = lstBlockElements.Where(x => x.IsBlockReferenceElement == true).ToList();  // modify 17Apr2023;
                    List<LayerCoordinateInfo> lstReferenceItems = lstBlockElements.Where(x => x.IsBlockElement == true).ToList();  // added 17Apr2023;

                    if (lstItems != null && lstItems.Count > 0)
                    {
                        int iTotalItems = lstItems.Count();
                        for (int i = 0; i < iTotalItems; i++)
                        {
                            LayerCoordinateInfo item = lstItems[i].DeepClone();
                            item.BlockName = ReferenceItem.ReferenceBlockName; // ReferenceItem.BlockName; // modify 17Apr2023;
                            item.LayerName = ReferenceItem.LayerName;  // added 17Apr2023;
                            foreach (Cordinates cord in item.Coordinates)
                            {
                                cord.X = diffX + cord.X * ReferenceItem.XScaling;
                                cord.Y = diffY + cord.Y * ReferenceItem.YScaling;
                            }

                            if (item.IsCircle)
                            {
                                item.CenterPoint.X = diffX + item.CenterPoint.X * ReferenceItem.XScaling;
                                item.CenterPoint.Y = diffY + item.CenterPoint.Y * ReferenceItem.YScaling;
                            }

                            if (item.HasBulge)
                            {
                                foreach (BulgeItem bulgeItem in item.CoordinateWithBulge)
                                {
                                    if (bulgeItem.IsBulgeValue)
                                    {
                                        bulgeItem.ItemValue.EndPoint.X = diffX + bulgeItem.ItemValue.EndPoint.X * ReferenceItem.XScaling;
                                        bulgeItem.ItemValue.EndPoint.Y = diffY + bulgeItem.ItemValue.EndPoint.Y * ReferenceItem.YScaling;
                                    }

                                    bulgeItem.ItemValue.StartPoint.X = diffX + bulgeItem.ItemValue.StartPoint.X * ReferenceItem.XScaling;
                                    bulgeItem.ItemValue.StartPoint.Y = diffY + bulgeItem.ItemValue.StartPoint.Y * ReferenceItem.YScaling;

                                }
                            }

                            ListElements.Add(item);
                        }
                    }

                    if (lstReferenceItems != null && lstReferenceItems.Count > 0)
                    {
                        int iTotalItems = lstReferenceItems.Count();
                        for (int i = 0; i < iTotalItems; i++)
                        {
                            LayerCoordinateInfo item = lstReferenceItems[i].DeepClone();

                            List<LayerCoordinateInfo> lstElementsFound = new List<LayerCoordinateInfo>(5);
                            List<LayerTextInfo> lstTextElementsFound = new List<LayerTextInfo>();

                            ProcessReferenceToGetBlockElements(item, ListBlockList, BlockItemsList, BlockTextItemsList, ref lstElementsFound, ref lstTextElementsFound);
                            if (lstElementsFound != null && lstElementsFound.Count > 0)
                            {
                                foreach (LayerCoordinateInfo referenceResolveItem in lstElementsFound)
                                {
                                    referenceResolveItem.BlockName = ReferenceItem.BlockName;
                                    foreach (Cordinates cord in referenceResolveItem.Coordinates)
                                    {
                                        cord.X = diffX + cord.X * ReferenceItem.XScaling;
                                        cord.Y = diffY + cord.Y * ReferenceItem.YScaling;
                                    }
                                }

                                ListElements.AddRange(lstElementsFound);
                            }
                            if (lstTextElementsFound != null && lstTextElementsFound.Count > 0)
                            {
                                foreach (LayerTextInfo referenceResolveItem in lstTextElementsFound)
                                {
                                    referenceResolveItem.BlockName = ReferenceItem.BlockName;
                                    foreach (Cordinates cord in referenceResolveItem.Coordinates)
                                    {
                                        cord.X = diffX + cord.X * ReferenceItem.XScaling;
                                        cord.Y = diffY + cord.Y * ReferenceItem.YScaling;
                                    }
                                }

                                ListTextElements.AddRange(lstTextElementsFound);
                            }
                        }
                    }
                }
            }
        }
        public void ProcessBlockBeginDataToExtractElements(string sBlock, ref List<LayerCoordinateInfo> lstNewItemsBlock, ref List<LayerCoordinateInfo> lstNewItemsBlockItems, ref List<LayerTextInfo> lstNewTextItemsBlockItem)
        {
            string[] arrLines = sBlock.Split(new char[] { '\n' });
            arrLines = arrLines.Select(x => "" + x.Replace("\r", "")).ToArray();
            string sLayerName = "", sCommand = "", sLineType = "", sLineColour = "";
            bool bEntityFound = false;
            string BlockName = "";

            for (int i = 0; i < arrLines.Length; i++)
            {
                string line = arrLines[i];

                if (IsSameText(line, DxfLayersName.Entity))
                {
                    bEntityFound = true;
                    sLayerName = ""; sCommand = ""; sLineType = ""; sLineColour = "";
                }
                else if (IsSameText(line, DxfLayersName.EndBlock) || IsSameText(line, DxfLayersName.AcdbBlockEnd))
                {
                    bEntityFound = false;
                }
                else if (bEntityFound)
                {
                    if (line == DxfLayersName.EntityStartCodeValue)
                    {
                        i++;
                        sLayerName = arrLines[i].Trim();
                        bEntityFound = true;
                    }
                    else if (IsSameText(line, DxfLayersName.EndBlock) || IsSameText(line, DxfLayersName.AcdbBlockEnd) || line == DxfLayersName.AcdbBlockEndValue)
                    {
                        sLayerName = ""; sCommand = ""; sLineType = ""; sLineColour = "";
                        bEntityFound = false;
                    }
                    else if (bEntityFound && line == DxfLayersName.LineTypeCodeValue)
                    {
                        i++;
                        sLineType = arrLines[i].Trim();
                        continue;
                    }
                    else if (bEntityFound && IsSameText(line, DxfLayersName.LineColourCode))
                    {
                        i++;
                        sLineColour = arrLines[i].Trim();
                    }
                    else if (bEntityFound && IsSameText(line, DxfLayersName.LayerCommandCode))
                    {
                        i++;
                        sCommand = arrLines[i].Trim();
                    }
                    else if (bEntityFound && IsSameText(sCommand, DxfLayersName.AcdbBlockBegin))
                    {
                        string sXPoint = "", sYPoint = "";
                        for (int j = i; j < arrLines.Length; j++)
                        {
                            LayerCoordinateInfo obj = new LayerCoordinateInfo();
                            line = arrLines[j];
                            if (line == DxfLayersName.BlockNameCodeValue)
                            {
                                j++;
                                BlockName = arrLines[j].Trim();
                            }
                            else if (line == DxfLayersName.LayerPolyLineX_ValueCode)
                            {
                                j++;
                                sXPoint = arrLines[j].Trim();
                            }
                            else if (line == DxfLayersName.LayerPolyLineY_ValueCode)
                            {
                                try
                                {
                                    j++;
                                    sYPoint = arrLines[j].Trim();
                                    Cordinates objCoordinate = new Cordinates
                                    {
                                        DXFOriginalX = double.Parse(sXPoint, CultureInfo.InvariantCulture),
                                        DXFOriginalY = double.Parse(sYPoint, CultureInfo.InvariantCulture),
                                        X = Math.Round(double.Parse(sXPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint),
                                        Y = Math.Round(double.Parse(sYPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint)
                                    };

                                    obj.Command = sCommand;
                                    obj.BlockName = BlockName;
                                    obj.IsBlockBeginEntry = true;
                                    obj.BlockCoordinate = objCoordinate;
                                    lstNewItemsBlock.Add(obj);
                                    i = j;
                                }
                                catch (Exception ex)
                                {
                                    string s = ex.Message;
                                }
                                break;
                            }
                        } // for loop

                        bEntityFound = false;
                        sCommand = "";
                    }
                    else if (bEntityFound && StartWith(sCommand, DxfLayersName.PolyLine) || StartWith(sCommand, DxfLayersName.Line))
                    {
                        bool bLineCommandFound = false;
                        if (!StartWith(sCommand, DxfLayersName.PolyLine))
                            bLineCommandFound = true; //Line command found

                        string sXPoint = "", sYPoint = "";
                        LayerCoordinateInfo obj = new LayerCoordinateInfo();
                        obj.LayerName = CleanLayerName(sLayerName);
                        obj.ColourCode = sLineColour;
                        obj.Command = sCommand;
                        obj.LineType = sLineType;
                        obj.Coordinates = new List<Cordinates>();
                        obj.BlockName = BlockName;
                        if (!string.IsNullOrEmpty(obj.BlockName))
                            obj.IsBlockElement = true;
                        bool bCoordinateFound = false;

                        for (int j = i; j < arrLines.Length; j++)
                        {
                            line = arrLines[j];
                            if (line == DxfLayersName.AcdbBlockEndValue)
                            {
                                sLayerName = ""; sCommand = ""; sLineType = ""; sLineColour = "";
                                bEntityFound = false;
                                if (bCoordinateFound)
                                    lstNewItemsBlockItems.Add(obj);

                                i = j;
                                break;
                            }
                            else if (line == DxfLayersName.LineTypeCode || line == DxfLayersName.LineTypeCodeValue)
                            {
                                j++;
                                sLineType = arrLines[j].Trim();
                                obj.LineType = sLineType;
                            }
                            else if (line == DxfLayersName.LineColourCode || line == DxfLayersName.LineColourCodeValue1 || line == DxfLayersName.LineColourCodeValue2) //(IsSameText(line, DxfLayersName.LineColourCode))
                            {
                                j++;
                                sLineColour = arrLines[j].Trim();
                                if (!System.Text.RegularExpressions.Regex.IsMatch(sLineColour, "bylayer", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                    obj.ColourCode = sLineColour;
                                else
                                    obj.ColourCode = "";
                            }
                            else if (IsSameText(line, DxfLayersName.LayerPolyLineBulge_ValueCode) && bCoordinateFound) //bulge fixed on 11March2022
                            {
                                j++;
                                line = arrLines[j].Trim();
                                double dBlugeValue = Math.Round(double.Parse(line, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint);

                                if (obj.Coordinates.Count > 0)
                                {
                                    foreach (Cordinates c in obj.Coordinates)
                                        obj.CoordinateWithBulge.Add(new BulgeItem { IsBulgeValue = false, IsCoordinateValue = true, ItemValue = new BulgeItemValue { StartPoint = c } });

                                    //reset all 
                                    obj.Coordinates = new List<Cordinates>();
                                }

                                if (!string.IsNullOrEmpty(obj.BlockName))
                                    obj.IsBlockElement = true;
                                obj.HasBulge = true;
                                obj.OnlyBulgeValue.Add(Math.Round(double.Parse(line, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint));
                                obj.CoordinateWithBulge.Add(new BulgeItem { IsBulgeValue = true, IsCoordinateValue = false, ItemValue = new BulgeItemValue { Bulge = dBlugeValue } });
                            }
                            else if (line == DxfLayersName.LayerPolyLineX_ValueCode || (bLineCommandFound && line == DxfLayersName.LayerLineX_ValueCode)) //(objFunction.IsSameText(line, DxfLayersName.LayerPolyLineX_ValueCode) || (bLineCommandFound && objFunction.IsSameText(line, DxfLayersName.LayerLineX_ValueCode)))
                            {
                                j++;
                                sXPoint = arrLines[j].Trim();
                                bCoordinateFound = true;
                            }
                            else if (line == DxfLayersName.LayerPolyLineY_ValueCode || (bLineCommandFound && line == DxfLayersName.LayerLineY_ValueCode)) //(objFunction.IsSameText(line, DxfLayersName.LayerPolyLineY_ValueCode) || (bLineCommandFound && objFunction.IsSameText(line, DxfLayersName.LayerLineY_ValueCode)))
                            {
                                j++;
                                sYPoint = arrLines[j].Trim();
                                bCoordinateFound = true;
                                if (!string.IsNullOrWhiteSpace(sXPoint))
                                {
                                    Cordinates objCoordinate = new Cordinates
                                    {
                                        DXFOriginalX = double.Parse(sXPoint, CultureInfo.InvariantCulture),
                                        DXFOriginalY = double.Parse(sYPoint, CultureInfo.InvariantCulture),
                                        X = Math.Round(double.Parse(sXPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint),
                                        Y = Math.Round(double.Parse(sYPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint)
                                    };

                                    if (obj.HasBulge)
                                        obj.CoordinateWithBulge.Add(new BulgeItem { IsBulgeValue = false, IsCoordinateValue = true, ItemValue = new BulgeItemValue { StartPoint = objCoordinate } });
                                    else
                                        obj.Coordinates.Add(objCoordinate);
                                }
                                else
                                {
                                    sXPoint = "";
                                    sYPoint = "";
                                }
                            }
                        } //for loop

                        bEntityFound = false;
                        sCommand = "";
                    }
                    else if (bEntityFound && (StartWith(sCommand, DxfLayersName.Text) || StartWith(sCommand, DxfLayersName.MText)))
                    {
                        string sXPoint = "", sYPoint = "";
                        string sTextAlignXPoint = "", sTextAlignYPoint = "";
                        LayerTextInfo obj = new LayerTextInfo();

                        obj.LayerName = CleanLayerName(sLayerName);
                        obj.Command = sCommand;
                        obj.Coordinates = new List<Cordinates>();
                        obj.BlockName = BlockName;
                        obj.ColourCode = sLineColour;

                        bool bCoordinateFound = false;
                        bool bTextAlignCordinateFound = false;
                        bool IsTextCode2Exists = false;
                        for (int j = i; j < arrLines.Length; j++)
                        {
                            line = arrLines[j];
                            string sOriLine = line;

                            //MText 250 char grater remain text in 3 Group Code right now ignoring it
                            if (line == DxfLayersName.LayerTextCodeValue || line == DxfLayersName.LayerTextCode3Value)
                            {
                                if (line == DxfLayersName.LayerTextCodeValue)
                                    IsTextCode2Exists = true;

                                j++;
                                string sOriLineContent = line;
                                line = arrLines[j].Trim();

                                //line = FilterTextInfo(line);
                                bool bFound = false;
                                try
                                {
                                    //20Sept2022 avoid this
                                    bFound = true;
                                    line = ("" + line).Trim();
                                    obj.Text += FilterTextInfo(line).Trim();

                                    if (IsTextCode2Exists)
                                    {
                                        obj.Text = FilterTextInfo(obj.Text);
                                        lstNewTextItemsBlockItem.Add(obj);
                                    }
                                }
                                catch { }
                                if (!bFound)
                                    j--;
                            }
                            else if ((IsSameText(line, DxfLayersName.AcdbBlockEnd) || IsSameText(line, DxfLayersName.EndBlock)))
                            {
                                BlockName = "";
                                if (obj != null)
                                {
                                    obj.Text = FilterTextInfo(obj.Text).Trim();
                                    lstNewTextItemsBlockItem.Add(obj);
                                }
                                bEntityFound = false;
                                j = i;
                                break;
                            }
                            else if (line == DxfLayersName.AcdbBlockEndValue)
                            {
                                if (obj != null)
                                {
                                    obj.Text = FilterTextInfo(obj.Text).Trim();
                                    lstNewTextItemsBlockItem.Add(obj);
                                }
                                bEntityFound = false;
                                j = i;
                                break;
                            }
                            else if (line == DxfLayersName.LayerPolyLineX_ValueCode)
                            {
                                j++;
                                line = arrLines[j].Trim();
                                sXPoint = line.Trim();
                                bCoordinateFound = true;
                            }
                            else if (line == DxfLayersName.LayerPolyLineY_ValueCode)
                            {
                                j++;
                                sYPoint = arrLines[j].Trim();
                                bCoordinateFound = true;
                                if (!string.IsNullOrWhiteSpace(sXPoint))
                                {
                                    obj.Coordinates.Add(new Cordinates
                                    {
                                        DXFOriginalX = double.Parse(sXPoint, CultureInfo.InvariantCulture),
                                        DXFOriginalY = double.Parse(sYPoint, CultureInfo.InvariantCulture),
                                        X = Math.Round(double.Parse(sXPoint), General.NumberOfDecimalPoint),
                                        Y = Math.Round(double.Parse(sYPoint), General.NumberOfDecimalPoint)
                                    });
                                }
                                else
                                {
                                    sXPoint = "";
                                    sYPoint = "";
                                }
                            }
                            else if (bCoordinateFound && IsSameText(line, DxfLayersName.LayerTextAlignX_ValueCode))  // && string.IsNullOrWhiteSpace(obj.Text)
                            {
                                j++;
                                line = arrLines[j];

                                sTextAlignXPoint = line.Trim();
                                bTextAlignCordinateFound = true;
                            }
                            else if (bTextAlignCordinateFound && IsSameText(line, DxfLayersName.LayerTextAlignY_ValueCode))  // && string.IsNullOrWhiteSpace(obj.Text)
                            {
                                j++;
                                line = arrLines[j];

                                sTextAlignYPoint = line.Trim();
                                bTextAlignCordinateFound = true;
                                if (!string.IsNullOrWhiteSpace(sTextAlignXPoint))
                                {
                                    if (obj.TextAlignCoordinates == null)
                                        obj.TextAlignCoordinates = new List<Cordinates>();

                                    //obj.TextAlignCoordinates.Add(new Cordinates
                                    //{
                                    //    DXFOriginalX = double.Parse(sXPoint, CultureInfo.InvariantCulture),
                                    //    DXFOriginalY = double.Parse(sYPoint, CultureInfo.InvariantCulture),
                                    //    X = Math.Round(double.Parse(sTextAlignXPoint), General.NumberOfDecimalPoint),
                                    //    Y = Math.Round(double.Parse(sTextAlignYPoint), General.NumberOfDecimalPoint)
                                    //});
                                }
                                else
                                {
                                    sTextAlignXPoint = "";
                                    sTextAlignYPoint = "";
                                }
                            }

                        } //for loop

                        bEntityFound = false;
                        sCommand = "";
                    }
                    else if (bEntityFound && StartWith(sCommand, DxfLayersName.Circle))
                    {
                        string sXPoint = "", sYPoint = "";
                        LayerCoordinateInfo obj = new LayerCoordinateInfo();

                        obj.LayerName = CleanLayerName(sLayerName);
                        obj.ColourCode = sLineColour;
                        obj.Command = sCommand;
                        obj.LineType = sLineType;
                        obj.Coordinates = new List<Cordinates>();
                        obj.BlockName = BlockName;
                        if (!string.IsNullOrEmpty(obj.BlockName))
                            obj.IsBlockElement = true;

                        bool bCoordinateFound = false;
                        for (int j = i; j < arrLines.Length; j++)
                        {
                            line = arrLines[j];
                            if (line == DxfLayersName.AcdbBlockEndValue)
                            {
                                lstNewItemsBlockItems.Add(obj);
                                break;
                            }
                            if (line == DxfLayersName.LineTypeCodeValue)
                            {
                                j++;
                                sLineType = arrLines[j].Trim();
                                obj.LineType = sLineType;
                            }
                            else if (line == DxfLayersName.LineColourCode || line == DxfLayersName.LineColourCodeValue1 || line == DxfLayersName.LineColourCodeValue2) //(IsSameText(line, DxfLayersName.LineColourCode))
                            {
                                j++;
                                sLineColour = arrLines[j].Trim();
                                if (!System.Text.RegularExpressions.Regex.IsMatch(sLineColour, "bylayer", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                    obj.ColourCode = sLineColour;
                                else
                                    obj.ColourCode = "";
                            }
                            else if (IsSameText(line, DxfLayersName.CircleRadius_ValueCode) && bCoordinateFound)
                            {
                                j++;
                                line = arrLines[j].Trim();
                                obj.Radius = Math.Round(double.Parse(line, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint);
                            }
                            else if (IsSameText(line, DxfLayersName.CircleRadius_StartAngle) && bCoordinateFound) //added on 8April2022
                            {
                                j++;
                                line = arrLines[j].Trim();
                                obj.StartAngle = Math.Round(double.Parse(line, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint);
                            }
                            else if (IsSameText(line, DxfLayersName.CircleRadius_EndAngle) && bCoordinateFound) //added on 8April2022
                            {
                                j++;
                                line = arrLines[j].Trim();
                                obj.EndAngle = Math.Round(double.Parse(line, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint);
                            }
                            else if (line == DxfLayersName.LayerPolyLineX_ValueCode || line == DxfLayersName.LayerLineX_ValueCode)
                            {
                                j++;
                                sXPoint = arrLines[j].Trim();
                                bCoordinateFound = true;
                            }
                            else if (line == DxfLayersName.LayerPolyLineY_ValueCode || line == DxfLayersName.LayerLineY_ValueCode)
                            {
                                j++;
                                sYPoint = arrLines[j].Trim(); ;
                                bCoordinateFound = true;
                                if (!string.IsNullOrWhiteSpace(sXPoint))
                                {
                                    Cordinates objCoordinate = new Cordinates
                                    {
                                        DXFOriginalX = double.Parse(sXPoint, CultureInfo.InvariantCulture),
                                        DXFOriginalY = double.Parse(sYPoint, CultureInfo.InvariantCulture),
                                        X = Math.Round(double.Parse(sXPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint),
                                        Y = Math.Round(double.Parse(sYPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint)
                                    };

                                    obj.Coordinates.Add(objCoordinate);
                                    obj.CenterPoint = objCoordinate;
                                    obj.IsCircle = true;
                                }
                            }
                        } //for loop

                        bEntityFound = false;
                        sCommand = "";
                    }
                    else if (bEntityFound && IsSameText(sCommand, DxfLayersName.AcdbBlockReference))
                    {
                        string sRefBlockName = "";
                        string sXPoint = "", sYPoint = "";
                        for (int j = i; j < arrLines.Length; j++)
                        {
                            LayerCoordinateInfo obj = new LayerCoordinateInfo();
                            line = arrLines[j];
                            if (line == DxfLayersName.BlockNameCodeValue)
                            {
                                j++;
                                sRefBlockName = arrLines[j].Trim();
                            }
                            else if (line == DxfLayersName.LayerPolyLineX_ValueCode)
                            {
                                j++;
                                sXPoint = arrLines[j].Trim();
                            }
                            else if (line == DxfLayersName.LayerPolyLineY_ValueCode)
                            {
                                j++;
                                sYPoint = arrLines[j].Trim();
                                Cordinates objCoordinate = new Cordinates
                                {
                                    DXFOriginalX = double.Parse(sXPoint, CultureInfo.InvariantCulture),
                                    DXFOriginalY = double.Parse(sYPoint, CultureInfo.InvariantCulture),
                                    X = Math.Round(double.Parse(sXPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint),
                                    Y = Math.Round(double.Parse(sYPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint)
                                };

                                obj.BlockName = BlockName;
                                obj.ReferenceBlockName = sRefBlockName;
                                obj.IsBlockReferenceElement = true;
                                obj.BlockReferenceCoordinate = objCoordinate;
                                i = j;
                            }
                            else if (line == DxfLayersName.ReferenceRotationAngle_ValueCode)
                            {
                                double dAngleValue = double.Parse(arrLines[j].Trim());
                                obj.ReferenceRotationAngle = dAngleValue;
                                lstNewItemsBlockItems.Add(obj);
                                i = j;
                                break;
                            }
                        } // for loop

                        bEntityFound = false;
                        sCommand = "";
                    }
                }
            }
        }
        public void ProcessBlockReferenceData(string sBlock, ref List<LayerCoordinateInfo> lstNewItemsBlockRef)
        {
            string[] arrLines = sBlock.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            string sLayerName = "", sCommand = "";
            bool bEntityFound = false;
            //string BlockName = "";

            for (int i = 0; i < arrLines.Length; i++)
            {
                string line = arrLines[i];

                if (IsSameText(line, DxfLayersName.Entity))
                {
                    bEntityFound = true;
                    sLayerName = ""; sCommand = "";
                }
                else if (bEntityFound)
                {
                    if (line == DxfLayersName.EntityStartCodeValue)
                    {
                        i++;
                        sLayerName = CleanLayerName(arrLines[i]).Trim();
                        bEntityFound = true;
                    }
                    else if (IsSameText(line, DxfLayersName.EndBlock) || IsSameText(line, DxfLayersName.AcdbBlockEnd) || line == DxfLayersName.AcdbBlockEndValue)
                    {
                        sLayerName = ""; sCommand = "";
                        bEntityFound = false;
                    }
                    else if (bEntityFound && IsSameText(line, DxfLayersName.LayerCommandCode))
                    {
                        i++;
                        sCommand = arrLines[i].Trim();
                    }
                    else if (bEntityFound && IsSameText(sCommand, DxfLayersName.AcdbBlockReference))
                    {
                        string sRefBlockName = "";
                        string sXPoint = "", sYPoint = "";

                        LayerCoordinateInfo obj = new LayerCoordinateInfo();
                        bool CoordinateFound = false;
                        for (int j = i; j < arrLines.Length; j++)
                        {
                            line = arrLines[j];
                            if (line == DxfLayersName.BlockNameCodeValue)
                            {
                                j++;
                                sRefBlockName = arrLines[j].Trim();
                            }
                            else if (line == DxfLayersName.LayerPolyLineX_ValueCode)
                            {
                                j++;
                                sXPoint = arrLines[j].Trim();
                            }
                            else if (line == DxfLayersName.LayerPolyLineY_ValueCode)
                            {
                                j++;
                                sYPoint = arrLines[j].Trim();
                                Cordinates objCoordinate = new Cordinates
                                {
                                    DXFOriginalX = double.Parse(sXPoint, CultureInfo.InvariantCulture),
                                    DXFOriginalY = double.Parse(sYPoint, CultureInfo.InvariantCulture),
                                    X = Math.Round(double.Parse(sXPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint),
                                    Y = Math.Round(double.Parse(sYPoint, CultureInfo.InvariantCulture), General.NumberOfDecimalPoint)
                                };

                                obj.LayerName = CleanLayerName(sLayerName);
                                obj.ReferenceBlockName = sRefBlockName;
                                obj.IsBlockReferenceElement = true;
                                obj.BlockReferenceCoordinate = objCoordinate;
                                CoordinateFound = true;

                            }
                            else if (line == DxfLayersName.XScaling_ValueCode)
                            {
                                j++;
                                string temp = arrLines[j].Trim();
                                try { obj.XScaling = double.Parse(temp, CultureInfo.InvariantCulture); } catch { }
                            }
                            else if (line == DxfLayersName.YScaling_ValueCode)
                            {
                                j++;
                                string temp = arrLines[j].Trim();
                                try { obj.YScaling = double.Parse(temp, CultureInfo.InvariantCulture); } catch { }
                            }
                            else if (line == DxfLayersName.ZScaling_ValueCode)
                            {
                                j++;
                                string temp = arrLines[j].Trim();
                                try { obj.ZScaling = double.Parse(temp, CultureInfo.InvariantCulture); } catch { }
                            }
                            else if (line == DxfLayersName.ReferenceRotationAngle_ValueCode)
                            {
                                if (CoordinateFound)
                                {
                                    double dAngleValue = Math.Round(double.Parse(arrLines[j].Trim()), 0);
                                    obj.ReferenceRotationAngle = dAngleValue;
                                     lstNewItemsBlockRef.Add(obj);
                                    i = j;
                                    CoordinateFound = false;
                                    break;
                                }
                            }
                            else if (line == DxfLayersName.AcdbBlockEndValue || line == DxfLayersName.EntityEndCode)
                            {
                                if (CoordinateFound)
                                    lstNewItemsBlockRef.Add(obj);
                            }

                        } // for loop

                        sCommand = "";
                    }

                }
            }
        }
        public List<LayerDataWithText> ProcessBindingTextWithCoordinate(List<LayerCoordinateInfo> lstCordinate, List<LayerTextInfo> lstTextInfo)
        {
            // I think new function is ready to use but remain check and confirmation and verify with the drawings. 

            List<LayerDataWithText> lstLayerCoordinateWithText = new List<LayerDataWithText>();

            List<string> lstDistinctCoordinateLayer = lstCordinate.Select(x => x.LayerName).Distinct().ToList();
            List<string> lstDistinctTextLayer = lstTextInfo.Select(x => x.LayerName).Distinct().ToList();

            List<string> lstTextLayerNotInCoordinate = lstDistinctTextLayer.Where(x => !lstDistinctCoordinateLayer.Contains(x)).ToList();
            List<string> lstCoordinateLayerNotInText = lstDistinctCoordinateLayer.Where(x => !lstDistinctTextLayer.Contains(x)).ToList();

            //loop for distinct layer in cordinate found
            foreach (string sLayer in lstDistinctCoordinateLayer.OrderBy(x => x).ToList())
            {
                List<LayerCoordinateInfo> lstCoordinateBlocks = lstCordinate.Where(x => x.LayerName == sLayer).ToList();
                List<LayerTextInfo> lstLayerText = lstTextInfo.Where(x => x.LayerName == sLayer).ToList();

                if (lstLayerText != null && lstCoordinateBlocks != null)
                {
                    //loop for distinct layers cordinate 
                    int iTotal = lstCoordinateBlocks.Count;
                    for (int iCnt = 0; iCnt < iTotal; iCnt++)
                    {
                        LayerCoordinateInfo coordinateInfo = lstCoordinateBlocks[iCnt];
                        //new object initialised
                        LayerDataWithText objLayerWithText = new LayerDataWithText { ColourCode = coordinateInfo.ColourCode, Command = coordinateInfo.Command, Coordinates = coordinateInfo.Coordinates, LineType = coordinateInfo.LineType, HasBulge = coordinateInfo.HasBulge, CoordinateWithBulge = coordinateInfo.CoordinateWithBulge, OnlyBulgeValue = coordinateInfo.OnlyBulgeValue, LayerName = coordinateInfo.LayerName, IsCircle = coordinateInfo.IsCircle };

                        //loop for distinct layers text
                        if (coordinateInfo.Coordinates != null && coordinateInfo.Coordinates.Count > 0)
                        {
                            double dMinX = coordinateInfo.Coordinates.Min(x => x.X);
                            double dMaxX = coordinateInfo.Coordinates.Max(x => x.X);
                            double dMinY = coordinateInfo.Coordinates.Min(x => x.Y);
                            double dMaxY = coordinateInfo.Coordinates.Max(x => x.Y);

                            int iTextCount = lstLayerText.Count;
                            for (int iTextCnt = 0; iTextCnt < iTextCount; iTextCnt++)
                            {
                                LayerTextInfo textInfo = lstLayerText[iTextCnt];
                                if (textInfo.Coordinates != null && textInfo.Coordinates.Count > 0)
                                {
                                    bool bInBetween = true;
                                    Cordinates textCord = textInfo.Coordinates[0];

                                    if (textCord.X < dMinX || textCord.X > dMaxX || textCord.Y < dMinY || textCord.Y > dMaxY)
                                        bInBetween = false;

                                    //added below on 21st April
                                    if (!bInBetween && textInfo.TextAlignCoordinates != null && textInfo.TextAlignCoordinates.Count > 0)
                                    {
                                        bInBetween = true;
                                        Cordinates textAlignCord = textInfo.TextAlignCoordinates[0];
                                        if (textAlignCord.X < dMinX || textAlignCord.X > dMaxX || textAlignCord.Y < dMinY || textAlignCord.Y > dMaxY)
                                            bInBetween = false;
                                    }

                                    if (!bInBetween)
                                        continue;

                                    bool bIsInPoly = false;

                                    //added on 27Jun2022 
                                    if (coordinateInfo.HasBulge)
                                    {
                                        //comment below line because cordinates have both alignment and cordinates both
                                        //bIsInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.Coordinates);

                                        //added logic of loop in 09Jan2024 1230
                                        for (int x1 = 0; x1 < textInfo.Coordinates.Count(); x1++)
                                        {
                                            bIsInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.Coordinates[x1]);
                                            if (bIsInPoly)
                                                break;
                                        }

                                        if (!bIsInPoly)
                                        {
                                            List<Cordinates> lstCords = new List<Cordinates>();
                                            foreach (BulgeItem objItem in coordinateInfo.CoordinateWithBulge)
                                            {
                                                if (objItem.IsBulgeValue)
                                                {
                                                    if (objItem.ItemValue.StartPoint.Equals(objItem.ItemValue.EndPoint) == false)
                                                    {
                                                        ArcSegment arc = new ArcSegment(objItem.ItemValue.StartPoint, objItem.ItemValue.EndPoint, objItem.ItemValue.Bulge);
                                                        lstCords.AddRange(arc.GetArcPoints());
                                                    }
                                                    else
                                                        lstCords.Add(objItem.ItemValue.StartPoint);
                                                }
                                                else
                                                    lstCords.Add(objItem.ItemValue.StartPoint);
                                            }

                                            bIsInPoly = IsInPolyUsingAngle(lstCords, textInfo.Coordinates);
                                        }
                                    }
                                    else
                                    {
                                        //bIsInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.Coordinates);
                                        
                                        //added logic of loop in 09Jan2024 1230
                                        for (int x1 = 0; x1 < textInfo.Coordinates.Count(); x1++)
                                        {
                                            bIsInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.Coordinates[x1]);
                                            if (bIsInPoly)
                                                break;
                                        }
                                    }

                                    //For door and window we need to do it.
                                    bool bTextAlignInPoly = false;
                                    if (!bIsInPoly && textInfo.TextAlignCoordinates != null && textInfo.TextAlignCoordinates.Count > 0)
                                        bTextAlignInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.TextAlignCoordinates);

                                    if (bIsInPoly || bTextAlignInPoly) //coordinate out but alignment in poly
                                        bIsInPoly = true;

                                    if (bIsInPoly && textInfo.ColourCode.Trim() == coordinateInfo.ColourCode.Trim())
                                    {
                                        bool bNotFound = true;

                                        //if already text exist or not if already then avoid duplicate
                                        if (objLayerWithText.TextInfoData != null && objLayerWithText.TextInfoData.Count > 0)
                                        {
                                            //if already text exist or not if already then avoid duplicate
                                            foreach (LayerTextInfo text in objLayerWithText.TextInfoData)
                                            {
                                                //ignore blank text
                                                if (string.IsNullOrWhiteSpace(textInfo.Text))
                                                    continue;

                                                if (text.Text.ToLower() == textInfo.Text.ToLower())
                                                {
                                                    //current textinfo object
                                                    foreach (Cordinates cord1 in text.Coordinates)
                                                    {
                                                        foreach (Cordinates cord2 in objLayerWithText.Coordinates)
                                                        {
                                                            if (cord1.X != cord2.X || cord1.Y != cord2.Y)
                                                            {
                                                                bNotFound = false;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    if (!bNotFound)
                                                        break;
                                                } // if
                                            } // for loop
                                        } // if condition

                                        if (bNotFound)
                                            objLayerWithText.TextInfoData.Add(textInfo);
                                    }
                                }
                            }
                        }

                        if (coordinateInfo.IsCircle)
                        {
                            objLayerWithText.StartAngle = coordinateInfo.StartAngle;
                            objLayerWithText.EndAngle = coordinateInfo.EndAngle;
                            objLayerWithText.Radius = coordinateInfo.Radius;
                            objLayerWithText.CenterPoint = coordinateInfo.CenterPoint;
                        }

                        //remove duplicate string
                        //added on 13Dec2022
                        if (objLayerWithText != null && objLayerWithText.TextInfoData != null)
                        {
                            if (objLayerWithText.TextInfoData.Count > 1)
                            {
                                int iTotText = objLayerWithText.TextInfoData.Count;
                                for (int iC = 0; iC < iTotText; iC++)
                                {
                                    if (string.IsNullOrWhiteSpace(objLayerWithText.TextInfoData[iC].Text))
                                        continue;

                                    for (int jC = 0; jC < iTotText; jC++)
                                    {
                                        if (iC == jC || string.IsNullOrWhiteSpace(objLayerWithText.TextInfoData[jC].Text))
                                            continue;

                                        if (objLayerWithText.TextInfoData[iC].Text.ToLower().Equals(objLayerWithText.TextInfoData[jC].Text.ToLower()))
                                        {
                                            objLayerWithText.TextInfoData[jC].Text = null;
                                        }
                                    }
                                }
                                objLayerWithText.TextInfoData = objLayerWithText.TextInfoData.Where(x => x != null).ToList();
                            }
                        }

                        lstLayerCoordinateWithText.Add(objLayerWithText);

                    } //for each layer cordinate

                } //if 

            }

            return lstLayerCoordinateWithText;
        }
        public List<LayerDataWithText> New_ProcessBindingTextWithCoordinate(List<LayerCoordinateInfo> lstCoordinate, List<LayerTextInfo> lstTextInfo)
        {
            List<LayerDataWithText> lstLayerCoordinateWithText = new List<LayerDataWithText>();

            List<string> lstDistinctCoordinateLayer = lstCoordinate.Where(x => !string.IsNullOrWhiteSpace(x.LayerName) && !CheckLineTypeIsCenterLine(x.LineType)).ToList().Select(x => x.LayerName.Trim().ToLower()).Distinct().ToList();

            //loop for distinct layer in cordinate found
            foreach (string sLayer in lstDistinctCoordinateLayer.OrderBy(x => x).ToList())
            {
                lstTextInfo = lstTextInfo.Where(x => x != null && x.Coordinates != null && x.Coordinates.Count > 0).ToList();

                List<LayerCoordinateInfo> lstCoordinateBlocks = lstCoordinate.Where(x => x.LayerName.Trim().ToLower() == sLayer.Trim().ToLower()).ToList();
                List<LayerTextInfo> lstLayerText = lstTextInfo.Where(x => x.LayerName.Trim().ToLower() == sLayer.Trim().ToLower()).ToList();

                if (lstCoordinateBlocks != null)
                {
                    //loop for distinct layers cordinate 
                    int iTotal = lstCoordinateBlocks.Count;
                    for (int iCnt = 0; iCnt < iTotal; iCnt++)
                    {
                        LayerCoordinateInfo coordinateInfo = lstCoordinateBlocks[iCnt];

                        //new object initialised
                        LayerDataWithText objLayerWithText = new LayerDataWithText { ColourCode = coordinateInfo.ColourCode, Command = coordinateInfo.Command, Coordinates = coordinateInfo.Coordinates, LineType = coordinateInfo.LineType, HasBulge = coordinateInfo.HasBulge, CoordinateWithBulge = coordinateInfo.CoordinateWithBulge, OnlyBulgeValue = coordinateInfo.OnlyBulgeValue, LayerName = coordinateInfo.LayerName, IsCircle = coordinateInfo.IsCircle };

                        if (CheckLineTypeIsCenterLine(coordinateInfo.LineType) || (!coordinateInfo.IsCircle && !coordinateInfo.HasBulge && (coordinateInfo.Coordinates == null || coordinateInfo.Coordinates.Count < 3)))
                        {
                            lstLayerCoordinateWithText.Add(objLayerWithText);
                            continue;
                        }

                        if (coordinateInfo.HasBulge || coordinateInfo.IsCircle)
                            SetAdjustCoordinate(ref coordinateInfo, 0.05);

                        if (coordinateInfo.IsCircle)
                        {
                            objLayerWithText.StartAngle = coordinateInfo.StartAngle;
                            objLayerWithText.EndAngle = coordinateInfo.EndAngle;
                            objLayerWithText.Radius = coordinateInfo.Radius;
                            objLayerWithText.CenterPoint = coordinateInfo.CenterPoint;
                        }

                        lstCoordinateBlocks[iCnt] = coordinateInfo;

                        //loop for distinct layers text
                        if (coordinateInfo.Coordinates != null && coordinateInfo.Coordinates.Count > 0)
                        {
                            if (lstLayerText != null)
                            {
                                //remove null text alignment
                                lstLayerText = lstLayerText.Where(x => x != null && x.Coordinates != null && x.Coordinates.Count > 0).ToList();

                                int iTextCount = lstLayerText.Count;
                                for (int iTextCnt = 0; iTextCnt < iTextCount; iTextCnt++)
                                {
                                    LayerTextInfo textInfo = lstLayerText[iTextCnt].DeepClone();
                                    if (textInfo.Coordinates != null && textInfo.Coordinates.Count > 0)
                                    {
                                        if (textInfo.ColourCode.Trim() != coordinateInfo.ColourCode.Trim())
                                            continue;

                                        bool bIsInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.Coordinates);
                                        if (!bIsInPoly && textInfo.TextAlignCoordinates != null && textInfo.TextAlignCoordinates.Count > 0)
                                            bIsInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.TextAlignCoordinates);

                                        if (bIsInPoly)
                                        {
                                            bool bNotFound = true;

                                            if (objLayerWithText.TextInfoData == null)
                                                objLayerWithText.TextInfoData = new List<LayerTextInfo>();

                                            if (bNotFound)
                                            {
                                                objLayerWithText.TextInfoData.Add(textInfo);
                                                lstLayerText[iTextCnt] = null;
                                            }
                                        }
                                    }

                                }
                            }
                        }

                        lstLayerCoordinateWithText.Add(objLayerWithText);

                    } //for each layer cordinate

                } //if 
            }

            //Now check which element has multiple text assignment extract diff.
            if (lstLayerCoordinateWithText != null && lstLayerCoordinateWithText.Count > 0)
            {
                List<LayerDataWithText> lstItemsHasMultipleTextAssignment = lstLayerCoordinateWithText.Where(x => x.TextInfoData != null && x.TextInfoData.Count > 1).ToList();
                List<LayerDataWithText> lstItemsHasNoTextAssignment = lstLayerCoordinateWithText.Where(x => x.Coordinates != null && (x.TextInfoData == null || x.TextInfoData.Count == 0)).ToList();

                foreach (LayerDataWithText item in lstItemsHasMultipleTextAssignment)
                {
                    if (item.TextInfoData.Count > 1)
                    {
                        int iCount = item.TextInfoData.Count;
                        for (int i = 0; i < iCount; i++)
                        {
                            if (item.TextInfoData[i] == null) continue;

                            LayerTextInfo txt = item.TextInfoData[i];
                            for (int j = i; j < iCount - 1; j++)
                            {
                                if (item.TextInfoData[j] == null) continue;
                                LayerTextInfo txt1 = item.TextInfoData[j];

                                if (txt1 == txt)
                                {
                                    item.TextInfoData[j] = null; //comment
                                    //Added below 2 lines on 09Oct2023 
                                    //item.TextInfoData[i] = null;
                                    //break;
                                }
                            }
                        }

                        item.TextInfoData = item.TextInfoData.Where(x => x != null).ToList();
                        // added below 3 lines on 09Oct2023
                        //item.TextInfoData = item.TextInfoData.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Text)).ToList();
                        //if (item.TextInfoData != null && item.TextInfoData.Count > 1)
                        //    item.TextInfoData = new List<LayerTextInfo> { item.TextInfoData.Last() };
                    }
                }

                lstItemsHasMultipleTextAssignment = lstLayerCoordinateWithText.Where(x => x.TextInfoData != null && x.TextInfoData.Count > 1).ToList();
                lstItemsHasNoTextAssignment = lstLayerCoordinateWithText.Where(x => x.Coordinates != null && (x.TextInfoData == null || x.TextInfoData.Count == 0)).ToList();

                List<LayerTextInfo> lstTextInfoCollection = new List<LayerTextInfo>();
                if (lstItemsHasMultipleTextAssignment != null && lstItemsHasMultipleTextAssignment.Count > 0)
                {
                    int iTotalItem = lstItemsHasMultipleTextAssignment.Count;
                    for (int i = 0; i < iTotalItem; i++)
                    {
                        foreach (LayerTextInfo item in lstItemsHasMultipleTextAssignment[i].TextInfoData)
                            lstTextInfoCollection.Add(item);

                        lstItemsHasMultipleTextAssignment[i].TextInfoData = new List<LayerTextInfo>();
                    }
                }

                lstItemsHasNoTextAssignment.AddRange(lstItemsHasMultipleTextAssignment);
                //loop for distinct layers cordinate 
                int iTotal = lstItemsHasNoTextAssignment.Count;
                for (int iCnt = 0; iCnt < iTotal; iCnt++)
                {
                    LayerDataWithText coordinateInfo = lstItemsHasNoTextAssignment[iCnt];
                    if (string.IsNullOrWhiteSpace(coordinateInfo.LayerName) || coordinateInfo.Coordinates == null || coordinateInfo.Coordinates.Count < 3)
                        continue;

                    lstTextInfoCollection = lstTextInfoCollection.Where(x => x != null && x.Coordinates != null && x.Coordinates.Count > 0).ToList();

                    List<LayerTextInfo> lstLayerText = lstTextInfoCollection.Where(x => x.LayerName.ToLower().Trim() == coordinateInfo.LayerName.ToLower().Trim()).ToList();
                    int iTextCount = lstLayerText.Count;
                    for (int iTextCnt = 0; iTextCnt < iTextCount; iTextCnt++)
                    {
                        LayerTextInfo textInfo = lstTextInfoCollection[iTextCnt].DeepClone();
                        if (textInfo.Coordinates != null && textInfo.Coordinates.Count > 0)
                        {
                            bool bIsInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.Coordinates);
                            if (!bIsInPoly && textInfo.TextAlignCoordinates != null && textInfo.TextAlignCoordinates.Count > 0)
                                bIsInPoly = IsInPolyUsingAngle(coordinateInfo.Coordinates, textInfo.TextAlignCoordinates);

                            if (bIsInPoly && textInfo.ColourCode.Trim() == coordinateInfo.ColourCode.Trim())
                            {
                                if (coordinateInfo.TextInfoData == null)
                                    coordinateInfo.TextInfoData = new List<LayerTextInfo>();

                                coordinateInfo.TextInfoData.Add(textInfo);

                                lstTextInfoCollection[iTextCnt] = null;
                            }
                        }

                    }
                }
            }

            return lstLayerCoordinateWithText;
        }
        private void SetAdjustCoordinate(ref LayerCoordinateInfo itemElement)
        {
            SetAdjustCoordinate(ref itemElement, 1);
        }
        private void SetAdjustCoordinate(ref LayerCoordinateInfo itemElement, double bulgLineWidth)
        {
            if (itemElement != null)
            {
                if (itemElement.HasBulge)
                {
                    List<Cordinates> lstBulgeCord = new List<Cordinates>();
                    foreach (BulgeItem bulgeItem in itemElement.CoordinateWithBulge)
                    {
                        BulgeItemValue bulgeValue = bulgeItem.ItemValue as BulgeItemValue;
                        if (bulgeItem.IsBulgeValue)
                        {
                            if (bulgeValue.StartPoint.X != bulgeValue.EndPoint.X ||
                                bulgeValue.StartPoint.Y != bulgeValue.EndPoint.Y)
                            {
                                ArcSegment arcmy = new ArcSegment(bulgeItem.ItemValue.StartPoint, bulgeItem.ItemValue.EndPoint, bulgeItem.ItemValue.Bulge);
                                lstBulgeCord.AddRange(arcmy.GetArcPoints(bulgLineWidth));
                            }
                        }
                        else
                            lstBulgeCord.Add(bulgeValue.StartPoint);

                    } // for each bulge

                    if (lstBulgeCord != null && lstBulgeCord.Count > 2)
                    {
                        if (!lstBulgeCord.First().Equals(lstBulgeCord.Last()))
                            lstBulgeCord.Add(lstBulgeCord.First());
                    }

                    itemElement.Coordinates = lstBulgeCord;
                }
                else if (itemElement.IsCircle)
                {
                    List<Cordinates> lstBulgeCord = new List<Cordinates>();
                    ArcSegment temp = new ArcSegment(itemElement.CenterPoint, itemElement.Radius, 0, 360);
                    lstBulgeCord.AddRange(temp.GetArcPoints(bulgLineWidth));
                    if (lstBulgeCord != null && lstBulgeCord.Count > 2)
                    {
                        if (!lstBulgeCord.First().Equals(lstBulgeCord.Last()))
                            lstBulgeCord.Add(lstBulgeCord.First());
                    }
                    itemElement.Coordinates = lstBulgeCord;
                }
            }
        }

        public string FilterTextInfo(string sText)
        {
            if (!string.IsNullOrWhiteSpace(sText))
            {
                sText = sText.Replace("\\P", " ");

                sText = sText.Replace("\\L", " ");
                sText = sText.Replace("\\I", "");
                sText = sText.Replace("\\i", "");

                sText = regexClearText.Replace(sText, "");

                sText = sText.Replace("{", "").Replace("}", "");

                //int idx = sText.IndexOf("{");
                //if (idx >= 0)
                //{
                //    if (idx > 0)
                //        sText = sText.Substring(0, idx) + sText.Substring(idx + 1);
                //    else
                //        sText = sText.Substring(idx + 1);

                //    int idx2 = sText.IndexOf(";");
                //    if (idx2 > 0)
                //        sText = sText.Substring(idx2 + 1);

                //    if (sText.IndexOf("}") >= 0)
                //        sText = sText.Substring(0, sText.IndexOf("}"));
                //}

                // \\pxqc", "\pxql", "\pxqr" remove from text
                //if (!string.IsNullOrWhiteSpace(sText))
                //    sText = regexTextOfDataClear.Replace(sText, " ").Trim();

                if (!string.IsNullOrWhiteSpace(sText))
                    sText = regexRemoveSpecialText.Replace(sText, " ").Trim();  //04Jul2022 added

                sText = regexMultipleSpace.Replace(sText, " ").Trim(); //04Jul2022 added

                sText = sText.Trim();
            }


            return sText;
        }

        public List<LayerDataWithText> ExtractLayersData(List<LayerDataWithText> MasterLayersList, string LayerName)
        {
            //Comment during clear code on 07Nov2022

            return MasterLayersList.Where(x => x.LayerName.ToLower().Trim() == LayerName.ToLower().Trim()).ToList();
        }
        public List<LayerInfo> SetLayerInfo(List<LayerDataWithText> LayerData, string LayerKey)
        {
            List<LayerInfo> listData = new List<LayerInfo>();
            foreach (LayerDataWithText data in LayerData)
            {
                LayerInfo obj = new LayerInfo();
                obj.Data = data;
                obj.Key = LayerKey;
                listData.Add(obj);
            }

            return listData;
        }

        //It will make list all child
        public void ExtractChildLayersForParentLayer(List<LayerDataWithText> MasterLayersList, string ChildLayerName, string ChildKeyName, ref List<LayerInfo> objParentLayer)
        {
            ExtractChildLayersForParentLayer(SetLayerInfo(ExtractLayersData(MasterLayersList, ChildLayerName), ChildKeyName), ref objParentLayer);
        }
        public void ExtractChildLayersForParentLayer(List<LayerDataWithText> MasterLayersList, List<LayerDataWithText> LayerData, string ChildKeyName, ref List<LayerInfo> objParentLayer)
        {
            ExtractChildLayersForParentLayer(SetLayerInfo(LayerData, ChildKeyName), ref objParentLayer);
        }
        public void ExtractChildLayersForParentLayer(List<LayerInfo> objChildLayersList, ref List<LayerInfo> objParentLayerList)
        {
            //parent all object
            foreach (LayerInfo parentItem in objParentLayerList)
            {
                if (parentItem.Data.Coordinates != null && parentItem.Data.Coordinates.Count > 0)
                    parentItem.Data.Lines = MakeClosePolyLines(parentItem.Data.Coordinates);

                foreach (LayerInfo childItem in objChildLayersList)
                {
                    if (childItem.Data.Coordinates != null && childItem.Data.Coordinates.Count > 0)
                        childItem.Data.Lines = MakeClosePolyLines(childItem.Data.Coordinates);

                    if (IsInPolyUsingAngle(parentItem.Data.Coordinates, childItem.Data.Coordinates))
                    {
                        if (!parentItem.Child.ContainsKey(childItem.Key))
                            parentItem.Child.Add(childItem.Key, new List<LayerInfo>());

                        parentItem.Child[childItem.Key].Add(childItem);
                    }
                }
            } //for loop
        }
        public void ExtractChildLayersForParentLayer(List<LayerInfo> objChildLayersList, ref LayerInfo objParentLayer)
        {
            if (objParentLayer.Data.Coordinates != null && objParentLayer.Data.Coordinates.Count > 0)
                objParentLayer.Data.Lines = MakeClosePolyLines(objParentLayer.Data.Coordinates);

            foreach (LayerInfo childItem in objChildLayersList)
            {
                if ((childItem.Data.Lines == null || childItem.Data.Lines.Count == 0) && (childItem.Data.Coordinates != null && childItem.Data.Coordinates.Count > 0))
                    childItem.Data.Lines = MakeClosePolyLines(childItem.Data.Coordinates);

                if (IsInPolyUsingAngle(objParentLayer.Data.Coordinates, childItem.Data.Coordinates))
                {
                    if (!objParentLayer.Child.ContainsKey(childItem.Key))
                        objParentLayer.Child.Add(childItem.Key, new List<LayerInfo>());

                    objParentLayer.Child[childItem.Key].Add(childItem);
                }
            }
        }
        public void ExtractChildLayersForParentLayer_old1(List<LayerInfo> objChildLayersList, ref List<LayerInfo> objParentLayerList)
        {
            //parent all object
            foreach (LayerInfo parentItem in objParentLayerList)
            {
                if (parentItem.Data.Coordinates != null && parentItem.Data.Coordinates.Count > 0)
                    parentItem.Data.Lines = MakeClosePolyLines(parentItem.Data.Coordinates);

                foreach (LayerInfo childItem in objChildLayersList)
                {
                    if (childItem.Data.Coordinates != null && childItem.Data.Coordinates.Count > 0)
                        childItem.Data.Lines = MakeClosePolyLines(childItem.Data.Coordinates);

                    if (IsInPolyUsingAngle(parentItem.Data.Coordinates, childItem.Data.Coordinates))
                    {
                        if (!parentItem.Child.ContainsKey(childItem.Key))
                            parentItem.Child.Add(childItem.Key, new List<LayerInfo>());

                        parentItem.Child[childItem.Key].Add(childItem);
                    }
                }
            } //for loop
        }
        public void ExtractChildLayersForParentLayer_old1(List<LayerInfo> objChildLayersList, ref LayerInfo objParentLayer)
        {
            if (objParentLayer.Data.Coordinates != null && objParentLayer.Data.Coordinates.Count > 0)
                objParentLayer.Data.Lines = MakeClosePolyLines(objParentLayer.Data.Coordinates);

            foreach (LayerInfo childItem in objChildLayersList)
            {
                if ((childItem.Data.Lines == null || childItem.Data.Lines.Count == 0) && (childItem.Data.Coordinates != null && childItem.Data.Coordinates.Count > 0))
                    childItem.Data.Lines = MakeClosePolyLines(childItem.Data.Coordinates);

                if (IsInPolyUsingAngle(objParentLayer.Data.Coordinates, childItem.Data.Coordinates))
                {
                    if (!objParentLayer.Child.ContainsKey(childItem.Key))
                        objParentLayer.Child.Add(childItem.Key, new List<LayerInfo>());

                    objParentLayer.Child[childItem.Key].Add(childItem);
                }
            }
        }
        public void ExtractChildLayersForParentLayer(List<LayerDataWithText> MasterLayersList, Dictionary<string, string> dictLayerNameAndKey, ref LayerInfo objParentLayerList)
        {
            //Find master layer list from it
            List<string> lstLayerMasterList = MasterLayersList.Where(x => x != null && string.IsNullOrWhiteSpace(x.LayerName) == false).Select(x => x.LayerName.Trim().ToLower()).Distinct().ToList();

            List<string> lstExistLayerInMaster = dictLayerNameAndKey.Keys.Where(x => lstLayerMasterList.Contains(x.ToLower())).ToList();

            //layer for this parent child 
            //foreach (string key in dictLayerNameAndKey.Keys)
            foreach (string key in lstExistLayerInMaster) 
            { 
                ExtractChildLayersForParentLayer(SetLayerInfo(ExtractLayersData(MasterLayersList, key), dictLayerNameAndKey[key]), ref objParentLayerList);
            } //for loop
        }
        public void ExtractChildLayersForParentLayer(List<LayerDataWithText> MasterLayersList, Dictionary<string, string> dictLayerNameAndKey, ref List<LayerInfo> objParentLayerList)
        {
            if (objParentLayerList == null || objParentLayerList.Count == 0)
                return;

            int iTotal = objParentLayerList.Count;
            for (int iCount = 0; iCount < iTotal; iCount++)
            {
                LayerInfo objLayer = objParentLayerList[iCount];

                ExtractChildLayersForParentLayer(MasterLayersList, dictLayerNameAndKey, ref objLayer);
            }
        }

    }
}
