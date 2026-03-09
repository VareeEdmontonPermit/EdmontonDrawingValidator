using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class LayerDataWithText
    {
        public string LayerName { get; set; } = "";
        public string ColourCode { get; set; } = "";
        public string Command { get; set; } = "";
        public string LineType { get; set; } = "";

        private List<CLineSegment> _lines = new List<CLineSegment>();
        private List<Cordinates> _coordinates = new List<Cordinates>();
        public List<Cordinates> Coordinates
        {
            get { return _coordinates; }
            set { _coordinates = value; }
        }
        public List<CLineSegment> Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }
        public List<LayerTextInfo> TextInfoData { get; set; } = new List<LayerTextInfo>();
        public bool HasBulge { get; set; } = false;
        public List<BulgeItem> CoordinateWithBulge { get; set; } = new List<BulgeItem>();
        public List<double> OnlyBulgeValue { get; set; } = new List<double>();

        //Typical use for same coordinate or area use for multiple building or floor
        public List<LayerTextInfo> Typical = new List<LayerTextInfo>();

        public bool IsCircle { get; set; } = false;
        public double Radius { get; set; }
        public Cordinates CenterPoint { get; set; }
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
         
    } 
}
