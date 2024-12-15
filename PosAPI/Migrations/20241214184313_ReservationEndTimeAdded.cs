using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ReservationEndTimeAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 709, DateTimeKind.Utc).AddTicks(1184),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 595, DateTimeKind.Utc).AddTicks(7872));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 711, DateTimeKind.Utc).AddTicks(1371),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 597, DateTimeKind.Utc).AddTicks(8894));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservationEndTime",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 710, DateTimeKind.Utc).AddTicks(2233),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 597, DateTimeKind.Utc).AddTicks(107));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 709, DateTimeKind.Utc).AddTicks(9856),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 596, DateTimeKind.Utc).AddTicks(7677));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 709, DateTimeKind.Utc).AddTicks(3435),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 596, DateTimeKind.Utc).AddTicks(1204));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 711, DateTimeKind.Utc).AddTicks(6239),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 598, DateTimeKind.Utc).AddTicks(3561));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationEndTime",
                table: "Reservations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 595, DateTimeKind.Utc).AddTicks(7872),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 709, DateTimeKind.Utc).AddTicks(1184));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 597, DateTimeKind.Utc).AddTicks(8894),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 711, DateTimeKind.Utc).AddTicks(1371));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 597, DateTimeKind.Utc).AddTicks(107),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 710, DateTimeKind.Utc).AddTicks(2233));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 596, DateTimeKind.Utc).AddTicks(7677),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 709, DateTimeKind.Utc).AddTicks(9856));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 596, DateTimeKind.Utc).AddTicks(1204),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 709, DateTimeKind.Utc).AddTicks(3435));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 598, DateTimeKind.Utc).AddTicks(3561),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 18, 43, 12, 711, DateTimeKind.Utc).AddTicks(6239));
        }
    }
}
