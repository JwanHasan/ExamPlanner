using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseConnection.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDatabaseRelationv1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSession_AssessmentEvent_AsseessmentId",
                table: "ExamSession");

            migrationBuilder.RenameColumn(
                name: "AsseessmentId",
                table: "ExamSession",
                newName: "AsseessmenEventId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamSession_AsseessmentId",
                table: "ExamSession",
                newName: "IX_ExamSession_AsseessmenEventId");

            migrationBuilder.RenameColumn(
                name: "ETCS",
                table: "Course",
                newName: "ECTS");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Student",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "PlanningEducationElement",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "PlanningElementId",
                table: "AssessmentEvent",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSession_AssessmentEvent_AsseessmenEventId",
                table: "ExamSession",
                column: "AsseessmenEventId",
                principalTable: "AssessmentEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSession_AssessmentEvent_AsseessmenEventId",
                table: "ExamSession");

            migrationBuilder.RenameColumn(
                name: "AsseessmenEventId",
                table: "ExamSession",
                newName: "AsseessmentId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamSession_AsseessmenEventId",
                table: "ExamSession",
                newName: "IX_ExamSession_AsseessmentId");

            migrationBuilder.RenameColumn(
                name: "ECTS",
                table: "Course",
                newName: "ETCS");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Student",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "PlanningEducationElement",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "PlanningElementId",
                table: "AssessmentEvent",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSession_AssessmentEvent_AsseessmentId",
                table: "ExamSession",
                column: "AsseessmentId",
                principalTable: "AssessmentEvent",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
