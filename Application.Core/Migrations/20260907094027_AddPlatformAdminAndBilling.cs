using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformAdminAndBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plan",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModuleKeys = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Quotas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plan", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "PlatformAdmin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAdmin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformAuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformAdminEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceBook",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PublishedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBook", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceBookEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceBookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceBookEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceBookEntry_PriceBook_PriceBookId",
                        column: x => x.PriceBookId,
                        principalTable: "PriceBook",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAdmin_Email",
                table: "PlatformAdmin",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAuditLog_CreatedOn",
                table: "PlatformAuditLog",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAuditLog_TenantId",
                table: "PlatformAuditLog",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceBook_Version",
                table: "PriceBook",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceBookEntry_PriceBookId_ItemType_ItemKey",
                table: "PriceBookEntry",
                columns: new[] { "PriceBookId", "ItemType", "ItemKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plan");

            migrationBuilder.DropTable(
                name: "PlatformAdmin");

            migrationBuilder.DropTable(
                name: "PlatformAuditLog");

            migrationBuilder.DropTable(
                name: "PriceBookEntry");

            migrationBuilder.DropTable(
                name: "PriceBook");
        }
    }
}
