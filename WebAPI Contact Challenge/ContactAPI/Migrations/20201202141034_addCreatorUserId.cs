using Microsoft.EntityFrameworkCore.Migrations;

namespace ContactsAPI.Migrations
{
    public partial class addCreatorUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "Skill",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "Contact",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "Contact");
        }
    }
}
