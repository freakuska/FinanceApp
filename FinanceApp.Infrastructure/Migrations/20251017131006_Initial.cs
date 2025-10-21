using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    permissions = table.Column<string>(type: "jsonb", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                },
                comment: "Роли пользователей в системе");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    login = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    settings = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                },
                comment: "Пользователи системы");

            migrationBuilder.CreateTable(
                name: "financial_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    operation_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "RUB")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_financial_operations_creator",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_financial_operations_owner",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_financial_operations_updater",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Финансовые операции пользователей");

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: true),
                    path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    sort_order = table.Column<int>(type: "integer", nullable: true),
                    usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    visibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Private")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_tags_owner",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tags_parent",
                        column: x => x.parent_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Категории и теги для операций");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_roles_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_roles",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Связь пользователей и ролей");

            migrationBuilder.CreateTable(
                name: "operation_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_operation_tags_operations",
                        column: x => x.operation_id,
                        principalTable: "financial_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_operation_tags_tags",
                        column: x => x.tag_id,
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Связь операций и тегов (многие-ко-многим)");

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "code", "created_at", "description", "is_system", "name", "permissions" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "SUPER_ADMIN", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(3978), "Полный доступ ко всей системе", true, "Суперадминистратор", "[\"*\"]" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "ADMIN", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(4148), "Управление пользователями и настройками", true, "Администратор", "[\"users.manage\",\"settings.manage\",\"roles.manage\"]" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "MANAGER", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(4151), "Просмотр отчетов и аналитики", true, "Менеджер", "[\"reports.view\",\"analytics.view\",\"operations.view\"]" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "USER", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(4153), "Операции с собственными данными", true, "Пользователь", "[\"operations.own.manage\",\"tags.own.manage\"]" }
                });

            migrationBuilder.InsertData(
                table: "tags",
                columns: new[] { "id", "color", "created_at", "icon", "is_active", "is_system", "level", "name", "owner_id", "parent_id", "path", "slug", "sort_order", "type", "updated_at", "visibility" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), "#4CAF50", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9207), "💰", true, true, 0, "Зарплата", null, null, "salary", "salary", null, "Income", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9283), "Public" },
                    { new Guid("a2222222-2222-2222-2222-222222222222"), "#F44336", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9360), "🛒", true, true, 0, "Продукты", null, null, "groceries", "groceries", null, "Expense", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9361), "Public" },
                    { new Guid("a3333333-3333-3333-3333-333333333333"), "#FF9800", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9364), "🚗", true, true, 0, "Транспорт", null, null, "transport", "transport", null, "Expense", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9364), "Public" },
                    { new Guid("a4444444-4444-4444-4444-444444444444"), "#9C27B0", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9367), "🎮", true, true, 0, "Развлечения", null, null, "entertainment", "entertainment", null, "Expense", new DateTime(2025, 10, 17, 13, 10, 5, 476, DateTimeKind.Utc).AddTicks(9367), "Public" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_financial_operations_created_by",
                table: "financial_operations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_financial_operations_updated_by",
                table: "financial_operations",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_operations_owner_date_active",
                table: "financial_operations",
                columns: new[] { "owner_id", "operation_datetime" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_operations_owner_type_date",
                table: "financial_operations",
                columns: new[] { "owner_id", "type", "operation_datetime" });

            migrationBuilder.CreateIndex(
                name: "IX_operation_tags_operation_tag",
                table: "operation_tags",
                columns: new[] { "operation_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operation_tags_tag_id",
                table: "operation_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_code",
                table: "roles",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_name_fulltext",
                table: "tags",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_tags_owner_visibility",
                table: "tags",
                columns: new[] { "owner_id", "visibility" });

            migrationBuilder.CreateIndex(
                name: "IX_tags_parent_id",
                table: "tags",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_tags_slug",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tags_type_active",
                table: "tags",
                columns: new[] { "type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_tags_usage_count_desc",
                table: "tags",
                column: "usage_count",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_assigned_by",
                table: "user_roles",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_role",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_active_verified",
                table: "users",
                columns: new[] { "is_active", "is_verified" });

            migrationBuilder.CreateIndex(
                name: "IX_users_email_active",
                table: "users",
                column: "email",
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "IX_users_last_login",
                table: "users",
                column: "last_login_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_login",
                table: "users",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_phone",
                table: "users",
                column: "phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "operation_tags");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "financial_operations");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
