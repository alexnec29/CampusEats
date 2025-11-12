using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CampusEats.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialFullMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KitchenTask_Order_OrderId",
                table: "KitchenTask");

            migrationBuilder.DropForeignKey(
                name: "FK_KitchenTask_Users_AssignedStaffId",
                table: "KitchenTask");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Users_UserId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_MenuItems_MenuItemId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Order_OrderId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Order_OrderId",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Order",
                table: "Order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KitchenTask",
                table: "KitchenTask");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "OrderItem",
                newName: "OrderItems");

            migrationBuilder.RenameTable(
                name: "Order",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "KitchenTask",
                newName: "KitchenTasks");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_OrderId",
                table: "Payments",
                newName: "IX_Payments_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_MenuItemId",
                table: "OrderItems",
                newName: "IX_OrderItems_MenuItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_UserId",
                table: "Orders",
                newName: "IX_Orders_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_KitchenTask_OrderId",
                table: "KitchenTasks",
                newName: "IX_KitchenTasks_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_KitchenTask_AssignedStaffId",
                table: "KitchenTasks",
                newName: "IX_KitchenTasks_AssignedStaffId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payments",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "OrderItems",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KitchenTasks",
                table: "KitchenTasks",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "LoyaltyAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PointsBalance = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoyaltyAccountId = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_LoyaltyAccounts_LoyaltyAccountId",
                        column: x => x.LoyaltyAccountId,
                        principalTable: "LoyaltyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyAccounts_UserId",
                table: "LoyaltyAccounts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_LoyaltyAccountId",
                table: "LoyaltyTransactions",
                column: "LoyaltyAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenTasks_Orders_OrderId",
                table: "KitchenTasks",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenTasks_Users_AssignedStaffId",
                table: "KitchenTasks",
                column: "AssignedStaffId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_MenuItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KitchenTasks_Orders_OrderId",
                table: "KitchenTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_KitchenTasks_Users_AssignedStaffId",
                table: "KitchenTasks");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_MenuItems_MenuItemId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "LoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "LoyaltyAccounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KitchenTasks",
                table: "KitchenTasks");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Payment");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Order");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItem");

            migrationBuilder.RenameTable(
                name: "KitchenTasks",
                newName: "KitchenTask");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_OrderId",
                table: "Payment",
                newName: "IX_Payment_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Orders_UserId",
                table: "Order",
                newName: "IX_Order_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItem",
                newName: "IX_OrderItem_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItem",
                newName: "IX_OrderItem_MenuItemId");

            migrationBuilder.RenameIndex(
                name: "IX_KitchenTasks_OrderId",
                table: "KitchenTask",
                newName: "IX_KitchenTask_OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_KitchenTasks_AssignedStaffId",
                table: "KitchenTask",
                newName: "IX_KitchenTask_AssignedStaffId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Payment",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Order",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "OrderItem",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Order",
                table: "Order",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KitchenTask",
                table: "KitchenTask",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenTask_Order_OrderId",
                table: "KitchenTask",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KitchenTask_Users_AssignedStaffId",
                table: "KitchenTask",
                column: "AssignedStaffId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Users_UserId",
                table: "Order",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_MenuItems_MenuItemId",
                table: "OrderItem",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Order_OrderId",
                table: "OrderItem",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Order_OrderId",
                table: "Payment",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
