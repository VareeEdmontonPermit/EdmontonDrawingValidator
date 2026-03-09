using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator.Model
{ 
    [Serializable]
    public sealed class LayerValueInfo
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public double value { get; set; }
        public Dictionary<string, List<LayerValueInfo>> Child { get; set; } = new Dictionary<string, List<LayerValueInfo>>();
    }
}
