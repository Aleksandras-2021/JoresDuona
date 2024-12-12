using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class CategoriesForOrderVarAndItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 371, DateTimeKind.Utc).AddTicks(9850),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 203, DateTimeKind.Utc).AddTicks(8853));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 373, DateTimeKind.Utc).AddTicks(9343),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 205, DateTimeKind.Utc).AddTicks(8425));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 373, DateTimeKind.Utc).AddTicks(508),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 204, DateTimeKind.Utc).AddTicks(9538));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 372, DateTimeKind.Utc).AddTicks(8217),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 204, DateTimeKind.Utc).AddTicks(7228));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 372, DateTimeKind.Utc).AddTicks(2100),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 204, DateTimeKind.Utc).AddTicks(1092));

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
                oldDefaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 206, DateTimeKind.Utc).AddTicks(2885));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                defaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 203, DateTimeKind.Utc).AddTicks(8853),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 371, DateTimeKind.Utc).AddTicks(9850));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 205, DateTimeKind.Utc).AddTicks(8425),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 373, DateTimeKind.Utc).AddTicks(9343));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 204, DateTimeKind.Utc).AddTicks(9538),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 373, DateTimeKind.Utc).AddTicks(508));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 204, DateTimeKind.Utc).AddTicks(7228),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 372, DateTimeKind.Utc).AddTicks(8217));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 204, DateTimeKind.Utc).AddTicks(1092),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 372, DateTimeKind.Utc).AddTicks(2100));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 9, 18, 11, 56, 206, DateTimeKind.Utc).AddTicks(2885),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 10, 23, 55, 28, 374, DateTimeKind.Utc).AddTicks(3743));
        }
    }
}
