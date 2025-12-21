using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyDevTemplate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityProviderId",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityProviderId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAtUtc",
                table: "Users");
        }
    }
}
