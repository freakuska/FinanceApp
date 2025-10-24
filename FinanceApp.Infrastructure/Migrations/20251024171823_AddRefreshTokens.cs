using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2025, 10, 24, 17, 18, 23, 623, DateTimeKind.Utc).AddTicks(9660));

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2025, 10, 24, 17, 18, 23, 623, DateTimeKind.Utc).AddTicks(9900));

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2025, 10, 24, 17, 18, 23, 623, DateTimeKind.Utc).AddTicks(9900));

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2025, 10, 24, 17, 18, 23, 623, DateTimeKind.Utc).AddTicks(9900));

            migrationBuilder.UpdateData(
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 24, 17, 18, 23, 624, DateTimeKind.Utc).AddTicks(4340), new DateTime(2025, 10, 24, 17, 18, 23, 624, DateTimeKind.Utc).AddTicks(4440) });

            migrationBuilder.UpdateData(
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 24, 17, 18, 23, 624, DateTimeKind.Utc).AddTicks(4540), new DateTime(2025, 10, 24, 17, 18, 23, 624, DateTimeKind.Utc).AddTicks(4540) });

            migrationBuilder.UpdateData(
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 24, 17, 18, 23, 624, DateTimeKind.Utc).AddTicks(4550), new DateTime(2025, 10, 24, 17, 18, 23, 624, DateTimeKind.Utc).AddTicks(4560) });

            migrationBuilder.UpdateData(
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("a4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 24, 17, 18, 23, 624, DateTimeKind.Utc).AddTicks(4560), new DateTime(2025, 10, 24, 17, 18, 23, 624, DateTimeKind.Utc).AddTicks(4560) });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "created_at",
                value: new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(3978));

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "created_at",
                value: new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(4148));

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "created_at",
                value: new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(4151));

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "created_at",
                value: new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(4153));

            migrationBuilder.UpdateData(
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9207), new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9283) });

            migrationBuilder.UpdateData(
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9360), new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9361) });

            migrationBuilder.UpdateData(
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9364), new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9364) });

            migrationBuilder.UpdateData(
                table: "tags",
                keyColumn: "id",
                keyValue: new Guid("a4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9367), new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9367) });
        }
    }
}
