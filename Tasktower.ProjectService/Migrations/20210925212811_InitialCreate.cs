using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tasktower.ProjectService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    picture = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "task_boards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, defaultValue: ""),
                    columns = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "[]"),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_boards", x => x.id);
                    table.ForeignKey(
                        name: "FK_task_boards_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    pending_invite = table.Column<bool>(type: "bit", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    project_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_roles_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mk_description = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: ""),
                    column = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    task_board_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_by = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    modified_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_tasks_task_boards_task_board_id",
                        column: x => x.task_board_id,
                        principalTable: "task_boards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_project_id_user_id",
                table: "project_roles",
                columns: new[] { "project_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_roles_user_id",
                table: "project_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_task_boards_project_id",
                table: "task_boards",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_task_board_id",
                table: "tasks",
                column: "task_board_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_roles");

            migrationBuilder.DropTable(
                name: "tasks");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "task_boards");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
