using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class add_WorkstationSession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkstationSessions",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    UnlockedBy = table.Column<byte>(nullable: false),
                    WorkstationId = table.Column<string>(nullable: true),
                    UserSession = table.Column<string>(nullable: true),
                    DeviceId = table.Column<string>(nullable: true),
                    EmployeeId = table.Column<string>(nullable: true),
                    DepartmentId = table.Column<string>(nullable: true),
                    DeviceAccountId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkstationSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkstationSessions_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationSessions_DeviceAccounts_DeviceAccountId",
                        column: x => x.DeviceAccountId,
                        principalTable: "DeviceAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationSessions_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationSessions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationSessions_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSessions_DepartmentId",
                table: "WorkstationSessions",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSessions_DeviceAccountId",
                table: "WorkstationSessions",
                column: "DeviceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSessions_DeviceId",
                table: "WorkstationSessions",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSessions_EmployeeId",
                table: "WorkstationSessions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationSessions_WorkstationId",
                table: "WorkstationSessions",
                column: "WorkstationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkstationSessions");
        }
    }
}
