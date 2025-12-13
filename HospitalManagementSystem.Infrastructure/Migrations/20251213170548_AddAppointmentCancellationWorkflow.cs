using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentCancellationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationApprovalNote",
                table: "DoctorAppointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CancellationApproved",
                table: "DoctorAppointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationApprovedAt",
                table: "DoctorAppointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancellationApprovedBy",
                table: "DoctorAppointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "DoctorAppointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CancellationRequested",
                table: "DoctorAppointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationRequestedAt",
                table: "DoctorAppointments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationApprovalNote",
                table: "DoctorAppointments");

            migrationBuilder.DropColumn(
                name: "CancellationApproved",
                table: "DoctorAppointments");

            migrationBuilder.DropColumn(
                name: "CancellationApprovedAt",
                table: "DoctorAppointments");

            migrationBuilder.DropColumn(
                name: "CancellationApprovedBy",
                table: "DoctorAppointments");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "DoctorAppointments");

            migrationBuilder.DropColumn(
                name: "CancellationRequested",
                table: "DoctorAppointments");

            migrationBuilder.DropColumn(
                name: "CancellationRequestedAt",
                table: "DoctorAppointments");
        }
    }
}
