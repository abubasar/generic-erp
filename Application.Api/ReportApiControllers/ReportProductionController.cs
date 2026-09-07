using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Industry;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.ReportApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("report")]
    public class ReportProductionController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly IIndustryProfile _industry;

        public ReportProductionController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService, IIndustryProfile industry)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
            _industry = industry;
        }

        [Authorize(Permissions.Productions.Production_Bill_Details_Report)]
        [HttpGet("production-bill-details/{productionId}/{reportType}")]
        public async Task<IActionResult> Productions(Guid productionId, int reportType)
        {
            // ==============================
            // PDF Settings and Customization
            // ==============================
            MemoryStream workStream = new();
            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(30f, 30f, 30f, 30f);
            PdfWriter.GetInstance(document, workStream).CloseStream = false;

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
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));


            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Production Bill Details", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Production
            // =============
            var production = _unitOfWork.Repository<Production>().TableNoTracking().Include(x => x.FinishedProduct).ThenInclude(x => x.PackSize).Include(x => x.FinishedProduct).ThenInclude(x => x.MeasurementUnit).Include(x => x.Fgstore).Where(d => d.Id == productionId).FirstOrDefault();

            PdfPTable tableProduction = new(4);
            PdfPTable tableProductionFilterForExcel = new(1);
            PdfPTable tableProductionItems = new(7);

            if (production is not null)
            {
                String? finishedProductCode = production.FinishedProduct.Code;
                String? finishedProductName = production.FinishedProduct.Name;
                String? finishedProductMeasurementUnitName = production.FinishedProduct?.MeasurementUnit?.Name;
                String? finishedProductPackSize = production.FinishedProduct?.PackSize?.Name;
                string productInfo;
                productInfo = _industry.Reports.ProductLabel(finishedProductName, finishedProductCode, finishedProductPackSize);
                String? rawMaterialStoreName = production.Fgstore.Name;
                String? productionNo = production.ProductionNo;
                String? manufacturingOrderNo = production.ManufacturingOrderNo;
                String? bomNo = production.BomNo;
                int? extraDamageQuantity = production.ExtraDamageQuantity;
                String? productionDate = production.ProductionDate.ToString("dd-MMM-yyyy");
                String? formulationNo = production.FormulationNo;
                String? batchNo = production.BatchNo;
                int? productionQuantity = production.ProductionQuantity;
                int? actualProductionQuantity = production.ActualProductionQuantity;
                int? dustLooseInQuantity = production.DustLooseInQuantity;
                int? dustLooseOutQuantity = production.DustLooseOutQuantity;
                decimal? totalRmused = production.TotalRmused;
                decimal? totalCost = production.TotalCost;
                String? createdOn = production.CreatedOn.ToString("dd-MMM-yyyy");
                String? remarks = production.Remark;
                String? preparedBy = production.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(production.CheckedBy))
                    checkedBy = production.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(production.ApprovedBy))
                    approvedBy = production.ApprovedBy;
                else
                    approvedBy = "";

                var difference = production.EndDateTime.TrimSecondsAndMilliseconds() - production.StartDateTime.TrimSecondsAndMilliseconds();
                var differenceInMinutes = difference.TotalMinutes;
                double finalDifferenceInMinutes = differenceInMinutes - production.BreakTime;


                if (reportType == 1)
                {
                    //PdfPTable tableProduction = new(4);
                    float[] widthscellsTableProduction = new float[] { 65f, 130f, 65f, 130f };
                    tableProduction.SetWidths(widthscellsTableProduction);
                    tableProduction.WidthPercentage = 100;
                    tableProduction.AddCell(new PdfPCell(new Phrase("Formulation No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(formulationNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Production No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(productionNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Product: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(productInfo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(productionDate, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Store: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(rawMaterialStoreName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("MO No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(manufacturingOrderNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Batch No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(batchNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("BOM No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(bomNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Finished Goods Qty: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(actualProductionQuantity.ToString() + " " + finishedProductMeasurementUnitName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Production Time: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(TimeSpanHelper.ConvertMinutesToHoursAndMinutes((int)finalDifferenceInMinutes) + "  Hour", fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Dust/Loose In Qty: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(dustLooseInQuantity.ToString() + " " + finishedProductMeasurementUnitName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Dust/Loose Out Qty: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(dustLooseOutQuantity.ToString() + " " + finishedProductMeasurementUnitName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProduction.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                    document.Add(tableProduction);
                }

                if (reportType == 2)
                {
                    float[] widthscellsTableProductionFilterForExcel = new float[] { 100f };
                    tableProductionFilterForExcel.SetWidths(widthscellsTableProductionFilterForExcel);
                    tableProductionFilterForExcel.WidthPercentage = 100;

                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Formulation No: " + formulationNo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Production No: " + productionNo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Product: " + productInfo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Date: " + productionDate, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Store: " + rawMaterialStoreName, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("MO No: " + manufacturingOrderNo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("BOM No: " + bomNo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Batch No: " + batchNo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Finished Goods Qty: " + actualProductionQuantity.ToString() + "  KG", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Production Time: " + TimeSpanHelper.ConvertMinutesToHoursAndMinutes((int)finalDifferenceInMinutes) + "  Hour", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Dust/Loose In Qty: " + dustLooseInQuantity.ToString() + " " + finishedProductMeasurementUnitName, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Dust/Loose Out Qty: " + dustLooseOutQuantity.ToString() + " " + finishedProductMeasurementUnitName, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableProductionFilterForExcel.AddCell(new PdfPCell(new Phrase("Remarks: " + remarks, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                }

                // ======================
                // Get Production Item
                // ======================
                var productionItems = _unitOfWork.Repository<ProductionDetail>().TableNoTracking().Include(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.ProductionId == productionId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.RawMaterial.Code,
                                           InventoryType = d.RawMaterial.InventoryType.Name,
                                           ProductTypeId = d.RawMaterial.ProductType.Id,
                                           ProductType = d.RawMaterial.ProductType.Name,
                                           ProductName = d.RawMaterial.Name,
                                           UnitName = d.RawMaterial.MeasurementUnit.Name,
                                           d.Quantity,
                                           d.AdjustmentQuantity,
                                           d.AdjustmentValue,
                                           d.Amount,
                                       }).ToList();
                var groupedData = productionItems.GroupBy(x => x.ProductType).OrderBy(x => x.Key).ToList();
                var packagingMaterialsQty = productionItems.Where(x => x.ProductTypeId == ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.Quantity + x.AdjustmentQuantity);
                var packagingMaterialsValue = productionItems.Where(x => x.ProductTypeId == ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.Amount + x.AdjustmentValue);

                if (productionItems.Any())
                {
                    //PdfPTable tableProductionItems = new(7);
                    float[] widthsCellsProductionItems = new float[] { 6f, 8f, 43f, 6f, 13f, 15f, 9f };
                    tableProductionItems.SetWidths(widthsCellsProductionItems);
                    tableProductionItems.WidthPercentage = 100;
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Item Description", fontArial9Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("R/M Quantity", fontArial9Bold)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Rate", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Value", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Average", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    decimal summationOfQuantity = 0;
                    decimal summationOfAmount = 0;
                    foreach (var productTypeGroup in groupedData)
                    {
                        decimal sumQuantity = productTypeGroup.Sum(item => item.Quantity + item.AdjustmentQuantity);
                        decimal sumAmount = productTypeGroup.Sum(item => item.Amount + item.AdjustmentValue);
                        summationOfQuantity += sumQuantity;
                        summationOfAmount += sumAmount;
                        tableProductionItems.AddCell(new PdfPCell(new Phrase(productTypeGroup.Key.ToString(), fontArial8Bold)) { Colspan = 7, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        foreach (var item in productTypeGroup.OrderBy(x => x.ProductName))
                        {
                            sl++;
                            tableProductionItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase((item.Quantity + item.AdjustmentQuantity).ToString("#,##0.000"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase((item.Amount + item.AdjustmentValue).ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase((item.Quantity + item.AdjustmentQuantity) == 0M ? "0.00" : ((item.Amount + item.AdjustmentValue) / (item.Quantity + item.AdjustmentQuantity)).ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        }
                        tableProductionItems.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableProductionItems.AddCell(new PdfPCell(new Phrase(sumQuantity.ToString("#,##0.000"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableProductionItems.AddCell(new PdfPCell(new Phrase(sumAmount.ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableProductionItems.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Total (Including Bag Quantity): ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase(summationOfQuantity.ToString("#,##0.000"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase(summationOfAmount.ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Total (Excluding Bag Quantity): ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase((summationOfQuantity - packagingMaterialsQty).ToString("#,##0.000"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase((summationOfAmount - packagingMaterialsValue).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableProductionItems);

                    document.Add(spaceTable);
                }
                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new(4);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Authorized by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });

                tableUsers.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 1f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();

            if (reportType == 2)
            {
                var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tableProductionFilterForExcel, tableProductionItems, SheetNameGenerator.Generate("Sheet1", null, null), tenantData?.Name, tenantData?.Address, "Production Bill Details");
                return File(excelBytes, MimeTypes.TextXlsx);
            }
            else
            {

                byte[] byteInfo = workStream.ToArray();
                workStream.Write(byteInfo, 0, byteInfo.Length);
                workStream.Position = 0;

                return new FileStreamResult(workStream, MimeTypes.ApplicationPdf);
            }
        }

        [Authorize(Permissions.Productions.Production_Bill_Summary_Report)]
        [HttpGet("production-bill-summary/{productionId}")]
        public async Task<IActionResult> ProductionBillSummary(Guid productionId)
        {
            // ==============================
            // PDF Settings and Customization
            // ==============================
            MemoryStream workStream = new();
            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(30f, 30f, 30f, 30f);
            PdfWriter.GetInstance(document, workStream).CloseStream = false;

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
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));


            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Production Bill Summary", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Production
            // =============
            var production = _unitOfWork.Repository<Production>().TableNoTracking().Include(x => x.FinishedProduct).ThenInclude(x => x.PackSize).Include(x => x.FinishedProduct).ThenInclude(x => x.MeasurementUnit).Include(x => x.Fgstore).Where(d => d.Id == productionId).FirstOrDefault();

            if (production is not null)
            {
                String? finishedProductCode = production.FinishedProduct.Code;
                String? finishedProductName = production.FinishedProduct.Name;
                String? finishedProductMeasurementUnitName = production.FinishedProduct?.MeasurementUnit?.Name;
                String? finishedProductPackSize = production.FinishedProduct?.PackSize?.Name;
                string productInfo;
                productInfo = _industry.Reports.ProductLabel(finishedProductName, finishedProductCode, finishedProductPackSize);
                String? rawMaterialStoreName = production.Fgstore.Name;
                String? productionNo = production.ProductionNo;
                String? manufacturingOrderNo = production.ManufacturingOrderNo;
                String? bomNo = production.BomNo;
                int? extraDamageQuantity = production.ExtraDamageQuantity;
                String? productionDate = production.ProductionDate.ToString("dd-MMM-yyyy");
                String? formulationNo = production.FormulationNo;
                String? batchNo = production.BatchNo;
                int? productionQuantity = production.ProductionQuantity;
                int? actualProductionQuantity = production.ActualProductionQuantity;
                int? dustLooseInQuantity = production.DustLooseInQuantity;
                int? dustLooseOutQuantity = production.DustLooseOutQuantity;
                decimal? totalRmused = production.TotalRmused;
                String? createdOn = production.CreatedOn.ToString("dd-MMM-yyyy");
                String? remarks = production.Remark;
                String? preparedBy = production.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(production.CheckedBy))
                    checkedBy = production.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(production.ApprovedBy))
                    approvedBy = production.ApprovedBy;
                else
                    approvedBy = "";

                var difference = production.EndDateTime.TrimSecondsAndMilliseconds() - production.StartDateTime.TrimSecondsAndMilliseconds();
                var differenceInMinutes = difference.TotalMinutes;
                double finalDifferenceInMinutes = differenceInMinutes - production.BreakTime;


                PdfPTable tableProduction = new(4);
                float[] widthscellsTableProduction = new float[] { 65f, 130f, 65f, 130f };
                tableProduction.SetWidths(widthscellsTableProduction);
                tableProduction.WidthPercentage = 100;
                tableProduction.AddCell(new PdfPCell(new Phrase("Formulation No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(formulationNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Production No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(productionNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Product: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(productInfo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(productionDate, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Store: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(rawMaterialStoreName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("MO No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(manufacturingOrderNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Batch No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(batchNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("BOM No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(bomNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Finished Goods Qty: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(actualProductionQuantity.ToString() + " " + finishedProductMeasurementUnitName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Production Time: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(TimeSpanHelper.ConvertMinutesToHoursAndMinutes((int)finalDifferenceInMinutes) + "  Hour", fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Dust/Loose In Qty: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(dustLooseInQuantity.ToString() + " " + finishedProductMeasurementUnitName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Dust/Loose Out Qty: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(dustLooseOutQuantity.ToString() + " " + finishedProductMeasurementUnitName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableProduction.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableProduction);

                // ======================
                // Get Production Item
                // ======================
                var productionItems = _unitOfWork.Repository<ProductionDetail>().TableNoTracking().Include(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.ProductionId == productionId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.RawMaterial.Code,
                                           InventoryType = d.RawMaterial.InventoryType.Name,
                                           ProductTypeId = d.RawMaterial.ProductType.Id,
                                           ProductType = d.RawMaterial.ProductType.Name,
                                           ProductName = d.RawMaterial.Name,
                                           UnitName = d.RawMaterial.MeasurementUnit.Name,
                                           d.Quantity,
                                           d.AdjustmentQuantity,
                                           d.AdjustmentValue,
                                           d.Amount,
                                       }).ToList();
                var groupedData = productionItems.GroupBy(x => x.ProductType).OrderBy(x => x.Key).ToList();
                var packagingMaterialsQty = productionItems.Where(x => x.ProductTypeId == ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.Quantity + x.AdjustmentQuantity);

                if (productionItems.Any())
                {
                    PdfPTable tableProductionItems = new(5);
                    float[] widthsCellsProductionItems = new float[] { 6f, 8f, 43f, 6f, 13f };
                    tableProductionItems.SetWidths(widthsCellsProductionItems);
                    tableProductionItems.WidthPercentage = 100;
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Item Description", fontArial9Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("R/M Quantity", fontArial9Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    decimal summationOfQuantity = 0;
                    foreach (var productTypeGroup in groupedData)
                    {
                        decimal sumQuantity = productTypeGroup.Sum(item => item.Quantity + item.AdjustmentQuantity);
                        summationOfQuantity += sumQuantity;
                        tableProductionItems.AddCell(new PdfPCell(new Phrase(productTypeGroup.Key.ToString(), fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        foreach (var item in productTypeGroup.OrderBy(x => x.ProductName))
                        {
                            sl++;
                            tableProductionItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableProductionItems.AddCell(new PdfPCell(new Phrase((item.Quantity + item.AdjustmentQuantity).ToString("#,##0.000"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        }
                        tableProductionItems.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableProductionItems.AddCell(new PdfPCell(new Phrase(sumQuantity.ToString("#,##0.000"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Total (Including Bag Quantity): ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase(summationOfQuantity.ToString("#,##0.000"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase("Total (Excluding Bag Quantity): ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableProductionItems.AddCell(new PdfPCell(new Phrase((summationOfQuantity - packagingMaterialsQty).ToString("#,##0.000"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableProductionItems);

                    document.Add(spaceTable);
                }
                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new(4);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Authorized by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });

                tableUsers.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 1f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}
