using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class LayerTextInfo
    {
        public string LayerName { get; set; }
        public string Command { get; set; }
        public string Text { get; set; }
        public string ColourCode { get; set; } = "";
        public string BlockName { get; set; } = "";
        public List<Cordinates> Coordinates { get; set; }
        public List<Cordinates> TextAlignCoordinates { get; set; }

    }
}
