using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace web.HES.Migrations
{
    public partial class add_new_device_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Battery",
                table: "Devices",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Firmware",
                table: "Devices",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSynced",
                table: "Devices",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Battery",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Firmware",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LastSynced",
                table: "Devices");
        }
    }
}
