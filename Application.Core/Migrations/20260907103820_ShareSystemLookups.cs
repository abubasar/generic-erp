using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <summary>
    /// Same idea as <see cref="ShareSystemAccountHeads"/> for the small system
    /// lookup tables the code hard-codes GUIDs for: AccountType (13),
    /// InventoryType (3), PaymentMode (3), and the single system ProductType.
    /// Their constant rows move to <c>TenancyConstants.SystemTenantId</c> so every
    /// tenant resolves them; tenant-created product types stay tenant-scoped.
    /// </summary>
    public partial class ShareSystemLookups : Migration
    {
        private const string Sys = "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
UPDATE dbo.AccountType SET TenantId = '{Sys}' WHERE Id IN (
 '24EE8424-A461-4462-9ECB-05F69E0306DF','02BE9020-7914-4D4F-9EC0-12A4CD73AC5B','C1152B40-58EE-40DE-AF43-1914901BF20C',
 '0371C7F5-51FF-4697-8602-400222F8C635','412AD3AB-D15C-4435-9090-19F4EEDE47A0','68AB9D6D-31DB-44C5-AAE9-2C98386F7C15',
 '05C03FC9-245C-4533-85A5-6D02ED008A20','71B71758-E93A-46AA-9CCD-4A7539214BBE','DEB76499-33B4-4923-81D9-810FA51F5F82',
 '3F7EAB88-1A1E-4B7D-8F5D-FB460413ECDD','F6D28306-C786-4B59-934F-47F37F152227','9E33FA6C-1B13-4A56-A886-A6EC498913B5',
 'BFC86B94-DCA0-4E81-B8EB-35843DBEB1B4');

UPDATE dbo.InventoryType SET TenantId = '{Sys}' WHERE Id IN (
 'C170BE30-B221-4014-BA31-0938FC107345','949C1208-DAF9-4F0C-9D7A-ABB94062569C','F843B6E5-9BE6-412A-B91E-51DDBAC6663D');

UPDATE dbo.PaymentMode SET TenantId = '{Sys}' WHERE Id IN (
 '0A90F956-1E5D-429F-B6D3-02AF4593E1A0','FC2AC288-A91F-49DB-A232-053B37D4F7DE','BECCF825-0B3F-4005-86E8-571D366EA8C4');

UPDATE dbo.ProductType SET TenantId = '{Sys}' WHERE Id = '53901015-FFC9-4C3D-BF7A-E3398AF1D0E9';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
DECLARE @t uniqueidentifier = (SELECT TOP 1 Id FROM dbo.Tenant WHERE Deleted = 0 ORDER BY CreatedOn);
IF @t IS NOT NULL
BEGIN
    UPDATE dbo.AccountType   SET TenantId = @t WHERE TenantId = '{Sys}';
    UPDATE dbo.InventoryType  SET TenantId = @t WHERE TenantId = '{Sys}';
    UPDATE dbo.PaymentMode    SET TenantId = @t WHERE TenantId = '{Sys}';
    UPDATE dbo.ProductType    SET TenantId = @t WHERE TenantId = '{Sys}';
END
");
        }
    }
}
