using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentsGradeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Student_Grades_GradeId",
                table: "Student");

            migrationBuilder.AlterColumn<int>(
                name: "GradeId",
                table: "Student",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Grades_GradeId",
                table: "Student",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Student_Grades_GradeId",
                table: "Student");

            migrationBuilder.AlterColumn<int>(
                name: "GradeId",
                table: "Student",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Student_Grades_GradeId",
                table: "Student",
                column: "GradeId",
                principalTable: "Grades",
                principalColumn: "Id");
        }
    }
}
