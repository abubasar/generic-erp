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
    public class ReportPaymentVoucherController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportPaymentVoucherController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{paymentVoucherId}")]
        public async Task<IActionResult> PaymentVouchers(Guid paymentVoucherId)
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

            var paymentVoucher = _unitOfWork.Repository<PaymentVoucher>().TableNoTracking().Include(x => x.CostCenter).Include(x => x.PaymentMode).Where(d => d.Id == paymentVoucherId).FirstOrDefault();

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
            //headerPage.AddCell(new PdfPCell(new Phrase("Cash Received Voucher", fontArial14Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Cash Payment Voucher", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Payment Voucher
            // =============

            if (paymentVoucher is not null)
            {

                String? voucherNo = paymentVoucher.VoucherNo;
                String cashBankAccountName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == paymentVoucher.CashBankAccountId)?.Name ?? "";
                String code = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == paymentVoucher.CashBankAccountId)?.Code ?? "";
                String? voucherDate = DateTimeHelper.UtcToLocal(paymentVoucher.VoucherDate).ToString("dd/MM/yyyy");
                String? costCenterName = paymentVoucher.CostCenter.Name;
                String? paymentModeName = paymentVoucher.PaymentMode.Name;
                String? transactionNo = paymentVoucher.TransactionNumber;
                String? referenceNo = paymentVoucher.ReferenceNo;
                Decimal totalAmount = paymentVoucher.TotalAmount;
                String? remark = paymentVoucher.Remark;
                String? preparedBy = paymentVoucher.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(paymentVoucher.CheckedBy))
                    checkedBy = paymentVoucher.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(paymentVoucher.ApprovedBy))
                    approvedBy = paymentVoucher.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tablePaymentVoucher = new(4);
                float[] widthscellsTableSaleOrder = new float[] { 15f, 35f, 35f, 15f };
                tablePaymentVoucher.SetWidths(widthscellsTableSaleOrder);
                tablePaymentVoucher.WidthPercentage = 100;
                tablePaymentVoucher.AddCell(new PdfPCell(new Phrase("Voucher No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePaymentVoucher.AddCell(new PdfPCell(new Phrase(voucherNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePaymentVoucher.AddCell(new PdfPCell(new Phrase("Date:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePaymentVoucher.AddCell(new PdfPCell(new Phrase(voucherDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePaymentVoucher.AddCell(new PdfPCell(new Phrase("Cost Center: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePaymentVoucher.AddCell(new PdfPCell(new Phrase(costCenterName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePaymentVoucher.AddCell(new PdfPCell(new Phrase("Ref. No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                tablePaymentVoucher.AddCell(new PdfPCell(new Phrase(referenceNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                document.Add(tablePaymentVoucher);

                document.Add(spaceTable);

                // ======================
                // Get Payment Voucher Details
                // ======================
                var userName = _workContext.GetUserName() ?? "";
                var paymentVoucherItems = _unitOfWork.Repository<PaymentVoucherDetail>().TableNoTracking().Include(x => x.Account).Where(d => d.PaymentVoucherId == paymentVoucherId).Select(d => new
                {
                    Id = d.Id,
                    AccountName = d.Account.Name,
                    AccountTypeName = d.Account.AccountType.Name,
                    Code = d.Account.Code,
                    Amount = d.Amount
                });
                if (paymentVoucherItems.Any())
                {
                    int sl = 1;
                    PdfPTable tablePaymentVoucherItems = new(5);
                    float[] widthsCellsPaymentVoucherItems = new float[] { 10f, 45f, 15f, 15f, 15f };
                    tablePaymentVoucherItems.SetWidths(widthsCellsPaymentVoucherItems);
                    tablePaymentVoucherItems.WidthPercentage = 100;
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase("Account Name / Description", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase("DR", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase("CR", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(cashBankAccountName, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(code, fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });

                    foreach (var paymentVoucherItem in paymentVoucherItems)
                    {
                        sl++;
                        tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(paymentVoucherItem.AccountName, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(paymentVoucherItem.Code, fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(paymentVoucherItem.Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }

                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial9Bold)) { Colspan = 3, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePaymentVoucherItems.AddCell(new PdfPCell(new Phrase(totalAmount.ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tablePaymentVoucherItems);
                    document.Add(spaceTable);
                }
                // ==============
                // Calculation
                // ==============

                PdfPTable calculationTable = new(2);
                float[] widthsCellsCalculationTable = new float[] { 18f, 82f };
                calculationTable.WidthPercentage = 100;
                calculationTable.SetWidths(widthsCellsCalculationTable);
                calculationTable.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial9Bold)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(totalAmount), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Narration: ", fontArial9Bold)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(remark, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Payment Mode: ", fontArial9Bold)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(paymentModeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Transaction No: ", fontArial9Bold)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(transactionNo, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                document.Add(calculationTable);
                document.Add(spaceTable);
                document.Add(spaceTable);
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
