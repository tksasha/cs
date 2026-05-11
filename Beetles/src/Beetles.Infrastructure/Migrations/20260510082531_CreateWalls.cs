using System;

using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Beetles.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateWalls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "walls",
                columns: table => new
                {
                    transaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    color = table.Column<string>(type: "text", nullable: false),
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    business_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    business_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "'infinity'::timestamptz"),
                    system_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    system_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamptz")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_walls", x => x.transaction_id);
                });

            migrationBuilder.Sql(@"
                CREATE EXTENSION IF NOT EXISTS btree_gist;

                ALTER TABLE walls
                    ADD COLUMN business_period tstzrange
                    GENERATED ALWAYS AS (tstzrange(business_start, business_end)) STORED;

                ALTER TABLE walls
                    ADD COLUMN system_period tstzrange
                    GENERATED ALWAYS AS (tstzrange(system_start, system_end)) STORED;

                ALTER TABLE walls
                    ADD CONSTRAINT no_overlap_walls
                    EXCLUDE USING gist (
                        id WITH =,
                        business_period WITH &&,
                        system_period WITH &&
                    );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "walls");

            migrationBuilder.Sql("drop EXTENSION IF EXISTS btree_gist");
        }
    }
}
