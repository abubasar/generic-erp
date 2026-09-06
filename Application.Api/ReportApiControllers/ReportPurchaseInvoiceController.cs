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
    public class ReportPurchaseInvoiceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportPurchaseInvoiceController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{purchaseInvoiceId}")]
        public async Task<IActionResult> GoodsReceiveNotes(Guid purchaseInvoiceId)
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
            headerPage.AddCell(new PdfPCell(new Phrase("Purchase Invoice", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Goods Receive Note
            // =============
            var purchaseInvoice = _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().Include(x => x.Supplier).Include(x => x.Store).Where(d => d.Id == purchaseInvoiceId).FirstOrDefault();

            if (purchaseInvoice is not null)
            {
                String? supplierCode = purchaseInvoice.Supplier.Code;
                String? supplierName = purchaseInvoice.Supplier.Name;
                String? address = purchaseInvoice.Supplier.Address;
                String? contactNo = purchaseInvoice.Supplier.ContactNo;
                String? email = purchaseInvoice.Supplier.Email;
                String? storeName = purchaseInvoice.Store.Name;
                String? poNumber = purchaseInvoice.Ponumber;
                String? grnNo = purchaseInvoice.Grnno;
                String? purchaseInvoiceNo = purchaseInvoice.PurchaseInvoiceNo;
                String? invoiceDate = purchaseInvoice.InvoiceDate.ToString("dd-MMM-yyyy");
                String? supplierInvoiceNo = purchaseInvoice.SupplierInvoiceNo;
                String? supplierInvoiceDate = purchaseInvoice.SupplierInvoiceDate?.ToString("dd-MMM-yyyy");
                int? paymentTermInDays = purchaseInvoice.PaymentTermInDays;
                decimal? subtotal = purchaseInvoice.Subtotal;
                decimal? discount = purchaseInvoice.Discount;
                decimal? total = purchaseInvoice.Total;
                decimal? advancePaymentAmount = purchaseInvoice.AdvancePaymentAmount;
                decimal? totalGrnAdjustmentAmount = purchaseInvoice.TotalGrnAdjustmentAmount;
                decimal? adjustmentValue = purchaseInvoice.AdjustmentValue;
                decimal? netPayable = purchaseInvoice.NetPayable;
                String? createdOn = purchaseInvoice.CreatedOn.ToString("dd-MMM-yyyy");
                String? remarks = purchaseInvoice.Remark;
                String? preparedBy = purchaseInvoice.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(purchaseInvoice.CheckedBy))
                    checkedBy = purchaseInvoice.CheckedBy;
                else
                    checkedBy = "";
                String? verifiedBy;
                if (!string.IsNullOrEmpty(purchaseInvoice.ApprovedBy))
                    verifiedBy = purchaseInvoice.ApprovedBy;
                else
                    verifiedBy = "";

                PdfPTable tablePurchaseInvoice = new(4);
                float[] widthscellsTablePurchaseInvoice = new float[] { 60f, 130f, 70f, 120f };
                tablePurchaseInvoice.SetWidths(widthscellsTablePurchaseInvoice);
                tablePurchaseInvoice.WidthPercentage = 100;
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("Supplier: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(supplierName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("Purchase Invoice No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(purchaseInvoiceNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("Address: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(address, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("Invoice Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(invoiceDate, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(contactNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("GRN No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(grnNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("Store: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(storeName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("PO No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(poNumber, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseInvoice.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tablePurchaseInvoice);

                // ======================
                // Get Goods Receive Note Item
                // ======================
                var purchaseInvoiceItems = _unitOfWork.Repository<PurchaseInvoiceDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.PurchaseInvoiceId == purchaseInvoiceId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.Grnquantity,
                                           d.Rate,
                                           d.Amount,
                                       });

                if (purchaseInvoiceItems.Any())
                {
                    PdfPTable tablePurchaseInvoiceItems = new(7);
                    float[] widthsCellsPurchaseInvoiceItems = new float[] { 7f, 8f, 40f, 10f, 10f, 10f, 15f };
                    tablePurchaseInvoiceItems.SetWidths(widthsCellsPurchaseInvoiceItems);
                    tablePurchaseInvoiceItems.WidthPercentage = 100;
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Qty", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var item in purchaseInvoiceItems)
                    {
                        sl++;
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(item.Grnquantity.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(purchaseInvoiceItems.Sum(x => x.Grnquantity).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { Colspan = 2, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Subtotal: ", fontArial8Bold)) { Colspan = 6, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(subtotal?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    if (purchaseInvoice.IsImportPurchase)
                    {
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Adjustment Value: ", fontArial8Bold)) { Colspan = 6, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(adjustmentValue?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    else
                    {
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Discount: ", fontArial8Bold)) { Colspan = 6, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(discount?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 6, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(total?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Advance Payment Amount: ", fontArial8Bold)) { Colspan = 6, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(advancePaymentAmount?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Total GRN Adjustment Amount: ", fontArial8Bold)) { Colspan = 6, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(totalGrnAdjustmentAmount?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase("Net Payable: ", fontArial8Bold)) { Colspan = 6, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tablePurchaseInvoiceItems.AddCell(new PdfPCell(new Phrase(netPayable?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tablePurchaseInvoiceItems);

                    document.Add(spaceTable);

                    //NumberToWords Table
                    PdfPTable tableNumberToWords = new(2);
                    float[] widthsCellsTableNumberToWords = new float[] { 20f, 80f };
                    tableNumberToWords.WidthPercentage = 100;
                    tableNumberToWords.SetWidths(widthsCellsTableNumberToWords);
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial8Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(netPayable ?? 0), fontArial8)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableNumberToWords);
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
                tableUsers.AddCell(new PdfPCell(new Phrase(verifiedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
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
