using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class update_workstation_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "RFID",
                table: "Workstations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RFID",
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
        }
    }
}
