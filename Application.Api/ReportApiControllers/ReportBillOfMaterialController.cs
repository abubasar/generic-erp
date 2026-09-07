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
    public class ReportBillOfMaterialController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly IIndustryProfile _industry;

        public ReportBillOfMaterialController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService, IIndustryProfile industry)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
            _industry = industry;
        }

        [HttpGet("{billOfMaterialId}/{reportType}")]
        public async Task<IActionResult> BillOfMaterials(Guid billOfMaterialId, int reportType)
        {
            // ==============================
            // PDF Settings and Customization
            // ==============================
            MemoryStream workStream = new MemoryStream();
            Rectangle rectangle = new Rectangle(PageSize.A4);
            Document document = new Document(rectangle, 72, 72, 72, 72);
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

            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));


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
            headerPage.AddCell(new PdfPCell(new Phrase("Feed Formula", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
            document.Add(headerPage);

            // =====
            // Space
            // =====
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 5f });

            document.Add(line);

            // =============
            // Bill of Material
            // =============
            var billOfMaterial = _unitOfWork.Repository<BillOfMaterial>().TableNoTracking().Include(x => x.FinishedProduct).ThenInclude(x => x.PackSize).Where(d => d.Id == billOfMaterialId).FirstOrDefault();
            PdfPTable tableBillOfMaterialForExcel = new(1);
            PdfPTable tableBillOfMaterial = new(4);
            PdfPTable tableBillOfMaterialItems = new(6);

            if (billOfMaterial is not null)
            {
                String? finishedProductCode = billOfMaterial.FinishedProduct.Code;
                String? finishedProductName = billOfMaterial.FinishedProduct.Name;
                String? finishedProductPackSize = billOfMaterial.FinishedProduct?.PackSize?.Name;
                string productInfo;
                productInfo = _industry.Reports.ProductLabel(finishedProductName, finishedProductCode, finishedProductPackSize);

                String? bomNo = billOfMaterial.BomNo;
                String? formulationNo = billOfMaterial.FormulationNo;
                int? dosageQuantity = billOfMaterial.DosageQuantity;
                String? createdOn = billOfMaterial.CreatedOn.ToString("dd-MMM-yyyy");
                String? remarks = billOfMaterial.Remark;
                String? preparedBy = billOfMaterial.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(billOfMaterial.CheckedBy))
                    checkedBy = billOfMaterial.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(billOfMaterial.ApprovedBy))
                    approvedBy = billOfMaterial.ApprovedBy;
                else
                    approvedBy = "";

                if (reportType == 1)
                {
                    //PdfPTable tableBillOfMaterial = new PdfPTable(4);
                    float[] widthscellsTableBillOfMaterial = new float[] { 55f, 130f, 60f, 130f };
                    tableBillOfMaterial.SetWidths(widthscellsTableBillOfMaterial);
                    tableBillOfMaterial.WidthPercentage = 100;
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase("BOM No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase(bomNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase("Formulation No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase(formulationNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase("Product: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase(productInfo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase("Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase(createdOn, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase("Dosage Quantity: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase(dosageQuantity.ToString(), fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterial.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                    document.Add(tableBillOfMaterial);
                }
                if (reportType == 2)
                {
                    float[] widthscellsTableBillOfMaterialForExcel = new float[] { 100f };
                    tableBillOfMaterialForExcel.SetWidths(widthscellsTableBillOfMaterialForExcel);
                    tableBillOfMaterialForExcel.WidthPercentage = 100;
                    tableBillOfMaterialForExcel.AddCell(new PdfPCell(new Phrase("BOM No: " + bomNo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterialForExcel.AddCell(new PdfPCell(new Phrase("Formulation No: " + formulationNo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterialForExcel.AddCell(new PdfPCell(new Phrase("Product: " + productInfo, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterialForExcel.AddCell(new PdfPCell(new Phrase("Date: " + createdOn, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterialForExcel.AddCell(new PdfPCell(new Phrase("Dosage Quantity: " + dosageQuantity.ToString(), fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                    tableBillOfMaterialForExcel.AddCell(new PdfPCell(new Phrase("Remarks: " + remarks, fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                }

                // ======================
                // Get Bill of Material Item
                // ======================
                var billOfMaterialItems = _unitOfWork.Repository<BillOfMaterialDetail>().TableNoTracking().Include(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.BillOfMaterialId == billOfMaterialId).Select(d => new
                                       {
                                           Id = d.Id,
                                           ProductCode = d.RawMaterial.Code,
                                           InventoryType = d.RawMaterial.InventoryType.Name,
                                           ProductTypeId = d.RawMaterial.ProductType.Id,
                                           ProductType = d.RawMaterial.ProductType.Name,
                                           ProductName = d.RawMaterial.Name,
                                           UnitName = d.RawMaterial.MeasurementUnit.Name,
                                           Quantity = d.Quantity,
                                           Percentage = d.Percentage,
                                       }).ToList();
                var groupedData = billOfMaterialItems.GroupBy(x => x.ProductType).OrderBy(x => x.Key).ToList();
                var packagingMaterialsQty = billOfMaterialItems.Where(x => x.ProductTypeId == ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.Quantity);
                var packagingMaterialsQtyPercentage = billOfMaterialItems.Where(x => x.ProductTypeId == ProductTypeConstant.PackagingMaterials.ToGuid()).Sum(x => x.Percentage);

                if (billOfMaterialItems.Any())
                {
                    //PdfPTable tableBillOfMaterialItems = new PdfPTable(6);
                    float[] widthsCellsSalesInvoiceItems = new float[] { 8f, 8f, 50f, 10f, 12f, 12f };
                    tableBillOfMaterialItems.SetWidths(widthsCellsSalesInvoiceItems);
                    tableBillOfMaterialItems.WidthPercentage = 100;
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("Raw Material Description", fontArial9Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("R/M Quantity", fontArial9Bold)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("%", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("Qty / Ton", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });

                    int sl = 0;
                    decimal summationOfQuantity = 0;
                    decimal summationOfPercentage = 0;
                    foreach (var productTypeGroup in groupedData)
                    {
                        decimal sumQuantity = productTypeGroup.Sum(item => item.Quantity);
                        decimal sumPercentage = productTypeGroup.Sum(item => item.Percentage);
                        summationOfQuantity += sumQuantity;
                        summationOfPercentage += sumPercentage;
                        tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(productTypeGroup.Key.ToString(), fontArial8Bold)) { Colspan = 6, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        foreach (var item in productTypeGroup.OrderBy(x => x.ProductName))
                        {
                            sl++;
                            tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(item.Percentage.ToString("N3"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                            tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString("N3"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        }
                        tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(sumPercentage.ToString("N3"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(sumQuantity.ToString("N3"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("Total (Including Bag Quantity): ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(summationOfPercentage.ToString("N3"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase(summationOfQuantity.ToString("N3"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase("Total (Excluding Bag Quantity): ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase((summationOfPercentage - packagingMaterialsQtyPercentage).ToString("N3"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableBillOfMaterialItems.AddCell(new PdfPCell(new Phrase((summationOfQuantity - packagingMaterialsQty).ToString("N3"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableBillOfMaterialItems);

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

            var reportTitle = "Feed Formula";

            if (reportType == 2)
            {
                var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tableBillOfMaterialForExcel, tableBillOfMaterialItems, SheetNameGenerator.Generate("Sheet1", null, null), tenantData?.Name, tenantData?.Address, reportTitle);
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
    }
}
