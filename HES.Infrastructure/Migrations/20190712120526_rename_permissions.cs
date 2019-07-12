using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rename_permissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevicePermissions");

            migrationBuilder.CreateTable(
                name: "AuthorizedDevices",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    WorkstationId = table.Column<string>(nullable: true),
                    AllowRfid = table.Column<bool>(nullable: false),
                    AllowBleTap = table.Column<bool>(nullable: false),
                    AllowProximity = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizedDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizedDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedDevices_DeviceId",
                table: "AuthorizedDevices",
                column: "DeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizedDevices");

            migrationBuilder.CreateTable(
                name: "DevicePermissions",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AllowBleTap = table.Column<bool>(nullable: false),
                    AllowProximity = table.Column<bool>(nullable: false),
                    AllowRfid = table.Column<bool>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    WorkstationId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevicePermissions", x => x.Id);
                });
        }
    }
}
