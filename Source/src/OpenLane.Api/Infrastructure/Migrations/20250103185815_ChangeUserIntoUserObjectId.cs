using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenLane.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserIntoUserObjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "User",
                table: "Bids",
                newName: "UserObjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserObjectId",
                table: "Bids",
                newName: "User");
        }
    }
}
