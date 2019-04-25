using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class upd_dev_acc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "IdFromDevice",
                table: "DeviceAccounts",
                nullable: true,
                oldClrType: typeof(short));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<short>(
                name: "IdFromDevice",
                table: "DeviceAccounts",
                nullable: false,
                oldClrType: typeof(short),
                oldNullable: true);
        }
    }
}
