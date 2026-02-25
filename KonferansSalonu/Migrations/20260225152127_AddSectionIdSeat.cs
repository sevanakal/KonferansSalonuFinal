using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KonferansSalonu.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionIdSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sectionid",
                table: "seats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sectionid",
                table: "seats");
        }
    }
}
