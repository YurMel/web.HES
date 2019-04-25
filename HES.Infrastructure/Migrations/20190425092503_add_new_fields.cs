using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class add_new_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryAccountId",
                table: "Devices",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "IdFromDevice",
                table: "DeviceAccounts",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryAccountId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "IdFromDevice",
                table: "DeviceAccounts");
        }
    }
}
