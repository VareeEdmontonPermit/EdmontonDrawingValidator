using System;

namespace EdmontonDrawingValidator.Model
{ 

    [Serializable]
    public sealed class GarageInfo
    {
        public string BuildingName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public string RoomName { get; set; } = "";
        public double RoomArea{ get; set; } = 0;         
        public string Name { get; set; }
        public double Area { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
    }
}
