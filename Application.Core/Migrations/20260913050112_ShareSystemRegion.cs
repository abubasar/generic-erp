using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <summary>
    /// Follow-up to <see cref="ShareSystemZoneAndFundTransferTransactionType"/>: the
    /// three shared BD-North/East/South zones all point at one Region ("BANGLADESH"),
    /// which wasn't ITenantSharable. Because Zone is read via
    /// <c>.Include(x => x.Region)</c>, and EF Core applies the Region's own tenant
    /// query filter as an inner join for the include, a shared Zone whose Region
    /// belongs to a different tenant is silently dropped from the result set entirely
    /// (not just given a null Region) for every tenant but the Region's owner —
    /// verified directly against SQL (INNER JOIN drops the 3 rows; LEFT JOIN keeps
    /// them with a null Region). Sharing the full referenced parent chain, not just
    /// the leaf entity, matches how the Account CoA skeleton was shared (the parent
    /// account groups moved to SystemTenantId together with the leaf heads).
    /// </summary>
    /// <inheritdoc />
    public partial class ShareSystemRegion : Migration
    {
        private const string Sys = "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF";
        private const string BangladeshRegionId = "96D7BF63-6E6C-43BB-9531-63D342D06B9F";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE dbo.Region SET TenantId = '{Sys}' WHERE Id = '{BangladeshRegionId}';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DECLARE @t uniqueidentifier = (SELECT TOP 1 Id FROM dbo.Tenant WHERE Deleted = 0 ORDER BY CreatedOn);
IF @t IS NOT NULL
BEGIN
    UPDATE dbo.Region SET TenantId = @t WHERE Id = '{BangladeshRegionId}' AND TenantId = '{Sys}';
END
");
        }
    }
}
