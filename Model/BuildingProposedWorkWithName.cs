using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class BuildingProposedWorkWithName
    {
        public string Name { get; set; }
        public double Area { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }

        public bool IsWithinCommonPlotBoundary { get; set; } = false;

        public string CommonPlotName { get; set; } = "";
    }
}
