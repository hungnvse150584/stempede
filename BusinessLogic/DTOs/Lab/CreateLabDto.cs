using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Lab
{
    public class CreateLabDto
    {
        [Required]
        [MaxLength(255)]
        public string LabName { get; set; }

        public string? Description { get; set; }

        [MaxLength(255)]
        public string? LabFileUrl { get; set; }
    }
}
