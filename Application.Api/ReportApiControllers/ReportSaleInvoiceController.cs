using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpReport.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("report")]
    public class ReportSaleInvoiceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportSaleInvoiceController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{saleInvoiceId}")]
        public async Task<IActionResult> Sales(Guid saleInvoiceId)
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
            Font fontArial17Bold = FontFactory.GetFont("Arial", 17, Font.BOLD);
            Font fontArial15Bold = FontFactory.GetFont("Arial", 15, Font.BOLD);
            Font fontArial11 = FontFactory.GetFont("Arial", 11);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 4.5F)));


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
            headerPage.AddCell(new PdfPCell(new Phrase("BIN No: 000318322-0306", fontArial11)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Sale Invoice", fontArial17Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
            document.Add(headerPage);

            // =====
            // Space
            // =====
            PdfPTable spaceTable = new(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

            document.Add(line);

            // =============
            // Sales Invoice
            // =============
            var userName = _workContext.GetUserName() ?? "";
            var salesInvoice = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d =>
                                d.Id == saleInvoiceId).FirstOrDefaultAsync();

            if (salesInvoice is not null)
            {
                String? customerCode = salesInvoice.Customer.Code;
                String? customerName = salesInvoice.Customer.Name;
                String? customerAddress = salesInvoice.Customer.Address;
                String? customerContactNo = salesInvoice.Customer.ContactNo;
                String? storeName = salesInvoice.Store.Name;
                String? transport = Enum.GetName(typeof(Transport), salesInvoice.Transport)?.Replace("_", " ");
                String? invoiceNo = salesInvoice.InvoiceNo;
                String? deliveryNoteNo = salesInvoice.DeliveryNoteNo;
                String? saleOrderNo = salesInvoice.SaleOrderNo;
                String? invoiceDate = DateTimeHelper.UtcToLocal(salesInvoice.InvoiceDate).ToString("dd/MM/yyyy");
                decimal? otherDiscount = salesInvoice.OtherDiscount;
                String? orderDate = "";
                if (salesInvoice.OrderDate.HasValue)
                {
                    orderDate = DateTimeHelper.UtcToLocal(salesInvoice.OrderDate.Value).ToString("dd/MM/yyyy");
                }
                decimal? subtotal = salesInvoice.Subtotal;
                decimal? total = salesInvoice.NetTotal;
                decimal? depoCharge = salesInvoice.DepoCharge;
                String? remarks = salesInvoice.Remark;
                string? moneyReceiptNo = salesInvoice.MoneyReceiptNo;
                decimal? balance = salesInvoice.Balance;
                var paymentAmount = 0M;
                if (!string.IsNullOrWhiteSpace(moneyReceiptNo))
                {
                    var arr = moneyReceiptNo.Split(',');
                    foreach (var code in arr)
                    {
                        var moneyReceipt = _unitOfWork.Repository<ReceivePayment>().TableNoTracking().SingleOrDefault(x => x.Code == code);
                        if (moneyReceipt is null) throw new Exception("Money Receipt Not Found !!");
                        paymentAmount += moneyReceipt.TotalAmount;
                    }
                }
                String? preparedBy = salesInvoice.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(salesInvoice.CheckedBy))
                    checkedBy = salesInvoice.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(salesInvoice.ApprovedBy))
                    approvedBy = salesInvoice.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableSalesInvoice = new(4);
                float[] widthscellsTableSaleOrder = new float[] { 15f, 40f, 17f, 28f };
                tableSalesInvoice.SetWidths(widthscellsTableSaleOrder);
                tableSalesInvoice.WidthPercentage = 100;
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Customer Code: ", fontArial9)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerCode, fontArial9)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Invoice No: ", fontArial9)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(invoiceNo, fontArial9)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Name: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Invoice Date: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(invoiceDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Address: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerAddress, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Challan No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(deliveryNoteNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Sale Order No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(saleOrderNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Transport: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(transport, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Sale Order Date:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(orderDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Depot:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(storeName, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(remarks, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableSalesInvoice);

                document.Add(spaceTable);

                // ======================
                // Get Sales Invoice Item
                // ======================
                var salesInvoiceItems = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.SaleInvoiceId == saleInvoiceId).Select(d => new
                                       {
                                           Id = d.Id,
                                           d.DeliveryDate,
                                           d.DeliveryPlace,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           BagWeight = d.Product.BagWeight,
                                           PrimaryQuantity = d.PrimaryQuantity,
                                           Quantity = d.Quantity,
                                           d.InvoiceDiscountPerUnit,
                                           d.CashDiscountPerUnit,
                                           d.SpecialDiscountPerUnit,
                                           Rate = d.Rate,
                                           DiscountAmount = d.DiscountAmount,
                                           Amount = d.Amount
                                       });
                var invoiceDiscount = 0m;
                var cashDiscount = 0m;
                var specialDiscount = 0m;

                if (salesInvoiceItems.Any())
                {
                    PdfPTable tableSalesOrderItems = new(11);
                    float[] widthsCellsSalesInvoiceItems = new float[] { 4f, 10f, 15f, 5f, 18f, 5f, 7f, 8f, 7f, 7f, 10f };
                    tableSalesOrderItems.SetWidths(widthsCellsSalesInvoiceItems);
                    tableSalesOrderItems.WidthPercentage = 100;
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial9)) { Colspan = 6, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9)) { Colspan = 3, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Rate & Amount", fontArial9)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Delivery Date", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Delivery Place", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Name", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Bag Weight", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Bag/PCS", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("Qty/KG", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("TP/KG", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesOrderItems.AddCell(new PdfPCell(new Phrase("TK", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });

                    int sl = 0;
                    foreach (var salesInvoiceItem in salesInvoiceItems)
                    {
                        invoiceDiscount += salesInvoiceItem.InvoiceDiscountPerUnit * salesInvoiceItem.Quantity;
                        cashDiscount += salesInvoiceItem.CashDiscountPerUnit * salesInvoiceItem.Quantity;
                        specialDiscount += salesInvoiceItem.SpecialDiscountPerUnit * salesInvoiceItem.Quantity;
                        sl++;
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.DeliveryDate?.ToString("dd/MM/yyyy"), fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.DeliveryPlace, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductCode, fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductName, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.UnitName, fontArial10)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.BagWeight.ToString(), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.PrimaryQuantity.ToString(), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Quantity.ToString(), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Rate.ToString(), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesOrderItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    }
                    document.Add(tableSalesOrderItems);

                    document.Add(spaceTable);
                }
                // ==============
                // Calculation
                // ==============

                PdfPTable calculationTable = new(5);
                float[] widthsCellsCalculationTable = new float[] { 15f, 14f, 37f, 20f, 14f };
                calculationTable.WidthPercentage = 100;
                calculationTable.SetWidths(widthsCellsCalculationTable);
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 12f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9Bold)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("TP Value: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 11f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(subtotal?.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 12f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Invoice Commission: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 11f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(invoiceDiscount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 12f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9Bold)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Cash Discount: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 11f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(cashDiscount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 12f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Special Discount: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 11f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(specialDiscount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 12f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Other Discount: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 11f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(otherDiscount?.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 12f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Depo Charge: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 11f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(depoCharge?.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("Balance Due: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 12f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase((balance + total)?.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Total Payable: ", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 11f, PaddingRight = 5f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(total?.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("*Vat Note: This is vat exemted product. By: SRO No-176-AIN/2022/176-VAT", fontArial9)) { Colspan = 5, Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                document.Add(calculationTable);

                document.Add(spaceTable);
                //line2
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 60.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
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
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            byteInfo = PdfHelper.AddFooter(byteInfo, userName);
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}
