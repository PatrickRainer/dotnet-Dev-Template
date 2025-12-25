using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyDevTemplate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedServiceModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Services",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "Features", "Name", "TenantId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 1, 0, 0, 0, DateTimeKind.Local), "Basic business management platform.", "Dashboard,Analytics,Settings", "Service 1", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 1, 0, 0, 0, DateTimeKind.Local), "Enhanced business management and collaboration.", "Dashboard,Analytics,Settings,Reports,Notifications,UserManagement,Integration", "Service 2", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 1, 0, 0, 0, DateTimeKind.Local), "Enterprise-level business management and automation.", "Dashboard,Analytics,Settings,Reports,Notifications,UserManagement,Integration,AdvancedAnalytics,Automation,Security,ApiAccess,CustomWorkflows", "Service 3", new Guid("00000000-0000-0000-0000-000000000000") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Services",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));
        }
    }
}
