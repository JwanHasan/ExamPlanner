using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseConnection.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningRulesAndPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSession_Room_RoomId",
                table: "ExamSession");

            migrationBuilder.DropIndex(
                name: "IX_ConflictAcknowledgements_ScheduleId",
                table: "ConflictAcknowledgements");

            migrationBuilder.RenameColumn(
                name: "Nickmane",
                table: "PlanningEducationElement",
                newName: "Nickname");

            migrationBuilder.RenameColumn(
                name: "CompletionAssesment",
                table: "PlanningEducationElement",
                newName: "CompletionAssessment");

            migrationBuilder.RenameColumn(
                name: "AssessmenType",
                table: "PlanningEducationElement",
                newName: "AssessmentType");

            migrationBuilder.AddColumn<string>(
                name: "RuleKey",
                table: "RuleSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "ExamSession",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "ExamSession",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "ExamSession",
                type: "time without time zone",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AddColumn<string>(
                name: "SnapshotJson",
                table: "ConflictAcknowledgements",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PlanningEventFacts",
                columns: table => new
                {
                    AssessmentEventId = table.Column<int>(type: "integer", nullable: false),
                    CoTaught = table.Column<bool>(type: "boolean", nullable: true),
                    SmallSemester = table.Column<bool>(type: "boolean", nullable: true),
                    GraduatingStudents = table.Column<bool>(type: "boolean", nullable: true),
                    LargeClass = table.Column<bool>(type: "boolean", nullable: true),
                    HoldsJson = table.Column<string>(type: "text", nullable: false),
                    AvoidWeekdaysJson = table.Column<string>(type: "text", nullable: false),
                    AvoidDatesJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningEventFacts", x => x.AssessmentEventId);
                    table.ForeignKey(
                        name: "FK_PlanningEventFacts_AssessmentEvent_AssessmentEventId",
                        column: x => x.AssessmentEventId,
                        principalTable: "AssessmentEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleEditAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BeforeJson = table.Column<string>(type: "text", nullable: false),
                    AfterJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleEditAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleEditAudits_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleEditAudits_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RuleSettings_RuleKey",
                table: "RuleSettings",
                column: "RuleKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConflictAcknowledgements_ScheduleId_FingerPrint",
                table: "ConflictAcknowledgements",
                columns: new[] { "ScheduleId", "FingerPrint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEditAudits_ScheduleId",
                table: "ScheduleEditAudits",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleEditAudits_UserId",
                table: "ScheduleEditAudits",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSession_Room_RoomId",
                table: "ExamSession",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSession_Room_RoomId",
                table: "ExamSession");

            migrationBuilder.DropTable(
                name: "PlanningEventFacts");

            migrationBuilder.DropTable(
                name: "ScheduleEditAudits");

            migrationBuilder.DropIndex(
                name: "IX_RuleSettings_RuleKey",
                table: "RuleSettings");

            migrationBuilder.DropIndex(
                name: "IX_ConflictAcknowledgements_ScheduleId_FingerPrint",
                table: "ConflictAcknowledgements");

            migrationBuilder.DropColumn(
                name: "RuleKey",
                table: "RuleSettings");

            migrationBuilder.DropColumn(
                name: "SnapshotJson",
                table: "ConflictAcknowledgements");

            migrationBuilder.RenameColumn(
                name: "Nickname",
                table: "PlanningEducationElement",
                newName: "Nickmane");

            migrationBuilder.RenameColumn(
                name: "CompletionAssessment",
                table: "PlanningEducationElement",
                newName: "CompletionAssesment");

            migrationBuilder.RenameColumn(
                name: "AssessmentType",
                table: "PlanningEducationElement",
                newName: "AssessmenType");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "ExamSession",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "ExamSession",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "ExamSession",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConflictAcknowledgements_ScheduleId",
                table: "ConflictAcknowledgements",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSession_Room_RoomId",
                table: "ExamSession",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
