using System;
using System.Collections.Generic;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class FloorNamingMapping
    {
        public string FloorName { get; set; }
        public List<string> FloorMapping { get; set; } = new List<string>();
         
    }
}
