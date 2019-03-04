using Microsoft.EntityFrameworkCore.Migrations;

namespace web.HES.Migrations
{
    public partial class upd_device : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_RegisteredUserId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_RegisteredUserId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "BleDeviceBatchId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "BootLoaderVersion",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RegisteredUserId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "ManufacturerUserId",
                table: "Devices",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Manufactured",
                table: "Devices",
                newName: "ImportedAt");

            migrationBuilder.RenameColumn(
                name: "CpuSerialNo",
                table: "Devices",
                newName: "RFID");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Devices",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_UserId",
                table: "Devices",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_AspNetUsers_UserId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_UserId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Devices",
                newName: "ManufacturerUserId");

            migrationBuilder.RenameColumn(
                name: "RFID",
                table: "Devices",
                newName: "CpuSerialNo");

            migrationBuilder.RenameColumn(
                name: "ImportedAt",
                table: "Devices",
                newName: "Manufactured");

            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerUserId",
                table: "Devices",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BleDeviceBatchId",
                table: "Devices",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BootLoaderVersion",
                table: "Devices",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredUserId",
                table: "Devices",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_RegisteredUserId",
                table: "Devices",
                column: "RegisteredUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_AspNetUsers_RegisteredUserId",
                table: "Devices",
                column: "RegisteredUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
