using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator.Model
{

    [Serializable]
    public sealed class DXFData
    {
        public string Layer { get; set; } = "";
        public string Name { get; set; } = "";
        public string X { get; set; } = "";
        public string Y { get; set; } = "";
    }
}
