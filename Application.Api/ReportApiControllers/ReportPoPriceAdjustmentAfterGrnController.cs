using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Core.Extensions;

namespace Application.Api.ReportApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportPoPriceAdjustmentAfterGrnController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportPoPriceAdjustmentAfterGrnController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{poPriceAdjustmentId}")]
        public async Task<IActionResult> PoPriceAdjustmentAfterGrns(Guid poPriceAdjustmentId)
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
            headerPage.AddCell(new PdfPCell(new Phrase("GRN Adjustment Voucher", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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

            var userName = _workContext.GetUserName() ?? "";
            // =============
            // Po Price Adjustment After Grn
            // =============
            var poPriceAdjustmentAfterGrn = _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().TableNoTracking().Include(x => x.Supplier).Include(x => x.Store).Where(d => d.Id == poPriceAdjustmentId).FirstOrDefault();

            if (poPriceAdjustmentAfterGrn is not null)
            {
                String? code = poPriceAdjustmentAfterGrn.Code;
                String? ponumber = poPriceAdjustmentAfterGrn.Ponumber;
                String? grnno = poPriceAdjustmentAfterGrn.Grnno;
                String? supplierName = poPriceAdjustmentAfterGrn.Supplier.Name;
                String? storeName = poPriceAdjustmentAfterGrn.Store.Name;
                decimal? totalAmount = poPriceAdjustmentAfterGrn.TotalAmount;
                String? adjustmentDate = poPriceAdjustmentAfterGrn.AdjustmentDate.ToLocal().ToString("dd/MM/yyyy");
                String? referenceNo = poPriceAdjustmentAfterGrn.ReferenceNo;
                String? remarks = poPriceAdjustmentAfterGrn.Remark;
                String? preparedBy = poPriceAdjustmentAfterGrn.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(poPriceAdjustmentAfterGrn.CheckedBy))
                    checkedBy = poPriceAdjustmentAfterGrn.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(poPriceAdjustmentAfterGrn.ApprovedBy))
                    approvedBy = poPriceAdjustmentAfterGrn.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tablePoPriceAdjustmentAfterGrn = new(4);
                float[] widthsCellsTablePoPriceAdjustmentAfterGrn = new float[] { 12f, 42f, 12f, 34f };
                tablePoPriceAdjustmentAfterGrn.SetWidths(widthsCellsTablePoPriceAdjustmentAfterGrn);
                tablePoPriceAdjustmentAfterGrn.WidthPercentage = 100;
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase("Code: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase(code, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase("Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase(adjustmentDate, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase("Supplier: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase(supplierName, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase("PO Number: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase(ponumber, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase("Reference:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase(referenceNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase("GRN No:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase(grnno, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePoPriceAdjustmentAfterGrn.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tablePoPriceAdjustmentAfterGrn);

                // ======================
                // Get Po Price Adjustment After Grn Item
                // ======================
                var poPriceAdjustmentAfterGrnItems = _unitOfWork.Repository<PoPriceAdjustmentAfterGrnDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.PoPriceAdjustmentAfterGrnId == poPriceAdjustmentId).Select(d => new
                                       {
                                           d.Id,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.GrnQuantity,
                                           d.GrnRate,
                                           d.AdjustmentQuantity,
                                           d.AdjustmentRate,
                                           d.Amount,
                                       }).ToList();

                if (poPriceAdjustmentAfterGrnItems.Any())
                {
                    PdfPTable tablePoPriceAdjustmentAfterGrnItems = new(8);
                    float[] widthsCellsPoPriceAdjustmentAfterGrnItems = new float[] { 5f, 22f, 6f, 12f, 12f, 14f, 14f, 15f };
                    tablePoPriceAdjustmentAfterGrnItems.SetWidths(widthsCellsPoPriceAdjustmentAfterGrnItems);
                    tablePoPriceAdjustmentAfterGrnItems.WidthPercentage = 100;

                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("Product", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("GRN Qty", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("GRN Rate", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("Adjustment Qty", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("Adjustment Rate", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("Variance", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });

                    int sl = 0;
                    foreach (var item in poPriceAdjustmentAfterGrnItems)
                    {
                        sl++;
                        tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(item.GrnQuantity.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(item.GrnRate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(item.AdjustmentQuantity.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(item.AdjustmentRate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase("Total Variance: ", fontArial8Bold)) { Colspan = 7, HorizontalAlignment = 2, Border = 0, PaddingTop = 10f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePoPriceAdjustmentAfterGrnItems.AddCell(new PdfPCell(new Phrase(totalAmount?.ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, Border = 0, PaddingTop = 10f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tablePoPriceAdjustmentAfterGrnItems);
                    document.Add(spaceTable);
                }

                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new(3);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });

                tableUsers.AddCell(new PdfPCell(new Phrase("Printed By: " + userName + ", Date: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 1f, HorizontalAlignment = 1 });
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
