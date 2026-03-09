using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace EdmontonDrawingValidator
{
    [Serializable]
    public class ArcSegment
    {
        public Cordinates StartPoint { get; set; }
        public Cordinates EndPoint { get; set; }
        public double Bulge { get; set; }
        public Cordinates CenterPoint { get; set; }
        public Cordinates MidPoint
        {
            get
            {
                double MidPointAngle = (EndAngleRadian + StartAngleRadian) / 2;
                return new Cordinates
                {
                    X = CenterPoint.X + Radius * Math.Cos(MidPointAngle),
                    Y = CenterPoint.Y + Radius * Math.Sin(MidPointAngle),
                };


            }
        }

        public double StartAngleDegree
        {
            get
            {
                return StartAngleRadian * 180 / Math.PI;
            }
        }
        public double StartAngleRadian { get; set; }
        public double EndAngleDegree
        {
            get
            {
                return EndAngleRadian * 180 / Math.PI;
            }
        }
        public double EndAngleRadian { get; set; }

        public double Radius { get; set; }
        public double AngleDegree
        {
            get
            {
                return AngleRadian * 180 / Math.PI;
            }
        }
        public double AngleRadian { get; set; }
        public double Length
        {
            get
            {
                return Math.Abs(Radius * AngleRadian);
            }
        }
        public double CirclePariferry
        {
            get
            {
                return Radius * 2 * Math.PI;
            }
        }
        public double CircleArea
        {
            get
            {
                return Math.PI * Radius * Radius;
            }
        }
        public double SectorArea
        {
            get
            {
                return AngleDegree / 360 * CircleArea;
            }
        }

        //Area of Sector - Area of tringle
        public double BulgeArea
        {
            get
            {
                LineOperations lineOperations = new LineOperations();
                double AreaOfTringle = lineOperations.AreaOfTringle(  StartPoint, EndPoint, CenterPoint);
                return SectorArea - AreaOfTringle;
            }
        }

        //         Angle beteen start and end point(Radian) = 4 Atan(ABS(Bulge) )
        //
        //
        //         Normal = Distance between StartPoint and EndPoint
        //
        //         S = Normal / 2
        //
        //
        //               S* ( 1 - Bulge^2)
        //         D = ----------------------
        //               2 * Bulge
        //
        //         U = (EndPoint.X - StartPoint.X) / Normal
        //         V = (EndPoint.Y - StartPoint.Y) / normal;
        //         
        //         Center Point Coordinates
        //
        //                                       (StartPoint.X + EndPoint.X)
        //         X =  ( -1 * V* D ) + --------------------------------------
        //                                          Normal
        //
        //                               (StartPoint.Y + EndPoint.Y)
        //         Y = (U* D ) + ---------------------------------------
        //                                  2
        //                                  
        //         Radius = Disttance between center point and StartPoint
        public ArcSegment(Cordinates StartPoint, Cordinates EndPoint, double Bulge)
        {
            this.StartPoint = StartPoint;
            this.EndPoint = EndPoint;
            this.Bulge = Bulge;
            AngleRadian = 4 * Math.Atan(Math.Abs(Bulge));
            double normal = StartPoint.GetDistanceFrom(EndPoint);
            double s = normal / 2;
            double d = s * ((1 - Math.Pow(Bulge, 2)) / (2 * Bulge));
            double u = (EndPoint.X - StartPoint.X) / normal;
            double v = (EndPoint.Y - StartPoint.Y) / normal;
            CenterPoint = new Cordinates
            {
                X = -1 * v * d + (StartPoint.X + EndPoint.X) / 2,
                Y = u * d + (StartPoint.Y + EndPoint.Y) / 2
            };
            Radius = CenterPoint.GetDistanceFrom(StartPoint);

            StartAngleRadian = Math.Atan2(StartPoint.Y - CenterPoint.Y, StartPoint.X - CenterPoint.X);
            EndAngleRadian = Math.Atan2(EndPoint.Y - CenterPoint.Y, EndPoint.X - CenterPoint.X);

            if (StartAngleRadian < 0)
            {
                StartAngleRadian += 2 * Math.PI;
            }
            if (EndAngleRadian < 0)
            {
                EndAngleRadian += 2 * Math.PI;
            }
        }

        // Start Point of arc 
        //
        //X = CenterPoint.X + (Radius* Math.Cos(StartAngleRadian))
        // Y = CenterPoint.Y + (Radius* Math.Sin(StartAngleRadian))
        // 
        // End Point of arc
        //
        //
        // X = CenterPoint.X + (Radius* Math.Cos(EndAngleRadian)),
        // Y = CenterPoint.Y + (Radius* Math.Sin(EndAngleRadian)),
        // 
        // Bulge = Math.Tan((AngleRadian / 4));
        public ArcSegment(Cordinates CenterPoint, double Radius, double StartAngleRadian, double EndAngleRadian)
        {
            //CONSIDERING : Start and end angle is always given in anti-clock wise direction i.e 0 degree to 360 degree
            this.CenterPoint = CenterPoint;
            this.Radius = Radius;
            this.StartAngleRadian = StartAngleRadian;
            this.EndAngleRadian = EndAngleRadian;

            AngleRadian = EndAngleRadian - StartAngleRadian;

            if (StartAngleRadian > EndAngleRadian)
            {
                double tmpEnd = EndAngleRadian + 6.28319;
                AngleRadian = tmpEnd - StartAngleRadian;
            }
            StartPoint = new Cordinates
            {
                X = CenterPoint.X + Radius * Math.Cos(StartAngleRadian),
                Y = CenterPoint.Y + Radius * Math.Sin(StartAngleRadian),
            };
            EndPoint = new Cordinates
            {
                X = CenterPoint.X + Radius * Math.Cos(EndAngleRadian),
                Y = CenterPoint.Y + Radius * Math.Sin(EndAngleRadian),
            };
            Bulge = Math.Tan(AngleRadian / 4);
        }
   
        public List<Cordinates> GetArcPoints()
        {
            return GetArcPoints(1);
        }
        public List<Cordinates> GetArcPoints(double lineLength)
        {
            List<Cordinates> points = new List<Cordinates>();
            double StartAngle = StartAngleRadian;
            double EndAngle = EndAngleRadian;


            if (Bulge < 0)
            {
                StartAngle = EndAngleRadian;
                EndAngle = StartAngleRadian;
            }
            else if(Bulge == 0) //added by DAC to avoid bulge value 0 and start and end both diff. then send cordinate
            {
                if(!StartPoint.Equals(EndPoint))
                {
                    return new List<Cordinates> { StartPoint, EndPoint };
                }
            }

            if (StartAngle > EndAngle)
            {
                EndAngle += 6.28319;
            }

            double increment = Math.PI / 180 * 2;//Default 1 degree

            //Decide increment as if we get segment of length : 1
            double length = 10;
            double CurrentDegree = 20;
            while (length > lineLength)
            {
                CurrentDegree = CurrentDegree / 2;
                increment = Math.PI / 180 * CurrentDegree;

                Cordinates tmpStart = new Cordinates
                {
                    X = CenterPoint.X + Radius * Math.Cos(0),
                    Y = CenterPoint.Y + Radius * Math.Sin(0),
                };
                Cordinates tmpEnd = new Cordinates
                {
                    X = CenterPoint.X + Radius * Math.Cos(increment),
                    Y = CenterPoint.Y + Radius * Math.Sin(increment),
                };
                length = tmpStart.GetDistanceFrom(tmpEnd);
            }


            //Get and set points for whole segment at given degree increment
            if (StartAngle < EndAngle)
            {
                for (double angle = StartAngle; angle <= EndAngle; angle += increment)
                {
                    double CalculateAngle = angle;
                    if (angle >= 6.28319)
                    {
                        CalculateAngle = angle - 6.28319;
                    }
                    Cordinates newPoint = new Cordinates
                    {
                        X = CenterPoint.X + Radius * Math.Cos(CalculateAngle),
                        Y = CenterPoint.Y + Radius * Math.Sin(CalculateAngle),
                    };
                    points.Add(newPoint);
                }
            }
            else
            {
                for (double angle = EndAngle; angle <= StartAngle; angle += increment)
                {
                    double CalculateAngle = angle;
                    if (angle >= 6.28319)
                    {
                        CalculateAngle = angle - 6.28319;
                    }
                    Cordinates newPoint = new Cordinates
                    {
                        X = CenterPoint.X + Radius * Math.Cos(CalculateAngle),
                        Y = CenterPoint.Y + Radius * Math.Sin(CalculateAngle),
                    };
                    points.Add(newPoint);
                }
                points.Reverse();
            }

            CPolygon p = new CPolygon(points);

            //If start and end point is toggled, for loop than reset points
            if (Bulge < 0)
            {
                p.ReverseDirection();
            }

            //If start and end points are missing as per increment, add those points
            if (!p.Points.First().Equals(StartPoint))
            {
                p.Points.Insert(0, StartPoint);
            }
            if (!p.Points.Last().Equals(EndPoint))
            {
                p.Points.Add(EndPoint);
            }

            return p.Points;
        }
        public List<CLineSegment> GetArcLineSegments()
        {
            CPolygon p = new CPolygon(GetArcPoints());
            return p.GetBoundaryLineSegments();
        }

    }

}
