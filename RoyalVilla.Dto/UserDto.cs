using System.ComponentModel.DataAnnotations;

namespace RoyalVilla.Dto
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
