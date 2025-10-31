using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MultitenantPerDb.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class InitialTenantDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "ConnectionString", "CreatedAt", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "Server=BORA\\BRCTN;Database=Tenant1Db;Trusted_Connection=True;TrustServerCertificate=True;", new DateTime(2025, 10, 31, 22, 3, 2, 878, DateTimeKind.Utc).AddTicks(9415), true, "Tenant1" },
                    { 2, "Server=BORA\\BRCTN;Database=Tenant2Db;Trusted_Connection=True;TrustServerCertificate=True;", new DateTime(2025, 10, 31, 22, 3, 2, 878, DateTimeKind.Utc).AddTicks(9417), true, "Tenant2" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "PasswordHash", "TenantId", "Username" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 31, 22, 3, 2, 878, DateTimeKind.Utc).AddTicks(9540), "user1@tenant1.com", true, "$2a$11$5ZqJKbGjmJ5J5YqHxJH5XO5mZxJH5XO5mZxJH5XO5mZxJH5XO5mZx", 1, "user1" },
                    { 2, new DateTime(2025, 10, 31, 22, 3, 2, 878, DateTimeKind.Utc).AddTicks(9542), "user2@tenant2.com", true, "$2a$11$5ZqJKbGjmJ5J5YqHxJH5XO5mZxJH5XO5mZxJH5XO5mZxJH5XO5mZx", 2, "user2" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
