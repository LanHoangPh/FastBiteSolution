using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FastBiteGroup.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorWorkspaceEntitiesToEntityBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentReports",
                table: "ContentReports");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContentReports");

            migrationBuilder.AlterColumn<int>(
                name: "ReportedContentID",
                table: "ContentReports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentReports",
                table: "ContentReports",
                column: "ReportedContentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentReports",
                table: "ContentReports");

            migrationBuilder.AlterColumn<int>(
                name: "ReportedContentID",
                table: "ContentReports",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ContentReports",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentReports",
                table: "ContentReports",
                column: "Id");
        }
    }
}
