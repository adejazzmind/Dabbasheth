using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dabbasheth.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupIdToThriftPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ThriftGroupId",
                table: "ThriftPlans",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ThriftGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupName = table.Column<string>(type: "text", nullable: false),
                    MonthlyContribution = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalMembers = table.Column<int>(type: "integer", nullable: false),
                    Frequency = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThriftGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThriftPlans_ThriftGroupId",
                table: "ThriftPlans",
                column: "ThriftGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ThriftPlans_ThriftGroups_ThriftGroupId",
                table: "ThriftPlans",
                column: "ThriftGroupId",
                principalTable: "ThriftGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThriftPlans_ThriftGroups_ThriftGroupId",
                table: "ThriftPlans");

            migrationBuilder.DropTable(
                name: "ThriftGroups");

            migrationBuilder.DropIndex(
                name: "IX_ThriftPlans_ThriftGroupId",
                table: "ThriftPlans");

            migrationBuilder.DropColumn(
                name: "ThriftGroupId",
                table: "ThriftPlans");
        }
    }
}
