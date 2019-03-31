using Microsoft.EntityFrameworkCore.Migrations;

namespace web.HES.Migrations
{
    public partial class update_table_shared_accounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "SharedAccounts",
                newName: "Password");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "SharedAccounts",
                newName: "PasswordHash");
        }
    }
}
