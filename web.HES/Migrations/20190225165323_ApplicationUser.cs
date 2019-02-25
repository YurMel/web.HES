using Microsoft.EntityFrameworkCore.Migrations;

namespace web.HES.Migrations
{
    public partial class ApplicationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestField",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestField",
                table: "AspNetUsers");
        }
    }
}
