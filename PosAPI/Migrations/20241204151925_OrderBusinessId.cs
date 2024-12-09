using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class OrderBusinessId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 477, DateTimeKind.Utc).AddTicks(8001),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(906));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 479, DateTimeKind.Utc).AddTicks(7110),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 164, DateTimeKind.Utc).AddTicks(258));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 478, DateTimeKind.Utc).AddTicks(8007),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 163, DateTimeKind.Utc).AddTicks(1405));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 478, DateTimeKind.Utc).AddTicks(5806),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(9126));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 478, DateTimeKind.Utc).AddTicks(337),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(3482));

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 480, DateTimeKind.Utc).AddTicks(1432),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 164, DateTimeKind.Utc).AddTicks(5109));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Orders");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(906),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 477, DateTimeKind.Utc).AddTicks(8001));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 164, DateTimeKind.Utc).AddTicks(258),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 479, DateTimeKind.Utc).AddTicks(7110));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 163, DateTimeKind.Utc).AddTicks(1405),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 478, DateTimeKind.Utc).AddTicks(8007));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(9126),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 478, DateTimeKind.Utc).AddTicks(5806));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(3482),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 478, DateTimeKind.Utc).AddTicks(337));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 164, DateTimeKind.Utc).AddTicks(5109),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 4, 15, 19, 25, 480, DateTimeKind.Utc).AddTicks(1432));
        }
    }
}
