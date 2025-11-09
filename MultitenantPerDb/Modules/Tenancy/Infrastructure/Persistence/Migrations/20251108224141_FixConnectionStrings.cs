using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MultitenantPerDb.Migrations
{
    /// <inheritdoc />
    public partial class FixConnectionStrings : Migration
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
                    Subdomain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BackgroundImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrimaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    SecondaryColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    CustomCss = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "BackgroundImageUrl", "ConnectionString", "CreatedAt", "CustomCss", "DeletedAt", "DisplayName", "IsActive", "IsDeleted", "LogoUrl", "Name", "PrimaryColor", "SecondaryColor", "Subdomain", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "Server=BORA\\BRCTN;Database=Tenant1Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;", new DateTime(2025, 11, 8, 22, 41, 41, 135, DateTimeKind.Utc).AddTicks(5923), null, null, "Tenant 1 Company", true, false, null, "Tenant1", "#1976D2", "#424242", "tenant1", null },
                    { 2, null, "Server=BORA\\BRCTN;Database=Tenant2Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;", new DateTime(2025, 11, 8, 22, 41, 41, 135, DateTimeKind.Utc).AddTicks(5929), null, null, "Tenant 2 Corporation", true, false, null, "Tenant2", "#D32F2F", "#616161", "tenant2", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants",
                column: "Subdomain",
                unique: true,
                filter: "[Subdomain] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
