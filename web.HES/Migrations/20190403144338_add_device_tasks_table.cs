using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace web.HES.Migrations
{
    public partial class add_device_tasks_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceTasks",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Password = table.Column<string>(nullable: true),
                    OtpSecret = table.Column<string>(nullable: true),
                    Operation = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    DeviceAccountId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTasks_DeviceAccounts_DeviceAccountId",
                        column: x => x.DeviceAccountId,
                        principalTable: "DeviceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceTasks_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTasks_DeviceAccountId",
                table: "DeviceTasks",
                column: "DeviceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTasks_DeviceId",
                table: "DeviceTasks",
                column: "DeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceTasks");
        }
    }
}
