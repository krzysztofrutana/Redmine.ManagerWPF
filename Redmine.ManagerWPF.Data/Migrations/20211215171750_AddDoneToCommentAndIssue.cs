using Microsoft.EntityFrameworkCore.Migrations;

namespace Redmine.ManagerWPF.Data.Migrations
{
    public partial class AddDoneToCommentAndIssue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Comments");

            migrationBuilder.AddColumn<bool>(
                name: "Done",
                table: "Issues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Done",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Done",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Done",
                table: "Comments");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
