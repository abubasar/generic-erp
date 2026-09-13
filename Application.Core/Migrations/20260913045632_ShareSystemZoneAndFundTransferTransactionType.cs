using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <summary>
    /// Same idea as <see cref="ShareSystemDepartment"/>: the Angular client hard-codes
    /// three Zone ids (BdSouthId/BdEastId/BdNorthId, used in fund-transfer-form and
    /// the analytics dashboard) and one FundTransferTransactionType id (ReverseId),
    /// assuming every tenant has these exact rows. Neither entity was ITenantSharable,
    /// so for every tenant but the original one these ids resolved to nothing.
    /// Moves the existing rows to <c>TenancyConstants.SystemTenantId</c> so every
    /// tenant resolves them; tenant-created zones/transaction types stay tenant-scoped.
    /// </summary>
    /// <inheritdoc />
    public partial class ShareSystemZoneAndFundTransferTransactionType : Migration
    {
        private const string Sys = "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE dbo.Zone SET TenantId = '{Sys}' WHERE Id IN (
 'EF34B779-0B6F-4F10-ADA2-D8FCA4487DC6','7412D791-E88E-4F43-84E3-AA19BF3C82AC','951932AA-ECF3-4B36-9CDC-5DD52058FA2F');

UPDATE dbo.FundTransferTransactionType SET TenantId = '{Sys}' WHERE Id = 'F540D434-D21F-468A-8C99-B3A8F077444B';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DECLARE @t uniqueidentifier = (SELECT TOP 1 Id FROM dbo.Tenant WHERE Deleted = 0 ORDER BY CreatedOn);
IF @t IS NOT NULL
BEGIN
    UPDATE dbo.Zone SET TenantId = @t WHERE TenantId = '{Sys}' AND Id IN (
     'EF34B779-0B6F-4F10-ADA2-D8FCA4487DC6','7412D791-E88E-4F43-84E3-AA19BF3C82AC','951932AA-ECF3-4B36-9CDC-5DD52058FA2F');
    UPDATE dbo.FundTransferTransactionType SET TenantId = @t WHERE TenantId = '{Sys}' AND Id = 'F540D434-D21F-468A-8C99-B3A8F077444B';
END
");
        }
    }
}
