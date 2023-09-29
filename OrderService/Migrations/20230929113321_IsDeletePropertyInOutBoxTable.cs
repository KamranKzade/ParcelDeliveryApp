using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderServer.API.Migrations
{
    public partial class IsDeletePropertyInOutBoxTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "OutBoxes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "OutBoxes");
        }
    }
}
