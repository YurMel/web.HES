using Microsoft.EntityFrameworkCore.Migrations;

namespace web.HES.Migrations
{
    public partial class add_shared_accounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Urls",
                table: "Templates",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Apps",
                table: "Templates",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateTable(
                name: "SharedAccounts",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Urls = table.Column<string>(nullable: true),
                    Apps = table.Column<string>(nullable: true),
                    Login = table.Column<string>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: false),
                    PasswordChangedAt = table.Column<string>(nullable: true),
                    OtpSecret = table.Column<string>(nullable: true),
                    OtpSecretChangedAt = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedAccounts", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "Urls",
                table: "Templates",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Apps",
                table: "Templates",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
