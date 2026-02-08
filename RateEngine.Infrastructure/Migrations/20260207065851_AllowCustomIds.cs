using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RateEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowCustomIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StarRating",
                table: "Hotels",
                newName: "Stars");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Stars",
                table: "Hotels",
                newName: "StarRating");
        }
    }
}
