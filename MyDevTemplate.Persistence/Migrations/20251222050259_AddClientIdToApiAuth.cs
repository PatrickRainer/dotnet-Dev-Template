using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyDevTemplate.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientIdToApiAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "ApiKeys",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_ClientId",
                table: "ApiKeys",
                column: "ClientId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_ClientId",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ApiKeys");
        }
    }
}
