using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Be.Migrations
{
    /// <inheritdoc />
    public partial class RenameNumberToFactInUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "number",
                table: "users",
                newName: "fact");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fact",
                table: "users",
                newName: "number");
        }
    }
}
