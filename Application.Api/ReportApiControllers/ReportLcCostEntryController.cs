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
    public class ReportLcCostEntryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportLcCostEntryController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{lcCostEntryId}")]
        public async Task<IActionResult> LcCostEntries(Guid lcCostEntryId)
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
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
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
            headerPage.AddCell(new PdfPCell(new Phrase("Import Voucher", fontArial14Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = Element.ALIGN_CENTER });
            document.Add(headerPage);

            // =====
            // Space
            // =====
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 4f });

            document.Add(line);

            // =============
            // Receive Payment
            // =============
            var userName = _workContext.GetUserName() ?? "";
            var lcCostEntry = _unitOfWork.Repository<LccostEntry>().TableNoTracking().Include(x => x.CostCenter).Where(d => d.Id == lcCostEntryId).FirstOrDefault();

            if (lcCostEntry is not null)
            {
                String? code = lcCostEntry.Code;
                String? poNumber = lcCostEntry.Ponumber;
                String? lcNumber = lcCostEntry.LcNumber;
                String? costCenterName = lcCostEntry.CostCenter?.Name;
                String? entryDate = lcCostEntry.EntryDate.ToLocal().ToString("dd/MM/yyyy");
                Decimal? totalAmount = lcCostEntry.Total;
                String? remarks = lcCostEntry.Remark;
                String? preparedBy = lcCostEntry.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(lcCostEntry.CheckedBy))
                    checkedBy = lcCostEntry.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(lcCostEntry.ApprovedBy))
                    approvedBy = lcCostEntry.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableLcCostEntry = new(4);
                float[] widthscellsTableLcCostEntry = new float[] { 60f, 130f, 60f, 130f };
                tableLcCostEntry.SetWidths(widthscellsTableLcCostEntry);
                tableLcCostEntry.WidthPercentage = 100;
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase("Voucher No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase(code, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase("Date:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase(entryDate, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase("LC Number: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase(lcNumber, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase("Cost Center: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase(costCenterName, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase("PO Number: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableLcCostEntry.AddCell(new PdfPCell(new Phrase(poNumber, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableLcCostEntry);

                document.Add(spaceTable);

                // ======================
                //  Get LC Cost Entry Item
                // ======================
                var lcCostEntryItems = _unitOfWork.Repository<LccostEntryDetail>().TableNoTracking().Include(x => x.DebitAccount).Include(x => x.CreditAccount).Where(d =>
                                       d.LccostEntryId == lcCostEntry.Id).Select(d => new
                                       {
                                           d.Id,
                                           DebitAccoutCode = d.DebitAccount.Code,
                                           CreditAccoutCode = d.CreditAccount.Code,
                                           DebitAccountName = d.DebitAccount.Name,
                                           CreditAccountName = d.CreditAccount.Name,
                                           d.Amount,
                                           d.IsIncludedWithinLandedCost,
                                       });

                if (lcCostEntryItems.Any())
                {
                    PdfPTable tableLcCostEntryItems = new(5);
                    float[] widthsCellsTableLcCostEntryItems = new float[] { 10f, 45f, 15f, 15f, 15f };
                    tableLcCostEntryItems.SetWidths(widthsCellsTableLcCostEntryItems);
                    tableLcCostEntryItems.WidthPercentage = 100;
                    tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase("Account Name / Description", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase("DR", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase("CR", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var lcCostEntryItem in lcCostEntryItems)
                    {
                        sl++;
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(lcCostEntryItem.DebitAccountName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(lcCostEntryItem.DebitAccoutCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(lcCostEntryItem.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });

                        sl++;
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(lcCostEntryItem.CreditAccountName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(lcCostEntryItem.CreditAccoutCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(lcCostEntryItem.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 3, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(totalAmount?.ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableLcCostEntryItems.AddCell(new PdfPCell(new Phrase(totalAmount?.ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableLcCostEntryItems);
                }

                // ==============
                // Calculation
                // ==============

                PdfPTable calculationTable = new(2);
                float[] widthsCellsCalculationTable = new float[] { 18f, 82f };
                calculationTable.WidthPercentage = 100;
                calculationTable.SetWidths(widthsCellsCalculationTable);
                calculationTable.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial8)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(totalAmount ?? 0), fontArial8)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Narration: ", fontArial8)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
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
                tableUsers.AddCell(new PdfPCell(new Phrase("Printed By: " + userName + ", Date: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial7)) { Colspan = 3, Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial7)) { Colspan = 3, Border = 0, PaddingTop = 0f, HorizontalAlignment = 1 });
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
