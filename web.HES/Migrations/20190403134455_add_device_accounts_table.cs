using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace web.HES.Migrations
{
    public partial class add_device_accounts_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Urls = table.Column<string>(nullable: true),
                    Apps = table.Column<string>(nullable: true),
                    Login = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    LastSyncedAt = table.Column<DateTime>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    PasswordUpdatedAt = table.Column<DateTime>(nullable: false),
                    OtpUpdatedAt = table.Column<DateTime>(nullable: true),
                    Deleted = table.Column<bool>(nullable: false),
                    EmployeeId = table.Column<string>(nullable: true),
                    DeviceId = table.Column<string>(nullable: true),
                    SharedAccountId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAccounts_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceAccounts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceAccounts_SharedAccounts_SharedAccountId",
                        column: x => x.SharedAccountId,
                        principalTable: "SharedAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAccounts_DeviceId",
                table: "DeviceAccounts",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAccounts_EmployeeId",
                table: "DeviceAccounts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAccounts_SharedAccountId",
                table: "DeviceAccounts",
                column: "SharedAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceAccounts");
        }
    }
}
