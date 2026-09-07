using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Entities;
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
    public class ReportFundTransferController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportFundTransferController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{fundTransferId}")]
        public async Task<IActionResult> FundTransfers(Guid fundTransferId)
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
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial11 = FontFactory.GetFont("Arial", 11);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
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
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Fund Transfer", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
            document.Add(headerPage);

            // =====
            // Space
            // =====
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

            document.Add(line);

            // =============
            // Fund Transfer
            // =============

            var fundTransfer = _unitOfWork.Repository<FundTransfer>().TableNoTracking().Include(x => x.FundTransferTransactionType).Include(x => x.CostCenter).Include(x => x.TransferFromAccount).Include(x => x.TransferToAccount).Where(d => d.Id == fundTransferId).FirstOrDefault();


            if (fundTransfer is not null)
            {
                String? fundTransferNo = fundTransfer.FundTransferNo;
                String? fundTransferDate = DateTimeHelper.UtcToLocal(fundTransfer.FundTransferDate).ToString("dd/MM/yyyy");
                String? transferTransactionType = fundTransfer.FundTransferTransactionType.Name;
                String? transferFromAccountName = fundTransfer.TransferFromAccount.Name;
                String? transferFromAccountCode = fundTransfer.TransferFromAccount.Code;
                String? transferToAccountName = fundTransfer.TransferToAccount.Name;
                String? transferToAccountCode = fundTransfer.TransferToAccount.Code;
                String? costCenterName = fundTransfer.CostCenter.Name;
                Decimal amount = fundTransfer.Amount;
                Decimal charges = fundTransfer.Charges;
                Decimal amountWithCharges = amount + charges;
                String? remark = fundTransfer.Remark;
                String? preparedBy = fundTransfer.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(fundTransfer.CheckedBy))
                    checkedBy = fundTransfer.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(fundTransfer.ApprovedBy))
                    approvedBy = fundTransfer.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableFundTransfer = new(4);
                float[] widthscellsTableSaleOrder = new float[] { 16f, 34f, 35f, 15f };
                tableFundTransfer.SetWidths(widthscellsTableSaleOrder);
                tableFundTransfer.WidthPercentage = 100;
                tableFundTransfer.AddCell(new PdfPCell(new Phrase("Fund Transfer No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                tableFundTransfer.AddCell(new PdfPCell(new Phrase(fundTransferNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                tableFundTransfer.AddCell(new PdfPCell(new Phrase("Date:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                tableFundTransfer.AddCell(new PdfPCell(new Phrase(fundTransferDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                tableFundTransfer.AddCell(new PdfPCell(new Phrase("Transaction Type: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                tableFundTransfer.AddCell(new PdfPCell(new Phrase(transferTransactionType, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                tableFundTransfer.AddCell(new PdfPCell(new Phrase("Cost Center: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                tableFundTransfer.AddCell(new PdfPCell(new Phrase(costCenterName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });

                document.Add(tableFundTransfer);

                document.Add(spaceTable);

                var userName = _workContext.GetUserName() ?? "";

                PdfPTable tableFundTransferItems = new(5);
                float[] widthsCellsFundTransferItems = new float[] { 10f, 45f, 15f, 15f, 15f };
                tableFundTransferItems.SetWidths(widthsCellsFundTransferItems);
                tableFundTransferItems.WidthPercentage = 100;
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("Account Name / Description", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("DR", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("CR", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f });

                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("1", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(transferFromAccountName, fontArial9)) { HorizontalAlignment = Element.ALIGN_LEFT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(transferFromAccountCode, fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial9)) { HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(amountWithCharges.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });

                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("2", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("Bank Charge", fontArial9)) { HorizontalAlignment = Element.ALIGN_LEFT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("11.01.01", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(fundTransfer?.Charges == null ? "0.00" : charges.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial9)) { HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });

                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("3", fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(transferToAccountName, fontArial9)) { HorizontalAlignment = Element.ALIGN_LEFT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(transferToAccountCode, fontArial9)) { HorizontalAlignment = Element.ALIGN_CENTER, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial9)) { HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });


                tableFundTransferItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial9Bold)) { Colspan = 3, HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(amountWithCharges.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                tableFundTransferItems.AddCell(new PdfPCell(new Phrase(amountWithCharges.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = Element.ALIGN_RIGHT, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                document.Add(tableFundTransferItems);
                document.Add(spaceTable);

                // ==============
                // Calculation
                // ==============

                PdfPTable calculationTable = new(2);
                float[] widthsCellsCalculationTable = new float[] { 18f, 82f };
                calculationTable.WidthPercentage = 100;
                calculationTable.SetWidths(widthsCellsCalculationTable);
                calculationTable.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial9Bold)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                calculationTable.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(amountWithCharges), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                calculationTable.AddCell(new PdfPCell(new Phrase("Narration: ", fontArial9Bold)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_LEFT });
                calculationTable.AddCell(new PdfPCell(new Phrase(remark, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_LEFT });

                document.Add(calculationTable);
                document.Add(spaceTable);
                document.Add(spaceTable);
                document.Add(spaceTable);

                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new(4);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Received by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase("Printed By: " + userName + ", Date: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
                tableUsers.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
