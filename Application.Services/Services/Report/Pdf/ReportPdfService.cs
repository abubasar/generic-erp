using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Report;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Report;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;
using System.Dynamic;

namespace Application.Services.Services.Report.Pdf
{
    public class ReportPdfService : IReportPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        public ReportPdfService(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
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

        public PdfPTable FilteringDataTable(Font fontArial9, DateTime? fromDate, DateTime? toDate, Guid? storeId, Guid? productId, Guid? inventoryTypeId = null, Guid? productTypeId = null)
        {
            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 3f });
            if (fromDate.HasValue && toDate.HasValue)
                headerPage.AddCell(new PdfPCell(new Phrase("Report for the period of " + fromDate.Value.ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });

            if (storeId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == storeId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Store : " + storeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (inventoryTypeId.HasValue)
            {
                var inventoryTypeName = _unitOfWork.Repository<InventoryType>().TableNoTracking().FirstOrDefault(x => x.Id == inventoryTypeId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Inventory Type Name : " + inventoryTypeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (productTypeId.HasValue)
            {
                var productTypeName = _unitOfWork.Repository<ProductType>().TableNoTracking().FirstOrDefault(x => x.Id == productTypeId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Product Type : " + productTypeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (productId.HasValue)
            {
                var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == productId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Product : " + productName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 3f });

            return headerPage;
        }

        public PdfPTable SearchFilterTable(DateTime? fromDate, DateTime? toDate, Guid? storeId, Guid? productId, Guid? inventoryTypeId = null, Guid? productTypeId = null)
        {
            PdfPTable headerPage = new(1);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            if (fromDate.HasValue && toDate.HasValue)
                headerPage.AddCell(new PdfPCell(new Phrase("Report for the period of " + fromDate.Value.ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_LEFT });

            if (storeId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == storeId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Store : " + storeName, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (inventoryTypeId.HasValue)
            {
                var inventoryTypeName = _unitOfWork.Repository<InventoryType>().TableNoTracking().FirstOrDefault(x => x.Id == inventoryTypeId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Inventory Type Name : " + inventoryTypeName, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (productTypeId.HasValue)
            {
                var productTypeName = _unitOfWork.Repository<ProductType>().TableNoTracking().FirstOrDefault(x => x.Id == productTypeId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Product Type : " + productTypeName, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (productId.HasValue)
            {
                var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == productId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Product : " + productName, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_LEFT });
            }

            return headerPage;
        }
        public async Task<PdfPTable> PrintReportToPdfAsync(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, List<string>? numericColumnsForSum = null, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, Guid? storeId = null, Guid? productId = null, Guid? inventoryTypeId = null, Guid? productTypeId = null)
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
            pdfGenerator.AddTable(SearchFilterTable(fromDate, toDate, storeId, productId, inventoryTypeId, productTypeId));
            // Add the table
            var reportTableData = pdfGenerator.AddTable(tableData, headers, columnWidths, tableHeaderFont, tableDataFont, numericColumnsForSum);

            // Close the PDF document
            pdfGenerator.Close();

            return reportTableData;
        }
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportForExpandoObjectToPdfAsync(MemoryStream stream, List<object> tableData, string reportTitleName, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, Guid? storeId = null, Guid? productId = null, Guid? inventoryTypeId = null, Guid? productTypeId = null)
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
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(fromDate, toDate, storeId, productId, inventoryTypeId, productTypeId));
            // Add the table
            var dataTable = pdfGenerator.AddTableForExpando(tableData, tableHeaderFont, tableDataFont);

            // Close the PDF document
            pdfGenerator.Close();
            return (filterTable, dataTable);
        }
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportForExpandoObjectForAverageToPdfAsync(MemoryStream stream, List<object> tableData, string reportTitleName, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, Guid? storeId = null, Guid? productId = null, Guid? inventoryTypeId = null, Guid? productTypeId = null)
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
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(fromDate, toDate, storeId, productId, inventoryTypeId, productTypeId));
            // Add the table
            var dataTable = pdfGenerator.AddTableForExpandoForAverage(tableData, tableHeaderFont, tableDataFont);

            // Close the PDF document
            pdfGenerator.Close();
            return (filterTable, dataTable);
        }
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportForExpandoObject2ToPdfAsync(MemoryStream stream, Dictionary<string, List<ExpandoObject>> tableData, string reportTitleName, bool isLandscape = false, DateTime? fromDate = null, DateTime? toDate = null, Guid? storeId = null, Guid? productId = null)
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
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(fromDate, toDate, storeId, productId));
            // Add the table
            var dataTable = pdfGenerator.AddTableForExpando2(tableData, tableHeaderFont, tableDataFont);

            // Close the PDF document
            pdfGenerator.Close();

            return (filterTable, dataTable);
        }

        public async Task<PdfPTable> PrintItemStockLedgerReportToPdf(MemoryStream stream, List<ItemStockLedger> list, string reportTitleName, StockLedgerRequestModel request)
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
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportTitleName, fontArial13Bold, fontArial14Bold, fontArial10);
            var itemStockLedgerDataTable = ItemStockLedgerDataTable(list, fontArial9Bold, fontArial8, fontArial8Bold);
            var filteringDataTable = FilteringDataTable(fontArial9, request?.FromDate, request?.ToDate, request?.StoreId, request?.ProductId, request!.InventoryTypeId = null, request.ProductTypeId = null);
            document.Add(headerTable);
            document.Add(filteringDataTable);
            document.Add(itemStockLedgerDataTable);

            document.Close();
            return itemStockLedgerDataTable;
        }

        public PdfPTable ItemStockLedgerDataTable(List<ItemStockLedger> list, Font fontArial9Bold, Font fontArial8, Font fontArial8Bold)
        {
            PdfPTable itemStockLedgerData = new(9);
            float[] widthsCellsItemStockLedgerDataTable = new float[] { 5f, 8f, 37f, 10f, 10f, 10f, 10f, 10f, 10f };
            itemStockLedgerData.SetWidths(widthsCellsItemStockLedgerDataTable);
            itemStockLedgerData.WidthPercentage = 100;

            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("SL", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Date", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Description", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("In", fontArial9Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Out", fontArial9Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Value", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Value", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Value", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            itemStockLedgerData.HeaderRows = 2;
            var sl = 0;
            foreach (var item in list)
            {
                sl++;
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase(item?.Date, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase(item?.Description, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase(item?.InQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase((item?.InQty * item?.InRate)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase(item?.OutQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase((item?.OutQty * item?.OutRate)?.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase(item?.QtyBalance.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                itemStockLedgerData.AddCell(new PdfPCell(new Phrase(item?.ValueBalance.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.InQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.InQty * x.InRate).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OutQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OutQty * x.OutRate).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase(list.Last().QtyBalance.ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2, VerticalAlignment = Element.ALIGN_MIDDLE });
            itemStockLedgerData.AddCell(new PdfPCell(new Phrase(list.Last().ValueBalance.ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2, VerticalAlignment = Element.ALIGN_MIDDLE });
            return itemStockLedgerData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintFinishedGoodsStockLedgerToPdfAsync(MemoryStream stream, List<FinishedGoodsStockReportViewModel> list, string headerText, FinishedGoodsStockReportRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(headerText, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(request.FromDate, request.ToDate, request.StoreId, null, null, null));
            // Add data table
            var dataTable = pdfGenerator.AddTable(FinishedGoodsStockDataTable(list, fontArial7, fontArial7Bold, fontArial8Bold));

            // Close the PDF document
            pdfGenerator.Close();

            return (filterTable, dataTable);
        }

        private PdfPTable FinishedGoodsStockDataTable(List<FinishedGoodsStockReportViewModel> list, Font fontArial7, Font fontArial7Bold, Font fontArial8Bold)
        {
            var groupedStoreData = list.GroupBy(x => x.StoreName).OrderBy(x => x.Key).ToList();
            PdfPTable finishedGoodsStockData = new(13);
            float[] widthsCellFinishedGoodsStockData = new float[] { 3f, 9f, 3f, 6f, 6f, 6f, 6f, 5f, 6f, 6f, 5f, 6f, 4f };
            finishedGoodsStockData.SetWidths(widthsCellFinishedGoodsStockData);
            finishedGoodsStockData.WidthPercentage = 100;

            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Description", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Bag Size", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Opening", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Stock In", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Stock Out", fontArial8Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Production", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Transfer Receive", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Sale Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Sale", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Transfer Issue", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Bag", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            finishedGoodsStockData.HeaderRows = 2;
            var grandTotalBagQty = 0;

            foreach (var store in groupedStoreData)
            {
                var storeTotalBagQuantity = 0;
                var groupedProductTypeData = store.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Key, fontArial7Bold)) { Colspan = 13, PaddingTop = 3, PaddingBottom = 3, BackgroundColor = BaseColor.LightGray, HorizontalAlignment = 1 });
                foreach (var productType in groupedProductTypeData)
                {
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Key, fontArial7Bold)) { Colspan = 13, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var totalBagQty = 0;
                    foreach (var item in productType)
                    {
                        var bagQty = (int)item.ClosingQty / item.BagSize;
                        totalBagQty += bagQty;

                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.BagSize.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.OpeningQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.ProductionQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.TransferReceiveQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.SaleReturnQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.AdjustmentInQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.SaleQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.TransferIssueQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.AdjustmentOutQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.ClosingQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(bagQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.TransferReceiveQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.SaleReturnQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.SaleQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.TransferIssueQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(totalBagQty.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    storeTotalBagQuantity += totalBagQty;
                }
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Key + " Total : ", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.TransferReceiveQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.SaleReturnQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.SaleQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.TransferIssueQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(storeTotalBagQuantity.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grandTotalBagQty += storeTotalBagQuantity;
            }
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransferReceiveQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleReturnQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransferIssueQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(grandTotalBagQty.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return finishedGoodsStockData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimaryFinishedGoodsStockLedgerToPdfAsync(MemoryStream stream, List<PrimaryFinishedGoodsStockReportViewModel> list, string headerText, FinishedGoodsStockReportRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(headerText, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(request.FromDate, request.ToDate, request.StoreId, null, null, null));
            // Add data table
            var dataTable = pdfGenerator.AddTable(PrimaryFinishedGoodsStockDataTable(list, fontArial7, fontArial7Bold, fontArial8Bold));

            // Close the PDF document
            pdfGenerator.Close();

            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryFinishedGoodsStockDataTable(List<PrimaryFinishedGoodsStockReportViewModel> list, Font fontArial7, Font fontArial7Bold, Font fontArial8Bold)
        {
            var groupedStoreData = list.GroupBy(x => x.StoreName).OrderBy(x => x.Key).ToList();
            PdfPTable finishedGoodsStockData = new(18);
            float[] widthsCellFinishedGoodsStockData = new float[] { 3f, 8f, 4f, 4f, 5f, 5f, 5f, 5f, 4f, 4f, 4f, 5f, 5f, 5f, 5f, 4f, 4f, 6f };
            finishedGoodsStockData.SetWidths(widthsCellFinishedGoodsStockData);
            finishedGoodsStockData.WidthPercentage = 100;

            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Opening Quantity", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Stock In", fontArial8Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Stock Out", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Description", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Production", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Purchase", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Transfer Receive", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Sale Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Bonus Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Adjust.", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Sale", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Bonus Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Purchase Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Transfer Issue", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Adjust.", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("TP Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            finishedGoodsStockData.HeaderRows = 2;

            foreach (var store in groupedStoreData)
            {
                var groupedProductTypeData = store.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Key, fontArial7Bold)) { Colspan = 18, PaddingTop = 3, PaddingBottom = 3, BackgroundColor = BaseColor.LightGray, HorizontalAlignment = 1 });
                foreach (var productType in groupedProductTypeData)
                {
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Key, fontArial7Bold)) { Colspan = 18, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    foreach (var item in productType)
                    {
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.PackSize, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.TradePrice.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.OpeningQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.ProductionQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.PurchaseQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.TransferReceiveQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase((item.SaleReturnQty - item.ReturnBonusQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.ReturnBonusQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.AdjustmentInQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase((item.SaleQty - item.SaleBonusQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.SaleBonusQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.PurchaseRetrunQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.TransferIssueQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.AdjustmentOutQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.ClosingQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        finishedGoodsStockData.AddCell(new PdfPCell(new Phrase((item.ClosingQty * item.TradePrice).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.PurchaseQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.TransferReceiveQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.SaleReturnQty - x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.SaleQty - x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.PurchaseRetrunQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.TransferIssueQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ClosingQty * x.TradePrice).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Key + " Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.PurchaseQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.TransferReceiveQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.SaleReturnQty - x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.SaleQty - x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.PurchaseRetrunQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.TransferIssueQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ClosingQty * x.TradePrice).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.PurchaseQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransferReceiveQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleReturnQty - x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleQty - x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.PurchaseRetrunQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransferIssueQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty * x.TradePrice).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return finishedGoodsStockData;
        }
        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintPrimaryStockReportWholeToPdfAsync(MemoryStream stream, List<PrimaryFinishedGoodsStockReportViewModel> list, string headerText, FinishedGoodsStockReportRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(headerText, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(request.FromDate, request.ToDate, request.StoreId, null, null, null));
            // Add data table
            var dataTable = pdfGenerator.AddTable(PrimaryStockReportWholeDataTable(list, fontArial7, fontArial7Bold, fontArial8Bold));

            // Close the PDF document
            pdfGenerator.Close();

            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryStockReportWholeDataTable(List<PrimaryFinishedGoodsStockReportViewModel> list, Font fontArial7, Font fontArial7Bold, Font fontArial8Bold)
        {
            var groupedProductTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
            PdfPTable finishedGoodsStockData = new(16);
            float[] widthsCellFinishedGoodsStockData = new float[] { 3f, 8f, 4f, 4f, 5f, 5f, 5f, 4f, 4f, 4f, 5f, 5f, 5f, 4f, 4f, 6f };
            finishedGoodsStockData.SetWidths(widthsCellFinishedGoodsStockData);
            finishedGoodsStockData.WidthPercentage = 100;

            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Opening Quantity", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Stock In", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Stock Out", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Description", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Production", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Purchase", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Sale Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Bonus Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Adjust.", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Sale", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Bonus Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Purchase Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Adjust.", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("TP Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            finishedGoodsStockData.HeaderRows = 2;

            foreach (var productType in groupedProductTypeData)
            {
                var groupedProductData = productType.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Key, fontArial7Bold)) { Colspan = 17, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                foreach (var item in groupedProductData)
                {
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.First().Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.First().ProductName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.First().PackSize, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.First().TradePrice.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.PurchaseQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase((item.Sum(x => x.SaleReturnQty) - item.Sum(x => x.ReturnBonusQty)).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.ReturnBonusQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase((item.Sum(x => x.SaleQty) - item.Sum(x => x.SaleBonusQty)).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.SaleBonusQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.PurchaseRetrunQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(item.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    finishedGoodsStockData.AddCell(new PdfPCell(new Phrase((item.Sum(x => x.ClosingQty * x.TradePrice)).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.PurchaseQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.SaleReturnQty - x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.SaleQty - x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.PurchaseRetrunQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ClosingQty * x.TradePrice).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.PurchaseQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleReturnQty - x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ReturnBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleQty - x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleBonusQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.PurchaseRetrunQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            finishedGoodsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty * x.TradePrice).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return finishedGoodsStockData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintRawMaterialsStockLedgerToPdfAsync(MemoryStream stream, List<RawMaterialsStockReportViewModel> list, string headerText, RawMaterialsStockReportRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(headerText, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(request.FromDate, request.ToDate, request.StoreId, null, null, request.ProductTypeId));
            // Add data table
            var dataTable = pdfGenerator.AddTable(RawMaterialsStockDataTable(list, fontArial7, fontArial7Bold, fontArial8Bold));

            // Close the PDF document
            pdfGenerator.Close();

            return (filterTable, dataTable);
        }
        private PdfPTable RawMaterialsStockDataTable(List<RawMaterialsStockReportViewModel> list, Font fontArial7, Font fontArial7Bold, Font fontArial8Bold)
        {
            var groupedProductTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
            PdfPTable rawMaterialsStockData = new(12);
            float[] widthsCellRawMaterialsStockData = new float[] { 4f, 12f, 3f, 7f, 7f, 7f, 7f, 7f, 7f, 7f, 7f, 7f };
            rawMaterialsStockData.SetWidths(widthsCellRawMaterialsStockData);
            rawMaterialsStockData.WidthPercentage = 100;

            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Description", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Opening", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Stock In", fontArial8Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Stock Out", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Purchase", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Transfer Receive", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Issue", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Transfer Issue", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Purchase Return", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            rawMaterialsStockData.HeaderRows = 2;

            foreach (var productType in groupedProductTypeData)
            {
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Key, fontArial7Bold)) { Colspan = 12, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                foreach (var item in productType)
                {
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.MeasurementUnitName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.OpeningQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.PurchaseQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.TransferReceiveQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.AdjustmentInQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.IssueQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.TransferIssueQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.PurchaseReturnQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.AdjustmentOutQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(item.ClosingQty.ToString("#,##0.000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.OpeningQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.PurchaseQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.TransferReceiveQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.AdjustmentInQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.IssueQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.TransferIssueQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.PurchaseReturnQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.AdjustmentOutQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.PurchaseQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransferReceiveQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentInQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.IssueQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransferIssueQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.PurchaseReturnQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentOutQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase("Grand Total (Without Packaging Materials) : ", fontArial7Bold)) { Colspan = 3, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.OpeningQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.PurchaseQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.TransferReceiveQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.AdjustmentInQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.IssueQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.TransferIssueQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.PurchaseReturnQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.AdjustmentOutQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            rawMaterialsStockData.AddCell(new PdfPCell(new Phrase(list.Where(x => x.ProductTypeId != ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return rawMaterialsStockData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintStockDepotProductWiseDetailsReportToPdf(MemoryStream stream, List<StockLedgerReportLine> list, string headerText, StockLedgerRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(headerText, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(request.FromDate, request.ToDate, request.StoreId, request.ProductId, request.InventoryTypeId, request.ProductTypeId));
            // Add the table
            var dataTable = request.IsDepotWiseReport ? pdfGenerator.AddTable(StockDepotWiseDetailsDataTable(list, fontArial8, fontArial8Bold)) : pdfGenerator.AddTable(StockProductWiseDetailsDataTable(list, fontArial8, fontArial8Bold));

            // Close the PDF document
            pdfGenerator.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable StockDepotWiseDetailsDataTable(List<StockLedgerReportLine> list, Font fontArial8, Font fontArial8Bold)
        {
            var storeData = list.GroupBy(x => x.StoreName).OrderBy(x => x.Key).ToList();

            PdfPTable stockDepotWiseDetailsData = new(10);
            float[] widthsCellsHeaderPage = new float[] { 4f, 20f, 9f, 9f, 9f, 9f, 9f, 9f, 9f, 9f };
            stockDepotWiseDetailsData.SetWidths(widthsCellsHeaderPage);
            stockDepotWiseDetailsData.WidthPercentage = 100;
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("Item Name", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("Opening Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("Opening Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("In Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("In Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("Out Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("Out Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("Closing Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("Closing Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            stockDepotWiseDetailsData.HeaderRows = 1;
            foreach (var store in storeData)
            {
                var sl = 0;
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Key!.ToString(), fontArial8Bold)) { Colspan = 9, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var item in store)
                {
                    sl++;
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.ItemName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.OpeningQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.OpeningValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.InQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.InValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.OutQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.OutValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.ClosingQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.ClosingValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Key + " Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.OpeningQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.OpeningValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.InQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.InValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.OutQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.OutValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ClosingValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.InQty).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.InValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OutQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OutValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return stockDepotWiseDetailsData;
        }

        public PdfPTable StockProductWiseDetailsDataTable(List<StockLedgerReportLine> list, Font fontArial8, Font fontArial8Bold)
        {
            var productData = list.GroupBy(x => x.ItemName).OrderBy(x => x.Key).ToList();

            PdfPTable stockProductWiseDetailsData = new(10);
            float[] widthsCellsHeaderPage = new float[] { 4f, 20f, 9f, 9f, 9f, 9f, 9f, 9f, 9f, 9f };
            stockProductWiseDetailsData.SetWidths(widthsCellsHeaderPage);
            stockProductWiseDetailsData.WidthPercentage = 100;
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("Store Name", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("Opening Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("Opening Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("In Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("In Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("Out Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("Out Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("Closing Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("Closing Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            stockProductWiseDetailsData.HeaderRows = 1;
            foreach (var product in productData)
            {
                var sl = 0;
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial8Bold)) { Colspan = 9, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var item in product)
                {
                    sl++;
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.StoreName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.OpeningQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.OpeningValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.InQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.InValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.OutQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.OutValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.ClosingQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(item?.ClosingValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.OpeningQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.OpeningValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.InQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.InValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.OutQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.OutValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.ClosingValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.InQty).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.InValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OutQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OutValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseDetailsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return stockProductWiseDetailsData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintStockDepotProductWiseShortReportToPdf(MemoryStream stream, List<StockLedgerReportLine> list, string headerText, StockLedgerRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(headerText, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(request.FromDate, request.ToDate, request.StoreId, request.ProductId, request.InventoryTypeId, request.ProductTypeId));
            // Add the table
            var dataTable = request.IsDepotWiseReport ? pdfGenerator.AddTable(StockDepotWiseShortDataTable(list, fontArial8, fontArial8Bold)) : pdfGenerator.AddTable(StockProductWiseShortDataTable(list, fontArial8, fontArial8Bold));

            // Close the PDF document
            pdfGenerator.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable StockDepotWiseShortDataTable(List<StockLedgerReportLine> list, Font fontArial8, Font fontArial8Bold)
        {
            var storeData = list.GroupBy(x => x.StoreName).OrderBy(x => x.Key).ToList();

            PdfPTable stockDepotWiseShortData = new(4);
            float[] widthsCellsHeaderPage = new float[] { 10f, 40f, 25f, 25f };
            stockDepotWiseShortData.SetWidths(widthsCellsHeaderPage);
            stockDepotWiseShortData.WidthPercentage = 100;
            stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase("Item Name", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase("Closing Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase("Closing Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            stockDepotWiseShortData.HeaderRows = 1;
            foreach (var store in storeData)
            {
                var sl = 0;
                stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(store.Key!.ToString(), fontArial8Bold)) { Colspan = 3, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var item in store)
                {
                    sl++;
                    stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(item?.ItemName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(item?.ClosingQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(item?.ClosingValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(store.Key + " Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(store.Sum(x => x.ClosingValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockDepotWiseShortData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return stockDepotWiseShortData;
        }

        public PdfPTable StockProductWiseShortDataTable(List<StockLedgerReportLine> list, Font fontArial8, Font fontArial8Bold)
        {
            var productData = list.GroupBy(x => x.ItemName).OrderBy(x => x.Key).ToList();

            PdfPTable stockProductWiseShortData = new(4);
            float[] widthsCellsHeaderPage = new float[] { 10f, 40f, 25f, 25f };
            stockProductWiseShortData.SetWidths(widthsCellsHeaderPage);
            stockProductWiseShortData.WidthPercentage = 100;
            stockProductWiseShortData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseShortData.AddCell(new PdfPCell(new Phrase("Store Name", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseShortData.AddCell(new PdfPCell(new Phrase("Closing Quantity", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            stockProductWiseShortData.AddCell(new PdfPCell(new Phrase("Closing Value", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            stockProductWiseShortData.HeaderRows = 1;
            foreach (var product in productData)
            {
                var sl = 0;
                stockProductWiseShortData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial8Bold)) { Colspan = 3, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var item in product)
                {
                    sl++;
                    stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(item?.StoreName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(item?.ClosingQty.ToString("#,##0.000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(item?.ClosingValue.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.ClosingValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            stockProductWiseShortData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0.000"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            stockProductWiseShortData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingValue).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return stockProductWiseShortData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintFinishedGoodsStockSummaryReportToPdfAsync(MemoryStream stream, List<FinishedGoodsStockReportViewModel> list, string headerText, FinishedGoodsStockReportRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(headerText, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            //filtering options
            var filterTable = pdfGenerator.AddTable(SearchFilterTable(request.FromDate, request.ToDate, request.StoreId, null, null, null));
            // Add data table
            var dataTable = pdfGenerator.AddTable(FinishedGoodsStockSummaryDataTable(list, fontArial8, fontArial8Bold));

            // Close the PDF document
            pdfGenerator.Close();

            return (filterTable, dataTable);
        }

        private PdfPTable FinishedGoodsStockSummaryDataTable(List<FinishedGoodsStockReportViewModel> list, Font fontArial8, Font fontArial8Bold)
        {
            var groupedProductTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
            PdfPTable finishedGoodsStockSummaryData = new(11);
            float[] widthsCellFinishedGoodsStockSummaryData = new float[] { 3f, 9f, 3f, 6f, 6f, 6f, 5f, 6f, 5f, 6f, 4f };
            finishedGoodsStockSummaryData.SetWidths(widthsCellFinishedGoodsStockSummaryData);
            finishedGoodsStockSummaryData.WidthPercentage = 100;

            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { Rowspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Description", fontArial8Bold)) { Rowspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Bag Size", fontArial8Bold)) { Rowspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Opening", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Stock In", fontArial8Bold)) { Colspan = 3, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Stock Out", fontArial8Bold)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Balance", fontArial8Bold)) { Colspan = 2, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Production", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Sale Return", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Sale", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Adjustment", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Bag", fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            finishedGoodsStockSummaryData.HeaderRows = 2;

            foreach (var productTypeData in groupedProductTypeData)
            {
                var groupedProductData = productTypeData.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Key, fontArial8Bold)) { Colspan = 11, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                foreach (var productData in groupedProductData)
                {
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.First().Code, fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Key, fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 0 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.First().BagSize.ToString(), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 1 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Sum(x => x.SaleReturnQty).ToString("#,##0"), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Sum(x => x.SaleQty).ToString("#,##0"), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                    finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productData.Sum(x => (int)x.ClosingQty / x.BagSize).ToString("#,##0"), fontArial8)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                }
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Key + " Total : ", fontArial8Bold)) { Colspan = 3, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.SaleReturnQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.SaleQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
                finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(productTypeData.Sum(x => (int)x.ClosingQty / x.BagSize).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            }

            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8Bold)) { Colspan = 3, PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.OpeningQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ProductionQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleReturnQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentInQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.SaleQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.AdjustmentOutQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.ClosingQty).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            finishedGoodsStockSummaryData.AddCell(new PdfPCell(new Phrase(list.Sum(x => (int)x.ClosingQty / x.BagSize).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 2, PaddingBottom = 2, HorizontalAlignment = 2 });
            return finishedGoodsStockSummaryData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintStockReportToPdfAsync(MemoryStream stream, Tuple<List<StockViewModel>, int> result, string headerText, StockRequestModel request)
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
            var filterTable = FilteringDataTable(fontArial9, null, null, request.StoreId, request.ProductId, request.InventoryTypeId, request.ProductTypeId);
            var dataTable = PrimaryStockReportDataTable(result, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable PrimaryStockReportDataTable(Tuple<List<StockViewModel>, int> result, Font fontArial7, Font fontArial7Bold)
        {
            var productData = result.Item1.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();
            PdfPTable primaryStockReportDataTable = new(4);
            float[] widthsCellsPrimaryStockReportDataTable = new float[] { 5f, 50f, 20f, 20f };
            primaryStockReportDataTable.SetWidths(widthsCellsPrimaryStockReportDataTable);
            primaryStockReportDataTable.WidthPercentage = 100;
            primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase("Sl No", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase("Store", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase("Available Quantity", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase("TP Value", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            primaryStockReportDataTable.HeaderRows = 1;
            var sl = 0;
            foreach (var product in productData)
            {
                primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(product.Key, fontArial7Bold)) { Colspan = 4, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 0 });
                foreach (var item in product)
                {

                    sl++;
                    primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(item.StoreName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(item.AvailableQty.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(item.SalePriceValue.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(product.Key + ", Total:", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.AvailableQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.SalePriceValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase("Grand Total:", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(result.Item1.Sum(x => x.AvailableQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            primaryStockReportDataTable.AddCell(new PdfPCell(new Phrase(result.Item1.Sum(x => x.SalePriceValue).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return primaryStockReportDataTable;
        }
    }
}
