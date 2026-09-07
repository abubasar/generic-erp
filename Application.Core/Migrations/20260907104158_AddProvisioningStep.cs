using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddProvisioningStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProvisioningStep",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvisioningStep", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProvisioningStep_TenantId_StepKey",
                table: "ProvisioningStep",
                columns: new[] { "TenantId", "StepKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvisioningStep");
        }
    }
}
