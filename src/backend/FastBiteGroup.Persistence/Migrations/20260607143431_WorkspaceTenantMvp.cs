using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FastBiteGroup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkspaceTenantMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Unique_User_Workspace_Invitation",
                table: "UserWorkspaceInvitations");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WorkspaceMembers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Active");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "UserWorkspaceInvitations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "InvitedUserID",
                table: "UserWorkspaceInvitations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpiresAt",
                table: "UserWorkspaceInvitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvitedEmail",
                table: "UserWorkspaceInvitations",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "UserWorkspaceInvitations" AS invitation
                SET "InvitedEmail" = LOWER("AppUser"."Email")
                FROM "AppUser"
                WHERE invitation."InvitedUserID" = "AppUser"."Id"
                  AND invitation."InvitedEmail" = ''
                  AND "AppUser"."Email" IS NOT NULL
                """);

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspaceInvitations_InvitedEmail_WorkspaceID",
                table: "UserWorkspaceInvitations",
                columns: new[] { "InvitedEmail", "WorkspaceID" });

            migrationBuilder.CreateIndex(
                name: "IX_UserWorkspaceInvitations_InvitedUserID",
                table: "UserWorkspaceInvitations",
                column: "InvitedUserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserWorkspaceInvitations_InvitedEmail_WorkspaceID",
                table: "UserWorkspaceInvitations");

            migrationBuilder.DropIndex(
                name: "IX_UserWorkspaceInvitations_InvitedUserID",
                table: "UserWorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WorkspaceMembers");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "UserWorkspaceInvitations");

            migrationBuilder.DropColumn(
                name: "InvitedEmail",
                table: "UserWorkspaceInvitations");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "UserWorkspaceInvitations",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "InvitedUserID",
                table: "UserWorkspaceInvitations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unique_User_Workspace_Invitation",
                table: "UserWorkspaceInvitations",
                columns: new[] { "InvitedUserID", "WorkspaceID" },
                unique: true);
        }
    }
}
