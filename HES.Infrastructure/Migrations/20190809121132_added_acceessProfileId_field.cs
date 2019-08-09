using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_acceessProfileId_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_DeviceAccessProfileId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "DeviceAccessProfileId",
                table: "Devices",
                newName: "AcceessProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_DeviceAccessProfileId",
                table: "Devices",
                newName: "IX_Devices_AcceessProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_AcceessProfileId",
                table: "Devices",
                column: "AcceessProfileId",
                principalTable: "DeviceAccessProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_AcceessProfileId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "AcceessProfileId",
                table: "Devices",
                newName: "DeviceAccessProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_AcceessProfileId",
                table: "Devices",
                newName: "IX_Devices_DeviceAccessProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_DeviceAccessProfileId",
                table: "Devices",
                column: "DeviceAccessProfileId",
                principalTable: "DeviceAccessProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
