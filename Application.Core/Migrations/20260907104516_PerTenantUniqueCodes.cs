using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <summary>
    /// Multi-tenant document / master codes. These were created DB-first as GLOBAL
    /// UNIQUE KEY constraints — so a second tenant could not create a financial
    /// year, store, company, product, PO or SO whose code another tenant already
    /// used. Each becomes a unique index on <c>(TenantId, code)</c>.
    /// (EF's generated DropIndex can't drop a UNIQUE constraint — raw SQL.)
    /// </summary>
    public partial class PerTenantUniqueCodes : Migration
    {
        private static readonly (string table, string constraint, string col)[] Targets =
        {
            ("FinancialYear", "UQ_FinancialYear_Code", "Code"),
            ("Store",         "UQ_Store_Code",         "Code"),
            ("Company",       "UQ_Company_Code",       "Code"),
            ("Product",       "UQ_Code",               "Code"),
            ("PurchaseOrder", "UQ_PONumber",           "PONumber"),
            ("SaleOrder",     "UQ_SaleOrderNo",        "SaleOrderNo"),
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, constraint, col) in Targets)
                migrationBuilder.Sql(
                    $"ALTER TABLE dbo.[{table}] DROP CONSTRAINT [{constraint}];\n" +
                    $"CREATE UNIQUE INDEX [{constraint}] ON dbo.[{table}] ([TenantId], [{col}]);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, constraint, col) in Targets)
                migrationBuilder.Sql(
                    $"DROP INDEX [{constraint}] ON dbo.[{table}];\n" +
                    $"ALTER TABLE dbo.[{table}] ADD CONSTRAINT [{constraint}] UNIQUE ([{col}]);");
        }
    }
}
