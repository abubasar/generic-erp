using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Application.Core.Migrations
{
    /// <inheritdoc />
    public partial class SeedSignupPlansAndPriceBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Plan",
                columns: new[] { "Key", "Description", "IsActive", "IsPublic", "ModuleKeys", "Name", "Quotas", "SortOrder" },
                values: new object[,]
                {
                    { "business", "For a growing team with more than one location.", true, true, "configuration,inventory,purchase,sales,production,accounts,report", "Business", "max_users=8,max_branches=3,max_pos_terminals=2", 2 },
                    { "enterprise", "For a large business or group of companies.", true, true, "configuration,inventory,purchase,sales,production,accounts,report", "Enterprise", "max_users=50,max_branches=20,max_pos_terminals=10", 3 },
                    { "starter", "For a small shop just getting started.", true, true, "configuration,inventory,purchase,sales,production,accounts,report", "Starter", "max_users=2,max_branches=1,max_pos_terminals=0", 1 }
                });

            migrationBuilder.InsertData(
                table: "PriceBook",
                columns: new[] { "Id", "CreatedOn", "Currency", "Note", "PublishedOn", "Status", "UpdatedOn", "Version" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BDT", "Initial seed price book.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Published", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.InsertData(
                table: "PriceBookEntry",
                columns: new[] { "Id", "ItemKey", "ItemType", "MonthlyPrice", "PriceBookId", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), "starter", "plan", 1500m, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("11111111-1111-1111-1111-111111111102"), "business", "plan", 3500m, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("11111111-1111-1111-1111-111111111103"), "enterprise", "plan", 8000m, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("11111111-1111-1111-1111-111111111111"), "max_users", "unit", 0m, new Guid("11111111-1111-1111-1111-111111111111"), 300m },
                    { new Guid("11111111-1111-1111-1111-111111111112"), "max_branches", "unit", 0m, new Guid("11111111-1111-1111-1111-111111111111"), 500m },
                    { new Guid("11111111-1111-1111-1111-111111111113"), "max_pos_terminals", "unit", 0m, new Guid("11111111-1111-1111-1111-111111111111"), 350m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Plan",
                keyColumn: "Key",
                keyValue: "business");

            migrationBuilder.DeleteData(
                table: "Plan",
                keyColumn: "Key",
                keyValue: "enterprise");

            migrationBuilder.DeleteData(
                table: "Plan",
                keyColumn: "Key",
                keyValue: "starter");

            migrationBuilder.DeleteData(
                table: "PriceBookEntry",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111101"));

            migrationBuilder.DeleteData(
                table: "PriceBookEntry",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111102"));

            migrationBuilder.DeleteData(
                table: "PriceBookEntry",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111103"));

            migrationBuilder.DeleteData(
                table: "PriceBookEntry",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "PriceBookEntry",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111112"));

            migrationBuilder.DeleteData(
                table: "PriceBookEntry",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111113"));

            migrationBuilder.DeleteData(
                table: "PriceBook",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
