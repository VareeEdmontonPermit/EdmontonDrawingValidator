using SharedClasses.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EdmontonDrawingValidator.Model
{
    public class UnitWallDoorWindowInformation
    {
        public List<SingleUnitWallDoorWindowInformation> UnitsWall { get; set; } = new List<SingleUnitWallDoorWindowInformation>();
        public double Minimum_Required_Area { get; set; }
        public int GetDecimalCount(double value)
        {
            string[] Splitres = value.ToString().Split('.');
            return (Splitres.Count() > 1)
           ? Splitres.ElementAt(1).Length > 1 ? Splitres.ElementAt(1).Length : ViewConstants.DefaultRounding
           : ViewConstants.DefaultRounding;
        }
        public void RoundVariablesAsRule()
        {
            foreach (SingleUnitWallDoorWindowInformation unit in UnitsWall)
            {
                unit.WallArea = Math.Round(unit.WallArea, ViewConstants.DefaultRounding);
                unit.Door = Math.Round(unit.Door, ViewConstants.DefaultRounding);
                unit.Window = Math.Round(unit.Window, ViewConstants.DefaultRounding); 


            }
        }
        //
        public string IsAllRulesValid
        {
            get
            {
                 
                return RuleTestResult.Complient;
            }
        }


    }
     

    public class SingleUnitWallDoorWindowInformation
    {
        public string LayerName { get; set; } = "";
        public string ElevationName { get; set; } = "";
        public string Name { get; set; } = "";
        public double WallArea { get; set; } = 0;
        public double Door { get; set; } = 0;
        public double Window { get; set; } = 0;

    }

}
