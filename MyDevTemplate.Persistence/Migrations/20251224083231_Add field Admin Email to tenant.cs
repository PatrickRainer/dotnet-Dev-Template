using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyDevTemplate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddfieldAdminEmailtotenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminEmail",
                table: "Tenants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminEmail",
                table: "Tenants");
        }
    }
}
