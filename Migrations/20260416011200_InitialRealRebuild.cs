using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dabbasheth.Migrations
{
    /// <inheritdoc />
    public partial class InitialRealRebuild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThriftPlans_ThriftGroups_ThriftGroupId",
                table: "ThriftPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThriftPlans",
                table: "ThriftPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThriftGroups",
                table: "ThriftGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SystemSettings",
                table: "SystemSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTickets",
                table: "SupportTickets");

            migrationBuilder.RenameTable(
                name: "Wallets",
                newName: "wallets");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "transactions");

            migrationBuilder.RenameTable(
                name: "ThriftPlans",
                newName: "thriftplans");

            migrationBuilder.RenameTable(
                name: "ThriftGroups",
                newName: "thriftgroups");

            migrationBuilder.RenameTable(
                name: "SystemSettings",
                newName: "systemsettings");

            migrationBuilder.RenameTable(
                name: "SupportTickets",
                newName: "supporttickets");

            migrationBuilder.RenameColumn(
                name: "WalletNumber",
                table: "wallets",
                newName: "walletnumber");

            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "wallets",
                newName: "useremail");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "wallets",
                newName: "currency");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "wallets",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "wallets",
                newName: "balance");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "wallets",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "users",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "users",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "IsVerified",
                table: "users",
                newName: "isverified");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "users",
                newName: "fullname");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "users",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "transactions",
                newName: "useremail");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "transactions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "transactions",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "transactions",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "transactions",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "transactions",
                newName: "reference");

            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "thriftplans",
                newName: "useremail");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "thriftplans",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "ThriftGroupId",
                table: "thriftplans",
                newName: "thriftgroupid");

            migrationBuilder.RenameColumn(
                name: "TargetAmount",
                table: "thriftplans",
                newName: "targetamount");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "thriftplans",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "thriftplans",
                newName: "startdate");

            migrationBuilder.RenameColumn(
                name: "PayoutOrder",
                table: "thriftplans",
                newName: "payoutorder");

            migrationBuilder.RenameColumn(
                name: "MaturityDate",
                table: "thriftplans",
                newName: "maturitydate");

            migrationBuilder.RenameColumn(
                name: "HasCollected",
                table: "thriftplans",
                newName: "hascollected");

            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "thriftplans",
                newName: "frequency");

            migrationBuilder.RenameColumn(
                name: "CurrentSavings",
                table: "thriftplans",
                newName: "currentsavings");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "thriftplans",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_ThriftPlans_ThriftGroupId",
                table: "thriftplans",
                newName: "IX_thriftplans_thriftgroupid");

            migrationBuilder.RenameColumn(
                name: "TotalMembers",
                table: "thriftgroups",
                newName: "totalmembers");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "thriftgroups",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "thriftgroups",
                newName: "startdate");

            migrationBuilder.RenameColumn(
                name: "MonthlyContribution",
                table: "thriftgroups",
                newName: "monthlycontribution");

            migrationBuilder.RenameColumn(
                name: "GroupName",
                table: "thriftgroups",
                newName: "groupname");

            migrationBuilder.RenameColumn(
                name: "Frequency",
                table: "thriftgroups",
                newName: "frequency");

            migrationBuilder.RenameColumn(
                name: "DurationMonths",
                table: "thriftgroups",
                newName: "durationmonths");

            migrationBuilder.RenameColumn(
                name: "CategoryAmount",
                table: "thriftgroups",
                newName: "categoryamount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "thriftgroups",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SettingValue",
                table: "systemsettings",
                newName: "settingvalue");

            migrationBuilder.RenameColumn(
                name: "SettingKey",
                table: "systemsettings",
                newName: "settingkey");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "systemsettings",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "systemsettings",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "supporttickets",
                newName: "useremail");

            migrationBuilder.RenameColumn(
                name: "Subject",
                table: "supporttickets",
                newName: "subject");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "supporttickets",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "supporttickets",
                newName: "priority");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "supporttickets",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "supporttickets",
                newName: "createdat");

            migrationBuilder.RenameColumn(
                name: "AdminResponse",
                table: "supporttickets",
                newName: "adminresponse");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "supporttickets",
                newName: "id");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "transactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "pk_wallets",
                table: "wallets",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_users",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_transactions",
                table: "transactions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_thriftplans",
                table: "thriftplans",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_thriftgroups",
                table: "thriftgroups",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_systemsettings",
                table: "systemsettings",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_supporttickets",
                table: "supporttickets",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_thriftplans_thriftgroups_thriftgroupid",
                table: "thriftplans",
                column: "thriftgroupid",
                principalTable: "thriftgroups",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_thriftplans_thriftgroups_thriftgroupid",
                table: "thriftplans");

            migrationBuilder.DropPrimaryKey(
                name: "pk_wallets",
                table: "wallets");

            migrationBuilder.DropPrimaryKey(
                name: "pk_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_transactions",
                table: "transactions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_thriftplans",
                table: "thriftplans");

            migrationBuilder.DropPrimaryKey(
                name: "pk_thriftgroups",
                table: "thriftgroups");

            migrationBuilder.DropPrimaryKey(
                name: "pk_systemsettings",
                table: "systemsettings");

            migrationBuilder.DropPrimaryKey(
                name: "pk_supporttickets",
                table: "supporttickets");

            migrationBuilder.DropColumn(
                name: "id",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "type",
                table: "transactions");

            migrationBuilder.RenameTable(
                name: "wallets",
                newName: "Wallets");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "transactions",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "thriftplans",
                newName: "ThriftPlans");

            migrationBuilder.RenameTable(
                name: "thriftgroups",
                newName: "ThriftGroups");

            migrationBuilder.RenameTable(
                name: "systemsettings",
                newName: "SystemSettings");

            migrationBuilder.RenameTable(
                name: "supporttickets",
                newName: "SupportTickets");

            migrationBuilder.RenameColumn(
                name: "walletnumber",
                table: "Wallets",
                newName: "WalletNumber");

            migrationBuilder.RenameColumn(
                name: "useremail",
                table: "Wallets",
                newName: "UserEmail");

            migrationBuilder.RenameColumn(
                name: "currency",
                table: "Wallets",
                newName: "Currency");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "Wallets",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "balance",
                table: "Wallets",
                newName: "Balance");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Wallets",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "Users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "isverified",
                table: "Users",
                newName: "IsVerified");

            migrationBuilder.RenameColumn(
                name: "fullname",
                table: "Users",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "useremail",
                table: "Transactions",
                newName: "UserEmail");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Transactions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "reference",
                table: "Transactions",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Transactions",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "Transactions",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Transactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "useremail",
                table: "ThriftPlans",
                newName: "UserEmail");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "ThriftPlans",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "thriftgroupid",
                table: "ThriftPlans",
                newName: "ThriftGroupId");

            migrationBuilder.RenameColumn(
                name: "targetamount",
                table: "ThriftPlans",
                newName: "TargetAmount");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "ThriftPlans",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "startdate",
                table: "ThriftPlans",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "payoutorder",
                table: "ThriftPlans",
                newName: "PayoutOrder");

            migrationBuilder.RenameColumn(
                name: "maturitydate",
                table: "ThriftPlans",
                newName: "MaturityDate");

            migrationBuilder.RenameColumn(
                name: "hascollected",
                table: "ThriftPlans",
                newName: "HasCollected");

            migrationBuilder.RenameColumn(
                name: "frequency",
                table: "ThriftPlans",
                newName: "Frequency");

            migrationBuilder.RenameColumn(
                name: "currentsavings",
                table: "ThriftPlans",
                newName: "CurrentSavings");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ThriftPlans",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_thriftplans_thriftgroupid",
                table: "ThriftPlans",
                newName: "IX_ThriftPlans_ThriftGroupId");

            migrationBuilder.RenameColumn(
                name: "totalmembers",
                table: "ThriftGroups",
                newName: "TotalMembers");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "ThriftGroups",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "startdate",
                table: "ThriftGroups",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "monthlycontribution",
                table: "ThriftGroups",
                newName: "MonthlyContribution");

            migrationBuilder.RenameColumn(
                name: "groupname",
                table: "ThriftGroups",
                newName: "GroupName");

            migrationBuilder.RenameColumn(
                name: "frequency",
                table: "ThriftGroups",
                newName: "Frequency");

            migrationBuilder.RenameColumn(
                name: "durationmonths",
                table: "ThriftGroups",
                newName: "DurationMonths");

            migrationBuilder.RenameColumn(
                name: "categoryamount",
                table: "ThriftGroups",
                newName: "CategoryAmount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ThriftGroups",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "settingvalue",
                table: "SystemSettings",
                newName: "SettingValue");

            migrationBuilder.RenameColumn(
                name: "settingkey",
                table: "SystemSettings",
                newName: "SettingKey");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "SystemSettings",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SystemSettings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "useremail",
                table: "SupportTickets",
                newName: "UserEmail");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "SupportTickets",
                newName: "Subject");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "SupportTickets",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "priority",
                table: "SupportTickets",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "SupportTickets",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "createdat",
                table: "SupportTickets",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "adminresponse",
                table: "SupportTickets",
                newName: "AdminResponse");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SupportTickets",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Reference");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThriftPlans",
                table: "ThriftPlans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThriftGroups",
                table: "ThriftGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SystemSettings",
                table: "SystemSettings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTickets",
                table: "SupportTickets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ThriftPlans_ThriftGroups_ThriftGroupId",
                table: "ThriftPlans",
                column: "ThriftGroupId",
                principalTable: "ThriftGroups",
                principalColumn: "Id");
        }
    }
}
