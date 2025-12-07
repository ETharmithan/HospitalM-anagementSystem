using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleDateAndIsRecurring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DayOfWeek",
                table: "DoctorSchedules",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "HospitalId",
                table: "DoctorSchedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "DoctorSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleDate",
                table: "DoctorSchedules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSchedules_HospitalId",
                table: "DoctorSchedules",
                column: "HospitalId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSchedules_Hospitals_HospitalId",
                table: "DoctorSchedules",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "HospitalId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSchedules_Hospitals_HospitalId",
                table: "DoctorSchedules");

            migrationBuilder.DropIndex(
                name: "IX_DoctorSchedules_HospitalId",
                table: "DoctorSchedules");

            migrationBuilder.DropColumn(
                name: "HospitalId",
                table: "DoctorSchedules");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "DoctorSchedules");

            migrationBuilder.DropColumn(
                name: "ScheduleDate",
                table: "DoctorSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "DayOfWeek",
                table: "DoctorSchedules",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
