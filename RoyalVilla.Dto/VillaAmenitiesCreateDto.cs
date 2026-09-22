using System.ComponentModel.DataAnnotations;

namespace RoyalVilla.Dto
{
    public class VillaAmenitiesCreateDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public int VillaId { get; set; }
    }
}
