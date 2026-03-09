using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EdmontonDrawingValidator.Models.Entities
{
    public partial class TblZoneWiseUse
    {
        public long FldZoneWiseUseId { get; set; }

        [Required]
        [Display(Name = "Zone")]
        public long FldZoneId { get; set; }

        [Required]
        [Display(Name = "Use Category")]
        public string FldUseCategory { get; set; } = null!;

        [Display(Name = "Use Sub-Category")]
        public string? FldUseSubCategory { get; set; }

        [Display(Name = "Actual Use")]
        public string FldActualUse { get; set; } = null!;
    }
}
