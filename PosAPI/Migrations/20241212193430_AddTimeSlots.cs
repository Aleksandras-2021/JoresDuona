using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 496, DateTimeKind.Utc).AddTicks(6120),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(360));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 497, DateTimeKind.Utc).AddTicks(4240),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(9670));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 497, DateTimeKind.Utc).AddTicks(660),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(5010));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 496, DateTimeKind.Utc).AddTicks(9580),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(3880));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 496, DateTimeKind.Utc).AddTicks(7180),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(1410));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 497, DateTimeKind.Utc).AddTicks(6140),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 866, DateTimeKind.Utc).AddTicks(1950));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(360),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 496, DateTimeKind.Utc).AddTicks(6120));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(9670),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 497, DateTimeKind.Utc).AddTicks(4240));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(5010),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 497, DateTimeKind.Utc).AddTicks(660));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(3880),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 496, DateTimeKind.Utc).AddTicks(9580));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(1410),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 496, DateTimeKind.Utc).AddTicks(7180));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 866, DateTimeKind.Utc).AddTicks(1950),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 19, 34, 29, 497, DateTimeKind.Utc).AddTicks(6140));
        }
    }
}
