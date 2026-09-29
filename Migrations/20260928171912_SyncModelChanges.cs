using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dabbasheth.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No-op: schema already matches the current model
            // (chatmessages and lowercase constraint names already exist in the database).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op
        }
    }
}
