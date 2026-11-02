using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_Libreria.Migrations
{
    /// <inheritdoc />
    public partial class VincularUsuarioCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCliente",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Dni",
                table: "Clientes",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdCliente",
                table: "Usuarios",
                column: "IdCliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Dni",
                table: "Clientes",
                column: "Dni",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Clientes_IdCliente",
                table: "Usuarios",
                column: "IdCliente",
                principalTable: "Clientes",
                principalColumn: "IdCliente",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Clientes_IdCliente",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_IdCliente",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Dni",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IdCliente",
                table: "Usuarios");

            migrationBuilder.AlterColumn<string>(
                name: "Dni",
                table: "Clientes",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20);
        }
    }
}
