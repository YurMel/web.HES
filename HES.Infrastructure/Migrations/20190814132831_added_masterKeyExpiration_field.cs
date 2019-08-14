using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_masterKeyExpiration_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ButtonExpirationTimeout",
                table: "DeviceAccessProfiles",
                newName: "MasterKeyExpiration");

            migrationBuilder.AddColumn<int>(
                name: "ButtonExpiration",
                table: "DeviceAccessProfiles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ButtonExpiration",
                table: "DeviceAccessProfiles");

            migrationBuilder.RenameColumn(
                name: "MasterKeyExpiration",
                table: "DeviceAccessProfiles",
                newName: "ButtonExpirationTimeout");
        }
    }
}
