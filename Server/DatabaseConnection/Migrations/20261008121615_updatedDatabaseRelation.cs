using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseConnection.Migrations
{
    /// <inheritdoc />
    public partial class updatedDatabaseRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollment_Class_ClassId",
                table: "Enrollment");

            migrationBuilder.DropIndex(
                name: "IX_Enrollment_ClassId",
                table: "Enrollment");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Enrollment");

            migrationBuilder.CreateTable(
                name: "PlanningEducationElement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceChecksum = table.Column<string>(type: "text", nullable: false),
                    SourceModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Nickmane = table.Column<string>(type: "text", nullable: false),
                    ECTS = table.Column<int>(type: "integer", nullable: false),
                    ExecutionElementCount = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ActivityOffering = table.Column<string>(type: "text", nullable: false),
                    Prefix = table.Column<string>(type: "text", nullable: false),
                    AssessmenType = table.Column<int>(type: "integer", nullable: false),
                    CompletionAssesment = table.Column<bool>(type: "boolean", nullable: false),
                    GradingScale = table.Column<string>(type: "text", nullable: false),
                    CombinedWrittenOral = table.Column<bool>(type: "boolean", nullable: false),
                    Oral = table.Column<bool>(type: "boolean", nullable: false),
                    PracticalExam = table.Column<bool>(type: "boolean", nullable: false),
                    Project = table.Column<bool>(type: "boolean", nullable: false),
                    Written = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanningEducationElement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teacher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teacher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teacher_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentEvent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    PlanningElementId = table.Column<int>(type: "integer", nullable: false),
                    ExamType = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentEvent_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentEvent_PlanningEducationElement_PlanningElementId",
                        column: x => x.PlanningElementId,
                        principalTable: "PlanningEducationElement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamDay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExamDayType = table.Column<int>(type: "integer", nullable: false),
                    Usable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamDay_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleReview",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleReview", x => new { x.ScheduleId, x.TeacherId });
                    table.ForeignKey(
                        name: "FK_ScheduleReview_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleReview_Teacher_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherConstraint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    ConstraintDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ConstraintType = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherConstraint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherConstraint_Teacher_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentHandIn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssessmentEventId = table.Column<int>(type: "integer", nullable: false),
                    HandInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Part = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentHandIn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentHandIn_AssessmentEvent_AssessmentEventId",
                        column: x => x.AssessmentEventId,
                        principalTable: "AssessmentEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AsseessmentId = table.Column<int>(type: "integer", nullable: false),
                    ScheduleId = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    LockDate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSession_AssessmentEvent_AsseessmentId",
                        column: x => x.AsseessmentId,
                        principalTable: "AssessmentEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSession_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamSession_Schedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentExamAssignment",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    ExamSessionId = table.Column<int>(type: "integer", nullable: false),
                    ExtraTime = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentExamAssignment", x => new { x.StudentId, x.ExamSessionId });
                    table.ForeignKey(
                        name: "FK_StudentExamAssignment_ExamSession_ExamSessionId",
                        column: x => x.ExamSessionId,
                        principalTable: "ExamSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentExamAssignment_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAssignment",
                columns: table => new
                {
                    ExamSessionId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAssignment", x => new { x.ExamSessionId, x.TeacherId });
                    table.ForeignKey(
                        name: "FK_TeacherAssignment_ExamSession_ExamSessionId",
                        column: x => x.ExamSessionId,
                        principalTable: "ExamSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherAssignment_Teacher_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teacher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEvent_CourseId",
                table: "AssessmentEvent",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEvent_PlanningElementId",
                table: "AssessmentEvent",
                column: "PlanningElementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentHandIn_AssessmentEventId",
                table: "AssessmentHandIn",
                column: "AssessmentEventId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamDay_ScheduleId",
                table: "ExamDay",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSession_AsseessmentId",
                table: "ExamSession",
                column: "AsseessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSession_RoomId",
                table: "ExamSession",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamSession_ScheduleId",
                table: "ExamSession",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleReview_TeacherId",
                table: "ScheduleReview",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentExamAssignment_ExamSessionId",
                table: "StudentExamAssignment",
                column: "ExamSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Teacher_UserId",
                table: "Teacher",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignment_TeacherId",
                table: "TeacherAssignment",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherConstraint_TeacherId",
                table: "TeacherConstraint",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentHandIn");

            migrationBuilder.DropTable(
                name: "ExamDay");

            migrationBuilder.DropTable(
                name: "ScheduleReview");

            migrationBuilder.DropTable(
                name: "StudentExamAssignment");

            migrationBuilder.DropTable(
                name: "TeacherAssignment");

            migrationBuilder.DropTable(
                name: "TeacherConstraint");

            migrationBuilder.DropTable(
                name: "ExamSession");

            migrationBuilder.DropTable(
                name: "Teacher");

            migrationBuilder.DropTable(
                name: "AssessmentEvent");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropTable(
                name: "PlanningEducationElement");

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "Enrollment",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_ClassId",
                table: "Enrollment",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollment_Class_ClassId",
                table: "Enrollment",
                column: "ClassId",
                principalTable: "Class",
                principalColumn: "Id");
        }
    }
}
