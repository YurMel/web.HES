using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class upd_workstation_tbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_DeviceAccounts_AccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Workstations_ComputerId",
                table: "WorkstationEvents");

            migrationBuilder.DropIndex(
                name: "IX_WorkstationEvents_AccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropIndex(
                name: "IX_WorkstationEvents_ComputerId",
                table: "WorkstationEvents");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "WorkstationEvents");

            migrationBuilder.DropColumn(
                name: "ComputerId",
                table: "WorkstationEvents");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "WorkstationEvents");

            migrationBuilder.AlterColumn<string>(
                name: "WorkstationId",
                table: "WorkstationEvents",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceAccountId",
                table: "WorkstationEvents",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SeverityId",
                table: "WorkstationEvents",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_DeviceAccountId",
                table: "WorkstationEvents",
                column: "DeviceAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_WorkstationId",
                table: "WorkstationEvents",
                column: "WorkstationId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_DeviceAccounts_DeviceAccountId",
                table: "WorkstationEvents",
                column: "DeviceAccountId",
                principalTable: "DeviceAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Workstations_WorkstationId",
                table: "WorkstationEvents",
                column: "WorkstationId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_DeviceAccounts_DeviceAccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkstationEvents_Workstations_WorkstationId",
                table: "WorkstationEvents");

            migrationBuilder.DropIndex(
                name: "IX_WorkstationEvents_DeviceAccountId",
                table: "WorkstationEvents");

            migrationBuilder.DropIndex(
                name: "IX_WorkstationEvents_WorkstationId",
                table: "WorkstationEvents");

            migrationBuilder.DropColumn(
                name: "SeverityId",
                table: "WorkstationEvents");

            migrationBuilder.AlterColumn<string>(
                name: "WorkstationId",
                table: "WorkstationEvents",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceAccountId",
                table: "WorkstationEvents",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "WorkstationEvents",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "WorkstationEvents",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComputerId",
                table: "WorkstationEvents",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "StatusId",
                table: "WorkstationEvents",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_AccountId",
                table: "WorkstationEvents",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationEvents_ComputerId",
                table: "WorkstationEvents",
                column: "ComputerId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_DeviceAccounts_AccountId",
                table: "WorkstationEvents",
                column: "AccountId",
                principalTable: "DeviceAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkstationEvents_Workstations_ComputerId",
                table: "WorkstationEvents",
                column: "ComputerId",
                principalTable: "Workstations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
