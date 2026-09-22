using System.ComponentModel.DataAnnotations;

namespace RoyalVilla.Dto
{
    public class VillaAmenitiesUpdateDto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public int VillaId { get; set; }
    }
}
