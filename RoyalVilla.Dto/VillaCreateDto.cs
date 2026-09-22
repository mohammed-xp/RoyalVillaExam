using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RoyalVilla.Dto
{
    public class VillaCreateDto
    {
        [MaxLength(30)]
        [Required]
        public required string Name { get; set; }
        public string? Details { get; set; }
        public double Rate { get; set; }
        public int Sqft { get; set; }
        public int Occupancy { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? Image { get; set; }
    }
}
