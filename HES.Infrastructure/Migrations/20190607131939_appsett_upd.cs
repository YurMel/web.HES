using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class appsett_upd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProtectedValue",
                table: "AppSettings",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AppSettings",
                newName: "Key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "AppSettings",
                newName: "ProtectedValue");

            migrationBuilder.RenameColumn(
                name: "Key",
                table: "AppSettings",
                newName: "Id");
        }
    }
}
