using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class FloorMap
    {
        [JsonProperty(Order = 2)]
        public string FloorName { get; set; }

        [JsonProperty(Order = 2)]
        public List<FloorMappingWithFloorInSection> FloorNameMap { get; set; } = new List<FloorMappingWithFloorInSection>();
    }
}
