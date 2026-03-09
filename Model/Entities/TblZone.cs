using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EdmontonDrawingValidator.Models.Entities
{
    public partial class TblZone
    {
        public long FldZoneId { get; set; }

        [Required]
        [Display(Name = "Primary Zone")]
        public string FldPrimaryZone { get; set; } = null!;

        [Required]
        [Display(Name = "Secondary Zone")]
        public string FldSecondaryZone { get; set; } = null!;

        [Required]
        [Display(Name = "Section")]
        public string FldSection { get; set; } = null!;

        [Required]
        [Display(Name = "Code")]
        public string FldCode { get; set; } = null!;

        [Required]
        [Display(Name = "Name")]
        public string? FldName { get; set; }
    }
}
