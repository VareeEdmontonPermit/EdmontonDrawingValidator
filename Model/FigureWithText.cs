using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class FigureWithText
    {
        public double value { get; set; }
        public string text { get; set; }
    }
}
