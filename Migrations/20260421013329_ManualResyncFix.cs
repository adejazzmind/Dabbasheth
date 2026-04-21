using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dabbasheth.Migrations
{
    public partial class ManualResyncFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 🟢 STEP 1: ADD MISSING COLUMNS ---

            // This is the one we actually need for the Payout logic
            migrationBuilder.AddColumn<DateTime>(
                name: "payoutdate",
                table: "thriftplans",
                type: "timestamp with time zone",
                nullable: true);

            // --- 🟡 STEP 2: COMMENTED OUT EXISTING COLUMNS ---
            // We comment this out because PostgreSQL told us "createdat" already exists.
            /*
            migrationBuilder.AddColumn<DateTime>(
                name: "createdat",
                table: "thriftgroups",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            */

            // --- 🟡 STEP 3: COMMENTED OUT PRIMARY KEY SHUFFLING ---
            // EF Core tries to rename keys from lowercase to uppercase (pk_ -> PK_). 
            // In a live Neon DB, this often fails if data exists. Best to skip it.
            /*
            migrationBuilder.DropPrimaryKey(name: "pk_wallets", table: "wallets");
            migrationBuilder.AddPrimaryKey(name: "PK_wallets", table: "wallets", column: "id");
            ... and so on ...
            */
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payoutdate",
                table: "thriftplans");
        }
    }
}