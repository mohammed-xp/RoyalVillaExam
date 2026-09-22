using System.ComponentModel.DataAnnotations;

namespace RoyalVilla_API.Models
{
    public class Villa
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        public string? Details { get; set; }
        public double Rate { get; set; }
        public int Sqft { get; set; }
        public int Occupancy { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<VillaAmenities>? Amenities { get; set; }
    }
}
