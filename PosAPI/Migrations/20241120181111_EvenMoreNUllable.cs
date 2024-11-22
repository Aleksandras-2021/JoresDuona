using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class EvenMoreNUllable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(906),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 113, DateTimeKind.Utc).AddTicks(6155));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 164, DateTimeKind.Utc).AddTicks(258),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 115, DateTimeKind.Utc).AddTicks(5417));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 163, DateTimeKind.Utc).AddTicks(1405),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 114, DateTimeKind.Utc).AddTicks(6249));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(9126),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 114, DateTimeKind.Utc).AddTicks(4018));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(3482),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 113, DateTimeKind.Utc).AddTicks(8438));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 164, DateTimeKind.Utc).AddTicks(5109),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 115, DateTimeKind.Utc).AddTicks(9796));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 113, DateTimeKind.Utc).AddTicks(6155),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(906));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 115, DateTimeKind.Utc).AddTicks(5417),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 164, DateTimeKind.Utc).AddTicks(258));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 114, DateTimeKind.Utc).AddTicks(6249),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 163, DateTimeKind.Utc).AddTicks(1405));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 114, DateTimeKind.Utc).AddTicks(4018),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(9126));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 113, DateTimeKind.Utc).AddTicks(8438),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 162, DateTimeKind.Utc).AddTicks(3482));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 11, 20, 18, 10, 4, 115, DateTimeKind.Utc).AddTicks(9796),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 11, 20, 18, 11, 11, 164, DateTimeKind.Utc).AddTicks(5109));
        }
    }
}
