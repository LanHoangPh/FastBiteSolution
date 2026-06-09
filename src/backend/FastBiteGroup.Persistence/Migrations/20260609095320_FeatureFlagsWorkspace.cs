using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastBiteGroup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FeatureFlagsWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkspaceType",
                table: "Workspaces");

            migrationBuilder.AddColumn<bool>(
                name: "IsChatEnabled",
                table: "Workspaces",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeedEnabled",
                table: "Workspaces",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChatEnabled",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "IsFeedEnabled",
                table: "Workspaces");

            migrationBuilder.AddColumn<string>(
                name: "WorkspaceType",
                table: "Workspaces",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
