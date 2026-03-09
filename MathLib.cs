using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EdmontonDrawingValidator.Model;
using SharedClasses;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Text;

namespace EdmontonDrawingValidator
{
    public class MathLib : General
    {
        //Functions objFunction = new Functions();
        //General objGeneral = new General();
        LineOperations objLineOperation = new LineOperations();
        private static NetTopologySuiteUtility objNet_Topology_Suite = new NetTopologySuiteUtility();

        /// <summary>
        ///find polygon area irregular shape
        /// Step 1: Find first and last points X and Y value is same or not if not then add at last
        /// multiply all XPoints with Y points Xi with Y(i+1)
        /// multiply all YPoints with X points Yi with X(i+1)
        /// substract above value X - Y and sum the diff.
        /// diff divided by 2 get area 
        /// return absolute value
        /// </summary>
        /// <param name="lstPoints"></param>
        /// <returns></returns>
        public double FindAreaByCoordinates(List<Cordinates> lstCoordinates)
        {
            double dArea = 0d;
            try
            {
                if (lstCoordinates != null && lstCoordinates.Count > 0)
                {
                    //check first and last element same if not then add 
                    if (lstCoordinates[0].GetDXFOriginalX != lstCoordinates[lstCoordinates.Count - 1].GetDXFOriginalX || lstCoordinates[0].GetDXFOriginalY != lstCoordinates[lstCoordinates.Count - 1].GetDXFOriginalY)
                        lstCoordinates.Add(lstCoordinates[0]);

                    double sum = 0d;
                    for (int i = 0; i < lstCoordinates.Count - 1; i++)
                    {
                        sum += (lstCoordinates[i].GetDXFOriginalX * lstCoordinates[i + 1].GetDXFOriginalY) - (lstCoordinates[i].GetDXFOriginalY * lstCoordinates[i + 1].GetDXFOriginalX);
                    }
                    dArea = sum / 2d;
                }
            }
            catch { }
            return Math.Abs(Math.Round(dArea, General.NumberOfDecimalPoint));
        }

        public double FindAreaByCoordinates(LayerDataWithText area)
        {
            if (area == null || area.Coordinates == null)
                return 0;

            LayerDataWithText areaForCalculation = area.DeepClone();
            if (areaForCalculation.HasBulge)
                SetAdjustCoordinate(ref areaForCalculation, 0.01);

            return FindAreaByCoordinates(areaForCalculation.Coordinates);
        }
        public double FindAreaByCoordinates(LayerInfo area)
        {
            if (area == null || area.Data == null || area.Data.Coordinates == null)
                return 0;

            LayerDataWithText areaForCalculation = area.Data.DeepClone();

            return FindAreaByCoordinates(areaForCalculation);
        }


        /// <summary>
        /// finx Minimum X and maximum X and substract Max(X) - Min(X)
        /// </summary>
        /// <param name="lstCoordinates"></param>
        /// <returns></returns>
        private double FindWidthByCoordinates(List<Cordinates> lstCoordinates)
        {
            double dWidth = 0d;
            try
            {
                if (lstCoordinates != null && lstCoordinates.Count > 0)
                {
                    List<double> lstXAxies = lstCoordinates.Select(x => x.X).ToList();

                    dWidth = lstXAxies.Max() - lstXAxies.Min();
                }
            }
            catch { }
            return Math.Abs(Math.Round(dWidth, General.NumberOfDecimalPoint));
        }

        public double FindWidthByCoordinates(LayerDataWithText layerData)
        {
            return FindWidthByCoordinates(layerData.Coordinates);
        }

        public double FindWidthByCoordinates(LayerInfo area)
        {
            return FindWidthByCoordinates(area.Data.Coordinates);
        }

        /// <summary>
        /// find Minimum Y and maximum Y and substract Max(Y) - Min(Y)
        /// </summary>
        /// <param name="lstCoordinates"></param>
        /// <returns></returns>
        public double FindDepthByCoordinates(List<Cordinates> lstCoordinates)
        {
            double dDepth = 0d;
            try
            {
                if (lstCoordinates != null && lstCoordinates.Count > 0)
                {
                    List<double> lstYAxies = lstCoordinates.Select(x => x.Y).ToList();

                    dDepth = lstYAxies.Max() - lstYAxies.Min();
                }
            }
            catch { }
            return Math.Abs(Math.Round(dDepth, General.NumberOfDecimalPoint));
        }
        public double FindDepthByCoordinates(LayerDataWithText layerData)
        {
            return FindDepthByCoordinates(layerData.Coordinates);
        }
        public double FindDepthByCoordinates(LayerInfo area)
        {
            return FindDepthByCoordinates(area.Data.Coordinates);
        }

        /// <summary>
        /// _mainroad layer apply min(x) and max(x) find difference 
        /// </summary>
        public double _XX_FindFloorHeight(List<Cordinates> lstCoordinates)
        {
            // Console.WriteLine($"Max: {lstCoordinates.Max(x => x.Y)} , Min: {lstCoordinates.Min(x => x.Y)}");
            return Math.Round(lstCoordinates.Max(x => x.Y) - lstCoordinates.Min(x => x.Y), 4);
        }

        public double GetMinimumDistance(CLineSegment line, Cordinates point)
        {
            if (line.IsHorizontal)
                return point.GetDistanceFrom(new Cordinates { Y = line.StartPoint.Y, X = point.X });     // CalculateDistanceBetweenTwoPoints(point, new Coordinates { Y = line.StartPoint.Y, X = point.X });
            else if (line.IsVertical)
                return point.GetDistanceFrom(new Cordinates { X = line.StartPoint.X, Y = point.Y });     //CalculateDistanceBetweenTwoPoints(point, new Coordinates { X = line.StartPoint.X, Y = point.Y }));
            else
            {
                Cordinates cordIntersectionPoint = FindIntersectionPoint(line.StartPoint, line.EndPoint, point);
                return point.GetDistanceFrom(cordIntersectionPoint);    //CalculateDistanceBetweenTwoPoints(point, cordIntersectionPoint));
            }
        }

        private List<Cordinates> ClearPointWhichOnlineOnParent(List<Cordinates> lstParent, List<Cordinates> lstChild)
        {
            List<CLineSegment> parentLines = MakeClosePolyLines(lstParent);
            List<Cordinates> childCoordinate = new List<Cordinates>();
            childCoordinate.AddRange(lstChild);

            return ClearCoordinateWhichOnlineOnParent(parentLines, lstChild, ErrorAllowScale);
        }

        private List<Cordinates> ClearCoordinateWhichOnlineOnParent(List<CLineSegment> parentLines, List<Cordinates> childCoordinate)
        {
            return ClearCoordinateWhichOnlineOnParent(parentLines, childCoordinate, ErrorAllowScale);
        }

        private List<Cordinates> ClearCoordinateWhichOnlineOnParent(List<CLineSegment> parentLines, List<Cordinates> childCoordinate, double ErrorAllowScale)
        {
            if (childCoordinate != null)
                childCoordinate = childCoordinate.Where(x => x != null).ToList();

            int iCount = childCoordinate.Count;
            for (int i = 0; i < iCount; i++)
            {
                foreach (CLineSegment parentLine in parentLines)
                {
                    if (parentLine.IsPointOnLine(childCoordinate[i], ErrorAllowScale)) //here 0.005 is big we needs to make less 
                    {
                        childCoordinate[i] = null;
                        break;
                    }
                }
            }

            //check all are on points found
            return childCoordinate.Where(x => x != null).ToList();
        }
        //private List<Coordinates> ChildPolygonAdjacentMidPoints(List<CLineSegment> parentLines, List<Coordinates> childCoordinate)

        private List<Cordinates> ChildPolygonAdjacentMidPoints(List<Cordinates> childCoordinate)
        {
            int iCount = childCoordinate.Count;
            //List<CLineSegment> lstAdjustanceLines = new List<CLineSegment>();
            List<Cordinates> lstAdjacentMidPoint = new List<Cordinates>();
            for (int i = 0; i < iCount; i++)
            {
                if (i + 2 < iCount)
                    lstAdjacentMidPoint.Add((new CLineSegment { StartPoint = childCoordinate[i], EndPoint = childCoordinate[i + 2] }).MidPoint);
                else
                {
                    Cordinates startCord = childCoordinate[i];
                    Cordinates endCord = null;
                    if (i == iCount - 1 && iCount >= 2) //last cordinate
                        endCord = childCoordinate[1];
                    if (i == iCount - 2 && iCount >= 2) //second last cordinate
                        endCord = childCoordinate[0];

                    if (startCord != null && endCord != null)
                        lstAdjacentMidPoint.Add((new CLineSegment { StartPoint = startCord, EndPoint = endCord }).MidPoint);
                }
            }
            //check all are on points found
            return lstAdjacentMidPoint;
        }
        private bool IsParentAndChildCoordinatesAreSame(List<Cordinates> parentCoordinate, List<Cordinates> childCoordinate, double errorAllow)
        {
            if (parentCoordinate == null || parentCoordinate.Count == 0 || childCoordinate == null || childCoordinate.Count == 0)
                return false;

            int iParentCordsCount = parentCoordinate.Count;
            int iChildCordsCount = childCoordinate.Count;
            if (iParentCordsCount != iChildCordsCount)
                return false;

            Cordinates parentLastCord = parentCoordinate.Last();
            Cordinates childLastCord = childCoordinate.Last();

            for (int i = 0; i < iParentCordsCount; i++)
            {
                iChildCordsCount = childCoordinate.Count;
                for (int j = 0; j < iChildCordsCount; j++)
                {
                    if (parentCoordinate[i].Equals(childCoordinate[j]))
                    {
                        childCoordinate[j] = null;
                        parentCoordinate[i] = null;
                        break;
                    }// below code added on 25March2023
                    else if (errorAllow > 0)
                    {
                        if (Math.Abs(parentCoordinate[i].X - childCoordinate[j].X) < errorAllow && Math.Abs(parentCoordinate[i].Y - childCoordinate[j].Y) < errorAllow)
                        {
                            childCoordinate[j] = null;
                            parentCoordinate[i] = null;
                            break;
                        }
                    }
                }
                childCoordinate = childCoordinate.Where(x => x != null).ToList();
            }

            parentCoordinate = parentCoordinate.Where(x => x != null).ToList();

            if ((childCoordinate == null || childCoordinate.Count == 0) && (parentCoordinate == null || parentCoordinate.Count == 0))
                return true;
            else if ((childCoordinate != null && childCoordinate.Count == 1 && childCoordinate[0].Equals(childLastCord)) &&
                       (parentCoordinate != null && parentCoordinate.Count == 1 && parentCoordinate[0].Equals(parentLastCord))
                   ) //This condition when closed polygon in this case one last point will not match
                return true;

            return false;
        }

        private List<Cordinates> ChildPolygonLongLineMidPoints(List<Cordinates> childCoordinate)
        {
            int iCount = childCoordinate.Count;
            List<Cordinates> lstLineMidPoint = new List<Cordinates>();
            for (int i = 0; i < iCount; i++)
            {
                if (i + 1 < iCount)
                {
                    CLineSegment  line = new CLineSegment { StartPoint = childCoordinate[i], EndPoint = childCoordinate[i + 1] };
                    if(line.Length > 2)
                        lstLineMidPoint.Add(line.MidPoint);
                }
                //else // If failed when center line comes 
                //{
                //    Cordinates startCord = childCoordinate[i];
                //    Cordinates endCord = null;
                //    if (i == iCount - 1 && iCount >= 1) //last coordinate
                //        endCord = childCoordinate[0];

                //    if (startCord != null && endCord != null)
                //    {
                //        CLineSegment line = new CLineSegment { StartPoint = startCord, EndPoint = endCord };
                //        if (line.Length > 2)
                //            lstLineMidPoint.Add(line.MidPoint);
                //    }
                //}
            }
            //check all are on points found
            return lstLineMidPoint;
        }

        private List<Cordinates> ChildPolygonMidPoints(List<Cordinates> childCoordinate)
        {
            int iCount = childCoordinate.Count;
            List<Cordinates> lstLineMidPoint = new List<Cordinates>();
            for (int i = 0; i < iCount; i++)
            {
                if (i + 1 < iCount)
                    lstLineMidPoint.Add((new CLineSegment { StartPoint = childCoordinate[i], EndPoint = childCoordinate[i + 1] }).MidPoint);
                //else
                //{
                //    Cordinates startCord = childCoordinate[i];
                //    Cordinates endCord = null;
                //    if (i == iCount - 1 && iCount >= 1) //last coordinate
                //        endCord = childCoordinate[0];

                //    if (startCord != null && endCord != null)
                //        lstLineMidPoint.Add((new CLineSegment { StartPoint = startCord, EndPoint = endCord }).MidPoint);
                //}
            }
            //check all are on points found
            return lstLineMidPoint;
        }
        public bool IsInPolyUsingAngle(List<Cordinates> lstParent, Cordinates childCord)
        {
            List<Cordinates> childCoordinate = new List<Cordinates>();
            childCoordinate.Add(childCord);

            return CheckAllCoordinatesInParentPolyByAngle(lstParent.ToList(), childCoordinate.ToList(), false);
        }
        public bool IsInPolyUsingAngle(List<Cordinates> lstParent, Cordinates childCord, double ErrorAllowScale)
        {
            List<Cordinates> childCoordinate = new List<Cordinates>();
            childCoordinate.Add(childCord);

            return IsInPolyUsingAngle(lstParent.ToList(), childCoordinate.ToList(), ErrorAllowScale);
        }
        public bool IsInPolyUsingAngle(List<Cordinates> lstParent, List<Cordinates> lstChild)
        {
            return IsInPolyUsingAngle(lstParent, lstChild, General.ErrorAllowScale);
        }
        public bool IsInPolyUsingAngle(List<Cordinates> lstParent1, List<Cordinates> lstChild1, double ErrorAllowScale)
        { 
            List<Cordinates> lstParent= lstParent1.DeepClone();
            List< Cordinates > lstChild= lstChild1.DeepClone();

            //Clear last coordinates are same or duplicate then remove it.
            if (lstParent != null && lstParent.Count > 1)
                lstParent = ClearDuplicateCoordinateFromBottom(lstParent);

            if (lstChild != null && lstChild.Count > 1)
                lstChild = ClearDuplicateCoordinateFromBottom(lstChild);

            List<CLineSegment> parentLines = MakeClosePolyLines(lstParent.ToList());
            List<Cordinates> childCoordinate = lstChild.DeepClone();

            List<Cordinates> OriginalParentCoordinate = lstParent.DeepClone();
            List<Cordinates> OriginalChildCoordinate = lstChild.DeepClone();

            //Add for circle 11April2022
            if (parentLines == null || parentLines.Count == 0)
                return false;

            //Add for midpoint on 14Mar2024 modify on 10Apr2024 if it closed otherwise not add mid point
            if(lstChild.Count() > 10 && lstChild.First().Equals(lstChild.Last()))
                childCoordinate.AddRange(ChildPolygonLongLineMidPoints(lstChild.ToList()));

            childCoordinate = ClearCoordinateWhichOnlineOnParent(parentLines.ToList(), childCoordinate.ToList(), ErrorAllowScale);
            bool bAllCoordinateOnLine = false;
            //Checked all coordinates are found on line
            if (childCoordinate == null || childCoordinate.Count == 0)
            {
                bAllCoordinateOnLine = true;
                childCoordinate = new List<Cordinates>();

                //29Jun2022 get all midpoint and check is all on line or not.
                List<Cordinates> lstMidPoints = ChildPolygonMidPoints(lstChild.ToList());
                childCoordinate = ClearCoordinateWhichOnlineOnParent(parentLines.ToList(), lstMidPoints.ToList(), ErrorAllowScale);

                //Checked all midpoint of coordinate line found on line
                if (childCoordinate == null || childCoordinate.Count == 0)
                {
                    // parent coordinates and child coordinates are same or not if same then both are same.
                    if (IsParentAndChildCoordinatesAreSame(lstParent.ToList(), lstChild.ToList(), ErrorAllowScale))
                    {
                        // Find the Adjacent coordinate line mid point 15Jul2024
                        //return true;
                        //return IsUnionAreaLessOrEqualParentArea(OriginalParentCoordinate, OriginalChildCoordinate);
                        bool? bChecked = IsUnionAreaLessOrEqualParentArea(OriginalParentCoordinate, OriginalChildCoordinate);
                        if (bChecked == null || bChecked.HasValue == false)
                            return true;
                        else
                            return bChecked.Value;
                    }
                    else
                    {
                        childCoordinate = new List<Cordinates>();

                        // Find the Adjacent coordinate line mid point 15Jul2024
                        //List<Coordinates> lstAdjacentMidPoints = ChildPolygonAdjacentMidPoints(parentLines.ToList(), lstChild.ToList());
                        List<Cordinates> lstAdjacentMidPoints = ChildPolygonMidPoints(lstChild.ToList()); // ChildPolygonAdjacentMidPoints(lstChild.ToList());

                        for (int iCnt = 0; iCnt < lstAdjacentMidPoints.Count; iCnt++)
                        {
                            lstAdjacentMidPoints[iCnt].X = FormatFigureInDecimalPoint(lstAdjacentMidPoints[iCnt].X);
                            lstAdjacentMidPoints[iCnt].Y = FormatFigureInDecimalPoint(lstAdjacentMidPoints[iCnt].Y);
                        }

                        // Remove the Adjacent coordinate line mid point which are online to consider is in line   
                        childCoordinate = ClearCoordinateWhichOnlineOnParent(parentLines.ToList(), lstAdjacentMidPoints.ToList(), ErrorAllowScale);

                        if (childCoordinate != null && childCoordinate.Count > 0)
                            childCoordinate = ClearCoordinateWhichOnlineOnParent(parentLines, childCoordinate, ErrorAllowScale);
                        else if (childCoordinate != null && childCoordinate.Count == 0) // 19Mar2025 added for it.
                            return true;
                    }
                }
            }

            if (childCoordinate != null && childCoordinate.Count > 0)
            {
                List<Cordinates> lstRemainChildCoordinate = new List<Cordinates>();
                lstRemainChildCoordinate.AddRange(childCoordinate);
                //return CheckAllCoordinatesInParentPolyByAngle(lstParent.ToList(), lstRemainChildCoordinate, bAllCoordinateOnLine);

                bool bIsIn = CheckAllCoordinatesInParentPolyByAngle(lstParent.ToList(), lstRemainChildCoordinate, bAllCoordinateOnLine);

                if (bIsIn == true)
                {
                    //added checking for 15July2024 condition arrived when union will get failed to handle
                    bool? bChecked = IsUnionAreaLessOrEqualParentArea(OriginalParentCoordinate, OriginalChildCoordinate);
                    if (bChecked == null || bChecked.HasValue == false)
                        return bIsIn;
                    else
                        return bChecked.Value;
                }
                else
                    return false; 
            }
            else
            {
                //return true;
                bool? bChecked = IsUnionAreaLessOrEqualParentArea(OriginalParentCoordinate, OriginalChildCoordinate);
                if (bChecked == null || bChecked.HasValue == false)
                    return false;
                else
                    return bChecked.Value;
            }
        }
         
        public bool? IsUnionAreaLessOrEqualParentArea(List<Cordinates>  lstParent, List<Cordinates> lstChild)
        {
            //return true;
            if (lstChild != null && lstParent != null && lstParent.Count > 3 && lstChild.Count > 3)
            {
                if (lstParent.First().Equals(lstParent.Last()) && lstChild.First().Equals(lstChild.Last()))
                {
                    List<List<Cordinates>> lstData = new List<List<Cordinates>>();
                    lstData.Add(lstParent);
                    lstData.Add(lstChild);
                    //List<Cordinates> UnionCoordinates = objNet_Topology_Suite.GetUnionPolygon(lstData);

                    List<Cordinates> UnionCoordinates = objNet_Topology_Suite.GetUnionPolygonWithoutBuffer(lstData);
                    if (UnionCoordinates == null)
                        return null;

                    double unionArea = FindAreaByCoordinates(UnionCoordinates);
                    double parentArea = FindAreaByCoordinates(lstParent);

                    if (Math.Abs(Math.Round(unionArea,2) - Math.Round(parentArea, 2)) <= 0.5)
                        return true;
                    else
                        return false;
                }
                else
                    return true;
            }
            else
                return true; //because this function call on true condition pass from function IsInPolyUsingAngle
        }

        public bool OLD_IsInPolyUsingAngle(List<Cordinates> lstParent, List<Cordinates> lstChild, double ErrorAllowScale)
        {
            List<CLineSegment> parentLines = MakeClosePolyLines(lstParent.ToList());
            List<Cordinates> childCoordinate = lstChild.ToList();

            //Add for circle 11April2022
            if (parentLines == null || parentLines.Count == 0)
                return false;

            //if (CheckBothPolygonHasSameCoordinates(lstParent, lstChild))
            //return true;

            childCoordinate = ClearCoordinateWhichOnlineOnParent(parentLines.ToList(), childCoordinate.ToList(), ErrorAllowScale);
            bool bAllCoordinateOnLine = false;
            if (childCoordinate == null || childCoordinate.Count == 0)
            {
                bAllCoordinateOnLine = true;
                childCoordinate = new List<Cordinates>();

                // Find the Adjacent coordinate line mid point
                //List<Coordinates> lstAdjacentMidPoints = ChildPolygonAdjacentMidPoints(parentLines.ToList(), lstChild.ToList());
                List<Cordinates> lstAdjacentMidPoints = ChildPolygonAdjacentMidPoints(lstChild.ToList());

                // Remove the Adjacent coordinate line mid point which are online to consider is in line    //childCoordinate = ClearPointWhichOnlineOnParent(parentLines.ToList(), childCoordinate.ToList());
                childCoordinate = ClearCoordinateWhichOnlineOnParent(parentLines.ToList(), lstAdjacentMidPoints.ToList());

                if (childCoordinate != null && childCoordinate.Count > 0)
                    childCoordinate = ClearCoordinateWhichOnlineOnParent(parentLines, childCoordinate);
            }

            if (childCoordinate != null && childCoordinate.Count > 0)
            {
                List<Cordinates> lstRemainChildCoordinate = new List<Cordinates>();
                lstRemainChildCoordinate.AddRange(childCoordinate);
                return CheckAllCoordinatesInParentPolyByAngle(lstParent.ToList(), lstRemainChildCoordinate, bAllCoordinateOnLine);
            }
            else
                return true;
        }

        private bool CheckAllCoordinatesInParentPolyByAngle(List<Cordinates> lstParentCords, List<Cordinates> lstChildCords, bool IsChildAllCoordinateOnLine)
        {
            List<CLineSegment> parentLines = MakeClosePolyLines(lstParentCords);
            foreach (Cordinates childCord in lstChildCords)
            {
                double dTotalDegree = 0d;
                double dTotalDegreeB = 0d;
                List<double> lstDegree = new List<double>();
                foreach (CLineSegment parentLine in parentLines)
                {
                    CLineSegment lineParentStartToChildPoint = new CLineSegment { StartPoint = parentLine.StartPoint, EndPoint = childCord };
                    CLineSegment lineParentEndToChildPoint = new CLineSegment { StartPoint = childCord, EndPoint = parentLine.EndPoint };

                    double distanceParent = parentLine.Length;
                    double distanceParentStartToChild = lineParentStartToChildPoint.Length;
                    double distanceParentEndToChild = lineParentEndToChildPoint.Length;

                    double StartXDiff = parentLine.StartPoint.X - childCord.X;
                    double StartYDiff = parentLine.StartPoint.Y - childCord.Y;
                    double EndXDiff = parentLine.EndPoint.X - childCord.X;
                    double EndYDiff = parentLine.EndPoint.Y - childCord.Y;

                    double crossMultiply = (EndYDiff * StartXDiff) - (EndXDiff * StartYDiff);

                    double dForACOSValue = ((Math.Pow(distanceParentStartToChild, 2) + Math.Pow(distanceParentEndToChild, 2) - Math.Pow(distanceParent, 2)) / (2d * distanceParentStartToChild * distanceParentEndToChild));

                    if (dForACOSValue > 0d)
                    {
                        if (dForACOSValue >= 1 && dForACOSValue <= 2)
                            dForACOSValue = 1;
                    }
                    else if (dForACOSValue < 0d)
                    {
                        if (dForACOSValue >= -1 && dForACOSValue <= -2)
                            dForACOSValue = -1;
                    }

                    double radian = Math.Acos(dForACOSValue);
                    double degree = RadiansToDegrees(radian);

                    dTotalDegreeB += degree;
                    if (crossMultiply < 0)
                        degree = degree * -1;

                    lstDegree.Add(degree);

                    dTotalDegree += degree;
                }

                dTotalDegree = Math.Abs(dTotalDegree);

                if (dTotalDegree < 357d || dTotalDegree > 363d)
                    return false;
                else if (IsChildAllCoordinateOnLine) //This condition move from else part to if on 29Jun2022 
                    return true;
            }

            return true;
        }

        public bool _CheckCoordinateInPolyByAngle(List<Cordinates> lstParentCords, Cordinates childCord)
        {
            List<CLineSegment> parentLines = MakeClosePolyLines(lstParentCords);

            double dTotalDegree = 0d;

            foreach (CLineSegment parentLine in parentLines)
            {

                if (parentLine.IsPointOnLine(childCord, General.ErrorAllowScale))
                    break;

                CLineSegment lineParentStartToChildPoint = new CLineSegment { StartPoint = parentLine.StartPoint, EndPoint = childCord };
                CLineSegment lineParentEndToChildPoint = new CLineSegment { StartPoint = childCord, EndPoint = parentLine.EndPoint };

                double distanceParent = parentLine.Length;
                double distanceParentStartToChild = lineParentStartToChildPoint.Length;
                double distanceParentEndToChild = lineParentEndToChildPoint.Length;

                double StartXDiff = parentLine.StartPoint.X - childCord.X;
                double StartYDiff = parentLine.StartPoint.Y - childCord.Y;
                double EndXDiff = parentLine.EndPoint.X - childCord.X;
                double EndYDiff = parentLine.EndPoint.Y - childCord.Y;

                double crossMultiply = (EndYDiff * StartXDiff) - (EndXDiff * StartYDiff);

                double dFromACOSValue = (Math.Pow(distanceParentStartToChild, 2) + Math.Pow(distanceParentEndToChild, 2) - Math.Pow(distanceParent, 2)) / (2d * distanceParentStartToChild * distanceParentEndToChild);

                if (dFromACOSValue > 0d)
                {
                    if (dFromACOSValue >= 1 && dFromACOSValue <= 2)
                        dFromACOSValue = 1;
                }
                else if (dFromACOSValue < 0d)
                {
                    if (dFromACOSValue >= -1 && dFromACOSValue <= -2)
                        dFromACOSValue = -1;
                }

                double radian = Math.Acos(dFromACOSValue);
                double degree = RadiansToDegrees(radian);

                if (crossMultiply < 0)
                    degree = degree * -1;

                dTotalDegree += degree;
            }

            dTotalDegree = Math.Abs(dTotalDegree);

            if (dTotalDegree >= 357d && dTotalDegree <= 363d)
                return true;

            return false;
        }

        public bool CheckBothPolygonHasSameCoordinates(List<Cordinates> lstParent, List<Cordinates> lstChild)
        {
            List<CLineSegment> parentLines = MakeClosePolyLines(lstParent.ToList());
            List<Cordinates> childCoordinate = lstChild.ToList();

            if ((parentLines == null && childCoordinate == null) || (lstParent.Count == 0 || childCoordinate.Count == 0))
                return true;

            if (parentLines != null && childCoordinate != null && lstParent.Count != childCoordinate.Count)
                return false;

            bool bothSame = true;
            foreach (Cordinates cordChild in lstChild)
            {
                bool bFound = false;
                foreach (Cordinates cordParent in lstParent)
                {
                    if (cordChild.Equals(cordParent, ErrorAllowScale))
                    {
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    bothSame = false;
                    break;
                }
            }

            return bothSame;
        }

        public bool _CheckChildPolygonCoordinateInParentPolygonCoordinates(List<Cordinates> lstParent, List<Cordinates> lstChild)
        {
            List<CLineSegment> parentLines = MakeClosePolyLines(lstParent.ToList());
            List<Cordinates> childCoordinate = lstChild.ToList();

            if ((parentLines == null && childCoordinate == null) || (parentLines.Count == 0 || childCoordinate.Count == 0))
                return true;

            if (parentLines != null && childCoordinate != null && parentLines.Count != childCoordinate.Count)
                return false;

            bool bothSame = true;
            foreach (Cordinates cordChild in lstChild)
            {
                bool bFound = false;
                foreach (Cordinates cordParent in lstParent)
                {
                    if (cordParent.Equals(cordChild, ErrorAllowScale))
                    {
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    bothSame = false;
                    break;
                }
            }

            return bothSame;
        }


        /// <summary>
        /// Step 1: first two cordinate add at bottom order to index first then second 
        /// 
        /// Step 2: prepare cordinate list called ListA
        /// X = X2 - X1 
        /// Y = Y2 - Y1
        /// 
        /// Step 3 : prepare cordinate list called ListB
        /// X = X3 - X2 
        /// Y = Y3 - Y2
        /// 
        /// prepare list of Theta for ListA called ThetaA
        /// ATAN2(ListA.Y, ListA.X) * (180/3.14)
        /// 
        /// prepare list of Theta for ListB called ThetaB
        /// ATAN2(ListB.Y, ListB.X) * (180/3.14)
        /// 
        /// Find intermediate angle for ThetaA prepare list of IntermediateAngleA
        /// 180 + ThetaA(1) - TheataB(1) + 360
        /// 
        /// Find intermediate angle for ThetaB prepare list of IntermediateAngleB
        /// 180 + ThetaB(1) - TheataA(1) + 360
        /// 
        /// prepare inner angle InnerAngle
        /// Loop IntermediateAngleA if angle value grater than 360 then divide till angle became less then or equal 360
        /// 
        /// prepare outer angle OuterAngle
        /// Loop IntermediateAngleB if angle value grater than 360 then divide till angle became less then or equal 360
        /// 
        /// Sum of inner agnle array element count equal to input array has cordingate count (note: not modified array count which has two element added)
        /// Sum of outer agnle array element count equal to input array has cordingate count (note: not modified array count which has two element added)
        /// 
        /// Find the correct anlge list using below formula
        /// 
        /// compareValue = (no of coridates - 2) * 180
        /// 
        /// which sum of angle equal to compareValue that list will return.
        /// 
        /// </summary>
        /// <param name="polygonCoordinate"></param>

        /// <summary>
        /// find the length from given cordinate loop 
        /// add first element at end of the list
        /// X2 to X1
        /// reutrn list of length
        /// </summary>
        /// <param name="polygonCoordinate"></param>
        /// <param name="lstResult"></param>
        public List<double> FindLengthOfEachSize(LayerInfo polygonCoordinate)
        {
            List<double> lstLengths = new List<double>();
            if (polygonCoordinate != null && polygonCoordinate.Data != null && polygonCoordinate.Data.Coordinates != null && polygonCoordinate.Data.Coordinates.Count > 1)
            {
                List<Cordinates> lstCoordinate = polygonCoordinate.Data.Coordinates.DeepClone();

                string temp = "";
                List<CLineSegment> lstLines = MakeClosePolyLines(lstCoordinate);
                lstLines = objLineOperation.MergePolyLineSegments(lstLines, ref temp);
                foreach (CLineSegment line in lstLines)
                {
                    lstLengths.Add(FormatFigureInDecimalPoint(line.Length));
                }
            }

            return lstLengths;
        }
        public double TotalLengthFromCoordinates(List<Cordinates> inputCoordinate)
        {
            double dTotalLength = 0d;

            List<Cordinates> lstCoordinate = new List<Cordinates>();

            lstCoordinate.AddRange(inputCoordinate);

            for (int i = 1; i < lstCoordinate.Count; i++)
                dTotalLength += lstCoordinate[i].GetDistanceFrom(lstCoordinate[i - 1]);    //CalculateDistanceBetweenTwoPoints(lstCoordinate[i], lstCoordinate[i - 1]);

            return FormatFigureInDecimalPoint(dTotalLength);
        }

        /// <summary>
        /// Function angle which has grater tha 360
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double GetAngleBelow360(double value)
        {
            while (value > 360d)
                value = value - 360d;

            return Math.Round(value, General.NumberOfDecimalPoint);

            //return value;
        }

        /// <summary>
        ///  if it infinity then it return isInfinity
        /// </summary>
        /// <param name="coordinateA"></param>
        /// <param name="coordinateB"></param>
        /// <returns></returns>
        public double CalculateSlope(Cordinates coordinateA, Cordinates coordinateB, ref bool isInfinity)
        {
            double slope = 0d;
            isInfinity = false;

            //it mean infinitive
            if (coordinateA.X == coordinateB.X)
            {
                isInfinity = true;
                return 0d;
            }
            else if (coordinateA.Y == coordinateB.Y)
            {
                return 0d;
            }

            slope = Math.Round((coordinateB.Y - coordinateA.Y) / (coordinateB.X - coordinateA.X), General.NumberOfDecimalPoint);

            return slope;
        }

        /// <summary>
        /// calculate Slope for line AB formula m = (y2-y1)/(x2-x1)
        /// if Slope isInfinitive consider intersection point (Ax,Py)
        /// if Slope = 0 then intersection point (Px, Ay)
        /// find m' perpendicular value according AB line for point P, formula m' = 1/ m
        /// find C (consider A) formula c = y - mx
        /// find C' (consider P) formula c' = Py - m'Px
        /// apply formula mx - y + c = 0 for line (AB)
        /// a1 = m , b1 = -1, c1 = c
        /// apply formula m'x - y + c = 0 for line (AB)
        ///  a2 = m' , b2 = -1, c2 = c'
        /// calculate x' = ((b1*c2)-(b2*c1))/((a1*b2)-(a2*b1))
        /// calculate y' = ((a2*c1)-(a1*c2))/((a1*b2)-(a2*b1))
        /// return IntersectionPoint (x', y')
        /// </summary>
        /// <param name="coordinateA"></param>
        /// <param name="coordinateB"></param>
        /// <param name="coordinateP"></param>
        /// <returns></returns>
        public Cordinates FindIntersectionPoint(Cordinates coordinateA, Cordinates coordinateB, Cordinates coordinateP)
        {
            Cordinates coordinateIntersectionPoint = new Cordinates();

            //Slope for line AB
            bool isInfinity = false;
            double Slope = CalculateSlope(coordinateA, coordinateB, ref isInfinity);

            if (isInfinity) // if Slope isInfinitive consider intersection point (Ax,Py)
                coordinateIntersectionPoint = new Cordinates { X = coordinateA.X, Y = coordinateP.Y };
            else if (Slope == 0)  // if Slope = 0 then intersection point (Px, Ay)
                coordinateIntersectionPoint = new Cordinates { X = coordinateP.X, Y = coordinateA.Y };
            else
            {
                double m = Slope;
                // find m' perpendicular value according AB line for point P, formula m' = 1/ m

                double mDash = FormatFigureInDecimalPoint(-1d / m);

                // find C (consider A) formula c = y - mx
                double c = FormatFigureInDecimalPoint(coordinateA.Y - (m * coordinateA.X));

                // find C' (consider P) formula c' = Py - m'Px
                double cDash = FormatFigureInDecimalPoint(coordinateP.Y - (mDash * coordinateP.X));

                // consider formula mx - y + c = 0 for line (AB)
                // a1 = m , b1 = -1, c1 = c

                // consider formula m'x - y + c = 0 for line (AB)
                //  a2 = m' , b2 = -1, c2 = c'

                // calculate x' = ((b1*c2)-(b2*c1))/((a1*b2)-(a2*b1)) //ref above two lines
                double IntersectionPointX = FormatFigureInDecimalPoint(((-1 * cDash) - (-1 * c)) / ((m * -1) - (mDash * -1)));

                // calculate y' = ((a2*c1)-(a1*c2))/((a1*b2)-(a2*b1))
                double IntersectionPointY = FormatFigureInDecimalPoint(((mDash * c) - (m * cDash)) / ((m * -1) - (mDash * -1)));

                // return IntersectionPoint (x', y')
                coordinateIntersectionPoint = new Cordinates { X = IntersectionPointX, Y = IntersectionPointY };
            }

            return coordinateIntersectionPoint;
        }
        public Cordinates FindIntersectionPoint(CLineSegment line, Cordinates coordinateP)
        {
            return FindIntersectionPoint(line.StartPoint, line.EndPoint, coordinateP);
        }

        /// <summary>
        /// find mid point
        /// </summary>
        /// <param name="cordinateA"></param>
        /// <param name="cordinateB"></param>
        /// <returns></returns>

        public List<CLineSegment> ConvertCoordinateToLineWithSortAndSwap(List<Cordinates> lstCords)
        {
            lstCords = lstCords.OrderBy(x => x.X).ToList();
            List<CLineSegment> lstLines = new List<CLineSegment>();
            lstLines = MakeConnectedLines(lstCords); // 08Nov2022 clean code

            lstLines = SetLineSegmentXYSwap(lstLines);

            return lstLines;
        }

        public List<CLineSegment> SetLineSegmentXYSwap(List<CLineSegment> lstLines)
        {
            if (lstLines == null || lstLines.Count == 0)
                return lstLines;

            lstLines = lstLines.OrderBy(x => x.StartPoint.X).ToList();
            List<CLineSegment> lstStairLinesSortedSwap = new List<CLineSegment>();

            lstStairLinesSortedSwap.Add(lstLines[0]);
            for (int i = 1; i < lstLines.Count; i++)
            {
                double distanceWithStart = lstLines[0].StartPoint.GetDistanceFrom(lstLines[i].StartPoint);
                double distanceWithEnd = lstLines[0].StartPoint.GetDistanceFrom(lstLines[i].EndPoint);

                if (distanceWithEnd < distanceWithStart)
                {
                    Cordinates temp = lstLines[i].EndPoint;
                    lstLines[i].EndPoint = lstLines[i].StartPoint;
                    lstLines[i].StartPoint = temp;
                }
            }

            return lstLines;
        }


        /// <summary>
        /// This is for CENTER line which is not polygon but it connected multiple points.
        /// </summary>
        /// <param name="cords"></param>
        /// <returns></returns>
        public List<CLineSegment> MakeConnectedLines(List<Cordinates> cords)
        {
            if (cords == null || cords.Count < 2)
                return null;

            List<CLineSegment> lstLines = new List<CLineSegment>();
            for (int i = 0; i < cords.Count - 1; i++)
            {
                lstLines.Add(new CLineSegment
                {
                    StartPoint = new Cordinates
                    {
                        X = cords[i].X,
                        Y = cords[i].Y
                    },
                    EndPoint = new Cordinates
                    {
                        X = cords[i + 1].X,
                        Y = cords[i + 1].Y
                    }
                });
            }

            return lstLines;

        }
        public List<CLineSegment> MakeClosePolyLines(List<Cordinates> cords)
        {
            if (cords == null || cords.Count < 2)
                return null;

            List<Cordinates> TempCords = cords.DeepClone();
            List<CLineSegment> lstLines = new List<CLineSegment>();
            if (TempCords[0].X != TempCords[cords.Count - 1].X || TempCords[0].Y != TempCords[cords.Count - 1].Y)
                TempCords.Add(TempCords[0]);

            for (int i = 0; i < TempCords.Count - 1; i++)
            {
                lstLines.Add(new CLineSegment
                {
                    StartPoint = new Cordinates
                    {
                        X = TempCords[i].X,
                        Y = TempCords[i].Y
                    },
                    EndPoint = new Cordinates
                    {
                        X = TempCords[i + 1].X,
                        Y = TempCords[i + 1].Y
                    }
                });
            }
            return lstLines;
        }

        public double? FindConnectedLineAtMinimumDistanceBetweenTwoArea(List<CLineSegment> FirstAreaLine, List<CLineSegment> SecondArea, ref CLineSegment FirstAreaLineIntersect, ref CLineSegment SecondAreaLineWhereIntersect)
        {
            double? dDistance = null;
            FirstAreaLineIntersect = null;
            SecondAreaLineWhereIntersect = null;

            foreach (CLineSegment firstAreaLine in FirstAreaLine.Where(x => x.IsHorizontal))
            {
                foreach (CLineSegment SecondAreaLine in SecondArea.Where(x => x.IsHorizontal))
                {
                    Cordinates SecondAreaLineMidPoint = SecondAreaLine.MidPoint;
                    Cordinates cordIntersectPoint = FindIntersectionPoint(firstAreaLine.StartPoint, firstAreaLine.EndPoint, SecondAreaLineMidPoint);
                    int iTry = 1;
                    while (iTry > 0)
                    {
                        if (firstAreaLine.IsPointOnLine(cordIntersectPoint, General.ErrorAllowScale) || SecondAreaLine.IsPointOnLine(cordIntersectPoint, General.ErrorAllowScale))
                        {
                            double distance = objLineOperation.GetSortestDistanceBetweenTwoLine(firstAreaLine, SecondAreaLine); // CalculateSortestDistance(firstAreaLine, SectondAreaLineMidPoint);
                            if (dDistance == null || distance < dDistance)
                            {
                                dDistance = distance;
                                FirstAreaLineIntersect = firstAreaLine;
                                SecondAreaLineWhereIntersect = SecondAreaLine;
                                break;
                            }
                        } // if point on line
                        else //try to alternate
                        {
                            SecondAreaLineMidPoint = firstAreaLine.MidPoint;
                            cordIntersectPoint = FindIntersectionPoint(SecondAreaLine.StartPoint, SecondAreaLine.EndPoint, firstAreaLine.MidPoint);
                        }
                        iTry--;
                    }
                }
            }
            return dDistance;
        }
        public double? FindLineAtMinimumDistanceWithReferenceLine(List<CLineSegment> FirstAreaLine, CLineSegment FromLine, ref CLineSegment FirstAreaLineIntersect)
        {
            //11 March
            if (FromLine == null || FirstAreaLine == null)
                return null;

            double? dDistance = null;
            FirstAreaLineIntersect = null;
            foreach (CLineSegment firstAreaLine in FirstAreaLine.Where(x => x.IsHorizontal))
            {
                Cordinates FromLineMidPoint = firstAreaLine.MidPoint;
                Cordinates cordIntersectPoint = FindIntersectionPoint(FromLine.StartPoint, FromLine.EndPoint, FromLineMidPoint);
                if (firstAreaLine.IsPointOnLine(cordIntersectPoint, General.ErrorAllowScale))
                {
                    // find distance
                    double distance = GetMinimumDistance(firstAreaLine, FromLineMidPoint);
                    if (dDistance == null || distance < dDistance)
                    {
                        dDistance = distance;
                        FirstAreaLineIntersect = firstAreaLine;
                    }
                } // if point on line
            }
            return dDistance;
        }

        public double? FindLineAtMinimumDistanceWithAllLines(List<CLineSegment> FirstAreaLine, CLineSegment FromLine, ref CLineSegment FirstAreaLineIntersect)
        {
            //11 March
            if (FromLine == null || FirstAreaLine == null)
                return null;

            double? dDistance = null;
            FirstAreaLineIntersect = null;
            foreach (CLineSegment firstAreaLine in FirstAreaLine)
            {
                Cordinates FromLineMidPoint = firstAreaLine.MidPoint;
                Cordinates cordIntersectPoint = FindIntersectionPoint(FromLine.StartPoint, FromLine.EndPoint, FromLineMidPoint);
                if (firstAreaLine.IsPointOnLine(cordIntersectPoint, General.ErrorAllowScale))
                {
                    // find distance
                    double distance = GetMinimumDistance(firstAreaLine, FromLineMidPoint);
                    if (dDistance == null || distance < dDistance)
                    {
                        dDistance = distance;
                        FirstAreaLineIntersect = firstAreaLine;
                    }
                } // if point on line
            }
            return dDistance;
        }

        public double? FindMinimumDistanceBetweenAreaAndLobby(List<CLineSegment> lstAreaLine, List<CLineSegment> lstLobbyLine, ref CLineSegment areaLineIntersect, ref CLineSegment lobbyLineWhereIntersect)
        {
            double? dDistance = null;
            areaLineIntersect = null;
            lobbyLineWhereIntersect = null;

            foreach (CLineSegment areaLine in lstAreaLine)
            {
                foreach (CLineSegment lobbyLine in lstLobbyLine)
                {
                    Cordinates lobbyLineMidPoint = lobbyLine.MidPoint;
                    Cordinates cordIntersectPoint = FindIntersectionPoint(areaLine.StartPoint, areaLine.EndPoint, lobbyLineMidPoint);

                    if (areaLine.IsPointOnLine(cordIntersectPoint, General.ErrorAllowScale))
                    {
                        // find distance
                        double distance = GetMinimumDistance(areaLine, lobbyLineMidPoint);

                        if (dDistance == null || distance < dDistance)
                        {
                            dDistance = distance;
                            areaLineIntersect = areaLine;
                            lobbyLineWhereIntersect = lobbyLine;
                        }

                    } // if point on line
                }
            }
            return dDistance;
        }
        public double? FindMinimumDistanceBetweenAndGetConnectedLines(List<CLineSegment> lstParentLines, List<CLineSegment> lstChildLines, ref CLineSegment parentLineIntersect, ref CLineSegment childLineWhereIntersect)
        {
            double? dDistance = null;
            parentLineIntersect = null;
            childLineWhereIntersect = null;

            foreach (CLineSegment parentLine in lstParentLines)
            {
                foreach (CLineSegment childLine in lstChildLines)
                {
                    if (
                            (parentLine.IsPointOnLine(childLine.StartPoint, ErrorAllowScale) && parentLine.IsPointOnLine(childLine.EndPoint, ErrorAllowScale)) ||
                            (childLine.IsPointOnLine(parentLine.StartPoint, ErrorAllowScale) && childLine.IsPointOnLine(parentLine.EndPoint, ErrorAllowScale))
                        )
                    {        // find distance
                        double distance = objLineOperation.GetSortestDistanceBetweenTwoLine(parentLine, childLine);
                        if (dDistance == null || distance < dDistance)
                        {
                            dDistance = distance;
                            parentLineIntersect = parentLine;
                            childLineWhereIntersect = childLine;
                        }
                    }
                    else
                    {
                        Cordinates childLineMidPoint = childLine.MidPoint;
                        Cordinates cordIntersectPoint = FindIntersectionPoint(parentLine.StartPoint, parentLine.EndPoint, childLineMidPoint);
                        if (parentLine.IsPointOnLine(cordIntersectPoint, General.ErrorAllowScale))
                        {
                            // find distance
                            double distance = GetMinimumDistance(parentLine, childLineMidPoint);

                            if (dDistance == null || distance < dDistance)
                            {
                                dDistance = distance;
                                parentLineIntersect = parentLine;
                                childLineWhereIntersect = childLine;
                            }

                        } // if point on line
                    }
                }
            }
            return dDistance;
        }

        public void CalculateBeamHeight(List<CLineSegment> FirstAreaLine, CLineSegment FromLine, ref CLineSegment LineIntersectWithMinDistance, ref CLineSegment LineIntersectWithMaxDistance, ref double? minDistance, ref double? maxDistance)
        {
            //11 March
            if (FromLine == null || FirstAreaLine == null)
                return;

            minDistance = null;
            maxDistance = null;
            LineIntersectWithMinDistance = null;
            LineIntersectWithMaxDistance = null;
            foreach (CLineSegment firstAreaLine in FirstAreaLine.Where(x => x.IsHorizontal))
            {
                double distance = objLineOperation.GetSortestDistanceFromLine(FromLine, firstAreaLine.MidPoint);
                if (distance == -1)
                    continue;

                if (maxDistance == null || distance > maxDistance.Value)
                {
                    maxDistance = distance;
                    LineIntersectWithMaxDistance = firstAreaLine;
                }
                if (minDistance == null || distance < minDistance.Value)
                {
                    minDistance = distance;
                    LineIntersectWithMinDistance = firstAreaLine;
                }
            }
            //return dDistance;
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        // This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public double DegreesToRadians(double degree)
        {
            return (degree * Math.PI / 180.0d);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        // This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public double RadiansToDegrees(double Radians)
        {
            return (Radians / Math.PI * 180.0d);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //:::                                                                         :::
        //:::  This routine calculates the distance between two points (given the     :::
        //:::  latitude/longitude of those points). It is being used to calculate     :::
        //:::  the distance between two locations using GeoDataSource(TM) products    :::
        //:::                                                                         :::
        //:::  Definitions:                                                           :::
        //:::    South latitudes are negative, east longitudes are positive           :::
        //:::                                                                         :::
        //:::  Passed to function:                                                    :::
        //:::    lat1, lon1 = Latitude and Longitude of point 1 (in decimal degrees)  :::
        //:::    lat2, lon2 = Latitude and Longitude of point 2 (in decimal degrees)  :::
        //:::    unit = the unit you desire for results                               :::
        //:::           where: 'M' is statute miles (default)                         :::
        //:::                  'K' is kilometers                                      :::
        //:::                  'N' is nautical miles                                  :::
        //:::                                                                         :::
        //:::  Worldwide cities and other features databases with latitude longitude  :::
        //:::  are available at https://www.geodatasource.com                         :::
        //:::                                                                         :::
        //:::  For enquiries, please contact sales@geodatasource.com                  :::
        //:::                                                                         :::
        //:::  Official Web site: https://www.geodatasource.com                       :::
        //:::                                                                         :::
        //:::           GeoDataSource.com (C) All Rights Reserved 2022                :::
        //:::                                                                         :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //https://www.geodatasource.com/developers/c-sharp
        private double _LatitudeLongitudeDistance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) * Math.Cos(DegreesToRadians(theta));
                dist = Math.Acos(dist);
                dist = RadiansToDegrees(dist);

                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }
        public double _Distance(Cordinates a, Cordinates b, Cordinates c)
        {
            // normalize points
            Cordinates cn = new Cordinates { X = c.X - a.X, Y = c.Y - a.Y };
            Cordinates bn = new Cordinates { X = b.X - a.X, Y = b.Y - a.Y };

            double angle = Math.Atan2(bn.Y, bn.X) - Math.Atan2(cn.Y, cn.X);
            double abLength = Math.Sqrt(bn.X * bn.X + bn.Y * bn.Y);

            return Math.Sin(angle) * abLength;

        }
        public int _Direction(Cordinates a, Cordinates b, Cordinates c)
        {
            double val = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);
            if (val == 0d)
                return 0;     //colinear
            else if (val < 0d)
                return 2;    //anti-clockwise direction
            return 1;    //clockwise direction
        }
        public double GetCenterLineLength(LayerInfo centerLine)
        {
            double dTemp = 0d;
            if (centerLine.Data.HasBulge)
            {
                int iTotal = centerLine.Data.CoordinateWithBulge.Count;
                if (centerLine.Data.CoordinateWithBulge[0].ItemValue.StartPoint.Equals(centerLine.Data.CoordinateWithBulge[iTotal - 1].ItemValue.StartPoint))
                    iTotal = iTotal - 1;

                for (int iCnt = 1; iCnt < iTotal; iCnt++)
                {
                    BulgeItemValue itemValue = centerLine.Data.CoordinateWithBulge[iCnt].ItemValue as BulgeItemValue;
                    if (centerLine.Data.CoordinateWithBulge[iCnt].IsBulgeValue)
                    {
                        ArcSegment arc = new ArcSegment(itemValue.StartPoint, itemValue.EndPoint, itemValue.Bulge);
                        double d1 = itemValue.StartPoint.GetDistanceFrom(centerLine.Data.CoordinateWithBulge[iCnt - 1].ItemValue.StartPoint);
                        double d3 = 0;
                        d3 = itemValue.EndPoint.GetDistanceFrom(centerLine.Data.CoordinateWithBulge[iCnt + 1].ItemValue.StartPoint);

                        dTemp += FormatFigureInDecimalPoint(d1 + d3 + arc.Length);

                        iCnt++;
                        if (centerLine.Data.CoordinateWithBulge[iCnt].IsBulgeValue)
                        {
                            itemValue = centerLine.Data.CoordinateWithBulge[iCnt].ItemValue as BulgeItemValue;
                            arc = new ArcSegment(itemValue.StartPoint, itemValue.EndPoint, itemValue.Bulge);
                            dTemp += arc.Length;
                            iCnt++;
                            dTemp += centerLine.Data.CoordinateWithBulge[iCnt].ItemValue.StartPoint.GetDistanceFrom(centerLine.Data.CoordinateWithBulge[iCnt - 1].ItemValue.EndPoint); // CalculateDistanceBetweenTwoPoints(centerLine.Data.CoordinateWithBulge[iCnt].ItemValue.StartPoint, centerLine.Data.CoordinateWithBulge[iCnt - 1].ItemValue.EndPoint);
                        }


                    }
                    else
                    {
                        dTemp += centerLine.Data.CoordinateWithBulge[iCnt].ItemValue.StartPoint.GetDistanceFrom(centerLine.Data.CoordinateWithBulge[iCnt - 1].ItemValue.StartPoint);   // CalculateDistanceBetweenTwoPoints(centerLine.Data.CoordinateWithBulge[iCnt].ItemValue.StartPoint, centerLine.Data.CoordinateWithBulge[iCnt - 1].ItemValue.StartPoint);
                    }
                }
                //Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Bulge Line: " + dTemp);
            }
            else
            {
                int iTotal = centerLine.Data.Coordinates.Count;
                if (centerLine.Data.Coordinates.First().Equals(centerLine.Data.Coordinates.Last()))
                    centerLine.Data.Coordinates = centerLine.Data.Coordinates.Take(centerLine.Data.Coordinates.Count - 1).ToList();

                dTemp += TotalLengthFromCoordinates(centerLine.Data.Coordinates);
            }
            return dTemp;
        }
        public List<double> FindAllDistanceBetweenPolygonAndPointUsingCoordinateAndLines(LayerInfo poly, Cordinates fromPoint)
        {
            List<double> lstDistances = new List<double>();
            if (poly.Data.Coordinates == null || fromPoint == null)
                return lstDistances;

            List<double> lstDistanceUsingCord = new List<double>();
            foreach (Cordinates cord in poly.Data.Coordinates)
                lstDistanceUsingCord.Add(cord.GetDistanceFrom(fromPoint));

            poly.Data.Lines = MakeClosePolyLines(poly.Data.Coordinates.DeepClone());
            foreach (CLineSegment line in poly.Data.Lines)
            {
                double? dDistance = objLineOperation.GetPerpendicularDistanceFromLineSegment(line, fromPoint);
                if (dDistance != null && dDistance.HasValue)
                    lstDistanceUsingCord.Add(dDistance.Value);
            }

            return lstDistanceUsingCord;
        }

        public List<double> FindAllDistanceBetweenPointsToPoint(List<Cordinates> toPoints, Cordinates fromPoint)
        {
            List<double> lstDistances = new List<double>();
            if (toPoints == null || fromPoint == null)
                return lstDistances;

            List<double> lstDistanceUsingCord = new List<double>();
            foreach (Cordinates cord in toPoints)
                lstDistanceUsingCord.Add(cord.GetDistanceFrom(fromPoint));

            return lstDistanceUsingCord;
        }

        public double? FindMiminumDistanceBetweenPointsToPoint(List<Cordinates> toPoints, Cordinates fromPoint)
        {
            List<double> lstDistances = new List<double>();
            if (toPoints == null || fromPoint == null)
                return null;

            List<double> lstDistanceUsingCord = new List<double>();
            foreach (Cordinates cord in toPoints)
                lstDistanceUsingCord.Add(cord.GetDistanceFrom(fromPoint));

            return lstDistanceUsingCord.Min();
        }

        public List<double> FindAllDistanceBetweenPolygonAndPointUsingCoordinateAndLines(LayerDataWithText poly, Cordinates fromPoint)
        {
            List<double> lstDistances = new List<double>();
            if (poly.Coordinates == null || fromPoint == null)
                return lstDistances;

            List<double> lstDistanceUsingCord = new List<double>();
            foreach (Cordinates cord in poly.Coordinates)
                lstDistanceUsingCord.Add(cord.GetDistanceFrom(fromPoint));

            if (poly.Lines == null)
                poly.Lines = MakeClosePolyLines(poly.Coordinates.DeepClone());

            foreach (CLineSegment line in poly.Lines)
            {
                double? dDistance = objLineOperation.GetPerpendicularDistanceFromLineSegment(line, fromPoint);
                if (dDistance != null && dDistance.HasValue)
                    lstDistanceUsingCord.Add(dDistance.Value);
            }

            return lstDistanceUsingCord;
        }
        public List<double> FindAllDistanceBetweenTwoPolygonUsingCoordinateAndLines(LayerInfo fromData, LayerInfo toData)
        {
            List<double> lstDistances = new List<double>();
            if (fromData.Data.Coordinates == null || toData.Data.Coordinates == null)
                return lstDistances;

            List<double> lstDistanceUsingCord = FindAllDistanceBetweenTwoPolygonUsingCoordinate(fromData.Data.Coordinates, toData.Data.Coordinates);
            if (lstDistanceUsingCord != null && lstDistanceUsingCord.Count > 0)
                lstDistances.AddRange(lstDistanceUsingCord);

            fromData.Data.Lines = MakeClosePolyLines(fromData.Data.Coordinates.DeepClone());
            toData.Data.Lines = MakeClosePolyLines(toData.Data.Coordinates.DeepClone());

            List<double> lstDistanceUsingLine = FindAllDistanceBetweenTwoPolygonUsingCoordinate(fromData.Data.Lines.DeepClone(), toData.Data.Lines.DeepClone());
            if (lstDistanceUsingLine != null && lstDistanceUsingLine.Count > 0)
                lstDistances.AddRange(lstDistanceUsingLine);

            return lstDistances;
        }
        public List<double> FindAllDistanceBetweenTwoPolygonUsingCoordinateAndLines(LayerDataWithText fromData, LayerDataWithText toData)
        {
            List<double> lstDistanceReturn = new List<double>();
            if (fromData.Coordinates == null || toData.ColourCode == null)
                return lstDistanceReturn;



            List<double> lstDistanceCord = FindAllDistanceBetweenTwoPolygonUsingCoordinate(fromData.Coordinates, toData.Coordinates);
            if (lstDistanceCord != null && lstDistanceCord.Count > 0)
                lstDistanceReturn.AddRange(lstDistanceCord);

            fromData.Lines = MakeClosePolyLines(fromData.Coordinates.DeepClone());
            toData.Lines = MakeClosePolyLines(toData.Coordinates.DeepClone());

            List<double> lstDistance = FindAllDistanceBetweenTwoPolygonUsingCoordinate(fromData.Lines.DeepClone(), toData.Lines.DeepClone());
            if (lstDistance != null && lstDistance.Count > 0)
                lstDistanceReturn.AddRange(lstDistance);

            return lstDistanceReturn;
        }
        private List<double> FindAllDistanceBetweenTwoPolygonUsingCoordinate(List<Cordinates> lstSourcePolygonCoordinates, List<Cordinates> lstCompareCoordinates)
        {
            List<double> lstDistance = new List<double>();
            if (lstSourcePolygonCoordinates == null || lstCompareCoordinates == null || lstSourcePolygonCoordinates.Count == 0 || lstCompareCoordinates.Count == 0)
                return lstDistance;

            foreach (Cordinates cord1 in lstSourcePolygonCoordinates)
            {
                foreach (Cordinates cord2 in lstCompareCoordinates)
                {
                    //double dDistance = Math.Abs( cord1.GetDistanceFrom(cord2));
                    double dDistance = cord1.GetDistanceFrom(cord2);
                    lstDistance.Add(dDistance);
                }
            } // for target 

            return lstDistance;
        }
        private List<double> FindAllDistanceBetweenTwoPolygonUsingCoordinate(List<CLineSegment> lstSourcePolygonLines, List<CLineSegment> lstCompareWithPolygonLines)
        {
            List<double> lstDistance = new List<double>();
            if (lstSourcePolygonLines == null || lstCompareWithPolygonLines == null || lstSourcePolygonLines.Count == 0 || lstCompareWithPolygonLines.Count == 0)
                return lstDistance;

            for (int iSourcePolygonLine = 0; iSourcePolygonLine < lstSourcePolygonLines.Count; iSourcePolygonLine++)
            {
                CLineSegment sourceLine = lstSourcePolygonLines[iSourcePolygonLine];
                for (int iComparePolygonLine = 0; iComparePolygonLine < lstCompareWithPolygonLines.Count; iComparePolygonLine++)
                {
                    CLineSegment comparePolygonLine = lstCompareWithPolygonLines[iComparePolygonLine];

                    double? dDistance = FindMinimumDistanceBetweenTwoLines(sourceLine, comparePolygonLine);
                    if (dDistance != null && dDistance.HasValue)
                        lstDistance.Add(dDistance.Value);

                } // for lines
            } // for target line

            return lstDistance;
        }
        public void FindMinimumDistanceCenterLineToPolygonLineSegmentMidPointPerpendicular(List<Cordinates> lstSourceCoordinates, List<CLineSegment> lstCompareWithPolygonLines, ref double? minDistance, ref CLineSegment DistanceWithLine)
        {
            minDistance = null;
            double iLineLength = -1d;

            for (int iComparePolygonLine = 0; iComparePolygonLine < lstCompareWithPolygonLines.Count; iComparePolygonLine++)
            {
                CLineSegment comparePolygonLine = lstCompareWithPolygonLines[iComparePolygonLine];
                foreach (Cordinates cord in lstSourceCoordinates)
                {
                    Cordinates cordIntersect = objLineOperation.GetPerpendicularIntersectionPoint(comparePolygonLine, cord);
                    if (comparePolygonLine.IsPointOnLine(cordIntersect, General.ErrorAllowScale))
                    {
                        double dDistance = cordIntersect.GetDistanceFrom(cord);
                        if (minDistance == null || minDistance.Value >= dDistance || Math.Abs(minDistance.Value - dDistance) < General.ErrorAllowScale)
                        {
                            if (minDistance == null)
                            {
                                minDistance = dDistance;
                                iLineLength = comparePolygonLine.Length;
                                DistanceWithLine = comparePolygonLine;
                            }
                            else if ((minDistance.Value > dDistance && (Math.Abs(minDistance.Value - dDistance) > General.ErrorAllowScale)) || (Math.Abs(minDistance.Value - dDistance) < General.ErrorAllowScale && iLineLength > comparePolygonLine.Length)) // if (minDistance.Value > dDistance || (minDistance.Value == dDistance && iLineLength > comparePolygonLine.Length))
                            {
                                minDistance = dDistance;
                                iLineLength = comparePolygonLine.Length;
                                DistanceWithLine = comparePolygonLine;
                            }
                        }
                    }

                } // for target line
            }
        }
        public void FindMinimumDistanceCenterLineToPolygonLineSegment(List<Cordinates> lstSourceCoordinates, List<CLineSegment> lstCompareWithPolygonLines, ref List<CLineSegment> CloseLinesWithCenterLine)
        {
            for (int iComparePolygonLine = 0; iComparePolygonLine < lstCompareWithPolygonLines.Count; iComparePolygonLine++)
            {
                CLineSegment comparePolygonLine = lstCompareWithPolygonLines[iComparePolygonLine];
                foreach (Cordinates cord in lstSourceCoordinates)
                {
                    Cordinates cordIntersect = FindIntersectionPoint(comparePolygonLine, cord);
                    if (comparePolygonLine.IsPointOnLine(cordIntersect, General.ErrorAllowScale))
                    {
                        double dDistance = cordIntersect.GetDistanceFrom(cord);
                        if (dDistance < General.ErrorAllowScale)
                            CloseLinesWithCenterLine.Add(comparePolygonLine);
                    }

                } // for target line
            }
        }
        public void FindMinimumDistanceCenterLineToPolygonLineSegmentMidPoint(List<Cordinates> lstSourceCoordinates, List<CLineSegment> lstCompareWithPolygonLines, ref double? minDistance, ref CLineSegment DistanceWithLine)
        {
            minDistance = null;
            double iLineLength = -1d;

            for (int iComparePolygonLine = 0; iComparePolygonLine < lstCompareWithPolygonLines.Count; iComparePolygonLine++)
            {
                CLineSegment comparePolygonLine = lstCompareWithPolygonLines[iComparePolygonLine];
                foreach (Cordinates cord in lstSourceCoordinates)
                {
                    Cordinates cordIntersect = FindIntersectionPoint(comparePolygonLine, cord);
                    if (comparePolygonLine.IsPointOnLine(cordIntersect, General.ErrorAllowScale))
                    {
                        double dDistance = cordIntersect.GetDistanceFrom(cord);
                        if (minDistance == null || minDistance.Value >= dDistance || Math.Abs(minDistance.Value - dDistance) < General.ErrorAllowScale) // General.fBufferScale) //(minDistance.Value >= dDistance)) //&& iLineLength > comparePolygonLine.Length))
                        {
                            if (minDistance == null)
                            {
                                minDistance = dDistance;
                                iLineLength = comparePolygonLine.Length;
                                DistanceWithLine = comparePolygonLine;
                            }
                            else if ((minDistance.Value > dDistance && (Math.Abs(minDistance.Value - dDistance) > General.ErrorAllowScale)) || (Math.Abs(minDistance.Value - dDistance) < General.ErrorAllowScale && iLineLength > comparePolygonLine.Length)) // if (minDistance.Value > dDistance || (minDistance.Value == dDistance && iLineLength > comparePolygonLine.Length))
                            {
                                minDistance = dDistance;
                                iLineLength = comparePolygonLine.Length;
                                DistanceWithLine = comparePolygonLine;
                            }
                        }
                    }

                } // for target line
            }
        }
        public void FindMinimumDistanceWithCordCenterLineToPolygonLineSegmentMidPoint(List<Cordinates> lstSourceCoordinates, List<CLineSegment> lstCompareWithPolygonLines, ref DistanceWithCordinate minDistance, ref CLineSegment DistanceWithLine)
        {
            minDistance = null;
            double iLineLength = -1d;

            for (int iComparePolygonLine = 0; iComparePolygonLine < lstCompareWithPolygonLines.Count; iComparePolygonLine++)
            {
                CLineSegment comparePolygonLine = lstCompareWithPolygonLines[iComparePolygonLine];
                foreach (Cordinates cord in lstSourceCoordinates)
                {
                    Cordinates cordIntersect = FindIntersectionPoint(comparePolygonLine, cord);
                    if (comparePolygonLine.IsPointOnLine(cordIntersect, General.ErrorAllowScale))
                    {
                        double dDistance = cordIntersect.GetDistanceFrom(cord);
                        if (minDistance == null || minDistance.Distance >= dDistance || Math.Abs(minDistance.Distance - dDistance) < General.ErrorAllowScale)
                        {
                            if (minDistance == null || minDistance.Distance == double.MaxValue)
                            {
                                minDistance = new DistanceWithCordinate { StartPoint = cordIntersect, EndPoint = cord, Distance = dDistance };
                                iLineLength = comparePolygonLine.Length;
                                DistanceWithLine = comparePolygonLine;
                            }
                            else if ((minDistance.Distance > dDistance && (Math.Abs(minDistance.Distance - dDistance) > General.ErrorAllowScale)) || (Math.Abs(minDistance.Distance - dDistance) < General.ErrorAllowScale && iLineLength > comparePolygonLine.Length))
                            {
                                minDistance = new DistanceWithCordinate { StartPoint = cordIntersect, EndPoint = cord, Distance = dDistance };
                                iLineLength = comparePolygonLine.Length;
                                DistanceWithLine = comparePolygonLine;
                            }
                        }
                    }
                } // for target line
            }
        }
        public double? FindMinimumDistanceBetweenTwoPolygon(List<CLineSegment> lstSourcePolygonLines, List<CLineSegment> lstCompareWithPolygonLines)
        {
            double? minDistance = null;
            for (int iSourcePolygonLine = 0; iSourcePolygonLine < lstSourcePolygonLines.Count; iSourcePolygonLine++)
            {
                CLineSegment sourceLine = lstSourcePolygonLines[iSourcePolygonLine];
                for (int iComparePolygonLine = 0; iComparePolygonLine < lstCompareWithPolygonLines.Count; iComparePolygonLine++)
                {
                    CLineSegment comparePolygonLine = lstCompareWithPolygonLines[iComparePolygonLine];
                    Cordinates intersect = FindIntersectionPoint(comparePolygonLine.StartPoint, comparePolygonLine.EndPoint, sourceLine.MidPoint);
                    if (comparePolygonLine.IsPointOnLine(intersect, General.ErrorAllowScale))
                    {
                        double dDistance = GetMinimumDistance(comparePolygonLine, sourceLine.MidPoint);

                        if (minDistance == null || minDistance > dDistance)
                            minDistance = dDistance;
                    }
                } // for lines
            } // for target line

            return minDistance;
        }
        public double? FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLines(LayerInfo fromData, LayerInfo toData)
        {
            if (fromData.Data.Coordinates == null || toData.Data.Coordinates == null)
                return null;

            double? dDistanceCord = FindMinimumDistanceBetweenTwoPolygonUsingCoordinate(fromData.Data.Coordinates, toData.Data.Coordinates);

            fromData.Data.Lines = MakeClosePolyLines(fromData.Data.Coordinates.DeepClone());
            toData.Data.Lines = MakeClosePolyLines(toData.Data.Coordinates.DeepClone());

            double? dDistance = FindMinimumDistanceBetweenTwoPolygonUsingCoordinate(fromData.Data.Lines.DeepClone(), toData.Data.Lines.DeepClone());

            if (dDistanceCord != null && dDistanceCord.HasValue && dDistance != null & dDistance.HasValue)
            {
                if (dDistance.Value > dDistanceCord.Value)
                    dDistance = dDistanceCord;
            }

            return dDistance;
        }
        public double? FindMinimumDistanceBetweenPolygonAndLineUsingCoordinateAndLines(LayerDataWithText polygon1, LayerDataWithText polygon2, bool MakeLinesAgain = true)
        {
            if (polygon1.Coordinates == null || polygon2.Coordinates == null)
            {
                return null;
            }

            // Make sure both object closed polygon otherwise it intersect in case of both are closed polygon 
            if (polygon1.Coordinates.First().Equals(polygon1.Coordinates.Last()) && polygon2.Coordinates.First().Equals(polygon2.Coordinates.Last()))
            {
                double dIntersectArea = 0, dTemp = 0;
                if (objNet_Topology_Suite.IsIntersects(polygon1.Coordinates.DeepClone(), polygon2.Coordinates.DeepClone(), ref dIntersectArea, ref dTemp))
                {
                    if (dIntersectArea > ErrorAllowScale)
                    {
                        return 0; // it is cross or overlap or intersect mean it touched. 09Apr2024
                    }
                }
            }

            double? dDistanceReturn = null;
            double? dDistanceCord = FindMinimumDistanceBetweenTwoPolygonUsingCoordinate(polygon1.Coordinates.DeepClone(), polygon2.Coordinates.DeepClone());
            if (dDistanceCord != null && dDistanceCord.HasValue)
            {
                dDistanceReturn = dDistanceCord;
            }

            if (MakeLinesAgain || polygon1.Lines == null)
            {
                if (!polygon1.IsCircle)
                    polygon1.Lines = MakeClosePolyLines(polygon1.Coordinates);
                else
                    polygon1.Lines = MakeConnectedLines(polygon1.Coordinates);
            }

            if (MakeLinesAgain || polygon2.Lines == null)
            {
                if (!polygon2.IsCircle)
                    polygon2.Lines = MakeClosePolyLines(polygon2.Coordinates);
                else
                    polygon2.Lines = MakeConnectedLines(polygon2.Coordinates);
            }

            double? dDistance = FindMinimumDistanceBetweenTwoPolygonUsingCoordinate(polygon1.Lines.DeepClone(), polygon2.Lines.DeepClone());
            if (dDistance != null & dDistance.HasValue)
            {
                if (dDistanceReturn != null && dDistanceReturn.HasValue && dDistance.Value < dDistanceReturn.Value)
                    dDistanceReturn = dDistance;
                else if (dDistanceReturn == null || !dDistanceReturn.HasValue)
                    dDistanceReturn = dDistance;
            }
            return dDistanceReturn;
        }
        public double? FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLines(LayerDataWithText fromData, LayerDataWithText toData)
        {
            if (fromData.Coordinates == null || toData.ColourCode == null)
                return null;

            double? dDistanceReturn = null;
            double? dDistanceCord = FindMinimumDistanceBetweenTwoPolygonUsingCoordinate(fromData.Coordinates, toData.Coordinates);
            if (dDistanceCord != null && dDistanceCord.HasValue)
                dDistanceReturn = dDistanceCord;

            if (!fromData.IsCircle)
                fromData.Lines = MakeClosePolyLines(fromData.Coordinates);
            else
                fromData.Lines = MakeConnectedLines(fromData.Coordinates);

            if (!toData.IsCircle)
                toData.Lines = MakeClosePolyLines(toData.Coordinates);
            else
                toData.Lines = MakeConnectedLines(toData.Coordinates);

            double? dDistance = FindMinimumDistanceBetweenTwoPolygonUsingCoordinate(fromData.Lines.DeepClone(), toData.Lines.DeepClone());
            if (dDistance != null & dDistance.HasValue)
            {
                if (dDistanceReturn != null && dDistanceReturn.HasValue && dDistance.Value < dDistanceReturn.Value)
                    dDistanceReturn = dDistance; //dDistance = dDistanceReturn;
                else if (dDistanceReturn == null || !dDistanceReturn.HasValue)
                    dDistanceReturn = dDistance;
            }

            return dDistanceReturn;
        }

        //added on 04Sept2024
        public double? FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLinesWithCoordinateValue(List<Cordinates> fromData, List<Cordinates> toData)
        {
            if (fromData == null || toData == null)
                return null;

            DistanceWithCordinate dDistanceReturn = new DistanceWithCordinate();
            dDistanceReturn.Distance = double.MaxValue;
            DistanceWithCordinate dDistanceCord = FindMinimumDistanceWithCordsBetweenTwoPolygonUsingCoordinate(fromData, toData);
            if (dDistanceCord != null)
                dDistanceReturn = dDistanceCord;

            List<CLineSegment> fromDataLines = MakeConnectedLines(fromData);
            List<CLineSegment> toDataLines = MakeConnectedLines(toData);

            DistanceWithCordinate dDistance = FindMinimumDistanceWithCordsBetweenTwoPolygonUsingCoordinate(fromDataLines, toDataLines);
            if (dDistance != null && dDistance.Distance != double.MaxValue)
            {
                if (dDistanceReturn.Distance != double.MaxValue && dDistance.Distance < dDistanceReturn.Distance)
                    dDistanceReturn = dDistance;
                else if (dDistanceReturn.Distance == double.MaxValue)
                    dDistanceReturn = dDistance;
            }

            if (dDistanceReturn.Distance == double.MaxValue)
                return null;

            return dDistanceReturn.Distance;
        }


        public DistanceWithCordinate FindMinimumDistanceBetweenTwoPolygonUsingCoordinateAndLinesWithCoordinateValue(LayerDataWithText fromData, LayerDataWithText toData, bool bRegenerateCalculate = true)
        {
            if (fromData.Coordinates == null || toData.ColourCode == null)
                return null;

            DistanceWithCordinate dDistanceReturn = new DistanceWithCordinate();
            dDistanceReturn.Distance = double.MaxValue;
            DistanceWithCordinate dDistanceCord = FindMinimumDistanceWithCordsBetweenTwoPolygonUsingCoordinate(fromData.Coordinates, toData.Coordinates);
            if (dDistanceCord != null)
                dDistanceReturn = dDistanceCord;

            if (bRegenerateCalculate)
            {
                if (!fromData.IsCircle)
                    fromData.Lines = MakeClosePolyLines(fromData.Coordinates);
                else
                    fromData.Lines = MakeConnectedLines(fromData.Coordinates);

                if (!toData.IsCircle)
                    toData.Lines = MakeClosePolyLines(toData.Coordinates);
                else
                    toData.Lines = MakeConnectedLines(toData.Coordinates);
            }

            DistanceWithCordinate dDistance = FindMinimumDistanceWithCordsBetweenTwoPolygonUsingCoordinate(fromData.Lines.DeepClone(), toData.Lines.DeepClone());
            if (dDistance != null && dDistance.Distance != double.MaxValue)
            {
                if (dDistanceReturn.Distance != double.MaxValue && dDistance.Distance < dDistanceReturn.Distance)
                    dDistanceReturn = dDistance;
                else if (dDistanceReturn.Distance == double.MaxValue)
                    dDistanceReturn = dDistance;
            }

            if (dDistanceReturn.Distance == double.MaxValue)
                return null;

            return dDistanceReturn;
        }
        private double? FindMinimumDistanceBetweenTwoPolygonUsingCoordinate(List<Cordinates> lstSourcePolygonCoordinates, List<Cordinates> lstCompareCoordinates)
        {
            if (lstSourcePolygonCoordinates == null || lstCompareCoordinates == null || lstSourcePolygonCoordinates.Count == 0 || lstCompareCoordinates.Count == 0)
                return null;

            double? minDistance = double.MaxValue;
            foreach (Cordinates cord1 in lstSourcePolygonCoordinates)
            {
                foreach (Cordinates cord2 in lstCompareCoordinates)
                {
                    double dDistance = cord1.GetDistanceFrom(cord2);
                    if (minDistance == null || minDistance.Value > dDistance)
                        minDistance = dDistance;
                }
            } // for target 

            return minDistance;
        }
        private DistanceWithCordinate FindMinimumDistanceWithCordsBetweenTwoPolygonUsingCoordinate(List<Cordinates> lstSourcePolygonCoordinates, List<Cordinates> lstCompareCoordinates)
        {
            if (lstSourcePolygonCoordinates == null || lstCompareCoordinates == null || lstSourcePolygonCoordinates.Count == 0 || lstCompareCoordinates.Count == 0)
                return null;

            DistanceWithCordinate minDistance = new DistanceWithCordinate();
            minDistance.Distance = double.MaxValue;
            foreach (Cordinates cord1 in lstSourcePolygonCoordinates)
            {
                foreach (Cordinates cord2 in lstCompareCoordinates)
                {
                    double dDistance = cord1.GetDistanceFrom(cord2);
                    if (minDistance == null || minDistance.Distance > dDistance)
                    {
                        minDistance.Distance = dDistance;
                        minDistance.StartPoint = cord1;
                        minDistance.EndPoint = cord2;
                    }
                }
            } // for target 

            if (minDistance.Distance == double.MaxValue)
                return null;

            return minDistance;
        }
        private double? FindMinimumDistanceBetweenTwoPolygonUsingCoordinate(List<CLineSegment> lstSourcePolygonLines, List<CLineSegment> lstCompareWithPolygonLines)
        {
            if (lstSourcePolygonLines == null || lstCompareWithPolygonLines == null || lstSourcePolygonLines.Count == 0 || lstCompareWithPolygonLines.Count == 0)
            {
                return null;
            }

            double? minDistance = double.MaxValue;
            LineOperations objLineOpt = new LineOperations();
            for (int iSourcePolygonLine = 0; iSourcePolygonLine < lstSourcePolygonLines.Count; iSourcePolygonLine++)
            {
                CLineSegment sourceLine = lstSourcePolygonLines[iSourcePolygonLine];
                for (int iComparePolygonLine = 0; iComparePolygonLine < lstCompareWithPolygonLines.Count; iComparePolygonLine++)
                {
                    CLineSegment comparePolygonLine = lstCompareWithPolygonLines[iComparePolygonLine];

                    double? dDistance = FindMinimumDistanceBetweenTwoLines(sourceLine, comparePolygonLine);
                    if (dDistance != null && dDistance.HasValue && minDistance.Value > dDistance.Value)
                        minDistance = dDistance;

                } // for lines
            } // for target line

            return minDistance;
        }

        private DistanceWithCordinate FindMinimumDistanceWithCordsBetweenTwoPolygonUsingCoordinate(List<CLineSegment> lstSourcePolygonLines, List<CLineSegment> lstCompareWithPolygonLines)
        {
            if (lstSourcePolygonLines == null || lstCompareWithPolygonLines == null || lstSourcePolygonLines.Count == 0 || lstCompareWithPolygonLines.Count == 0)
                return null;

            DistanceWithCordinate minDistance = new DistanceWithCordinate();
            minDistance.Distance = double.MaxValue;
            LineOperations objLineOpt = new LineOperations();
            for (int iSourcePolygonLine = 0; iSourcePolygonLine < lstSourcePolygonLines.Count; iSourcePolygonLine++)
            {
                CLineSegment sourceLine = lstSourcePolygonLines[iSourcePolygonLine];
                for (int iComparePolygonLine = 0; iComparePolygonLine < lstCompareWithPolygonLines.Count; iComparePolygonLine++)
                {
                    CLineSegment comparePolygonLine = lstCompareWithPolygonLines[iComparePolygonLine];
                    DistanceWithCordinate dDistance = FindMinimumDistanceWithCordsBetweenTwoLines(sourceLine, comparePolygonLine);
                    if (dDistance != null && dDistance.Distance != double.MaxValue && minDistance.Distance > dDistance.Distance)
                    {
                        minDistance = dDistance;
                    }
                } // for lines
            } // for target line

            if (minDistance.Distance == double.MaxValue)
                return null;

            return minDistance;
        }

        public List<DistanceWithLineReference> FindMinimumDistanceWithLineUsingReferenceLine(CLineSegment referenceLine, List<CLineSegment> listLines)
        {
            List<DistanceWithLineReference> lstDistance = new List<DistanceWithLineReference>();
            CLineSegment PerpendicularLineFromReferenceLineMidPoint = objLineOperation.GetPerpendicularSegmentPassingFromPoint(referenceLine, referenceLine.MidPoint);

            foreach (CLineSegment line in listLines)
            {
                Cordinates intersectPoint = objLineOperation.GetLineIntersectionPoint(PerpendicularLineFromReferenceLineMidPoint, line);
                if (line.IsPointOnLine(intersectPoint, General.ErrorAllowScale))
                {
                    double dDistance = intersectPoint.GetDistanceFrom(referenceLine.MidPoint);
                    if (dDistance != -1)
                    {
                        DistanceWithLineReference obj = new DistanceWithLineReference();
                        obj.Distance = dDistance;
                        obj.lineFrom = referenceLine;
                        obj.lineTo = line;
                        lstDistance.Add(obj);
                    }
                }
            }

            return lstDistance;
        }
        public List<DistanceWithLineReference> FindMinimumDistanceWithLineUsingReferenceLineInPolygonWithNonParallelLines(CLineSegment referenceLine, List<CLineSegment> listLines)
        {
            List<DistanceWithLineReference> lstDistance = new List<DistanceWithLineReference>();
            LineOperations objLineOpt = new LineOperations();
            foreach (CLineSegment line in listLines)
            {
                if (!line.IsParallel(referenceLine))
                    continue;

                double? dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(line, referenceLine.MidPoint);
                if (dDistance != null && dDistance.HasValue && dDistance != -1)
                {
                    DistanceWithLineReference obj = new DistanceWithLineReference();
                    obj.Distance = dDistance.Value;
                    obj.lineFrom = referenceLine;
                    obj.lineTo = line;
                    lstDistance.Add(obj);
                }
            }

            return lstDistance;
        }
        public List<DistanceWithLineReference> FindMinimumDistanceWithLineUsingReferenceLineInPolygon(CLineSegment referenceLine, List<CLineSegment> listLines)
        {
            List<DistanceWithLineReference> lstDistance = new List<DistanceWithLineReference>();
            LineOperations objLineOpt = new LineOperations();
            foreach (CLineSegment line in listLines)
            {
                if (!line.IsConnectedLine(referenceLine))
                {
                    double? dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(line, referenceLine.MidPoint);
                    if (dDistance != null && dDistance.HasValue && dDistance != -1)
                    {
                        DistanceWithLineReference obj = new DistanceWithLineReference();
                        obj.Distance = dDistance.Value;
                        obj.lineFrom = referenceLine;
                        obj.lineTo = line;
                        lstDistance.Add(obj);
                    }
                }
            }

            return lstDistance;
        }
        public List<DistanceWithLineReference> OLD_FindMinimumDistanceWithLineUsingReferenceLineInPolygon(CLineSegment referenceLine, List<CLineSegment> listLines)
        {
            List<DistanceWithLineReference> lstDistance = new List<DistanceWithLineReference>();
            LineOperations objLineOpt = new LineOperations();
            foreach (CLineSegment line in listLines)
            {
                double? dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(line, referenceLine.MidPoint);
                if (dDistance != null && dDistance.HasValue && dDistance != -1)
                {
                    DistanceWithLineReference obj = new DistanceWithLineReference();
                    obj.Distance = dDistance.Value;
                    obj.lineFrom = referenceLine;
                    obj.lineTo = line;
                    lstDistance.Add(obj);
                }
            }

            return lstDistance;
        }
        public List<double> FindMinimumDistanceInPolygonInReferenceLine(CLineSegment referenceLine, List<CLineSegment> listLines)
        {
            List<double> lstDistance = new List<double>();
            LineOperations objLineOpt = new LineOperations();
            foreach (CLineSegment line in listLines)
            {
                double? dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(line, referenceLine.MidPoint);
                if (dDistance != null && dDistance.HasValue && dDistance != -1)
                {
                    lstDistance.Add(dDistance.Value);
                }
            }

            return lstDistance;
        }
        public double? FindMinimumDistanceBetweenTwoLines(CLineSegment sourceLine, CLineSegment comparePolygonLine)
        {
            double? minDistance = double.MaxValue;
            LineOperations objLineOpt = new LineOperations();

            double? dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(sourceLine, comparePolygonLine.MidPoint);
            if (dDistance != null && dDistance.HasValue && dDistance.Value != -1)
            {
                if (minDistance == null || (minDistance.HasValue && dDistance != null && dDistance.HasValue && minDistance.Value > dDistance.Value))
                    minDistance = dDistance;
            }

            dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(comparePolygonLine, sourceLine.MidPoint);
            if (dDistance != null && dDistance.HasValue && dDistance.Value != -1)
            {
                if (minDistance == null || (minDistance.HasValue && minDistance.Value > dDistance.Value))
                    minDistance = dDistance;
            }

            dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(comparePolygonLine, sourceLine.StartPoint);
            if (dDistance != null && dDistance.HasValue && dDistance.Value != -1)
            {
                if (minDistance == null || (minDistance.HasValue && minDistance.Value > dDistance.Value))
                    minDistance = dDistance;
            }

            dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(comparePolygonLine, sourceLine.EndPoint);
            if (dDistance != null && dDistance.HasValue && dDistance.Value != -1)
            {
                if (minDistance == null || (minDistance.HasValue && minDistance.Value > dDistance.Value))
                    minDistance = dDistance;
            }

            dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(sourceLine, comparePolygonLine.StartPoint);
            if (dDistance != null && dDistance.HasValue && dDistance.Value != -1)
            {
                if (minDistance == null || (minDistance.HasValue && minDistance.Value > dDistance.Value))
                    minDistance = dDistance;
            }

            dDistance = objLineOpt.GetPerpendicularDistanceFromLineSegment(sourceLine, comparePolygonLine.EndPoint);
            if (dDistance != null && dDistance.HasValue && dDistance.Value != -1)
            {
                if (minDistance == null || (minDistance.HasValue && minDistance.Value > dDistance.Value))
                    minDistance = dDistance;
            }

            //intersection
            Cordinates cordIntersectPoint = FindIntersectionPoint(sourceLine.StartPoint, sourceLine.EndPoint, comparePolygonLine.MidPoint);
            double mainLineWidth1 = sourceLine.StartPoint.GetDistanceFrom(cordIntersectPoint);
            double mainLineWidth2 = cordIntersectPoint.GetDistanceFrom(sourceLine.EndPoint);
            double diffOffSet = (mainLineWidth1 + mainLineWidth2) - sourceLine.Length;
            if (Math.Abs(diffOffSet) < 0.005d)
            {
                double temp = comparePolygonLine.MidPoint.GetDistanceFrom(cordIntersectPoint);
                if (temp < minDistance.Value)
                    minDistance = temp;
            }

            cordIntersectPoint = FindIntersectionPoint(sourceLine.StartPoint, sourceLine.EndPoint, comparePolygonLine.StartPoint);
            mainLineWidth1 = sourceLine.StartPoint.GetDistanceFrom(cordIntersectPoint);
            mainLineWidth2 = cordIntersectPoint.GetDistanceFrom(sourceLine.EndPoint);
            diffOffSet = (mainLineWidth1 + mainLineWidth2) - sourceLine.Length;
            if (Math.Abs(diffOffSet) < 0.005d)
            {
                double temp = comparePolygonLine.StartPoint.GetDistanceFrom(cordIntersectPoint);
                if (temp < minDistance.Value)
                    minDistance = temp;
            }

            cordIntersectPoint = FindIntersectionPoint(sourceLine.StartPoint, sourceLine.EndPoint, comparePolygonLine.EndPoint);
            mainLineWidth1 = sourceLine.StartPoint.GetDistanceFrom(cordIntersectPoint);
            mainLineWidth2 = cordIntersectPoint.GetDistanceFrom(sourceLine.EndPoint);
            diffOffSet = (mainLineWidth1 + mainLineWidth2) - sourceLine.Length;
            if (Math.Abs(diffOffSet) < 0.005d)
            {
                double temp = comparePolygonLine.EndPoint.GetDistanceFrom(cordIntersectPoint);
                if (temp < minDistance.Value)
                    minDistance = temp;
            }

            cordIntersectPoint = FindIntersectionPoint(comparePolygonLine.StartPoint, comparePolygonLine.EndPoint, sourceLine.MidPoint);
            mainLineWidth1 = comparePolygonLine.StartPoint.GetDistanceFrom(cordIntersectPoint);
            mainLineWidth2 = cordIntersectPoint.GetDistanceFrom(comparePolygonLine.EndPoint);
            diffOffSet = (mainLineWidth1 + mainLineWidth2) - comparePolygonLine.Length;
            if (Math.Abs(diffOffSet) < 0.005d)
            {
                double temp = sourceLine.MidPoint.GetDistanceFrom(cordIntersectPoint);
                if (temp < minDistance.Value)
                    minDistance = temp;
            }

            cordIntersectPoint = FindIntersectionPoint(comparePolygonLine.StartPoint, comparePolygonLine.EndPoint, sourceLine.StartPoint);
            mainLineWidth1 = comparePolygonLine.StartPoint.GetDistanceFrom(cordIntersectPoint);
            mainLineWidth2 = cordIntersectPoint.GetDistanceFrom(comparePolygonLine.EndPoint);
            diffOffSet = (mainLineWidth1 + mainLineWidth2) - comparePolygonLine.Length;
            if (Math.Abs(diffOffSet) < ErrorAllowScale)
            {
                double temp = sourceLine.StartPoint.GetDistanceFrom(cordIntersectPoint);
                if (temp < minDistance.Value)
                    minDistance = temp;
            }

            cordIntersectPoint = FindIntersectionPoint(comparePolygonLine.StartPoint, comparePolygonLine.EndPoint, sourceLine.EndPoint);
            mainLineWidth1 = comparePolygonLine.StartPoint.GetDistanceFrom(cordIntersectPoint);
            mainLineWidth2 = cordIntersectPoint.GetDistanceFrom(comparePolygonLine.EndPoint);
            diffOffSet = (mainLineWidth1 + mainLineWidth2) - comparePolygonLine.Length;
            if (Math.Abs(diffOffSet) < ErrorAllowScale)
            {
                double temp = sourceLine.EndPoint.GetDistanceFrom(cordIntersectPoint);
                if (temp < minDistance.Value)
                    minDistance = temp;
            }

            if (minDistance == double.MaxValue)
                minDistance = null;

            return minDistance;
        }

        public DistanceWithCordinate FindMinimumDistanceWithCordsBetweenTwoLines(CLineSegment sourceLine, CLineSegment comparePolygonLine)
        {
            DistanceWithCordinate minDistance = new DistanceWithCordinate();
            minDistance.Distance = double.MaxValue;
            LineOperations objLineOpt = new LineOperations();

            int i = 0;
            while (i < 13)
            {
                i++;

                DistanceWithCordinate dDistance = null;
                if (i == 1)
                    dDistance = objLineOpt.GetPerpendicularDistanceWithCordsFromLineSegment(sourceLine, comparePolygonLine.MidPoint);
                else if (i == 2)
                    dDistance = objLineOpt.GetPerpendicularDistanceWithCordsFromLineSegment(sourceLine, comparePolygonLine.MidPoint);
                else if (i == 3)
                    dDistance = objLineOpt.GetPerpendicularDistanceWithCordsFromLineSegment(comparePolygonLine, sourceLine.StartPoint);
                else if (i == 4)
                    dDistance = objLineOpt.GetPerpendicularDistanceWithCordsFromLineSegment(comparePolygonLine, sourceLine.EndPoint);
                else if (i == 5)
                    dDistance = objLineOpt.GetPerpendicularDistanceWithCordsFromLineSegment(sourceLine, comparePolygonLine.StartPoint);
                else if (i == 6)
                    dDistance = objLineOpt.GetPerpendicularDistanceWithCordsFromLineSegment(sourceLine, comparePolygonLine.EndPoint);
                else if (i == 7)
                {
                    Cordinates cordIntersectPoint = FindIntersectionPoint(sourceLine.StartPoint, sourceLine.EndPoint, comparePolygonLine.MidPoint);
                    if (sourceLine.IsPointOnLine(cordIntersectPoint, ErrorAllowScale))
                    {
                        double temp = comparePolygonLine.MidPoint.GetDistanceFrom(cordIntersectPoint);
                        dDistance = new DistanceWithCordinate { Distance = temp, StartPoint = comparePolygonLine.MidPoint, EndPoint = cordIntersectPoint };
                    }
                }
                else if (i == 8)
                {
                    Cordinates cordIntersectPoint = FindIntersectionPoint(sourceLine.StartPoint, sourceLine.EndPoint, comparePolygonLine.StartPoint);
                    if (sourceLine.IsPointOnLine(cordIntersectPoint, ErrorAllowScale))
                    {
                        double temp = comparePolygonLine.MidPoint.GetDistanceFrom(cordIntersectPoint);
                        dDistance = new DistanceWithCordinate { Distance = temp, StartPoint = comparePolygonLine.StartPoint, EndPoint = cordIntersectPoint };
                    }
                }
                else if (i == 9)
                {
                    Cordinates cordIntersectPoint = FindIntersectionPoint(sourceLine.StartPoint, sourceLine.EndPoint, comparePolygonLine.EndPoint);
                    if (sourceLine.IsPointOnLine(cordIntersectPoint, ErrorAllowScale))
                    {
                        double temp = comparePolygonLine.MidPoint.GetDistanceFrom(cordIntersectPoint);
                        dDistance = new DistanceWithCordinate { Distance = temp, StartPoint = comparePolygonLine.EndPoint, EndPoint = cordIntersectPoint };
                    }
                }
                else if (i == 10)
                {
                    Cordinates cordIntersectPoint = FindIntersectionPoint(comparePolygonLine.StartPoint, comparePolygonLine.EndPoint, sourceLine.MidPoint);
                    if (sourceLine.IsPointOnLine(cordIntersectPoint, ErrorAllowScale))
                    {
                        double temp = comparePolygonLine.MidPoint.GetDistanceFrom(cordIntersectPoint);
                        dDistance = new DistanceWithCordinate { Distance = temp, StartPoint = sourceLine.MidPoint, EndPoint = cordIntersectPoint };
                    }
                }
                else if (i == 11)
                {
                    Cordinates cordIntersectPoint = FindIntersectionPoint(comparePolygonLine.StartPoint, comparePolygonLine.EndPoint, sourceLine.StartPoint);
                    if (sourceLine.IsPointOnLine(cordIntersectPoint, ErrorAllowScale))
                    {
                        double temp = comparePolygonLine.MidPoint.GetDistanceFrom(cordIntersectPoint);
                        dDistance = new DistanceWithCordinate { Distance = temp, StartPoint = sourceLine.StartPoint, EndPoint = cordIntersectPoint };
                    }
                }
                else if (i == 12)
                {
                    Cordinates cordIntersectPoint = FindIntersectionPoint(comparePolygonLine.StartPoint, comparePolygonLine.EndPoint, sourceLine.EndPoint);
                    if (sourceLine.IsPointOnLine(cordIntersectPoint, ErrorAllowScale))
                    {
                        double temp = comparePolygonLine.MidPoint.GetDistanceFrom(cordIntersectPoint);
                        dDistance = new DistanceWithCordinate { Distance = temp, StartPoint = sourceLine.EndPoint, EndPoint = cordIntersectPoint };
                    }
                }

                if (dDistance != null && dDistance.Distance != double.MaxValue && dDistance.Distance != -1)
                {
                    if (minDistance.Distance == double.MaxValue || (minDistance.Distance != double.MaxValue && dDistance.Distance != double.MaxValue && minDistance.Distance > dDistance.Distance))
                        minDistance = dDistance;
                }
            }

            if (minDistance.Distance == double.MaxValue)
                minDistance = null;

            return minDistance;
        }
        public void DistanceForConnectedLineFromGivenReferenceLine(CLineSegment referenceLine, List<CLineSegment> lstLines, ref double? minDistance, ref double? maxDistance)
        {
            minDistance = null;
            maxDistance = null;

            List<CLineSegment> lstConnectedLines = new List<CLineSegment>();
            if (referenceLine != null && lstLines != null && lstLines.Count > 0)
            {
                foreach (CLineSegment line in lstLines)
                {
                    if (line.Equals(referenceLine))
                        continue;

                    if (line.IsConnectedLine(referenceLine))
                    {
                        lstConnectedLines.Add(line);
                    }
                }
            }

            if (lstConnectedLines != null && lstConnectedLines.Count > 0)
            {
                minDistance = lstConnectedLines.Min(x => x.Length);
                maxDistance = lstConnectedLines.Max(x => x.Length);
            }
        }
        public List<LayerInfo> ClearSubItems(List<LayerInfo> lstMasterPolygons, List<LayerInfo> lstSubPolygons)
        {
            if (lstMasterPolygons == null || lstMasterPolygons.Count == 0 || lstSubPolygons == null || lstSubPolygons.Count == 0)
                return lstSubPolygons;

            int iSubCnt = lstSubPolygons.Count;
            int iBigCnt = lstMasterPolygons.Count;
            for (int i = 0; i < iSubCnt; i++)
            {
                LayerInfo subItemInfo = lstSubPolygons[i];
                for (int j = 0; j < iBigCnt; j++)
                {
                    LayerInfo masterItemInfo = lstMasterPolygons[j];
                    if (IsInPolyUsingAngle(masterItemInfo.Data.Coordinates, subItemInfo.Data.Coordinates))
                    {
                        lstSubPolygons[i] = null;
                        break;
                    }
                }
            }

            return lstSubPolygons.Where(x => x != null).ToList();
        }

        public void CalculateAreaOfAllPolygonWhichInsideLayer(ref List<LayerInfo> ListParentInfo, ref List<LayerInfo> lstChildLayerInfo, ref List<LayerInfo> lstArea, double errorAllow = 0)
        {
            if (lstChildLayerInfo == null && lstChildLayerInfo.Count == 0)
                return;

            if (ListParentInfo == null && ListParentInfo.Count == 0)
                return;

            lstArea = new List<LayerInfo>();
            foreach (LayerInfo ParentInfo in ListParentInfo)
            {
                if (ParentInfo == null || ParentInfo.Data == null || ParentInfo.Data.Coordinates == null || ParentInfo.Data.Coordinates.Count == 0)
                    continue;

                for (int iCnt = 0; iCnt < lstChildLayerInfo.Count; iCnt++)
                {
                    if (lstChildLayerInfo[iCnt] == null || lstChildLayerInfo[iCnt].Data == null || lstChildLayerInfo[iCnt].Data.Coordinates == null || lstChildLayerInfo[iCnt].Data.Coordinates.Count == 0)
                        continue;

                    if (IsInPolyUsingAngle(ParentInfo.Data.Coordinates, lstChildLayerInfo[iCnt].Data.Coordinates, errorAllow))
                    {
                        lstArea.Add(lstChildLayerInfo[iCnt]);
                        lstChildLayerInfo[iCnt] = null;
                        //break;
                    }
                }

                lstChildLayerInfo = lstChildLayerInfo.Where(x => x != null).ToList();
            }
        }

        public void CalculateAreaOfAllPolygonWhichInsideLayer(ref LayerInfo ParentInfo, ref List<LayerInfo> lstChildLayerInfo, ref List<LayerInfo> lstArea)
        {
            CalculateAreaOfAllPolygonWhichInsideLayer(ref ParentInfo, ref lstChildLayerInfo, ref lstArea, ErrorAllowScale);
        }
        public void CalculateAreaOfAllPolygonWhichInsideLayer(ref LayerInfo ParentInfo, ref List<LayerInfo> lstChildLayerInfo, ref List<LayerInfo> lstArea, double errorAllow)
        {
            if (lstChildLayerInfo == null && lstChildLayerInfo.Count == 0)
                return;

            if (ParentInfo == null || ParentInfo.Data == null || ParentInfo.Data.Coordinates == null || ParentInfo.Data.Coordinates.Count == 0)
                return;

            lstArea = new List<LayerInfo>();
            for (int iCnt = 0; iCnt < lstChildLayerInfo.Count; iCnt++)
            {
                if (IsInPolyUsingAngle(ParentInfo.Data.Coordinates, lstChildLayerInfo[iCnt].Data.Coordinates, errorAllow))
                {
                    lstArea.Add(lstChildLayerInfo[iCnt]);
                    lstChildLayerInfo[iCnt] = null;
                    //break;
                }
            }

            lstChildLayerInfo = lstChildLayerInfo.Where(x => x != null).ToList();
        }
        public void CalculateAreaOfPolygonCoordinateInsideLayer(ref LayerInfo ParentInfo, ref List<LayerInfo> lstChildLayerInfo, ref List<LayerInfo> lstArea, double errorAllow = 0)
        {
            if (lstChildLayerInfo == null && lstChildLayerInfo.Count == 0)
                return;

            if (ParentInfo == null || ParentInfo.Data == null || ParentInfo.Data.Coordinates == null || ParentInfo.Data.Coordinates.Count == 0)
                return;

            lstArea = new List<LayerInfo>();
            for (int iCnt = 0; iCnt < lstChildLayerInfo.Count; iCnt++)
            {
                if (lstChildLayerInfo[iCnt] == null || lstChildLayerInfo[iCnt].Data == null || lstChildLayerInfo[iCnt].Data.Coordinates == null || lstChildLayerInfo[iCnt].Data.Coordinates.Count == 0)
                    continue;

                if (IsInPolyUsingAngle(ParentInfo.Data.Coordinates, lstChildLayerInfo[iCnt].Data.Coordinates, errorAllow))
                {
                    lstArea.Add(lstChildLayerInfo[iCnt].DeepClone());
                    lstChildLayerInfo[iCnt] = null;
                }
            }

            lstChildLayerInfo = lstChildLayerInfo.Where(x => x != null).ToList();
        }

        public CLineSegment FindBottomBaseLine(LayerInfo layerInfoInput)
        {
            LayerDataWithText layerInfo = layerInfoInput.Data.DeepClone();
            if (layerInfo.HasBulge)
                SetAdjustCoordinate(ref layerInfo);

            return FindBottomBaseLine(layerInfo.Coordinates);
        }

        public CLineSegment FindBottomBaseLine(List<Cordinates> coordinates)
        {
            try
            {
                List<CLineSegment> lines = MakeClosePolyLines(coordinates);
                lines = objLineOperation.MergePolyLineSegments(lines, ref General.TempBufferString);

                double minY = coordinates.Min(x => x.Y);

                CLineSegment baseLine = lines.Where(x => x.StartPoint.Y == minY && x.EndPoint.Y == minY).FirstOrDefault();

                return baseLine;
            }
            catch
            {

            }
            return null;
        }
         
        public void GetBuildingGroundLine(List<LayerDataWithText> lstResult, LayerInfo building, ref CLineSegment grdLine, ref bool IsHeightFromHFL)
        {
            List<LayerDataWithText> lstAllGroundLines = lstResult.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.GroundLevel).ToList();

            //added on 27Jun2022 if ground line not found then check high flood line exists or not
            if (lstAllGroundLines == null || lstAllGroundLines.Count == 0)
            {
                lstAllGroundLines = lstResult.Where(x => x.LayerName.ToLower().Trim() == DxfLayersName.HighFloodLevel).ToList();
                if (lstAllGroundLines != null && lstAllGroundLines.Count > 0)
                    IsHeightFromHFL = true;
            }

            foreach (LayerDataWithText groundLine in lstAllGroundLines)
            {
                if (groundLine == null || groundLine.Coordinates == null || groundLine.Coordinates.Count == 0)
                    continue;

                if (IsInPolyUsingAngle(building.Data.Coordinates, groundLine.Coordinates))
                {
                    grdLine = MakeConnectedLines(groundLine.Coordinates).FirstOrDefault();
                    break;
                }
            }
        }

        /// <summary>
        /// Get stair landing 
        /// </summary>
        /// <param name="BoundryPolygon"></param>
        /// <param name="Groups"></param>
        /// <param name="Output"></param>
        /// <returns></returns>
        public double CalculateTotalArea(List<LayerInfo> lstLayerInfo)
        {
            double dArea = 0d;
            if (lstLayerInfo == null || lstLayerInfo.Count == 0)
                return dArea;

            int iTotal = lstLayerInfo.Count;
            for (int i = 0; i < iTotal; i++)
            {
                LayerInfo layer = lstLayerInfo[i];
                SetAdjustCoordinate(ref layer);
                dArea += FindAreaByCoordinates(layer);
            }

            return FormatFigureInDecimalPoint(dArea);
        }
        public List<Cordinates> GetCoordinatesFromLines(List<CLineSegment> lines)
        {
            List<Cordinates> lstCords = new List<Cordinates>();
            if (lines == null || lines.Count == 0)
                return lstCords;

            bool IsClosePolygon = false;
            //13Apr2023 modified method              
            if (lines[0].StartPoint.Equals(lines[lines.Count - 1].EndPoint))
                IsClosePolygon = true;

            foreach (CLineSegment line in lines)
            {
                lstCords.Add(line.StartPoint);
            }

            //13Apr2023 added condition
            if (IsClosePolygon && lstCords.First().Equals(lstCords.Last()) == false)
                lstCords.Add(lstCords[0]);

            return lstCords;
        }

        public void SetAdjustCoordinate(ref LayerInfo itemElement)
        {
            if (itemElement != null && itemElement.Data != null)
            {
                LayerDataWithText objData = itemElement.Data;
                SetAdjustCoordinate(ref objData);
                itemElement.Data = objData;
            }
        }
        public void SetAdjustCoordinate(ref LayerDataWithText itemElement)
        {
            SetAdjustCoordinate(ref itemElement, 1);
        }
        public void SetAdjustCoordinate(ref LayerDataWithText itemElement, double bulgeLineWidth)
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
                                if (bulgeItem.ItemValue.Bulge == 0 || bulgeItem.ItemValue.Bulge > 500) //handle bulge value 0.0000xxx 30Nov2022
                                {
                                    lstBulgeCord.Add(bulgeValue.StartPoint);
                                    lstBulgeCord.Add(bulgeValue.EndPoint);
                                }
                                else
                                {
                                    ArcSegment arcTmp = new ArcSegment(bulgeItem.ItemValue.StartPoint, bulgeItem.ItemValue.EndPoint, bulgeItem.ItemValue.Bulge);
                                    lstBulgeCord.AddRange(arcTmp.GetArcPoints(bulgeLineWidth));
                                }
                            }
                        }
                        else
                            lstBulgeCord.Add(bulgeValue.StartPoint);

                    } // for each bulge

                    //added on 19Sept2022 for fixed which polygon is not closed needs to close because area calculation goes incorrect
                    if (lstBulgeCord != null && lstBulgeCord.Count > 2 && CheckLineTypeIsCenterLine(itemElement.LineType) == false && itemElement.LayerName.ToLower().Trim().Equals(DxfLayersName.MarginLine) == false)
                    {
                        if (!lstBulgeCord.First().Equals(lstBulgeCord.Last()))
                            lstBulgeCord.Add(lstBulgeCord.First());
                    }
                    if (CheckLineTypeIsCenterLine(itemElement.LineType) == false && itemElement.LayerName.ToLower().Trim().Equals(DxfLayersName.MarginLine) == false && lstBulgeCord.Count() > 2)
                        itemElement.Lines = MakeClosePolyLines(lstBulgeCord);
                    else
                        itemElement.Lines = MakeConnectedLines(lstBulgeCord);
                    
                    itemElement.Coordinates = lstBulgeCord;
                }
                else
                {
                    if (CheckLineTypeIsCenterLine(itemElement.LineType) == false && itemElement.LayerName.ToLower().Trim().Equals(DxfLayersName.MarginLine) == false && itemElement.Coordinates.Count() > 2)
                        itemElement.Lines = MakeClosePolyLines(itemElement.Coordinates);
                    else
                        itemElement.Lines = MakeConnectedLines(itemElement.Coordinates);
                }
            }
        }

 
        //Haversine  formula
        //        var rad = function(x) {
        //  return x* Math.PI / 180;
        //    };

        //    var getDistance = function(p1, p2) {
        //  var R = 6378137; // Earth’s mean radius in meter
        //    var dLat = rad(p2.lat() - p1.lat());
        //    var dLong = rad(p2.lng() - p1.lng());
        //    var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        //      Math.cos(rad(p1.lat())) * Math.cos(rad(p2.lat())) *
        //      Math.sin(dLong / 2) * Math.sin(dLong / 2);
        //    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
        //    var d = R * c;
        //  return d; // returns the distance in meter
        //}

        public CPolygon GetPolygonTranslateResult(List<Cordinates> MainPolygon, List<Cordinates> OtherPolygon, Cordinates MainRedRef, Cordinates OtherRedRef, Cordinates MainBlueRef, Cordinates OtherBlueRef, ref StringBuilder output)
        {

            //-------Initialise and close original polygons
            CPolygon MainPoly = new CPolygon(MainPolygon);
            CPolygon OtherPoly = new CPolygon(OtherPolygon);
            MainPoly.ClosePolygon();
            OtherPoly.ClosePolygon();

            CLineSegment MainPolyReferenceLine = new CLineSegment();
            MainPolyReferenceLine.StartPoint = MainRedRef;
            MainPolyReferenceLine.EndPoint = MainBlueRef;

            CLineSegment OtherPolyReferenceLine = new CLineSegment();
            OtherPolyReferenceLine.StartPoint = OtherRedRef;
            OtherPolyReferenceLine.EndPoint = OtherBlueRef;

            output.AppendLine("---------- Original Coordinates -----------");

            output.AppendLine("\n\n =>Main Polygon\n\n");
            output.AppendLine(MainPoly.PrintPointsRounded(2));
            output.AppendLine("\nReference line : " + MainPolyReferenceLine.PrintLineRounded(2));


            output.AppendLine("\n\n =>Other Polygon\n\n");
            output.AppendLine(OtherPoly.PrintPointsRounded(2));
            output.AppendLine("\nReference line : " + OtherPolyReferenceLine.PrintLineRounded(2));

            //------Translate Other Polygon to Main
            output.AppendLine("---------- Translate 2nd polygon to first polygon -----------");

            double diffX = MainRedRef.X - OtherRedRef.X;
            double diffY = MainRedRef.Y - OtherRedRef.Y;

            List<Cordinates> TranslatedCoordinates = new List<Cordinates>();
            foreach (Cordinates c in OtherPolygon)
            {
                Cordinates NewC = new Cordinates();
                NewC.X = c.X + diffX;
                NewC.Y = c.Y + diffY;
                TranslatedCoordinates.Add(NewC);
            }
            CPolygon OtherPolyTranslated = new CPolygon(TranslatedCoordinates);

            CLineSegment TranslatedOtherPolyReferenceLine = new CLineSegment();
            TranslatedOtherPolyReferenceLine.StartPoint = new Cordinates();
            TranslatedOtherPolyReferenceLine.EndPoint = new Cordinates();
            TranslatedOtherPolyReferenceLine.StartPoint.X = OtherRedRef.X + diffX;
            TranslatedOtherPolyReferenceLine.StartPoint.Y = OtherRedRef.Y + diffY;
            TranslatedOtherPolyReferenceLine.EndPoint.X = OtherBlueRef.X + diffX;
            TranslatedOtherPolyReferenceLine.EndPoint.Y = OtherBlueRef.Y + diffY;


            output.AppendLine("\n\n =>Other Polygon\n\n");
            output.AppendLine(OtherPolyTranslated.PrintPointsRounded(2));
            output.AppendLine("\nReference line : " + TranslatedOtherPolyReferenceLine.PrintLineRounded(2));


            //------ Get Angle
            output.AppendLine("\n\n ---------- Rotation angle -----------\n");


            output.AppendLine("\n\n => Main Polygon \n\n" + MainPolyReferenceLine.PrintLineRounded(2));
            output.AppendLine("\nAngle : " + MainPolyReferenceLine.AngleRelativeToPositiveXAxisDegree);

            output.AppendLine("\n\n => Other Polygon \n\n" + TranslatedOtherPolyReferenceLine.PrintLineRounded(2));
            output.AppendLine("\nAngle : " + TranslatedOtherPolyReferenceLine.AngleRelativeToPositiveXAxisDegree);

            //------ Rotate to set same angle
            double AngleDiff = MainPolyReferenceLine.AngleRelativeToPositiveXAxisRadian - TranslatedOtherPolyReferenceLine.AngleRelativeToPositiveXAxisRadian;
            double AngleDiffDegree = MainPolyReferenceLine.AngleRelativeToPositiveXAxisDegree - TranslatedOtherPolyReferenceLine.AngleRelativeToPositiveXAxisDegree;
            output.AppendLine("\n\n Angle Difference (Degree) : \n" + AngleDiffDegree);
            output.AppendLine("\n\n Angle Difference (Radian) : \n" + AngleDiff);

            output.AppendLine("\n\n ---------- Rotation : Other Polygon -----------\n");

            CPolygon OtherPolyRotated = OtherPolyTranslated.RotateRelativeToPoint(TranslatedOtherPolyReferenceLine.StartPoint, AngleDiff);
            output.AppendLine("\n=>Other Polygon\n\n");
            output.AppendLine(OtherPolyRotated.PrintPointsRounded(2));

            return OtherPolyRotated;

        }
        public void GetMirrorCoordinate(List<Cordinates> OtherPolygon, Cordinates OtherRedRef, Cordinates OtherYellowRef, ref List<Cordinates> MirrorPolygon, ref Cordinates MirrorRedRef, ref Cordinates MirrorYellowRef)
        {

            List<Cordinates> OtherMirror = new List<Cordinates>();
            foreach (Cordinates c in OtherPolygon)
            {
                OtherMirror.Add(GetMirrorPointRefToYAxisCoordinates(c));
            }

            MirrorPolygon = OtherMirror.DeepClone();

            MirrorRedRef = GetMirrorPointRefToYAxisCoordinates(OtherRedRef);

            MirrorYellowRef = GetMirrorPointRefToYAxisCoordinates(OtherYellowRef);
        }
        public Cordinates GetMirrorPointRefToYAxisCoordinates(Cordinates Point)
        {

            Cordinates New = new Cordinates();
            New.X = Point.X * -1;
            New.Y = Point.Y;

            return New;
        }

    } //class
}
