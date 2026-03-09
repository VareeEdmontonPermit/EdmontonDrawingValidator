using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class BulgeItemValue
    {
        public double Bulge { get; set; }
        public Cordinates StartPoint { get; set; } = new Cordinates();
        public Cordinates EndPoint { get; set; } = new Cordinates();
    }
}
