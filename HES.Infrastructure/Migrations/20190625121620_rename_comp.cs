using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_comp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Computers");

            migrationBuilder.CreateTable(
                name: "Workstations",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ClientVersion = table.Column<string>(nullable: true),
                    CompanyId = table.Column<string>(nullable: true),
                    DepartmentId = table.Column<string>(nullable: true),
                    OS = table.Column<string>(nullable: true),
                    IP = table.Column<string>(nullable: true),
                    LastSeen = table.Column<DateTime>(nullable: false),
                    Approved = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workstations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workstations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workstations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workstations_CompanyId",
                table: "Workstations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Workstations_DepartmentId",
                table: "Workstations",
                column: "DepartmentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workstations");

            migrationBuilder.CreateTable(
                name: "Computers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Approved = table.Column<bool>(nullable: false),
                    ClientVersion = table.Column<string>(nullable: true),
                    CompanyId = table.Column<string>(nullable: true),
                    DepartmentId = table.Column<string>(nullable: true),
                    IP = table.Column<string>(nullable: true),
                    LastSeen = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    OS = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Computers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Computers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Computers_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Computers_CompanyId",
                table: "Computers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Computers_DepartmentId",
                table: "Computers",
                column: "DepartmentId");
        }
    }
}
