using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator.Model
{
    public sealed class DistanceWithLineReference
    {
        public CLineSegment lineTo { get; set; }
        public CLineSegment lineFrom { get; set; }
        public double Distance { get; set; }

    }
}
