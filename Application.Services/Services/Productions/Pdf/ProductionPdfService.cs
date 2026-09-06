using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Production;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Productions.Pdf
{
    public class ProductionPdfService : IProductionPdfService
    {
        private readonly DataContext _context;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;
        public ProductionPdfService(DataContext context, IWorkContext workContext, IUnitOfWork unitOfWork, ITenantService tenantService)
        {
            _context = context;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
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

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductionReportToPdf(MemoryStream stream, List<ProductionViewModel> productionViewModels, string reportTitle, ProductionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (productionViewModels == null)
                throw new ArgumentNullException(nameof(productionViewModels));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(18f, 18f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = FilteringDataTable(request, fontArial9);
            var dataTable = ProductionDataTable(productionViewModels, fontArial8Bold, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable FilteringDataTable(ProductionRequestModel request, Font fontArial9)
        {
            PdfPTable filteringData = new(1);
            float[] widthsCellsFilteringData = new float[] { 100f };
            filteringData.SetWidths(widthsCellsFilteringData);
            filteringData.WidthPercentage = 100;
            filteringData.SpacingAfter = 3;

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                filteringData.AddCell(new PdfPCell(new Phrase("Date:                   " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            }
            if (request.FinishedProductId.HasValue)
            {
                var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == request.FinishedProductId.Value)?.Name;
                filteringData.AddCell(new PdfPCell(new Phrase("Product Name:    " + productName, fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            }
            if (request.FgstoreId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.FgstoreId.Value)?.Name;
                filteringData.AddCell(new PdfPCell(new Phrase("Store:                  " + storeName, fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            }
            return filteringData;
        }

        public PdfPTable ProductionDataTable(List<ProductionViewModel> productionViewModels, Font fontArial8Bold, Font fontArial7, Font fontArial7Bold)
        {
            PdfPTable productionData = new(19);
            float[] widthsCellsProductionDataTable = new float[] { 3f, 5f, 8f, 3f, 5f, 7f, 4f, 9f, 7f, 7f, 6f, 7f, 4f, 6f, 4f, 5f, 3f, 4f, 5f };
            productionData.SetWidths(widthsCellsProductionDataTable);
            productionData.WidthPercentage = 100;
            productionData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Date", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Store", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Shift", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Machine", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Product", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Formula", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Batch", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("RM Used", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Bag Used", fontArial8Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Actual Production", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Weight Loss", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Time", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("No.", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Weight", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Qty", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("%", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Qty", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("%", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Hour", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionData.AddCell(new PdfPCell(new Phrase("Prod. / Hour (KG)", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            productionData.HeaderRows = 2;
            var sl = 0;
            double totalMinites = 0;
            decimal packagingMaterialsTotalQty = 0;
            foreach (var item in productionViewModels)
            {
                var packagingMaterialsQty = item.ProductionDetails!.Where(x => x.RawMaterial!.ProductTypeId == ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.ActualUsedQuantity);
                packagingMaterialsTotalQty += packagingMaterialsQty;
                var difference = item.EndDateTime!.Value.TrimSecondsAndMilliseconds() - item.StartDateTime!.Value.TrimSecondsAndMilliseconds();
                var differenceInMinutes = difference.TotalMinutes;
                double finalDifferenceInMinutes = differenceInMinutes - item.BreakTime;
                totalMinites += finalDifferenceInMinutes;
                sl++;
                productionData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productionData.AddCell(new PdfPCell(new Phrase(item.ProductionDate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productionData.AddCell(new PdfPCell(new Phrase(item.Fgstore?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionData.AddCell(new PdfPCell(new Phrase(item.Shift?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionData.AddCell(new PdfPCell(new Phrase(item.Machine?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionData.AddCell(new PdfPCell(new Phrase(item.ProductionNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionData.AddCell(new PdfPCell(new Phrase(item.FinishedProduct?.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productionData.AddCell(new PdfPCell(new Phrase(item.FinishedProduct?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionData.AddCell(new PdfPCell(new Phrase(item.FormulationNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionData.AddCell(new PdfPCell(new Phrase(item.BatchNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionData.AddCell(new PdfPCell(new Phrase(item.ProductionQuantity.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionData.AddCell(new PdfPCell(new Phrase((item.TotalRmused + item.TotalAdjustmentQuantity - packagingMaterialsQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionData.AddCell(new PdfPCell(new Phrase(packagingMaterialsQty.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionData.AddCell(new PdfPCell(new Phrase(item.ActualProductionQuantity.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionData.AddCell(new PdfPCell(new Phrase(Math.Round(((item.ActualProductionQuantity * 100m) / (item.TotalRmused + item.TotalAdjustmentQuantity - packagingMaterialsQty)), 2).ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionData.AddCell(new PdfPCell(new Phrase((item.ActualProductionQuantity - (item.TotalRmused + item.TotalAdjustmentQuantity) + packagingMaterialsQty).ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionData.AddCell(new PdfPCell(new Phrase(Math.Round((((item.ActualProductionQuantity - (item.TotalRmused + item.TotalAdjustmentQuantity) + packagingMaterialsQty) * 100m) / (item.TotalRmused + item.TotalAdjustmentQuantity - packagingMaterialsQty)), 2).ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionData.AddCell(new PdfPCell(new Phrase(TimeSpanHelper.ConvertMinutesToHoursAndMinutes((int)finalDifferenceInMinutes).ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productionData.AddCell(new PdfPCell(new Phrase(Math.Round(((item.ActualProductionQuantity * 60) / finalDifferenceInMinutes), 2).ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            var totalExtraDamageQuantityPercentage = Math.Round(((productionViewModels.Sum(x => x.ActualProductionQuantity - (x.TotalRmused + x.TotalAdjustmentQuantity)) + packagingMaterialsTotalQty) * 100m) / (productionViewModels.Sum(x => x.TotalRmused + x.TotalAdjustmentQuantity) - packagingMaterialsTotalQty), 2);
            var totalActualProductionQuantityPercentage = Math.Round((productionViewModels.Sum(x => x.ActualProductionQuantity) * 100m) / (productionViewModels.Sum(x => x.TotalRmused + x.TotalAdjustmentQuantity) - packagingMaterialsTotalQty), 2);
            productionData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial7Bold)) { Colspan = 10, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase(productionViewModels.Sum(x => x.ProductionQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase((productionViewModels.Sum(x => x.TotalRmused + x.TotalAdjustmentQuantity) - packagingMaterialsTotalQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase(packagingMaterialsTotalQty.ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase(productionViewModels.Sum(x => x.ActualProductionQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase(totalActualProductionQuantityPercentage.ToString(), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase((productionViewModels.Sum(x => x.ActualProductionQuantity - (x.TotalRmused + x.TotalAdjustmentQuantity)) + packagingMaterialsTotalQty).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase(totalExtraDamageQuantityPercentage.ToString(), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase(TimeSpanHelper.ConvertMinutesToHoursAndMinutes((int)totalMinites), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionData.AddCell(new PdfPCell(new Phrase(Math.Round(((productionViewModels.Sum(x => x.ActualProductionQuantity) * 60) / totalMinites), 2).ToString(), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return productionData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductionDetailsReportToPdf(MemoryStream stream, List<ProductionViewModel> productionViewModels, string reportTitle, ProductionRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (productionViewModels == null)
                throw new ArgumentNullException(nameof(productionViewModels));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(18f, 18f, 20f, 20f);
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

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = FilteringDataTable(request, fontArial9);
            var dataTable = ProductionDetailDataTable(productionViewModels, fontArial9, fontArial8, fontArial8Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable ProductionDetailDataTable(List<ProductionViewModel> productionViewModels, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            PdfPTable productionDetailsData = new(18);
            float[] widthsCellsProductionDetailsDataTable = new float[] { 3f, 6f, 10f, 4f, 5f, 8f, 4f, 10f, 7f, 5f, 6f, 4f, 6f, 4f, 5f, 6f, 8f, 4f };
            productionDetailsData.SetWidths(widthsCellsProductionDetailsDataTable);
            productionDetailsData.WidthPercentage = 100;
            productionDetailsData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Date", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Store", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Shift", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Machine", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Product", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("RM Used", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Bag Used", fontArial9)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Actual Production", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Weight Loss", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Time", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Cost", fontArial9)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Name", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Qty", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("%", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Qty", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("%", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Hour", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Prod. / Hour (KG)", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("RM Cost", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Rate / KG", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            productionDetailsData.HeaderRows = 2;
            var sl = 0;
            double totalMinites = 0;
            decimal packagingMaterialsTotalQty = 0;
            foreach (var item in productionViewModels)
            {
                var packagingMaterialsQty = item.ProductionDetails!.Where(x => x.RawMaterial!.ProductTypeId == ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.ActualUsedQuantity);
                packagingMaterialsTotalQty += packagingMaterialsQty;
                var difference = item.EndDateTime!.Value.TrimSecondsAndMilliseconds() - item.StartDateTime!.Value.TrimSecondsAndMilliseconds();
                var differenceInMinutes = difference.TotalMinutes;
                double finalDifferenceInMinutes = differenceInMinutes - item.BreakTime;
                totalMinites += finalDifferenceInMinutes;

                sl++;
                productionDetailsData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(item.ProductionDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(item.Fgstore?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(item.Shift?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(item.Machine?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(item.ProductionNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(item.FinishedProduct?.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(item.FinishedProduct?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase((item.TotalRmused + item.TotalAdjustmentQuantity - packagingMaterialsQty).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(packagingMaterialsQty.ToString("#,##0"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(item.ActualProductionQuantity.ToString("#,##0"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(Math.Round(((item.ActualProductionQuantity * 100m) / (item.TotalRmused + item.TotalAdjustmentQuantity - packagingMaterialsQty)), 2).ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase((item.ActualProductionQuantity - (item.TotalRmused + item.TotalAdjustmentQuantity) + packagingMaterialsQty).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(Math.Round((((item.ActualProductionQuantity - (item.TotalRmused + item.TotalAdjustmentQuantity) + packagingMaterialsQty) * 100m) / (item.TotalRmused + item.TotalAdjustmentQuantity - packagingMaterialsQty)), 2).ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(TimeSpanHelper.ConvertMinutesToHoursAndMinutes((int)finalDifferenceInMinutes), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(Math.Round(((item.ActualProductionQuantity * 60) / finalDifferenceInMinutes), 2).ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase((item.RmCost + item.TotalAdjustmentCost).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                productionDetailsData.AddCell(new PdfPCell(new Phrase(((item.RmCost + item.TotalAdjustmentCost) / item.ActualProductionQuantity).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            var totalExtraDamageQuantityPercentage = Math.Round(((productionViewModels.Sum(x => x.ActualProductionQuantity - (x.TotalRmused + x.TotalAdjustmentQuantity)) + packagingMaterialsTotalQty) * 100m) / (productionViewModels.Sum(x => x.TotalRmused + x.TotalAdjustmentQuantity) - packagingMaterialsTotalQty), 2);
            var totalActualProductionQuantityPercentage = Math.Round((productionViewModels.Sum(x => x.ActualProductionQuantity) * 100m) / (productionViewModels.Sum(x => x.TotalRmused + x.TotalAdjustmentQuantity) - packagingMaterialsTotalQty), 2);
            productionDetailsData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8Bold)) { Colspan = 8, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase((productionViewModels.Sum(x => x.TotalRmused + x.TotalAdjustmentQuantity) - packagingMaterialsTotalQty).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase(packagingMaterialsTotalQty.ToString("#,##0"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase(productionViewModels.Sum(x => x.ActualProductionQuantity).ToString("#,##0"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase(totalActualProductionQuantityPercentage.ToString(), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase((productionViewModels.Sum(x => x.ActualProductionQuantity - (x.TotalRmused + x.TotalAdjustmentQuantity)) + packagingMaterialsTotalQty).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase(totalExtraDamageQuantityPercentage.ToString(), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase(TimeSpanHelper.ConvertMinutesToHoursAndMinutes((int)totalMinites), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase(Math.Round(((productionViewModels.Sum(x => x.ActualProductionQuantity) * 60) / totalMinites), 2).ToString(), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase(productionViewModels.Sum(x => x.RmCost + x.TotalAdjustmentCost).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            productionDetailsData.AddCell(new PdfPCell(new Phrase((productionViewModels.Sum(x => x.RmCost + x.TotalAdjustmentCost) / productionViewModels.Sum(x => x.ActualProductionQuantity)).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return productionDetailsData;
        }
    }
}
