using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class update_device_task_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AppsChanged",
                table: "DeviceTasks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LoginChanged",
                table: "DeviceTasks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NameChanged",
                table: "DeviceTasks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OtpSecretChanged",
                table: "DeviceTasks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PasswordChanged",
                table: "DeviceTasks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UrlsChanged",
                table: "DeviceTasks",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppsChanged",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "LoginChanged",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "NameChanged",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "OtpSecretChanged",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "PasswordChanged",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "UrlsChanged",
                table: "DeviceTasks");
        }
    }
}
