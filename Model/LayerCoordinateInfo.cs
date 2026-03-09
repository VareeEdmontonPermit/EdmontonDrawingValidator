using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EdmontonDrawingValidator.Model;
using SharedClasses;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class LayerCoordinateInfo
    {
        public string LayerName { get; set; } = "";
        public string ColourCode { get; set; } = "";
        public string Command { get; set; } = "";
        public bool IsEntity { get; set; } = false;
        public bool IsBlockBeginEntry { get; set; } = false;
        public string BlockName { get; set; } = "";
        public bool IsBlockElement { get; set; } = false;
        public string ReferenceBlockName { get; set; } = "";
        public bool IsBlockReferenceElement { get; set; } = false;
        public Cordinates BlockReferenceCoordinate { get; set; }
        public Cordinates BlockCoordinate { get; set; }
        public string LineType { get; set; } = "";
        public List<Cordinates> Coordinates { get; set; } = new List<Cordinates>();
        public bool IsCircle { get; set; } = false;
        public double Radius { get; set; }
        public Cordinates CenterPoint { get; set; } = new Cordinates();
        public bool HasBulge { get; set; } = false;
        public List<BulgeItem> CoordinateWithBulge { get; set; } = new List<BulgeItem>();
        public List<double> OnlyBulgeValue { get; set; } = new List<double>();
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
        public double ReferenceRotationAngle { get; set; } = 0d;
        public double XScaling { get; set; } = 1; //41
        public double YScaling { get; set; } = 1; //42
        public double ZScaling { get; set; } = 1;  //43
    }
 }
