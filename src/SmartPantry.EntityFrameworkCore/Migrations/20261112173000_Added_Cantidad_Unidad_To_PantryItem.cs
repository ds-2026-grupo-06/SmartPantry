using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartPantry.EntityFrameworkCore;

#nullable disable

namespace SmartPantry.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SmartPantryDbContext))]
    [Migration("20261112173000_Added_Cantidad_Unidad_To_PantryItem")]
    public partial class Added_Cantidad_Unidad_To_PantryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Cantidad",
                table: "AppPantryItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unidad",
                table: "AppPantryItems",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "AppPantryItems");

            migrationBuilder.DropColumn(
                name: "Unidad",
                table: "AppPantryItems");
        }
    }
}
