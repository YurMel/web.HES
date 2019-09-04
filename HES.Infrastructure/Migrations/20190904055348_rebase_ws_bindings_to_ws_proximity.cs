using Microsoft.EntityFrameworkCore.Migrations;

namespace HES.Infrastructure.Migrations
{
    public partial class rebase_ws_bindings_to_ws_proximity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkstationBindings");

            migrationBuilder.CreateTable(
                name: "WorkstationProximityDevices",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    WorkstationId = table.Column<string>(nullable: true),
                    LockProximity = table.Column<int>(nullable: false),
                    UnlockProximity = table.Column<int>(nullable: false),
                    LockTimeout = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkstationProximityDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkstationProximityDevices_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationProximityDevices_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationProximityDevices_DeviceId",
                table: "WorkstationProximityDevices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationProximityDevices_WorkstationId",
                table: "WorkstationProximityDevices",
                column: "WorkstationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkstationProximityDevices");

            migrationBuilder.CreateTable(
                name: "WorkstationBindings",
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
                    table.PrimaryKey("PK_WorkstationBindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkstationBindings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkstationBindings_Workstations_WorkstationId",
                        column: x => x.WorkstationId,
                        principalTable: "Workstations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationBindings_DeviceId",
                table: "WorkstationBindings",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkstationBindings_WorkstationId",
                table: "WorkstationBindings",
                column: "WorkstationId");
        }
    }
}
