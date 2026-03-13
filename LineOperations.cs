using EdmontonDrawingValidator.Model;
using NetTopologySuite.Triangulate;
using SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EdmontonDrawingValidator
{
    public sealed class LineOperations
    {
        //Check if two lines are paraller 
        public bool IsParallel(CLineSegment line1, CLineSegment line2, double AllowError = 0)
        {
            if (line1.IsHorizontal && line2.IsHorizontal)
            {
                return true;
            }
            else if (line1.IsVertical && line2.IsVertical)
            {
                return true;
            }
            else if (line1.Slope == line2.Slope)
            {
                return true;
            }
            else if (Math.Abs(line1.Slope - line2.Slope) <= AllowError)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Check if two lines are perpendicular
        public bool IsPerpendicular(CLineSegment line1, CLineSegment line2)
        {
            if (line1.IsHorizontal && line2.IsVertical)
            {
                return true;
            }
            else if (line1.IsVertical && line2.IsHorizontal)
            {
                return true;
            }
            else if (line1.Slope == -1d / line2.Slope)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public AngleAtPoint FindAngle(CLineSegment line1, CLineSegment line2)
        {
            Cordinates next = new Cordinates(), prev = new Cordinates(), middle = new Cordinates();

            if (line2.IsBothLineConnected(line1))
            {
                if (line1.StartPoint.Equals(line2.EndPoint))
                {
                    middle = line2.EndPoint;
                    next = line2.StartPoint;
                    prev = line1.EndPoint;
                }
                else if (line1.EndPoint.Equals(line2.EndPoint))
                {
                    middle = line2.EndPoint;
                    next = line2.StartPoint;
                    prev = line1.StartPoint;
                }
                else if (line1.StartPoint.Equals(line2.StartPoint))
                {
                    middle = line2.StartPoint;
                    next = line2.EndPoint;
                    prev = line1.EndPoint;
                }
                else if (line1.EndPoint.Equals(line2.StartPoint))
                {
                    middle = line2.StartPoint;
                    next = line2.EndPoint;
                    prev = line1.StartPoint;
                }

                return GetAngleAtPoint(next, prev, middle);

            }

            return null;
        }

        //Return intersection point when two lines are not 
        public Cordinates GetLineIntersectionPoint(CLineSegment line1, CLineSegment line2)
        {
            if (line1.IsVertical)
            {
                //line 1 is vertical i.e x=1
                // so put value of x of line 1 into line 2's equation y = mx + c
                return new Cordinates
                {
                    X = line1.StartPoint.X,
                    Y = line2.Slope * line1.StartPoint.X + line2.C
                };
            }
            else if (line2.IsVertical)
            {
                //line 2 is vertical i.e x=1
                // so put value of x of line 2 into line 1's equation y = mx + c
                return new Cordinates
                {
                    X = line2.StartPoint.X,
                    Y = line1.Slope * line2.StartPoint.X + line1.C
                };
            }
            else
            {
                return new Cordinates
                {
                    X = (line1.B * line2.C - line2.B * line1.C) / (line1.A * line2.B - line2.A * line1.B),
                    Y = (line1.C * line2.A - line2.C * line1.A) / (line1.A * line2.B - line2.A * line1.B)
                };

            }
        }

        //Check if two line segments have common endpoint
        public bool HasCommonEndpoint(CLineSegment line1, CLineSegment line2)
        {
            if (line1.StartPoint.Equals(line2.StartPoint)
                || line1.StartPoint.Equals(line2.EndPoint)
                || line1.EndPoint.Equals(line2.StartPoint)
                || line1.EndPoint.Equals(line2.EndPoint))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Give shortest/perpendicular distance of point from line
        //** doses not consider if intersection point lies on line or not **
        public double GetSortestDistanceFromLine(CLineSegment line, Cordinates point)
        {
            Cordinates IntersectionPoint = GetPerpendicularIntersectionPoint(line, point);
            return IntersectionPoint.GetDistanceFrom(point); //22March2022 // new LineSegment { StartPoint = IntersectionPoint, EndPoint = point }.Length;
        }
        public double GetSortestDistanceBetweenTwoLine(CLineSegment line1, CLineSegment line2)
        {
            if (IsParallel(line1, line2))
            {
                Cordinates IntersectionPoint = GetPerpendicularIntersectionPoint(line1, line2.MidPoint);
                if (line1.IsPointOnLine(IntersectionPoint, General.ErrorAllowScale)) //condition added on 19March it gives 0
                    return IntersectionPoint.GetDistanceFrom(line2.MidPoint);
                else
                {
                    //IntersectionPoint = GetPerpendicularIntersectionPoint(line2, line1.MidPoint);
                    //if (line2.IsPointOnLine(IntersectionPoint, General.ErrorAllowScale)) //condition added on 19March it gives 0
                    //    return IntersectionPoint.GetDistanceFrom(line1.MidPoint);
                    //else
                    return -1;
                }
            }
            else
            {
                double PerpDistanceFromStartPoint = -1;
                double PerpDistanceFromEndPoint = -1;

                //------------Line1's Distance from line 2's Start point
                Cordinates Line2StartToLine1 = GetPerpendicularIntersectionPoint(line1, line2.StartPoint);
                if (line1.IsPointOnLine(Line2StartToLine1, General.ErrorAllowScale))
                {
                    PerpDistanceFromStartPoint = Line2StartToLine1.GetDistanceFrom(line2.StartPoint);
                }

                //------------Line2's Distance from line 1's Start point
                Cordinates Line1StartToLine2 = GetPerpendicularIntersectionPoint(line2, line1.StartPoint);
                if (line2.IsPointOnLine(Line1StartToLine2, General.ErrorAllowScale))
                {
                    double tmp = Line1StartToLine2.GetDistanceFrom(line1.StartPoint);
                    //assign only if distance is unassigned or less than already assigned distance
                    if (PerpDistanceFromStartPoint == -1 || tmp < PerpDistanceFromStartPoint)
                    {
                        PerpDistanceFromStartPoint = tmp;
                    }
                }

                //------------Line1's Distance from line 2's End point
                Cordinates Line2EndToLine1 = GetPerpendicularIntersectionPoint(line1, line2.EndPoint);
                if (line1.IsPointOnLine(Line2EndToLine1, General.ErrorAllowScale))
                {

                    PerpDistanceFromEndPoint = Line2EndToLine1.GetDistanceFrom(line2.EndPoint);
                }

                //------------Line2's Distance from line 1's End point

                Cordinates Line1EndToLine2 = GetPerpendicularIntersectionPoint(line2, line1.EndPoint);
                if (line2.IsPointOnLine(Line1EndToLine2, General.ErrorAllowScale))
                {
                    double tmp = Line1EndToLine2.GetDistanceFrom(line1.EndPoint);
                    //assign only if distance is unassigned or less than already assigned distance
                    if (PerpDistanceFromEndPoint == -1 || tmp < PerpDistanceFromEndPoint)
                    {
                        PerpDistanceFromEndPoint = tmp;
                    }
                }


                if (PerpDistanceFromStartPoint == -1) // No intersection found from start point
                {
                    return PerpDistanceFromEndPoint;
                }
                else if (PerpDistanceFromEndPoint == -1) // No intersection found from end point
                {
                    return PerpDistanceFromStartPoint;
                }
                else  // intersection found from start and end both point, than return minimum 
                {
                    return PerpDistanceFromEndPoint >= PerpDistanceFromStartPoint ? PerpDistanceFromStartPoint : PerpDistanceFromEndPoint;
                }
            }
        }

        public DistanceWithCordinate GetSortestDistanceBetweenTwoLineWithCord(CLineSegment line1, CLineSegment line2)
        {
            if (IsParallel(line1, line2))
            {
                Cordinates IntersectionPoint = GetPerpendicularIntersectionPoint(line1, line2.MidPoint);
                if (line1.IsPointOnLine(IntersectionPoint, General.ErrorAllowScale)) //condition added on 19March it gives 0
                    return new DistanceWithCordinate { StartPoint = line2.MidPoint, EndPoint = IntersectionPoint, Distance = IntersectionPoint.GetDistanceFrom(line2.MidPoint) };
                else
                {
                    return null;
                }
            }
            else
            {
                DistanceWithCordinate PerpDistanceFromStartPoint = null;
                DistanceWithCordinate PerpDistanceFromEndPoint = null;

                //------------Line1's Distance from line 2's Start point
                Cordinates Line2StartToLine1 = GetPerpendicularIntersectionPoint(line1, line2.StartPoint);
                if (line1.IsPointOnLine(Line2StartToLine1, General.ErrorAllowScale))
                {
                    PerpDistanceFromStartPoint = new DistanceWithCordinate { StartPoint = Line2StartToLine1 , EndPoint =line2.StartPoint , Distance = Line2StartToLine1.GetDistanceFrom(line2.StartPoint) };
                }

                //------------Line2's Distance from line 1's Start point
                Cordinates Line1StartToLine2 = GetPerpendicularIntersectionPoint(line2, line1.StartPoint);
                if (line2.IsPointOnLine(Line1StartToLine2, General.ErrorAllowScale))
                {
                    double tmp = Line1StartToLine2.GetDistanceFrom(line1.StartPoint);
                    //assign only if distance is unassigned or less than already assigned distance
                    if (PerpDistanceFromStartPoint == null || tmp < PerpDistanceFromStartPoint.Distance)
                    {
                        PerpDistanceFromStartPoint = new DistanceWithCordinate { StartPoint = Line1StartToLine2, EndPoint = line1.StartPoint, Distance = tmp };
                    }
                }

                //------------Line1's Distance from line 2's End point
                Cordinates Line2EndToLine1 = GetPerpendicularIntersectionPoint(line1, line2.EndPoint);
                if (line1.IsPointOnLine(Line2EndToLine1, General.ErrorAllowScale))
                {

                    PerpDistanceFromEndPoint = new DistanceWithCordinate { StartPoint = Line2EndToLine1, EndPoint = line2.EndPoint, Distance = Line2EndToLine1.GetDistanceFrom(line2.EndPoint) };
                }

                //------------Line2's Distance from line 1's End point

                Cordinates Line1EndToLine2 = GetPerpendicularIntersectionPoint(line2, line1.EndPoint);
                if (line2.IsPointOnLine(Line1EndToLine2, General.ErrorAllowScale))
                {
                    double tmp = Line1EndToLine2.GetDistanceFrom(line1.EndPoint);
                    //assign only if distance is unassigned or less than already assigned distance
                    if (PerpDistanceFromEndPoint == null || tmp < PerpDistanceFromEndPoint.Distance)
                    {
                        PerpDistanceFromEndPoint = new DistanceWithCordinate { StartPoint = Line1EndToLine2, EndPoint = line1.EndPoint , Distance = tmp };
                    }
                }


                if (PerpDistanceFromStartPoint == null) // No intersection found from start point
                {
                    return PerpDistanceFromEndPoint;
                }
                else if (PerpDistanceFromEndPoint == null) // No intersection found from end point
                {
                    return PerpDistanceFromStartPoint;
                }
                else  // intersection found from start and end both point, than return minimum 
                {
                    return PerpDistanceFromEndPoint.Distance >= PerpDistanceFromStartPoint.Distance ? PerpDistanceFromStartPoint : PerpDistanceFromEndPoint;
                }
            }
        }

        //Give perpendicular distance  of point from line segment
        //When intersection point does not lie on line, it return null
        public double? GetPerpendicularDistanceFromLineSegment(CLineSegment line, Cordinates point)
        {
            Cordinates IntersectionPoint = GetPerpendicularIntersectionPoint(line, point);
            if (!line.IsPointOnLine(IntersectionPoint, General.ErrorAllowScale))
                return null;

            return new CLineSegment { StartPoint = IntersectionPoint, EndPoint = point }.Length;

        }

        public DistanceWithCordinate GetPerpendicularDistanceWithCordsFromLineSegment(CLineSegment line, Cordinates point)
        {
            Cordinates IntersectionPoint = GetPerpendicularIntersectionPoint(line, point);
            if (!line.IsPointOnLine(IntersectionPoint, General.ErrorAllowScale))
                return null;

            return new DistanceWithCordinate { StartPoint = IntersectionPoint, EndPoint = point, Distance = new CLineSegment { StartPoint = IntersectionPoint, EndPoint = point }.Length };
        }

        /// <summary>
        /// Consider Line AB and Point P 
        /// Method return perpendicular ( minimum ) distance from P to AB
        /// --------------------------------------------------
        /// For Vertical line >> intersection point (Ax,Py)
        /// --------------------------------------------------
        /// For Horizontal line >> intersection point (Px, Ay)
        /// --------------------------------------------------
        /// for slanting line, we need to create equation for perpendicular line passing from point P
        /// 
        /// y' = m'x' + c' -----> Equation of perpendicular line of AB
        /// 
        /// slope(m') of perpendicular line of AB
        /// 
        ///       -1
        /// m' = -----------
        ///      Slope of AB
        ///      
        /// Find C' for line passing threw Pont P
        /// 
        /// C' = Py - m'Px
        /// 
        /// THis perpendicular line passing from P intersect AB on point AB'
        /// 
        /// apply formula mx - y + c = 0 for line (AB)
        /// a1 = m , b1 = -1, c1 = c
        /// 
        /// apply formula m'x' - y' + c' = 0 for line (PB')
        ///  a2 = m' , b2 = -1, c2 = c'
        ///  
        /// 
        /// B'x = ((b1*c2)-(b2*c1))/((a1*b2)-(a2*b1))
        /// B'y = ((a2*c1)-(a1*c2))/((a1*b2)-(a2*b1))
        /// 
        /// return IntersectionPoint B'(B'x, B'y)
        /// </summary>
        public Cordinates GetPerpendicularIntersectionPoint(CLineSegment AB, Cordinates PointP)
        {
            Cordinates IntersectionPoint = new Cordinates();

            if (AB.IsVertical)
            {
                // if Slope isInfinitive consider intersection point (Ax,Py)
                IntersectionPoint = new Cordinates { X = AB.StartPoint.X, Y = PointP.Y };
            }
            else if (AB.IsHorizontal)
            {
                // if Slope = 0 then intersection point (Px, Ay)
                IntersectionPoint = new Cordinates { X = PointP.X, Y = AB.StartPoint.Y };
            }
            else
            {
                General objGeneral = new General();
                // find m' perpendicular value according AB line for point P, formula m' = 1/ m
                double mDash = objGeneral.FormatFigureInDecimalPoint(-1d / AB.Slope);

                // find C' (consider P) formula c' = Py - m'Px 
                double cDash = objGeneral.FormatFigureInDecimalPoint(PointP.Y - mDash * PointP.X);

                // consider formula mx - y + c = 0 for line (AB)
                // a1 = m , b1 = -1, c1 = c

                // consider formula m'x - y + c = 0 for line (AB)
                //  a2 = m' , b2 = -1, c2 = c'

                // calculate x' = ((b1*c2)-(b2*c1))/((a1*b2)-(a2*b1)) //ref above two lines
                double IntersectionPointX = objGeneral.FormatFigureInDecimalPoint((-1 * cDash - (-1 * AB.C)) / (AB.Slope * -1 - mDash * -1));

                // calculate y' = ((a2*c1)-(a1*c2))/((a1*b2)-(a2*b1))
                double IntersectionPointY = objGeneral.FormatFigureInDecimalPoint((mDash * AB.C - AB.Slope * cDash) / (AB.Slope * -1 - mDash * -1));

                // return IntersectionPoint (x', y')
                IntersectionPoint = new Cordinates { X = IntersectionPointX, Y = IntersectionPointY };
            }

            return IntersectionPoint;
        }

        //Return two angle between three points
        public AngleAtPoint GetAngleAtPoint(Cordinates NextPoint, Cordinates PreviousPoint, Cordinates AngleAtPoint)
        {
            AngleAtPoint CurrentPoint = new AngleAtPoint();
            CurrentPoint.Point = AngleAtPoint;

            double DiffXNext = NextPoint.X - CurrentPoint.Point.X;
            double DiffYNext = NextPoint.Y - CurrentPoint.Point.Y;

            double DiffXPrev = CurrentPoint.Point.X - PreviousPoint.X;
            double DiffYPrev = CurrentPoint.Point.Y - PreviousPoint.Y;

            double Theta1 = Math.Atan2(DiffYNext, DiffXNext) * (180 / Math.PI);
            double Theta2 = Math.Atan2(DiffYPrev, DiffXPrev) * (180 / Math.PI);

            double tmpTheta1 = 180 + Theta1 - Theta2 + 360;
            double tmpTheta2 = 180 + Theta2 - Theta1 + 360;

            while (tmpTheta1 >= 360)
            {
                tmpTheta1 = tmpTheta1 - 360d;
            }
            while (tmpTheta2 >= 360)
            {
                tmpTheta2 = tmpTheta2 - 360d;
            }
            CurrentPoint.InternalAngleDegree = tmpTheta1;
            CurrentPoint.ExternalAngleDegree = tmpTheta2;

            return CurrentPoint;
        }

        // Return Area of tringle made my three point
        // Consider three points of tringle A,B & C
        // Side lengths 
        // a = AB
        // b = AC
        // c = BC
        //
        //       a + b + c
        // s = -------------
        //           2
        //
        //          _________________________________
        // Area =  / s ( s - a ) ( s - b ) ( s - c )
        //        V
        //
        public double AreaOfTringle(Cordinates PointA, Cordinates PointB, Cordinates PointC)
        {
            double a = PointA.GetDistanceFrom(PointB); // Distance AB
            double b = PointA.GetDistanceFrom(PointC); // Distance AC
            double c = PointB.GetDistanceFrom(PointC); // Distance BC

            double s = (a + b + c) / 2;
            double AreaOfTringle = Math.Sqrt(s * (s - a) * (s - b) * (s - c));

            return AreaOfTringle;
        }

        //Return perpendicular line segment of given line passing from given point
        public CLineSegment GetPerpendicularSegmentPassingFromPoint(CLineSegment AB, Cordinates PointP)
        {
            CLineSegment PerpendicularLine = new CLineSegment();
            PerpendicularLine.StartPoint = PointP;
            Cordinates IntersectionPoint = new Cordinates();
            if (AB.IsVertical)
            {
                // if Slope isInfinitive consider intersection point (Ax,Py)
                IntersectionPoint = new Cordinates { X = AB.StartPoint.X, Y = PointP.Y };
                PerpendicularLine.EndPoint = new Cordinates
                {
                    X = 0,
                    Y = PointP.Y
                };
            }
            else if (AB.IsHorizontal)
            {
                // if Slope = 0 then intersection point (Px, Ay)
                IntersectionPoint = new Cordinates { X = PointP.X, Y = AB.StartPoint.Y };
                PerpendicularLine.EndPoint = new Cordinates
                {
                    X = PointP.X,
                    Y = 0
                };
            }
            else
            {
                General objGeneral = new General();
                // find m' perpendicular value according AB line for point P, formula m' = 1/ m
                double mDash = objGeneral.FormatFigureInDecimalPoint(-1d / AB.Slope);

                // find C' (consider P) formula c' = Py - m'Px 
                double cDash = objGeneral.FormatFigureInDecimalPoint(PointP.Y - mDash * PointP.X);

                // consider formula mx - y + c = 0 for line (AB)
                // a1 = m , b1 = -1, c1 = c

                // consider formula m'x - y + c = 0 for line (AB)
                //  a2 = m' , b2 = -1, c2 = c'

                // calculate x' = ((b1*c2)-(b2*c1))/((a1*b2)-(a2*b1)) //ref above two lines
                double IntersectionPointX = objGeneral.FormatFigureInDecimalPoint((-1 * cDash - (-1 * AB.C)) / (AB.Slope * -1 - mDash * -1));

                // calculate y' = ((a2*c1)-(a1*c2))/((a1*b2)-(a2*b1))
                double IntersectionPointY = objGeneral.FormatFigureInDecimalPoint((mDash * AB.C - AB.Slope * cDash) / (AB.Slope * -1 - mDash * -1));

                // return IntersectionPoint (x', y')
                IntersectionPoint = new Cordinates { X = IntersectionPointX, Y = IntersectionPointY };
                PerpendicularLine.EndPoint = new Cordinates
                {
                    X = 0,
                    Y = cDash
                };
            }
            return PerpendicularLine;


        }

        //Check if two lines are overlapping or not
        public bool IsOverlapping(CLineSegment line1, CLineSegment line2)
        {
            if (line1.Length < line2.Length)
            {
                CLineSegment tmp = line1;
                line1 = line2;
                line2 = tmp;
            }

            if (line1.IsPointOnLine(line2.StartPoint, General.ErrorAllowScale) && line1.IsPointOnLine(line2.EndPoint, General.ErrorAllowScale))
                return true;
            else
                return false;

        }

        //Two polygons having minimum 1 common boundary is given
        //reduce other polygon from main polygon and return remaining polygon as result
        public CPolygon IntersectionPolygonHavingCommonBoundary(List<Cordinates> MainPolygon, List<Cordinates> OtherPolygon, ref string output)
        {
            int RoundingApplied = 4;

        RestartWithRound:

            output += "\n All Points rounded : " + RoundingApplied;
            CPolygon MainPoly = new CPolygon(MainPolygon);
            CPolygon OtherPoly = new CPolygon(OtherPolygon);

            MainPoly.RoundAllPoints(RoundingApplied);
            OtherPoly.RoundAllPoints(RoundingApplied);
            //Final result polygon
            CPolygon ResultPolygon = new CPolygon(new List<Cordinates>());

            MainPoly.ClosePolygon();
            OtherPoly.ClosePolygon();

            List<CLineSegment> MainPolySegments = MainPoly.GetBoundaryLineSegments();
            List<CLineSegment> OtherPolySegments = OtherPoly.GetBoundaryLineSegments();

            LineOperations objOp = new LineOperations();

            //---------Display polygon area
            output += "\nMain Polygon Area  : " + MainPoly.Area();
            output += "\nOther Polygon Area : " + OtherPoly.Area();

            List<Cordinates> MainPolyWithIntersectionPoints = new List<Cordinates>();
            List<Cordinates> SubPolyWithIntersectionPoints = new List<Cordinates>();

            double ErrorCorrection = 0.0000d;
            int IntersectionCount = 0;
            List<Cordinates> IntersectionPoints = new List<Cordinates>();
            //-----Add intersection points in both polygon
            //----In overlapping boundary, add point to another polygon i.e 
            while (IntersectionCount != 2 && ErrorCorrection < 0.005)
            {
                ErrorCorrection = ErrorCorrection + 0.0001d;
                MainPolyWithIntersectionPoints = new List<Cordinates>();
                SubPolyWithIntersectionPoints = new List<Cordinates>();
                SubPolyWithIntersectionPoints.AddRange(OtherPoly.Points);

                IntersectionCount = 0;

                foreach (CLineSegment main in MainPolySegments)
                {
                    MainPolyWithIntersectionPoints.Add(main.StartPoint);
                    foreach (CLineSegment sub in OtherPolySegments)
                    {
                        //Find and add intersection point in both polygon
                        Cordinates intersectionPoint = objOp.GetLineIntersectionPoint(main, sub);
                        intersectionPoint.X = Math.Round(intersectionPoint.X, RoundingApplied);
                        intersectionPoint.Y = Math.Round(intersectionPoint.Y, RoundingApplied);

                        if (main.IsPointOnLine(intersectionPoint, ErrorCorrection) && sub.IsPointOnLine(intersectionPoint, ErrorCorrection))
                        {
                            int CheckWithMinorError = RoundingApplied;
                            if (!intersectionPoint.Equals(main.StartPoint, CheckWithMinorError) && !intersectionPoint.Equals(main.EndPoint, CheckWithMinorError) || !intersectionPoint.Equals(sub.StartPoint, CheckWithMinorError) && !intersectionPoint.Equals(sub.EndPoint, CheckWithMinorError))
                            {
                                IntersectionCount++;
                                output += $"\n Intersection: {intersectionPoint.PrintPointRounded(RoundingApplied)}";
                            }
                            if (!intersectionPoint.Equals(main.StartPoint, CheckWithMinorError) && !intersectionPoint.Equals(main.EndPoint, CheckWithMinorError))
                            {
                                output += $" + main";
                                MainPolyWithIntersectionPoints.Add(intersectionPoint);
                            }
                            if (!intersectionPoint.Equals(sub.StartPoint, CheckWithMinorError) && !intersectionPoint.Equals(sub.EndPoint, CheckWithMinorError))
                            {
                                output += $" + sub";
                                //output += $"\nSub. Intersection: {intersectionPoint.PrintPointRounded(3)}";
                                SubPolyWithIntersectionPoints.Insert(SubPolyWithIntersectionPoints.IndexOf(sub.StartPoint) + 1, intersectionPoint);
                            }
                        }

                        //sub segment lies on main segment
                        else if (main.IsPointOnLine(sub.StartPoint, General.ErrorAllowScaleForBoundary) && main.IsPointOnLine(sub.EndPoint, General.ErrorAllowScaleForBoundary))
                        {

                            List<Cordinates> addPoints = main.AddPointsInLineSegment(new Cordinates[] { sub.StartPoint, sub.EndPoint }.ToList());
                            MainPolyWithIntersectionPoints.AddRange(addPoints);
                        }
                        //main segment lies on sub segment
                        else if (sub.IsPointOnLine(main.StartPoint, General.ErrorAllowScaleForBoundary) && sub.IsPointOnLine(main.EndPoint, General.ErrorAllowScaleForBoundary))
                        {
                            //Add points of main segment in sub polygon
                            List<Cordinates> addPoints = sub.AddPointsInLineSegment(new Cordinates[] { main.StartPoint, main.EndPoint }.ToList());

                            SubPolyWithIntersectionPoints.InsertRange(SubPolyWithIntersectionPoints.IndexOf(sub.StartPoint) + 1, addPoints);
                        }

                    }

                }

                output += "\n Intersection point count with error (" + ErrorCorrection + ") : " + IntersectionCount + "\n\n";

            }
            if (IntersectionCount != 2)
            {
                RoundingApplied--;
                if (RoundingApplied > 0)
                {
                    goto RestartWithRound;
                }
                else
                {
                    return null;
                }
            }
            //initialise and get main 
            MainPoly = new CPolygon(MainPolyWithIntersectionPoints);
            OtherPoly = new CPolygon(SubPolyWithIntersectionPoints);

            MainPoly.ClosePolygon();
            OtherPoly.ClosePolygon();
            MainPolySegments = MainPoly.GetBoundaryLineSegments();
            OtherPolySegments = OtherPoly.GetBoundaryLineSegments();


            //--------Check common boundaries
            List<CLineSegment> CommonBoundaries = new List<CLineSegment>();
            foreach (CLineSegment main in MainPolySegments)
            {
                foreach (CLineSegment sub in OtherPolySegments)
                {
                    if (main.IsSameAs(sub, RoundingApplied))
                    {
                        CommonBoundaries.Add(main);
                        output += "\nCommon Boundary : " + main.PrintLine();
                    }

                }
            }

            //When common boundaries found, check if those boundaries are in same sequence in both polygon
            if (CommonBoundaries.Count > 0)
            {
                while (CommonBoundaries.Last().EndPoint.Equals(CommonBoundaries.First().StartPoint))
                {
                    CLineSegment tmp = CommonBoundaries.Last();
                    CommonBoundaries.Insert(0, tmp);
                    CommonBoundaries.RemoveAt(CommonBoundaries.Count - 1);
                }
                while (!MainPoly.Points.First().Equals(CommonBoundaries.First().StartPoint))
                {
                    MainPoly.ShiftPoints();
                }
                MainPoly.ShiftPoints();
                //Shift points until start point is same boundary
                while (!OtherPoly.Points.First().Equals(CommonBoundaries.First().StartPoint))
                {
                    OtherPoly.ShiftPoints();
                }
                //if first segment of other polygon is not common boundary than reverse the points to get same direction
                if (!OtherPoly.Points[1].Equals(CommonBoundaries.First().EndPoint))
                {
                    OtherPoly.ReverseDirection();
                }

                //move common boundary to 2nd position, so intersection point will found on 1st position
                OtherPoly.ShiftPoints();
            }

            output += "\n-----Converted 1st polygon as per required direction";
            output += "\n" + MainPoly.PrintPoints();
            output += "\n-----Converted 2nd polygon as per required direction";
            output += "\n" + OtherPoly.PrintPoints();

            //----------Other polygon's boundary points inside main plot
            List<Cordinates> insidePoints = new List<Cordinates>();
            bool isFirst = true;
            foreach (Cordinates c in OtherPoly.Points)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }
                if (MainPoly.IsPointInsidePolygon(c) && !MainPoly.IsPointOnBoundary(c))
                {
                    if (insidePoints.FindIndex(x => x.X == c.X && x.Y == c.Y) == -1)
                    {
                        output += "\n inside : " + c.X + " , " + c.Y;
                        insidePoints.Add(c);
                    }
                }
            }

            insidePoints.Reverse();
            OtherPolySegments = OtherPoly.GetBoundaryLineSegments();
            MainPolySegments = MainPoly.GetBoundaryLineSegments();


            //This flag checks weather internal points are added in polygon or not, when added, just finish loop with main polygon
            bool IsCheckIntersection = true;
            //get intersection points and add those points into other poly
            //Start traversing main polygon
            for (int mainBoundaryCount = 0; mainBoundaryCount < MainPolySegments.Count; mainBoundaryCount++)
            {

                CLineSegment main = MainPolySegments[mainBoundaryCount];

                //Add first point of boundary in result polygon
                ResultPolygon.Points.Add(main.StartPoint);

                if (!IsCheckIntersection)
                {
                    continue;
                }

                for (int SubBoundaryCount = 0; SubBoundaryCount < OtherPolySegments.Count; SubBoundaryCount++)
                {
                    CLineSegment sub = OtherPolySegments[SubBoundaryCount];

                    Cordinates inter = new Cordinates();

                    if (objOp.IsOverlapping(main, sub))
                    {
                        inter = sub.StartPoint;
                    }
                    else if (objOp.IsParallel(main, sub))
                    {
                        continue;
                    }
                    else
                    {
                        inter = objOp.GetLineIntersectionPoint(main, sub);
                    }
                    inter.X = Math.Round(inter.X, RoundingApplied);
                    inter.Y = Math.Round(inter.Y, RoundingApplied);

                    if (main.IsPointOnLine(inter, ErrorCorrection) && sub.IsPointOnLine(inter, ErrorCorrection))
                    {
                        output += "\nIntersection found : " + inter.PrintPointRounded(2) + mainBoundaryCount + " to " + SubBoundaryCount;
                        //when first intersection point found, add it to result polygon
                        ResultPolygon.Points.Add(inter);

                        //-----Next boundary will be common lines
                        mainBoundaryCount++;
                        SubBoundaryCount++;
                        bool GoForNextIteration = true;
                        //go to forward direction, to match common edges & reach to intersecting edge
                        while (GoForNextIteration)
                        {
                            if (SubBoundaryCount == OtherPolySegments.Count)
                            {
                                SubBoundaryCount = 0;
                            }
                            CLineSegment CompareMain = MainPolySegments[mainBoundaryCount];
                            CLineSegment CompareSub = OtherPolySegments[SubBoundaryCount];
                            //When both boundary are same
                            if (CompareMain.IsSameAs(CompareSub))
                            {
                                mainBoundaryCount++;
                                SubBoundaryCount++;
                            }
                            //Sub segment lies on main segment
                            else if (CompareMain.IsPointOnLine(CompareSub.StartPoint, General.ErrorAllowScaleForBoundary) && CompareMain.IsPointOnLine(CompareSub.EndPoint, General.ErrorAllowScaleForBoundary))
                            {
                                SubBoundaryCount++;
                            }
                            //main segment lies on sub segment
                            else if (CompareSub.IsPointOnLine(CompareMain.StartPoint, General.ErrorAllowScaleForBoundary) && CompareSub.IsPointOnLine(CompareMain.EndPoint, General.ErrorAllowScaleForBoundary))
                            {
                                mainBoundaryCount++;
                            }
                            else
                            {
                                GoForNextIteration = false;
                            }
                        }

                        //Add inside point into result polygon   
                        ResultPolygon.Points.AddRange(insidePoints);
                        //Add endpoint of current segment after intersection
                        ResultPolygon.Points.Add(MainPolySegments[mainBoundaryCount - 1].EndPoint);
                        IsCheckIntersection = false;
                        break;
                    }
                }
            }
            //--------
            output += "\n\n---------Required poly with intersection point----------\n\n";
            output += ResultPolygon.PrintPoints();
            return ResultPolygon;


        }
        
        public List<CLineSegment> new_MergeCollinearSegments(List<CLineSegment> segments, ref string outstring) //tolerance = 1e-9
        {
            return new_MergeCollinearSegments(segments, 0.05, ref outstring);
        }

        public List<CLineSegment> new_MergeCollinearSegments(List<CLineSegment> segments, double tolerance, ref string outstring) //tolerance = 1e-9
        {
            if (segments.Count < 2) return segments;

            var result = new List<CLineSegment>();
            var current = segments[0];

            for (int i = 1; i < segments.Count; i++)
            {
                var next = segments[i];

                // Direction vectors
                double dx1 = current.EndPoint.X - current.StartPoint.X;
                double dy1 = current.EndPoint.Y - current.StartPoint.Y;
                double dx2 = next.EndPoint.X - next.StartPoint.X;
                double dy2 = next.EndPoint.Y - next.StartPoint.Y;

                // Cross product → checks if parallel
                double cross = dx1 * dy2 - dy1 * dx2;

                // Check if next.StartPoint == current.EndPoint (connected)
                bool connected = Math.Abs(next.StartPoint.X - current.EndPoint.X) < tolerance &&
                                 Math.Abs(next.StartPoint.Y - current.EndPoint.Y) < tolerance;

                if (Math.Abs(cross) < tolerance && connected)
                {
                    // Merge: extend current segment to next.EndPoint
                    current = new CLineSegment { StartPoint = current.StartPoint, EndPoint = next.EndPoint };
                }
                else
                {
                    // Different slope → keep current
                    result.Add(current);
                    current = next;
                }
            }

            // Add last segment
            result.Add(current);

            return result;
        }

        public List<CLineSegment> new_MergePolyLineSegments(List<CLineSegment> lines, ref string Output)
        {
            return new_MergeCollinearSegments(lines, 0.005, ref Output);
        }

        public List<CLineSegment> new_MergePolyLineSegments(List<CLineSegment> lines, double AllowDifference, ref string Output)
        {
            return new_MergeCollinearSegments(lines, AllowDifference, ref Output);
        }

        public List<CLineSegment> MergePolyLineSegments(List<CLineSegment> lines, ref string Output)
        {
            //Output += "\n-----------------------------------------";
            //Output += "\nInput Line Count : " + lines.Count;
            //Output += "\n-----------------------------------------";
            //foreach (CLineSegment Line in lines)
            //{
            //    Output += "\n " + Line.PrintLine();
            //}
            //Output += "\n-----------------------------------------";

            List<CLineSegment> result = new List<CLineSegment>();
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    CLineSegment Newline = VerifyAndMergeLines(lines[i], lines[j]);
                    if (Newline != null)
                    {
                        //Output += "\n" + lines[j].PrintLine() + "------ Merged With Next";
                        lines[i] = Newline;
                        lines[j] = Newline;
                        i = j;
                    }
                    else
                    {
                        break;
                    }
                }
                result.Add(lines[i]);
            }
            lines.Clear();
            lines.AddRange(result);
            result.Clear();

            bool FirstLastMatch = true;
            while (FirstLastMatch && lines.Count > 1)
            {
                CLineSegment NewLine = VerifyAndMergeLines(lines.Last(), lines.First());
                if (NewLine != null)
                {
                    lines[0] = NewLine;
                    //Output += "\n" + lines.Last().PrintLine() + "--Merged With First";
                    lines.Remove(lines.Last());
                }
                else
                {
                    FirstLastMatch = false;
                }
            }
            //Output += "\n-----------------------------------------";
            //Output += "\nOutput Line Count : " + lines.Count;
            //Output += "\n-----------------------------------------";

            //foreach (CLineSegment Line in lines)
            //{
            //    Output += "\n " + Line.PrintLine();
            //}
            return lines;
        }

        public List<CLineSegment> MergePolyLineSegments(List<CLineSegment> lines, double AllowDifference, ref string Output)
        {

            Output += "\n-----------------------------------------";
            Output += "\nInput Line Count : " + lines.Count;
            Output += "\n-----------------------------------------";
            foreach (CLineSegment Line in lines)
            {
                Output += "\n " + Line.PrintLine();
            }
            Output += "\n-----------------------------------------";


            List<CLineSegment> result = new List<CLineSegment>();
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    CLineSegment Newline = VerifyAndMergeLines(lines[i], lines[j], AllowDifference);
                    if (Newline != null)
                    {
                        Output += "\n" + lines[j].PrintLine() + "------ Merged With Next";
                        lines[i] = Newline;
                        lines[j] = Newline;
                        i = j;
                    }
                    else
                    {
                        break;
                    }
                }
                result.Add(lines[i]);
            }
            lines.Clear();
            lines.AddRange(result);
            result.Clear();

            bool FirstLastMatch = true;
            while (FirstLastMatch && lines.Count > 1)
            {
                CLineSegment NewLine = VerifyAndMergeLines(lines.Last(), lines.First(), AllowDifference);
                if (NewLine != null)
                {
                    lines[0] = NewLine;
                    Output += "\n" + lines.Last().PrintLine() + "--Merged With First";
                    lines.Remove(lines.Last());
                }
                else
                {
                    FirstLastMatch = false;
                }
            }
            Output += "\n-----------------------------------------";
            Output += "\nOutput Line Count : " + lines.Count;
            Output += "\n-----------------------------------------";

            foreach (CLineSegment Line in lines)
            {
                Output += "\n " + Line.PrintLine();
            }
            return lines;
        }

        /// <summary>
        /// Check if tow lines are parallel and having common endpoint
        /// 
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <param name="line2"></param>
        /// <returns>
        /// if both can be combined, it return single line segment
        /// else return null
        /// </returns>
        public CLineSegment VerifyAndMergeLines(CLineSegment line1, CLineSegment line2, double AllowDifference)
        {
            CLineSegment NewLine = new CLineSegment();
            Cordinates CommonPoint = new Cordinates();
            if (Math.Abs(line1.StartPoint.X - line2.StartPoint.X) <= AllowDifference && Math.Abs(line1.StartPoint.Y - line2.StartPoint.Y) <= AllowDifference)
            {
                CommonPoint = line1.StartPoint;
                NewLine.StartPoint = line1.EndPoint;
                NewLine.EndPoint = line2.EndPoint;
            }
            else if (Math.Abs(line1.StartPoint.X - line2.EndPoint.X) <= AllowDifference && Math.Abs(line1.StartPoint.Y - line2.EndPoint.Y) <= AllowDifference)
            {//line1.StartPoint.Equals(line2.EndPoint)
                CommonPoint = line1.StartPoint;
                NewLine.StartPoint = line1.EndPoint;
                NewLine.EndPoint = line2.StartPoint;

            }
            else if (Math.Abs(line1.EndPoint.X - line2.StartPoint.X) <= AllowDifference && Math.Abs(line1.EndPoint.Y - line2.StartPoint.Y) <= AllowDifference)
            { //(line1.EndPoint.Equals(line2.StartPoint))
                CommonPoint = line1.EndPoint;
                NewLine.StartPoint = line1.StartPoint;
                NewLine.EndPoint = line2.EndPoint;

            }
            else if (Math.Abs(line1.EndPoint.X - line2.EndPoint.X) <= AllowDifference && Math.Abs(line1.EndPoint.Y - line2.EndPoint.Y) <= AllowDifference)
            {  //(line1.EndPoint.Equals(line2.EndPoint))
                CommonPoint = line1.EndPoint;
                NewLine.StartPoint = line1.StartPoint;
                NewLine.EndPoint = line2.StartPoint;
            }
            else
            {
                return null;
            }
            LineOperations lineOperations = new LineOperations();
            if (lineOperations.IsParallel(line1, line2, 0.5) && NewLine.IsPointOnLine(CommonPoint, 0.05))
            {
                return NewLine;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Check if tow lines are parallel and having common endpoint
        /// 
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns>
        /// if both can be combined, it return single line segment
        /// else return null
        /// </returns>
        public CLineSegment VerifyAndMergeLines(CLineSegment line1, CLineSegment line2)
        {
            CLineSegment NewLine = new CLineSegment();
            Cordinates CommonPoint = new Cordinates();
            if (line1.StartPoint.Equals(line2.StartPoint))
            {
                CommonPoint = line1.StartPoint;
                NewLine.StartPoint = line1.EndPoint;
                NewLine.EndPoint = line2.EndPoint;
            }
            else if (line1.StartPoint.Equals(line2.EndPoint))
            {
                CommonPoint = line1.StartPoint;
                NewLine.StartPoint = line1.EndPoint;
                NewLine.EndPoint = line2.StartPoint;

            }
            else if (line1.EndPoint.Equals(line2.StartPoint))
            {
                CommonPoint = line1.EndPoint;
                NewLine.StartPoint = line1.StartPoint;
                NewLine.EndPoint = line2.EndPoint;

            }
            else if (line1.EndPoint.Equals(line2.EndPoint))
            {
                CommonPoint = line1.EndPoint;
                NewLine.StartPoint = line1.StartPoint;
                NewLine.EndPoint = line2.StartPoint;
            }
            else
            {
                return null;
            }
            LineOperations lineOperations = new LineOperations();
            // comment on 15May2024
            //if (lineOperations.IsParallel(line1, line2, 0.5) && NewLine.IsPointOnLine(CommonPoint, 0.05))
            if (lineOperations.IsParallel(line1, line2, 0.005) && NewLine.IsPointOnLine(CommonPoint, 0.05))
            {
                return NewLine;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Remove line with length less than length 1 and reset previous and next line segments start and end point with midpoint
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="Output"></param>
        /// <returns></returns>
        public List<CLineSegment> SmoodhPolyLineSegments(List<CLineSegment> lines, ref string Output)
        {
            List<CLineSegment> segments = new List<CLineSegment>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length < 1)
                {
                    Output += "\n " + lines[i].PrintLine() + " ----- Removed (Smoodh) " + lines[i].Length;
                    if (i == 0)
                    {
                        lines[lines.Count - 1].EndPoint = lines[i].MidPoint;
                        lines[i + 1].StartPoint = lines[i].MidPoint;
                        lines.RemoveAt(i);
                        i--;
                    }
                    else if (i == lines.Count - 1)
                    {
                        segments[segments.Count - 1].EndPoint = lines[i].MidPoint;
                        segments[0].StartPoint = lines[i].MidPoint;
                    }
                    else
                    {
                        segments[segments.Count - 1].EndPoint = lines[i].MidPoint;
                        lines[i + 1].StartPoint = lines[i].MidPoint;
                    }

                }
                else
                {
                    segments.Add(lines[i]);
                }
            }
            return segments;
        }

        //NEW function
        public List<CLineSegment> New_MergePolyLineSegments(List<CLineSegment> lines, double AllowDifference, ref string Output)
        {
            Output += "\n-----------------------------------------";
            Output += "\nInput Line Count : " + lines.Count;
            Output += "\n-----------------------------------------";
            foreach (CLineSegment line in lines)
            {
                Output += "\n " + line.PrintLine();
            }
            Output += "\n-----------------------------------------";

            List<CLineSegment> result = new List<CLineSegment>();

            int i = 0;
            while (i < lines.Count)
            {
                CLineSegment current = lines[i];
                bool merged = false;

                // Try merging with next lines
                for (int j = i + 1; j < lines.Count; j++)
                {
                    CLineSegment mergedLine = New_VerifyAndMergeLines(current, lines[j], AllowDifference, 0.0);
                    if (mergedLine != null)
                    {
                        Output += "\n" + lines[j].PrintLine() + " ------ Merged With " + current.PrintLine();
                        current = mergedLine;
                        i = j; // skip ahead to merged index
                        merged = true;
                    }
                    else
                    {
                        break; // stop merging further
                    }
                }

                result.Add(current);
                i++;
            }

            // Handle first–last merge
            bool firstLastMerged = true;
            while (firstLastMerged && result.Count > 1)
            {
                CLineSegment mergedLine = New_VerifyAndMergeLines(result.Last(), result.First(), AllowDifference, 0.0);
                if (mergedLine != null)
                {
                    result[0] = mergedLine;
                    Output += "\n" + result.Last().PrintLine() + " -- Merged With First";
                    result.RemoveAt(result.Count - 1);
                }
                else
                {
                    firstLastMerged = false;
                }
            }

            Output += "\n-----------------------------------------";
            Output += "\nOutput Line Count : " + result.Count;
            Output += "\n-----------------------------------------";
            foreach (CLineSegment line in result)
            {
                Output += "\n " + line.PrintLine();
            }

            return result;
        }

        public CLineSegment New_VerifyAndMergeLines(CLineSegment line1, CLineSegment line2, double allowDifference = 0.0, double parallelTolerance = 0.005)
        {
            // Helper to check if two points are "equal" within tolerance
            bool PointsEqual(Cordinates p1, Cordinates p2, double tol) =>
                Math.Abs(p1.X - p2.X) <= tol && Math.Abs(p1.Y - p2.Y) <= tol;

            CLineSegment newLine = null;
            Cordinates commonPoint = null;

            if (PointsEqual(line1.StartPoint, line2.StartPoint, allowDifference))
            {
                commonPoint = line1.StartPoint;
                newLine = new CLineSegment();
                newLine.StartPoint = line1.EndPoint;
                newLine.EndPoint = line2.EndPoint;
            }
            else if (PointsEqual(line1.StartPoint, line2.EndPoint, allowDifference))
            {
                commonPoint = line1.StartPoint;
                newLine = new CLineSegment();
                newLine.StartPoint = line1.EndPoint;
                newLine.EndPoint = line2.StartPoint;
            }
            else if (PointsEqual(line1.EndPoint, line2.StartPoint, allowDifference))
            {
                commonPoint = line1.EndPoint;
                newLine = new CLineSegment();
                newLine.StartPoint = line1.StartPoint;
                newLine.EndPoint = line2.EndPoint;
            }
            else if (PointsEqual(line1.EndPoint, line2.EndPoint, allowDifference))
            {
                commonPoint = line1.EndPoint;
                newLine = new CLineSegment();
                newLine.StartPoint = line1.StartPoint;
                newLine.EndPoint = line2.StartPoint;
            }

            if (newLine == null)
                return null;

            LineOperations lineOps = new LineOperations();
            if (lineOps.IsParallel(line1, line2, parallelTolerance) &&
                newLine.IsPointOnLine(commonPoint, 0.05))
            {
                return newLine;
            }

            return null;
        }

        public CLineSegment New_VerifyAndMergeLines(CLineSegment line1, CLineSegment line2, double allowDifference = 0.0, double angleToleranceDegrees = 2.0, double pointOnLineTolerance = 0.05)
        {
            // Helper: check if two points are "equal" within tolerance
            bool PointsEqual(Cordinates p1, Cordinates p2, double tol) =>
                Math.Abs(p1.X - p2.X) <= tol && Math.Abs(p1.Y - p2.Y) <= tol;

            CLineSegment newLine = null;
            Cordinates commonPoint = null;

            if (PointsEqual(line1.StartPoint, line2.StartPoint, allowDifference))
            {
                commonPoint = line1.StartPoint;
                newLine = new CLineSegment();
                newLine.StartPoint = line1.EndPoint;
                newLine.EndPoint = line2.EndPoint;
            }
            else if (PointsEqual(line1.StartPoint, line2.EndPoint, allowDifference))
            {
                commonPoint = line1.StartPoint;
                newLine = new CLineSegment();
                newLine.StartPoint = line1.EndPoint;
                newLine.EndPoint = line2.StartPoint;
            }
            else if (PointsEqual(line1.EndPoint, line2.StartPoint, allowDifference))
            {
                commonPoint = line1.EndPoint;
                newLine = new CLineSegment();
                newLine.StartPoint = line1.StartPoint;
                newLine.EndPoint = line2.EndPoint;
            }
            else if (PointsEqual(line1.EndPoint, line2.EndPoint, allowDifference))
            {
                commonPoint = line1.EndPoint;
                newLine = new CLineSegment();
                newLine.StartPoint = line1.StartPoint;
                newLine.EndPoint = line2.StartPoint;
            }

            if (newLine == null)
                return null;

            LineOperations lineOps = new LineOperations();

            // Check collinearity instead of just parallel
            double angle1 = lineOps.Angle(line1);
            double angle2 = lineOps.Angle(line2);
            double angleDiff = Math.Abs(angle1 - angle2);

            bool nearlyCollinear = angleDiff <= angleToleranceDegrees ||
                                   Math.Abs(angleDiff - 180.0) <= angleToleranceDegrees;

            if (nearlyCollinear && newLine.IsPointOnLine(commonPoint, pointOnLineTolerance))
            {
                return newLine;
            }

            return null;
        }

        /// <summary>
        /// Returns the angle of the line segment in degrees (0–360).
        /// </summary>
        public double Angle(CLineSegment line)
        {
            double dx = line.EndPoint.X - line.StartPoint.X;
            double dy = line.EndPoint.Y - line.StartPoint.Y;

            // atan2 gives angle in radians between -π and +π
            double angleRadians = Math.Atan2(dy, dx);

            // Convert to degrees
            double angleDegrees = angleRadians * (180.0 / Math.PI);

            // Normalize to 0–360
            if (angleDegrees < 0)
                angleDegrees += 360.0;

            return angleDegrees;
        }


    }

}


