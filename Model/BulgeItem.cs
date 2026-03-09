using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;

namespace EdmontonDrawingValidator.Model
{
    [Serializable]
    public sealed class BulgeItem
    {
        public BulgeItemValue ItemValue { get; set; }
        
        public bool IsCoordinateValue { get; set; } = true;  //We can remove as no more use in prorgramming
        public bool IsBulgeValue { get; set; } = false;
    }
}
