using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_ChatSessions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatSessions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatSessions_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "DoctorChatAvailabilities",
                columns: table => new
                {
                    AvailabilityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAvailableForChat = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailableForVideo = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastOnlineAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxConcurrentChats = table.Column<int>(type: "int", nullable: false),
                    CurrentActiveChatCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorChatAvailabilities", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK_DoctorChatAvailabilities_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatRequests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeclineReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ChatRequests_ChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSessions",
                        principalColumn: "SessionId");
                    table.ForeignKey(
                        name: "FK_ChatRequests_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRequests_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRequests_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SessionId",
                table: "ChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_AdminId",
                table: "ChatRequests",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_DoctorId",
                table: "ChatRequests",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_PatientId",
                table: "ChatRequests",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRequests_SessionId",
                table: "ChatRequests",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_AdminId",
                table: "ChatSessions",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_DoctorId",
                table: "ChatSessions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_PatientId",
                table: "ChatSessions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorChatAvailabilities_DoctorId",
                table: "DoctorChatAvailabilities",
                column: "DoctorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatRequests");

            migrationBuilder.DropTable(
                name: "DoctorChatAvailabilities");

            migrationBuilder.DropTable(
                name: "ChatSessions");
        }
    }
}
