using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(WorkshopDbContext))]
    [Migration("20260202160080_CreateRepairOrderStatusHistories")]
    public class CreateRepairOrderStatusHistories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepairOrderStatusHistories",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DateChanged = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderStatusHistories", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_RepairOrderStatusHistories_RepairOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrderStatusHistories_RepairOrderStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "RepairOrderStatuses",
                        principalColumn: "StatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrderStatusHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderStatusHistories_OrderId",
                table: "RepairOrderStatusHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderStatusHistories_StatusId",
                table: "RepairOrderStatusHistories",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderStatusHistories_UserId",
                table: "RepairOrderStatusHistories",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairOrderStatusHistories");
        }
    }
}
