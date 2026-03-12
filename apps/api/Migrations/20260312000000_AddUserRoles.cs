using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSaas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create UserRoles table
            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_Role_BranchId",
                table: "UserRoles",
                columns: new[] { "UserId", "Role", "BranchId" },
                unique: true);

            // 2. Migrate existing Users.Role data into UserRoles
            migrationBuilder.Sql(@"
                INSERT INTO ""UserRoles"" (""Id"", ""UserId"", ""Role"", ""RestaurantId"", ""BranchId"", ""AssignedAt"")
                SELECT gen_random_uuid(), ""Id"", ""Role"", ""RestaurantId"", ""BranchId"", NOW()
                FROM ""Users""
                WHERE ""Role"" IS NOT NULL AND ""Role"" <> '';
            ");

            // 3. Drop the old Role column from Users
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore Role column (nullable during rollback — can't guarantee single-role)
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "Cashier");

            // Copy first role back (best-effort)
            migrationBuilder.Sql(@"
                UPDATE ""Users"" u
                SET ""Role"" = (
                    SELECT ""Role"" FROM ""UserRoles"" ur
                    WHERE ur.""UserId"" = u.""Id""
                    ORDER BY ur.""AssignedAt"" ASC
                    LIMIT 1
                )
                WHERE EXISTS (
                    SELECT 1 FROM ""UserRoles"" ur WHERE ur.""UserId"" = u.""Id""
                );
            ");

            migrationBuilder.DropTable(name: "UserRoles");
        }
    }
}
