using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class added_deviceAccessProfiles_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceAccessProfileId",
                table: "Devices",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeviceAccessProfiles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    ButtonBonding = table.Column<bool>(nullable: false),
                    ButtonConnection = table.Column<bool>(nullable: false),
                    ButtonNewChannel = table.Column<bool>(nullable: false),
                    ButtonNewLink = table.Column<bool>(nullable: false),
                    PinBonding = table.Column<bool>(nullable: false),
                    PinConnection = table.Column<bool>(nullable: false),
                    PinNewChannel = table.Column<bool>(nullable: false),
                    PinNewLink = table.Column<bool>(nullable: false),
                    MasterKeyBonding = table.Column<bool>(nullable: false),
                    MasterKeyConnection = table.Column<bool>(nullable: false),
                    MasterKeyNewChannel = table.Column<bool>(nullable: false),
                    MasterKeyNewLink = table.Column<bool>(nullable: false),
                    PinExpiration = table.Column<int>(nullable: false),
                    PinLength = table.Column<int>(nullable: false),
                    PinTryCount = table.Column<int>(nullable: false),
                    ButtonExpirationTimeout = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAccessProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceAccessProfileId",
                table: "Devices",
                column: "DeviceAccessProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_DeviceAccessProfileId",
                table: "Devices",
                column: "DeviceAccessProfileId",
                principalTable: "DeviceAccessProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceAccessProfiles_DeviceAccessProfileId",
                table: "Devices");

            migrationBuilder.DropTable(
                name: "DeviceAccessProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Devices_DeviceAccessProfileId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "DeviceAccessProfileId",
                table: "Devices");
        }
    }
}
