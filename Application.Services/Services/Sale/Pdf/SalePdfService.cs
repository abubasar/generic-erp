using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.SearchRequestModels.Inventory;
using Application.Services.SearchRequestModels.Report;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Inventory;
using Application.Services.ViewModels.Report.Sales;
using Application.Services.ViewModels.Sale;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Sale.Pdf
{
    public class SalePdfService : ISalePdfService
    {
        private readonly DataContext _context;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;

        public SalePdfService(DataContext context, IWorkContext workContext, IUnitOfWork unitOfWork, ITenantService tenantService)
        {
            _context = context;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
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

        public async Task<PdfPTable> AddHeaderAsync(string reportTitleName, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10)
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
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitleName, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

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
                filterTable.AddCell(new PdfPCell(new Phrase("Date: " + fromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 4f, HorizontalAlignment = 0 });
            }

            return filterTable;
        }

        public PdfPTable AddSubHeader(Font fontArial10, string? customerName, string? productName, string? marketingOfficerName, string? storeName, string? customerZoneName, string? customerAreaName, DateTime? fromDate, DateTime? toDate)
        {
            PdfPTable subHeaderPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            subHeaderPage.SetWidths(widthsCellsHeaderPage);
            subHeaderPage.WidthPercentage = 100;
            subHeaderPage.SpacingAfter = 3;
            if (fromDate.HasValue && toDate.HasValue)
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Date :                      " + fromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (!string.IsNullOrEmpty(customerName))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Customer :              " + customerName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (!string.IsNullOrEmpty(productName))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Product :                 " + productName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (!string.IsNullOrEmpty(marketingOfficerName))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Marketing Officer :  " + marketingOfficerName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (!string.IsNullOrEmpty(storeName))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Store :                     " + storeName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (!string.IsNullOrEmpty(customerZoneName))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Zone :                     " + customerZoneName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (!string.IsNullOrEmpty(customerAreaName))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Area :                      " + customerAreaName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            //subHeaderPage.AddCell(new PdfPCell(new Phrase("", fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            return subHeaderPage;
        }

        public PdfPTable AddPrimarySubHeader(Font fontArial10, SalesItemDetailRequestModel request)
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
            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Customer :              " + customerName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.ProductId.HasValue)
            {
                var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == request.ProductId.Value)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Product :                 " + productName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value)?.FullName;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Marketing Officer :  " + marketingOfficerName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
            }
            if (request.StoreId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Store :                     " + storeName, fontArial10)) { Border = 0, HorizontalAlignment = 0 });
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
            return subHeaderTable;
        }

        public PdfPTable FooterForPreparedCheckedApproved(Font fontArial8, Font fontArial9Bold)
        {
            //Line
            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
            // ==============
            // User Signature
            // ==============
            PdfPTable tableUsers = new(3);
            float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
            tableUsers.WidthPercentage = 100;
            tableUsers.SetWidths(widthsCellsTableUsers);
            tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Prepared By", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Checked By", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Approved By", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            return tableUsers;
        }

        public PdfPTable SearchFilterTable(DateTime? fromDate, DateTime? toDate)
        {
            PdfPTable headerPage = new(1);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            if (fromDate.HasValue && toDate.HasValue)
                headerPage.AddCell(new PdfPCell(new Phrase("Report for the period of " + fromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_LEFT });
            return headerPage;
        }

        public async Task PrintReportToPdf(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, List<string>? numericColumnsForSum = null, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null)
        {
            Guid? tenantId = _workContext.GetTenantId();
            TenantViewModel tenantData = await _tenantService.GetByIdAsync(tenantId);

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, isLandscape);
            //fonts
            Font tableHeaderFont = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font tableDataFont = FontFactory.GetFont("Arial", 9);
            // Add the header
            pdfGenerator.AddHeader(reportTitleName, tenantData?.Name, tenantData?.Address, tenantData?.ContactNo, tenantData?.Email);
            //filtering options
            pdfGenerator.AddTable(SearchFilterTable(fromDate, toDate));
            // Add the table
            pdfGenerator.AddTable(tableData, headers, columnWidths, tableHeaderFont, tableDataFont, numericColumnsForSum);

            // Close the PDF document
            pdfGenerator.Close();
        }

        //Warehouse Wise Sale Invoice
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintWarehouseWiseSaleInvoiceReportToPdf(MemoryStream stream, List<SaleInvoiceViewModel> saleInvoiceViewModels, string reportTitleName, SaleInvoiceRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (saleInvoiceViewModels == null)
                throw new ArgumentNullException(nameof(saleInvoiceViewModels));

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

            var storeName = "";
            if (request.StoreId.HasValue) storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId)?.Name;
            var customerName = "";
            if (request.CustomerId.HasValue) customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;

            var headerTable = await AddHeaderAsync(reportTitleName, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddSubHeader(fontArial9, customerName, "", "", storeName, "", "", request.FromDate, request.ToDate?.AddDays(1));
            var dataTable = WarehouseWiseSaleInvoiceDataTable(saleInvoiceViewModels, fontArial9, fontArial8, fontArial8Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable WarehouseWiseSaleInvoiceDataTable(List<SaleInvoiceViewModel> saleInvoiceViewModels, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            var groupedData = saleInvoiceViewModels.GroupBy(x => x.Store?.Name).OrderBy(x => x.Key).ToList();

            PdfPTable saleInvoiceData = new(12);
            float[] widthsCellsHeaderPage = new float[] { 4f, 6f, 9f, 7f, 25f, 6f, 8f, 7f, 6f, 7f, 8f, 8f };
            saleInvoiceData.SetWidths(widthsCellsHeaderPage);
            saleInvoiceData.WidthPercentage = 100;
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Name", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Commission", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Other Comm.", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Depo Charge", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Net Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            saleInvoiceData.HeaderRows = 1;
            var grandTotalQuantity = 0;
            var sl = 0;
            foreach (var storeGroup in groupedData)
            {
                var totalQty = 0;
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(storeGroup.Key!.ToString(), fontArial8Bold)) { Colspan = 12, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var item in storeGroup)
                {
                    var qty = item.SaleInvoiceDetails!.Sum(x => x.Quantity);
                    totalQty += qty;
                    sl++;
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item.InvoiceDate.ToString("dd/MM/yyyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item.InvoiceNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Customer?.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Customer?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(qty.ToString("#,##0"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Subtotal.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Discount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.OtherDiscount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.DepoCharge.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.NetTotal.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    saleInvoiceData.AddCell(new PdfPCell(new Phrase((item?.Balance + item?.NetTotal)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                grandTotalQuantity += totalQty;
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(storeGroup.Key + " Total : ", fontArial8)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(totalQty.ToString("#,##0"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(storeGroup.Sum(x => x.Subtotal).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(storeGroup.Sum(x => x.Discount).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(storeGroup.Sum(x => x.OtherDiscount).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(storeGroup.Sum(x => x.DepoCharge).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(storeGroup.Sum(x => x.NetTotal).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(storeGroup.Sum(x => x.Balance + x.NetTotal)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(grandTotalQuantity.ToString("#,##0"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.Subtotal).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.Discount).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.OtherDiscount).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.DepoCharge).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.NetTotal).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.Balance + x.NetTotal)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return saleInvoiceData;
        }

        //Sales: Customer, Item Wise
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesCustomerItemWiseReportToPdf(MemoryStream stream, List<SalesItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate, string? customerName, string? marketingOfficerName, string? storeName, string? customerZoneName, string? customerAreaName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

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
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddSubHeader(fontArial10, customerName, "", marketingOfficerName, storeName, customerZoneName, customerAreaName, fromDate, toDate);
            var dataTable = SalesCustomerItemWiseDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable SalesCustomerItemWiseDataTable(List<SalesItemViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            var customerData = list.GroupBy(x => x.CustomerName).OrderBy(x => x.Key).ToList();

            PdfPTable customerWiseSalesItemData = new(19);
            float[] widthsCellsHeaderPage = new float[] { 3f, 8f, 6f, 15f, 9f, 6f, 4f, 5f, 5f, 5f, 5f, 5f, 5f, 9f, 5f, 5f, 5f, 4f, 9f };
            customerWiseSalesItemData.SetWidths(widthsCellsHeaderPage);
            customerWiseSalesItemData.WidthPercentage = 100;
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Date", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Store", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Sale Order No", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Rate", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Invoice Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Cash Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Special Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Offer Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Other Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Invoice Net Rate", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Invoice Net Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Monthly Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Yearly Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Target Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Net Rate", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Net Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            customerWiseSalesItemData.HeaderRows = 1;
            foreach (var customer in customerData)
            {
                var productData = customer.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(customer.Key!.ToString(), fontArial7Bold)) { Colspan = 19, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var product in productData)
                {
                    var sl = 0;
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("SL", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial7Bold)) { Colspan = 15, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    foreach (var item in product)
                    {
                        sl++;
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.BillNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.Date.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.StoreName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.SaleOrderNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.Quantity.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.Rate.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.InvoiceDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.CashDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.SpecialDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.OfferDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.OtherDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.InvoiceNetRate.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.Value.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.MonthlyDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.YearlyDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.TargetDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.NetRate.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(item?.NetAmount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Quantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.Rate) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.InvoiceDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.CashDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.SpecialDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.OfferDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.OtherDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Value) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Value).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.MonthlyDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.YearlyDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.TargetDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.NetAmount) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(customer.Key + " Total : ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.Quantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.Value).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Quantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Value).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerWiseSalesItemData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return customerWiseSalesItemData;
        }

        //Sales: Item, Customer Wise
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesItemCustomerWiseReportToPdf(MemoryStream stream, List<SalesItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate, string? productName, string? marketingOfficerName, string? storeName, string? customerZoneName, string? customerAreaName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

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
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddSubHeader(fontArial10, "", productName, marketingOfficerName, storeName, customerZoneName, customerAreaName, fromDate, toDate);
            var dataTable = SalesItemCustomerWiseDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();

            return (filterTable, dataTable);
        }

        public PdfPTable SalesItemCustomerWiseDataTable(List<SalesItemViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            var productData = list.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();

            PdfPTable salesItemWiseCustomerData = new(19);
            float[] widthsCellsHeaderPage = new float[] { 3f, 8f, 6f, 15f, 9f, 6f, 4f, 5f, 5f, 5f, 5f, 5f, 5f, 9f, 5f, 5f, 5f, 4f, 9f };
            salesItemWiseCustomerData.SetWidths(widthsCellsHeaderPage);
            salesItemWiseCustomerData.WidthPercentage = 100;
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Date", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Store", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Sale Order No", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Rate", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Invoice Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Cash Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Special Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Offer Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Other Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Invoice Net Rate", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Invoice Net Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Monthly Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Yearly Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Target Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Net Rate", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Net Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesItemWiseCustomerData.HeaderRows = 1;
            foreach (var product in productData)
            {
                var customerData = product.GroupBy(x => x.CustomerName).OrderBy(x => x.Key).ToList();
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial7Bold)) { Colspan = 19, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var customer in customerData)
                {
                    var sl = 0;
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("SL", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(customer.Key!.ToString(), fontArial7Bold)) { Colspan = 15, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    foreach (var item in customer)
                    {
                        sl++;
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.BillNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.Date.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.StoreName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.SaleOrderNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.Quantity.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.Rate.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.InvoiceDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.CashDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.SpecialDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.OfferDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.OtherDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.InvoiceNetRate.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.Value.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.MonthlyDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.YearlyDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.TargetDiscountPerUnit.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.NetRate.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item?.NetAmount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(customer.Key + " Total : ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.Quantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.Rate) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.InvoiceDiscountPerUnit) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.CashDiscountPerUnit) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.SpecialDiscountPerUnit) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.OfferDiscountPerUnit) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.OtherDiscountPerUnit) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Value) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.Value).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.MonthlyDiscountPerUnit) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.YearlyDiscountPerUnit) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.Quantity * x.TargetDiscountPerUnit) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.NetAmount) / customer.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Quantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.Rate) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.InvoiceDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.CashDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.SpecialDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.OfferDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.OtherDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Value) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Value).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.MonthlyDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.YearlyDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.TargetDiscountPerUnit) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.NetAmount) / product.Sum(x => x.Quantity)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Quantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Value).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return salesItemWiseCustomerData;
        }

        //Sale Total : Product Wise
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSaleTotalProductWiseReportToPdf(MemoryStream stream, List<SalesItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate, string? productName, string? marketingOfficerName, string? storeName, string? customerZoneName, string? customerAreaName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

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

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddSubHeader(fontArial9, "", productName, marketingOfficerName, storeName, customerZoneName, customerAreaName, fromDate, toDate);
            var dataTable = SaleTotalProductWiseDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable SaleTotalProductWiseDataTable(List<SalesItemViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            decimal SafeDivide(decimal numerator, decimal denominator) => denominator == 0 ? 0 : numerator / denominator;

            decimal TotalInstantDiscount(SalesItemViewModel x) =>
                (x.Quantity * x.InvoiceDiscountPerUnit) +
                (x.Quantity * x.CashDiscountPerUnit) +
                (x.Quantity * x.SpecialDiscountPerUnit) +
                (x.Quantity * x.OfferDiscountPerUnit) +
                (x.Quantity * x.OtherDiscountPerUnit);

            decimal TotalPayableDiscount(SalesItemViewModel x) =>
                (x.Quantity * x.MonthlyDiscountPerUnit) +
                (x.Quantity * x.YearlyDiscountPerUnit) +
                (x.Quantity * x.TargetDiscountPerUnit);

            var productTypeData = list.GroupBy(x => x.ProductTypeName ?? "N/A").OrderBy(x => x.Key).ToList();

            PdfPTable salesItemWiseCustomerData = new(22);
            float[] widthsCellsHeaderPage = new float[] { 6f, 20f, 6f, 10f, 14f, 14f, 8f, 18f, 10f, 8f, 10f, 8f, 8f, 18f, 10f, 18f, 10f, 9f, 9f, 18f, 8f, 18f };

            salesItemWiseCustomerData.SetWidths(widthsCellsHeaderPage);
            salesItemWiseCustomerData.WidthPercentage = 100;

            // ===== HEADER =====
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Product", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Rate", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Instant Discount Details", fontArial7Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Invoice Net Rate", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Invoice Net Amount", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Discount Payable", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Net Rate", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Net Amount", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            // ===== SUB HEADER =====
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Bag Size", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Bag Qty", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("KG", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Ton", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Invoice", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Cash", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Special", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Offer", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Other", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Monthly", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Yearly", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Target", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesItemWiseCustomerData.HeaderRows = 2;
            var sl = 0;
            foreach (var productType in productTypeData)
            {
                var productData = productType.GroupBy(x => x.ProductName ?? "N/A").OrderBy(x => x.Key).ToList();
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Key!.ToString(), fontArial7Bold)) { Colspan = 22, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var product in productData)
                {
                    sl++;

                    var totalQty = product.Sum(x => x.Quantity);
                    var totalPrimaryQty = product.Sum(x => x.PrimaryQuantity);
                    var totalAmount = product.Sum(x => x.Quantity * x.Rate);
                    var totalValue = product.Sum(x => x.Value);
                    var totalNetAmount = product.Sum(x => x.NetAmount);

                    var totalInstantDiscount = product.Sum(x => TotalInstantDiscount(x));
                    var totalPayableDiscount = product.Sum(x => TotalPayableDiscount(x));


                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Key, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.First().BagWeight.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(totalPrimaryQty.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(totalQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((totalQty / 1000m).ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((totalAmount / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.InvoiceDiscountPerUnit) / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.CashDiscountPerUnit) / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.SpecialDiscountPerUnit) / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.OfferDiscountPerUnit) / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.OtherDiscountPerUnit) / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(totalInstantDiscount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((totalValue / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(totalValue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.MonthlyDiscountPerUnit) / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.YearlyDiscountPerUnit) / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((product.Sum(x => x.Quantity * x.TargetDiscountPerUnit) / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(totalPayableDiscount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((totalNetAmount / totalQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(totalNetAmount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                // ===== PRODUCT TYPE TOTAL =====
                var typeQty = productType.Sum(x => x.Quantity);
                var typePrimaryQty = productType.Sum(x => x.PrimaryQuantity);
                var typeAmount = productType.Sum(x => x.Quantity * x.Rate);
                var typeValue = productType.Sum(x => x.Value);
                var typeNetAmount = productType.Sum(x => x.NetAmount);

                var typeInstantDiscount = productType.Sum(x => TotalInstantDiscount(x));
                var typePayableDiscount = productType.Sum(x => TotalPayableDiscount(x));

                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(typePrimaryQty.ToString(), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(typeQty.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((typeQty / 1000m).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((typeAmount / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(typeAmount.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((productType.Sum(x => x.Quantity * x.InvoiceDiscountPerUnit) / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((productType.Sum(x => x.Quantity * x.CashDiscountPerUnit) / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((productType.Sum(x => x.Quantity * x.SpecialDiscountPerUnit) / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((productType.Sum(x => x.Quantity * x.OfferDiscountPerUnit) / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((productType.Sum(x => x.Quantity * x.OtherDiscountPerUnit) / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(typeInstantDiscount.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((typeValue / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(typeValue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((productType.Sum(x => x.Quantity * x.MonthlyDiscountPerUnit) / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((productType.Sum(x => x.Quantity * x.YearlyDiscountPerUnit) / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((productType.Sum(x => x.Quantity * x.TargetDiscountPerUnit) / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(typePayableDiscount.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((typeNetAmount / typeQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(typeNetAmount.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }

            // ===== GRAND TOTAL =====
            var gQty = list.Sum(x => x.Quantity);
            var gPrimaryQty = list.Sum(x => x.PrimaryQuantity);
            var gAmount = list.Sum(x => x.Quantity * x.Rate);
            var gValue = list.Sum(x => x.Value);
            var gNetAmount = list.Sum(x => x.NetAmount);

            var gInstantDiscount = list.Sum(x => TotalInstantDiscount(x));
            var gPayableDiscount = list.Sum(x => TotalPayableDiscount(x));

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(gPrimaryQty.ToString(), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(gQty.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase((gQty / 1000m).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(gAmount.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(gInstantDiscount.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(gValue.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(gPayableDiscount.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(gNetAmount.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return salesItemWiseCustomerData;
        }

        //Customer Monthly Discount
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerDateWiseDiscountReportToPdf(MemoryStream stream, List<CustomerDiscountViewModel> list, string reportTitle, CustomerDiscountRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
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
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddCustomerMonthlyDiscountHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddCustomerDateWiseDiscountSubHeader(fontArial10, request);
            var dataTable = CustomerMonthlyDiscountDataTable(list, fontArial9Bold, fontArial8, fontArial8Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }
        public PdfPTable AddCustomerDateWiseDiscountSubHeader(Font fontArial10, CustomerDiscountRequestModel request)
        {
            var customerName = "";
            if (request.CustomerId.HasValue) customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;

            PdfPTable subHeaderPage = new(1);
            float[] widthsCellsSubHeaderPage = new float[] { 100f };
            subHeaderPage.SetWidths(widthsCellsSubHeaderPage);
            subHeaderPage.WidthPercentage = 100;
            subHeaderPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.ToString("dd/MM/yyyy") + " to " + request.ToDate.ToString("dd/MM/yyyy"), fontArial10)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Customer: " + customerName, fontArial10)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
            }
            subHeaderPage.AddCell(new PdfPCell(new Phrase("", fontArial10)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            return subHeaderPage;
        }

        //Customer Monthly Discount
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerMonthlyDiscountReportToPdf(MemoryStream stream, List<CustomerDiscountViewModel> list, string reportTitle, string? customerName, int year, int? month)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
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
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddCustomerMonthlyDiscountHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddCustomerMonthlyDiscountSubHeader(fontArial10, customerName, year, month);
            var dataTable = CustomerMonthlyDiscountDataTable(list, fontArial9Bold, fontArial8, fontArial8Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable CustomerMonthlyDiscountDataTable(List<CustomerDiscountViewModel> list, Font fontArial9Bold, Font fontArial8, Font fontArial8Bold)
        {
            var customerData = list.GroupBy(x => x.CustomerName).OrderBy(x => x.Key).ToList();

            PdfPTable customerDiscountData = new(7);
            float[] widthsCellsHeaderPage = new float[] { 6f, 38f, 14f, 14f, 14f, 14f, 14f };
            customerDiscountData.SetWidths(widthsCellsHeaderPage);
            customerDiscountData.WidthPercentage = 100;
            customerDiscountData.AddCell(new PdfPCell(new Phrase("Product", fontArial9Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 4, HorizontalAlignment = 1 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 4, HorizontalAlignment = 1 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase("Monthly Discount", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 4, HorizontalAlignment = 1 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase("Yearly Discount", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 4, HorizontalAlignment = 1 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase("Target Discount", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 4, HorizontalAlignment = 1 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase("Total", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 4, HorizontalAlignment = 1 });

            customerDiscountData.HeaderRows = 1;
            foreach (var customer in customerData)
            {
                var sl = 0;
                customerDiscountData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                customerDiscountData.AddCell(new PdfPCell(new Phrase(customer.Key!.ToString(), fontArial8Bold)) { Colspan = 6, HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 4f });
                foreach (var item in customer)
                {
                    sl++;
                    customerDiscountData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    customerDiscountData.AddCell(new PdfPCell(new Phrase(item?.ProductName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    customerDiscountData.AddCell(new PdfPCell(new Phrase(item?.TotalQuantity.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerDiscountData.AddCell(new PdfPCell(new Phrase(item?.MonthlyDiscount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerDiscountData.AddCell(new PdfPCell(new Phrase(item?.YearlyDiscount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerDiscountData.AddCell(new PdfPCell(new Phrase(item?.TargetDiscount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    customerDiscountData.AddCell(new PdfPCell(new Phrase((item?.MonthlyDiscount + item?.YearlyDiscount + item?.TargetDiscount)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                customerDiscountData.AddCell(new PdfPCell(new Phrase(customer.Key + " Total : ", fontArial8)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerDiscountData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.TotalQuantity).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerDiscountData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.MonthlyDiscount).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerDiscountData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.YearlyDiscount).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerDiscountData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.TargetDiscount).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                customerDiscountData.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.MonthlyDiscount + x.YearlyDiscount + x.TargetDiscount).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            customerDiscountData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TotalQuantity).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.MonthlyDiscount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.YearlyDiscount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TargetDiscount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            customerDiscountData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.MonthlyDiscount + x.YearlyDiscount + x.TargetDiscount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return customerDiscountData;
        }

        public PdfPTable AddCustomerMonthlyDiscountSubHeader(Font fontArial10, string? customerName, int year, int? month)
        {
            string? monthName;
            switch (month)
            {
                case 1:
                    monthName = "January";
                    break;
                case 2:
                    monthName = "February";
                    break;
                case 3:
                    monthName = "March";
                    break;
                case 4:
                    monthName = "April";
                    break;
                case 5:
                    monthName = "May";
                    break;
                case 6:
                    monthName = "June";
                    break;
                case 7:
                    monthName = "July";
                    break;
                case 8:
                    monthName = "August";
                    break;
                case 9:
                    monthName = "September";
                    break;
                case 10:
                    monthName = "October";
                    break;
                case 11:
                    monthName = "November";
                    break;
                case 12:
                    monthName = "December";
                    break;
                default:
                    monthName = "";
                    break;
            }

            PdfPTable subHeaderPage = new(1);
            float[] widthsCellsSubHeaderPage = new float[] { 100f };
            subHeaderPage.SetWidths(widthsCellsSubHeaderPage);
            subHeaderPage.WidthPercentage = 100;
            subHeaderPage.AddCell(new PdfPCell(new Phrase("Year:         " + year.ToString(), fontArial10)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
            subHeaderPage.AddCell(new PdfPCell(new Phrase("Month:      " + monthName, fontArial10)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Customer: " + customerName, fontArial10)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
            }
            subHeaderPage.AddCell(new PdfPCell(new Phrase("", fontArial10)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            return subHeaderPage;
        }

        public async Task<PdfPTable> AddCustomerMonthlyDiscountHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10)
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

            return headerPage;
        }

        //Sale Order
        public async Task PrintSaleOrderReportToPdf(MemoryStream stream, List<SaleOrderViewModel> saleOrderViewModels, string reportTitle, bool isDetails, SaleOrderRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (saleOrderViewModels == null)
                throw new ArgumentNullException(nameof(saleOrderViewModels));

            //InitializeDocument
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
            var headerTable = await AddSaleOrderHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var saleOrderData = SaleOrderDataTable(saleOrderViewModels, isDetails, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(saleOrderData);

            document.Close();
        }

        private async Task<PdfPTable> AddSaleOrderHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, SaleOrderRequestModel request)
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
            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Customer: " + customerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.StoreId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Store: " + storeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        private PdfPTable SaleOrderDataTable(List<SaleOrderViewModel> saleOrderViewModels, bool isDetails, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable salerOrderData = new(15);
            float[] widthCellsHeaderPage = new float[] { 6f, 17f, 12f, 11f, 30f, 20f, 11f, 13f, 11f, 11f, 11f, 13f, 11f, 9f, 13f };
            salerOrderData.SetWidths(widthCellsHeaderPage);
            salerOrderData.WidthPercentage = 100;
            salerOrderData.HeaderRows = 1;

            salerOrderData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Sale Order NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Reference NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Order Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Customer", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Store", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Delivery Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Subtotal", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Offer Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Other Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Total", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Transport Cost", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Depo Charge", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            salerOrderData.AddCell(new PdfPCell(new Phrase("Net Total", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in saleOrderViewModels)
            {
                sl++;
                salerOrderData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.SaleOrderNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.ReferenceNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.OrderDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.Customer?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.DeliveryDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.Subtotal.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.Discount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.OfferDiscount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.OtherDiscount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.Total.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.TransportationCost.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.DepoCharge.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salerOrderData.AddCell(new PdfPCell(new Phrase(item?.NetTotal.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(13);
                    float[] widthsCellsProductDetailsTable = new float[] { 6f, 32f, 8f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 10f, 12f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bag Weight", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bag Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bonus Bag Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bonus Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Discount Per Unit", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Offer Disc. (P.U.)", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Net Rate", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item?.SaleOrderDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.BagWeight.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.PrimaryQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Quantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.PrimaryBonusQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.BonusQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Rate.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.DiscountPerUnit.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.OfferDiscountPerUnit.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.NetRate.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    salerOrderData.AddCell(new PdfPCell(new Phrase("Sale Order Details", fontArial8Gray)) { Colspan = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial9);
                    productDetailsCell2.Colspan = 12;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    salerOrderData.AddCell(productDetailsCell2);
                }
            }

            salerOrderData.AddCell(new PdfPCell(new Phrase("Total:", fontArial8Bold)) { Colspan = 7, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            salerOrderData.AddCell(new PdfPCell(new Phrase(saleOrderViewModels.Sum(x => x.Subtotal).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            salerOrderData.AddCell(new PdfPCell(new Phrase(saleOrderViewModels.Sum(x => x.Discount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            salerOrderData.AddCell(new PdfPCell(new Phrase(saleOrderViewModels.Sum(x => x.OfferDiscount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            salerOrderData.AddCell(new PdfPCell(new Phrase(saleOrderViewModels.Sum(x => x.OtherDiscount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            salerOrderData.AddCell(new PdfPCell(new Phrase(saleOrderViewModels.Sum(x => x.Total).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            salerOrderData.AddCell(new PdfPCell(new Phrase(saleOrderViewModels.Sum(x => x.TransportationCost).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            salerOrderData.AddCell(new PdfPCell(new Phrase(saleOrderViewModels.Sum(x => x.DepoCharge).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            salerOrderData.AddCell(new PdfPCell(new Phrase(saleOrderViewModels.Sum(x => x.NetTotal).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return salerOrderData;
        }

        //Delivery Note
        public async Task PrintDeliveryNoteReportToPdf(MemoryStream stream, List<DeliveryNoteViewModel> deliveryNoteViewModels, string reportTitle, bool isDetails, DeliveryNoteRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (deliveryNoteViewModels == null)
                throw new ArgumentNullException(nameof(deliveryNoteViewModels));

            //InitializeDocument
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
            var headerTable = await AddDeliveryNoteHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var deliveryNoteData = DeliveryNoteDataTable(deliveryNoteViewModels, isDetails, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(deliveryNoteData);

            document.Close();
        }

        private async Task<PdfPTable> AddDeliveryNoteHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, DeliveryNoteRequestModel request)
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
            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Customer: " + customerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.StoreId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Store: " + storeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        private PdfPTable DeliveryNoteDataTable(List<DeliveryNoteViewModel> deliveryNoteViewModels, bool isDetails, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable deliveryNoteData = new(14);
            float[] widthCellsHeaderPage = new float[] { 7f, 18f, 19f, 13f, 12f, 34f, 22f, 11f, 13f, 10f, 10f, 15f, 12f, 15f };
            deliveryNoteData.SetWidths(widthCellsHeaderPage);
            deliveryNoteData.WidthPercentage = 100;
            deliveryNoteData.HeaderRows = 1;

            deliveryNoteData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Delivery Note NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Sale Order NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Ref. NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Delivery Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Customer", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Store", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Driver Name", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Driver Contact NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Transpt.", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Transpt. Cost", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Subtotal", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Total", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in deliveryNoteViewModels)
            {
                sl++;
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.DeliveryNoteNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.SaleOrderNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.ReferenceNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.DeliveryDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.Customer?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.DriverName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.DriverContactNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.TransportName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.TransportationCost.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.Subtotal.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.Discount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                deliveryNoteData.AddCell(new PdfPCell(new Phrase(item?.Total.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(10);
                    float[] widthsCellsProductDetailsTable = new float[] { 6f, 32f, 8f, 10f, 10f, 10f, 10f, 10f, 10f, 12f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Order Bag Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Order Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Delivery Bag Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Delivery Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bonus Bag Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bonus Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Delivery Bonus Bag Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item?.DeliveryNoteDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.OrderedPrimaryQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.OrderedQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.DeliveryPrimaryQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.DeliveryQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.OrderedPrimaryBonusQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.OrderedBonusQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.DeliveryPrimaryBonusQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    deliveryNoteData.AddCell(new PdfPCell(new Phrase("Delivery Note Details", fontArial8Gray)) { Colspan = 4, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial9);
                    productDetailsCell2.Colspan = 10;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    deliveryNoteData.AddCell(productDetailsCell2);
                }
            }

            deliveryNoteData.AddCell(new PdfPCell(new Phrase("Total:", fontArial8Bold)) { Colspan = 10, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase(deliveryNoteViewModels.Sum(x => x.TransportationCost).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase(deliveryNoteViewModels.Sum(x => x.Subtotal).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase(deliveryNoteViewModels.Sum(x => x.Discount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            deliveryNoteData.AddCell(new PdfPCell(new Phrase(deliveryNoteViewModels.Sum(x => x.Total).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return deliveryNoteData;
        }

        //Sale Invoice
        public async Task PrintSaleInvoiceReportToPdf(MemoryStream stream, List<SaleInvoiceViewModel> saleInvoiceViewModels, string reportTitle, bool isDetails, SaleInvoiceRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (saleInvoiceViewModels == null)
                throw new ArgumentNullException(nameof(saleInvoiceViewModels));

            //InitializeDocument
            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;

            document.Open();

            //Fonts
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Gray = FontFactory.GetFont("Arial", 7, BaseColor.Gray);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            //Add header table
            var headerTable = await AddSaleInvoiceHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var saleInvoiceData = SaleInvoiceDataTable(saleInvoiceViewModels, isDetails, fontArial8, fontArial7, fontArial7Bold, fontArial7Gray);

            document.Add(headerTable);
            document.Add(saleInvoiceData);

            document.Close();
        }

        private async Task<PdfPTable> AddSaleInvoiceHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, SaleInvoiceRequestModel request)
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
            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Customer: " + customerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.StoreId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Store: " + storeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        private PdfPTable SaleInvoiceDataTable(List<SaleInvoiceViewModel> saleInvoiceViewModels, bool isDetails, Font fontArial8, Font fontArial7, Font fontArial7Bold, Font fontArial7Gray)
        {
            PdfPTable saleInvoiceData = new(16);
            float[] widthCellsHeaderPage = new float[] { 7f, 16f, 17f, 11f, 18f, 32f, 12f, 14f, 12f, 11f, 11f, 14f, 12f, 11f, 11f, 14f };
            saleInvoiceData.SetWidths(widthCellsHeaderPage);
            saleInvoiceData.WidthPercentage = 100;
            saleInvoiceData.HeaderRows = 1;

            saleInvoiceData.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Sale Invoice NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Delivery Note NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Invoice Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Store", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Customer", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Ref. NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Subtotal", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Discount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Offer DIS.", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Other DIS.", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Net Sale", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Transpt.", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Transpt. Cost", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Depo Charge", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Total Payable", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            var sl = 0;

            foreach (var item in saleInvoiceViewModels)
            {
                sl++;
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.InvoiceNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.DeliveryNoteNo?.Replace(',', '\n'), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.InvoiceDate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Customer?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.ReferenceNo?.Replace(',', '\n'), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Subtotal.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Discount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.OfferDiscount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.OtherDiscount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Total.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.TransportName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.TransportationCost.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.DepoCharge.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.NetTotal.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(14);
                    float[] widthsCellsProductDetailsTable = new float[] { 6f, 30f, 8f, 8f, 10f, 8f, 8f, 8f, 8f, 9f, 8f, 10f, 12f, 12f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bag Qty", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Quantity", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bonus Bag Qty", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bonus Qty", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Rate", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Discount Per Unit", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Offer DIS. P.U.", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Net Rate", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Discount Amount", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Delivery Date", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item?.SaleInvoiceDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.PrimaryQuantity.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Quantity.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.PrimaryBonusQuantity.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.BonusQuantity.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Rate.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.DiscountPerUnit.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.OfferDiscountPerUnit.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.NetRate.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.DiscountAmount.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.DeliveryDate?.ToString("dd/MM/yyy"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    }

                    saleInvoiceData.AddCell(new PdfPCell(new Phrase("Sale Invoice Details", fontArial7Gray)) { Colspan = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial8);
                    productDetailsCell2.Colspan = 13;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    saleInvoiceData.AddCell(productDetailsCell2);
                }
            }

            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Total:", fontArial7Bold)) { Colspan = 7, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.Subtotal).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.Discount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.OfferDiscount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.OtherDiscount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.Total).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.TransportationCost).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.DepoCharge).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase(saleInvoiceViewModels.Sum(x => x.NetTotal).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return saleInvoiceData;
        }

        //Sale Return
        public async Task PrintSaleReturnReportToPdf(MemoryStream stream, List<SaleReturnViewModel> saleReturnViewModels, string reportTitle, bool isDetails, SaleReturnRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (saleReturnViewModels == null)
                throw new ArgumentNullException(nameof(saleReturnViewModels));

            //InitializeDocument
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
            var headerTable = await AddSaleReturnHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var saleReturnData = SaleReturnDataTable(saleReturnViewModels, isDetails, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(saleReturnData);

            document.Close();
        }

        private async Task<PdfPTable> AddSaleReturnHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, SaleReturnRequestModel request)
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
            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Customer: " + customerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.StoreId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Store: " + storeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        private PdfPTable SaleReturnDataTable(List<SaleReturnViewModel> saleReturnViewModels, bool isDetails, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable saleReturnData = new(9);
            float[] widthCellsHeaderPage = new float[] { 5f, 12f, 12f, 10f, 15f, 36f, 12f, 20f, 10f };
            saleReturnData.SetWidths(widthCellsHeaderPage);
            saleReturnData.WidthPercentage = 100;
            saleReturnData.HeaderRows = 1;

            saleReturnData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleReturnData.AddCell(new PdfPCell(new Phrase("Sale Return NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleReturnData.AddCell(new PdfPCell(new Phrase("Delivery Note NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleReturnData.AddCell(new PdfPCell(new Phrase("Return Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleReturnData.AddCell(new PdfPCell(new Phrase("Store", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleReturnData.AddCell(new PdfPCell(new Phrase("Customer", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleReturnData.AddCell(new PdfPCell(new Phrase("Ref. NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleReturnData.AddCell(new PdfPCell(new Phrase("Remarks", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleReturnData.AddCell(new PdfPCell(new Phrase("Total", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in saleReturnViewModels)
            {
                sl++;
                saleReturnData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleReturnData.AddCell(new PdfPCell(new Phrase(item?.SaleReturnNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleReturnData.AddCell(new PdfPCell(new Phrase(item?.DeliveryNoteNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleReturnData.AddCell(new PdfPCell(new Phrase(item?.SaleReturnDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleReturnData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                saleReturnData.AddCell(new PdfPCell(new Phrase(item?.Customer?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                saleReturnData.AddCell(new PdfPCell(new Phrase(item?.ReferenceNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleReturnData.AddCell(new PdfPCell(new Phrase(item?.Remark, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleReturnData.AddCell(new PdfPCell(new Phrase(item?.Total.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(10);
                    float[] widthsCellsProductDetailsTable = new float[] { 6f, 32f, 8f, 10f, 10f, 12f, 8f, 10f, 10f, 12f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bag Weight", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Delivered Bag Qty", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Delivered Qty", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Return Bag Qty", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Return Qty", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item?.SaleReturnDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.BagWeight.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.PrimaryQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Quantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Rate.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.ReturnPrimaryQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.ReturnQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    saleReturnData.AddCell(new PdfPCell(new Phrase("Sale Return Details", fontArial8Gray)) { Colspan = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial9);
                    productDetailsCell2.Colspan = 6;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    saleReturnData.AddCell(productDetailsCell2);
                }
            }

            saleReturnData.AddCell(new PdfPCell(new Phrase("Total:", fontArial8Bold)) { Colspan = 8, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            saleReturnData.AddCell(new PdfPCell(new Phrase(saleReturnViewModels.Sum(x => x.Total).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return saleReturnData;
        }

        //Stock Transfer
        public async Task PrintStockTransferReportToPdfAsync(MemoryStream stream, List<StockTransferViewModel> stockTransferViewModels, string reportTitle, bool isDetails, StockTransferRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stockTransferViewModels == null)
                throw new ArgumentNullException(nameof(stockTransferViewModels));

            //InitializeDocument
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
            var headerTable = await AddStockTransferHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var stockTransferData = StockTransferDataTable(stockTransferViewModels, isDetails, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(stockTransferData);

            document.Close();
        }

        private async Task<PdfPTable> AddStockTransferHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, StockTransferRequestModel request)
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
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        private PdfPTable StockTransferDataTable(List<StockTransferViewModel> stockTransferViewModels, bool isDetails, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable stockTransferData = new(7);
            float[] widthCellsHeaderPage = new float[] { 5f, 8f, 15f, 15f, 8f, 8f, 10f };
            stockTransferData.SetWidths(widthCellsHeaderPage);
            stockTransferData.WidthPercentage = 100;
            stockTransferData.HeaderRows = 1;

            stockTransferData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockTransferData.AddCell(new PdfPCell(new Phrase("Transfer NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockTransferData.AddCell(new PdfPCell(new Phrase("Source", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockTransferData.AddCell(new PdfPCell(new Phrase("Destination", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockTransferData.AddCell(new PdfPCell(new Phrase("Tansfer Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockTransferData.AddCell(new PdfPCell(new Phrase("Truck NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockTransferData.AddCell(new PdfPCell(new Phrase("Remarks", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in stockTransferViewModels)
            {
                sl++;
                stockTransferData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                stockTransferData.AddCell(new PdfPCell(new Phrase(item?.TransferNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                stockTransferData.AddCell(new PdfPCell(new Phrase(item?.Source?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                stockTransferData.AddCell(new PdfPCell(new Phrase(item?.Destination?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                stockTransferData.AddCell(new PdfPCell(new Phrase(item?.TransferDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                stockTransferData.AddCell(new PdfPCell(new Phrase(item?.TruckNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                stockTransferData.AddCell(new PdfPCell(new Phrase(item?.Remark, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

                if (isDetails && item?.Source?.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid())
                {
                    PdfPTable productDetailsTable = new(6);
                    float[] widthsCellsProductDetailsTable = new float[] { 6f, 25f, 8f, 8f, 10f, 14f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Bag Weight", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Transfer Bag Qty", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Transfer Qty", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item?.StockTransferDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.BagWeight.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.TransferBagQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.TransferQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    stockTransferData.AddCell(new PdfPCell(new Phrase("Stock Transfer Details", fontArial8Gray)) { Colspan = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial9);
                    productDetailsCell2.Colspan = 4;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    stockTransferData.AddCell(productDetailsCell2);
                }
                if (isDetails && item?.Source?.InventoryTypeId != InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid())
                {
                    PdfPTable rmProductDetailsTable = new(4);
                    float[] widthsCellsRmProductDetailsTable = new float[] { 6f, 25f, 8f, 14f };
                    rmProductDetailsTable.SetWidths(widthsCellsRmProductDetailsTable);
                    rmProductDetailsTable.WidthPercentage = 100;
                    rmProductDetailsTable.SpacingAfter = 0;
                    rmProductDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    rmProductDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    rmProductDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    rmProductDetailsTable.AddCell(new PdfPCell(new Phrase("Transfer Qty", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var rmMiniSL = 0;
                    foreach (var miniItem in item?.StockTransferDetails!)
                    {
                        rmMiniSL++;
                        rmProductDetailsTable.AddCell(new PdfPCell(new Phrase(rmMiniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        rmProductDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        rmProductDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        rmProductDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.TransferQuantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    stockTransferData.AddCell(new PdfPCell(new Phrase("Stock Transfer Details", fontArial8Gray)) { Colspan = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var rmProductDetailsCell2 = GetPdfCell("", fontArial9);
                    rmProductDetailsCell2.Colspan = 4;
                    rmProductDetailsCell2.Padding = 0;
                    rmProductDetailsCell2.AddElement(rmProductDetailsTable);
                    stockTransferData.AddCell(rmProductDetailsCell2);
                }
            }

            return stockTransferData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesReportToPdf(MemoryStream stream, SalesReportViewModel list, string reportTitle, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);


            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringByDateTable(fontArial9, fromDate, toDate);
            var dataTable = SalesReportDataTable(list, fontArial9, fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable SalesReportDataTable(SalesReportViewModel list, Font fontArial9, Font fontArial9Bold)
        {
            var netSales = list.TotalSales - list.TotalSalesReturn - list.TotalOfferDiscount - list.TotalOtherDiscount - list.TotalInvoiceDiscount - list.TotalCashDiscount - list.TotalSpecialDiscount - list.TotalMonthlyDiscount - list.TotalYearlyDiscount - list.TotalTargetDiscount;
            PdfPTable salesReportData = new(2);
            float[] widthsCellsHeaderPage = new float[] { 50f, 50f };
            salesReportData.SetWidths(widthsCellsHeaderPage);
            salesReportData.WidthPercentage = 100;
            salesReportData.AddCell(new PdfPCell(new Phrase("Particular", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Sales", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalSales.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Sales Return", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalSalesReturn.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Invoice Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalInvoiceDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Cash Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalCashDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Special Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalSpecialDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Monthly Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalMonthlyDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Yearly Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalYearlyDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Target Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalTargetDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Offer Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalOfferDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Less: Other Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(list.TotalOtherDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesReportData.AddCell(new PdfPCell(new Phrase("Net Sales", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            salesReportData.AddCell(new PdfPCell(new Phrase(netSales.ToString("#,##0.00"), fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return salesReportData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesTotalMonthWiseReportToPdf(MemoryStream stream, List<SalesTotalMonthWiseViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);


            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringByDateTable(fontArial9, fromDate, toDate);
            var dataTable = SalesTotalMonthWiseReportDataTable(list, fontArial8, fontArial8Bold);
            var footer = FooterForPreparedCheckedApproved(fontArial8, fontArial9Bold);
            var spaceTable = SpaceTable(fontArial8);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(footer);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable SalesTotalMonthWiseReportDataTable(List<SalesTotalMonthWiseViewModel> list, Font fontArial8, Font fontArial8Bold)
        {
            PdfPTable salesTotalMonthWiseReportData = new(13);
            float[] widthsCellsHeaderPage = new float[] { 4f, 5f, 5f, 10f, 6f, 10f, 10f, 9f, 8f, 7f, 10f, 7f, 10f };
            salesTotalMonthWiseReportData.SetWidths(widthsCellsHeaderPage);
            salesTotalMonthWiseReportData.WidthPercentage = 100;
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Sale Quantity", fontArial8Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Sale Amount", fontArial8Bold)) { Colspan = 7, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Year", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Month", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Sale", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Net", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Total Discount", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Depo Charge", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Trans. Cost", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Sale", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Net", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            var sl = 0;
            salesTotalMonthWiseReportData.HeaderRows = 2;
            foreach (var item in list)
            {
                sl++;
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.Year.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.Month.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleQuantity?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleReturnQuantity?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase((item?.SaleQuantity - item?.SaleReturnQuantity)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleAmount?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleCommission?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.DepoCharge?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.TransportationCost?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase((item?.SaleAmount + item?.DepoCharge + item?.TransportationCost - item?.SaleCommission)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleReturnValue?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase((item?.SaleAmount + item?.DepoCharge + item?.TransportationCost - item?.SaleCommission - item?.SaleReturnValue)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase("Grand Total: ", fontArial8Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleQuantity)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleReturnQuantity)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase((list.Sum(x => x.SaleQuantity) - list.Sum(x => x.SaleReturnQuantity))?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleAmount)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleCommission)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DepoCharge)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransportationCost)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase((list.Sum(x => x.SaleAmount + x.DepoCharge + x.TransportationCost - x.SaleCommission))?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleReturnValue)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalMonthWiseReportData.AddCell(new PdfPCell(new Phrase((list.Sum(x => x.SaleAmount + x.DepoCharge + x.TransportationCost - x.SaleCommission - x.SaleReturnValue))?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return salesTotalMonthWiseReportData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesTotalDateWiseReportToPdf(MemoryStream stream, List<SalesTotalDateWiseViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);


            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringByDateTable(fontArial9, fromDate, toDate);
            var dataTable = SalesTotalDateWiseReportDataTable(list, fontArial7, fontArial7Bold);
            var footer = FooterForPreparedCheckedApproved(fontArial7, fontArial9Bold);
            var spaceTable = SpaceTable(fontArial7);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(footer);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable SalesTotalDateWiseReportDataTable(List<SalesTotalDateWiseViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            PdfPTable salesTotalDateWiseReportData = new(12);
            float[] widthsCellsHeaderPage = new float[] { 4f, 8f, 9f, 7f, 9f, 10f, 10f, 8f, 7f, 10f, 8f, 10f };
            salesTotalDateWiseReportData.SetWidths(widthsCellsHeaderPage);
            salesTotalDateWiseReportData.WidthPercentage = 100;
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Date", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Sale Quantity", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Sale Amount", fontArial7Bold)) { Colspan = 7, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Return", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Net", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Total Discount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Depo Charge", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Trans. Cost", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Sale", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Return", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Net", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            var sl = 0;
            salesTotalDateWiseReportData.HeaderRows = 2;
            foreach (var item in list)
            {
                sl++;
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(item?.Date.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleQuantity?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleReturnQuantity?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase((item?.SaleQuantity - item?.SaleReturnQuantity)?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleAmount?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleCommission?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(item?.DepoCharge?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(item?.TransportationCost?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase((item?.SaleAmount + item?.DepoCharge + item?.TransportationCost - item?.SaleCommission)?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(item?.SaleReturnValue?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase((item?.SaleAmount + item?.DepoCharge + item?.TransportationCost - item?.SaleCommission - item?.SaleReturnValue)?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase("Grand Total: ", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleQuantity)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleReturnQuantity)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase((list.Sum(x => x.SaleQuantity) - list.Sum(x => x.SaleReturnQuantity))?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleAmount)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleCommission)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DepoCharge)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransportationCost)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleAmount + x.DepoCharge + x.TransportationCost - x.SaleCommission)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleReturnValue)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesTotalDateWiseReportData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleAmount + x.DepoCharge + x.TransportationCost - x.SaleCommission - x.SaleReturnValue)?.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return salesTotalDateWiseReportData;
        }

        //Receive Payment
        public async Task PrintReceivePaymentReportToPdf(MemoryStream stream, List<ReceivePaymentViewModel> receivePaymentViewModels, string reportTitle, bool isDetails, ReceivePaymentRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (receivePaymentViewModels == null)
                throw new ArgumentNullException(nameof(receivePaymentViewModels));

            //InitializeDocument
            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;

            document.Open();

            //Fonts
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Gray = FontFactory.GetFont("Arial", 7, BaseColor.Gray);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            //Add header table
            var headerTable = await AddReceivePaymentHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var receivePaymentData = ReceivePaymentDataTable(receivePaymentViewModels, isDetails, fontArial8, fontArial7, fontArial7Bold, fontArial7Gray);

            document.Add(headerTable);
            document.Add(receivePaymentData);

            document.Close();
        }

        private async Task<PdfPTable> AddReceivePaymentHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, ReceivePaymentRequestModel request)
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
            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Customer: " + customerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.CostCenterId.HasValue)
            {
                var costCenterName = _unitOfWork.Repository<CostCenter>().TableNoTracking().FirstOrDefault(x => x.Id == request.CostCenterId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Cost Center: " + costCenterName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.PaymentModeId.HasValue)
            {
                var paymentModeName = _unitOfWork.Repository<Core.Entities.PaymentMode>().TableNoTracking().FirstOrDefault(x => x.Id == request.PaymentModeId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Payment Mode: " + paymentModeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        private PdfPTable ReceivePaymentDataTable(List<ReceivePaymentViewModel> receivePaymentViewModels, bool isDetails, Font fontArial8, Font fontArial7, Font fontArial7Bold, Font fontArial7Gray)
        {
            PdfPTable receivePaymentData = new(8);
            float[] widthCellsHeaderPage = new float[] { 5f, 10f, 10f, 30f, 15f, 10f, 10f, 10f };
            receivePaymentData.SetWidths(widthCellsHeaderPage);
            receivePaymentData.WidthPercentage = 100;
            receivePaymentData.HeaderRows = 1;

            receivePaymentData.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            receivePaymentData.AddCell(new PdfPCell(new Phrase("Code", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            receivePaymentData.AddCell(new PdfPCell(new Phrase("Payment Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            receivePaymentData.AddCell(new PdfPCell(new Phrase("Customer", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            receivePaymentData.AddCell(new PdfPCell(new Phrase("Cost Center", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            receivePaymentData.AddCell(new PdfPCell(new Phrase("Payment Mode", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            receivePaymentData.AddCell(new PdfPCell(new Phrase("Remark", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            receivePaymentData.AddCell(new PdfPCell(new Phrase("Total Amount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in receivePaymentViewModels)
            {
                sl++;
                receivePaymentData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                receivePaymentData.AddCell(new PdfPCell(new Phrase(item?.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                receivePaymentData.AddCell(new PdfPCell(new Phrase(item?.PaymentDate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                receivePaymentData.AddCell(new PdfPCell(new Phrase(item?.Customer?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                receivePaymentData.AddCell(new PdfPCell(new Phrase(item?.CostCenter?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                receivePaymentData.AddCell(new PdfPCell(new Phrase(item?.PaymentMode?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                receivePaymentData.AddCell(new PdfPCell(new Phrase(item?.Remark, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                receivePaymentData.AddCell(new PdfPCell(new Phrase(item?.TotalAmount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(4);
                    float[] widthsCellsProductDetailsTable = new float[] { 5f, 25f, 60f, 12f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Account", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Account Tree", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

                    var miniSL = 0;
                    foreach (var miniItem in item?.ReceivePaymentDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Account?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.AccountDescription, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    receivePaymentData.AddCell(new PdfPCell(new Phrase("Money Receipt Details", fontArial7Gray)) { Colspan = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial8);
                    productDetailsCell2.Colspan = 6;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    receivePaymentData.AddCell(productDetailsCell2);
                }
            }

            receivePaymentData.AddCell(new PdfPCell(new Phrase("Total:", fontArial7Bold)) { Colspan = 7, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            receivePaymentData.AddCell(new PdfPCell(new Phrase(receivePaymentViewModels.Sum(x => x.TotalAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return receivePaymentData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerLedgerFeedWiseReportToPdfAsync(MemoryStream stream, List<CustomerLedgerFeedWiseViewModel> list, string headerText, CustomerLedgerFeedWiseRequestModel requestModel)
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
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = await FilteringDataTable(fontArial9, requestModel);
            var dataTable = CustomerLedgerFeedWiseDataTable(fontArial7, fontArial7Bold, list);
            var footerTable = FooterForPreparedCheckedApproved(fontArial7, fontArial9Bold);
            var spaceTable = SpaceTable(fontArial7);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(footerTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public async Task<PdfPTable> FilteringDataTable(Font fontArial9, CustomerLedgerFeedWiseRequestModel requestModel)
        {
            var account = await _unitOfWork.Repository<Account>().FindAsync(requestModel.AccountId);
            PdfPTable filteringTable = new(1);
            float[] widthsCellsFilteringTable = new float[] { 100f };
            filteringTable.WidthPercentage = 100;
            filteringTable.SetWidths(widthsCellsFilteringTable);
            if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
            {
                filteringTable.AddCell(new PdfPCell(new Phrase("Date:                            " + requestModel.FromDate?.ToString("dd/MM/yyy") + " to " + requestModel.ToDate?.ToString("dd/MM/yyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
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
            filteringTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 0 });
            return filteringTable;
        }

        public PdfPTable CustomerLedgerFeedWiseDataTable(Font fontArial8, Font fontArial8Bold, IList<CustomerLedgerFeedWiseViewModel> list)
        {
            var groupedData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
            PdfPTable customerLedgerFeedWiseData = new(17);
            float[] widthsCellsCustomerLedgerFeedWiseDataTable = new float[] { 13f, 9f, 9f, 7f, 5f, 5f, 5f, 5f, 5f, 5f, 7f, 7f, 7f, 7f, 7f, 7f, 7f };
            customerLedgerFeedWiseData.WidthPercentage = 100;
            customerLedgerFeedWiseData.SetWidths(widthsCellsCustomerLedgerFeedWiseDataTable);
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Description", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("DN No", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Qty", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { Colspan = 6, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Offer", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Other", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Net", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Offer", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Other", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Net", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Depo Charge", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.HeaderRows = 2;
            foreach (var productTypeData in groupedData)
            {
                var productData = productTypeData.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Key!.ToString() + " FEED", fontArial8Bold)) { Colspan = 17, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                foreach (var product in productData)
                {
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial8Bold)) { Colspan = 17, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                    foreach (var item in product)
                    {
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.InvoiceNo, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.DeliveryNoteNo, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.Date?.ToString("dd/MM/yyyy"), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.CommissionRate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.OfferDiscountPerUnit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.OtherDiscountPerUnit.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.NetRate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.RateAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.CommissionAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.OfferDiscountAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.OtherDiscountAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(item.NetRateAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase((item.Quantity * item.DepoChargePerKg).ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase((item.NetRateAmount + (item.Quantity * item.DepoChargePerKg)).ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    }
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Quantity).ToString("#,##0"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.RateAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.CommissionAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.OfferDiscountAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.OtherDiscountAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.NetRateAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Quantity * x.DepoChargePerKg).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.NetRateAmount + (x.Quantity * x.DepoChargePerKg)).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                }
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Key + " FEED Total : ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.Quantity).ToString("#,##0"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.RateAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.CommissionAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.OfferDiscountAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.OtherDiscountAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.NetRateAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.Quantity * x.DepoChargePerKg).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.NetRateAmount + (x.Quantity * x.DepoChargePerKg)).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            }
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Quantity).ToString("#,##0"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.RateAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.CommissionAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OfferDiscountAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OtherDiscountAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.NetRateAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Quantity * x.DepoChargePerKg).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            customerLedgerFeedWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.NetRateAmount + (x.Quantity * x.DepoChargePerKg)).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });

            return customerLedgerFeedWiseData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintTransitSalesReportToPdfAsync(MemoryStream stream, List<SalesOrderItemWithoutDeliveryItemViewModel> list, string headerText, SalesOrderItemRequestModel request)
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
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddTransitSalesReportFilteringTable(fontArial9, request);
            var dataTable = TransitSalesDataTable(fontArial8, fontArial8Bold, list);
            var footerTable = FooterForPreparedCheckedApproved(fontArial8, fontArial9Bold);
            var spaceTable = SpaceTable(fontArial8);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(footerTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable TransitSalesDataTable(Font fontArial8, Font fontArial8Bold, List<SalesOrderItemWithoutDeliveryItemViewModel> list)
        {
            var groupedData = list.GroupBy(x => x.ProductName).ToList().OrderBy(x => x.Key);
            PdfPTable transitSalesDataTable = new(5);
            float[] widthCellTransitSalesDataTable = new float[] { 10f, 35f, 10f, 22f, 22f };
            transitSalesDataTable.SetWidths(widthCellTransitSalesDataTable);
            transitSalesDataTable.WidthPercentage = 100;

            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { Rowspan = 2, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Bold)) { Rowspan = 2, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { Colspan = 2, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { Rowspan = 2, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Bag", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("KG", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            transitSalesDataTable.HeaderRows = 2;
            var sl = 0;
            foreach (var item in groupedData)
            {
                sl++;
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Key, fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.OrderedPrimaryQuantity - x.DeliveredPrimaryQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.OrderedQuantity - x.DeliveredQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => (x.OrderedQuantity - x.DeliveredQuantity) * x.Rate)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            }
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 2, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OrderedPrimaryQuantity - x.DeliveredPrimaryQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OrderedQuantity - x.DeliveredQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => (x.OrderedQuantity - x.DeliveredQuantity) * x.Rate)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });

            return transitSalesDataTable;
        }

        public PdfPTable AddTransitSalesReportFilteringTable(Font fontArial9, SalesOrderItemRequestModel request)
        {
            PdfPTable filteringTable = new(1);
            float[] widthsCellsFilteringTable = new float[] { 100f };
            filteringTable.SetWidths(widthsCellsFilteringTable);
            filteringTable.WidthPercentage = 100;
            if (request.ToDate.HasValue && request.FromDate.HasValue)
            {
                filteringTable.AddCell(new PdfPCell(new Phrase("Date:                        " + request.FromDate?.ToString("dd/MM/yyy") + " to " + request.ToDate?.ToString("dd/MM/yyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            else
            {
                filteringTable.AddCell(new PdfPCell(new Phrase("Date:                        " + request.ToDate?.ToString("dd/MM/yyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            if (request.CustomerId.HasValue)
            {
                var account = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId);
                filteringTable.AddCell(new PdfPCell(new Phrase("Customer Code:       " + account?.Code, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
                filteringTable.AddCell(new PdfPCell(new Phrase("Customer:                " + account?.Name, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            if (request.StoreId.HasValue)
            {
                var store = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId);
                filteringTable.AddCell(new PdfPCell(new Phrase("Store:                       " + store?.Name, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId)?.FullName;
                filteringTable.AddCell(new PdfPCell(new Phrase("Marketing Officer:    " + marketingOfficerName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            if (request.CustomerZoneId.HasValue)
            {
                var zoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)?.Name;
                filteringTable.AddCell(new PdfPCell(new Phrase("Zone:                       " + zoneName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }
            if (request.CustomerAreaId.HasValue)
            {
                var areaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerAreaId)?.Name;
                filteringTable.AddCell(new PdfPCell(new Phrase("Area:                        " + areaName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, HorizontalAlignment = 0 });
            }

            filteringTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 0 });
            return filteringTable;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSalesOrderHistoryReportToPdfAsync(MemoryStream stream, List<SalesOrderItemWithoutDeliveryItemViewModel> list, string headerText, SalesOrderItemRequestModel request)
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
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddTransitSalesReportFilteringTable(fontArial9, request);
            var dataTable = SalesOrderHistoryDataTable(fontArial8, fontArial8Bold, list);
            var footerTable = FooterForPreparedCheckedApproved(fontArial8, fontArial9Bold);
            var spaceTable = SpaceTable(fontArial8);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(footerTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable SalesOrderHistoryDataTable(Font fontArial8, Font fontArial8Bold, List<SalesOrderItemWithoutDeliveryItemViewModel> list)
        {
            var groupedData = list.GroupBy(x => x.ProductName).ToList().OrderBy(x => x.Key);
            PdfPTable transitSalesDataTable = new(14);
            float[] widthCellTransitSalesDataTable = new float[] { 4f, 15f, 6f, 6f, 8f, 6f, 6f, 8f, 6f, 6f, 8f, 6f, 6f, 8f };
            transitSalesDataTable.SetWidths(widthCellTransitSalesDataTable);
            transitSalesDataTable.WidthPercentage = 100;

            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { Rowspan = 2, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Bold)) { Rowspan = 2, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Delivery Quantity & Amount", fontArial8Bold)) { Colspan = 3, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Transit Quantity & Amount", fontArial8Bold)) { Colspan = 3, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Cancel Quantity & Amount", fontArial8Bold)) { Colspan = 3, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Total Quantity & Amount", fontArial8Bold)) { Colspan = 3, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Bag", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("KG", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Bag", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("KG", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Bag", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("KG", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Bag", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("KG", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            transitSalesDataTable.HeaderRows = 2;
            var sl = 0;
            foreach (var item in groupedData)
            {
                sl++;
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Key, fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.DeliveredPrimaryQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.DeliveredQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.DeliveredQuantity * x.Rate)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.OrderedPrimaryQuantity - x.DeliveredPrimaryQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.OrderedQuantity - x.DeliveredQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => (x.OrderedQuantity - x.DeliveredQuantity) * x.Rate)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.CancelPrimaryQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.CancelQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.CancelQuantity * x.Rate)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.OrderedPrimaryQuantity + x.CancelPrimaryQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.OrderedQuantity + x.CancelQuantity)?.ToString("#,##0"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
                transitSalesDataTable.AddCell(new PdfPCell(new Phrase(item.Sum(x => (x.OrderedQuantity + x.CancelQuantity) * x.Rate)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            }
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 2, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DeliveredPrimaryQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DeliveredQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DeliveredQuantity * x.Rate)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OrderedPrimaryQuantity - x.DeliveredPrimaryQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OrderedQuantity - x.DeliveredQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => (x.OrderedQuantity - x.DeliveredQuantity) * x.Rate)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.CancelPrimaryQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.CancelQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.CancelQuantity * x.Rate)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OrderedPrimaryQuantity + x.CancelPrimaryQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OrderedQuantity + x.CancelQuantity)?.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });
            transitSalesDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => (x.OrderedQuantity + x.CancelQuantity) * x.Rate)?.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 2 });

            return transitSalesDataTable;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimarySaleTotalProductWiseReportToPdf(MemoryStream stream, List<SalesItemDetailViewModel> list, string headerText, SalesItemDetailRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

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
            var dataTable = PrimarySaleTotalProductWiseDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable PrimarySaleTotalProductWiseDataTable(List<SalesItemDetailViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            var productTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();

            PdfPTable salesItemWiseCustomerData = new(17);
            float[] widthsCellsHeaderPage = new float[] { 4f, 5f, 15f, 7f, 8f, 8f, 12f, 8f, 12f, 8f, 8f, 12f, 12f, 8f, 8f, 12f, 10f };
            salesItemWiseCustomerData.SetWidths(widthsCellsHeaderPage);
            salesItemWiseCustomerData.WidthPercentage = 100;
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Product Description", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Trade Price", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Dispatch Quantity", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Dispatch Value", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Bonus Quantity", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Discount Amount", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Return", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Net Bonus Quantity", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Sold Quantity", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Sold TP", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Total Vat", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Code", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Dispatch Quantity", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Bonus Quantity", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("TP", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Discount Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesItemWiseCustomerData.HeaderRows = 2;
            var sl = 0;
            foreach (var productType in productTypeData)
            {
                var productData = productType.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Key!.ToString(), fontArial7Bold)) { Colspan = 17, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var product in productData)
                {
                    sl++;
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.First().ProductCode, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.First().ProductName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.First().PackSize, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.DispatchQuantity) == 0 ? "0.00" : (product.Sum(x => x.DispatchValue) / product.Sum(x => x.DispatchQuantity)).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.DispatchQuantity).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.BonusQuantity).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.InvoiceDiscountAmount).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.ReturnDispatchQuantity).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.ReturnBonusQuantity).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.ReturnDiscountAmount).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.BonusQuantity - x.ReturnBonusQuantity).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.TotalVat).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Key + ", Total : ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.DispatchQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.BonusQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.InvoiceDiscountAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ReturnDispatchQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ReturnBonusQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ReturnDiscountAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.BonusQuantity - x.ReturnBonusQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.TotalVat).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.BonusQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.InvoiceDiscountAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ReturnDispatchQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ReturnBonusQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ReturnDiscountAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.BonusQuantity - x.ReturnBonusQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TotalVat).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return salesItemWiseCustomerData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimarySaleTotalProductWiseMultiTerritoryReportToPdf(MemoryStream stream, List<SalesItemDetailViewModel> list, string headerText, SalesItemDetailRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            //Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);

            // Territory list
            var territoryCount = list
                .Select(x => x.TerritoryId)
                .Distinct()
                .Count();

            // For each column approx width
            float columnWidth = 50f; // px per column
            int fixedColumns = 7; // First 5 col + R Sold Qty + R Sold TP
            float minPageWidth = PageSize.A4.Height; // Landscape A4 width
            float calculatedWidth = (fixedColumns + territoryCount) * columnWidth;

            // Minimum A4 width
            float finalWidth = Math.Max(minPageWidth, calculatedWidth);

            Rectangle rectangle = new Rectangle(finalWidth, PageSize.A4.Width);

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
            var dataTable = PrimarySaleTotalProductWiseMultiTerritoryDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable PrimarySaleTotalProductWiseMultiTerritoryDataTable(List<SalesItemDetailViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            // Step 1: All territory
            var territories = list.Select(x => x.TerritoryId).Distinct().OrderBy(x => x).ToList();

            // Step 2: Column count = Base columns (SL, Code, Name, PackSize, TradePrice) + territory columns + R Sold Qty + R Sold TP
            int baseColumnCount = 5;
            int territoryColumnCount = territories.Count;
            int totalColumns = baseColumnCount + territoryColumnCount + 2;

            // Step 3: Table define
            PdfPTable table = new(totalColumns);
            float[] columnWidths = new float[totalColumns];
            columnWidths[0] = 3f; // SL
            columnWidths[1] = 3f; // Code
            columnWidths[2] = 5f; // Name
            columnWidths[3] = 3f; // Pack Size
            columnWidths[4] = 4f; // Trade Price

            // Each territory column width
            for (int i = 0; i < territoryColumnCount; i++)
                columnWidths[baseColumnCount + i] = 4f;

            // R Sold Qty
            columnWidths[baseColumnCount + territoryColumnCount] = 4f;
            // R Sold TP
            columnWidths[baseColumnCount + territoryColumnCount + 1] = 4f;

            table.SetWidths(columnWidths);
            table.WidthPercentage = 100;

            // Step 4: Header Row 1
            table.AddCell(new PdfPCell(new Phrase("Product Description", fontArial7Bold))
            { Colspan = baseColumnCount, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            table.AddCell(new PdfPCell(new Phrase("Territory", fontArial7Bold)) { Colspan = territoryColumnCount, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            table.AddCell(new PdfPCell(new Phrase("Sold Quantity", fontArial7Bold))
            { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            table.AddCell(new PdfPCell(new Phrase("Sold TP", fontArial7Bold))
            { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            // Step 5: Header Row 2
            table.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            table.AddCell(new PdfPCell(new Phrase("Code", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            table.AddCell(new PdfPCell(new Phrase("Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            table.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            table.AddCell(new PdfPCell(new Phrase("Trade Price", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            foreach (var territory in territories)
            {
                var territoryName = _unitOfWork.Repository<Territory>().TableNoTracking().FirstOrDefault(x => x.Id == territory)?.Name;
                table.AddCell(new PdfPCell(new Phrase(territoryName, fontArial7Bold))
                { Rowspan = 1, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            }

            table.HeaderRows = 2;

            // Step 6: Data Rows
            var productTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
            int sl = 0;

            // Here details row will be added by productType.Key
            foreach (var productType in productTypeData)
            {
                // Product type title row
                table.AddCell(new PdfPCell(new Phrase(productType.Key, fontArial7Bold)) { Colspan = totalColumns, HorizontalAlignment = 1, PaddingTop = 3, PaddingBottom = 3 });

                var productData = productType.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();

                foreach (var product in productData)
                {
                    sl++;
                    var first = product.First();

                    // SL, Code, Name, PackSize, TradePrice
                    table.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    table.AddCell(new PdfPCell(new Phrase(first.ProductCode, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    table.AddCell(new PdfPCell(new Phrase(first.ProductName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    table.AddCell(new PdfPCell(new Phrase(first.PackSize, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    table.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.DispatchQuantity) == 0
                        ? "0.00"
                        : (product.Sum(x => x.DispatchValue) / product.Sum(x => x.DispatchQuantity)).ToString("#,##0.00"), fontArial7))
                    { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    // For each territory sold qty
                    foreach (var territory in territories)
                    {
                        var terrData = product.Where(x => x.TerritoryId == territory);
                        var qty = terrData.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);
                        table.AddCell(new PdfPCell(new Phrase(qty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    // R Sold Qty total
                    var totalQty = product.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);
                    table.AddCell(new PdfPCell(new Phrase(totalQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    // R Sold TP total
                    var totalTP = product.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount);
                    table.AddCell(new PdfPCell(new Phrase(totalTP.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }

                // === Product Type Total Row ===
                table.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial7Bold))
                {
                    Colspan = 5,
                    PaddingTop = 3,
                    PaddingBottom = 3,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });



                foreach (var territory in territories) // territories is  dynamic Territory List
                {
                    var terrData = productType.Where(x => x.TerritoryId == territory);
                    var totalSoldQtyForTerritory = terrData.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);
                    //var totalSoldQtyForTerritory = list
                    //    .Where(x => x.TerritoryId == territory)
                    //    .Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);

                    table.AddCell(new PdfPCell(new Phrase(totalSoldQtyForTerritory.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                // Sold Qty
                table.AddCell(new PdfPCell(new Phrase(
                    productType.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity).ToString("#,##0"),
                    fontArial7Bold))
                {
                    PaddingTop = 3,
                    PaddingBottom = 3,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });

                // Sold TP
                table.AddCell(new PdfPCell(new Phrase(
                    productType.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount).ToString("#,##0.00"),
                    fontArial7Bold))
                {
                    PaddingTop = 3,
                    PaddingBottom = 3,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });

            }

            // Grand Total row
            table.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold))
            { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            // For Each Territory : Grand Total Sold Qty
            foreach (var territory in territories) //territories is  dynamic Territory List
            {
                var totalSoldQtyForTerritory = list
                    .Where(x => x.TerritoryId == territory)
                    .Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);

                table.AddCell(new PdfPCell(new Phrase(totalSoldQtyForTerritory.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            table.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity).ToString("#,##0"), fontArial7Bold))
            { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            table.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount).ToString("#,##0.00"), fontArial7Bold))
            { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return table;
        }


        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimarySaleTotalProductWiseMultiOfficerReportToPdf(MemoryStream stream, List<SalesItemDetailViewModel> list, string headerText, SalesItemDetailRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            //Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);

            // Territory list
            var territoryCount = list
                .Select(x => x.TerritoryId)
                .Distinct()
                .Count();

            // For each column approx width
            float columnWidth = 50f; // px per column
            int fixedColumns = 7; // First 5 col + R Sold Qty + R Sold TP
            float minPageWidth = PageSize.A4.Height; // Landscape A4 width
            float calculatedWidth = (fixedColumns + territoryCount) * columnWidth;

            // Minimum A4 width
            float finalWidth = Math.Max(minPageWidth, calculatedWidth);

            Rectangle rectangle = new Rectangle(finalWidth, PageSize.A4.Width);

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
            var dataTable = PrimarySaleTotalProductWiseMultiOfficerDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable PrimarySaleTotalProductWiseMultiOfficerDataTable(List<SalesItemDetailViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            // Step 1: All Marketing Officers
            var officers = list.Select(x => x.MarketingOfficerId).Distinct().OrderBy(x => x).ToList();

            // Step 2: Column count = Base columns (SL, Code, Name, PackSize, TradePrice) + territory columns + R Sold Qty + R Sold TP
            int baseColumnCount = 5;
            int officerColumnCount = officers.Count;
            int totalColumns = baseColumnCount + officerColumnCount + 2;

            // Step 3: Table define
            PdfPTable table = new(totalColumns);
            float[] columnWidths = new float[totalColumns];
            columnWidths[0] = 3f; // SL
            columnWidths[1] = 3f; // Code
            columnWidths[2] = 5f; // Name
            columnWidths[3] = 3f; // Pack Size
            columnWidths[4] = 4f; // Trade Price

            // Each territory column width
            for (int i = 0; i < officerColumnCount; i++)
                columnWidths[baseColumnCount + i] = 4f;

            // R Sold Qty
            columnWidths[baseColumnCount + officerColumnCount] = 4f;
            // R Sold TP
            columnWidths[baseColumnCount + officerColumnCount + 1] = 4f;

            table.SetWidths(columnWidths);
            table.WidthPercentage = 100;

            // Step 4: Header Row 1
            table.AddCell(new PdfPCell(new Phrase("Product Description", fontArial7Bold))
            { Colspan = baseColumnCount, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            table.AddCell(new PdfPCell(new Phrase("Marketing Officer", fontArial7Bold)) { Colspan = officerColumnCount, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            table.AddCell(new PdfPCell(new Phrase("Sold Quantity", fontArial7Bold))
            { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            table.AddCell(new PdfPCell(new Phrase("Sold TP", fontArial7Bold))
            { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            // Step 5: Header Row 2
            table.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            table.AddCell(new PdfPCell(new Phrase("Code", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            table.AddCell(new PdfPCell(new Phrase("Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            table.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            table.AddCell(new PdfPCell(new Phrase("Trade Price", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            foreach (var officer in officers)
            {
                var officerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == officer)?.FullName;
                table.AddCell(new PdfPCell(new Phrase(officerName, fontArial7Bold))
                { Rowspan = 1, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            }

            table.HeaderRows = 2;

            // Step 6: Data Rows
            var productTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
            int sl = 0;

            // Here details row will be added by productType.Key
            foreach (var productType in productTypeData)
            {
                // Product type title row
                table.AddCell(new PdfPCell(new Phrase(productType.Key, fontArial7Bold)) { Colspan = totalColumns, HorizontalAlignment = 1, PaddingTop = 3, PaddingBottom = 3 });

                var productData = productType.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();

                foreach (var product in productData)
                {
                    sl++;
                    var first = product.First();

                    // SL, Code, Name, PackSize, TradePrice
                    table.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    table.AddCell(new PdfPCell(new Phrase(first.ProductCode, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    table.AddCell(new PdfPCell(new Phrase(first.ProductName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    table.AddCell(new PdfPCell(new Phrase(first.PackSize, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    table.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.DispatchQuantity) == 0
                        ? "0.00"
                        : (product.Sum(x => x.DispatchValue) / product.Sum(x => x.DispatchQuantity)).ToString("#,##0.00"), fontArial7))
                    { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    // For each territory sold qty
                    foreach (var officer in officers)
                    {
                        var officerData = product.Where(x => x.MarketingOfficerId == officer);
                        var qty = officerData.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);
                        table.AddCell(new PdfPCell(new Phrase(qty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    // R Sold Qty total
                    var totalQty = product.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);
                    table.AddCell(new PdfPCell(new Phrase(totalQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                    // R Sold TP total
                    var totalTP = product.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount);
                    table.AddCell(new PdfPCell(new Phrase(totalTP.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }

                // === Product Type Total Row ===
                table.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial7Bold))
                {
                    Colspan = 5,
                    PaddingTop = 3,
                    PaddingBottom = 3,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });



                foreach (var officer in officers) // territories is  dynamic Territory List
                {
                    var officerData = productType.Where(x => x.MarketingOfficerId == officer);
                    var totalSoldQtyForOfficer = officerData.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);
                    //var totalSoldQtyForTerritory = list
                    //    .Where(x => x.TerritoryId == territory)
                    //    .Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);

                    table.AddCell(new PdfPCell(new Phrase(totalSoldQtyForOfficer.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                // Sold Qty
                table.AddCell(new PdfPCell(new Phrase(
                    productType.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity).ToString("#,##0"),
                    fontArial7Bold))
                {
                    PaddingTop = 3,
                    PaddingBottom = 3,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });

                // Sold TP
                table.AddCell(new PdfPCell(new Phrase(
                    productType.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount).ToString("#,##0.00"),
                    fontArial7Bold))
                {
                    PaddingTop = 3,
                    PaddingBottom = 3,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });

            }

            // Grand Total row
            table.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold))
            { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            // For Each Territory : Grand Total Sold Qty
            foreach (var officer in officers) //territories is  dynamic Territory List
            {
                var totalSoldQtyForOfficer = list
                    .Where(x => x.MarketingOfficerId == officer)
                    .Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity);

                table.AddCell(new PdfPCell(new Phrase(totalSoldQtyForOfficer.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            table.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchQuantity - x.ReturnDispatchQuantity).ToString("#,##0"), fontArial7Bold))
            { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            table.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchValue - x.InvoiceDiscountAmount - x.ReturnValue + x.ReturnDiscountAmount).ToString("#,##0.00"), fontArial7Bold))
            { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return table;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimaryCustomerWiseProductWiseSalesQuantityReportToPdf(MemoryStream stream, List<SalesItemDetailViewModel> list, string headerText, SalesItemDetailRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
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
            var dataTable = PrimaryCustomerWiseProductWiseSalesQuantityDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable PrimaryCustomerWiseProductWiseSalesQuantityDataTable(List<SalesItemDetailViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            var productData = list.GroupBy(x => new { x.ProductId, x.CustomerId, x.MarketingOfficerId }).OrderBy(x => x.Key.ProductId).ThenBy(x => x.Key.CustomerId).ToList();

            PdfPTable salesItemWiseCustomerData = new(8);
            float[] widthsCellsHeaderPage = new float[] { 3f, 12f, 13f, 6f, 8f, 6f, 3f, 4f };
            salesItemWiseCustomerData.SetWidths(widthsCellsHeaderPage);
            salesItemWiseCustomerData.WidthPercentage = 100;
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Customer Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Address", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Territory Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Officer Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Product Name", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesItemWiseCustomerData.HeaderRows = 1;
            var sl = 0;
            foreach (var product in productData)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == product.First().MarketingOfficerId)?.FullName;
                var territoryName = _unitOfWork.Repository<Territory>().TableNoTracking().FirstOrDefault(x => x.Id == product.First().TerritoryId)?.Name;
                sl++;
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.First().CustomerName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.First().CustomerAddress, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(territoryName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(marketingOfficerName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.First().ProductName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.First().PackSize, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.DispatchQuantity + x.BonusQuantity - x.ReturnDispatchQuantity - x.ReturnBonusQuantity).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            }
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial7Bold)) { Colspan = 7, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchQuantity + x.BonusQuantity - x.ReturnDispatchQuantity - x.ReturnBonusQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return salesItemWiseCustomerData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimarySaleAgingReportToPdf(MemoryStream stream, List<SalesAgingReportViewModel> list, string headerText, SalesItemDetailRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

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
            var dataTable = PrimarySaleAgingDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable PrimarySaleAgingDataTable(List<SalesAgingReportViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial7Red = FontFactory.GetFont("Arial", 7, BaseColor.Red);
            var regionData = list.GroupBy(x => x.RegionId).OrderBy(x => x.Key).ToList();
            PdfPTable salesAgingDataTable = new(11);
            float[] widthsCellsSalesAgingDataTable = new float[] { 6f, 10f, 8f, 9f, 9f, 11f, 11f, 11f, 11f, 11f, 11f };
            salesAgingDataTable.SetWidths(widthsCellsSalesAgingDataTable);
            salesAgingDataTable.WidthPercentage = 100;
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Sl No", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Invoice No", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Invoice Date", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Over Due Days", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Payment Mode", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Invoice Amount", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Return Amount", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Net Sale", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Paid Amount", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Total Due", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesAgingDataTable.HeaderRows = 1;
            var sl = 0;
            var grandTotalAdjustment = 0M;
            foreach (var region in regionData)
            {
                var regionAdjustment = 0M;
                var customerRegionName = _unitOfWork.Repository<Region>().TableNoTracking().FirstOrDefault(x => x.Id == region.Key)?.Name;
                var zoneData = region.GroupBy(x => x.ZoneId).OrderBy(x => x.Key).ToList();
                salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Zone: " + customerRegionName, fontArial8Bold)) { Colspan = 11, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
                foreach (var zone in zoneData)
                {
                    var zoneAdjustment = 0M;
                    var customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == zone.Key)?.Name;
                    var areaData = zone.GroupBy(x => x.AreaId).OrderBy(x => x.Key).ToList();
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Region: " + customerZoneName, fontArial8Bold)) { Colspan = 11, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                    foreach (var area in areaData)
                    {
                        var areaAdjustment = 0M;
                        var customerAreaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == area.Key)?.Name;
                        var territoryData = area.GroupBy(x => x.TerritoryId).OrderBy(x => x.Key).ToList();
                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Area: " + customerAreaName, fontArial8Bold)) { Colspan = 11, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                        foreach (var territory in territoryData)
                        {
                            var territoryAdjustment = 0M;
                            var customerTerritoryName = _unitOfWork.Repository<Territory>().TableNoTracking().FirstOrDefault(x => x.Id == territory.Key)?.Name;
                            var marketingOfficerData = territory.GroupBy(x => x.MarketingOfficerId).OrderBy(x => x.Key).ToList();
                            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Territory: " + customerTerritoryName, fontArial8Bold)) { Colspan = 11, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                            foreach (var marketingOfficer in marketingOfficerData)
                            {
                                var marketingOfficerAdjustment = 0M;
                                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == marketingOfficer.Key)?.FullName;
                                var customerData = marketingOfficer.GroupBy(x => x.CustomerId).OrderBy(x => x.Key).ToList();
                                salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer: " + marketingOfficerName, fontArial8Bold)) { Colspan = 11, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 0 });
                                foreach (var customer in customerData)
                                {
                                    var customerInfo = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == customer.Key);
                                    var customersJournalEntry = _unitOfWork.Repository<JournalEntryDetail>().TableNoTracking().Include(x => x.JournalEntry).Where(x => x.AccountId == customer.Key && x.JournalEntry.Status >= (int)JournalEntryStatus.Approved);
                                    var customerAdjustment = customersJournalEntry.Where(x => x.PostType == (int)PostType.Debit).Sum(x => x.Amount) - customersJournalEntry.Where(x => x.PostType == (int)PostType.Credit).Sum(x => x.Amount);
                                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Customer: " + customerInfo?.Name + " - " + customerInfo?.Code[^4..] + "  (" + customerInfo?.Address + ")", fontArial7Bold)) { Colspan = 11, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 0 });
                                    foreach (var item in customer)
                                    {
                                        if (item.DispatchValue - item.ReturnValue - item.Paid == 0) continue;
                                        sl++;
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.SaleInvoiceNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.InvoiceDate?.ToString("dd/MM/yyyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase((item.DispatchValue - item.ReturnValue - item.Paid) <= 0 ? "0" : item.OverDueDays.ToString(), ((item.DispatchValue - item.ReturnValue - item.Paid) > 0 && item.OverDueDays >= 30) ? fontArial7Red : fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(Enum.GetName(typeof(Core.Enums.PaymentTerm), item.PaymentTerm), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.DispatchValue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.ReturnValue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase((item.DispatchValue - item.ReturnValue).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.Paid.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase("0.00", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase((item.DispatchValue - item.ReturnValue - item.Paid).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    }
                                    marketingOfficerAdjustment += customerAdjustment;
                                    if (customerAdjustment != 0)
                                    {
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Adjustment: ", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase("0.00", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase("0.00", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase("0.00", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase("0.00", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customerAdjustment.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customerAdjustment.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    }
                                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customerInfo?.Name + ", Total:", fontArial7Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.DispatchValue - x.ReturnValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customer.Sum(x => x.Paid).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customerAdjustment.ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase((customer.Sum(x => x.DispatchValue - x.ReturnValue - x.Paid) + customerAdjustment).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                }
                                territoryAdjustment += marketingOfficerAdjustment;
                                salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer: " + marketingOfficerName + ", Total:", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.DispatchValue - x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficer.Sum(x => x.Paid).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficerAdjustment.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                                salesAgingDataTable.AddCell(new PdfPCell(new Phrase((marketingOfficer.Sum(x => x.DispatchValue - x.ReturnValue - x.Paid) + marketingOfficerAdjustment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            }
                            areaAdjustment += territoryAdjustment;
                            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Territory: " + customerTerritoryName + ", Total:", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.DispatchValue - x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(territory.Sum(x => x.Paid).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(territoryAdjustment.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                            salesAgingDataTable.AddCell(new PdfPCell(new Phrase((territory.Sum(x => x.DispatchValue - x.ReturnValue - x.Paid) + territoryAdjustment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        }
                        zoneAdjustment += areaAdjustment;
                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Area: " + customerAreaName + ", Total:", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.DispatchValue - x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(area.Sum(x => x.Paid).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase(areaAdjustment.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        salesAgingDataTable.AddCell(new PdfPCell(new Phrase((area.Sum(x => x.DispatchValue - x.ReturnValue - x.Paid) + areaAdjustment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    regionAdjustment += zoneAdjustment;
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Region: " + customerZoneName + ", Total:", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.DispatchValue - x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(zone.Sum(x => x.Paid).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(zoneAdjustment.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase((zone.Sum(x => x.DispatchValue - x.ReturnValue - x.Paid) + zoneAdjustment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                grandTotalAdjustment += regionAdjustment;
                salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Zone: " + customerRegionName + ", Total:", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.DispatchValue - x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(region.Sum(x => x.Paid).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesAgingDataTable.AddCell(new PdfPCell(new Phrase(regionAdjustment.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                salesAgingDataTable.AddCell(new PdfPCell(new Phrase((region.Sum(x => x.DispatchValue - x.ReturnValue - x.Paid) + regionAdjustment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Grand Total: ", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.DispatchValue - x.ReturnValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Paid).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase(grandTotalAdjustment.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase((list.Sum(x => x.DispatchValue - x.ReturnValue - x.Paid) + grandTotalAdjustment).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            return salesAgingDataTable;
        }
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimaryTemporarySaleAgingReportToPdf(MemoryStream stream, List<SalesAgingReportViewModel> list, string headerText, SalesItemDetailRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial6 = FontFactory.GetFont("Arial", 6);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddPrimarySubHeader(fontArial9, request);
            var dataTable = PrimaryTemporarySaleAgingDataTable(list, fontArial6, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable PrimaryTemporarySaleAgingDataTable(List<SalesAgingReportViewModel> list, Font fontArial6, Font fontArial7Bold)
        {
            var regionData = list.GroupBy(x => x.RegionId).OrderBy(x => x.Key).ToList();
            var regions = _unitOfWork.Repository<Region>().TableNoTracking().ToList();
            var zones = _unitOfWork.Repository<Zone>().TableNoTracking().ToList();
            var areas = _unitOfWork.Repository<Area>().TableNoTracking().ToList();
            var territories = _unitOfWork.Repository<Territory>().TableNoTracking().ToList();
            var marketingOfficers = _unitOfWork.Repository<Employee>().TableNoTracking().ToList();
            PdfPTable salesAgingDataTable = new(14);
            float[] widthsCellsSalesAgingDataTable = new float[] { 4f, 5f, 8f, 5f, 6f, 10f, 25f, 7f, 6f, 5f, 5f, 5f, 5f, 6f };
            salesAgingDataTable.SetWidths(widthsCellsSalesAgingDataTable);
            salesAgingDataTable.WidthPercentage = 100;
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Sl No", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Zone", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Region", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Area", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Territory", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Marketing Officer", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Customer", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Invoice No", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Invoice Date", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Payment Mode", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Invoice Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Return Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Paid Amount", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesAgingDataTable.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesAgingDataTable.HeaderRows = 1;
            var sl = 0;
            list = list.OrderBy(x => x.RegionId).ThenBy(x => x.ZoneId).ThenBy(x => x.AreaId).ThenBy(x => x.TerritoryId).ThenBy(x => x.MarketingOfficerId).ToList();
            var customerData = list.GroupBy(x => x.CustomerId).ToList();
            foreach (var customer in customerData)
            {
                var adjustCount = 0;
                var customerInfo = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == customer.Key);
                var customersJournalEntry = _unitOfWork.Repository<JournalEntryDetail>().TableNoTracking().Include(x => x.JournalEntry).Where(x => x.AccountId == customer.Key && x.JournalEntry.Status >= (int)JournalEntryStatus.Approved);
                var customerAdjustment = customersJournalEntry.Where(x => x.PostType == (int)PostType.Debit).Sum(x => x.Amount) - customersJournalEntry.Where(x => x.PostType == (int)PostType.Credit).Sum(x => x.Amount);
                foreach (var item in customer)
                {
                    adjustCount++;
                    var regionName = regions.Where(x => x.Id == item.RegionId)?.FirstOrDefault()?.Name;
                    var zoneName = zones.Where(x => x.Id == item.ZoneId)?.FirstOrDefault()?.Name;
                    var areaName = areas.Where(x => x.Id == item.AreaId)?.FirstOrDefault()?.Name;
                    var territoryName = territories.Where(x => x.Id == item.TerritoryId)?.FirstOrDefault()?.Name;
                    var marketingOfficer = marketingOfficers.Where(x => x.Id == item.MarketingOfficerId)?.FirstOrDefault();
                    var marketingOfficerName = marketingOfficer?.FirstName + " " + marketingOfficer?.LastName;
                    sl++;
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(regionName, fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(zoneName, fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(areaName, fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(territoryName, fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(marketingOfficerName, fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(customerInfo?.Name + " - " + customerInfo?.Code[^4..] + "  (" + customerInfo?.Address + ")", fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.SaleInvoiceNo, fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.InvoiceDate?.ToString("dd/MM/yyyy"), fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(Enum.GetName(typeof(PaymentTerm), item.PaymentTerm), fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.DispatchValue.ToString("#,##0.00"), fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.ReturnValue.ToString("#,##0.00"), fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(item.Paid.ToString("#,##0.00"), fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    salesAgingDataTable.AddCell(new PdfPCell(new Phrase(adjustCount == 1 ? customerAdjustment.ToString("#,##0.00") : "0.00", fontArial6)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
            }

            return salesAgingDataTable;
        }

        public async Task PrintGatePassReportToPdfAsync(MemoryStream stream, Guid deliveryNoteId, string userName, string headerText)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            Rectangle rectangle = new(PageSize.A5);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(10f, 10f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 10.0F)));
            // Header Page
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Mobile: +8801313019130" + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(headerText, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
            document.Add(headerPage);

            // =====
            // Space
            // =====
            PdfPTable spaceTable = new(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 5f });

            document.Add(line);

            // =============
            // Delivery Note
            // =============
            var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d => d.Id == deliveryNoteId).FirstOrDefaultAsync();

            if (deliveryNote is not null)
            {
                PdfPTable tableDeliveryNote = new(4);
                float[] widthscellsTableDeliveryNote = new float[] { 19f, 39f, 19f, 23f };
                tableDeliveryNote.SetWidths(widthscellsTableDeliveryNote);
                tableDeliveryNote.WidthPercentage = 100;
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Challan No: ", fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.DeliveryNoteNo, fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Sale Order No: ", fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.SaleOrderNo, fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Customer Name: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.Customer?.Name, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Reference No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.ReferenceNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Address: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.Customer?.Address, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Delivery Date: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.DeliveryDate.ToLocal().ToString("dd/MM/yyyy"), fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.Customer?.ContactNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Store Location: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.Store?.Name, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Email: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.Customer?.Email, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Vehicle No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.TruckNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Delivery Place: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.DeliveryPlace, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Driver Name: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.DriverName, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Transportation By: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(Enum.GetName(typeof(Transport), deliveryNote!.Transport)?.Replace("_", " "), fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Driver Contact No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.DriverContactNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNote?.Remark, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableDeliveryNote);

                // Get Delivery Note Item
                var deliveryNoteItems = _unitOfWork.Repository<DeliveryNoteDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.DeliveryNoteId == deliveryNoteId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.BagWeight,
                                           d.DeliveryPrimaryQuantity,
                                           d.DeliveryQuantity,
                                       }).OrderBy(x => x.ProductName);

                if (deliveryNoteItems.Any())
                {
                    PdfPTable tableDeliveryNoteItems = new(7);
                    float[] widthsCellsDeliveryNoteItems = new float[] { 7f, 10f, 37f, 10f, 12f, 12f, 12f };
                    tableDeliveryNoteItems.SetWidths(widthsCellsDeliveryNoteItems);
                    tableDeliveryNoteItems.WidthPercentage = 100;
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8)) { Colspan = 5, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Bag Size", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Bag Qty", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var item in deliveryNoteItems)
                    {
                        sl++;
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.BagWeight.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.DeliveryPrimaryQuantity.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.DeliveryQuantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    }
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(deliveryNoteItems.Sum(x => x.DeliveryPrimaryQuantity).ToString(), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(deliveryNoteItems.Sum(x => x.DeliveryQuantity).ToString("#,##0"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });

                    document.Add(tableDeliveryNoteItems);

                    document.Add(spaceTable);

                    //NumberToWords Table
                    PdfPTable tableNumberToWords = new(2);
                    float[] widthsCellsTableNumberToWords = new float[] { 31f, 69f };
                    tableNumberToWords.WidthPercentage = 100;
                    tableNumberToWords.SetWidths(widthsCellsTableNumberToWords);
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase("Total Quantity (KG) In Words: ", fontArial8Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(deliveryNoteItems.Sum(x => x.DeliveryQuantity)), fontArial8)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                    document.Add(tableNumberToWords);
                    document.Add(spaceTable);
                    document.Add(spaceTable);

                }
                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // Signature
                PdfPTable tableUsers = new(3);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Delivered By", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Store Keeper/Officer (Dist.)", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Received By", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Disclaimer: This is a computer generated Delivery Note. No signature is required. If you have any query, Please call this number: 01313-019140", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 14f, PaddingBottom = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();
        }
    }
}

public class PDFFooterEvent : PdfPageEventHelper
{
    public string Username { get; set; }
    public PDFFooterEvent(string username)
    {
        this.Username = username;
    }
    Font FONT1 = new Font(Font.HELVETICA, 8, Font.BOLD);
    Font FONT2 = new Font(Font.HELVETICA, 8, Font.NORMAL);

    public override void OnEndPage(PdfWriter writer, Document document)
    {
        PdfContentByte canvas = writer.DirectContent;

        ColumnText.ShowTextAligned(
          canvas, Element.ALIGN_CENTER,
          new Phrase("Printed By: " + Username + ", Date: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), FONT2), 415, 16, 0);
        ColumnText.ShowTextAligned(
          canvas, Element.ALIGN_CENTER,
          new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", FONT2), 415, 6, 0);

        ColumnText.ShowTextAligned(
             canvas, Element.ALIGN_CENTER,
          new Phrase("Page: " + writer.PageNumber, FONT2), 810, 16, 0);
    }
}