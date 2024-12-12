using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(360),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 572, DateTimeKind.Utc).AddTicks(8721));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(9670),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 574, DateTimeKind.Utc).AddTicks(8599));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(5010),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(9579));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(3880),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(7213));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(1410),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(1077));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 866, DateTimeKind.Utc).AddTicks(1950),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 575, DateTimeKind.Utc).AddTicks(3066));

            migrationBuilder.CreateTable(
                name: "TimeSlot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    ReservationId = table.Column<int>(type: "integer", nullable: true),
                    ReservationId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlot_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TimeSlot_Reservations_ReservationId1",
                        column: x => x.ReservationId1,
                        principalTable: "Reservations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlot_ReservationId",
                table: "TimeSlot",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlot_ReservationId1",
                table: "TimeSlot",
                column: "ReservationId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeSlot");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Services");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 572, DateTimeKind.Utc).AddTicks(8721),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(360));

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 574, DateTimeKind.Utc).AddTicks(8599),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(9670));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(9579),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(5010));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(7213),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(3880));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 573, DateTimeKind.Utc).AddTicks(1077),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 865, DateTimeKind.Utc).AddTicks(1410));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 11, 0, 14, 25, 575, DateTimeKind.Utc).AddTicks(3066),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 15, 2, 33, 866, DateTimeKind.Utc).AddTicks(1950));
        }
    }
}
