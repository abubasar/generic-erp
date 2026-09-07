using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <summary>
    /// Multi-tenant account heads. The accounting engine keys off ~46 hard-coded
    /// account-head GUIDs (<c>AccountHeadConstants</c>) plus the intermediate group
    /// accounts between them and the roots. Those rows are moved to the shared
    /// sentinel tenant (<c>TenancyConstants.SystemTenantId</c>); the EF query filter
    /// for <c>Account</c> (an <c>ITenantSharable</c>) matches "my tenant OR shared",
    /// so every tenant — existing and future — resolves the same head GUIDs while
    /// its own customer / supplier / expense accounts stay tenant-scoped.
    /// <c>UQ_Account_Code</c> becomes unique per tenant.
    /// </summary>
    public partial class ShareSystemAccountHeads : Migration
    {
        // AccountHeadConstants (46) + the 10 intermediate group accounts on the
        // path from a constant head up to its root. Snapshot — do not regenerate.
        private const string SharedHeadIds = @"
'02D3C859-9FF9-4393-A351-C61A8D5C8334','0B114C3A-F1C7-49AE-A54E-0029FA7E1268','0D876F1C-4EDE-4FF6-9E37-52FAA4560F9F',
'13A4746B-FE51-49D8-B87B-6012A4078E50','16EBC06A-696C-40AE-A0F3-62B56047CAD4','1918A910-4C03-455F-A078-31AA44DEDD3A',
'22688043-45EE-4115-9637-19F4A32F5A98','2A0E9B94-F14D-47D3-BF2F-DD63FCDDA3B4','2B7BAF14-19DC-4811-B030-5A66803B32E5',
'2DF7205B-C6DA-4FD1-B6EA-BD06D392AD8D','38379B9F-8DEB-4D87-A0F7-CBBCA6C2AFFF','3A71AD51-684D-4485-AB73-95EB126DF96E',
'3B878403-AEBD-4E7C-8BF3-7C3B8B6AC04A','4025017E-2282-4FCA-AE39-0AE329A752DC','40972A38-8B0A-4C8B-A548-3D28E839483F',
'410FC7E7-A6E4-4FF0-B607-ADD87AAABA04','4143CF72-F126-4662-B852-8EAECF19673C','4AA4D940-115F-4E7E-A878-B818BC746DFC',
'4CDC56EE-DAF3-49DB-8BAD-766F5CD285E4','576D9241-135C-4387-80B6-40935ADC9C64','57953A76-6F03-4F95-AF8A-B8FCD81A2029',
'5EA472D9-EAFE-4918-83BF-933CBB1AA22E','64285947-97B2-4384-87D5-7DF72002E6A8','662A6C6F-09D2-43DA-A696-369B8C756393',
'6BC1A6E0-F6C9-4555-8819-943B7A9B5B24','7E912FE0-3C3E-4CC1-A21B-8123F34934FD','91CC1EE1-25FD-441E-90FB-464472C11498',
'9803751B-D516-4DA9-B360-978EC98846BB','9D42889F-A3CA-4445-87D8-DBC5811C44CF','B645E0ED-B562-4397-9C95-AB5D8E45FDB2',
'B88340C0-67A0-4B42-9E40-CD101E843F4C','B93DEFD2-C16F-41CB-BF13-E835612F5DE1','BD4CB478-5699-4DFC-B19C-D2F9DC14D959',
'BD5596F6-AC93-4F7C-A909-E8F6717572FF','C2969634-CC6F-4FAC-8F3F-8D8F4CD06137','C3E8E158-1AA7-4160-9358-007A80D2CE6F',
'C3EEB823-A66F-4C64-9CA0-6F9EA087E5B6','C7B4103B-8703-43D7-A235-000B4F07B6C9','CA112047-F529-4B78-9C73-F0AD0482A4F7',
'CE5F1F14-925C-4D0D-91A3-D7C5AAA11994','CFFC6233-78D5-4CB5-8DFF-00370E8177EE','D603492E-6B71-407C-AB49-9B0ACDC099B8',
'D690995B-747A-45C6-91F6-19F5B2C8A4B0','DD07995E-3F49-4326-BCF4-5357BF0355DA','E3749624-1410-4D35-8B67-801BDF35D825',
'E8B00859-29C9-4B9F-AF2D-B58617020CF7',
'100B270F-7279-4DCE-801D-28B354D60DB8','4D052490-5F17-4286-BFD2-1A3521718A37','8750D395-E6EF-4A64-8884-6C02DD7C6C3E',
'A0189E41-5B59-4DCA-A598-EE4C2BF3FE0D','AB099C1E-C5AF-4AE2-8751-8FB33641199C','D0051C6C-A73A-46C2-8475-3836D0E7A513',
'E2508B2B-ED67-4ADC-952E-1D1EA827DDF8','E4B22118-DB0B-4F91-9369-10825A2E2EF6','EE15C905-0D78-4670-BAC1-172F6F956B56',
'F73E58EB-EBF2-4EDB-A987-ECAD98233973'";

        private const string SystemTenantId = "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Move the shared CoA skeleton to the sentinel tenant BEFORE the new
            //    per-tenant unique index (so codes never collide during the switch).
            migrationBuilder.Sql($@"
UPDATE dbo.Account SET TenantId = '{SystemTenantId}'
WHERE Id IN ({SharedHeadIds});");

            // UQ_Account_Code was created DB-first as a UNIQUE KEY *constraint*, not a
            // plain index (EF's model believed it an index — DropIndex would fail).
            migrationBuilder.Sql(@"
ALTER TABLE dbo.Account DROP CONSTRAINT UQ_Account_Code;
CREATE UNIQUE INDEX UQ_Account_Code ON dbo.Account ([TenantId], [Code]);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP INDEX UQ_Account_Code ON dbo.Account;
ALTER TABLE dbo.Account ADD CONSTRAINT UQ_Account_Code UNIQUE ([Code]);");

            // Best effort: hand the skeleton back to the oldest live tenant. Only
            // sound while a single tenant exists (as it did when this ran).
            migrationBuilder.Sql($@"
DECLARE @t uniqueidentifier = (SELECT TOP 1 Id FROM dbo.Tenant WHERE Deleted = 0 ORDER BY CreatedOn);
IF @t IS NOT NULL
    UPDATE dbo.Account SET TenantId = @t WHERE TenantId = '{SystemTenantId}';");
        }
    }
}
