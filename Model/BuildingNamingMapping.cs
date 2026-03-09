using System;
using System.Collections.Generic;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class BuildingNamingMapping
    {
        public string BuildingName { get; set; }
        public List<string> BuildingMapping { get; set; } = new List<string>();
        public List<FloorNamingMapping> FloorMapping { get; set; } = new List<FloorNamingMapping>();
         
    }
}
