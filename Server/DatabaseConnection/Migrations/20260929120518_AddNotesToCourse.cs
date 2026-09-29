using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseConnection.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ECTS",
                table: "Course",
                newName: "ECTS");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Course",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Course");

            migrationBuilder.RenameColumn(
                name: "ECTS",
                table: "Course",
                newName: "ECTS");
        }
    }
}
