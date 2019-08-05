using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_col : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "WorkstationSessions",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "WorkstationSessions",
                newName: "EndDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "WorkstationSessions",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "WorkstationSessions",
                newName: "EndTime");
        }
    }
}
