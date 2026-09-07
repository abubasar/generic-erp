using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <summary>
    /// Puts every existing tenant onto the platform layer: template from its
    /// BusinessType (1 = pharmacy, 2 = feed), an Active subscription, and a
    /// TenantModule row for each of its template's default modules. Idempotent.
    /// </summary>
    public partial class BackfillExistingTenants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET NOCOUNT ON;

-- 1. Template + status + currency on the Tenant row.
UPDATE t SET
    t.BusinessTemplateKey = CASE t.BusinessType WHEN 1 THEN 'pharmacy' WHEN 2 THEN 'feed' ELSE t.BusinessTemplateKey END,
    t.Status              = COALESCE(t.Status, 'Active'),
    t.Currency            = COALESCE(t.Currency, 'BDT')
FROM dbo.Tenant t
WHERE t.Deleted = 0 AND (t.BusinessTemplateKey IS NULL OR t.Status IS NULL);

-- 2. One Active subscription per tenant that has none.
INSERT INTO dbo.Subscription (Id, TenantId, PlanKey, Status, PeriodStart, PeriodEnd, TrialEndsOn)
SELECT NEWID(), t.Id, NULL, 'Active', SYSUTCDATETIME(), DATEADD(year, 1, SYSUTCDATETIME()), NULL
FROM dbo.Tenant t
WHERE t.Deleted = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.Subscription s WHERE s.TenantId = t.Id);

-- 3. A TenantModule row for every module in the tenant's template default list.
WITH keys AS (
    SELECT t.Id AS TenantId, LTRIM(RTRIM(value)) AS ModuleKey
    FROM dbo.Tenant t
    JOIN dbo.BusinessTemplate bt ON bt.[Key] = t.BusinessTemplateKey
    CROSS APPLY STRING_SPLIT(bt.DefaultModuleKeys, ',')
    WHERE t.Deleted = 0
)
INSERT INTO dbo.TenantModule (Id, TenantId, ModuleKey, Status, ActivatedOn, ExpiresOn)
SELECT NEWID(), k.TenantId, k.ModuleKey, 'Active', SYSUTCDATETIME(), NULL
FROM keys k
JOIN dbo.Module m ON m.[Key] = k.ModuleKey
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.TenantModule tm
    WHERE tm.TenantId = k.TenantId AND tm.ModuleKey = k.ModuleKey);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM dbo.TenantModule;
DELETE FROM dbo.Subscription;
UPDATE dbo.Tenant SET BusinessTemplateKey = NULL, Status = NULL, Currency = NULL;
");
        }
    }
}
