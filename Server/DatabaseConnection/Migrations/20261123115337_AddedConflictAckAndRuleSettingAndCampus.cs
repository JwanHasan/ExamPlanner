using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseConnection.Migrations
{
    /// <inheritdoc />
    public partial class AddedConflictAckAndRuleSettingAndCampus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentEvent_Course_CourseId",
                table: "AssessmentEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Course_CourseId",
                table: "Enrollment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_CourseId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentEvent_CourseId",
                table: "AssessmentEvent");

            migrationBuilder.DropColumn(
                name: "Campus",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "Campus",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "AssessmentEvent");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Enrollment",
                newName: "SourceImportId");

            migrationBuilder.AddColumn<int>(
                name: "AttemptNo",
                table: "Student",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SourceStudentId",
                table: "Student",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "Room",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "PlanningEducationElement",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "PlanningEducationElement",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "ExamSession",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "ExamSession",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "GroupLabel",
                table: "ExamSession",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RowVersion",
                table: "ExamSession",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "ExamSession",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "PlanningElementId",
                table: "Enrollment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Hold",
                table: "Enrollment",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment",
                columns: new[] { "StudentId", "PlanningElementId" });

            migrationBuilder.CreateTable(
                name: "Campuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsVirtual = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuleSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConflictAcknowledgements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    RuleKeyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    FingerPrint = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConflictAcknowledgements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConflictAcknowledgements_RuleSettings_RuleKeyId",
                        column: x => x.RuleKeyId,
                        principalTable: "RuleSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConflictAcknowledgements_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConflictAcknowledgements_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Room_CampusId",
                table: "Room",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningEducationElement_CampusId",
                table: "PlanningEducationElement",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanningEducationElement_CourseId",
                table: "PlanningEducationElement",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSession_CampusId",
                table: "ExamSession",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_PlanningElementId",
                table: "Enrollment",
                column: "PlanningElementId");

            migrationBuilder.CreateIndex(
                name: "IX_ConflictAcknowledgements_RuleKeyId",
                table: "ConflictAcknowledgements",
                column: "RuleKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ConflictAcknowledgements_ScheduleId",
                table: "ConflictAcknowledgements",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ConflictAcknowledgements_UserId",
                table: "ConflictAcknowledgements",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_PlanningEducationElement_PlanningElementId",
                table: "Enrollment",
                column: "PlanningElementId",
                principalTable: "PlanningEducationElement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSession_Campuses_CampusId",
                table: "ExamSession",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningEducationElement_Campuses_CampusId",
                table: "PlanningEducationElement",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningEducationElement_Course_CourseId",
                table: "PlanningEducationElement",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Room_Campuses_CampusId",
                table: "Room",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_PlanningEducationElement_PlanningElementId",
                table: "Enrollment");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSession_Campuses_CampusId",
                table: "ExamSession");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningEducationElement_Campuses_CampusId",
                table: "PlanningEducationElement");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningEducationElement_Course_CourseId",
                table: "PlanningEducationElement");

            migrationBuilder.DropForeignKey(
                name: "FK_Room_Campuses_CampusId",
                table: "Room");

            migrationBuilder.DropTable(
                name: "Campuses");

            migrationBuilder.DropTable(
                name: "ConflictAcknowledgements");

            migrationBuilder.DropTable(
                name: "RuleSettings");

            migrationBuilder.DropIndex(
                name: "IX_Room_CampusId",
                table: "Room");

            migrationBuilder.DropIndex(
                name: "IX_PlanningEducationElement_CampusId",
                table: "PlanningEducationElement");

            migrationBuilder.DropIndex(
                name: "IX_PlanningEducationElement_CourseId",
                table: "PlanningEducationElement");

            migrationBuilder.DropIndex(
                name: "IX_ExamSession_CampusId",
                table: "ExamSession");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_PlanningElementId",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "AttemptNo",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "SourceStudentId",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "PlanningEducationElement");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "PlanningEducationElement");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "ExamSession");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ExamSession");

            migrationBuilder.DropColumn(
                name: "GroupLabel",
                table: "ExamSession");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ExamSession");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ExamSession");

            migrationBuilder.DropColumn(
                name: "PlanningElementId",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "Hold",
                table: "Enrollment");

            migrationBuilder.RenameColumn(
                name: "SourceImportId",
                table: "Enrollment",
                newName: "CourseId");

            migrationBuilder.AddColumn<string>(
                name: "Campus",
                table: "Room",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Campus",
                table: "Course",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "AssessmentEvent",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrollment",
                table: "Enrollment",
                columns: new[] { "StudentId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_CourseId",
                table: "Enrollment",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEvent_CourseId",
                table: "AssessmentEvent",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentEvent_Course_CourseId",
                table: "AssessmentEvent",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Course_CourseId",
                table: "Enrollment",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
