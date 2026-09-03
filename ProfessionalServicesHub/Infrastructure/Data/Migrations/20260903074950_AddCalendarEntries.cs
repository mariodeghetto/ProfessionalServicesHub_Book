using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfessionalServicesHub.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAllDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: true),
                    EngagementId = table.Column<int>(type: "INTEGER", nullable: true),
                    WorkActivityId = table.Column<int>(type: "INTEGER", nullable: true),
                    Assignee = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarEntries_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEntries_Engagements_EngagementId",
                        column: x => x.EngagementId,
                        principalTable: "Engagements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalendarEntries_WorkActivities_WorkActivityId",
                        column: x => x.WorkActivityId,
                        principalTable: "WorkActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEntries_Assignee",
                table: "CalendarEntries",
                column: "Assignee");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEntries_ClientId",
                table: "CalendarEntries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEntries_EndTime",
                table: "CalendarEntries",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEntries_EngagementId",
                table: "CalendarEntries",
                column: "EngagementId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEntries_StartTime",
                table: "CalendarEntries",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEntries_WorkActivityId",
                table: "CalendarEntries",
                column: "WorkActivityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEntries");
        }
    }
}
