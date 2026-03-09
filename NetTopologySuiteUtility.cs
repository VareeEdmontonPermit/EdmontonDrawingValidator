using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using NetTopologySuite.Mathematics;
using NetTopologySuite.Operation;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Overlay.Snap;
using NetTopologySuite.Operation.Predicate;
using NetTopologySuite.Operation.Relate;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Simplify;
using NetTopologySuite.Utilities;
//using GeoAPI.Geometries;
//using GeoAPI.CoordinateSystems.Transformations;
using SharedClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EdmontonDrawingValidator
{
    public sealed class NetTopologySuiteUtility : MathLib {

        public NetTopologySuiteUtility() {
            NetTopologySuite.NtsGeometryServices.Instance = new NetTopologySuite.NtsGeometryServices(
                                                            // default CoordinateSequenceFactory
                                                            NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
                                                            // default precision model
                                                            new PrecisionModel(GeometryPrecision), //1000d  //update on 05Aug2022
                                                                                                   // default SRID
                                                            4326,
                                                            /********************************************************************
                                                             * Note: the following arguments are only valid for NTS >= v2.2
                                                             ********************************************************************/
                                                            // Geometry overlay operation function set to use (Legacy or NG)
                                                            GeometryOverlay.NG,
                                                            // Coordinate equality comparer to use (CoordinateEqualityComparer or PerOrdinateEqualityComparer)
                                                            new CoordinateEqualityComparer());
        }

        public GeometryFactory XXX_GetFactory() {

            GeometryFactory gf = new GeometryFactory(new PrecisionModel(GeometryPrecision), 4326);

            //GeometryFactory gf = new GeometryFactory(new NetTopologySuite.Geometries.PrecisionModel(General.GeometryPrecision), 4326, NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance);

            //GeometryFactory gf = new GeometryFactory(new NetTopologySuite.Geometries.PrecisionModel(1d), 4326, NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance);

            return gf;
        }

        //LineString IsCoordinate(Coordinate)
        //Returns true if the given point is a vertex of this LineString.
        //public LineSegment(double x0, double y0, double x1, double y1)
        //Angle IsVertical IsHorizontal  Length  MaxX  MaxY  MidPoint MinX MinY
        //ClosestPoint(Coordinate)
        //Computes the closest point on this line segment to another point.
        //ClosestPoints(LineSegment)
        //Computes the closest points on a line segment.
        //Distance(Coordinate)
        //Computes the distance between this line segment and a point.
        //Distance(LineSegment)
        //DistancePerpendicular(Coordinate)//Equals(Object)
        //Intersection(LineSegment)
        //LineIntersection(LineSegment)
        //PointAlong(Double)
        //Computes the Coordinate that lies a given fraction along the line defined by this segment.
        //PointAlongOffset(Double, Double)
        //Computes the Coordinate that lies a given
        //Equality(LineSegment, LineSegment)

        public bool IsCross(List<Cordinates> parentPolygon, List<Cordinates> childPolygon) {
            try {
                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(parentPolygon).ToArray()), XXX_GetFactory());
                Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(childPolygon).ToArray()), XXX_GetFactory());

                return p1.Crosses(p2);
            }
            catch { }
            return false;
        }

        public bool IsOverlaps(List<Cordinates> parentPolygon, List<Cordinates> childPolygon, ref double intersectionAreaValue, ref double wallArea) {
            return IsOverlaps(parentPolygon, childPolygon, ref intersectionAreaValue, ref wallArea, false);
        }

        public bool IsOverlaps(List<Cordinates> parentPolygon, List<Cordinates> childPolygon, ref double intersectionAreaValue, ref double wallArea, bool isLog = false) {
            try 
            {
                if (parentPolygon.First().Equals(parentPolygon.Last()) == false)
                    parentPolygon.Add(parentPolygon.First());

                if (childPolygon.First().Equals(childPolygon.Last()) == false)
                    childPolygon.Add(childPolygon.First());

                var p1 = new Polygon(new LinearRing(ConvertFromCoordinate(parentPolygon).ToArray()), XXX_GetFactory());
                var p2 = new Polygon(new LinearRing(ConvertFromCoordinate(childPolygon).ToArray()), XXX_GetFactory());

                bool resultIntersect = p1.Intersects(p2);
                bool resultOverlap = p1.Overlaps(p2) || p2.Overlaps(p1);
                bool resultCross = p1.Crosses(p2) || p2.Crosses(p1);
                 
                if (resultIntersect)  
                {
                    Geometry unionPoly = p1.Union(p2);
                    double dDiffUnionWithParent = Math.Abs(unionPoly.Area - p1.Area);
                    double dDiffUnionWithChild = Math.Abs(p2.Area - dDiffUnionWithParent);
                     
                    if (dDiffUnionWithParent > ErrorAllowScale)
                    {
                        if (dDiffUnionWithChild > ErrorAllowScale)
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public bool IsIntersects(List<Cordinates> parentPolygon, List<Cordinates> childPolygon, ref double intersectionAreaValue, ref double wallArea) {
            try {
                if (parentPolygon.First().Equals(parentPolygon.Last()) == false)
                    parentPolygon.Add(parentPolygon.First());  

                if (childPolygon.First().Equals(childPolygon.Last()) == false)
                    childPolygon.Add(childPolygon.First());  

                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(parentPolygon).ToArray()), XXX_GetFactory());
                Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(childPolygon).ToArray()), XXX_GetFactory());
                
                if (p1.Intersects(p2)) // || p2.Intersects(p1))
                {
                    var intersectionArea = p1.Intersection(p2);

                    //Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : intersectionArea.OgcGeometryType: " + intersectionArea.OgcGeometryType);

                    //intersectionArea.OgcGeometryType
                    /*
                        ------------------------------------
                        Name 	            Description
                        ------------------------------------
                        CircularString 	    CircularString
                        CompoundCurve 	    CompoundCurve
                        Curve 	            Curve
                        CurvePolygon 	    CurvePolygon
                        GeometryCollection 	GeometryCollection.
                        LineString 	        LineString.
                        MultiCurve 	        MultiCurve
                        MultiLineString 	MultiLineString.
                        MultiPoint          MultiPoint.
                   */

                    //if (intersectionArea.OgcGeometryType == OgcGeometryType.Polygon || intersectionArea.OgcGeometryType == OgcGeometryType.MultiPolygon)
                    {
                        double dValue = 0;
                        //Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Number of Intersection found: " + intersectionArea.NumGeometries);
                        if (intersectionArea.NumGeometries >= 1) // > 1 05Aug2022
                        {
                            //Polygon p3 = new Polygon(new LinearRing(    .Coordinates.ToArray()), XXX_GetFactory());
                            //intersectionAreaValue = intersectionArea.Area;
                            // double intersectionAreaValue1 = p3.Area;
                            for (int i = 0; i < intersectionArea.NumGeometries; i++)
                                dValue += intersectionArea.GetGeometryN(i).Area;
                        }
                        else
                            dValue = intersectionArea.Area;

                        intersectionAreaValue = dValue;
                        wallArea = p2.Area;

                        //Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : wallArea = " + wallArea );
                        //Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Intersection Area Value: " + intersectionAreaValue);

                        //update on 05Aug2022
                        //if (dValue > 0.005d && Math.Round( p1.Area,4) !=  Math.Round(dValue,4))

                        //if (dValue > 0d ) //&& Math.Round(p1.Area, 5) != Math.Round(dValue, 5))


                        //if (dValue > ErrorAllowScale && Math.Round(p2.Area, 4) != Math.Round(dValue, 4))
                        if (dValue > ErrorAllowScaleForIntersection && Math.Round(p2.Area, 4) != Math.Round(dValue, 4))
                            return true;
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Exception on NetTopology utility IsIntersects: " + ex.Message);
            }
            return false;
        }

        public Geometry GetIntersectsArea(List<Cordinates> parentPolygon, List<Cordinates> childPolygon) {
            try {
                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(parentPolygon).ToArray()), XXX_GetFactory());
                Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(childPolygon).ToArray()), XXX_GetFactory());

                return p1.Intersection(p2);
            }
            catch { }
            return null;
        }

        public Envelope GetEnvelope(List<Cordinates> lstCoordinate) {
            try {
                //Envelope obj = new Envelope(ConvertFromCoordinate(lstCoordinate));
                // obj.Init(obj);
                //return obj;
                List<Coordinate> coords = ConvertFromCoordinate(lstCoordinate);
                Envelope env = new Envelope();
                foreach (var coord in coords) {
                    env.ExpandToInclude(coord);
                }
                return env;

            }
            catch { }
            return null;
        }

        public List<Coordinate> ConvertFromCoordinate(List<Cordinates> lstCoordinate) {
            List<Coordinate> cords = new List<Coordinate>();

            int dDecimal = 0;
            if (GeometryPrecision == 1d)
                dDecimal = 0;
            else {
                dDecimal = ("" + GeometryPrecision).Length - 2;
                if (dDecimal < 0)
                    dDecimal = 0;
            }

            foreach (Cordinates cord in lstCoordinate)
                cords.Add(new Coordinate(FormatFigureInDecimalPoint(cord.X, dDecimal), FormatFigureInDecimalPoint(cord.Y, dDecimal))); //


            if (!cords.First().Equals(cords.Last())) //cords[0].X != cords[cords.Count - 1].X || cords[0].Y != cords[cords.Count - 1].Y)
                cords.Add(cords.First());

            return cords;
        }

        public List<Coordinate> ConvertFromCoordinate(List<Cordinates> lstCoordinate, int decimalPoint) {
            List<Coordinate> cords = new List<Coordinate>();

            foreach (Cordinates cord in lstCoordinate)
                cords.Add(new Coordinate(FormatFigureInDecimalPoint(cord.X, decimalPoint), FormatFigureInDecimalPoint(cord.Y, decimalPoint)));

            if (!cords.First().Equals(cords.Last()))
                cords.Add(cords.First());

            return cords;
        }

        public List<Cordinates> ConvertFromCoordinate(List<Coordinate> lstCoordinate) {
            List<Cordinates> cords = new List<Cordinates>();

            foreach (Coordinate cord in lstCoordinate)
                cords.Add(new Cordinates { X = FormatFigureInDecimalPoint(cord.X), Y = FormatFigureInDecimalPoint(cord.Y) }); //

            if (!cords.First().Equals(cords.Last())) //cords[0].X != cords[cords.Count - 1].X || cords[0].Y != cords[cords.Count - 1].Y)
                cords.Add(cords.First());

            return cords;
        }

        public Polygon ConvertPolygonFromCoordinate(List<Coordinate> lstCoordinate) {
            return new Polygon(new LinearRing(lstCoordinate.ToArray()), XXX_GetFactory());
        }

        public Polygon GetUnionPolygon(List<Cordinates> parentPolygon, List<Cordinates> childPolygon) {
            //https://www.webfresh.co.za/2018/08/10/simplifying-spatial-data-while-preserving-topologies-in-c/
            try {
                if (parentPolygon.First().Equals(parentPolygon.Last()) == false)
                    parentPolygon.Add(parentPolygon.First());

                if (childPolygon.First().Equals(childPolygon.Last()) == false)
                    childPolygon.Add(childPolygon.First());

                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(parentPolygon).ToArray()), XXX_GetFactory());
                Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(childPolygon).ToArray()), XXX_GetFactory());

                // Apply small buffer to both polygons to fix topology issues
                var bufferedP1 = p1.Buffer(0.01);
                var bufferedP2 = p2.Buffer(0.01);

                var union = bufferedP1.Union(bufferedP2);
                var union1 = p1.Union(p2);
                if (union1 is Polygon)
                    p1 = (Polygon)union1;
                else if (union is Polygon)
                    p1 = (Polygon)union;
                  
                ////p1.ExteriorRing.Union(p2.ExteriorRing);

                ////// get exterior rings and merge them into a single geometry
                ////var unionedExteriorRings = p1.ExteriorRing.Union(p2.ExteriorRing);

                ////// merge the line strings
                ////var merger = new LineMerger();
                ////merger.Add(unionedExteriorRings);
                ////var mergedLineStrings = merger.GetMergedLineStrings(); 

                ////var geometryCollection = new GeometryCollection(mergedLineStrings.ToArray());


                //return (Polygon)p1.Union(p2);

            }
            catch { }
            return null;
        }
          
        public List<Cordinates> NewGetUnionPolygon(List<List<Cordinates>> lstPolygonCords) {
            //https://www.webfresh.co.za/2018/08/10/simplifying-spatial-data-while-preserving-topologies-in-c/
            try {

                if (lstPolygonCords[0].First().Equals(lstPolygonCords[0].Last()) == false)
                    lstPolygonCords[0].Add(lstPolygonCords[0].First());

                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[0], 4).ToArray()), XXX_GetFactory());

                for (int i = 1; i < lstPolygonCords.Count; i++) {

                    if (lstPolygonCords[i].First().Equals(lstPolygonCords[i].Last()) == false)
                        lstPolygonCords[i].Add(lstPolygonCords[i].First());
                }

                List<Geometry> lstGeometry = new List<Geometry>();
                lstGeometry.Add(p1);
                for (int i = 1; i < lstPolygonCords.Count; i++)
                {
                    Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[i], 4).ToArray()), XXX_GetFactory());
                    lstGeometry.Add(p2);
                } 
                GeometryCollection Gcol = new GeometryCollection(lstGeometry.ToArray());

                Geometry gUnion = Gcol.Union();

                //var lstPolygonCords1 = gUnion.Coordinates;
                List<Coordinate> lstCords = new List<Coordinate>();
                lstCords.AddRange(gUnion.Coordinates);

                return ConvertFromCoordinate(lstCords.ToList());

                //for (int i = 1; i < lstPolygonCords.Count; i++)
                //{
                //    try
                //    {
                //        Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[i], 4).ToArray()), XXX_GetFactory());


                //        //p1.ExteriorRing.Union(p2.ExteriorRing);

                //        //// get exterior rings and merge them into a single geometry
                //        //var unionedExteriorRings = p1.ExteriorRing.Union(p2.ExteriorRing);

                //        //// merge the line strings
                //        //var merger = new LineMerger();
                //        //merger.Add(unionedExteriorRings);
                //        //var mergedLineStrings = merger.GetMergedLineStrings(); 

                //        //var geometryCollection = new GeometryCollection(mergedLineStrings.ToArray());

                //        p1 = (Polygon)p1.Union(p2);
                //    }
                //    catch { }
                //}
                //return ConvertFromCoordinate(p1.Coordinates.ToList());

            }
            catch { }
            return null;
        }

        public List<Cordinates> GetUnionPolygon(List<List<Cordinates>> lstPolygonCords)
        {
            //https://www.webfresh.co.za/2018/08/10/simplifying-spatial-data-while-preserving-topologies-in-c/
            try
            {

                if (lstPolygonCords[0].First().Equals(lstPolygonCords[0].Last()) == false)
                    lstPolygonCords[0].Add(lstPolygonCords[0].First());

                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[0], 4).ToArray()), XXX_GetFactory());
                bool bIsMerge = true;
                while (bIsMerge)
                {
                    bIsMerge = false;
                    lstPolygonCords = lstPolygonCords.Where(x => x != null).ToList();

                    for (int i = 1; i < lstPolygonCords.Count; i++)
                    {
                        try {

                            if (lstPolygonCords[i].First().Equals(lstPolygonCords[i].Last()) == false)
                                lstPolygonCords[i].Add(lstPolygonCords[i].First());

                            Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[i], 4).ToArray()), XXX_GetFactory());

                            // Apply small buffer to both polygons to fix topology issues
                            var bufferedP1 = p1.Buffer(0.01);
                            var bufferedP2 = p2.Buffer(0.01);

                            var union = bufferedP1.Union(bufferedP2);
                            var union1 = p1.Union(p2);
                            if (union1 is Polygon)
                                p1 = (Polygon)union1;
                            else if (union is Polygon)
                                p1 = (Polygon)union;
                            else 
                                p1 = (Polygon)union1;


                            bIsMerge = true;
                            lstPolygonCords[i] = null;

                            //else // it is MultiPolygon
                            //    p1 = null;

                            //if (p1 == null)
                            //return null;

                            //p1.ExteriorRing.Union(p2.ExteriorRing);

                            //// get exterior rings and merge them into a single geometry
                            //var unionExteriorRings = p1.ExteriorRing.Union(p2.ExteriorRing);

                            //// merge the line strings
                            //var merger = new LineMerger();
                            //merger.Add(unionExteriorRings);
                            //var mergedLineStrings = merger.GetMergedLineStrings(); 

                            //var geometryCollection = new GeometryCollection(mergedLineStrings.ToArray());

                            //p1 = (Polygon)p1.Union(p2);
                        }
                        catch { }
                    }
                }

                lstPolygonCords = lstPolygonCords.Where(x => x != null).ToList();
                if (lstPolygonCords.Count() > 1) // first element will not null so condition more than 1
                {
                    //UnMergeDataBlock = lstPolygonCords;
                    return null;
                }
                else
                    return ConvertFromCoordinate(p1.Coordinates.ToList());

                //return ConvertFromCoordinate(p1.Coordinates.ToList());
            }
            catch {
                   
            }
            return null;
        }

        public List<Cordinates> GetUnionPolygonWithoutBuffer(List<List<Cordinates>> lstPolygonCords)
        {
            //https://www.webfresh.co.za/2018/08/10/simplifying-spatial-data-while-preserving-topologies-in-c/
            try
            {

                if (lstPolygonCords[0].First().Equals(lstPolygonCords[0].Last()) == false)
                    lstPolygonCords[0].Add(lstPolygonCords[0].First());

                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[0], 4).ToArray()), XXX_GetFactory());
                bool bIsMerge = true;
                while (bIsMerge)
                {
                    bIsMerge = false;
                    lstPolygonCords = lstPolygonCords.Where(x => x != null).ToList();

                    for (int i = 1; i < lstPolygonCords.Count; i++)
                    {
                        try
                        {
                            if (lstPolygonCords[i].First().Equals(lstPolygonCords[i].Last()) == false)
                                lstPolygonCords[i].Add(lstPolygonCords[i].First());

                            Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[i], 4).ToArray()), XXX_GetFactory());

                            // Apply small buffer to both polygons to fix topology issues
                            //var bufferedP1 = p1.Buffer(0.01);
                            //var bufferedP2 = p2.Buffer(0.01);

                            //var union = bufferedP1.Union(bufferedP2);
                            var union1 = p1.Union(p2);
                            //if (union1 is Polygon)
                            p1 = (Polygon)union1;

                            bIsMerge = true;
                            lstPolygonCords[i] = null;

                            //else
                            //    p1 = null; // union is MultiPolygon or GeometryCollection

                            //if (p1 == null)
                            //    return null;

                            //else if (union is Polygon)
                            //    p1 = (Polygon)union;

                            //p1.ExteriorRing.Union(p2.ExteriorRing);

                            //// get exterior rings and merge them into a single geometry
                            //var unionExteriorRings = p1.ExteriorRing.Union(p2.ExteriorRing);

                            //// merge the line strings
                            //var merger = new LineMerger();
                            //merger.Add(unionExteriorRings);
                            //var mergedLineStrings = merger.GetMergedLineStrings(); 

                            //var geometryCollection = new GeometryCollection(mergedLineStrings.ToArray());

                            //p1 = (Polygon)p1.Union(p2);
                        }
                        catch { }
                    }
                }
                lstPolygonCords = lstPolygonCords.Where(x => x != null).ToList();
                if (lstPolygonCords.Count() > 1) // first element will not null so condition more than 1
                {
                    //UnMergeDataBlock = lstPolygonCords;
                    return null;
                }
                else
                    return ConvertFromCoordinate(p1.Coordinates.ToList());

                //return ConvertFromCoordinate(p1.Coordinates.ToList());
            }
            catch
            {

            }
            return null;
        }

        //Added to which is merge then do it
        public List<Cordinates> GetUnionPolygon2(List<List<Cordinates>> lstPolygonCords, ref List<List<Cordinates>> UnMergeDataBlock)
        {
            //https://www.webfresh.co.za/2018/08/10/simplifying-spatial-data-while-preserving-topologies-in-c/
            try
            {

                if (lstPolygonCords[0].First().Equals(lstPolygonCords[0].Last()) == false)
                    lstPolygonCords[0].Add(lstPolygonCords[0].First());

                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[0], 4).ToArray()), XXX_GetFactory());

                bool bIsMerge = true;
                while (bIsMerge)
                {
                    bIsMerge = false;
                    lstPolygonCords = lstPolygonCords.Where(x => x != null).ToList();
                    for (int i = 1; i < lstPolygonCords.Count; i++)
                    {
                        try
                        {
                            if (lstPolygonCords[i].First().Equals(lstPolygonCords[i].Last()) == false)
                                lstPolygonCords[i].Add(lstPolygonCords[i].First());

                            Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(lstPolygonCords[i], 4).ToArray()), XXX_GetFactory());

                            //p1.ExteriorRing.Union(p2.ExteriorRing);

                            //// get exterior rings and merge them into a single geometry
                            //var unionExteriorRings = p1.ExteriorRing.Union(p2.ExteriorRing);

                            //// merge the line strings
                            //var merger = new LineMerger();
                            //merger.Add(unionExteriorRings);
                            //var mergedLineStrings = merger.GetMergedLineStrings(); 

                            //var geometryCollection = new GeometryCollection(mergedLineStrings.ToArray());

                            p1 = (Polygon)p1.Union(p2);
                            lstPolygonCords[i] = null;
                            bIsMerge = true;
                        }
                        catch { }
                    }
                    lstPolygonCords = lstPolygonCords.Where(x => x != null).ToList();
                }

                lstPolygonCords = lstPolygonCords.Where(x => x != null).ToList();
                if (lstPolygonCords.Count() > 1) // first element will not null so condition more than 1
                {
                    UnMergeDataBlock = lstPolygonCords;
                    return null;
                }
                else
                    return ConvertFromCoordinate(p1.Coordinates.ToList());

            }
            catch { }
            return null;
        }
        public List<Cordinates> GetSubstractPolygon(List<Cordinates> lstParentPolygon, List<Cordinates> childPolygon) {
            //https://www.webfresh.co.za/2018/08/10/simplifying-spatial-data-while-preserving-topologies-in-c/
            try {

                if (lstParentPolygon.First().Equals(lstParentPolygon.Last()) == false)
                    lstParentPolygon.Add(lstParentPolygon.First());

                if (childPolygon.First().Equals(childPolygon.Last()) == false)
                    childPolygon.Add(childPolygon.First());

                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(lstParentPolygon, 4).ToArray()), XXX_GetFactory());
                Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(childPolygon, 4).ToArray()), XXX_GetFactory());

                p1 = (Polygon)p1.Difference(p2);

                return ConvertFromCoordinate(p1.Coordinates.ToList());

            }
            catch { }
            return null;
        }

        public List<Cordinates> GetDiffPolygon(List<Cordinates> lstParentPolygon, List<Cordinates> childPolygon)
        {
            //https://www.webfresh.co.za/2018/08/10/simplifying-spatial-data-while-preserving-topologies-in-c/
            try
            {

                if (lstParentPolygon.First().Equals(lstParentPolygon.Last()) == false)
                    lstParentPolygon.Add(lstParentPolygon.First());

                if (childPolygon.First().Equals(childPolygon.Last()) == false)
                    childPolygon.Add(childPolygon.First());

                Polygon p1 = new Polygon(new LinearRing(ConvertFromCoordinate(lstParentPolygon, 4).ToArray()), XXX_GetFactory());
                Polygon p2 = new Polygon(new LinearRing(ConvertFromCoordinate(childPolygon, 4).ToArray()), XXX_GetFactory());

                Polygon pUnion = (Polygon)p1.Union(p2);

                Geometry g = pUnion.Difference(p2);
                Polygon pfinal=null;
                bool bfirst = true;

                for (int i = 0; i < g.NumGeometries; i++)
                {
                    if (i == 0)
                        pfinal = (Polygon)g.GetGeometryN(i);
                    else  
                        pfinal = (Polygon)g.GetGeometryN(i).Union(pfinal);
                }

                return ConvertFromCoordinate(p1.Coordinates.ToList());

            }
            catch { }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        private void CheckConnectedInteriors(GeometryGraph graph) {
            ConnectedInteriorTester cit = new ConnectedInteriorTester(graph);
            if (!cit.IsInteriorsConnected()) {
                var validErr = new TopologyValidationError(TopologyValidationErrors.DisconnectedInteriors,
                    cit.Coordinate);
            }
        }

        //public static IEnvelope TransformBox(IEnvelope box, IMathTransform transform)
        //{
        //    if (box == null) return null;

        //    double[][] corners = new double[4][];
        //    corners[0] = transform.Transform(new double[] { box.MinX, box.MinY }); //LL
        //    corners[1] = transform.Transform(new double[] { box.MaxX, box.MaxY }); //UR
        //    corners[2] = transform.Transform(new double[] { box.MinX, box.MaxY }); //UL
        //    corners[3] = transform.Transform(new double[] { box.MaxX, box.MinY }); //LR

        //    IEnvelope result = new GeoAPI.Geometries.Envelope();
        //    foreach (double[] p in corners)
        //        result.ExpandToInclude(p[0], p[1]);
        //    return result;
        //}

        /// <summary>
        /// Tests that no hole is nested inside another hole.
        /// This routine assumes that the holes are disjoint.
        /// To ensure this, holes have previously been tested
        /// to ensure that:
        /// They do not partially overlap
        /// (checked by <c>checkRelateConsistency</c>).
        /// They are not identical
        /// (checked by <c>checkRelateConsistency</c>).
        /// </summary>
        private void CheckHolesNotNested(Polygon p, GeometryGraph graph) {
            var nestedTester = new IndexedNestedRingTester(graph);
            foreach (LinearRing innerHole in p.Holes)
                nestedTester.Add(innerHole);
            bool isNonNested = nestedTester.IsNonNested();
            if (!isNonNested) {
                var validErr = new TopologyValidationError(TopologyValidationErrors.NestedHoles,
                    nestedTester.NestedPoint);
            }
        }

        ///// <summary>
        ///// Check if a shell is incorrectly nested within a polygon.  This is the case
        ///// if the shell is inside the polygon shell, but not inside a polygon hole.
        ///// (If the shell is inside a polygon hole, the nesting is valid.)
        ///// The algorithm used relies on the fact that the rings must be properly contained.
        ///// E.g. they cannot partially overlap (this has been previously checked by
        ///// <c>CheckRelateConsistency</c>).
        ///// </summary>
        //private void CheckShellNotNested(LinearRing shell, Polygon p, GeometryGraph graph)
        //{
        //    Coordinate[] shellPts = shell.Coordinates;
        //    // test if shell is inside polygon shell
        //    LinearRing polyShell = p.Shell;
        //    Coordinate[] polyPts = polyShell.Coordinates;
        //    Coordinate shellPt = IsValidOp.FindPointNotNode(shellPts, polyShell, graph);
        //    // if no point could be found, we can assume that the shell is outside the polygon
        //    if (shellPt == null) return;
        //    bool insidePolyShell = CGAlgorithms.IsPointInRing(shellPt, polyPts);
        //    if (!insidePolyShell) return;
        //    // if no holes, this is an error!
        //    if (p.NumInteriorRings <= 0)
        //    {
        //        var validErr1 = new TopologyValidationError(TopologyValidationErrors.NestedShells, shellPt);
        //        return;
        //    }

        //    /*
        //     * Check if the shell is inside one of the holes.
        //     * This is the case if one of the calls to checkShellInsideHole
        //     * returns a null coordinate.
        //     * Otherwise, the shell is not properly contained in a hole, which is an error.
        //     */
        //    Coordinate badNestedPt = null;
        //    for (int i = 0; i < p.NumInteriorRings; i++)
        //    {
        //        LinearRing hole = p.Holes[i];
        //        badNestedPt = IsValidOp.CheckShellInsideHole(shell, hole, graph);
        //        if (badNestedPt == null) return;
        //    }
        //    var validErr = new TopologyValidationError(TopologyValidationErrors.NestedShells, badNestedPt);
        //}

        public void testPredicatesReturnFalseForEmptyGeometries() {
            var p1 = new GeometryFactory().CreatePoint((Coordinate)null);
            var p2 = new GeometryFactory().CreatePoint(new Coordinate(5, 5));
            Assert.IsEquals(false, p1.Equals(p2));
            Assert.IsEquals(true, p1.Disjoint(p2));
            Assert.IsEquals(false, p1.Intersects(p2));
            Assert.IsEquals(false, p1.Touches(p2));
            Assert.IsEquals(false, p1.Crosses(p2));
            Assert.IsEquals(false, p1.Within(p2));
            Assert.IsEquals(false, p1.Contains(p2));
            Assert.IsEquals(false, p1.Overlaps(p2));

            Assert.IsEquals(false, p2.Equals(p1));
            Assert.IsEquals(true, p2.Disjoint(p1));
            Assert.IsEquals(false, p2.Intersects(p1));
            Assert.IsEquals(false, p2.Touches(p1));
            Assert.IsEquals(false, p2.Crosses(p1));
            Assert.IsEquals(false, p2.Within(p1));
            Assert.IsEquals(false, p2.Contains(p1));
            Assert.IsEquals(false, p2.Overlaps(p1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        private void CheckClosedRing(LinearRing ring) {
            if (!ring.IsClosed) {
                var validErr = new TopologyValidationError(TopologyValidationErrors.RingNotClosed,
                    ring.GetCoordinateN(0));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coords"></param>

        public void test(double x, double y, int size, int nPunt) {
            var gsf = new GeometricShapeFactory();
            gsf.Centre = new Coordinate(x, y);
            gsf.Size = size;
            gsf.NumPoints = nPunt;
            var circle = gsf.CreateCircle();
        }
        public Polygon ConvertPolygonFromCoordinate(List<Cordinates> lstCoordinate) {
            List<Coordinate> lstCords = ConvertFromCoordinate(lstCoordinate);
            if (!lstCords[0].Equals2D(lstCords[lstCords.Count - 1])) {
                lstCords.Add(lstCords[0]);
            }

            LinearRing lineRing = new LinearRing(lstCords.ToArray());

            //Geometry line = XXX_GetFactory().CreateLineString(lstCords.ToArray());
            //IsValidOp isValidOp = new IsValidOp(line);
            //bool valid = isValidOp.IsValid;
            var rdr = new WKTReader();
            string sLine = new Polygon(lineRing).AsText();
            return (Polygon)rdr.Read(sLine); //, XXX_GetFactory());

        }
        public double DistanceBetweenGeometries(Geometry geo1, Geometry geo2) {
            double dist = geo1.Distance(geo2);
            // This is the same as doing the following

            //Finding the closest points
            Coordinate[] closestPoints = DistanceOp.NearestPoints(geo1, geo2);

            //Then Running Distance Calculations on the pair of coordinates (or more returned)
            //Something like this
            double distBetweenClosestPoints = DistanceOp.Distance(new Point(closestPoints[0]),
                new Point(closestPoints[1]));

            return dist; // Or closest Points
        }    
        public static Polygon SimplifyToQuadrilateral(Polygon inputPolygon)
        {
            // Step 1: Simplify polygon (reduce tiny segments)
            var simplified = DouglasPeuckerSimplifier.Simplify(inputPolygon, distanceTolerance: 0.5);

            // Step 2: Get minimum bounding rectangle (4 edges)
            var coords = simplified.Coordinates;
            var convexHull = new ConvexHull(coords, new GeometryFactory()).GetConvexHull();

            // Step 3: Force quadrilateral (minimum area rectangle)
            var minRect = MinimumDiameter.GetMinimumRectangle(convexHull);

            return (Polygon)minRect;
        }

        public static Coordinate[] ToNtsCoordinates(List<CLineSegment> lines)
        {
            List<Coordinate> coords = new List<Coordinate>();

            foreach (var line in lines)
            {
                coords.Add(new Coordinate(line.StartPoint.X, line.StartPoint.Y));
            }

            // Ensure polygon is closed (last point == first point)
            if (!coords.First().Equals2D(coords.Last()))
            {
                coords.Add(coords.First());
            }

            return coords.ToArray();
        }

        public static Polygon BuildPolygon(List<CLineSegment> lines)
        {
            var factory = new GeometryFactory();
            var coords = ToNtsCoordinates(lines);
            var linearRing = factory.CreateLinearRing(coords);
            return factory.CreatePolygon(linearRing);
        }

        public static Coordinate ToNtsCoordinate(Cordinates c)
        {
            return new Coordinate(Math.Round(c.X, 4), Math.Round(c.Y, 4));
        }
        public static List<CLineSegment> ToCustomSegments(Polygon polygon)
        {
            var coords = polygon.Coordinates;
            List<CLineSegment> segments = new List<CLineSegment>();

            for (int i = 0; i < coords.Length - 1; i++)
            {
                var start = new Cordinates { X = Math.Round(coords[i].X,4), Y = Math.Round(coords[i].Y,4) };
                var end = new Cordinates { X = Math.Round(coords[i + 1].X,4), Y = Math.Round(coords[i + 1].Y,4) };
                segments.Add(new CLineSegment { StartPoint = start, EndPoint = end });
            }

            return segments;
        }

        public static Cordinates ToCustomCoordinate(Coordinate ntsCoord)
        {
            return new Cordinates
            {
                X = Math.Round(ntsCoord.X, 4),
                Y = Math.Round(ntsCoord.Y,4),
                DXFOriginalX = Math.Round(ntsCoord.X,4), // optional: keep original DXF values
                DXFOriginalY = Math.Round(ntsCoord.Y,4)
            };
        }
        public static List<Cordinates> ToCustomCoordinates(Coordinate[] ntsCoords)
        {
            List<Cordinates> result = new List<Cordinates>();
            foreach (var c in ntsCoords)
            {
                result.Add(ToCustomCoordinate(c));
            }
            return result;
        }



    }
}
