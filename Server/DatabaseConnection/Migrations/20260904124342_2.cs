using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseConnection.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_examHandIn_Exam_ExamId",
                table: "examHandIn");

            migrationBuilder.DropPrimaryKey(
                name: "PK_examHandIn",
                table: "examHandIn");

            migrationBuilder.RenameTable(
                name: "examHandIn",
                newName: "ExamHandIn");

            migrationBuilder.RenameIndex(
                name: "IX_examHandIn_ExamId",
                table: "ExamHandIn",
                newName: "IX_ExamHandIn_ExamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamHandIn",
                table: "ExamHandIn",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamHandIn_Exam_ExamId",
                table: "ExamHandIn",
                column: "ExamId",
                principalTable: "Exam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamHandIn_Exam_ExamId",
                table: "ExamHandIn");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamHandIn",
                table: "ExamHandIn");

            migrationBuilder.RenameTable(
                name: "ExamHandIn",
                newName: "examHandIn");

            migrationBuilder.RenameIndex(
                name: "IX_ExamHandIn_ExamId",
                table: "examHandIn",
                newName: "IX_examHandIn_ExamId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_examHandIn",
                table: "examHandIn",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_examHandIn_Exam_ExamId",
                table: "examHandIn",
                column: "ExamId",
                principalTable: "Exam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
