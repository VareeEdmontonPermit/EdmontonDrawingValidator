using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class FloorMappingWithFloorInSection
    {
        [JsonProperty(Order = 1)]
        public int FloorInSectionFloorIndex { get; set; }

        [JsonProperty(Order = 2)]
        public string FloorName { get; set; }

        [JsonProperty(Order = 3)]
        public string FloorInSectionFloorName { get; set; }
    }
}
