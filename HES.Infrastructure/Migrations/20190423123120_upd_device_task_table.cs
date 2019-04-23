using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class upd_device_task_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "Apps",
                table: "DeviceTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "DeviceTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DeviceTasks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Urls",
                table: "DeviceTasks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apps",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "Login",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DeviceTasks");

            migrationBuilder.DropColumn(
                name: "Urls",
                table: "DeviceTasks");

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
    }
}
