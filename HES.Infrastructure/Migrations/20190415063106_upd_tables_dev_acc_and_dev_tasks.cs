using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class upd_tables_dev_acc_and_dev_tasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceTasks_Devices_DeviceId",
                table: "DeviceTasks");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTasks_DeviceId",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "DeviceTasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DeviceAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DeviceAccounts");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "DeviceTasks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTasks_DeviceId",
                table: "DeviceTasks",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceTasks_Devices_DeviceId",
                table: "DeviceTasks",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
