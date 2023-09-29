using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderServer.API.Migrations
{
    public partial class AddOutBoxTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSend",
                table: "Orders",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsSend",
                table: "Orders");
        }
    }
}
