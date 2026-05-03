using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Beetles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateBeetles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beetles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recorded_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "'infinity'::timestamptz"),
                    recorded_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamptz")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beetles", x => new { x.id, x.valid_from, x.recorded_from });
                });

            migrationBuilder.Sql(@"
                CREATE EXTENSION IF NOT EXISTS btree_gist;

                ALTER TABLE beetles
                    ADD COLUMN valid_period tstzrange
                        GENERATED ALWAYS AS (tstzrange(valid_from, valid_to)) STORED;

                ALTER TABLE beetles
                    ADD COLUMN recorded_period tstzrange
                        GENERATED ALWAYS AS (tstzrange(recorded_from, recorded_to)) STORED;

                ALTER TABLE beetles
                    ADD CONSTRAINT no_overlap
                        EXCLUDE USING gist (
                            id WITH =,
                            valid_period WITH &&,
                            recorded_period WITH &&
                        );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beetles");
        }
    }
}
