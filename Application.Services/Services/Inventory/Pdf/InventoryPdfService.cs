using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Purchase;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Inventory.Pdf
{
    public class InventoryPdfService : IInventoryPdfService
    {
        private readonly DataContext _context;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;

        public InventoryPdfService(DataContext context, IWorkContext workContext, IUnitOfWork unitOfWork, ITenantService tenantService)
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

        //StockAdjustment
        public async Task PrintStockAdjustmentReportToPdfAsync(MemoryStream stream, List<StockAdjustmentViewModel> stockAdjustmentViewModels, string reportTitle, bool isDetails, StockAdjustmentRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stockAdjustmentViewModels == null)
                throw new ArgumentNullException(nameof(stockAdjustmentViewModels));

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
            var headerTable = await AddStockAdjustmentHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var saleInvoiceData = StockAdjustmentDataTable(stockAdjustmentViewModels, isDetails, fontArial8, fontArial7, fontArial7Bold, fontArial7Gray);

            document.Add(headerTable);
            document.Add(saleInvoiceData);

            document.Close();
        }

        private async Task<PdfPTable> AddStockAdjustmentHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, StockAdjustmentRequestModel request)
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
            if (request.StoreId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Store: " + storeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        private PdfPTable StockAdjustmentDataTable(List<StockAdjustmentViewModel> stockAdjustmentViewModels, bool isDetails, Font fontArial8, Font fontArial7, Font fontArial7Bold, Font fontArial7Gray)
        {
            PdfPTable stockAdjustmentData = new(5);
            float[] widthCellsHeaderPage = new float[] { 10f, 20f, 20f, 30f, 20f };
            stockAdjustmentData.SetWidths(widthCellsHeaderPage);
            stockAdjustmentData.WidthPercentage = 100;
            stockAdjustmentData.HeaderRows = 1;

            stockAdjustmentData.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockAdjustmentData.AddCell(new PdfPCell(new Phrase("Code", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockAdjustmentData.AddCell(new PdfPCell(new Phrase("Adjustment Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockAdjustmentData.AddCell(new PdfPCell(new Phrase("Store", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            stockAdjustmentData.AddCell(new PdfPCell(new Phrase("Total Adjustment Qty", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in stockAdjustmentViewModels)
            {
                sl++;
                stockAdjustmentData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                stockAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                stockAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.AdjustmentDate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                stockAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                stockAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.TotalAdjustmentQty.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(4);
                    float[] widthsCellsProductDetailsTable = new float[] { 10f, 55f, 10f, 30f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Adjustment Qty", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

                    var miniSL = 0;
                    foreach (var miniItem in item?.StockAdjustmentDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.AdjustmentQty.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    stockAdjustmentData.AddCell(new PdfPCell(new Phrase("Stock Adjustment Details", fontArial7Gray)) { Colspan = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial8);
                    productDetailsCell2.Colspan = 3;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    stockAdjustmentData.AddCell(productDetailsCell2);
                }
            }

            stockAdjustmentData.AddCell(new PdfPCell(new Phrase("Total:", fontArial7Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            stockAdjustmentData.AddCell(new PdfPCell(new Phrase(stockAdjustmentViewModels.Sum(x => x.TotalAdjustmentQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return stockAdjustmentData;
        }

    }
}
