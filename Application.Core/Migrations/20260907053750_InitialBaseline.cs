using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartingNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Company",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "code"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CostCenter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Country",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Country", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currency",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryPlace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPlace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscountProductWise",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountProductWise", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true, collation: "Latin1_General_CI_AS"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, collation: "Latin1_General_CI_AS"),
                    Host = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, collation: "Latin1_General_CI_AS"),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, collation: "Latin1_General_CI_AS"),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, collation: "Latin1_General_CI_AS"),
                    Port = table.Column<int>(type: "int", nullable: false),
                    EnableSsl = table.Column<bool>(type: "bit", nullable: false),
                    IsDefaultEmailAccount = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialYear",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialYear", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FundTransferTransactionType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTransferTransactionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobLocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobLocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TimeStamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "xml", nullable: true),
                    LogEvent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IP = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Machine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machine", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementUnit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementUnit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Route = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Not", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackSize",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackSize", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Picture",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Picture", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceSupplierPaymentMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvanceSupplierPaymentTransactionRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceivePaymentAgainstSaleSaleInvoiceMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivePaymentAgainstSaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousAvailableReceivable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivePaymentAgainstSaleSaleInvoiceMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceivePaymentPictureMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivePaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivePaymentPictureMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Region",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shift",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FromTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ToTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shift", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierProductMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierProductMapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BINNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    BusinessType = table.Column<int>(type: "int", nullable: false),
                    Logo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountTransactionType = table.Column<int>(type: "int", nullable: false),
                    TransactionQty = table.Column<int>(type: "int", nullable: false),
                    TransactionQtyValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentModeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContraAccountIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContraAccountNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsOpeningBalance = table.Column<bool>(type: "bit", nullable: false),
                    IsPreviousYearClosingBalance = table.Column<bool>(type: "bit", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleInvoiceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntry_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Designation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Designation_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductType_InventoryType",
                        column: x => x.InventoryTypeId,
                        principalTable: "InventoryType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Store",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "code"),
                    InventoryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepoChargePerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Store", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Store_InventoryType",
                        column: x => x.InventoryTypeId,
                        principalTable: "InventoryType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Zone",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zone_Region",
                        column: x => x.RegionId,
                        principalTable: "Region",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Group = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Role",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EmployeeIdNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    BloodGroup = table.Column<int>(type: "int", nullable: false),
                    MaritalStatus = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employee_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employee_Designation",
                        column: x => x.DesignationId,
                        principalTable: "Designation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employee_JobLocation",
                        column: x => x.JobLocationId,
                        principalTable: "JobLocation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Generic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Generic_ProductType",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequisition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequisitionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    PaymentTermInDays = table.Column<int>(type: "int", nullable: false),
                    Transport = table.Column<int>(type: "int", nullable: false),
                    PaymentMode = table.Column<int>(type: "int", nullable: false),
                    TermAndCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImportPurchaseIncoTerm = table.Column<int>(type: "int", nullable: false),
                    ImportPurchasePaymentTerm = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequisitionStatus = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequisition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequisition_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequisition_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAdjustmentQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAdjustmentValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustment_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockTransfer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TruckNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransfer_Destination",
                        column: x => x.DestinationId,
                        principalTable: "Store",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockTransfer_Source",
                        column: x => x.SourceId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Area",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Area", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Area_Zone",
                        column: x => x.ZoneId,
                        principalTable: "Zone",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsControlAccount = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    OwnersName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TradeLicense = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactPersonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPersonContactNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPersonEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPersonDesignation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerRegionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerTerritoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerMarketingOfficerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerCreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerAgreement = table.Column<bool>(type: "bit", nullable: false),
                    CustomerCreditDays = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, defaultValueSql: "((0))"),
                    CustomerTargetQuantity = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, defaultValueSql: "((0))"),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForAppUser = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountChild_AccountParent",
                        column: x => x.ParentId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Account_AccountType",
                        column: x => x.AccountTypeId,
                        principalTable: "AccountType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MarketingOfficer_Employee",
                        column: x => x.CustomerMarketingOfficerId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(1024)", maxLength: 1024, nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(1024)", maxLength: 1024, nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Employee",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Role",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InventoryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenericId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Composition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackSizeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsPurchaseProduct = table.Column<bool>(type: "bit", nullable: false),
                    IsSaleProduct = table.Column<bool>(type: "bit", nullable: false),
                    MeasurementUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MRP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AlertQuantity = table.Column<int>(type: "int", nullable: false),
                    LastPurchaseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BagWeight = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    VatPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Category",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_Country",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_Generic",
                        column: x => x.GenericId,
                        principalTable: "Generic",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_InventoryType",
                        column: x => x.InventoryTypeId,
                        principalTable: "InventoryType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_Manufacturer",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_MeasurementUnit",
                        column: x => x.MeasurementUnitId,
                        principalTable: "MeasurementUnit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_PackSize",
                        column: x => x.PackSizeId,
                        principalTable: "PackSize",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Product_ProductType",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductAudit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InventoryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenericId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Composition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PackSizeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsPurchaseProduct = table.Column<bool>(type: "bit", nullable: false),
                    IsSaleProduct = table.Column<bool>(type: "bit", nullable: false),
                    MeasurementUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MRP = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AlertQuantity = table.Column<int>(type: "int", nullable: false),
                    LastPurchaseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BagWeight = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    VatPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeletedHistory = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ProductA__3214EC07F0C19D8C", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductAudit_Category",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAudit_Country",
                        column: x => x.CountryId,
                        principalTable: "Country",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAudit_Generic",
                        column: x => x.GenericId,
                        principalTable: "Generic",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAudit_InventoryType",
                        column: x => x.InventoryTypeId,
                        principalTable: "InventoryType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAudit_Manufacturer",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAudit_MeasurementUnit",
                        column: x => x.MeasurementUnitId,
                        principalTable: "MeasurementUnit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAudit_PackSize",
                        column: x => x.PackSizeId,
                        principalTable: "PackSize",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductAudit_ProductType",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Territory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Territory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Territory_Area",
                        column: x => x.AreaId,
                        principalTable: "Area",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BankAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RoutingNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccount_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerWiseProductDiscount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicableDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerWiseProductDiscount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerWiseProductDiscount_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryNoteNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SaleOrderNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerMarketingOfficerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Transport = table.Column<int>(type: "int", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LimitAvailed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TruckNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DriverContactNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MoneyReceiptNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OfferDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportationCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepoCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryNote_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryNote_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FundTransfer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FundTransferNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FundTransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FundTransferTransactionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferFromAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferToAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Charges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FundTransfer_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundTransfer_FundTransferTransactionType",
                        column: x => x.FundTransferTransactionTypeId,
                        principalTable: "FundTransferTransactionType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundTransfer_TransferFromAccount",
                        column: x => x.TransferFromAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FundTransfer_TransferToAccount",
                        column: x => x.TransferToAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiveNote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GRNNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GRNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallanNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChallanDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TruckNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DriverContactNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Transport = table.Column<int>(type: "int", nullable: true),
                    PaymentTermInDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportationCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdditionalLandedCost = table.Column<decimal>(type: "decimal(22,12)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalGrnAdjustmentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseOrderTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsImportPurchase = table.Column<bool>(type: "bit", nullable: false),
                    ProformaInvoiceNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LcNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImportPurchaseIncoTerm = table.Column<int>(type: "int", nullable: false),
                    ImportPurchasePaymentTerm = table.Column<int>(type: "int", nullable: false),
                    PortOfLoading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortOfDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiveNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiveNote_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoodsReceiveNote_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JournalEntryDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsOpeningBalance = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntryDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntryDetail_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_JournalEntryDetail_JournalEntry",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntry",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentVoucher",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FundTransferTransactionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentModeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CashBankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentVoucher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentVoucher_CashBankAccount",
                        column: x => x.CashBankAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentVoucher_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentVoucher_FundTransferTransactionType",
                        column: x => x.FundTransferTransactionTypeId,
                        principalTable: "FundTransferTransactionType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentVoucher_PaymentMode",
                        column: x => x.PaymentModeId,
                        principalTable: "PaymentMode",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PoPriceAdjustmentAfterGrn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GRNNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoPriceAdjustmentAfterGrn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoPriceAdjustmentAfterGrn_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PoPriceAdjustmentAfterGrn_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseInvoiceNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GRNNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PONumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierInvoiceNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentTermInDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    TransportationCost = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    TotalVat = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    SupplierPaymentCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdvancePaymentAmount = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    TotalGrnAdjustmentAmount = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    PurchaseOrderTotal = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    NetPayable = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    IsImportPurchase = table.Column<bool>(type: "bit", nullable: false),
                    ProformaInvoiceNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LcNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(22,2)", nullable: false),
                    ImportPurchaseIncoTerm = table.Column<int>(type: "int", nullable: false),
                    ImportPurchasePaymentTerm = table.Column<int>(type: "int", nullable: false),
                    BillOfEntryNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BillOfEntryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PortOfLoading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortOfDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalLandedCost = table.Column<decimal>(type: "decimal(22,12)", nullable: false),
                    AdjustmentValue = table.Column<decimal>(type: "decimal(22,12)", nullable: false),
                    IsLcAdjusted = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoice_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseInvoice_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QuotationNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PONumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PODate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryPlaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductOrigin = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PackagingType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiryTime = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Transport = table.Column<int>(type: "int", nullable: false),
                    PaymentTermInDays = table.Column<int>(type: "int", nullable: false),
                    DeliveryTermInDays = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPartialDelivery = table.Column<bool>(type: "bit", nullable: false),
                    PaymentMode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WeightVariance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TermAndCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsImportPurchase = table.Column<bool>(type: "bit", nullable: false),
                    ProformaInvoiceNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LcNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImportPurchaseIncoTerm = table.Column<int>(type: "int", nullable: false),
                    ImportPurchasePaymentTerm = table.Column<int>(type: "int", nullable: false),
                    PortOfLoading = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortOfDestination = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalLandedCost = table.Column<decimal>(type: "decimal(22,12)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrder_DeliveryPlace",
                        column: x => x.DeliveryPlaceId,
                        principalTable: "DeliveryPlace",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrder_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrder_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseReturnNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PurchaseReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GRNNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturn_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseReturn_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReceivePayment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerMarketingOfficerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FundTransferTransactionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentModeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FeedSalesPurpose = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditRecoveryPurpose = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivePayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivePayment_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceivePayment_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceivePayment_FundTransferTransactionType",
                        column: x => x.FundTransferTransactionTypeId,
                        principalTable: "FundTransferTransactionType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceivePayment_PaymentMode",
                        column: x => x.PaymentModeId,
                        principalTable: "PaymentMode",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReceivePaymentAgainstSale",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerMarketingOfficerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivePaymentAgainstSale", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivePaymentAgainstSale_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceivePaymentAgainstSale_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceivePaymentAgainstSale_ToAccount",
                        column: x => x.ToAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReceiveVoucher",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FundTransferTransactionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentModeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CashBankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiveVoucher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiveVoucher_CashBankAccount",
                        column: x => x.CashBankAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiveVoucher_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiveVoucher_FundTransferTransactionType",
                        column: x => x.FundTransferTransactionTypeId,
                        principalTable: "FundTransferTransactionType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiveVoucher_PaymentMode",
                        column: x => x.PaymentModeId,
                        principalTable: "PaymentMode",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RfqSent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfqSent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RfqSent_PurchaseRequisition",
                        column: x => x.RequisitionId,
                        principalTable: "PurchaseRequisition",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RfqSent_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleQuotation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QuotationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TermAndCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsMailSent = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleQuotation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleQuotation_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierPayment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SupplierPaymentType = table.Column<int>(type: "int", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FundTransferTransactionTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentModeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedAmountInPurchaseInvoice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPayment_1", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPayment1_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierPayment_FundTransferTransactionType",
                        column: x => x.FundTransferTransactionTypeId,
                        principalTable: "FundTransferTransactionType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierPayment_PaymentMode",
                        column: x => x.PaymentModeId,
                        principalTable: "PaymentMode",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierPayment_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VendorQuotation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuotationNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequisitionNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Transport = table.Column<int>(type: "int", nullable: false),
                    PaymentMode = table.Column<int>(type: "int", nullable: false),
                    PaymentTermInDays = table.Column<int>(type: "int", nullable: false),
                    DeliveryTermInDays = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TermAndCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImportPurchaseIncoTerm = table.Column<int>(type: "int", nullable: false),
                    ImportPurchasePaymentTerm = table.Column<int>(type: "int", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorQuotation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorQuotation_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoucherEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoucherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentModeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoucherType = table.Column<int>(type: "int", nullable: false),
                    CashBankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherEntry_CashBankAccount",
                        column: x => x.CashBankAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoucherEntry_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoucherEntry_PaymentMode",
                        column: x => x.PaymentModeId,
                        principalTable: "PaymentMode",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BillOfMaterial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BomNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CopiedFromBomNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FinishedProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormulationNo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DosageQuantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillOfMaterial_FinishedProduct",
                        column: x => x.FinishedProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiscountProductWiseDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscountProductWiseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscountAmountPerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountProductWiseDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountProductWiseDetail_DiscountProductWise",
                        column: x => x.DiscountProductWiseId,
                        principalTable: "DiscountProductWise",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiscountProductWiseDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufacturingOrderNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BomNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormulationNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FinishedProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionQuantity = table.Column<int>(type: "int", nullable: false),
                    RawMaterialStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalRMUsed = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    RmCost = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    StandardDirectExpenseAmount = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    StandardFactoryOverheadAmount = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingOrder_FinishedProduct",
                        column: x => x.FinishedProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ManufacturingOrder_RawMaterialStore",
                        column: x => x.RawMaterialStoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductCostSetup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DirectExpense = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FactoryOverhead = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCostSetup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCostSetup_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Production",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ManufacturingOrderNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BomNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreakTime = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MachineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraDamageQuantity = table.Column<int>(type: "int", nullable: false),
                    DustLooseInQuantity = table.Column<int>(type: "int", nullable: false),
                    DustLooseOutQuantity = table.Column<int>(type: "int", nullable: false),
                    FGStoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalRMUsed = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    RmCost = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ProductionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormulationNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FinishedProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionQuantity = table.Column<int>(type: "int", nullable: false),
                    ActualProductionQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAdjustmentQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAdjustmentCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Production", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Production_FinishedGoodsStore",
                        column: x => x.FGStoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Production_FinishedProduct",
                        column: x => x.FinishedProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Production_Machine",
                        column: x => x.MachineId,
                        principalTable: "Machine",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Production_Shift",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequisitionDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseRequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LastPurchaseRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OrderedQuantity = table.Column<int>(type: "int", nullable: false),
                    AlertQuantity = table.Column<int>(type: "int", nullable: false),
                    CurrentStockQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequisitionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequisitionDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequisitionDetail_PurchaseRequisition",
                        column: x => x.PurchaseRequisitionId,
                        principalTable: "PurchaseRequisition",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactonType = table.Column<int>(type: "int", nullable: false),
                    InventoryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AvailableQty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    InQty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    OutQty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    InRate = table.Column<decimal>(type: "decimal(18,12)", nullable: false),
                    InTransportCost = table.Column<decimal>(type: "decimal(18,12)", nullable: false),
                    OutRate = table.Column<decimal>(type: "decimal(18,12)", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsConsumedInProduction = table.Column<bool>(type: "bit", nullable: false),
                    IsUnpostedEntry = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsStockIssuedForCustomer = table.Column<bool>(type: "bit", nullable: false),
                    SaleInvoiceType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stock_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Stock_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockAdjustmentDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockAdjustmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdjustmentQty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AdjustmentValue = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockAdjustmentDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockAdjustmentDetail_StockAdjustment",
                        column: x => x.StockAdjustmentId,
                        principalTable: "StockAdjustment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StockTransferDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockTransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferBagQuantity = table.Column<int>(type: "int", nullable: false),
                    TransferQuantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockTransferDetail_StockTransfer",
                        column: x => x.StockTransferId,
                        principalTable: "StockTransfer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleInvoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryNoteNo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SaleOrderNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerMarketingOfficerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Transport = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPercentageDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OfferDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalVat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableReceivable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportationCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepoCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TermAndCondition = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MoneyReceiptNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Paid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTerm = table.Column<int>(type: "int", nullable: false),
                    CustomerTerritoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StockIssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StockIssuedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdjustmentOtherDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAdjustmentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAdjustmentPercentageDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleInvoice_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleInvoice_CustomerTerritory",
                        column: x => x.CustomerTerritoryId,
                        principalTable: "Territory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleInvoice_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleOrder",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleOrderNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuotationNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Transport = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerMarketingOfficerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LimitAvailed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPercentageDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OfferDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransportationCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepoCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsMailSent = table.Column<bool>(type: "bit", nullable: false),
                    MoneyReceiptNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentTerm = table.Column<int>(type: "int", nullable: false),
                    CustomerTerritoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleOrder_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleOrder_CustomerTerritory",
                        column: x => x.CustomerTerritoryId,
                        principalTable: "Territory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleOrder_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleReturn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryNoteNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SaleReturnNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SaleReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerMarketingOfficerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerTerritoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalVat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPercentageDiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OtherDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleReturn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleReturn_Customer",
                        column: x => x.CustomerId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleReturn_CustomerTerritory",
                        column: x => x.CustomerTerritoryId,
                        principalTable: "Territory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleReturn_Store",
                        column: x => x.StoreId,
                        principalTable: "Store",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerWiseProductDiscountDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerWiseProductDiscountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpecialDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YearlyDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TargetDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerWiseProductDiscountDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerWiseProductDiscountDetail_CustomerWiseProductDiscount",
                        column: x => x.CustomerWiseProductDiscountId,
                        principalTable: "CustomerWiseProductDiscount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerWiseProductDiscountDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryNoteDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleOrderDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BagWeight = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NetRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OfferDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InvoiceDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CashDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SpecialDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MonthlyDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    YearlyDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TargetDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OrderedPrimaryQuantity = table.Column<int>(type: "int", nullable: false),
                    OrderedQuantity = table.Column<int>(type: "int", nullable: false),
                    OrderedPrimaryBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    OrderedBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    DeliveryPrimaryQuantity = table.Column<int>(type: "int", nullable: false),
                    DeliveryQuantity = table.Column<int>(type: "int", nullable: false),
                    DeliveryPrimaryBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    OtherDiscountPerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    TransportationCostPerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    DepoChargePerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockIssueCogs = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryNoteDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryNoteDetail_DeliveryNote",
                        column: x => x.DeliveryNoteId,
                        principalTable: "DeliveryNote",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryNoteDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiveNoteDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiveNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    POQuantity = table.Column<int>(type: "int", nullable: false),
                    GRNQuantity = table.Column<int>(type: "int", nullable: false),
                    BagWeightDeductionQuantity = table.Column<int>(type: "int", nullable: false),
                    NumberOfBagQuantity = table.Column<int>(type: "int", nullable: false),
                    NetQuantity = table.Column<int>(type: "int", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedQuantity = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrencyRate = table.Column<decimal>(type: "decimal(25,18)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RateAfterBagWeightDeduction = table.Column<decimal>(type: "decimal(25,18)", nullable: false),
                    CurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsReceiveNoteDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiveNoteDetail_GoodsReceiveNote",
                        column: x => x.GoodsReceiveNoteId,
                        principalTable: "GoodsReceiveNote",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GoodsReceiveNoteDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentVoucherDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentVoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentVoucherDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentVoucherDetail_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentVoucherDetail_PaymentVoucher",
                        column: x => x.PaymentVoucherId,
                        principalTable: "PaymentVoucher",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PoPriceAdjustmentAfterGrnDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PoPriceAdjustmentAfterGrnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrnQuantity = table.Column<int>(type: "int", nullable: false),
                    GrnRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AdjustmentQuantity = table.Column<int>(type: "int", nullable: false),
                    AdjustmentRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoPriceAdjustmentAfterGrnDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoPriceAdjustmentAfterGrnDetail_PoPriceAdjustmentAfterGrn",
                        column: x => x.PoPriceAdjustmentAfterGrnId,
                        principalTable: "PoPriceAdjustmentAfterGrn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PoPriceAdjustmentAfterGrnDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LcAdjustment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PurchaseInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseInvoiceNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvoiceTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LcMarginTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LCAdjustment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LcAdjustment_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LcAdjustment_PurchaseInvoice",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GRNQuantity = table.Column<int>(type: "int", nullable: false),
                    BagWeightDeductionQuantity = table.Column<int>(type: "int", nullable: false),
                    NumberOfBagQuantity = table.Column<int>(type: "int", nullable: false),
                    NetQuantity = table.Column<int>(type: "int", nullable: false),
                    CurrencyRate = table.Column<decimal>(type: "decimal(25,18)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RateAfterBagWeightDeduction = table.Column<decimal>(type: "decimal(25,18)", nullable: false),
                    CurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    GRNNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GRNDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VatPercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceDetail_PurchaseInvoice",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierPaymentAgainstPurchase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnpostedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPayment_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierPayment_FromAccount",
                        column: x => x.FromAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierPayment_PurchaseInvoice",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LCCostEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LcNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInactive = table.Column<bool>(type: "bit", nullable: false),
                    InactivatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LCCostEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LCCostEntry_CostCenter",
                        column: x => x.CostCenterId,
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LCCostEntry_PurchaseOrder",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    CurrencyRate = table.Column<decimal>(type: "decimal(25,18)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CurrencyAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderDetail_PurchaseOrder",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseRequisitionPurchaseOrderMapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseRequisitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderQuantity = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    RequisitionQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseRequisitionPurchaseOrderMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseRequisitionPurchaseOrderMapping_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseRequisitionPurchaseOrderMapping_PurchaseOrder",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierTransactionAgainstPo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SupplierInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentTermInDays = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierTransactionType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    FinancialYearId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierTransactionAgainstPo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierTransactionAgainstPo_PurchaseOrder",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrder",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierTransactionAgainstPo_Supplier",
                        column: x => x.SupplierId,
                        principalTable: "Account",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseReturnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GRNQuantity = table.Column<int>(type: "int", nullable: false),
                    GrnBagWeightDeductionQty = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BagWeightDeductionQty = table.Column<int>(type: "int", nullable: false),
                    NumberOfBagQuantity = table.Column<int>(type: "int", nullable: false),
                    NetQuantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetail_PurchaseReturn",
                        column: x => x.PurchaseReturnId,
                        principalTable: "PurchaseReturn",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReceivePaymentDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceivePaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivePaymentDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivePaymentDetail_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceivePaymentDetail_ReceivePayment",
                        column: x => x.ReceivePaymentId,
                        principalTable: "ReceivePayment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReceiveVoucherDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiveVoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiveVoucherDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiveVoucherDetail_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiveVoucherDetail_ReceiveVoucher",
                        column: x => x.ReceiveVoucherId,
                        principalTable: "ReceiveVoucher",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleQuotationDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleQuotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BagWeight = table.Column<int>(type: "int", nullable: false),
                    PrimaryQuantity = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleQuotationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleQuotationDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleQuotationDetail_SaleQuotation",
                        column: x => x.SaleQuotationId,
                        principalTable: "SaleQuotation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupplierPaymentDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseInvoiceNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPaymentDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPaymentDetail_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierPaymentDetail_SupplierPayment",
                        column: x => x.SupplierPaymentId,
                        principalTable: "SupplierPayment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VendorQuotationDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VendorQuotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorQuotaionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorQuotaionDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VendorQuotaionDetail_VendorQuotation",
                        column: x => x.VendorQuotationId,
                        principalTable: "VendorQuotation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VoucherEntryDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherEntryDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherEntryDetail_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VoucherEntryDetail_VoucherEntry",
                        column: x => x.VoucherEntryId,
                        principalTable: "VoucherEntry",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BillOfMaterialDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillOfMaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfMaterialDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillOfMaterialDetail_BillOfMaterial",
                        column: x => x.BillOfMaterialId,
                        principalTable: "BillOfMaterial",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillOfMaterialDetail_RawMaterial",
                        column: x => x.RawMaterialId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ManufacturingOrderDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ManufacturingOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockQuantity = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManufacturingOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufacturingOrderDetail_ManufacturingOrder",
                        column: x => x.ManufacturingOrderId,
                        principalTable: "ManufacturingOrder",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ManufacturingOrderDetail_RawMaterial",
                        column: x => x.RawMaterialId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductionDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AdjustmentQuantity = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AdjustmentValue = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    ActualUsedQuantity = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,10)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionDetail_Production",
                        column: x => x.ProductionId,
                        principalTable: "Production",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionDetail_RawMaterial",
                        column: x => x.RawMaterialId,
                        principalTable: "Product",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleInvoiceDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleOrderDetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrimaryQuantity = table.Column<int>(type: "int", nullable: false),
                    PrimaryBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BonusQuantity = table.Column<int>(type: "int", nullable: false),
                    ReturnQuantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NetRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OfferDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InvoiceDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CashDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SpecialDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MonthlyDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    YearlyDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TargetDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReturnAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DeliveryNoteNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtherDiscountPerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    TransportationCostPerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    DepoChargePerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentageDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    VatPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdjustmentAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AdjustmentBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    AdjustmentPercentageDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    AdjustmentQuantity = table.Column<int>(type: "int", nullable: false),
                    StockIssueCogs = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleInvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleInvoiceDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleInvoiceDetail_SaleInvoice",
                        column: x => x.SaleInvoiceId,
                        principalTable: "SaleInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerWiseProductDiscountUsageHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerWiseProductDiscountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerWiseProductDiscountUsageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerWiseProductDiscountUsageHistory_CustomerWiseProductDiscount",
                        column: x => x.CustomerWiseProductDiscountId,
                        principalTable: "CustomerWiseProductDiscount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerWiseProductDiscountUsageHistory_SaleOrder",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiscountProductWiseUsageHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscountProductWiseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountProductWiseUsageHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountProductWiseUsageHistory_DiscountProductWise",
                        column: x => x.DiscountProductWiseId,
                        principalTable: "DiscountProductWise",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiscountProductWiseUsageHistory_SaleOrder",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleOrderDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BagWeight = table.Column<int>(type: "int", nullable: false),
                    PrimaryQuantity = table.Column<int>(type: "int", nullable: false),
                    PrimaryBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    BonusQuantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    NetRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OfferDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InvoiceDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CashDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    SpecialDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DeliveredPrimaryQuantity = table.Column<int>(type: "int", nullable: false),
                    DeliveredPrimaryBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    OtherDiscountPerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    CurrentStockQuantity = table.Column<int>(type: "int", nullable: false),
                    TransportationCostPerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    DepoChargePerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentageDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    VatPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleOrderDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleOrderDetail_SaleOrder",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SaleReturnDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleReturnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BagWeight = table.Column<int>(type: "int", nullable: false),
                    PrimaryQuantity = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PrimaryBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    BonusQuantity = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReturnPrimaryQuantity = table.Column<int>(type: "int", nullable: false),
                    ReturnQuantity = table.Column<int>(type: "int", nullable: false),
                    ReturnPrimaryBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    ReturnBonusQuantity = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    MonthlyDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    YearlyDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TargetDiscountPerUnit = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PercentageDiscountAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OtherDiscountPerUnit = table.Column<decimal>(type: "decimal(20,12)", nullable: false),
                    VatPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleReturnDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleReturnDetail_Product",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SaleReturnDetail_SaleReturn",
                        column: x => x.SaleReturnId,
                        principalTable: "SaleReturn",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LcAdjustmentDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LcAdjustmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LcAdjustmentDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LcAdjustmentDetail_Account",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LcAdjustmentDetail_LcAdjustment",
                        column: x => x.LcAdjustmentId,
                        principalTable: "LcAdjustment",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LCCostEntryDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LCCostEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebitAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsIncludedWithinLandedCost = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LCCostEntryDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LCCostEntryDetail_CreditAccount",
                        column: x => x.CreditAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LCCostEntryDetail_DebitAccount",
                        column: x => x.DebitAccountId,
                        principalTable: "Account",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LCCostEntryDetail_LCCostEntry",
                        column: x => x.LCCostEntryId,
                        principalTable: "LCCostEntry",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_AccountTypeId",
                table: "Account",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_CustomerMarketingOfficerId",
                table: "Account",
                column: "CustomerMarketingOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_ParentId",
                table: "Account",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "UQ_Account_Code",
                table: "Account",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Area_ZoneId",
                table: "Area",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccount_AccountId",
                table: "BankAccount",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterial_FinishedProductId",
                table: "BillOfMaterial",
                column: "FinishedProductId");

            migrationBuilder.CreateIndex(
                name: "UQ_BomNo",
                table: "BillOfMaterial",
                column: "BomNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialDetail_BillOfMaterialId",
                table: "BillOfMaterialDetail",
                column: "BillOfMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialDetail_RawMaterialId",
                table: "BillOfMaterialDetail",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "UQ_Company_Code",
                table: "Company",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWiseProductDiscount_CustomerId",
                table: "CustomerWiseProductDiscount",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWiseProductDiscountDetail_CustomerWiseProductDiscountId",
                table: "CustomerWiseProductDiscountDetail",
                column: "CustomerWiseProductDiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWiseProductDiscountDetail_ProductId",
                table: "CustomerWiseProductDiscountDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWiseProductDiscountUsageHistory_CustomerWiseProductDiscountId",
                table: "CustomerWiseProductDiscountUsageHistory",
                column: "CustomerWiseProductDiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWiseProductDiscountUsageHistory_SaleOrderId",
                table: "CustomerWiseProductDiscountUsageHistory",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNote_CustomerId",
                table: "DeliveryNote",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNote_StoreId",
                table: "DeliveryNote",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "UQ_DeliveryNoteNo",
                table: "DeliveryNote",
                column: "DeliveryNoteNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteDetail_DeliveryNoteId",
                table: "DeliveryNoteDetail",
                column: "DeliveryNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNoteDetail_ProductId",
                table: "DeliveryNoteDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Designation_DepartmentId",
                table: "Designation",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountProductWiseDetail_DiscountProductWiseId",
                table: "DiscountProductWiseDetail",
                column: "DiscountProductWiseId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountProductWiseDetail_ProductId",
                table: "DiscountProductWiseDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountProductWiseUsageHistory_DiscountProductWiseId",
                table: "DiscountProductWiseUsageHistory",
                column: "DiscountProductWiseId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountProductWiseUsageHistory_SaleOrderId",
                table: "DiscountProductWiseUsageHistory",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DepartmentId",
                table: "Employee",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_DesignationId",
                table: "Employee",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_JobLocationId",
                table: "Employee",
                column: "JobLocationId");

            migrationBuilder.CreateIndex(
                name: "UQ_FinancialYear_Code",
                table: "FinancialYear",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundTransfer_CostCenterId",
                table: "FundTransfer",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransfer_FundTransferTransactionTypeId",
                table: "FundTransfer",
                column: "FundTransferTransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransfer_TransferFromAccountId",
                table: "FundTransfer",
                column: "TransferFromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransfer_TransferToAccountId",
                table: "FundTransfer",
                column: "TransferToAccountId");

            migrationBuilder.CreateIndex(
                name: "UQ_FundTransferNo",
                table: "FundTransfer",
                column: "FundTransferNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Generic_ProductTypeId",
                table: "Generic",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiveNote_StoreId",
                table: "GoodsReceiveNote",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiveNote_SupplierId",
                table: "GoodsReceiveNote",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "UQ_GRNNo",
                table: "GoodsReceiveNote",
                column: "GRNNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiveNoteDetail_GoodsReceiveNoteId",
                table: "GoodsReceiveNoteDetail",
                column: "GoodsReceiveNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiveNoteDetail_ProductId",
                table: "GoodsReceiveNoteDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntry_CostCenterId",
                table: "JournalEntry",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "UQ_JournalEntry_VoucherNo",
                table: "JournalEntry",
                column: "VoucherNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryDetail_AccountId",
                table: "JournalEntryDetail",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntryDetail_JournalEntryId",
                table: "JournalEntryDetail",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_LcAdjustment_CostCenterId",
                table: "LcAdjustment",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_LcAdjustment_PurchaseInvoiceId",
                table: "LcAdjustment",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_LcAdjustmentDetail_AccountId",
                table: "LcAdjustmentDetail",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LcAdjustmentDetail_LcAdjustmentId",
                table: "LcAdjustmentDetail",
                column: "LcAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_LCCostEntry_CostCenterId",
                table: "LCCostEntry",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_LCCostEntry_PurchaseOrderId",
                table: "LCCostEntry",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LCCostEntryDetail_CreditAccountId",
                table: "LCCostEntryDetail",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LCCostEntryDetail_DebitAccountId",
                table: "LCCostEntryDetail",
                column: "DebitAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LCCostEntryDetail_LCCostEntryId",
                table: "LCCostEntryDetail",
                column: "LCCostEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrder_FinishedProductId",
                table: "ManufacturingOrder",
                column: "FinishedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrder_RawMaterialStoreId",
                table: "ManufacturingOrder",
                column: "RawMaterialStoreId");

            migrationBuilder.CreateIndex(
                name: "UQ_ManufacturingOrderNo",
                table: "ManufacturingOrder",
                column: "ManufacturingOrderNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrderDetail_ManufacturingOrderId",
                table: "ManufacturingOrderDetail",
                column: "ManufacturingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ManufacturingOrderDetail_RawMaterialId",
                table: "ManufacturingOrderDetail",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVoucher_CashBankAccountId",
                table: "PaymentVoucher",
                column: "CashBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVoucher_CostCenterId",
                table: "PaymentVoucher",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVoucher_FundTransferTransactionTypeId",
                table: "PaymentVoucher",
                column: "FundTransferTransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVoucher_PaymentModeId",
                table: "PaymentVoucher",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "UQ_PaymentVoucher_VoucherNo",
                table: "PaymentVoucher",
                column: "VoucherNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVoucherDetail_AccountId",
                table: "PaymentVoucherDetail",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentVoucherDetail_PaymentVoucherId",
                table: "PaymentVoucherDetail",
                column: "PaymentVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_PoPriceAdjustmentAfterGrn_StoreId",
                table: "PoPriceAdjustmentAfterGrn",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PoPriceAdjustmentAfterGrn_SupplierId",
                table: "PoPriceAdjustmentAfterGrn",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "UQ_PoPriceAdjustmentAfterGrn_Code",
                table: "PoPriceAdjustmentAfterGrn",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoPriceAdjustmentAfterGrnDetail_PoPriceAdjustmentAfterGrnId",
                table: "PoPriceAdjustmentAfterGrnDetail",
                column: "PoPriceAdjustmentAfterGrnId");

            migrationBuilder.CreateIndex(
                name: "IX_PoPriceAdjustmentAfterGrnDetail_ProductId",
                table: "PoPriceAdjustmentAfterGrnDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CategoryId",
                table: "Product",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CountryId",
                table: "Product",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_GenericId",
                table: "Product",
                column: "GenericId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_InventoryTypeId",
                table: "Product",
                column: "InventoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ManufacturerId",
                table: "Product",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_MeasurementUnitId",
                table: "Product",
                column: "MeasurementUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_PackSizeId",
                table: "Product",
                column: "PackSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductTypeId",
                table: "Product",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Code",
                table: "Product",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAudit_CategoryId",
                table: "ProductAudit",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAudit_CountryId",
                table: "ProductAudit",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAudit_GenericId",
                table: "ProductAudit",
                column: "GenericId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAudit_InventoryTypeId",
                table: "ProductAudit",
                column: "InventoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAudit_ManufacturerId",
                table: "ProductAudit",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAudit_MeasurementUnitId",
                table: "ProductAudit",
                column: "MeasurementUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAudit_PackSizeId",
                table: "ProductAudit",
                column: "PackSizeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAudit_ProductTypeId",
                table: "ProductAudit",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostSetup_ProductId",
                table: "ProductCostSetup",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Production_FGStoreId",
                table: "Production",
                column: "FGStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Production_FinishedProductId",
                table: "Production",
                column: "FinishedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Production_MachineId",
                table: "Production",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Production_ShiftId",
                table: "Production",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "UQ_ProductionNo",
                table: "Production",
                column: "ProductionNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetail_ProductionId",
                table: "ProductionDetail",
                column: "ProductionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetail_RawMaterialId",
                table: "ProductionDetail",
                column: "RawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductType_InventoryTypeId",
                table: "ProductType",
                column: "InventoryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_StoreId",
                table: "PurchaseInvoice",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_SupplierId",
                table: "PurchaseInvoice",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "UQ_PurchaseInvoiceNo",
                table: "PurchaseInvoice",
                column: "PurchaseInvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetail_ProductId",
                table: "PurchaseInvoiceDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceDetail_PurchaseInvoiceId",
                table: "PurchaseInvoiceDetail",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrder_DeliveryPlaceId",
                table: "PurchaseOrder",
                column: "DeliveryPlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrder_StoreId",
                table: "PurchaseOrder",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrder_SupplierId",
                table: "PurchaseOrder",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "UQ_PONumber",
                table: "PurchaseOrder",
                column: "PONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetail_ProductId",
                table: "PurchaseOrderDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetail_PurchaseOrderId",
                table: "PurchaseOrderDetail",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisition_DepartmentId",
                table: "PurchaseRequisition",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisition_StoreId",
                table: "PurchaseRequisition",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "UQ_RequisitionNo",
                table: "PurchaseRequisition",
                column: "RequisitionNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionDetail_ProductId",
                table: "PurchaseRequisitionDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionDetail_PurchaseRequisitionId",
                table: "PurchaseRequisitionDetail",
                column: "PurchaseRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionPurchaseOrderMapping_ProductId",
                table: "PurchaseRequisitionPurchaseOrderMapping",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionPurchaseOrderMapping_PurchaseOrderId",
                table: "PurchaseRequisitionPurchaseOrderMapping",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturn_StoreId",
                table: "PurchaseReturn",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturn_SupplierId",
                table: "PurchaseReturn",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "UQ_PurchaseReturnNo",
                table: "PurchaseReturn",
                column: "PurchaseReturnNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetail_ProductId",
                table: "PurchaseReturnDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetail_PurchaseReturnId",
                table: "PurchaseReturnDetail",
                column: "PurchaseReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePayment_CostCenterId",
                table: "ReceivePayment",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePayment_CustomerId",
                table: "ReceivePayment",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePayment_FundTransferTransactionTypeId",
                table: "ReceivePayment",
                column: "FundTransferTransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePayment_PaymentModeId",
                table: "ReceivePayment",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePayment_TenantId_Deleted",
                table: "ReceivePayment",
                columns: new[] { "Deleted", "TenantId" });

            migrationBuilder.CreateIndex(
                name: "UQ_ReceivePayment_Code",
                table: "ReceivePayment",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePaymentAgainstSale_CostCenterId",
                table: "ReceivePaymentAgainstSale",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePaymentAgainstSale_CustomerId",
                table: "ReceivePaymentAgainstSale",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePaymentAgainstSale_ToAccountId",
                table: "ReceivePaymentAgainstSale",
                column: "ToAccountId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReceivePaymentAgainstSale_Code",
                table: "ReceivePaymentAgainstSale",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePaymentDetail_AccountId",
                table: "ReceivePaymentDetail",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivePaymentDetail_ReceivePaymentId",
                table: "ReceivePaymentDetail",
                column: "ReceivePaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiveVoucher_CashBankAccountId",
                table: "ReceiveVoucher",
                column: "CashBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiveVoucher_CostCenterId",
                table: "ReceiveVoucher",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiveVoucher_FundTransferTransactionTypeId",
                table: "ReceiveVoucher",
                column: "FundTransferTransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiveVoucher_PaymentModeId",
                table: "ReceiveVoucher",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReceiveVoucher_VoucherNo",
                table: "ReceiveVoucher",
                column: "VoucherNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiveVoucherDetail_AccountId",
                table: "ReceiveVoucherDetail",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiveVoucherDetail_ReceiveVoucherId",
                table: "ReceiveVoucherDetail",
                column: "ReceiveVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_RfqSent_RequisitionId",
                table: "RfqSent",
                column: "RequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RfqSent_SupplierId",
                table: "RfqSent",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_CustomerId",
                table: "SaleInvoice",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_CustomerTerritoryId",
                table: "SaleInvoice",
                column: "CustomerTerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_StoreId",
                table: "SaleInvoice",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "UQ_InvoiceNo",
                table: "SaleInvoice",
                column: "InvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoiceDetail_ProductId",
                table: "SaleInvoiceDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoiceDetail_SaleInvoiceId",
                table: "SaleInvoiceDetail",
                column: "SaleInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrder_CustomerId",
                table: "SaleOrder",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrder_CustomerTerritoryId",
                table: "SaleOrder",
                column: "CustomerTerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrder_StoreId",
                table: "SaleOrder",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "UQ_SaleOrderNo",
                table: "SaleOrder",
                column: "SaleOrderNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderDetail_ProductId",
                table: "SaleOrderDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderDetail_SaleOrderId",
                table: "SaleOrderDetail",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleQuotation_CustomerId",
                table: "SaleQuotation",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "UQ_SaleQuotation_QuotationNo",
                table: "SaleQuotation",
                column: "QuotationNo",
                unique: true,
                filter: "[QuotationNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SaleQuotationDetail_ProductId",
                table: "SaleQuotationDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleQuotationDetail_SaleQuotationId",
                table: "SaleQuotationDetail",
                column: "SaleQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleReturn_CustomerId",
                table: "SaleReturn",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleReturn_CustomerTerritoryId",
                table: "SaleReturn",
                column: "CustomerTerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleReturn_StoreId",
                table: "SaleReturn",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "UQ_SaleReturnNo",
                table: "SaleReturn",
                column: "SaleReturnNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleReturnDetail_ProductId",
                table: "SaleReturnDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleReturnDetail_SaleReturnId",
                table: "SaleReturnDetail",
                column: "SaleReturnId");

            migrationBuilder.CreateIndex(
                name: "idx_available_quantity_non_zero",
                table: "Stock",
                column: "AvailableQty",
                filter: "([AvailableQty]<>(0))");

            migrationBuilder.CreateIndex(
                name: "idx_stock_financialyearid",
                table: "Stock",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "idx_stock_productid",
                table: "Stock",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "idx_stock_stockindate",
                table: "Stock",
                column: "StockInDate");

            migrationBuilder.CreateIndex(
                name: "idx_stock_storeid",
                table: "Stock",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "idx_stock_tenantid",
                table: "Stock",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "idx_stock_transactiontype",
                table: "Stock",
                column: "TransactonType");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustment_StoreId",
                table: "StockAdjustment",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "UQ_StockAdjustment_Code",
                table: "StockAdjustment",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetail_ProductId",
                table: "StockAdjustmentDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentDetail_StockAdjustmentId",
                table: "StockAdjustmentDetail",
                column: "StockAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_DestinationId",
                table: "StockTransfer",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_SourceId",
                table: "StockTransfer",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "UQ_TransferNo",
                table: "StockTransfer",
                column: "TransferNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferDetail_ProductId",
                table: "StockTransferDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferDetail_StockTransferId",
                table: "StockTransferDetail",
                column: "StockTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Store_InventoryTypeId",
                table: "Store",
                column: "InventoryTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_Store_Code",
                table: "Store",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayment_CostCenterId",
                table: "SupplierPayment",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayment_FundTransferTransactionTypeId",
                table: "SupplierPayment",
                column: "FundTransferTransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayment_PaymentModeId",
                table: "SupplierPayment",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayment_SupplierId",
                table: "SupplierPayment",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "UQ_SupplierPayment_Code",
                table: "SupplierPayment",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPaymentAgainstPurchase_CostCenterId",
                table: "SupplierPaymentAgainstPurchase",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPaymentAgainstPurchase_FromAccountId",
                table: "SupplierPaymentAgainstPurchase",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPaymentAgainstPurchase_PurchaseInvoiceId",
                table: "SupplierPaymentAgainstPurchase",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "UQ_SupplierPaymentAgainstPurchase_Code",
                table: "SupplierPaymentAgainstPurchase",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPaymentDetail_AccountId",
                table: "SupplierPaymentDetail",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPaymentDetail_SupplierPaymentId",
                table: "SupplierPaymentDetail",
                column: "SupplierPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierTransactionAgainstPo_PurchaseOrderId",
                table: "SupplierTransactionAgainstPo",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierTransactionAgainstPo_SupplierId",
                table: "SupplierTransactionAgainstPo",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Territory_AreaId",
                table: "Territory",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_User_EmployeeId",
                table: "User",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorQuotation_SupplierId",
                table: "VendorQuotation",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "UQ_QuotationNo",
                table: "VendorQuotation",
                column: "QuotationNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorQuotationDetail_ProductId",
                table: "VendorQuotationDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorQuotationDetail_VendorQuotationId",
                table: "VendorQuotationDetail",
                column: "VendorQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherEntry_CashBankAccountId",
                table: "VoucherEntry",
                column: "CashBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherEntry_CostCenterId",
                table: "VoucherEntry",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherEntry_PaymentModeId",
                table: "VoucherEntry",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "UQ_VoucherEntry_VoucherNo",
                table: "VoucherEntry",
                column: "VoucherNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoucherEntryDetail_AccountId",
                table: "VoucherEntryDetail",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherEntryDetail_VoucherEntryId",
                table: "VoucherEntryDetail",
                column: "VoucherEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_Zone_RegionId",
                table: "Zone",
                column: "RegionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankAccount");

            migrationBuilder.DropTable(
                name: "BillOfMaterialDetail");

            migrationBuilder.DropTable(
                name: "Company");

            migrationBuilder.DropTable(
                name: "Currency");

            migrationBuilder.DropTable(
                name: "CustomerWiseProductDiscountDetail");

            migrationBuilder.DropTable(
                name: "CustomerWiseProductDiscountUsageHistory");

            migrationBuilder.DropTable(
                name: "DeliveryNoteDetail");

            migrationBuilder.DropTable(
                name: "DiscountProductWiseDetail");

            migrationBuilder.DropTable(
                name: "DiscountProductWiseUsageHistory");

            migrationBuilder.DropTable(
                name: "EmailAccount");

            migrationBuilder.DropTable(
                name: "EventLog");

            migrationBuilder.DropTable(
                name: "FinancialYear");

            migrationBuilder.DropTable(
                name: "FundTransfer");

            migrationBuilder.DropTable(
                name: "GoodsReceiveNoteDetail");

            migrationBuilder.DropTable(
                name: "JournalEntryDetail");

            migrationBuilder.DropTable(
                name: "LcAdjustmentDetail");

            migrationBuilder.DropTable(
                name: "LCCostEntryDetail");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "ManufacturingOrderDetail");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "PaymentMethod");

            migrationBuilder.DropTable(
                name: "PaymentVoucherDetail");

            migrationBuilder.DropTable(
                name: "Picture");

            migrationBuilder.DropTable(
                name: "PoPriceAdjustmentAfterGrnDetail");

            migrationBuilder.DropTable(
                name: "ProductAudit");

            migrationBuilder.DropTable(
                name: "ProductCostSetup");

            migrationBuilder.DropTable(
                name: "ProductionDetail");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceDetail");

            migrationBuilder.DropTable(
                name: "PurchaseInvoiceSupplierPaymentMapping");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDetail");

            migrationBuilder.DropTable(
                name: "PurchaseRequisitionDetail");

            migrationBuilder.DropTable(
                name: "PurchaseRequisitionPurchaseOrderMapping");

            migrationBuilder.DropTable(
                name: "PurchaseReturnDetail");

            migrationBuilder.DropTable(
                name: "ReceivePaymentAgainstSale");

            migrationBuilder.DropTable(
                name: "ReceivePaymentAgainstSaleSaleInvoiceMapping");

            migrationBuilder.DropTable(
                name: "ReceivePaymentDetail");

            migrationBuilder.DropTable(
                name: "ReceivePaymentPictureMapping");

            migrationBuilder.DropTable(
                name: "ReceiveVoucherDetail");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "RfqSent");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "SaleInvoiceDetail");

            migrationBuilder.DropTable(
                name: "SaleOrderDetail");

            migrationBuilder.DropTable(
                name: "SaleQuotationDetail");

            migrationBuilder.DropTable(
                name: "SaleReturnDetail");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropTable(
                name: "Stock");

            migrationBuilder.DropTable(
                name: "StockAdjustmentDetail");

            migrationBuilder.DropTable(
                name: "StockTransferDetail");

            migrationBuilder.DropTable(
                name: "SupplierPaymentAgainstPurchase");

            migrationBuilder.DropTable(
                name: "SupplierPaymentDetail");

            migrationBuilder.DropTable(
                name: "SupplierProductMapping");

            migrationBuilder.DropTable(
                name: "SupplierTransactionAgainstPo");

            migrationBuilder.DropTable(
                name: "Tenant");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "VendorQuotationDetail");

            migrationBuilder.DropTable(
                name: "VoucherEntryDetail");

            migrationBuilder.DropTable(
                name: "BillOfMaterial");

            migrationBuilder.DropTable(
                name: "CustomerWiseProductDiscount");

            migrationBuilder.DropTable(
                name: "DeliveryNote");

            migrationBuilder.DropTable(
                name: "DiscountProductWise");

            migrationBuilder.DropTable(
                name: "GoodsReceiveNote");

            migrationBuilder.DropTable(
                name: "JournalEntry");

            migrationBuilder.DropTable(
                name: "LcAdjustment");

            migrationBuilder.DropTable(
                name: "LCCostEntry");

            migrationBuilder.DropTable(
                name: "ManufacturingOrder");

            migrationBuilder.DropTable(
                name: "PaymentVoucher");

            migrationBuilder.DropTable(
                name: "PoPriceAdjustmentAfterGrn");

            migrationBuilder.DropTable(
                name: "Production");

            migrationBuilder.DropTable(
                name: "PurchaseReturn");

            migrationBuilder.DropTable(
                name: "ReceivePayment");

            migrationBuilder.DropTable(
                name: "ReceiveVoucher");

            migrationBuilder.DropTable(
                name: "PurchaseRequisition");

            migrationBuilder.DropTable(
                name: "SaleInvoice");

            migrationBuilder.DropTable(
                name: "SaleOrder");

            migrationBuilder.DropTable(
                name: "SaleQuotation");

            migrationBuilder.DropTable(
                name: "SaleReturn");

            migrationBuilder.DropTable(
                name: "StockAdjustment");

            migrationBuilder.DropTable(
                name: "StockTransfer");

            migrationBuilder.DropTable(
                name: "SupplierPayment");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "VendorQuotation");

            migrationBuilder.DropTable(
                name: "VoucherEntry");

            migrationBuilder.DropTable(
                name: "PurchaseInvoice");

            migrationBuilder.DropTable(
                name: "PurchaseOrder");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Machine");

            migrationBuilder.DropTable(
                name: "Shift");

            migrationBuilder.DropTable(
                name: "Territory");

            migrationBuilder.DropTable(
                name: "FundTransferTransactionType");

            migrationBuilder.DropTable(
                name: "CostCenter");

            migrationBuilder.DropTable(
                name: "PaymentMode");

            migrationBuilder.DropTable(
                name: "DeliveryPlace");

            migrationBuilder.DropTable(
                name: "Store");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Country");

            migrationBuilder.DropTable(
                name: "Generic");

            migrationBuilder.DropTable(
                name: "Manufacturer");

            migrationBuilder.DropTable(
                name: "MeasurementUnit");

            migrationBuilder.DropTable(
                name: "PackSize");

            migrationBuilder.DropTable(
                name: "Area");

            migrationBuilder.DropTable(
                name: "AccountType");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "ProductType");

            migrationBuilder.DropTable(
                name: "Zone");

            migrationBuilder.DropTable(
                name: "Designation");

            migrationBuilder.DropTable(
                name: "JobLocation");

            migrationBuilder.DropTable(
                name: "InventoryType");

            migrationBuilder.DropTable(
                name: "Region");

            migrationBuilder.DropTable(
                name: "Department");
        }
    }
}
