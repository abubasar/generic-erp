using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.Reports;
using Application.Services.ViewModels.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Accounts.PdfAccount
{
    public class AccountReportPdfService : IAccountReportPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly IAccountService _accountService;

        public AccountReportPdfService(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
            _accountService = accountService;
        }

        public Paragraph CreateSmallLineSeparator()
        {
            //Small Line
            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
            return line;
        }

        public Paragraph CreateLineSeparator()
        {
            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 80.0F, BaseColor.Black, Element.ALIGN_CENTER, 13F)));
            return line;
        }

        public PdfPTable SpaceTable(Font fontArial9)
        {
            // =====
            // Space
            // =====
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 5f });

            return spaceTable;
        }

        public async Task<PdfPTable> AddHeaderAsync(string reportTitleName, Font fontArial9, Font fontArial12Bold, Font fontArial8Bold)
        {
            Guid? tenantId = _workContext.GetTenantId();
            TenantViewModel tenantData = await _tenantService.GetByIdAsync(tenantId);

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(tenantData.Logo!)));

            // Set the size of the image if needed
            logo.ScaleToFit(44, 110);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 0f;
            logo.SpacingAfter = 0f;
            // ===========
            // Header Page
            // ===========

            PdfPTable headerPage = new PdfPTable(3);
            float[] widthsCellsHeaderPage = new float[] { 10f, 80f, 10f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(" ", fontArial12Bold)) { Rowspan = 4, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData.Name, fontArial12Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(logo) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, Rowspan = 4 });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData.Address, fontArial9)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email :" + tenantData?.Email, fontArial9)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitleName, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });

            return headerPage;
        }

        public PdfPTable SubHeaderTable(Font fontArial9, Font fontArial9Bold, Account? account)
        {
            PdfPTable subHeaderTable = new(1);
            float[] widthsCellsSubHeaderTable = new float[] { 100f };
            subHeaderTable.WidthPercentage = 100;
            subHeaderTable.SpacingAfter = 7;
            subHeaderTable.SetWidths(widthsCellsSubHeaderTable);
            subHeaderTable.AddCell(new PdfPCell(new Phrase("Name:              " + account?.Name, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

            if (account?.ParentId == Guid.Parse(AccountHeadConstants.TradeReceivable) || account?.ParentId == Guid.Parse(AccountHeadConstants.TradePayable))
            {
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Address:          " + account?.Address, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                subHeaderTable.AddCell(new PdfPCell(new Phrase((account?.ParentId == Guid.Parse(AccountHeadConstants.TradeReceivable) ? "Customer ID:   " : "Supplier ID:     ") + account?.Code, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Contact No:     " + account?.ContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            }
            return subHeaderTable;
        }

        public PdfPTable AddFooter(string preparedBy, string checkedBy, string approvedBy, Font fontArial8, Font fontArial9Bold)
        {
            var line = CreateSmallLineSeparator();
            PdfPTable tableUsers = new(4);
            float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
            tableUsers.WidthPercentage = 100;
            tableUsers.SetWidths(widthsCellsTableUsers);
            tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Authorized by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });

            tableUsers.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 1f, HorizontalAlignment = 1 });

            return tableUsers;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSubsidiaryLedgerReportToPdfAsync(MemoryStream stream, IList<SubsidiaryLedgerViewModel> list, string reportTitleName, Account? account)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportTitleName, fontArial9, fontArial12Bold, fontArial8Bold);
            var filterTable = SubHeaderTable(fontArial9, fontArial9Bold, account);
            var dataTable = GetSubsidiaryLedgerTable(fontArial8, fontArial8Bold, list);
            var line = CreateLineSeparator();
            document.Add(headerTable);
            document.Add(line);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable GetSubsidiaryLedgerTable(Font fontArial8, Font fontArial8Bold, IList<SubsidiaryLedgerViewModel> list)
        {
            PdfPTable subsidiaryLedgerTable = new(7);
            float[] widthsCellsSubHeaderTable = new float[] { 8f, 13f, 21f, 25f, 11f, 11f, 11f };
            subsidiaryLedgerTable.WidthPercentage = 100;
            subsidiaryLedgerTable.SetWidths(widthsCellsSubHeaderTable);
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase("Voucher No", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase("Account", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase("Narration / Remarks", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase("Debit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase("Credit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            subsidiaryLedgerTable.HeaderRows = 1;
            foreach (var item in list)
            {
                subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(item.VoucherDate, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(item.Vnumber, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(item.AccountName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(item.Particular, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(item.Debit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(item.Credit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            }
            //..................total..................................
            var totalDebit = list.Sum(x => x.Debit);
            var totalCredit = list.Sum(x => x.Credit);
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase("Total Transaction", fontArial8)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(totalDebit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(totalCredit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            subsidiaryLedgerTable.AddCell(new PdfPCell(new Phrase(list.Last().Balance.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });

            return subsidiaryLedgerTable;
        }

        public PdfPTable SearchFilterTable(DateTime? fromDate, DateTime? toDate, string? supplierName = "", string? costCenterName = "", string? customerName = "")
        {
            PdfPTable headerPage = new(1);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 9, Font.NORMAL);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            if (fromDate.HasValue && toDate.HasValue)
                headerPage.AddCell(new PdfPCell(new Phrase("Report for the period of " + fromDate.Value.ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            if (!string.IsNullOrEmpty(supplierName))
                headerPage.AddCell(new PdfPCell(new Phrase("Supplier:           " + supplierName, fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });

            if (!string.IsNullOrEmpty(customerName))
                headerPage.AddCell(new PdfPCell(new Phrase("Customer:         " + customerName, fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });

            if (!string.IsNullOrEmpty(costCenterName))
                headerPage.AddCell(new PdfPCell(new Phrase("Cost Center:     " + costCenterName, fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });

            return headerPage;
        }

        public PdfPTable AddPrimarySubHeader(Font fontArial10, PaymentCollectionRequestModel request, int businessType)
        {
            PdfPTable subHeaderTable = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            subHeaderTable.SetWidths(widthsCellsHeaderPage);
            subHeaderTable.WidthPercentage = 100;
            subHeaderTable.SpacingAfter = 3;
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Report for the period of " + request.FromDate.Value.ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.ToString("dd/MM/yyyy"), fontArial10)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (request.CostCenterId.HasValue)
            {
                var costCenterName = _unitOfWork.Repository<CostCenter>().TableNoTracking().FirstOrDefault(x => x.Id == request.CostCenterId)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Cost Center : " + costCenterName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.PaymentModeId.HasValue)
            {
                var paymentModeName = _unitOfWork.Repository<Core.Entities.PaymentMode>().TableNoTracking().FirstOrDefault(x => x.Id == request.PaymentModeId.Value)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Payment Mode : " + paymentModeName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerZoneId.HasValue)
            {
                if (businessType == (int)BusinessType.Secondary)
                {
                    var customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)?.Name;
                    subHeaderTable.AddCell(new PdfPCell(new Phrase("Zone : " + customerZoneName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });

                }
                else
                {
                    var customerZoneName = _unitOfWork.Repository<Region>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)?.Name;
                    subHeaderTable.AddCell(new PdfPCell(new Phrase("Zone : " + customerZoneName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
                }
            }
            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Customer : " + customerName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value)?.FullName;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Marketing Officer : " + marketingOfficerName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            return subHeaderTable;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPaymentCollectionReportToPdfAsync(MemoryStream stream, List<Transaction> transactions, PaymentCollectionRequestModel request, string reportTitle)
        {
            // Prepare headers & column widths
            List<string> headers = new List<string> { "Sl", "Date", "VoucherNo", "Code", "Name", "Cash/Bank", "Description", "Amount" };
            List<float> columnWidths = new List<float> { 4f, 7f, 10f, 8f, 20f, 22f, 19f, 10f };

            // Create a list of data with properties mapped to header text
            List<object> tableData = new List<object>();
            int sl = 0;

            foreach (var transaction in transactions)
            {
                var account = await _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == transaction.AccountId);
                sl++;
                tableData.Add(new
                {
                    SL = sl,
                    Date = transaction.TransactionDate.ToString("dd/MM/yyyy"),
                    VoucherNo = transaction.Vnumber,
                    account?.Code,
                    account?.Name,
                    //CashBankAccountName = await _accountService.GetContraAccountName(transaction),
                    CashBankAccountName = transaction.ContraAccountNames,
                    Particulars = transaction.Description,
                    Amount = transaction.Credit
                });
            }

            var tables = await GeneratePdfAsync(stream, headers, columnWidths, tableData, reportTitle, request, numericColumnsForSum: new List<string> { "Amount" }, true);

            return tables;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> GeneratePdfAsync(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, PaymentCollectionRequestModel request, List<string>? numericColumnsForSum = null, bool isLandscape = false)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            //Tenant info
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);


            //PDF generator setup
            PdfGenerator pdfGenerator = new PdfGenerator(stream, isLandscape);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font tableHeaderFont = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font tableDataFont = FontFactory.GetFont("Arial", 8);

            // Add main header
            pdfGenerator.AddHeader(reportTitleName, tenantData.Name!, tenantData?.Address!, tenantData?.ContactNo!, tenantData?.Email!);

            //Add subheader (filter info)
            var filterTable = pdfGenerator.AddTable(AddPrimarySubHeader(fontArial9, request, tenantData!.BusinessType));

            // Add main data table
            var dataTable = pdfGenerator.AddTable(tableData, headers, columnWidths, tableHeaderFont, tableDataFont, numericColumnsForSum);

            // Close the PDF document
            pdfGenerator.Close();

            return (filterTable, dataTable);
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportToPdfAsync(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, List<string>? numericColumnsForSum = null, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, string? supplierName = null, string? costCenterName = null, string? customerName = null)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, isLandscape);
            //fonts
            Font tableHeaderFont = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font tableDataFont = FontFactory.GetFont("Arial", 8);
            // Add the header
            pdfGenerator.AddHeader(reportTitleName, tenantData.Name!, tenantData?.Address!, tenantData?.ContactNo!, tenantData?.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(fromDate, toDate, supplierName, costCenterName, customerName));
            // Add the table
            var dataTable = pdfGenerator.AddTable(tableData, headers, columnWidths, tableHeaderFont, tableDataFont, numericColumnsForSum);

            // Close the PDF document
            pdfGenerator.Close();
            return (filterTable, dataTable);
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintDayWiseAccountLedgerReportToPdfAsync(MemoryStream stream, IList<SubsidiaryLedgerViewModel> list, Account account, string? costCenterName = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);

            var headerTable = await AddDayWiseAccountLedgerHeaderAsync(costCenterName, fontArial10Bold, fontArial9, fontArial12Bold, fontArial8Bold, fromDate, toDate);
            var filterTable = SubHeaderTable(fontArial9, fontArial9Bold, account);
            var dataTable = GetDayWiseAccountLedgerTable(fontArial8, fontArial8Bold, list);
            var dayWiseAccountLedgerFooter = AddDayWiseAccountLedgerFooter(fontArial9Bold);
            var spaceTable = SpaceTable(fontArial8Bold);
            var line = CreateLineSeparator();
            document.Add(headerTable);
            document.Add(line);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(dayWiseAccountLedgerFooter);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable GetDayWiseAccountLedgerTable(Font fontArial8, Font fontArial8Bold, IList<SubsidiaryLedgerViewModel> list)
        {
            var groupedData = list.GroupBy(x => x.VoucherDate).OrderBy(x => x.Key).ToList();
            PdfPTable dayWiseAccountLedgerTable = new(7);
            float[] widthsCellsDayWiseAccountLedgerTablee = new float[] { 8f, 13f, 21f, 25f, 11f, 11f, 11f };
            dayWiseAccountLedgerTable.WidthPercentage = 100;
            dayWiseAccountLedgerTable.SetWidths(widthsCellsDayWiseAccountLedgerTablee);
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Balance:     " + list.Last().Balance.ToString("#,##0.00"), fontArial8Bold)) { Colspan = 7, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Voucher No", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Account", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Narration / Remarks", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Debit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Credit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, BackgroundColor = BaseColor.LightGray });
            foreach (var dateWiseGroup in groupedData)
            {
                decimal sumDebit = dateWiseGroup.Sum(item => item.Debit);
                decimal sumCredit = dateWiseGroup.Sum(item => item.Credit);
                dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(dateWiseGroup.Key, fontArial8Bold)) { Colspan = 7, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                foreach (var item in dateWiseGroup)
                {
                    dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(item.VoucherDate, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                    dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(item.Vnumber, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                    dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(item.AccountName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                    dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(item.Particular, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                    dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(item.Debit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                    dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(item.Credit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                    dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                }
                dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(dateWiseGroup.Key, fontArial8)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(sumDebit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(sumCredit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
                dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            }
            //..................total..................................
            var totalDebit = list.Sum(x => x.Debit);
            var totalCredit = list.Sum(x => x.Credit);
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase("Total Transaction", fontArial8)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(totalDebit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(totalCredit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            dayWiseAccountLedgerTable.AddCell(new PdfPCell(new Phrase(list.Last().Balance.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });

            return dayWiseAccountLedgerTable;
        }

        public async Task<PdfPTable> AddDayWiseAccountLedgerHeaderAsync(string? costCenterName, Font fontArial10Bold, Font fontArial9, Font fontArial12Bold, Font fontArial8Bold, DateTime? fromDate, DateTime? toDate)
        {
            Guid? tenantId = _workContext.GetTenantId();
            TenantViewModel tenantData = await _tenantService.GetByIdAsync(tenantId);

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(tenantData.Logo!)));

            // Set the size of the image if needed
            logo.ScaleToFit(44, 110);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 0f;
            logo.SpacingAfter = 0f;

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new PdfPTable(3);
            float[] widthsCellsHeaderPage = new float[] { 10f, 80f, 10f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(" ", fontArial12Bold)) { Rowspan = 4, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData.Name, fontArial12Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(logo) { Rowspan = 4, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData.Address, fontArial9)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial9)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Account Ledger (Day Wise)", fontArial10Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });

            if (fromDate.HasValue && toDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Account Statement for the period of " + fromDate.Value.ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy") + ",   Cost Center: " + costCenterName, fontArial8Bold)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
            }

            return headerPage;
        }

        public PdfPTable AddDayWiseAccountLedgerFooter(Font fontArial9Bold)
        {
            var line = CreateSmallLineSeparator();
            PdfPTable tableUsers = new(4);
            float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
            tableUsers.WidthPercentage = 100;
            tableUsers.SetWidths(widthsCellsTableUsers);
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Authorized by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            return tableUsers;
        }

        public async Task<PdfPTable> SupplierLedgerProductWiseReportToPdfAsync(MemoryStream stream, IList<SupplierLedgerProductWiseViewModel> list, string reportTitle, Account account, string? costCenterName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);

            var headerTable = await AddReportHeaderAsync(reportTitle, fontArial10, fontArial13Bold, fontArial14Bold);
            var filteringDataTable = FilteringDataTable(fontArial9, account, costCenterName, fromDate, toDate);
            var supplierLedgerProductWiseData = SupplierLedgerProductWiseDataTable(fontArial8, fontArial8Bold, list);
            document.Add(headerTable);
            document.Add(filteringDataTable);
            document.Add(supplierLedgerProductWiseData);

            document.Close();

            return supplierLedgerProductWiseData;
        }

        public async Task<PdfPTable> AddReportHeaderAsync(string? titleName, Font fontArial10, Font fontArial13Bold, Font fontArial14Bold)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email :" + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(titleName, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });
            return headerPage;
        }
        public PdfPTable FilteringDataTable(Font fontArial9, Account? account, string? costCenterName, DateTime? fromDate, DateTime? toDate)
        {
            PdfPTable filteringTable = new(1);
            float[] widthsCellsFilteringTable = new float[] { 100f };
            filteringTable.WidthPercentage = 100;
            filteringTable.SetWidths(widthsCellsFilteringTable);
            if (fromDate.HasValue && toDate.HasValue)
            {
                filteringTable.AddCell(new PdfPCell(new Phrase("Date:                            " + fromDate?.ToString("dd/MM/yyy") + " to " + toDate?.ToString("dd/MM/yyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            if (account?.ParentId == Guid.Parse(AccountHeadConstants.TradeReceivable) || account?.ParentId == Guid.Parse(AccountHeadConstants.TradePayable))
            {
                filteringTable.AddCell(new PdfPCell(new Phrase((account?.ParentId == Guid.Parse(AccountHeadConstants.TradeReceivable) ? "Customer Code:          " : "Supplier Code:             ") + account?.Code, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            filteringTable.AddCell(new PdfPCell(new Phrase("Name:                          " + account?.Name, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });

            if (account?.ParentId == Guid.Parse(AccountHeadConstants.TradeReceivable) || account?.ParentId == Guid.Parse(AccountHeadConstants.TradePayable))
            {
                filteringTable.AddCell(new PdfPCell(new Phrase("Address:                      " + account?.Address, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
                filteringTable.AddCell(new PdfPCell(new Phrase("Contact No:                 " + account?.ContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            return filteringTable;
        }

        public PdfPTable SupplierLedgerProductWiseDataTable(Font fontArial8, Font fontArial8Bold, IList<SupplierLedgerProductWiseViewModel> list)
        {
            PdfPTable supplierLedgerProductWiseData = new(9);
            float[] widthsCellsSupplierLedgerProductWiseDataTable = new float[] { 6f, 9f, 38f, 8f, 7f, 5f, 9f, 9f, 9f };
            supplierLedgerProductWiseData.WidthPercentage = 100;
            supplierLedgerProductWiseData.SetWidths(widthsCellsSupplierLedgerProductWiseDataTable);
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Balance:  " + list.Last().Balance.ToString("#,##0.00"), fontArial8Bold)) { Colspan = 9, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Description", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Purchase", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Paid", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("PO", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Qty", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.HeaderRows = 3;
            foreach (var item in list)
            {
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Date, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.BillNo, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Description, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.PO, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Qty.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Paid.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            }

            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Qty).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 1f, PaddingRight = 1f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Amount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Paid).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            supplierLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });

            return supplierLedgerProductWiseData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerLedgerProductWiseReportToPdfAsync(MemoryStream stream, IList<CustomerLedgerProductWiseViewModel> list, Account account, string reportTitle, string costCenterName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);

            var headerTable = await AddReportHeaderAsync(reportTitle, fontArial10, fontArial13Bold, fontArial14Bold);
            var filterTable = FilteringDataTable(fontArial9, account, costCenterName, fromDate, toDate);
            var dataTable = CustomerLedgerProductWiseDataTable(fontArial7, fontArial7Bold, list);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable CustomerLedgerProductWiseDataTable(Font fontArial8, Font fontArial8Bold, IList<CustomerLedgerProductWiseViewModel> list)
        {
            PdfPTable customerLedgerProductWiseData = new(18);
            float[] widthsCellsCustomerLedgerProductWiseDataTable = new float[] { 9f, 6f, 12f, 6f, 4f, 5f, 4f, 4f, 4f, 8f, 7f, 7f, 7f, 8f, 6f, 6f, 8f, 7f };
            customerLedgerProductWiseData.WidthPercentage = 100;
            customerLedgerProductWiseData.SetWidths(widthsCellsCustomerLedgerProductWiseDataTable);
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Balance:  " + list.Last().Balance.ToString("#,##0.00"), fontArial8Bold)) { Colspan = 18, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Description", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Qty", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { Colspan = 7, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Paid", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Offer", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Other", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Net", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Offer", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Other", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Net", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Depo Charge", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Carring Cost", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.HeaderRows = 3;
            foreach (var item in list)
            {
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.BillNo, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Date, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Description, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Qty.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.TP.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Commission.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.OfferDiscountPerUnit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.OtherDiscountPerUnit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.NetRate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.TPAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.CommissionAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.OfferDiscountAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.OtherDiscountAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.NetRateAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase((item.Qty * item.DepoChargePerKg).ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase((item.Qty * item.TransportationCostPerUnit).ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Paid.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            }

            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8Bold)) { Colspan = 3, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Qty).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TPAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.CommissionAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OfferDiscountAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OtherDiscountAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.NetRateAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Qty * x.DepoChargePerKg).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Qty * x.TransportationCostPerUnit).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Paid).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerProductWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });

            return customerLedgerProductWiseData;
        }
    }
}
