using System;
using System.Collections.Generic;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class BuildingNameWithProposeAndFloorMap
    {
        public string BuildingName { get; set; }
        public List<string> ProposeWorkMap { get; set; } = new List<string>();
        public List<FloorMap> FloorMap { get; set; } = new List<FloorMap>();
    
    }
}
