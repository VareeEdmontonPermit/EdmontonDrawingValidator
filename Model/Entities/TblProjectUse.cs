using System;
using System.Collections.Generic;

namespace EdmontonDrawingValidator.Models.Entities
{
    public partial class TblProjectUse
    {
        public long TblProjectUseId { get; set; }
        public long FldProjectId { get; set; }
        public long FldZoneWiseUseId { get; set; }
    }
}
