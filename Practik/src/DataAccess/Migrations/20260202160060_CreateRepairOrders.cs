using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(WorkshopDbContext))]
    [Migration("20260202160060_CreateRepairOrders")]
    public class CreateRepairOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepairOrders",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CurrentStatusId = table.Column<int>(type: "int", nullable: false),
                    ProblemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrders", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_RepairOrders_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrders_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairOrders_RepairOrderStatuses_CurrentStatusId",
                        column: x => x.CurrentStatusId,
                        principalTable: "RepairOrderStatuses",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_DeviceId",
                table: "RepairOrders",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_CreatedByUserId",
                table: "RepairOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_CurrentStatusId",
                table: "RepairOrders",
                column: "CurrentStatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairOrders");
        }
    }
}
