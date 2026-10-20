using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPantry.Migrations
{
    /// <inheritdoc />
    public partial class Added_PantryItem_Entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppPantryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPropietario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPantryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPantryItemWarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PantryItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoAviso = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EstaActivo = table.Column<bool>(type: "bit", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPantryItemWarnings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppPantryItemWarnings_PantryItemId_TipoAviso",
                table: "AppPantryItemWarnings",
                columns: new[] { "PantryItemId", "TipoAviso" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppPantryItemWarnings");

            migrationBuilder.DropTable(
                name: "AppPantryItems");
        }
    }
}
