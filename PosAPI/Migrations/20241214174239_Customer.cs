using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosAPI.Migrations
{
    /// <inheritdoc />
    public partial class Customer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Customers_CustomerId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Users_EmployeeId",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlot_Reservations_ReservationId1",
                table: "TimeSlot");

            migrationBuilder.DropIndex(
                name: "IX_TimeSlot_ReservationId1",
                table: "TimeSlot");

            migrationBuilder.DropColumn(
                name: "ReservationId1",
                table: "TimeSlot");

            migrationBuilder.DropColumn(
                name: "NumberOfGuests",
                table: "Reservations");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Services",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 595, DateTimeKind.Utc).AddTicks(7872),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 975, DateTimeKind.Utc).AddTicks(2230));

            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "Reservations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Reservations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Reservations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Reservations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 597, DateTimeKind.Utc).AddTicks(8894),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 977, DateTimeKind.Utc).AddTicks(2903));

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 597, DateTimeKind.Utc).AddTicks(107),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 976, DateTimeKind.Utc).AddTicks(3504));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 596, DateTimeKind.Utc).AddTicks(7677),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 976, DateTimeKind.Utc).AddTicks(1069));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 596, DateTimeKind.Utc).AddTicks(1204),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 975, DateTimeKind.Utc).AddTicks(4600));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 598, DateTimeKind.Utc).AddTicks(3561),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 977, DateTimeKind.Utc).AddTicks(7477));

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Customers_CustomerId",
                table: "Reservations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Users_EmployeeId",
                table: "Services",
                column: "EmployeeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_Customers_CustomerId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Users_EmployeeId",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "ReservationId1",
                table: "TimeSlot",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Services",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdate",
                table: "Schedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 975, DateTimeKind.Utc).AddTicks(2230),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 595, DateTimeKind.Utc).AddTicks(7872));

            migrationBuilder.AlterColumn<string>(
                name: "CustomerPhone",
                table: "Reservations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerName",
                table: "Reservations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerEmail",
                table: "Reservations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "BookedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 977, DateTimeKind.Utc).AddTicks(2903),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 597, DateTimeKind.Utc).AddTicks(8894));

            migrationBuilder.AddColumn<int>(
                name: "NumberOfGuests",
                table: "Reservations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RefundDate",
                table: "Refunds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 976, DateTimeKind.Utc).AddTicks(3504),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 597, DateTimeKind.Utc).AddTicks(107));

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 976, DateTimeKind.Utc).AddTicks(1069),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 596, DateTimeKind.Utc).AddTicks(7677));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 975, DateTimeKind.Utc).AddTicks(4600),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 596, DateTimeKind.Utc).AddTicks(1204));

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 12, 21, 41, 13, 977, DateTimeKind.Utc).AddTicks(7477),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 12, 14, 17, 42, 38, 598, DateTimeKind.Utc).AddTicks(3561));

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlot_ReservationId1",
                table: "TimeSlot",
                column: "ReservationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Customers_CustomerId",
                table: "Reservations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Users_EmployeeId",
                table: "Services",
                column: "EmployeeId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlot_Reservations_ReservationId1",
                table: "TimeSlot",
                column: "ReservationId1",
                principalTable: "Reservations",
                principalColumn: "Id");
        }
    }
}
