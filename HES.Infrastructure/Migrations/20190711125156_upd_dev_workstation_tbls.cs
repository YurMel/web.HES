using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class upd_dev_workstation_tbls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workstations_Companies_CompanyId",
                table: "Workstations");

            migrationBuilder.DropIndex(
                name: "IX_Workstations_CompanyId",
                table: "Workstations");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Workstations");

            migrationBuilder.AddColumn<int>(
                name: "LockProximity",
                table: "Workstations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LockTimeout",
                table: "Workstations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnlockProximity",
                table: "Workstations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "UsePin",
                table: "Devices",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockProximity",
                table: "Workstations");

            migrationBuilder.DropColumn(
                name: "LockTimeout",
                table: "Workstations");

            migrationBuilder.DropColumn(
                name: "UnlockProximity",
                table: "Workstations");

            migrationBuilder.DropColumn(
                name: "UsePin",
                table: "Devices");

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "Workstations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workstations_CompanyId",
                table: "Workstations",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workstations_Companies_CompanyId",
                table: "Workstations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
