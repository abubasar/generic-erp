using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.Reports;
using Application.Services.ViewModels.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Accounts.Pdf
{
    public class AccountPdfService : IAccountPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public AccountPdfService(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        protected virtual Font GetFont()
        {
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pdf", Path.GetFileName("RobotoRegular.ttf"));
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 8, Font.NORMAL);
            return font;
        }

        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }

        public Paragraph CreateSmallLineSeparator()
        {
            //Small Line
            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
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

        public PdfPTable AddCustomerTransactionFilteringTable(Font font, CustomerTransactionRequestModel request)
        {
            PdfPTable filterTable = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            filterTable.SetWidths(widthsCellsHeaderPage);
            filterTable.WidthPercentage = 100;
            filterTable.SpacingAfter = 3;

            if (request.FromDate.HasValue && request.ToDate.HasValue)
                filterTable.AddCell(new PdfPCell(new Phrase("Report for the period of " + request.FromDate.Value.ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.ToString("dd/MM/yyyy"), font)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value)?.FullName;
                filterTable.AddCell(new PdfPCell(new Phrase("Marketing Officer :  " + marketingOfficerName, font)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerZoneId.HasValue)
            {
                var customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)?.Name;
                filterTable.AddCell(new PdfPCell(new Phrase("Zone :                     " + customerZoneName, font)) { Border = 0, HorizontalAlignment = 0 });
            }
            return filterTable;
        }
        public PdfPTable AddFilteringTable(Font font, DateTime? fromDate, DateTime? toDate, string? accountCode, string? accountName)
        {
            PdfPTable filterTable = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            filterTable.SetWidths(widthsCellsHeaderPage);
            filterTable.WidthPercentage = 100;
            filterTable.SpacingAfter = 3;

            if (fromDate.HasValue && toDate.HasValue)
                filterTable.AddCell(new PdfPCell(new Phrase("Report for the period of " + fromDate.Value.ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), font)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            if (!string.IsNullOrWhiteSpace(accountCode))
                filterTable.AddCell(new PdfPCell(new Phrase("Code:                         " + accountCode, font)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            if (!string.IsNullOrWhiteSpace(accountName))
                filterTable.AddCell(new PdfPCell(new Phrase("Account Name:         " + accountName, font)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            return filterTable;
        }

        public async Task<PdfPTable> AddHeaderAsync(string reportName, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = 1 });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData.Address, fontArial10)) { Border = 0, HorizontalAlignment = 1 });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = 1 });
            headerPage.AddCell(new PdfPCell(new Phrase(reportName, fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 4f, HorizontalAlignment = 1 });

            return headerPage;
        }

        public PdfPTable AddFilteringByDateTable(Font fontArial9, DateTime? fromDate, DateTime? toDate)
        {
            PdfPTable filterTable = new(1);
            float[] widthsCellFilterTable = new float[] { 100f };
            filterTable.SetWidths(widthsCellFilterTable);
            filterTable.WidthPercentage = 100;
            filterTable.SpacingAfter = 3;

            if (fromDate.HasValue && toDate.HasValue)
            {
                filterTable.AddCell(new PdfPCell(new Phrase("Date: " + fromDate.Value.ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 4f, HorizontalAlignment = 0 });
            }

            return filterTable;
        }

        public PdfPTable AddPrimarySubHeader(Font fontArial10, SalesAndCollectionRequestModel request)
        {
            PdfPTable subHeaderTable = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            subHeaderTable.SetWidths(widthsCellsHeaderPage);
            subHeaderTable.WidthPercentage = 100;
            subHeaderTable.SpacingAfter = 3;
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Date :                      " + request.FromDate.Value.ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.ToString("dd/MM/yyyy"), fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value)?.FullName;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Marketing Officer :  " + marketingOfficerName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerRegionId.HasValue)
            {
                var customerRegionName = _unitOfWork.Repository<Region>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerRegionId)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Zone :                     " + customerRegionName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerZoneId.HasValue)
            {
                var customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Region :                  " + customerZoneName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerAreaId.HasValue)
            {
                var customerAreaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerAreaId)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Area :                      " + customerAreaName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerTerritoryId.HasValue)
            {
                var customerTerritoryName = _unitOfWork.Repository<Territory>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerTerritoryId)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Territory :                " + customerTerritoryName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Customer :              " + customerName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            return subHeaderTable;
        }

        public PdfPTable AddMarketingOfficerYearlySalesHeader(Font font, MarketingOfficerYearlySalesAndCollectionRequestModel request)
        {
            PdfPTable subHeaderTable = new(1)
            {
                WidthPercentage = 100,
                SpacingAfter = 3
            };

            subHeaderTable.SetWidths(new float[] { 100f });

            // Year info
            subHeaderTable.AddCell(new PdfPCell(new Phrase($"Year: {request.Year}", font))
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            // Marketing Officer info
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                var officer = _unitOfWork.Repository<Employee>()
                    .TableNoTracking()
                    .FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value);

                if (officer != null)
                {
                    subHeaderTable.AddCell(new PdfPCell(new Phrase($"Marketing Officer: {officer.FullName}", font))
                    {
                        Border = 0,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    });
                }
            }

            return subHeaderTable;
        }


        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportToPdfAsync<T>(int numberOfColumns, MemoryStream stream, List<TableHeader> headers, List<float> columnWidths, List<T> tableData, string reportTitleName, DateTime? fromDate = null, DateTime? toDate = null, List<Func<T, decimal>>? sumProperties = null, bool isLandscape = false)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, isLandscape);
            //fonts
            Font tableHeaderFont = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font tableDataFont = FontFactory.GetFont("Arial", 8);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(reportTitleName, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(AddFilteringTable(tableHeaderFont, fromDate?.AddDays(1), toDate, "", ""));
            // Add the table
            var dataTable = pdfGenerator.AddTable(numberOfColumns, tableData, headers, columnWidths, tableHeaderFont, tableDataFont, sumProperties);

            // Close the PDF document
            pdfGenerator.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable AddSupCusTransactionLedgerFooter(Font fontArial9Bold) //Sup = Supplier, Cus = Customer
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
            tableUsers.AddCell(new PdfPCell(new Phrase("Received by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            return tableUsers;
        }

        public PdfPTable AddCashBookFooter(Font fontArial9Bold)
        {
            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 40.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
            //var line = CreateSmallLineSeparator();
            PdfPTable tableUsers = new(3);
            float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
            tableUsers.WidthPercentage = 100;
            tableUsers.SetWidths(widthsCellsTableUsers);
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            return tableUsers;
        }

        protected virtual async Task PrintReportHeaderAsync(PdfWriter pdfWriter, string reportHeader, Font headerTitleFont, Font addressFont, Font reportTitleFont, Document doc, DateTime? fromDate, DateTime? toDatde)
        {
            Guid? tenantId = _workContext.GetTenantId();
            TenantViewModel tenantData = await _tenantService.GetByIdAsync(tenantId);

            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 10;
            font.SetStyle(Font.NORMAL);
            font.SetStyle(Font.BOLD);
            font.Color = BaseColor.Black;

            //header
            var numColumns = 2;
            var headerTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 70,30 } }
            };

            headerTable.SetWidths(widths[numColumns]);
            //SL
            //var company = _context.Companies.First();

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(tenantData.Logo!)));//company.LogoPath
            logo.ScaleToFit(100f, 500f);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 5f;
            logo.SpacingAfter = 2f;
            //Item

            headerTable.AddCell(new PdfPCell(logo) { Colspan = 2, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase(tenantData!.Name, headerTitleFont)) { Colspan = 2, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase(tenantData?.Address, addressFont)) { Colspan = 2, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email :" + tenantData?.Email, addressFont)) { Colspan = 2, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase(reportHeader, reportTitleFont)) { Colspan = 2, Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (fromDate.HasValue && toDatde.HasValue)
            {
                headerTable.AddCell(new PdfPCell(new Phrase("From: " + fromDate.Value.ToString("dd/MM/yyyy"), font)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, PaddingBottom = 5f });
                headerTable.AddCell(new PdfPCell(new Phrase("To: " + toDatde.Value.ToString("dd/MM/yyyy"), font)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, PaddingBottom = 5f });
            }

            headerTable.AddCell(new PdfPCell(new Phrase("", font)) { Colspan = 2, Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT, PaddingBottom = 5f });


            doc.Add(headerTable);
        }

        public async Task PrintBalanceSheetReportToPdfAsync(MemoryStream stream, (IList<AccountHeadWithBalanceViewModel> nonCurrentAssetHeads, IList<AccountHeadWithBalanceViewModel> currentAssetHeads, IList<AccountHeadWithBalanceViewModel> nonCurrentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> currentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> ownersEquityHeads, IList<AccountHeadWithBalanceViewModel> othersEquityHeads, RootAccountBalance rootAccount) result, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            var pageSize = PageSize.A4;
            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            await PrintReportHeaderAsync(pdfWriter, reportName, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintBalanceSheetReport(reportTitleFont, doc, result, defaultFont, attributesFont);


            doc.Close();
        }

        protected virtual void PrintBalanceSheetReport(Font titleFont, Document doc, (IList<AccountHeadWithBalanceViewModel> nonCurrentAssetHeads, IList<AccountHeadWithBalanceViewModel> currentAssetHeads, IList<AccountHeadWithBalanceViewModel> nonCurrentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> currentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> ownersEquityHeads, IList<AccountHeadWithBalanceViewModel> othersEquityHeads, RootAccountBalance rootAccount) result, Font font, Font attributesFont, decimal netIncome = 0)
        {
            var numColumns = 6;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] {25,15,15,15,15,15 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline
            //  purchaseReportTable.HeaderRows = 1;

            //non Current Asset
            var cellPurchaseReportItem = GetPdfCell("Non-Current Asset", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell(result.rootAccount.NonCurrentAsset.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in result.nonCurrentAssetHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            //Current Asset
            cellPurchaseReportItem = GetPdfCell("Current Asset", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell(result.rootAccount.CurrentAsset.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            foreach (var pl in result.currentAssetHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            cellPurchaseReportItem = GetPdfCell("Total Asset", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell((result.rootAccount.NonCurrentAsset + result.rootAccount.CurrentAsset).ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Non Current Liability
            cellPurchaseReportItem = GetPdfCell("Non-Current Liability", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell(result.rootAccount.NonCurrentLiability.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            foreach (var pl in result.nonCurrentLiabilityHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            //Current Liability
            cellPurchaseReportItem = GetPdfCell("Current Liability", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell(result.rootAccount.CurrentLiability.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            foreach (var pl in result.currentLiabilityHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            //Owners Equity
            cellPurchaseReportItem = GetPdfCell("Owners Equity", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell(result.rootAccount.OwnersEquity.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            foreach (var pl in result.ownersEquityHeads)
            {
                if (pl.Id == AccountHeadConstants.RetainedEarnings.ToGuid() || pl.Id == AccountHeadConstants.CurrentYearsProfitLoss.ToGuid())
                {
                    pl.Balance = pl.Balance + result.rootAccount.PL;
                }

                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            //Owners Equity
            cellPurchaseReportItem = GetPdfCell("Others Equity", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell(result.rootAccount.OthersEquity.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            foreach (var pl in result.othersEquityHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }

            cellPurchaseReportItem = GetPdfCell("Total Liability & Equity", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell((result.rootAccount.NonCurrentLiability + result.rootAccount.CurrentLiability + result.rootAccount.OwnersEquity + result.rootAccount.OthersEquity).ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            doc.Add(purchaseReportTable);
        }

        public async Task PrintIncomeStatementReportToPdfAsync(MemoryStream stream, (IList<AccountHeadWithBalanceViewModel> revenueHeads, IList<AccountHeadWithBalanceViewModel> cogsHeads, IList<AccountHeadWithBalanceViewModel> operatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> otherIncomeHeads, IList<AccountHeadWithBalanceViewModel> nonOperatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> csrFundHeads, IList<AccountHeadWithBalanceViewModel> taxHeads, RootAccountBalance rootAccount) isHeads, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            var pageSize = PageSize.A4;
            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 10;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);


            //headers
            await PrintReportHeaderAsync(pdfWriter, reportName, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            //  PrintIncomeStatementReport(reportTitleFont, doc, isHeads, defaultFont, attributesFont);
            PrintIncomeStatementReport2(reportTitleFont, doc, isHeads, defaultFont, attributesFont);


            doc.Close();
        }

        private void PrintIncomeStatementReport(Font titleFont, Document doc, (IList<AccountHeadWithBalanceViewModel> revenueHeads, IList<AccountHeadWithBalanceViewModel> cogsHeads, IList<AccountHeadWithBalanceViewModel> operatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> otherIncomeHeads, IList<AccountHeadWithBalanceViewModel> nonOperatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> csrFundHeads, IList<AccountHeadWithBalanceViewModel> taxHeads, RootAccountBalance rootAccount) isHeads, Font font, Font attributesFont)
        {
            var numColumns = 6;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] {25,15,15,15,15,15 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline


            //  purchaseReportTable.HeaderRows = 1;


            //revenue
            var cellPurchaseReportItem = GetPdfCell("Revenue", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.Revenue, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.revenueHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString(), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //cogs
            cellPurchaseReportItem = GetPdfCell("Less Cost of Goods Sold", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.Cogs, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.cogsHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString(), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            cellPurchaseReportItem = GetPdfCell("Gross Profit", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.Revenue - isHeads.rootAccount.Cogs, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //operating expenses
            cellPurchaseReportItem = GetPdfCell("Less Operating Expenses", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.OperatingExpenses, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.operatingExpensesHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString(), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            cellPurchaseReportItem = GetPdfCell("Operating Income", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.Revenue - isHeads.rootAccount.Cogs - isHeads.rootAccount.OperatingExpenses, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //non operating expenses
            cellPurchaseReportItem = GetPdfCell("Less Non Operating Expenses", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.NonOperatingExpenses, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.nonOperatingExpensesHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString(), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //other income
            cellPurchaseReportItem = GetPdfCell("Plus Other Income", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.OtherIncome, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.otherIncomeHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString(), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //Csr Fund
            cellPurchaseReportItem = GetPdfCell("Less Contribution to CSR Fund", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.CsrFund, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.csrFundHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString(), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //tax
            cellPurchaseReportItem = GetPdfCell("Less Tax", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.Tax, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.taxHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString(), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //..................total..................................
            cellPurchaseReportItem = GetPdfCell("Net Income/Loss", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.PL, titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);




            doc.Add(purchaseReportTable);
        }

        private void PrintIncomeStatementReport2(Font titleFont, Document doc, (IList<AccountHeadWithBalanceViewModel> revenueHeads, IList<AccountHeadWithBalanceViewModel> cogsHeads, IList<AccountHeadWithBalanceViewModel> operatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> otherIncomeHeads, IList<AccountHeadWithBalanceViewModel> nonOperatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> csrFundHeads, IList<AccountHeadWithBalanceViewModel> taxHeads, RootAccountBalance rootAccount) isHeads, Font font, Font attributesFont)
        {
            var numColumns = 6;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] {25,15,15,15,15,15 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline


            //  purchaseReportTable.HeaderRows = 1;


            //revenue
            var cellPurchaseReportItem = GetPdfCell("Revenue", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.Revenue.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.revenueHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : Math.Abs(pl.Balance).ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //cogs
            cellPurchaseReportItem = GetPdfCell("Cost of Goods Sold", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.Cogs.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.cogsHeads)
            {
                if (pl.Id == AccountHeadConstants.FactoryOverhead_Standard.ToGuid()
                    || pl.Id == AccountHeadConstants.DirectExpenses_Standard.ToGuid()) continue;
                if (pl.Id == AccountHeadConstants.COGS_Standard.ToGuid())
                {
                    pl.Name = "Cost of Raw Material";
                    pl.Balance = pl.Balance + isHeads.rootAccount.DirectExpenses_Standard + isHeads.rootAccount.FactoryOverhead_Standard;
                }
                if (pl.Id == AccountHeadConstants.DirectExpenses.ToGuid())
                    pl.Balance = pl.Balance - isHeads.rootAccount.DirectExpenses_Standard;
                if (pl.Id == AccountHeadConstants.FactoryOverhead.ToGuid())
                    pl.Balance = pl.Balance - isHeads.rootAccount.FactoryOverhead_Standard;
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            cellPurchaseReportItem = GetPdfCell("Gross Profit", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell((isHeads.rootAccount.Revenue - isHeads.rootAccount.Cogs).ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //operating expenses
            cellPurchaseReportItem = GetPdfCell("Operating Expenses", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.OperatingExpenses.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.operatingExpensesHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            cellPurchaseReportItem = GetPdfCell("Operating Income", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell((isHeads.rootAccount.Revenue - isHeads.rootAccount.Cogs - isHeads.rootAccount.OperatingExpenses).ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //non operating expenses
            cellPurchaseReportItem = GetPdfCell("Non Operating Expenses", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.NonOperatingExpenses.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.nonOperatingExpensesHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //other income
            cellPurchaseReportItem = GetPdfCell("Other Income", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.OtherIncome.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.otherIncomeHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //Csr Fund
            cellPurchaseReportItem = GetPdfCell("Contribution to CSR Fund", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.CsrFund.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.csrFundHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //tax
            cellPurchaseReportItem = GetPdfCell("Tax", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.Tax.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            foreach (var pl in isHeads.taxHeads)
            {
                //Name
                cellPurchaseReportItem = GetPdfCell(pl.Code + "-" + pl.Name, font);
                cellPurchaseReportItem.Colspan = 4;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //balance
                cellPurchaseReportItem = GetPdfCell(pl.Balance == 0M ? "" : pl.Balance.ToString("#,##0.00"), font);
                cellPurchaseReportItem.Colspan = 2;
                cellPurchaseReportItem.BorderColor = BaseColor.White;
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
            }
            //..................total..................................
            cellPurchaseReportItem = GetPdfCell("Net Income/Loss", titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell(isHeads.rootAccount.PL.ToString("#,##0.00"), titleFont);
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);




            doc.Add(purchaseReportTable);
        }

        public async Task PrintTrailBalanceReportToPdfAsync(MemoryStream stream, IList<TrialBalanceViewModel> trialBalanceReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (trialBalanceReportLines == null)
                throw new ArgumentNullException(nameof(trialBalanceReportLines));

            var pageSize = PageSize.A4;
            var doc = new Document(pageSize, 20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 12;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 11;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetFamily("Times New Roman");
            attributesFont.SetStyle(Font.NORMAL);
            attributesFont.Color = BaseColor.Black;
            attributesFont.Size = 9;


            //headers
            await PrintReportHeaderAsync(pdfWriter, reportName, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);

            //Profit Report
            PrintTrialBalanceReport(reportTitleFont, doc, trialBalanceReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = trialBalanceReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintTrialBalanceReport(Font titleFont, Document doc, IList<TrialBalanceViewModel> trialBalanceReportLines, Font font, Font attributesFont, int supplierId = 0)
        {
            var redColorFont = GetFont();
            redColorFont.Color = new BaseColor(113, 18, 26, 255);
            var blueColorFont = GetFont();
            blueColorFont.Color = BaseColor.Blue;
            var magentaColorFont = GetFont();
            magentaColorFont.Color = BaseColor.Magenta;
            var pinkColorFont = GetFont();
            pinkColorFont.Color = BaseColor.Pink;
            var blackColorFont = GetFont();
            blackColorFont.Color = BaseColor.Black;


            var numColumns = 8;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 10,21,11,11,12,12,12,11 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline
            //SL
            var cellPurchaseReportItem = GetPdfCell("Account", attributesFont);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("Opening", attributesFont);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //Category
            cellPurchaseReportItem = GetPdfCell("Transaction for the period", attributesFont);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Brand
            cellPurchaseReportItem = GetPdfCell("Balance", attributesFont);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //table header
            //SL
            cellPurchaseReportItem = GetPdfCell("A/C Code", attributesFont);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("A/C Name", attributesFont);

            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //Category
            cellPurchaseReportItem = GetPdfCell("Debit", attributesFont);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Brand
            cellPurchaseReportItem = GetPdfCell("Credit", attributesFont);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Stock Quantity




            cellPurchaseReportItem = GetPdfCell("Debit", attributesFont);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Credit", attributesFont);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Debit", attributesFont);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Credit", attributesFont);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            purchaseReportTable.HeaderRows = 2;
            foreach (var pl in trialBalanceReportLines)
            {


                //SL
                cellPurchaseReportItem = GetPdfCell(pl.Code, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //ItemName

                switch (pl.Level)
                {
                    case 1:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blueColorFont);
                        break;
                    case 2:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, magentaColorFont);
                        break;
                    case 3:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, redColorFont);
                        break;
                    case 4:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                    case 5:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                    default:
                        cellPurchaseReportItem = GetPdfCell(pl.Name, blackColorFont);
                        break;
                }
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.OpeningDebit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //CategoryName
                cellPurchaseReportItem = GetPdfCell(pl.OpeningCredit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //BrandName
                cellPurchaseReportItem = GetPdfCell(pl.ThisPeriodDebit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);



                cellPurchaseReportItem = GetPdfCell(pl.ThisPeriodCredit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.BalanceDebit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);


                cellPurchaseReportItem = GetPdfCell(pl.BalanceCredit.ToString("#,##0.00"), font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);




            }
            //..................total..................................
            var levelOneTrialBalanceReportLines = trialBalanceReportLines.Where(x => x.Level == 1);
            //SL
            cellPurchaseReportItem = GetPdfCell("Total", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.OpeningDebit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.OpeningCredit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.ThisPeriodDebit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.ThisPeriodCredit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.BalanceDebit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(levelOneTrialBalanceReportLines.Sum(x => x.BalanceCredit).ToString("#,##0.00"), font);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            doc.Add(purchaseReportTable);
        }

        public async Task PrintSubsidiaryLedgerReportToPdfAsync(MemoryStream stream, IList<SubsidiaryLedgerViewModel> subsidiaryLedgerReportLines, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (subsidiaryLedgerReportLines == null)
                throw new ArgumentNullException(nameof(subsidiaryLedgerReportLines));

            var pageSize = PageSize.A4;
            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 10;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 8;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 10;
            reportTitleFont.SetStyle(Font.BOLD);
            reportTitleFont.Color = BaseColor.Black;

            //defaultFont
            var defaultFont = GetFont();
            defaultFont.SetFamily("Times New Roman");
            defaultFont.SetStyle(Font.NORMAL);
            defaultFont.Color = BaseColor.Black;
            defaultFont.Size = 8;

            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.NORMAL);
            defaultFont.Size = 8;
            addressFont.Color = BaseColor.Red;

            //headers
            await PrintReportHeaderAsync(pdfWriter, reportName, headerTitleFont, addressFont, reportTitleFont, doc, fromDate, toDate);
            //Line
            //  DrawHorizontalLineProtrait(pdfWriter);
            //Profit Report
            PrintSubsidiaryLedgerReport(reportTitleFont, doc, subsidiaryLedgerReportLines, defaultFont, attributesFont);

            var stockReportLineNumber = 20;
            var stockReportLineNumberCount = subsidiaryLedgerReportLines.Count;
            stockReportLineNumber++;

            if (stockReportLineNumber <= stockReportLineNumberCount)
            {
                doc.NewPage();
            }

            doc.Close();
        }

        protected virtual void PrintSubsidiaryLedgerReport(Font titleFont, Document doc, IList<SubsidiaryLedgerViewModel> subsidiaryLedgerReportLines, Font font, Font attributesFont, int supplierId = 0)
        {
            var redColorFont = GetFont();
            redColorFont.Color = new BaseColor(113, 18, 26, 255);
            var blueColorFont = GetFont();
            blueColorFont.Color = BaseColor.Blue;
            var magentaColorFont = GetFont();
            magentaColorFont.Color = BaseColor.Magenta;
            var pinkColorFont = GetFont();
            pinkColorFont.Color = BaseColor.Pink;
            var blackColorFont = GetFont();
            blackColorFont.Color = BaseColor.Black;


            var numColumns = 7;
            var purchaseReportTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] {17,13,17,17,12,12,12 } }
            };

            purchaseReportTable.SetWidths(widths[numColumns]);

            //headline
            //SL
            var cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 1;
            cellPurchaseReportItem.BorderColor = BaseColor.White;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 3;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Category
            cellPurchaseReportItem = GetPdfCell("Transaction for the period", font);
            cellPurchaseReportItem.Colspan = 2;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Brand
            cellPurchaseReportItem = GetPdfCell("", font);
            cellPurchaseReportItem.Colspan = 1;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //table header
            //SL
            cellPurchaseReportItem = GetPdfCell("VNumber", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Item
            cellPurchaseReportItem = GetPdfCell("Date", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //Item
            cellPurchaseReportItem = GetPdfCell("Account", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //Category
            cellPurchaseReportItem = GetPdfCell("Particular", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Debit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Credit", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            cellPurchaseReportItem = GetPdfCell("Balance", font);
            cellPurchaseReportItem.BackgroundColor = BaseColor.LightGray;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            purchaseReportTable.HeaderRows = 1;
            foreach (var pl in subsidiaryLedgerReportLines)
            {
                //SL
                cellPurchaseReportItem = GetPdfCell(pl.Vnumber, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //ItemName
                cellPurchaseReportItem = GetPdfCell(pl.VoucherDate, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //CategoryName
                cellPurchaseReportItem = GetPdfCell(pl.AccountName, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);
                //CategoryName
                cellPurchaseReportItem = GetPdfCell(pl.Particular, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                //BrandName
                cellPurchaseReportItem = GetPdfCell(pl.Debit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                cellPurchaseReportItem = GetPdfCell(pl.Credit, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

                cellPurchaseReportItem = GetPdfCell(pl.Balance, font);
                cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseReportTable.AddCell(cellPurchaseReportItem);

            }
            //..................total..................................
            var totalDebit = subsidiaryLedgerReportLines.Sum(x => x.Debit);
            var totalCredit = subsidiaryLedgerReportLines.Sum(x => x.Credit);

            //SL
            cellPurchaseReportItem = GetPdfCell("Total Transaction", redColorFont);
            cellPurchaseReportItem.Colspan = 4;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);


            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(totalDebit, redColorFont);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(totalCredit, redColorFont);
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);
            //CurrentStock
            cellPurchaseReportItem = GetPdfCell(subsidiaryLedgerReportLines.Last().Balance, redColorFont);
            cellPurchaseReportItem.Colspan = 1;
            cellPurchaseReportItem.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseReportTable.AddCell(cellPurchaseReportItem);

            doc.Add(purchaseReportTable);
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSupplierTransactionLedgerReportToPdfAsync(MemoryStream stream, List<SupplierTransactionLedgerViewModel> result, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportName, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringTable(fontArial9, fromDate, toDate, "", "");
            var dataTable = SupplierTransactionReportDataTable(result, fontArial9, fontArial8, fontArial8Bold);
            var spaceTable = SpaceTable(fontArial9);
            var supCusTransactionLedgerFooter = AddSupCusTransactionLedgerFooter(fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(supCusTransactionLedgerFooter);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable SupplierTransactionReportDataTable(List<SupplierTransactionLedgerViewModel> result, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            PdfPTable supplierTransactionReportData = new(10);
            float[] widthsCellsSupplierTransactionReportData = new float[] { 4f, 8f, 29f, 9f, 9f, 9f, 9f, 9f, 9f, 9f };
            supplierTransactionReportData.SetWidths(widthsCellsSupplierTransactionReportData);
            supplierTransactionReportData.WidthPercentage = 100;
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { Rowspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Supplier", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Previous Due", fontArial9)) { Rowspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Transaction", fontArial9)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Name", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Net Purchase", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Payment", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Due", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Advance", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Qty", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Paid", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            var sl = 0;
            var due = 0m;
            var advance = 0m;
            supplierTransactionReportData.HeaderRows = 3;
            foreach (var item in result)
            {
                sl++;
                if (item.Due > 0) due += item.Due;
                else advance += item.Due;
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.OpeningDue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodPurchaseQty.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodPurchaseValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodPayment.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodAdjustment.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Due > 0m ? item.Due.ToString("#,##0.00") : "0.00", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Due < 0m ? item.Due.ToString("#,##0.00") : "0.00", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }

            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.OpeningDue).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodPurchaseQty).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodPurchaseValue).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodPayment).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(due.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            supplierTransactionReportData.AddCell(new PdfPCell(new Phrase(advance.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            return supplierTransactionReportData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerTransactionLedgerReportToPdfAsync(MemoryStream stream, List<CustomerTransactionLedgerViewModel> result, string reportName, CustomerTransactionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportName, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddCustomerTransactionFilteringTable(fontArial9, request);
            var dataTable = CustomerTransactionReportDataTable(result, fontArial9Bold, fontArial8, fontArial8Bold);
            var spaceTable = SpaceTable(fontArial9);
            var supCusTransactionLedgerFooter = AddSupCusTransactionLedgerFooter(fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(supCusTransactionLedgerFooter);

            document.Close();

            return (filterTable, dataTable);
        }

        public PdfPTable CustomerTransactionReportDataTable(List<CustomerTransactionLedgerViewModel> result, Font fontArial9Bold, Font fontArial8, Font fontArial8Bold)
        {
            var marketingOfficerData = result.GroupBy(x => new { x.CustomerMarketingOfficerName, x.CustomerMarketingOfficerId }).OrderBy(x => x.Key.CustomerMarketingOfficerName).ToList();
            PdfPTable customerTransactionReportData = new(11);
            float[] widthsCellsHeaderPage = new float[] { 3f, 7f, 24f, 9f, 8f, 9f, 9f, 8f, 9f, 9f, 9f };
            customerTransactionReportData.SetWidths(widthsCellsHeaderPage);
            customerTransactionReportData.WidthPercentage = 100;
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("SL", fontArial9Bold)) { Rowspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Customer", fontArial9Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Previous Due", fontArial9Bold)) { Rowspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Transaction", fontArial9Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Credit Limit", fontArial9Bold)) { Rowspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Code", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Name", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Net Sales", fontArial9Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Collection", fontArial9Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Due", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Advance", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Qty", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Received", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            var sl = 0;
            var due = 0m;
            var advance = 0m;
            customerTransactionReportData.HeaderRows = 3;
            foreach (var marketingOfficer in marketingOfficerData)
            {
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Marketing Officer: " + marketingOfficer.Key.CustomerMarketingOfficerName, fontArial8Bold)) { Colspan = 11, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                var marketingOfficerDue = 0m;
                var marketingOfficerAdvance = 0m;
                foreach (var item in marketingOfficer)
                {
                    sl++;
                    if (item.Due > 0) marketingOfficerDue += item.Due;
                    else marketingOfficerAdvance += item.Due;
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.OpeningDue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodSaleQty.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodSaleValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodCollection.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodAdjustment.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Due > 0m ? item.Due.ToString("#,##0.00") : "0.00", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Due < 0m ? item.Due.ToString("#,##0.00") : "0.00", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerTransactionReportData.AddCell(new PdfPCell(new Phrase(item.CreditLimit.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                due += marketingOfficerDue;
                advance += marketingOfficerAdvance;
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficer.Key.CustomerMarketingOfficerName + ", Total : ", fontArial8Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.OpeningDue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodSaleQty).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficerDue.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficerAdvance.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerTransactionReportData.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.CreditLimit).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }

            customerTransactionReportData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.OpeningDue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleQty).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase(due.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase(advance.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.CreditLimit).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            return customerTransactionReportData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintDailyTransactionLedgerReportToPdfAsync(MemoryStream stream, List<CashBankTransactionLedgerViewModel> result, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportName, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringTable(fontArial9, fromDate, toDate, "", "");
            var dataTable = DailyTransactionReportDataTable(result, fontArial9, fontArial8, fontArial8Bold);
            var spaceTable = SpaceTable(fontArial9);
            var supCusTransactionLedgerFooter = AddSupCusTransactionLedgerFooter(fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(supCusTransactionLedgerFooter);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable DailyTransactionReportDataTable(List<CashBankTransactionLedgerViewModel> result, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            PdfPTable dailyTransactionReportData = new(7);
            float[] widthsCellsHeaderPage = new float[] { 5f, 9f, 30f, 14f, 14f, 14f, 14f };
            dailyTransactionReportData.SetWidths(widthsCellsHeaderPage);
            dailyTransactionReportData.WidthPercentage = 100;
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Cash / Bank", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Opening", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Current Transaction", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Closing", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Receipt", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Payment", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            var sl = 0;
            dailyTransactionReportData.HeaderRows = 2;
            foreach (var item in result)
            {
                sl++;
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.OpeningBalance.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodReceipt.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodPayment.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }

            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodReceipt).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodPayment).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.Balance).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return dailyTransactionReportData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCashBankBalanceReportToPdfAsync(MemoryStream stream, List<CashBankTransactionLedgerViewModel> result, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportName, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringTable(fontArial9, fromDate, toDate, "", "");
            var dataTable = CashBankBalanceReportDataTable(result, fontArial9, fontArial7, fontArial7Bold);
            var spaceTable = SpaceTable(fontArial9);
            var supCusTransactionLedgerFooter = AddSupCusTransactionLedgerFooter(fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(supCusTransactionLedgerFooter);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable CashBankBalanceReportDataTable(List<CashBankTransactionLedgerViewModel> result, Font fontArial9, Font fontArial7, Font fontArial7Bold)
        {
            PdfPTable dailyTransactionReportData = new(9);
            float[] widthsCellsHeaderPage = new float[] { 5f, 10f, 50f, 15f, 15f, 15f, 15f, 15f, 15f };
            dailyTransactionReportData.SetWidths(widthsCellsHeaderPage);
            dailyTransactionReportData.WidthPercentage = 100;
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Code", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Cash / Bank", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Opening Balance", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Current Transaction", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Closing Balance", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Fund Transfer (Receipt)", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Receipt", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Fund Transfer (Payment)", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Payment", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });

            // filter out rows where all numeric columns are zero
            var visibleRows = result.Where(item =>
                item != null && (
                   item.OpeningBalance != 0m
                || item.ThisPeriodFundTransferReceipt != 0m
                || item.ThisPeriodReceipt != 0m
                || item.ThisPeriodFundTransferPayment != 0m
                || item.ThisPeriodFundTransferCharge != 0m
                || item.ThisPeriodPayment != 0m
                || item.Balance != 0m
            )).ToList();

            var sl = 0;
            dailyTransactionReportData.HeaderRows = 2;
            foreach (var item in visibleRows)
            {
                sl++;
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.OpeningBalance.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.ThisPeriodFundTransferReceipt.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase((item.ThisPeriodReceipt - item.ThisPeriodFundTransferReceipt).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase((item.ThisPeriodFundTransferPayment - item.ThisPeriodFundTransferCharge).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase((item.ThisPeriodPayment - item.ThisPeriodFundTransferPayment + item.ThisPeriodFundTransferCharge).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }

            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial7)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodFundTransferReceipt).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodReceipt - x.ThisPeriodFundTransferReceipt).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodFundTransferPayment - x.ThisPeriodFundTransferCharge).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodPayment - x.ThisPeriodFundTransferPayment + x.ThisPeriodFundTransferCharge).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionReportData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.Balance).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return dailyTransactionReportData;
        }


        public async Task PrintDailyTransactionDetailLedgerReportToPdfAsync(MemoryStream stream, List<CashBankTransactionDetailLedgerViewModel> result, string reportName, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportName, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringTable(fontArial9, fromDate, toDate, "", "");
            var dailyTransactionDetailReportDataTable = DailyTransactionDetailReportDataTable(result, fontArial9, fontArial8, fontArial8Bold);
            var spaceTable = SpaceTable(fontArial9);
            var supCusTransactionLedgerFooter = AddSupCusTransactionLedgerFooter(fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dailyTransactionDetailReportDataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(supCusTransactionLedgerFooter);

            document.Close();
        }

        public PdfPTable DailyTransactionDetailReportDataTable(List<CashBankTransactionDetailLedgerViewModel> result, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            PdfPTable dailyTransactionDetailData = new(7);
            float[] widthsCellsHeaderPage = new float[] { 2f, 7f, 12f, 8f, 31f, 31f, 8f };
            dailyTransactionDetailData.SetWidths(widthsCellsHeaderPage);
            dailyTransactionDetailData.WidthPercentage = 100;
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Cash / Bank", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Opening", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Current Transaction", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Closing", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Receipt", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Payment", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            var sl = 0;
            dailyTransactionDetailData.HeaderRows = 2;
            foreach (var item in result)
            {
                sl++;
                dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.OpeningBalance.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (item.ReceiptDetails is not null && item.ReceiptDetails.Any())
                {
                    PdfPTable receiptDetailsTable = new(5);
                    float[] widthsCellsPage = new float[] { 7f, 20f, 18f, 37f, 18f };
                    receiptDetailsTable.SetWidths(widthsCellsPage);
                    receiptDetailsTable.WidthPercentage = 100;
                    receiptDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    receiptDetailsTable.AddCell(new PdfPCell(new Phrase("Voucher No", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    receiptDetailsTable.AddCell(new PdfPCell(new Phrase("Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    receiptDetailsTable.AddCell(new PdfPCell(new Phrase("Particular", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    receiptDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item.ReceiptDetails)
                    {
                        miniSL++;
                        receiptDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        receiptDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem.Vnumber, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        receiptDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem.Date.ToString("dd/MM/yyyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        receiptDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem.Particular, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        receiptDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem.Amount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    receiptDetailsTable.AddCell(new PdfPCell(new Phrase("Total:", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    receiptDetailsTable.AddCell(new PdfPCell(new Phrase(item.ThisPeriodReceipt.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    var receiptDetailsCell = GetPdfCell("", fontArial8);
                    receiptDetailsCell.AddElement(receiptDetailsTable);
                    dailyTransactionDetailData.AddCell(receiptDetailsCell);

                }
                else
                    dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("-", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

                if (item.PaymentDetails is not null && item.PaymentDetails.Any())
                {
                    PdfPTable paymentDetailsTable = new(5);
                    float[] widthsCellsPage = new float[] { 7f, 20f, 18f, 37f, 18f };
                    paymentDetailsTable.SetWidths(widthsCellsPage);
                    paymentDetailsTable.WidthPercentage = 100;
                    paymentDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    paymentDetailsTable.AddCell(new PdfPCell(new Phrase("Voucher No", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    paymentDetailsTable.AddCell(new PdfPCell(new Phrase("Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    paymentDetailsTable.AddCell(new PdfPCell(new Phrase("Particular", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    paymentDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item.PaymentDetails)
                    {
                        miniSL++;
                        paymentDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        paymentDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem.Vnumber, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        paymentDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem.Date.ToString("dd/MM/yyyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        paymentDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem.Particular, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        paymentDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem.Amount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    paymentDetailsTable.AddCell(new PdfPCell(new Phrase("Total:", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    paymentDetailsTable.AddCell(new PdfPCell(new Phrase(item.ThisPeriodPayment.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    var paymentDetailsCell = GetPdfCell("", fontArial8);
                    paymentDetailsCell.AddElement(paymentDetailsTable);
                    dailyTransactionDetailData.AddCell(paymentDetailsCell);

                }
                else
                    dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("-", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Balance.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }

            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodReceipt).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodPayment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            dailyTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.Balance).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return dailyTransactionDetailData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCashBookTransactionDetailLedgerReportToPdfAsync(MemoryStream stream, CashBookReportViewModel result, string reportTitle, CashBookReportRequestModel requestModel)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var account = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == requestModel.CashBookAccountId);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringTable(fontArial8Bold, null, null, account?.Code, account?.Name);
            var dataTable = CashBookTransactionDetailReportDataTable(result, requestModel.FromDate, requestModel.ToDate, fontArial11Bold, fontArial7, fontArial8Bold);
            var spaceTable = SpaceTable(fontArial9);
            var addCashBookFooter = AddCashBookFooter(fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(addCashBookFooter);

            document.Close();

            return (filterTable, dataTable);
        }

        public PdfPTable CashBookTransactionDetailReportDataTable(CashBookReportViewModel result, DateTime? fromDate, DateTime? toDate, Font fontArial11Bold, Font fontArial7, Font fontArial8Bold)
        {
            PdfPTable cashBookTransactionDetailData = new(12);
            float[] widthsCellsHeaderPage = new float[] { 17f, 10f, 12f, 25f, 23f, 13f, 17f, 10f, 12f, 25f, 23f, 13f };
            cashBookTransactionDetailData.SetWidths(widthsCellsHeaderPage);
            cashBookTransactionDetailData.WidthPercentage = 100;
            cashBookTransactionDetailData.HeaderRows = 4;
            if (fromDate.HasValue && toDate.HasValue)
            {
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("For the period of " + fromDate.Value.ToString("dd-MMM-yyyy") + " to " + toDate.Value.ToString("dd-MMM-yyyy"), fontArial11Bold)) { Colspan = 12, Border = 0, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            }
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Debit", fontArial11Bold)) { Colspan = 6, Border = 0, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Credit", fontArial11Bold)) { Colspan = 6, Border = 0, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("RECEIPTS (TK)", fontArial11Bold)) { Colspan = 6, BackgroundColor = BaseColor.LightGray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("PAYMENTS (TK)", fontArial11Bold)) { Colspan = 6, BackgroundColor = BaseColor.LightGray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            if (result.CombinedReceiptsAndPayments is not null && result.CombinedReceiptsAndPayments.Any())
            {
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Voucher No", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Cost Center", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Head of Account / Code No", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Particulars", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Taka", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Voucher No", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Cost Center", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Head of Account / Code No", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Particulars", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Taka", fontArial8Bold)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                foreach (var item in result.CombinedReceiptsAndPayments)
                {
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Vnumber1, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Date1, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.CostCenterName1, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.HeadOfAccountName1, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Particular1, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Amount1, fontArial7)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Vnumber2, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Date2, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.CostCenterName2, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.HeadOfAccountName2, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Particular2, fontArial7)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
                    cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(item.Amount2, fontArial7)) { BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                }
            }
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Total Receipt During the Period: ", fontArial8Bold)) { Colspan = 4, BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.ThisPeriodReceipt.ToString("#,##0.00"), fontArial8Bold)) { Colspan = 2, BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Total Payment During the Period: ", fontArial8Bold)) { Colspan = 4, BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.ThisPeriodPayment.ToString("#,##0.00"), fontArial8Bold)) { Colspan = 2, BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Total (OB+TRDM): ", fontArial8Bold)) { Colspan = 4, PaddingTop = 2, BackgroundColor = BaseColor.LightGray, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.ThisPeriodOB_TRDM.ToString("#,##0.00"), fontArial8Bold)) { Colspan = 2, BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Total (OB+TRDM): ", fontArial8Bold)) { Colspan = 4, BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.ThisPeriodOB_TRDM.ToString("#,##0.00"), fontArial8Bold)) { Colspan = 2, BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase("Closing Balance: ", fontArial8Bold)) { Colspan = 10, PaddingTop = 2, BackgroundColor = BaseColor.LightGray, PaddingBottom = 2, HorizontalAlignment = 2 });
            cashBookTransactionDetailData.AddCell(new PdfPCell(new Phrase(result.Balance.ToString("#,##0.00"), fontArial8Bold)) { Colspan = 2, BackgroundColor = BaseColor.LightGray, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });

            return cashBookTransactionDetailData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCogsCalculationReportToPdfAsync(MemoryStream stream, CogsViewModel result, string headerText, CogsCalculationRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // Fonts
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringByDateTable(fontArial9, request.FromDate, request.ToDate);
            var dataTable = CogsCalculationDataTable(result, fontArial9, fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();

            return (filterTable, dataTable);
        }

        public PdfPTable CogsCalculationDataTable(CogsViewModel result, Font fontArial9, Font fontArial9Bold)
        {
            PdfPTable cogsCalculationTable = new(4);
            float[] widthsCellsCogsCalculationTable = new float[] { 18f, 22f, 30f, 30f };
            cogsCalculationTable.SetWidths(widthsCellsCogsCalculationTable);
            cogsCalculationTable.WidthPercentage = 100;

            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Particular", fontArial9Bold)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial9Bold)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Opening Raw Materials", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.RawMaterialsOpeningValue.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Closing Raw Materials", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.RawMaterialsClosingValue.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Opening Finished Goods", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.FinishedGoodsOpeningValue.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Closing Finished Goods", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.FinishedGoodsClosingValue.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Purchase", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.TotalCreditInPurchaseClearingAccount.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Work In Progress Inventory", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.WorkInProgressInventory.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Purchase Price Variance (Adjustment)", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.PurchasePriceVarianceDuringThisPeriod.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("LC Cost", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.TotalCreditInLcCostClearingAccount.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Purchase Return", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.BalanceInPurchaseReturnClearingAccount.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Transportation Cost", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.TransportationCost.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Purchase Discount", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.TotalCreditInPurchaseDiscount.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Gain Loss Inventory Variance", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.GainLossInventoryVariance.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Factory Overhead", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.FactoryOverhead.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Direct Expenses", fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.DirectExpenses.ToString("#,##0.00"), fontArial9)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });

            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });

            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Net Purchase            = ", fontArial9Bold)) { Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Purchase + Transportation - Discount + Adjustment + Gain Loss Inventory - Purchase Return + LC Cost", fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });

            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("                                   = ", fontArial9)) { Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.TotalCreditInPurchaseClearingAccount.ToString("#,##0.00") + " + " + result.TransportationCost.ToString("#,##0.00") + " - " + result.TotalCreditInPurchaseDiscount.ToString("#,##0.00") + " + " + result.PurchasePriceVarianceDuringThisPeriod.ToString("#,##0.00") + " + " + result.GainLossInventoryVariance.ToString("#,##0.00") + " - " + result.BalanceInPurchaseReturnClearingAccount.ToString("#,##0.00") + " + " + result.TotalCreditInLcCostClearingAccount.ToString("#,##0.00"), fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });

            var netPurchase = result.TotalCreditInPurchaseClearingAccount + result.TransportationCost - result.TotalCreditInPurchaseDiscount + result.PurchasePriceVarianceDuringThisPeriod + result.GainLossInventoryVariance - result.BalanceInPurchaseReturnClearingAccount + result.TotalCreditInLcCostClearingAccount;
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("                                   = ", fontArial9)) { Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(netPurchase.ToString("#,##0.00"), fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });

            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });

            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Cost of Goods Sold  = ", fontArial9Bold)) { Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Opening Raw Materials + Net Purchase - Work In Progress Inventory - Closing Raw Materials + Opening Finished Goods + Factory Overhead + Direct Expenses - Closing Finished Goods", fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });

            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("                                    = ", fontArial9)) { Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.RawMaterialsOpeningValue.ToString("#,##0.00") + " + " + netPurchase.ToString("#,##0.00") + " - " + result.WorkInProgressInventory.ToString("#,##0.00") + " - " + result.RawMaterialsClosingValue.ToString("#,##0.00") + " + " + result.FinishedGoodsOpeningValue.ToString("#,##0.00") + " + " + result.FactoryOverhead.ToString("#,##0.00") + " + " + result.DirectExpenses.ToString("#,##0.00") + " - " + result.FinishedGoodsClosingValue.ToString("#,##0.00"), fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });

            var total = result.RawMaterialsOpeningValue + netPurchase - result.WorkInProgressInventory - result.RawMaterialsClosingValue + result.FinishedGoodsOpeningValue + result.FactoryOverhead + result.DirectExpenses - result.FinishedGoodsClosingValue;
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("                                    = ", fontArial9)) { Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(total.ToString("#,##0.00"), fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase("Reference Value : ", fontArial9Bold)) { Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
            cogsCalculationTable.AddCell(new PdfPCell(new Phrase(result.Cogs.ToString("#,##0.00"), fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });

            return cogsCalculationTable;
        }

        public async Task PrintFundTransferReportToPdf(MemoryStream stream, List<FundTransferViewModel> fundTransferViewModels, string reportTitle, bool isDetails, FundTransferRequestModel request)
        {
            //ValidateParameters
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (fundTransferViewModels == null) throw new ArgumentNullException(nameof(fundTransferViewModels));

            //InitializeDocumnet
            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;

            document.Open();

            //Fonts
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            //Add header table
            var headerTable = await AddFundTransferHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var fundTransferData = FundTransferDataTable(fundTransferViewModels, isDetails, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(fundTransferData);

            document.Close();
        }

        private async Task<PdfPTable> AddFundTransferHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, FundTransferRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.CostCenterId.HasValue)
            {
                var costCenterName = _unitOfWork.Repository<CostCenter>().TableNoTracking().FirstOrDefault(x => x.Id == request.CostCenterId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Cost Center: " + costCenterName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.TransferFromAccountId.HasValue)
            {
                var transferFromAccountName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.TransferFromAccountId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Transfer From: " + transferFromAccountName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.TransferToAccountId.HasValue)
            {
                var transferToAccountName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.TransferToAccountId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Transfer To: " + transferToAccountName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });

            return headerPage;
        }

        private PdfPTable FundTransferDataTable(List<FundTransferViewModel> fundTransferViewModels, bool isDetails, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable fundTransferData = new(10);
            float[] widthCellsHeaderPage = new float[] { 5f, 9f, 6f, 15f, 8f, 19f, 18f, 8f, 9f, 6f };
            fundTransferData.SetTotalWidth(widthCellsHeaderPage);
            fundTransferData.WidthPercentage = 100;
            fundTransferData.HeaderRows = 1;

            fundTransferData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("FT NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("FT Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("Transaction Type", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("Cost Center", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("Transfer From", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("Transfer To", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("Remarks", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            fundTransferData.AddCell(new PdfPCell(new Phrase("Charges", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in fundTransferViewModels)
            {
                sl++;
                fundTransferData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.FundTransferNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.FundTransferDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.FundTransferTransactionType?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.CostCenter?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.TransferFromAccount?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.TransferToAccount?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.Remark, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.Amount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                fundTransferData.AddCell(new PdfPCell(new Phrase(item?.Charges?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            fundTransferData.AddCell(new PdfPCell(new Phrase("Total:", fontArial8Bold)) { Colspan = 8, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            fundTransferData.AddCell(new PdfPCell(new Phrase(fundTransferViewModels.Sum(x => x.Amount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            fundTransferData.AddCell(new PdfPCell(new Phrase(fundTransferViewModels.Sum(x => x.Charges)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return fundTransferData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerWiseSalesAndCollectionReportToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddPrimarySubHeader(fontArial9, request);
            var dataTable = PrimaryCustomerWiseSalesAndCollectionDataTable(result, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryCustomerWiseSalesAndCollectionDataTable(List<SalesAndCollectionViewModel> result, Font fontArial7, Font fontArial7Bold)
        {
            var regionData = result.GroupBy(x => x.RegionId).OrderBy(x => x.Key).ToList();
            PdfPTable customerWiseSalesAndCollectionDataTable = new(12);
            float[] widthsCellsCustomerWiseSalesAndCollectionDataTable = new float[] { 4f, 8f, 25f, 25f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            customerWiseSalesAndCollectionDataTable.SetWidths(widthsCellsCustomerWiseSalesAndCollectionDataTable);
            customerWiseSalesAndCollectionDataTable.WidthPercentage = 100;
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Customer", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Opening Due", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Transaction", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Balance", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Code", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Address", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Return", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Net Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Paid", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Due", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Advance", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            var sl = 0;
            var due = 0m;
            var advance = 0m;
            customerWiseSalesAndCollectionDataTable.HeaderRows = 2;
            foreach (var region in regionData)
            {
                var regionDue = 0m;
                var regionAdvance = 0m;
                var customerRegionName = _unitOfWork.Repository<Region>().TableNoTracking().FirstOrDefault(x => x.Id == region.Key)?.Name;
                var zoneData = region.GroupBy(x => x.ZoneId).OrderBy(x => x.Key).ToList();
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Zone: " + customerRegionName, fontArial7Bold)) { Colspan = 12, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
                foreach (var zone in zoneData)
                {
                    var zoneDue = 0m;
                    var zoneAdvance = 0m;
                    var customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == zone.Key)?.Name;
                    var areaData = zone.GroupBy(x => x.AreaId).OrderBy(x => x.Key).ToList();
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Region: " + customerZoneName, fontArial7Bold)) { Colspan = 12, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                    foreach (var area in areaData)
                    {
                        var areaDue = 0m;
                        var areaAdvance = 0m;
                        var customerAreaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == area.Key)?.Name;
                        var territoryData = area.GroupBy(x => x.TerritoryId).OrderBy(x => x.Key).ToList();
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Area: " + customerAreaName, fontArial7Bold)) { Colspan = 12, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                        foreach (var territory in territoryData)
                        {
                            var territoryDue = 0m;
                            var territoryAdvance = 0m;
                            var customerTerritoryName = _unitOfWork.Repository<Territory>().TableNoTracking().FirstOrDefault(x => x.Id == territory.Key)?.Name;
                            var marketingOfficerData = territory.GroupBy(x => x.CustomerMarketingOfficerId).OrderBy(x => x.Key).ToList();
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Territory: " + customerTerritoryName, fontArial7Bold)) { Colspan = 12, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                            foreach (var marketingOfficer in marketingOfficerData)
                            {
                                var marketingOfficerDue = 0m;
                                var marketingOfficerAdvance = 0m;
                                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == marketingOfficer.Key)?.FullName;
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer: " + marketingOfficerName, fontArial7Bold)) { Colspan = 12, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                                foreach (var customer in marketingOfficer)
                                {
                                    sl++;
                                    if (customer.ClosingBalance > 0) marketingOfficerDue += customer.ClosingBalance;
                                    else marketingOfficerAdvance += customer.ClosingBalance;

                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.CustomerCode, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.CustomerName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.CustomerAddress, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.OpeningBalance.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.ThisPeriodSaleValue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.ThisPeriodSaleReturnValue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase((customer.ThisPeriodSaleValue - customer.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.ThisPeriodCollection.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.ThisPeriodAdjustment.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.ClosingBalance > 0m ? customer.ClosingBalance.ToString("#,##0.00") : "0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(customer.ClosingBalance < 0m ? customer.ClosingBalance.ToString("#,##0.00") : "0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                }
                                territoryDue += marketingOfficerDue;
                                territoryAdvance += marketingOfficerAdvance;
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer: " + marketingOfficerName + ", Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase((marketingOfficer.Sum(x => x.ThisPeriodSaleValue) - marketingOfficer.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficerDue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficerAdvance.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            }
                            areaDue += territoryDue;
                            areaAdvance += territoryAdvance;
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Territory: " + customerTerritoryName + ", Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase((territory.Sum(x => x.ThisPeriodSaleValue) - territory.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(territoryDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(territoryAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        }
                        zoneDue += areaDue;
                        zoneAdvance += areaAdvance;
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Area: " + customerAreaName + ", Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase((area.Sum(x => x.ThisPeriodSaleValue) - area.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(areaDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(areaAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    regionDue += zoneDue;
                    regionAdvance += zoneAdvance;
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Region: " + customerZoneName + ", Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase((zone.Sum(x => x.ThisPeriodSaleValue) - zone.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(zoneDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(zoneAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                due += regionDue;
                advance += regionAdvance;
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Zone: " + customerRegionName + ", Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase((region.Sum(x => x.ThisPeriodSaleValue) - region.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(regionDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(regionAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase((result.Sum(x => x.ThisPeriodSaleValue) - result.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(due.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesAndCollectionDataTable.AddCell(new PdfPCell(new Phrase(advance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            return customerWiseSalesAndCollectionDataTable;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintNationalWiseSalesCollectionAndDueReportToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddPrimarySubHeader(fontArial9, request);
            var dataTable = PrimaryNationalWiseSalesCollectionAndDueDataTable(result, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryNationalWiseSalesCollectionAndDueDataTable(List<SalesAndCollectionViewModel> result, Font fontArial7, Font fontArial7Bold)
        {
            var regionData = result.GroupBy(x => x.RegionId).OrderBy(x => x.Key).ToList();
            PdfPTable nationalWiseSalesCollectionAndDueDataTable = new(10);
            float[] widthsCellsNationalWiseSalesCollectionAndDueDataTable = new float[] { 5f, 25f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            nationalWiseSalesCollectionAndDueDataTable.SetWidths(widthsCellsNationalWiseSalesCollectionAndDueDataTable);
            nationalWiseSalesCollectionAndDueDataTable.WidthPercentage = 100;
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Opening Due", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Transaction", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Balance", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Return", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Net Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Paid", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Due", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Advance", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            var sl = 0;
            var due = 0m;
            var advance = 0m;
            nationalWiseSalesCollectionAndDueDataTable.HeaderRows = 2;
            foreach (var region in regionData)
            {
                var regionDue = 0m;
                var regionAdvance = 0m;
                var customerRegionName = _unitOfWork.Repository<Region>().TableNoTracking().FirstOrDefault(x => x.Id == region.Key)?.Name;
                var zoneData = region.GroupBy(x => x.ZoneId).OrderBy(x => x.Key).ToList();
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Zone: " + customerRegionName, fontArial7Bold)) { Colspan = 10, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
                foreach (var zone in zoneData)
                {
                    var zoneDue = 0m;
                    var zoneAdvance = 0m;
                    var customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == zone.Key)?.Name;
                    var areaData = zone.GroupBy(x => x.AreaId).OrderBy(x => x.Key).ToList();
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Region: " + customerZoneName, fontArial7Bold)) { Colspan = 10, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                    foreach (var area in areaData)
                    {
                        var areaDue = 0m;
                        var areaAdvance = 0m;
                        var customerAreaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == area.Key)?.Name;
                        var territoryData = area.GroupBy(x => x.TerritoryId).OrderBy(x => x.Key).ToList();
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Area: " + customerAreaName, fontArial7Bold)) { Colspan = 10, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                        foreach (var territory in territoryData)
                        {
                            var territoryDue = 0m;
                            var territoryAdvance = 0m;
                            var customerTerritoryName = _unitOfWork.Repository<Territory>().TableNoTracking().FirstOrDefault(x => x.Id == territory.Key)?.Name;
                            var marketingOfficerData = territory.GroupBy(x => x.CustomerMarketingOfficerId).OrderBy(x => x.Key).ToList();
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Territory: " + customerTerritoryName, fontArial7Bold)) { Colspan = 10, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                            foreach (var marketingOfficer in marketingOfficerData)
                            {
                                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == marketingOfficer.Key)?.FullName;
                                sl++;
                                if (marketingOfficer.Sum(x => x.ClosingBalance) > 0) territoryDue += marketingOfficer.Sum(x => x.ClosingBalance);
                                else territoryAdvance += marketingOfficer.Sum(x => x.ClosingBalance);

                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficerName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((marketingOfficer.Sum(x => x.ThisPeriodSaleValue) - marketingOfficer.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ClosingBalance) > 0m ? marketingOfficer.Sum(x => x.ClosingBalance).ToString("#,##0.00") : "0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ClosingBalance) < 0m ? marketingOfficer.Sum(x => x.ClosingBalance).ToString("#,##0.00") : "0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            }
                            areaDue += territoryDue;
                            areaAdvance += territoryAdvance;
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Territory: " + customerTerritoryName + ", Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((territory.Sum(x => x.ThisPeriodSaleValue) - territory.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(territoryDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(territoryAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        }
                        zoneDue += areaDue;
                        zoneAdvance += areaAdvance;
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Area: " + customerAreaName + ", Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((area.Sum(x => x.ThisPeriodSaleValue) - area.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(areaDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(areaAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    regionDue += zoneDue;
                    regionAdvance += zoneAdvance;
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Region: " + customerZoneName + ", Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((zone.Sum(x => x.ThisPeriodSaleValue) - zone.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(zoneDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(zoneAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                due += regionDue;
                advance += regionAdvance;
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Zone: " + customerRegionName + ", Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((region.Sum(x => x.ThisPeriodSaleValue) - region.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((result.Sum(x => x.ThisPeriodSaleValue) - result.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(due.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(advance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            return nationalWiseSalesCollectionAndDueDataTable;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintMarketingOfficerWiseSalesCollectionAndDueReportToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddPrimarySubHeader(fontArial9, request);
            var dataTable = PrimaryMarketingOfficerWiseSalesCollectionAndDueDataTable(result, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryMarketingOfficerWiseSalesCollectionAndDueDataTable(List<SalesAndCollectionViewModel> result, Font fontArial7, Font fontArial7Bold)
        {
            PdfPTable nationalWiseSalesCollectionAndDueDataTable = new(10);
            float[] widthsCellsNationalWiseSalesCollectionAndDueDataTable = new float[] { 5f, 25f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            nationalWiseSalesCollectionAndDueDataTable.SetWidths(widthsCellsNationalWiseSalesCollectionAndDueDataTable);
            nationalWiseSalesCollectionAndDueDataTable.WidthPercentage = 100;
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Opening Due", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Transaction", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Balance", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Return", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Net Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Collection", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Due", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Advance", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            var sl = 0;
            var grandTotalDue = 0m;
            var grandTotalAdvance = 0m;
            nationalWiseSalesCollectionAndDueDataTable.HeaderRows = 2;
            var resultData = result.OrderBy(x => x.RegionId).ToList();
            var groupedByRegionData = resultData.GroupBy(x => x.RegionId).ToList();

            foreach (var regionData in groupedByRegionData)
            {
                var regionDue = 0m;
                var regionAdvance = 0m;
                var marketingOfficerData = regionData.GroupBy(x => x.CustomerMarketingOfficerId).ToList();
                var customerRegionName = _unitOfWork.Repository<Region>().TableNoTracking().FirstOrDefault(x => x.Id == regionData.Key)?.Name;

                foreach (var marketingOfficer in marketingOfficerData)
                {
                    var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == marketingOfficer.Key)?.FullName;
                    sl++;
                    if (marketingOfficer.Sum(x => x.ClosingBalance) > 0) regionDue += marketingOfficer.Sum(x => x.ClosingBalance);
                    else regionAdvance += marketingOfficer.Sum(x => x.ClosingBalance);

                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficerName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((marketingOfficer.Sum(x => x.ThisPeriodSaleValue) - marketingOfficer.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ClosingBalance) > 0m ? marketingOfficer.Sum(x => x.ClosingBalance).ToString("#,##0.00") : "0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ClosingBalance) < 0m ? marketingOfficer.Sum(x => x.ClosingBalance).ToString("#,##0.00") : "0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customerRegionName + " Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionData.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionData.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionData.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((regionData.Sum(x => x.ThisPeriodSaleValue) - regionData.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionData.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionData.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(regionAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grandTotalDue += regionDue;
                grandTotalAdvance += regionAdvance;
            }

            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.OpeningBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((result.Sum(x => x.ThisPeriodSaleValue) - result.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(grandTotalDue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(grandTotalAdvance.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            return nationalWiseSalesCollectionAndDueDataTable;
        }
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintMarketingOfficerWiseSalesCollectionAndDueDetailsReportToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddPrimarySubHeader(fontArial9, request);
            var dataTable = PrimaryMarketingOfficerWiseSalesCollectionAndDueDetailsDataTable(result, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryMarketingOfficerWiseSalesCollectionAndDueDetailsDataTable(List<SalesAndCollectionViewModel> result, Font fontArial7, Font fontArial7Bold)
        {
            PdfPTable moWiseSalesCollectionAndDueDataTable = new(11);
            float[] widthsCellsMoWiseSalesCollectionAndDueDataTable = new float[] { 5f, 15f, 35f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f };
            moWiseSalesCollectionAndDueDataTable.SetWidths(widthsCellsMoWiseSalesCollectionAndDueDataTable);
            moWiseSalesCollectionAndDueDataTable.WidthPercentage = 100;
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Customer", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Opening Due", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Transaction", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Balance", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Return", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Net Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Collection", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Due", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Advance", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            var sl = 0;
            moWiseSalesCollectionAndDueDataTable.HeaderRows = 2;
            var resultData = result.OrderBy(x => x.CustomerMarketingOfficerId).ToList();
            var groupedMarketingOfficerData = resultData.GroupBy(x => x.CustomerMarketingOfficerId).ToList();

            foreach (var marketingOfficerData in groupedMarketingOfficerData)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == marketingOfficerData.Key)?.FullName;
                foreach (var customer in marketingOfficerData)
                {
                    sl++;
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficerName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customer.CustomerName + " (" + customer.CustomerCode + ")", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customer.OpeningBalance.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customer.ThisPeriodSaleValue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customer.ThisPeriodSaleReturnValue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((customer.ThisPeriodSaleValue - customer.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customer.ThisPeriodCollection.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customer.ThisPeriodAdjustment.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customer.ClosingBalance > 0m ? customer.ClosingBalance.ToString("#,##0.00") : "0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    moWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(customer.ClosingBalance < 0m ? customer.ClosingBalance.ToString("#,##0.00") : "0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
            }   

            return moWiseSalesCollectionAndDueDataTable;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintMarketingOfficerWiseSalesCollectionAndDueReportShortToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Width, PageSize.A4.Height);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddPrimarySubHeader(fontArial9, request);
            var dataTable = PrimaryMarketingOfficerWiseSalesCollectionAndDueShortDataTable(result, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryMarketingOfficerWiseSalesCollectionAndDueShortDataTable(List<SalesAndCollectionViewModel> result, Font fontArial7, Font fontArial7Bold)
        {
            PdfPTable nationalWiseSalesCollectionAndDueDataTable = new(5);
            float[] widthsCellsNationalWiseSalesCollectionAndDueDataTable = new float[] { 5f, 25f, 10f, 10f, 10f };
            nationalWiseSalesCollectionAndDueDataTable.SetWidths(widthsCellsNationalWiseSalesCollectionAndDueDataTable);
            nationalWiseSalesCollectionAndDueDataTable.WidthPercentage = 100;
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 1, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer", fontArial7Bold)) { Rowspan = 1, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Sales", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Collections", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Dues", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            var sl = 0;
            nationalWiseSalesCollectionAndDueDataTable.HeaderRows = 1;
            var resultData = result.OrderBy(x => x.RegionId).ToList();
            var marketingOfficerData = resultData.GroupBy(x => x.CustomerMarketingOfficerId).ToList();

            foreach (var marketingOfficer in marketingOfficerData)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == marketingOfficer.Key)?.FullName;
                sl++;

                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficerName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((marketingOfficer.Sum(x => x.ThisPeriodSaleValue) - marketingOfficer.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((marketingOfficer.Sum(x => x.ThisPeriodSaleValue) - marketingOfficer.Sum(x => x.ThisPeriodSaleReturnValue) - marketingOfficer.Sum(x => x.ThisPeriodCollection)).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }

            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((result.Sum(x => x.ThisPeriodSaleValue) - result.Sum(x => x.ThisPeriodSaleReturnValue)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodCollection).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((result.Sum(x => x.ThisPeriodSaleValue) - result.Sum(x => x.ThisPeriodSaleReturnValue) - result.Sum(x => x.ThisPeriodCollection)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            return nationalWiseSalesCollectionAndDueDataTable;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintMonthlySalesCollectionAndDueReportToPdfAsync(MemoryStream stream, List<MarketingOfficerMonthlySalesCollectionViewModel> result, string headerText, MarketingOfficerYearlySalesAndCollectionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (result == null)
                throw new ArgumentNullException(nameof(result));

            Rectangle rectangle = new(PageSize.A4.Width, PageSize.A4.Height);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddMarketingOfficerYearlySalesHeader(fontArial9, request);
            var dataTable = PrimaryMonthlySalesCollectionAndDueReportDataTable(result, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryMonthlySalesCollectionAndDueReportDataTable(List<MarketingOfficerMonthlySalesCollectionViewModel> result, Font fontArial7, Font fontArial7Bold)
        {
            PdfPTable nationalWiseSalesCollectionAndDueDataTable = new(5);
            float[] widthsCellsNationalWiseSalesCollectionAndDueDataTable = new float[] { 5f, 25f, 10f, 10f, 10f };
            nationalWiseSalesCollectionAndDueDataTable.SetWidths(widthsCellsNationalWiseSalesCollectionAndDueDataTable);
            nationalWiseSalesCollectionAndDueDataTable.WidthPercentage = 100;
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 1, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Months", fontArial7Bold)) { Rowspan = 1, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Net Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Paid", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Due", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            var sl = 0;

            nationalWiseSalesCollectionAndDueDataTable.HeaderRows = 1;
            //var resultData = result.OrderBy(x => x.RegionId).ToList();
            //var marketingOfficerData = result.GroupBy(x => x.MarketingOfficerId).ToList();

            foreach (var marketingOfficer in result)
            {
                sl++;

                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.MonthName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((marketingOfficer.ThisPeriodSaleValue - marketingOfficer.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase((marketingOfficer.ThisPeriodCollection + marketingOfficer.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.ThisPeriodBalance.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }

            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodSaleValue - x.ThisPeriodSaleReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodCollection + x.ThisPeriodAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            nationalWiseSalesCollectionAndDueDataTable.AddCell(new PdfPCell(new Phrase(result.Sum(x => x.ThisPeriodBalance).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            return nationalWiseSalesCollectionAndDueDataTable;
        }
    }
}
