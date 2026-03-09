using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdmontonDrawingValidator.Model;
using SharedClasses;

namespace EdmontonDrawingValidator
{
    [Serializable]
    public sealed class CPolygon {
        public List<Cordinates> Points = new List<Cordinates>();
        public bool IsClosed = false;
        public CPolygon(List<Cordinates> Points) {
            this.Points = Points;
            if (Points != null && Points.Count > 0) {
                IsClosed = Points.First().Equals(Points.Last()) ? true : false;
            }

        }
        public double MinX {
            get {
                return Points.Select(p => p.X).Min();
            }
        }
        public double MaxX {
            get {
                return Points.Select(p => p.X).Max();
            }
        }
        public double MinY {
            get {
                return Points.Select(p => p.Y).Min();
            }
        }
        public double MaxY {
            get {
                return Points.Select(p => p.Y).Max();
            }
        }
        public double Area() {
            double dArea = 0d;
            try {
                if (Points != null && Points.Count > 0) {
                    ClosePolygon();

                    double sum = 0d;
                    for (int i = 0; i < Points.Count - 1; i++) {
                        sum += Points[i].X * Points[i + 1].Y - Points[i].Y * Points[i + 1].X;
                    }
                    dArea = sum / 2d;
                }
            }
            catch { }
            return Math.Abs(Math.Round(dArea, General.NumberOfDecimalPoint));
        }
        public double Periphery() {
            List<CLineSegment> segments = GetBoundaryLineSegments();
            double Periphery = 0d;
            foreach (CLineSegment s in segments) {
                Periphery += s.Length;
            }
            return Periphery;
        }

        //Check if polygon is closed or not
        // if not closed than check and add first point as last point
        public void ClosePolygon() {
            if (Points != null && Points.Count > 0) {
                if (!Points.First().Equals(Points.Last())) {
                    Points.Add(Points.First());
                }
                IsClosed = true;
            }
        }

        //Give total internal angle
        // Total Internal angle = ( No of sides - 2 ) * 180 degree 
        public double TotalInternalAnglesDegree() {
            if (IsClosed) {
                var NoOfSides = Points.Count - 1;
                return (NoOfSides - 2) * 180;
            }
            else {
                var NoOfSides = Points.Count;
                return (NoOfSides - 2) * 180;
            }
        }

        public List<AngleAtPoint> GetInternalAngles() {
            LineOperations objLineOp = new LineOperations();
            List<AngleAtPoint> angles = new List<AngleAtPoint>();

            List<Cordinates> coordinates = new List<Cordinates>();
            coordinates.AddRange(Points);
            if (IsClosed) {
                coordinates.RemoveAt(coordinates.Count - 1);
            }
            for (int i = 0; i < coordinates.Count(); i++) {
                Cordinates AngleAtPoint = coordinates[i];
                Cordinates PreviousPoint = i == 0 ? coordinates.Last() : coordinates[i - 1];
                Cordinates NextPoint = i == coordinates.Count - 1 ? coordinates.First() : coordinates[i + 1];

                AngleAtPoint CurrentPoint = objLineOp.GetAngleAtPoint(NextPoint, PreviousPoint, AngleAtPoint);
                angles.Add(CurrentPoint);
            }
            if (Math.Round(angles.Sum(x => x.InternalAngleDegree)) != TotalInternalAnglesDegree()) {
                foreach (AngleAtPoint p in angles) {
                    p.SwapAngles();
                }
            }
            return angles;
        }

        public List<AngleAtPoint> OldGetInternalAngles() {
            LineOperations objLineOp = new LineOperations();
            List<AngleAtPoint> angles = new List<AngleAtPoint>();

            for (int i = 0; i < Points.Count; i++) {
                if (i == 0 && IsClosed) {
                    continue;
                }
                Cordinates AngleAtPoint = Points[i];
                Cordinates PreviousPoint = i == 0 ? Points.Last() : Points[i - 1];
                Cordinates NextPoint = i == Points.Count - 1 ? Points.First() : Points[i + 1];

                AngleAtPoint CurrentPoint = objLineOp.GetAngleAtPoint(NextPoint, PreviousPoint, AngleAtPoint);
                angles.Add(CurrentPoint);
            }
            if (Math.Round(angles.Sum(x => x.InternalAngleDegree)) != TotalInternalAnglesDegree()) {
                foreach (AngleAtPoint p in angles) {
                    p.SwapAngles();
                }
            }
            return angles;
        }

        public List<CLineSegment> GetBoundaryLineSegments() {
            List<CLineSegment> segments = new List<CLineSegment>();
            for (int i = 0; i < Points.Count - 1; i++) {
                segments.Add(new CLineSegment { StartPoint = Points[i], EndPoint = Points[i + 1] });
            }
            return segments;
        }

        //Check if given point is inside polygon or not
        public bool IsPointInsidePolygon(Cordinates point) {
            List<CLineSegment> segments = GetBoundaryLineSegments();
            //2 lines can not make polygon
            if (segments.Count < 3) {
                return false;
            }
            //Create horizontal line parallel to x axis passing from point P
            CLineSegment ProjectionLine = new CLineSegment { StartPoint = point, EndPoint = new Cordinates { X = 0, Y = point.Y } };

            LineOperations ObjLineOperation = new LineOperations();
            int IntersectionCount = 0;
            foreach (CLineSegment Segment in segments) {
                Cordinates intersectionPoint = ObjLineOperation.GetLineIntersectionPoint(Segment, ProjectionLine);
                if (intersectionPoint.X >= point.X && Segment.IsPointOnLine(intersectionPoint, General.ErrorAllowScale)) {
                    IntersectionCount++;
                }
            }
            return IntersectionCount % 2 == 0 ? false : true;



        }

        //Check if given point reside on boundary of polygon
        public bool IsPointOnBoundary(Cordinates point) {
            List<CLineSegment> segments = GetBoundaryLineSegments();
            //2 lines can not make polygon
            if (segments.Count < 3) {
                return false;
            }

            bool IsOnBoundary = false;
            foreach (CLineSegment Segment in segments) {
                if (Segment.IsPointOnLine(point, General.ErrorAllowScale)) {
                    IsOnBoundary = true;
                    break;
                }
            }
            return IsOnBoundary;
        }

        //Change point sequence into reverse direction
        public void ReverseDirection() {
            Points.Reverse();
        }

        //Shift last point to first point
        public void ShiftPoints() {
            bool CloseAgain = false;
            if (IsClosed && Points.Count > 1) {
                Points.RemoveAt(Points.Count - 1);
                CloseAgain = true;
            }
            Points.Insert(0, Points.Last());
            if (CloseAgain) {
                ClosePolygon();
            }
        }

        //Print polygon points
        public string PrintPoints() {
            string sb = "";
            foreach (Cordinates point in Points) {
                sb += point.PrintPoint() + "\n";
            }
            return sb;
        }
        public string PrintPointsRounded(int round) {
            string sb = "";
            foreach (Cordinates point in Points) {
                sb += point.PrintPointRounded(round) + "\n";
            }
            return sb;
        }
        public void RoundAllPoints(int round) {
            foreach (Cordinates c in Points) {
                c.X = Math.Round(c.X, round);
                c.Y = Math.Round(c.Y, round);
            }
        }

        //Rearrange polygon points in clockwise direction
        public void SetVerticesClockwise() {
            if (!IsVerticesClockwise()) {
                this.ReverseDirection();
            }
        }
        public void SetVerticesAntiClockwise() {
            if (IsVerticesClockwise()) {
                this.ReverseDirection();
            }
        }

        public bool IsVerticesClockwise() {
            //Convert polygon to convex polygon
            List<AngleAtPoint> AnglesAtPoints = GetInternalAngles();
            List<AngleAtPoint> removePoint = AnglesAtPoints.FindAll(x => x.InternalAngleDegree > 180);
            List<Cordinates> newPoints = new List<Cordinates>();
            newPoints.AddRange(this.Points);
            foreach (AngleAtPoint c in removePoint) {
                newPoints.Remove(c.Point);
            }

            //Trapezoidal formula
            double area = 0;
            for (var i = 0; i < (newPoints.Count()); i++) {
                int j = (i + 1) % newPoints.Count();
                area += newPoints[i].X * newPoints[j].Y;
                area -= newPoints[j].X * newPoints[i].Y;
                // console.log(area);
            }
            return (area < 0);
        }

        public CPolygon TranslateToOrigin() {
            List<Cordinates> TranslatedPoints = new List<Cordinates>();
            foreach (Cordinates MyPoint in Points) {
                Cordinates NewPoint = new Cordinates();
                if (MinX >= 0) {
                    NewPoint.X = MyPoint.X - MinX;
                }
                else {
                    NewPoint.X = MyPoint.X + Math.Abs(MinX);
                }
                if (MinY >= 0) {
                    NewPoint.Y = MyPoint.Y - MinY;
                }
                else {
                    NewPoint.Y = MyPoint.Y + Math.Abs(MinY);
                }
                TranslatedPoints.Add(NewPoint);
            }
            return new CPolygon(TranslatedPoints);
        }

        public CPolygon RotateRelativeToPoint(Cordinates ReferencePoint, double Angle) {
            List<Cordinates> TranslatedPoints = new List<Cordinates>();
            foreach (Cordinates MyPoint in Points) {
                Cordinates NewPoint = new Cordinates();
                NewPoint.X = Math.Cos(Angle) * (MyPoint.X - ReferencePoint.X) - Math.Sin(Angle) * (MyPoint.Y - ReferencePoint.Y) + ReferencePoint.X;
                NewPoint.Y = Math.Sin(Angle) * (MyPoint.X - ReferencePoint.X) + Math.Cos(Angle) * (MyPoint.Y - ReferencePoint.Y) + ReferencePoint.Y;

                TranslatedPoints.Add(NewPoint);
            }
            return new CPolygon(TranslatedPoints);
        }

    }
}
