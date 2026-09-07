using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Application.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessTemplateKey",
                table: "Tenant",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Tenant",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomDomain",
                table: "Tenant",
                type: "nvarchar(253)",
                maxLength: 253,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DbConnectionKey",
                table: "Tenant",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Tenant",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subdomain",
                table: "Tenant",
                type: "nvarchar(63)",
                maxLength: 63,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BusinessTemplate",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultModuleKeys = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IndustryProfileKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessTemplate", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Entitlement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Limit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entitlement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entitlement_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DependsOn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PermissionGroup = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsMetered = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrialEndsOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscription_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantSetting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSetting_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantModule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActivatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantModule_Module_ModuleKey",
                        column: x => x.ModuleKey,
                        principalTable: "Module",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantModule_Tenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BusinessTemplate",
                columns: new[] { "Key", "DefaultModuleKeys", "Description", "IndustryProfileKey", "IsPublic", "Name", "SortOrder" },
                values: new object[,]
                {
                    { "feed", "configuration,inventory,purchase,sales,production,accounts,report", "Animal feed / poultry feed manufacturing & distribution.", "feed", true, "Feed industry", 2 },
                    { "pharmacy", "configuration,inventory,purchase,sales,production,accounts,report", "Pharmaceutical manufacturing & distribution.", "pharmacy", true, "Pharmaceutical", 1 }
                });

            migrationBuilder.InsertData(
                table: "Module",
                columns: new[] { "Key", "Category", "DependsOn", "Description", "IsMetered", "Name", "PermissionGroup", "SortOrder" },
                values: new object[,]
                {
                    { "accounts", "Business", null, "Chart of accounts, journals, vouchers, fund transfers.", false, "Accounting", "Permissions.AccessModules.Accounts", 6 },
                    { "configuration", "Core", null, "Company, stores, products, customers, suppliers, settings.", false, "Configuration & masters", "Permissions.AccessModules.Configuration", 1 },
                    { "inventory", "Business", null, "Stock ledger, adjustments, transfers.", false, "Inventory & stock", "Permissions.AccessModules.Inventory", 2 },
                    { "production", "Business", "inventory", "BOM / formula, manufacturing orders, raw material.", false, "Production", "Permissions.AccessModules.Production", 5 },
                    { "purchase", "Business", "inventory", "Requisition, PO, GRN, purchase invoice, returns, LC.", false, "Purchase", "Permissions.AccessModules.Purchase", 3 },
                    { "report", "Business", null, "COGS, ledgers, stock and sales analysis.", false, "Reports & analytics", "Permissions.AccessModules.Report", 7 },
                    { "sales", "Business", "inventory", "Quotation, sale order, delivery note, invoice, returns.", false, "Sales", "Permissions.AccessModules.Sales", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenant_Subdomain",
                table: "Tenant",
                column: "Subdomain",
                unique: true,
                filter: "[Subdomain] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Entitlement_TenantId_Key",
                table: "Entitlement",
                columns: new[] { "TenantId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_TenantId",
                table: "Subscription",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModule_ModuleKey",
                table: "TenantModule",
                column: "ModuleKey");

            migrationBuilder.CreateIndex(
                name: "IX_TenantModule_TenantId_ModuleKey",
                table: "TenantModule",
                columns: new[] { "TenantId", "ModuleKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSetting_TenantId_Key",
                table: "TenantSetting",
                columns: new[] { "TenantId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessTemplate");

            migrationBuilder.DropTable(
                name: "Entitlement");

            migrationBuilder.DropTable(
                name: "Subscription");

            migrationBuilder.DropTable(
                name: "TenantModule");

            migrationBuilder.DropTable(
                name: "TenantSetting");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropIndex(
                name: "IX_Tenant_Subdomain",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "BusinessTemplateKey",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "CustomDomain",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "DbConnectionKey",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tenant");

            migrationBuilder.DropColumn(
                name: "Subdomain",
                table: "Tenant");
        }
    }
}
