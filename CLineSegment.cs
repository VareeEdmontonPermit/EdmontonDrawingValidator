using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace EdmontonDrawingValidator
{
    [Serializable]
    public class CLineSegment {
        public Cordinates StartPoint { get; set; }

        public Cordinates EndPoint { get; set; }

        public Cordinates MidPoint {
            get {
                return new Cordinates {
                    X = (StartPoint.X + EndPoint.X) / 2d,
                    Y = (StartPoint.Y + EndPoint.Y) / 2d
                };
            }
        } 
        public List<Cordinates> GetCoordinate() {
            List<Cordinates> lstCord = new List<Cordinates>();
            lstCord.Add(StartPoint);
            lstCord.Add(EndPoint);

            return lstCord;
        }

        public override string ToString() {
            return $"Start Point: {StartPoint.X},{StartPoint.Y}, End Point: {EndPoint.X},{EndPoint.Y}, Mid Point: {MidPoint.X},{MidPoint.Y}";
        }
        public bool IsHorizontal {
            get {
                if (Math.Abs(StartPoint.Y - EndPoint.Y) <= General.ErrorAllowScale)
                    return true;
                else
                    return false;
            }
        }

        public bool IsVertical {
            get {
                if (Math.Abs(StartPoint.X - EndPoint.X) <= General.ErrorAllowScale)
                    return true;
                else
                    return false;
            }
        }
        // Startpoint (x1,y1) & endpoint (x2,y2)
        //         (y2 - y1)
        // Slope = -----------
        //         (x2 - x1)
        public double Slope {
            get {
                General objGeneral = new General();
                return objGeneral.FormatFigureInDecimalPoint((EndPoint.Y - StartPoint.Y) / (EndPoint.X - StartPoint.X));
            }
        }

        //            ___________________
        // length =  /       2          2
        //          V (x2-x1)  + (y2-y1)
        public double Length {
            get {
                General objGeneral = new General();

                //remove decimal on 12Sept2022 issue solve in poly AForCosValue different compare to Excel
                return Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2d) + Math.Pow(EndPoint.Y - StartPoint.Y, 2d));

                //return objGeneral.FormatFigureInDecimalPoint(Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2d) + Math.Pow(EndPoint.Y - StartPoint.Y, 2d)));
            }
        }

        //public double LengthWithoutRound
        //{
        //    get
        //    {
        //        General objGeneral = new General();

        //        return Math.Sqrt(Math.Pow(EndPoint.X - StartPoint.X, 2d) + Math.Pow(EndPoint.Y - StartPoint.Y, 2d));
        //    }
        //}

        // Consider line y = mx + c 
        // Convert it into ax + by + c = 0
        public double A {
            get { return Slope; }
        }

        public double C {
            get {
                if (IsVertical) {
                    return StartPoint.X;
                }
                else {
                    return StartPoint.Y - Slope * StartPoint.X;
                }
            }
        }

        public double B {
            get { return -1; }
        }

        // Minimum X for line segment
        public double MinX {
            get {
                return StartPoint.X < EndPoint.X ? StartPoint.X : EndPoint.X;
            }
        }

        // Maximum X for line segment
        public double MaxX {
            get {
                return StartPoint.X > EndPoint.X ? StartPoint.X : EndPoint.X;
            }
        }

        // Minimum Y for line segment
        public double MinY {
            get {
                return StartPoint.Y < EndPoint.Y ? StartPoint.Y : EndPoint.Y;
            }
        }
        // Maximum y for line segment
        public double MaxY {
            get {
                return StartPoint.Y > EndPoint.Y ? StartPoint.Y : EndPoint.Y;
            }
        }

        public bool IsParallel(CLineSegment line2, double dErrorCorrection = 0.0d) {
            if (IsHorizontal && line2.IsHorizontal)
                return true;
            else if (IsVertical && line2.IsVertical)
                return true;
            else if (Slope == line2.Slope)
                return true;
            else if (Math.Abs(Math.Abs(Slope) - Math.Abs(line2.Slope)) <= dErrorCorrection)
                return true;
            else
                return false;
        }
        public bool HasCommonEndPointBetweenLines(CLineSegment line2) {
            return StartPoint.X == line2.StartPoint.X && StartPoint.Y == line2.StartPoint.Y ||
                    EndPoint.X == line2.StartPoint.X && EndPoint.Y == line2.StartPoint.Y ||
                    StartPoint.X == line2.EndPoint.X && StartPoint.Y == line2.EndPoint.Y ||
                    EndPoint.X == line2.EndPoint.X && EndPoint.Y == line2.EndPoint.Y
                    ;
        }

        public Cordinates FindIntersectionPoint(CLineSegment line2) {
            return new Cordinates {
                X = (B * line2.C - line2.B * C) / (A * line2.B - line2.A * B),
                Y = (C * line2.A - line2.C * A) / (A * line2.B - line2.A * B)
            };
        }

        public bool IsPerpendicular(CLineSegment line2) {
            if (IsHorizontal && line2.IsVertical)
                return true;
            else if (IsVertical && line2.IsHorizontal)
                return true;
            else if (Slope == -1d / line2.Slope)
                return true;
            else
                return false;
        }

        // Consider point P as any point
        // Line segment is AB
        //
        // AP + PB = AB
        //
        // AP, PB and AB is length of each segment
        public bool IsPointOnLine(Cordinates cord, double AllowError) {
            double AB = StartPoint.GetDistanceFrom(cord);
            double BP = EndPoint.GetDistanceFrom(cord);

            //change on 14Jun2022
            //if (Math.Round(AB + BP, 2) == this.Length)

            if (Math.Round(AB + BP, 5) == Length) {
                return true;
            }
            else if (Math.Abs(Math.Abs(AB + BP) - Math.Abs(Length)) < AllowError) //General.fBufferScale) //General.fBufferScale) // < 0.0001d && ((d1 + d2) - this.Length) > -0.001d)
            {
                return true;
            }
            else {
                return false;
            }
        }

        //Check is given line is same as current line
        public bool IsSameAs(CLineSegment Segment, int Error = 0) {

            if (StartPoint.Equals(Segment.EndPoint) && EndPoint.Equals(Segment.StartPoint)) {
                return true;
            }
            else if (StartPoint.Equals(Segment.StartPoint) && EndPoint.Equals(Segment.EndPoint)) {
                return true;
            }
            if (StartPoint.Equals(Segment.EndPoint, Error) && EndPoint.Equals(Segment.StartPoint, Error)) {
                return true;
            }
            else if (StartPoint.Equals(Segment.StartPoint, Error) && EndPoint.Equals(Segment.EndPoint, Error)) {
                return true;
            }
            else {
                return false;
            }
        }

        //Check if given line has one common point
        public bool HasCommonEndPoint(CLineSegment Segment) {
            if (StartPoint.Equals(Segment.StartPoint) || StartPoint.Equals(Segment.EndPoint)) {
                return true;
            }
            else if (EndPoint.Equals(Segment.StartPoint) || EndPoint.Equals(Segment.EndPoint)) {
                return true;
            }
            else {
                return false;
            }
        }

        public string PrintLine() {
            return "Start Point : " + StartPoint.PrintPoint() + " End Pont : " + EndPoint.PrintPoint();
        }
        public string PrintLineRounded(int Round) {
            return "Start Point : " + StartPoint.PrintPointRounded(Round) + " End Pont : " + EndPoint.PrintPointRounded(Round);
        }

        public List<Cordinates> AddPointsInLineSegment(List<Cordinates> NewPoints) {
            Dictionary<Cordinates, double> DistanceFromStartPoint = new Dictionary<Cordinates, double>();

            foreach (Cordinates point in NewPoints) {
                //Ignore start and end point
                if (IsPointOnLine(point, 0) && !StartPoint.Equals(point) && !EndPoint.Equals(point)) {
                    DistanceFromStartPoint.Add(point, point.GetDistanceFrom(StartPoint));
                }
            }
            DistanceFromStartPoint.OrderBy(x => x.Value);
            NewPoints.Clear();
            foreach (Cordinates point in DistanceFromStartPoint.Keys) {
                NewPoints.Add(point);
            }
            return NewPoints;

        }

        public bool IsConnectedLine(CLineSegment line2) {
            return StartPoint.Equals(line2.StartPoint) || StartPoint.Equals(line2.EndPoint) ||
                    EndPoint.Equals(line2.StartPoint) || EndPoint.Equals(line2.EndPoint);
        }

        public bool IsBothLineConnected(CLineSegment line2) {
            return StartPoint.Equals(line2.StartPoint) || StartPoint.Equals(line2.EndPoint) ||
                    EndPoint.Equals(line2.StartPoint) || EndPoint.Equals(line2.EndPoint);
        }

        public bool Equals(CLineSegment line2) { 
            return
                StartPoint.Equals(line2.StartPoint) && EndPoint.Equals(line2.EndPoint) ||
                StartPoint.Equals(line2.EndPoint) && EndPoint.Equals(line2.StartPoint)
                ;
        }

        public bool IsAlmostZero(double value) {
            return Math.Abs(value) <= General.ErrorAllowScaleForAlmostZero;
        }
        public bool IsLineOverlap(CLineSegment line2) {
            double slope = (EndPoint.Y - StartPoint.Y) / (EndPoint.X - StartPoint.X);
            bool isHorizontal = slope <= General.ErrorAllowScaleForAlmostZero; // AlmostZero(slope);
            bool isDescending = slope < 0 && !isHorizontal;
            double invertY = isDescending || isHorizontal ? -1 : 1;

            Cordinates min1 = new Cordinates { X = Math.Min(StartPoint.X, EndPoint.X), Y = Math.Min(StartPoint.Y * invertY, EndPoint.Y * invertY) };
            Cordinates max1 = new Cordinates { X = Math.Max(StartPoint.X, EndPoint.X), Y = Math.Max(StartPoint.Y * invertY, EndPoint.Y * invertY) };

            Cordinates min2 = new Cordinates { X = Math.Min(line2.StartPoint.X, line2.EndPoint.X), Y = Math.Min(line2.StartPoint.Y * invertY, line2.EndPoint.Y * invertY) };
            Cordinates max2 = new Cordinates { X = Math.Max(line2.StartPoint.X, line2.EndPoint.X), Y = Math.Max(line2.StartPoint.Y * invertY, line2.EndPoint.Y * invertY) };

            Cordinates minIntersection;
            if (isDescending)
                minIntersection = new Cordinates { X = Math.Max(min1.X, min2.X), Y = Math.Min(min1.Y * invertY, min2.Y * invertY) };
            else
                minIntersection = new Cordinates { X = Math.Max(min1.X, min2.X), Y = Math.Max(min1.Y * invertY, min2.Y * invertY) };

            Cordinates maxIntersection;
            if (isDescending)
                maxIntersection = new Cordinates { X = Math.Min(max1.X, max2.X), Y = Math.Max(max1.Y * invertY, max2.Y * invertY) };
            else
                maxIntersection = new Cordinates { X = Math.Min(max1.X, max2.X), Y = Math.Min(max1.Y * invertY, max2.Y * invertY) };

            bool intersect = minIntersection.X <= maxIntersection.X &&
                             (!isDescending && minIntersection.Y <= maxIntersection.Y ||
                               isDescending && minIntersection.Y >= maxIntersection.Y);

            if (!intersect)
                return false;
            else
                return true;
        }
        public double AngleRelativeToPositiveXAxisDegree {
            get {
                double angleInDegrees = AngleRelativeToPositiveXAxisRadian * 180 / Math.PI;
                return angleInDegrees;
            }
        }
        public double AngleRelativeToPositiveXAxisRadian {
            get {
                double deltaY = EndPoint.Y - StartPoint.Y;
                double deltaX = EndPoint.X - StartPoint.X;
                double angleInRad = Math.Atan2(deltaY, deltaX);
                return angleInRad;
            }
        }
    }
}