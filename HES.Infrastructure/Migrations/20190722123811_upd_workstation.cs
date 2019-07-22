using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class upd_workstation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "WorkstationSessions");

            migrationBuilder.AlterColumn<int>(
                name: "UnlockedBy",
                table: "WorkstationSessions",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Workstations",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SeverityId",
                table: "WorkstationEvents",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<int>(
                name: "EventId",
                table: "WorkstationEvents",
                nullable: false,
                oldClrType: typeof(byte));

            migrationBuilder.AlterColumn<string>(
                name: "WorkstationId",
                table: "WorkstationBindings",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationBindings_WorkstationId",
                table: "WorkstationBindings",
                column: "WorkstationId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationBindings_Workstations_WorkstationId",
                table: "WorkstationBindings",
                column: "WorkstationId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationBindings_Workstations_WorkstationId",
                table: "WorkstationBindings");

            migrationBuilder.DropIndex(
                name: "IX_WorkstationBindings_WorkstationId",
                table: "WorkstationBindings");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Workstations");

            migrationBuilder.AlterColumn<byte>(
                name: "UnlockedBy",
                table: "WorkstationSessions",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "WorkstationSessions",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AlterColumn<byte>(
                name: "SeverityId",
                table: "WorkstationEvents",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<byte>(
                name: "EventId",
                table: "WorkstationEvents",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<string>(
                name: "WorkstationId",
                table: "WorkstationBindings",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
