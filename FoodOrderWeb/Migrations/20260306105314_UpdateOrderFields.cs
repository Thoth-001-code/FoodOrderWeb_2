using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodOrderWeb.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_FoodItems_FoodItemId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_FoodItems_FoodItemId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Orders_OrderId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
     name: "Review",
     columns: table => new
     {
         Id = table.Column<int>(type: "int", nullable: false)
             .Annotation("SqlServer:Identity", "1, 1"),
         UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
         FoodItemId = table.Column<int>(type: "int", nullable: false),
         OrderId = table.Column<int>(type: "int", nullable: false),
         Rating = table.Column<int>(type: "int", nullable: false),
         Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
         CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
     },
     constraints: table =>
     {
         table.PrimaryKey("PK_Review", x => x.Id);

         // GIỮ NGUYÊN - UserId vẫn là CASCADE
         table.ForeignKey(
             name: "FK_Review_AspNetUsers_UserId",
             column: x => x.UserId,
             principalTable: "AspNetUsers",
             principalColumn: "Id",
             onDelete: ReferentialAction.Cascade);

         // SỬA - FoodItemId đổi thành RESTRICT
         table.ForeignKey(
             name: "FK_Review_FoodItems_FoodItemId",
             column: x => x.FoodItemId,
             principalTable: "FoodItems",
             principalColumn: "Id",
             onDelete: ReferentialAction.Restrict);  // Đổi từ Cascade sang Restrict

         // SỬA - OrderId đổi thành RESTRICT
         table.ForeignKey(
             name: "FK_Review_Orders_OrderId",
             column: x => x.OrderId,
             principalTable: "Orders",
             principalColumn: "Id",
             onDelete: ReferentialAction.Restrict);  // Đổi từ Cascade sang Restrict
     });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 17, 53, 14, 411, DateTimeKind.Local).AddTicks(3274));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 17, 53, 14, 411, DateTimeKind.Local).AddTicks(3276));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 17, 53, 14, 411, DateTimeKind.Local).AddTicks(3278));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 17, 53, 14, 411, DateTimeKind.Local).AddTicks(3279));

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_IsAvailable",
                table: "FoodItems",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Review_FoodItemId",
                table: "Review",
                column: "FoodItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_OrderId",
                table: "Review",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Review_UserId",
                table: "Review",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_FoodItems_FoodItemId",
                table: "CartItems",
                column: "FoodItemId",
                principalTable: "FoodItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_FoodItems_FoodItemId",
                table: "OrderItems",
                column: "FoodItemId",
                principalTable: "FoodItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Orders_OrderId",
                table: "Transactions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_FoodItems_FoodItemId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_FoodItems_FoodItemId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Orders_OrderId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Status",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_FoodItems_IsAvailable",
                table: "FoodItems");

            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 48, 16, 663, DateTimeKind.Local).AddTicks(4838));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 48, 16, 663, DateTimeKind.Local).AddTicks(4852));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 48, 16, 663, DateTimeKind.Local).AddTicks(4853));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 5, 17, 48, 16, 663, DateTimeKind.Local).AddTicks(4854));

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_FoodItems_FoodItemId",
                table: "CartItems",
                column: "FoodItemId",
                principalTable: "FoodItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_FoodItems_FoodItemId",
                table: "OrderItems",
                column: "FoodItemId",
                principalTable: "FoodItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Orders_OrderId",
                table: "Transactions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
