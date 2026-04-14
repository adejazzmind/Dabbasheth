using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dabbasheth.Migrations
{
    /// <inheritdoc />
    public partial class AddTieredAjoCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCollected",
                table: "ThriftPlans",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PayoutOrder",
                table: "ThriftPlans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CategoryAmount",
                table: "ThriftGroups",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "DurationMonths",
                table: "ThriftGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCollected",
                table: "ThriftPlans");

            migrationBuilder.DropColumn(
                name: "PayoutOrder",
                table: "ThriftPlans");

            migrationBuilder.DropColumn(
                name: "CategoryAmount",
                table: "ThriftGroups");

            migrationBuilder.DropColumn(
                name: "DurationMonths",
                table: "ThriftGroups");
        }
    }
}
