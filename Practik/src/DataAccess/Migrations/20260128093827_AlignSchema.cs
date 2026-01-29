using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AlignSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderAssignments_RepairOrders_RepairOrderOrderId",
                table: "RepairOrderAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderStatusHistories_RepairOrders_RepairOrderOrderId",
                table: "RepairOrderStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkActs_RepairOrders_RepairOrderOrderId",
                table: "WorkActs");

            migrationBuilder.DropIndex(
                name: "IX_WorkActs_RepairOrderOrderId",
                table: "WorkActs");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrderStatusHistories_RepairOrderOrderId",
                table: "RepairOrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrderAssignments_RepairOrderOrderId",
                table: "RepairOrderAssignments");

            migrationBuilder.DropColumn(
                name: "RepairOrderOrderId",
                table: "WorkActs");

            migrationBuilder.DropColumn(
                name: "RepairOrderOrderId",
                table: "RepairOrderStatusHistories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "RepairOrderOrderId",
                table: "RepairOrderAssignments");

            migrationBuilder.DropColumn(
                name: "DeviceType",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "ReceptionistId",
                table: "RepairOrders",
                newName: "CurrentStatusId");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "RepairOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "RepairOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeviceTypeId",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkActs_OrderId",
                table: "WorkActs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderStatusHistories_OrderId",
                table: "RepairOrderStatusHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_CreatedByUserId",
                table: "RepairOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_CurrentStatusId",
                table: "RepairOrders",
                column: "CurrentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrders_DeviceId",
                table: "RepairOrders",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderAssignments_OrderId",
                table: "RepairOrderAssignments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ClientId",
                table: "Devices",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceTypeId",
                table: "Devices",
                column: "DeviceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Clients_ClientId",
                table: "Devices",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceTypes_DeviceTypeId",
                table: "Devices",
                column: "DeviceTypeId",
                principalTable: "DeviceTypes",
                principalColumn: "DeviceTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderAssignments_RepairOrders_OrderId",
                table: "RepairOrderAssignments",
                column: "OrderId",
                principalTable: "RepairOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_Devices_DeviceId",
                table: "RepairOrders",
                column: "DeviceId",
                principalTable: "Devices",
                principalColumn: "DeviceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_RepairOrderStatuses_CurrentStatusId",
                table: "RepairOrders",
                column: "CurrentStatusId",
                principalTable: "RepairOrderStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrders_Users_CreatedByUserId",
                table: "RepairOrders",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderStatusHistories_RepairOrders_OrderId",
                table: "RepairOrderStatusHistories",
                column: "OrderId",
                principalTable: "RepairOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkActs_RepairOrders_OrderId",
                table: "WorkActs",
                column: "OrderId",
                principalTable: "RepairOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Clients_ClientId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceTypes_DeviceTypeId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderAssignments_RepairOrders_OrderId",
                table: "RepairOrderAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_Devices_DeviceId",
                table: "RepairOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_RepairOrderStatuses_CurrentStatusId",
                table: "RepairOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrders_Users_CreatedByUserId",
                table: "RepairOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_RepairOrderStatusHistories_RepairOrders_OrderId",
                table: "RepairOrderStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkActs_RepairOrders_OrderId",
                table: "WorkActs");

            migrationBuilder.DropIndex(
                name: "IX_WorkActs_OrderId",
                table: "WorkActs");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrderStatusHistories_OrderId",
                table: "RepairOrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_CreatedByUserId",
                table: "RepairOrders");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_CurrentStatusId",
                table: "RepairOrders");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrders_DeviceId",
                table: "RepairOrders");

            migrationBuilder.DropIndex(
                name: "IX_RepairOrderAssignments_OrderId",
                table: "RepairOrderAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Devices_ClientId",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_DeviceTypeId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "RepairOrders");

            migrationBuilder.DropColumn(
                name: "DeviceTypeId",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "CurrentStatusId",
                table: "RepairOrders",
                newName: "ReceptionistId");

            migrationBuilder.AddColumn<int>(
                name: "RepairOrderOrderId",
                table: "WorkActs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RepairOrderOrderId",
                table: "RepairOrderStatusHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "RepairOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "RepairOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepairOrderOrderId",
                table: "RepairOrderAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DeviceType",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WorkActs_RepairOrderOrderId",
                table: "WorkActs",
                column: "RepairOrderOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderStatusHistories_RepairOrderOrderId",
                table: "RepairOrderStatusHistories",
                column: "RepairOrderOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderAssignments_RepairOrderOrderId",
                table: "RepairOrderAssignments",
                column: "RepairOrderOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderAssignments_RepairOrders_RepairOrderOrderId",
                table: "RepairOrderAssignments",
                column: "RepairOrderOrderId",
                principalTable: "RepairOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepairOrderStatusHistories_RepairOrders_RepairOrderOrderId",
                table: "RepairOrderStatusHistories",
                column: "RepairOrderOrderId",
                principalTable: "RepairOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkActs_RepairOrders_RepairOrderOrderId",
                table: "WorkActs",
                column: "RepairOrderOrderId",
                principalTable: "RepairOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
