using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class add_workstation_events : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkstationEvents",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    EventId = table.Column<byte>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    WorkstationId = table.Column<string>(nullable: true),
                    UserSession = table.Column<string>(nullable: true),
                    DeviceId = table.Column<string>(nullable: true),
                    EmployeeId = table.Column<string>(nullable: true),
                    DepartmentId = table.Column<string>(nullable: true),
                    DeviceAccountId = table.Column<string>(nullable: true),
                    AccountType = table.Column<int>(nullable: true),
                    ComputerId = table.Column<string>(nullable: true),
                    AccountId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkstationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkstationEvents_DeviceAccounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "DeviceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationEvents_Workstations_ComputerId",
                        column: x => x.ComputerId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationEvents_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationEvents_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_AccountId",
                table: "WorkstationEvents",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_ComputerId",
                table: "WorkstationEvents",
                column: "ComputerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_DepartmentId",
                table: "WorkstationEvents",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_DeviceId",
                table: "WorkstationEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_EmployeeId",
                table: "WorkstationEvents",
                column: "EmployeeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkstationEvents");
        }
    }
}
