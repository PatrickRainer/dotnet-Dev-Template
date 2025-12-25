using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyDevTemplate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameServiceToSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Tenants",
                newName: "SubscriptionId");

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "CreatedAtUtc", "Description", "Features", "Name", "TenantId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 1, 0, 0, 0, DateTimeKind.Local), "Basic business management platform.", "Dashboard,Analytics,Settings", "Subscription 1", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 1, 0, 0, 0, DateTimeKind.Local), "Enhanced business management and collaboration.", "Dashboard,Analytics,Settings,Reports,Notifications,UserManagement,Integration", "Subscription 2", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 1, 0, 0, 0, DateTimeKind.Local), "Enterprise-level business management and automation.", "Dashboard,Analytics,Settings,Reports,Notifications,UserManagement,Integration,AdvancedAnalytics,Automation,Security,ApiAccess,CustomWorkflows", "Subscription 3", new Guid("00000000-0000-0000-0000-000000000000") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "Tenants",
                newName: "ServiceId");

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

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
    }
}
