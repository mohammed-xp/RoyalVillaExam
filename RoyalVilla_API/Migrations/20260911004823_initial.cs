using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RoyalVilla_API.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Villas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rate = table.Column<double>(type: "float", nullable: false),
                    Sqft = table.Column<int>(type: "int", nullable: false),
                    Occupancy = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Villas", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Villas",
                columns: new[] { "Id", "CreatedAt", "Details", "ImageUrl", "Name", "Occupancy", "Rate", "Sqft", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "This is a luxurious villa with stunning views.", "https://example.com/images/royal-villa.jpg", "Royal Villa", 8, 500.0, 3500, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "A beautiful villa located right on the beach.", "https://example.com/images/beachside-villa.jpg", "Beachside Villa", 6, 400.0, 3000, new DateTime(2026, 4, 18, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2026, 7, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "A cozy villa nestled in the mountains.", "https://example.com/images/mountain-retreat.jpg", "Mountain Retreat", 4, 350.0, 2500, new DateTime(2026, 7, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2026, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "A modern penthouse in the heart of the city.", "https://example.com/images/city-penthouse.jpg", "City Penthouse", 10, 600.0, 4000, new DateTime(2026, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "A charming cottage in the countryside.", "https://example.com/images/countryside-cottage.jpg", "Countryside Cottage", 4, 300.0, 2000, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Villas");
        }
    }
}
