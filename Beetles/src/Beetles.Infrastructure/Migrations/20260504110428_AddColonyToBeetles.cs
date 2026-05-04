using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beetles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColonyToBeetles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "colony_id",
                table: "beetles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_beetles_colony_id",
                table: "beetles",
                column: "colony_id");

            migrationBuilder.AddForeignKey(
                name: "fk_beetles_colonies_colony_id",
                table: "beetles",
                column: "colony_id",
                principalTable: "colonies",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_beetles_colonies_colony_id",
                table: "beetles");

            migrationBuilder.DropIndex(
                name: "ix_beetles_colony_id",
                table: "beetles");

            migrationBuilder.DropColumn(
                name: "colony_id",
                table: "beetles");
        }
    }
}
