using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 360, DateTimeKind.Utc).AddTicks(8935),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 78, DateTimeKind.Utc).AddTicks(4240));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 362, DateTimeKind.Utc).AddTicks(6404),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 80, DateTimeKind.Utc).AddTicks(3195));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 361, DateTimeKind.Utc).AddTicks(9135),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 79, DateTimeKind.Utc).AddTicks(5218));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 361, DateTimeKind.Utc).AddTicks(6967),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 79, DateTimeKind.Utc).AddTicks(2921));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 361, DateTimeKind.Utc).AddTicks(1299),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 78, DateTimeKind.Utc).AddTicks(6736));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 363, DateTimeKind.Utc).AddTicks(577),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 80, DateTimeKind.Utc).AddTicks(7390));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 78, DateTimeKind.Utc).AddTicks(4240),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 360, DateTimeKind.Utc).AddTicks(8935));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 80, DateTimeKind.Utc).AddTicks(3195),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 362, DateTimeKind.Utc).AddTicks(6404));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 79, DateTimeKind.Utc).AddTicks(5218),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 361, DateTimeKind.Utc).AddTicks(9135));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 79, DateTimeKind.Utc).AddTicks(2921),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 361, DateTimeKind.Utc).AddTicks(6967));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 78, DateTimeKind.Utc).AddTicks(6736),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 361, DateTimeKind.Utc).AddTicks(1299));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 16, 18, 48, 43, 80, DateTimeKind.Utc).AddTicks(7390),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 16, 18, 49, 9, 363, DateTimeKind.Utc).AddTicks(577));
        }
    }
}
