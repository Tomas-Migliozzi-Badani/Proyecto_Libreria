using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Libreria.Migrations
{
    /// <inheritdoc />
    public partial class VersionarTokensJwt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VersionToken",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VersionToken",
                table: "Usuarios");
        }
    }
}
