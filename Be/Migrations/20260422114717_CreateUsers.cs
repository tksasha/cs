using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Be.Migrations
{
    /// <inheritdoc />
    public partial class CreateUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recorded_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamptz"),
                    recorded_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "'infinity'::timestamptz")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => new { x.id, x.valid_from, x.recorded_from });
                });

            migrationBuilder.Sql(@"
                CREATE EXTENSION IF NOT EXISTS btree_gist;

                ALTER TABLE users
                    ADD COLUMN valid_period tstzrange
                    GENERATED ALWAYS AS (tstzrange(valid_from, valid_to)) STORED;

                ALTER TABLE users
                    ADD COLUMN recorded_period tstzrange
                    GENERATED ALWAYS AS (tstzrange(recorded_from, recorded_to)) STORED;

                ALTER TABLE USERS
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
                name: "users");
        }
    }
}
