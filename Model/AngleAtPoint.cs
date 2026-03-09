using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace EdmontonDrawingValidator.Model {
    [Serializable]
    public sealed class AngleAtPoint
    {
        public Cordinates Point { get; set; }
        public double InternalAngleRadian { get { return InternalAngleDegree * (Math.PI / 180); } }
        public double ExternalAngleRadian { get { return ExternalAngleDegree * (Math.PI / 180); } }
        public double InternalAngleDegree { get; set; }
        public double ExternalAngleDegree { get; set; }
        public void SwapAngles()
        {
            double tmp = InternalAngleDegree;
            InternalAngleDegree = ExternalAngleDegree;
            ExternalAngleDegree = tmp;
        }
    }
}
