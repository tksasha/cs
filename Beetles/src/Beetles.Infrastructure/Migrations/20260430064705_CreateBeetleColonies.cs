using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Beetles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateBeetleColonies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beetle_colonies",
                columns: table => new
                {
                    beetle_id = table.Column<int>(type: "integer", nullable: false),
                    colony_id = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recorded_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "'infinity'::timestamptz"),
                    recorded_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamptz")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beetle_colonies", x => new { x.beetle_id, x.colony_id, x.valid_from, x.recorded_from });
                    table.ForeignKey(
                        name: "fk_beetle_colonies_beetles_beetle_id",
                        column: x => x.beetle_id,
                        principalTable: "beetles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_beetle_colonies_colonies_colony_id",
                        column: x => x.colony_id,
                        principalTable: "colonies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_beetle_colonies_colony_id",
                table: "beetle_colonies",
                column: "colony_id");

            migrationBuilder.Sql(@"
                CREATE EXTENSION IF NOT EXISTS btree_gist;

                ALTER TABLE beetle_colonies
                    ADD COLUMN valid_period tstzrange
                    GENERATED ALWAYS AS (tstzrange(valid_from, valid_to)) STORED;

                ALTER TABLE beetle_colonies
                    ADD COLUMN recorded_period tstzrange
                    GENERATED ALWAYS AS (tstzrange(recorded_from, recorded_to)) STORED;

                ALTER TABLE beetle_colonies
                    ADD CONSTRAINT no_overlap
                    EXCLUDE USING gist (
                        beetle_id WITH =,
                        colony_id WITH =,
                        valid_period WITH &&,
                        recorded_period WITH &&
                    );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beetle_colonies");
        }
    }
}
