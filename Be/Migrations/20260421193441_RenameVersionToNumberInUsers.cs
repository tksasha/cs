using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Be.Migrations
{
    /// <inheritdoc />
    public partial class RenameVersionToNumberInUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "version",
                table: "users",
                newName: "number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "number",
                table: "users",
                newName: "version");
        }
    }
}
