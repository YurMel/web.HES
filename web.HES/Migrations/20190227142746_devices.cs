using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace web.HES.Migrations
{
    public partial class devices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    MAC = table.Column<string>(nullable: true),
                    ManufacturerUserId = table.Column<string>(nullable: true),
                    Model = table.Column<string>(nullable: true),
                    BootLoaderVersion = table.Column<string>(nullable: true),
                    Manufactured = table.Column<DateTime>(nullable: false),
                    CpuSerialNo = table.Column<string>(nullable: true),
                    DeviceKey = table.Column<byte[]>(nullable: true),
                    BleDeviceBatchId = table.Column<int>(nullable: true),
                    RegisteredUserId = table.Column<string>(maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_AspNetUsers_RegisteredUserId",
                        column: x => x.RegisteredUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_RegisteredUserId",
                table: "Devices",
                column: "RegisteredUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
