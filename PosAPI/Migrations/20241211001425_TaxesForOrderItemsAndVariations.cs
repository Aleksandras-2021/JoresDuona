using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TaxesForOrderItemsAndVariations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "OrderItemVariations");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "OrderItems");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 572, DateTimeKind.Utc).AddTicks(8721),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 371, DateTimeKind.Utc).AddTicks(9850));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 574, DateTimeKind.Utc).AddTicks(8599),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 373, DateTimeKind.Utc).AddTicks(9343));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(9579),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 373, DateTimeKind.Utc).AddTicks(508));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(7213),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 372, DateTimeKind.Utc).AddTicks(8217));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(1077),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 372, DateTimeKind.Utc).AddTicks(2100));

            migrationBuilder.AddColumn<decimal>(
                name: "TaxedAmount",
                table: "OrderItemVariations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxedAmount",
                table: "OrderItems",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 575, DateTimeKind.Utc).AddTicks(3066),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 374, DateTimeKind.Utc).AddTicks(3743));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxedAmount",
                table: "OrderItemVariations");

            migrationBuilder.DropColumn(
                name: "TaxedAmount",
                table: "OrderItems");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 371, DateTimeKind.Utc).AddTicks(9850),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 572, DateTimeKind.Utc).AddTicks(8721));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 373, DateTimeKind.Utc).AddTicks(9343),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 574, DateTimeKind.Utc).AddTicks(8599));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 373, DateTimeKind.Utc).AddTicks(508),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(9579));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 372, DateTimeKind.Utc).AddTicks(8217),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(7213));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 372, DateTimeKind.Utc).AddTicks(2100),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(1077));

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "OrderItemVariations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 374, DateTimeKind.Utc).AddTicks(3743),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 575, DateTimeKind.Utc).AddTicks(3066));
        }
    }
}
