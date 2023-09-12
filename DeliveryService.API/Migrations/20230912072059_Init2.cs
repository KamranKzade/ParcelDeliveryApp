using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryServer.API.Migrations
{
    public partial class Init2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "OrderDeliveries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DestinationAddress",
                table: "OrderDeliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "OrderDeliveries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "OrderDeliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "OrderDeliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "OrderDeliveries");

            migrationBuilder.DropColumn(
                name: "DestinationAddress",
                table: "OrderDeliveries");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "OrderDeliveries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OrderDeliveries");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "OrderDeliveries");
        }
    }
}
