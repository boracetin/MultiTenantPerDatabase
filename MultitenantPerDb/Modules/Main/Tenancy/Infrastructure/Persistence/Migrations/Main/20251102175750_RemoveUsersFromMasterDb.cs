using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MultitenantPerDb.Modules.Main.Tenancy.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RemoveUsersFromMasterDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "Tenants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomCss",
                table: "Tenants",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Tenants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Tenants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "Tenants",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "Tenants",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "Tenants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BackgroundImageUrl", "CreatedAt", "CustomCss", "DisplayName", "LogoUrl", "PrimaryColor", "SecondaryColor", "Subdomain" },
                values: new object[] { null, new DateTime(2025, 11, 2, 17, 57, 50, 456, DateTimeKind.Utc).AddTicks(2462), null, "Tenant 1 Company", null, "#1976D2", "#424242", "tenant1" });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BackgroundImageUrl", "CreatedAt", "CustomCss", "DisplayName", "LogoUrl", "PrimaryColor", "SecondaryColor", "Subdomain" },
                values: new object[] { null, new DateTime(2025, 11, 2, 17, 57, 50, 456, DateTimeKind.Utc).AddTicks(2465), null, "Tenant 2 Corporation", null, "#D32F2F", "#616161", "tenant2" });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants",
                column: "Subdomain",
                unique: true,
                filter: "[Subdomain] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_TenantId",
                table: "User",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CustomCss",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "Tenants");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 23, 24, 39, 248, DateTimeKind.Utc).AddTicks(4382));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 23, 24, 39, 248, DateTimeKind.Utc).AddTicks(4385));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "PasswordHash", "TenantId", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 31, 23, 24, 39, 248, DateTimeKind.Utc).AddTicks(4422), "user1@tenant1.com", true, "$2a$11$5ZqJKbGjmJ5J5YqHxJH5XO5mZxJH5XO5mZxJH5XO5mZxJH5XO5mZx", 1, null, "user1" },
                    { 2, new DateTime(2025, 10, 31, 23, 24, 39, 248, DateTimeKind.Utc).AddTicks(4423), "user2@tenant2.com", true, "$2a$11$5ZqJKbGjmJ5J5YqHxJH5XO5mZxJH5XO5mZxJH5XO5mZxJH5XO5mZx", 2, null, "user2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }
    }
}
