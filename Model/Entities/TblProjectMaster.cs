using System;
using System.Collections.Generic;

namespace EdmontonDrawingValidator.Models.Entities
{
    public partial class TblProjectMaster
    {
        public long FldProjectId { get; set; }
        public string FldProjectName { get; set; } = null!;
        public DateTime FldCreationDate { get; set; }
        public long FldZoneId { get; set; }
        public string FldDwgFilePath { get; set; } = null!;
    }
}
