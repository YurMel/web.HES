using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class del_device_key : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceKey",
                table: "Devices");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "DeviceKey",
                table: "Devices",
                nullable: true);
        }
    }
}
