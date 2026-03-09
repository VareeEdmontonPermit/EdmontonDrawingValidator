using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public class LineSegmentWithMarked : CLineSegment
    {
        public bool IsMarked { get; set; } = false;

    }
}
