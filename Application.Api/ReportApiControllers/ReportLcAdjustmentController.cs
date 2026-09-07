using Application.Api.Attributes;

using Application.Core.Common;
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
    [RequiresModule("report")]
    public class ReportLcAdjustmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportLcAdjustmentController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{lcAdjustmentId}")]
        public async Task<IActionResult> LcAdjustments(Guid lcAdjustmentId)
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

            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);

            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 10F)));
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("LC Adjustment", fontArial14Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = Element.ALIGN_CENTER });
            document.Add(headerPage);

            // =====
            // Space
            // =====
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 4f });

            document.Add(line);

            // =============
            // Receive Payment
            // =============
            var userName = _workContext.GetUserName() ?? "";
            var lcAdjustment = _unitOfWork.Repository<LcAdjustment>().TableNoTracking().Include(x => x.CostCenter).Where(d => d.Id == lcAdjustmentId).FirstOrDefault();

            if (lcAdjustment is not null)
            {
                String? code = lcAdjustment.Code;
                String? purchaseInvoiceNo = lcAdjustment.PurchaseInvoiceNo;
                String? costCenterName = lcAdjustment.CostCenter.Name;
                String? adjustmentDate = lcAdjustment.AdjustmentDate.ToLocal().ToString("dd/MM/yyyy");
                String? remarks = lcAdjustment.Remark;
                String? preparedBy = lcAdjustment.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(lcAdjustment.CheckedBy))
                    checkedBy = lcAdjustment.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(lcAdjustment.ApprovedBy))
                    approvedBy = lcAdjustment.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableLcAdjustment = new(4);
                float[] widthscellsTableLcAdjustment = new float[] { 20f, 35f, 15f, 30f };
                tableLcAdjustment.SetWidths(widthscellsTableLcAdjustment);
                tableLcAdjustment.WidthPercentage = 100;
                tableLcAdjustment.AddCell(new PdfPCell(new Phrase("Code: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcAdjustment.AddCell(new PdfPCell(new Phrase(code, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcAdjustment.AddCell(new PdfPCell(new Phrase("Date:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcAdjustment.AddCell(new PdfPCell(new Phrase(adjustmentDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcAdjustment.AddCell(new PdfPCell(new Phrase("Purchase Invoice No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcAdjustment.AddCell(new PdfPCell(new Phrase(purchaseInvoiceNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcAdjustment.AddCell(new PdfPCell(new Phrase("Cost Center: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcAdjustment.AddCell(new PdfPCell(new Phrase(costCenterName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableLcAdjustment);

                document.Add(spaceTable);

                // ======================
                //  Get Receive Payment Item
                // ======================
                var lcAdjustmentItems = _unitOfWork.Repository<LcAdjustmentDetail>().TableNoTracking().Include(x => x.Account).Where(d =>
                                       d.LcAdjustmentId == lcAdjustment.Id).Select(d => new
                                       {
                                           d.Id,
                                           Code = d.Account.Code,
                                           AccountName = d.Account.Name,
                                           d.PostType,
                                           d.Amount,
                                       });

                var totalDebit = 0.0M;
                var totalCredit = 0.0M;
                if (lcAdjustmentItems.Any())
                {
                    PdfPTable tableLcAdjustmentItems = new(5);
                    float[] widthsCellsTableLcAdjustmentItems = new float[] { 10f, 45f, 15f, 15f, 15f };
                    tableLcAdjustmentItems.SetWidths(widthsCellsTableLcAdjustmentItems);
                    tableLcAdjustmentItems.WidthPercentage = 100;
                    tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase("Account Name / Description", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase("DR", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase("CR", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var lcAdjustmentItem in lcAdjustmentItems)
                    {
                        if (lcAdjustmentItem.PostType == 1)
                            totalDebit += lcAdjustmentItem.Amount;
                        else
                            totalCredit += lcAdjustmentItem.Amount;
                        sl++;
                        tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase(lcAdjustmentItem.AccountName, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase(lcAdjustmentItem.Code, fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase(lcAdjustmentItem.PostType == 1 ? lcAdjustmentItem.Amount.ToString("#,##0.00") : "0.00", fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase(lcAdjustmentItem.PostType == 2 ? lcAdjustmentItem.Amount.ToString("#,##0.00") : "0.00", fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial9Bold)) { Colspan = 3, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase(totalDebit.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableLcAdjustmentItems.AddCell(new PdfPCell(new Phrase(totalCredit.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableLcAdjustmentItems);
                    document.Add(spaceTable);
                }
                // ==============
                // Calculation
                // ==============

                PdfPTable calculationTable = new(2);
                float[] widthsCellsCalculationTable = new float[] { 18f, 82f };
                calculationTable.WidthPercentage = 100;
                calculationTable.SetWidths(widthsCellsCalculationTable);
                calculationTable.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(totalDebit), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Narration: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(remarks, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                document.Add(calculationTable);
                document.Add(spaceTable);

                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new(3);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
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
