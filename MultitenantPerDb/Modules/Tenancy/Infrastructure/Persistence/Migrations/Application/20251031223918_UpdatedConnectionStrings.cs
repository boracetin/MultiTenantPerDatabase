using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultitenantPerDb.Migrations.Application
{
    /// <inheritdoc />
    public partial class UpdatedConnectionStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConnectionString", "CreatedAt" },
                values: new object[] { "Server=BORA\\\\BRCTN;Database=Tenant1Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;", new DateTime(2025, 10, 31, 22, 39, 18, 34, DateTimeKind.Utc).AddTicks(8684) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConnectionString", "CreatedAt" },
                values: new object[] { "Server=BORA\\\\BRCTN;Database=Tenant2Db;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;Max Pool Size=2000;", new DateTime(2025, 10, 31, 22, 39, 18, 34, DateTimeKind.Utc).AddTicks(8686) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 22, 39, 18, 34, DateTimeKind.Utc).AddTicks(8824));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 22, 39, 18, 34, DateTimeKind.Utc).AddTicks(8826));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConnectionString", "CreatedAt" },
                values: new object[] { "Server=BORA\\BRCTN;Database=Tenant1Db;Trusted_Connection=True;TrustServerCertificate=True;", new DateTime(2025, 10, 31, 22, 3, 2, 878, DateTimeKind.Utc).AddTicks(9415) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConnectionString", "CreatedAt" },
                values: new object[] { "Server=BORA\\BRCTN;Database=Tenant2Db;Trusted_Connection=True;TrustServerCertificate=True;", new DateTime(2025, 10, 31, 22, 3, 2, 878, DateTimeKind.Utc).AddTicks(9417) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 22, 3, 2, 878, DateTimeKind.Utc).AddTicks(9540));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 10, 31, 22, 3, 2, 878, DateTimeKind.Utc).AddTicks(9542));
        }
    }
}

