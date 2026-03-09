using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class LayerInfo
    {
        public int Idx { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public LayerDataWithText Data { get; set; }
        public Dictionary<string, List<LayerInfo>> Child { get; set; } = new Dictionary<string, List<LayerInfo>>();
         
    }
}
