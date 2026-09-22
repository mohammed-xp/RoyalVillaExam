using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Models;

namespace RoyalVilla_API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Villa> Villas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<VillaAmenities> VillaAmenities { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Villa>().HasData(
                new Villa
                {
                    Id = 1,
                    Name = "Royal Villa",
                    Details = "This is a luxurious villa with stunning views.",
                    Rate = 500.0,
                    Sqft = 3500,
                    Occupancy = 8,
                    ImageUrl = "https://images.unsplash.com/photo-1580587771525-78b9dba3b914",
                    CreatedAt = new DateTime(2025, 1, 1),
                    UpdatedAt = new DateTime(2025, 1, 1)
                },
                new Villa
                {
                    Id = 2,
                    Name = "Beachside Villa",
                    Details = "A beautiful villa located right on the beach.",
                    Rate = 400.0,
                    Sqft = 3000,
                    Occupancy = 6,
                    ImageUrl = "https://images.unsplash.com/photo-1688653802629-5360086bf632?w=600&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NHx8dmlsbGF8ZW58MHx8MHx8fDA%3D",
                    CreatedAt = new DateTime(2026, 4, 18),
                    UpdatedAt = new DateTime(2026, 4, 18)
                },
                new Villa
                {
                    Id = 3,
                    Name = "Mountain Retreat",
                    Details = "A cozy villa nestled in the mountains.",
                    Rate = 350.0,
                    Sqft = 2500,
                    Occupancy = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=600&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8Nnx8dmlsbGF8ZW58MHx8MHx8fDA%3D",
                    CreatedAt = new DateTime(2026, 7, 4),
                    UpdatedAt = new DateTime(2026, 7, 4)
                },
                new Villa
                {
                    Id = 4,
                    Name = "City Penthouse",
                    Details = "A modern penthouse in the heart of the city.",
                    Rate = 600.0,
                    Sqft = 4000,
                    Occupancy = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1706808849780-7a04fbac83ef?w=600&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MzF8fHZpbGxhfGVufDB8fDB8fHwy",
                    CreatedAt = new DateTime(2026, 9, 10),
                    UpdatedAt = new DateTime(2026, 9, 10)
                },
                new Villa
                {
                    Id = 5,
                    Name = "Countryside Cottage",
                    Details = "A charming cottage in the countryside.",
                    Rate = 300.0,
                    Sqft = 2000,
                    Occupancy = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1670589953903-b4e2f17a70a9?w=600&auto=format&fit=crop&q=60&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxzZWFyY2h8NDV8fHZpbGxhfGVufDB8fDB8fHwy",
                    CreatedAt = new DateTime(2026, 1, 1),
                    UpdatedAt = new DateTime(2026, 1, 1)
                }
            );
        }
    }
}
