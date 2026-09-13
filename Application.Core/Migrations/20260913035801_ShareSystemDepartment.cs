using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <summary>
    /// Same idea as <see cref="ShareSystemLookups"/>: the Angular client hard-codes
    /// one Department id (Department_Id_SALES_AND_MARKETING, in ~20 files) assuming
    /// every tenant has a "Sales &amp; Marketing" department under that fixed id — true
    /// only for the single pre-multitenancy database. Department is not
    /// ITenantSharable, so for every other tenant the id resolved to nothing and
    /// every marketing-officer picker/filter silently came back empty.
    /// Moves that one existing row to <c>TenancyConstants.SystemTenantId</c> so every
    /// tenant resolves it; all other tenant-created departments stay tenant-scoped.
    /// </summary>
    /// <inheritdoc />
    public partial class ShareSystemDepartment : Migration
    {
        private const string Sys = "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF";
        private const string SalesAndMarketingDepartmentId = "5997829B-5D80-489D-8024-74B371B08329";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE dbo.Department SET TenantId = '{Sys}' WHERE Id = '{SalesAndMarketingDepartmentId}';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DECLARE @t uniqueidentifier = (SELECT TOP 1 Id FROM dbo.Tenant WHERE Deleted = 0 ORDER BY CreatedOn);
IF @t IS NOT NULL
BEGIN
    UPDATE dbo.Department SET TenantId = @t WHERE Id = '{SalesAndMarketingDepartmentId}' AND TenantId = '{Sys}';
END
");
        }
    }
}
