using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Report.Sales
{
    public class SaleInvoicePdfService : ISaleInvoicePdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;

        public SaleInvoicePdfService(IUnitOfWork unitOfWork, IWorkContext workContext)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
        }
        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }
        public async Task PrintSaleInvoicePrimaryReportToPdfAsync(MemoryStream stream, Guid saleInvoiceId, TenantViewModel tenantData, string userName, string headerText)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 10.0F)));

            // Sales Invoice
            var salesInvoice = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Include(x => x.Customer).Include(x => x.CustomerTerritory).Include(x => x.Store).Where(d =>
                                d.Id == saleInvoiceId).FirstOrDefaultAsync();

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(tenantData.Logo!)));
            // Set the size of the image if needed
            logo.ScaleToFit(100, 80);
            logo.Alignment = Image.ALIGN_CENTER;
            logo.IndentationLeft = 0f;
            logo.IndentationRight = 0f;
            logo.SpacingBefore = 0f;
            logo.SpacingAfter = 0f;

            // Header Page
            PdfPTable headerTable = new(4);
            float[] widthsCellsHeaderTable = new float[] { 10f, 35f, 30f, 45f };
            headerTable.SetWidths(widthsCellsHeaderTable);
            headerTable.WidthPercentage = 100;
            headerTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 2, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            PdfPCell saleInvoiceCell = new PdfPCell(new Phrase("Sale Invoice", fontArial12Bold))
            {
                Border = Rectangle.NO_BORDER, // Disable default borders
                PaddingBottom = 4f,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
            };
            saleInvoiceCell.CellEvent = new RoundedRectangleCell(BaseColor.Black, 8f); // Border color and radius
            headerTable.AddCell(saleInvoiceCell);
            headerTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 2, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase(Enum.GetName(typeof(PaymentTerm), salesInvoice!.PaymentTerm)?.Replace("_", " "), fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            PdfPCell logoCell = new PdfPCell(logo);
            logoCell.Border = Rectangle.NO_BORDER;
            logoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            logoCell.Rowspan = 3;
            headerTable.AddCell(logoCell);
            headerTable.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial14Bold)) { Colspan = 3, Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });
            headerTable.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial9Bold)) { Colspan = 3, Border = 0, PaddingBottom = 3f, HorizontalAlignment = Element.ALIGN_LEFT });
            document.Add(headerTable);

            // Space
            PdfPTable spaceTable = new(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 5f });

            document.Add(line);

            if (salesInvoice is not null)
            {
                String? customerCode = salesInvoice.Customer.Code;
                String? customerName = salesInvoice.Customer.Name;
                String? customerAddress = salesInvoice.Customer.Address;
                String? customerContactNo = salesInvoice.Customer.ContactNo;
                String? storeName = salesInvoice.Store.Name;
                String? invoiceNo = salesInvoice.InvoiceNo;
                String? saleOrderNo = salesInvoice.SaleOrderNo;
                decimal? otherDiscount = salesInvoice.OtherDiscount;
                decimal? totalPercentageDiscountAmount = salesInvoice.TotalPercentageDiscountAmount;
                String? invoiceDate = DateTimeHelper.UtcToLocal(salesInvoice.InvoiceDate).ToString("dd/MM/yyyy");
                String? orderDate = "";
                if (salesInvoice.OrderDate.HasValue)
                {
                    orderDate = DateTimeHelper.UtcToLocal(salesInvoice.OrderDate.Value).ToString("dd/MM/yyyy");
                }
                decimal? subtotal = salesInvoice.Subtotal;
                decimal? total = salesInvoice.NetTotal;
                decimal? totalVat = salesInvoice.TotalVat;
                String? remarks = salesInvoice.Remark;
                String? territoryName = salesInvoice?.CustomerTerritory?.Name;
                decimal? balance = salesInvoice?.Balance;
                String? approvedBy;
                if (!string.IsNullOrEmpty(salesInvoice?.ApprovedBy))
                    approvedBy = salesInvoice.ApprovedBy;
                else
                    approvedBy = "";

                var marketingOfficerName = "";
                var marketingOfficerContactNo = "";
                if (salesInvoice!.CustomerMarketingOfficerId.HasValue)
                {
                    var marketingOfficer = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == salesInvoice.CustomerMarketingOfficerId.Value);
                    marketingOfficerName = marketingOfficer?.FullName;
                    marketingOfficerContactNo = marketingOfficer?.ContactNo;
                }
                ;

                PdfPTable tableSalesInvoice = new(4);
                float[] widthscellsTableSaleOrder = new float[] { 18f, 40f, 18f, 24f };
                tableSalesInvoice.SetWidths(widthscellsTableSaleOrder);
                tableSalesInvoice.WidthPercentage = 100;
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Customer Code: ", fontArial8Bold)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerCode, fontArial8Bold)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Invoice No: ", fontArial8Bold)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(invoiceNo, fontArial8Bold)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Customer Name: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerName, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Invoice Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(invoiceDate, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Address: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerAddress, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Sales Rep.: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(marketingOfficerName, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerContactNo, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Territory: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(territoryName, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Order No: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(saleOrderNo, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(marketingOfficerContactNo, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Order Date:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(orderDate, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Depot:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(storeName, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(remarks, fontArial8Bold)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableSalesInvoice);

                // Get Sales Invoice Item
                var salesInvoiceItems = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.SaleInvoiceId == saleInvoiceId).Select(d => new
                                       {
                                           Id = d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           PackSize = d.Product.PackSize,
                                           d.PrimaryQuantity,
                                           d.Quantity,
                                           d.BonusQuantity,
                                           d.Rate,
                                           d.VatPercentage,
                                           d.DiscountPercentage,
                                           d.PercentageDiscountAmount,
                                           d.Amount
                                       });

                if (salesInvoiceItems.Any())
                {
                    PdfPTable tableSalesInvoiceItems = new(13);
                    float[] widthsCellsSalesInvoiceItems = new float[] { 4f, 5f, 18f, 6f, 8f, 6f, 8f, 8f, 9f, 10f, 8f, 10f, 10f };
                    tableSalesInvoiceItems.SetWidths(widthsCellsSalesInvoiceItems);
                    tableSalesInvoiceItems.WidthPercentage = 100;
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Rate & Amount", fontArial8Bold)) { Colspan = 7, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Bonus", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit Price", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit Vat", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit Price with VAT", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Total VAT", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Discount (%)", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Total Price", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Net Amount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });

                    int sl = 0;
                    foreach (var salesInvoiceItem in salesInvoiceItems)
                    {
                        var perc = 1 + (salesInvoiceItem.VatPercentage / 100);
                        var basePrice = (salesInvoiceItem.Rate / perc);
                        var totalVatPerItem = salesInvoiceItem?.Quantity * (salesInvoiceItem?.Rate - basePrice);
                        sl++;
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.PackSize?.Name, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Quantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.BonusQuantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(basePrice.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase((salesInvoiceItem?.Rate - basePrice)?.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Rate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(totalVatPerItem?.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.DiscountPercentage.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase((salesInvoiceItem?.Amount - totalVatPerItem)?.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    }
                    document.Add(tableSalesInvoiceItems);
                }
                // Calculation
                PdfPTable calculationTable = new(5);
                float[] widthsCellsCalculationTable = new float[] { 15f, 16f, 33f, 20f, 16f };
                calculationTable.WidthPercentage = 100;
                calculationTable.SetWidths(widthsCellsCalculationTable);
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Gross TP: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase((subtotal - totalVat)?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Vat on TP: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(totalVat?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Discount on TP: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(totalPercentageDiscountAmount?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Other Discount on TP: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(otherDiscount?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Net Payable: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(total?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                var financialYear = _workContext.GetCurrentFinancialYear();
                var customerTransaction = await _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountId == salesInvoice.CustomerId && x.FinancialYearId == financialYear.Id).ToListAsync();
                var customerBalance = customerTransaction.Sum(x => x.Debit) - customerTransaction.Sum(x => x.Credit);

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Total Due (Including this invoice): " + customerBalance.ToString("#,##0.00"), fontArial9Bold)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 2, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });

                document.Add(calculationTable);
                document.Add(spaceTable);

                //var creditSalesInvoices = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(d =>
                //                    d.CustomerId == salesInvoice.CustomerId && d.AvailableReceivable > 0 && d.InvoiceDate.Date <= salesInvoice.InvoiceDate.Date).OrderBy(x => x.InvoiceDate).ToListAsync();
                //if (creditSalesInvoices.Any())
                //{
                //    PdfPTable creditInviceTable = new(4);
                //    float[] widthsCellsCreditInviceTable = new float[] { 100f, 100f, 100f, 100f };
                //    creditInviceTable.WidthPercentage = 100;
                //    creditInviceTable.SetWidths(widthsCellsCreditInviceTable);
                //    creditInviceTable.AddCell(new PdfPCell(new Phrase("Date", fontArial9Bold)) { Border = 0, CellEvent = new CustomDashedTopBottomCell(BaseColor.Black, 4f, 4f), PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                //    creditInviceTable.AddCell(new PdfPCell(new Phrase("Invoice No", fontArial9Bold)) { Border = 0, CellEvent = new CustomDashedTopBottomCell(BaseColor.Black, 4f, 4f), PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                //    creditInviceTable.AddCell(new PdfPCell(new Phrase("Invoice Amount", fontArial9Bold)) { Border = 0, CellEvent = new CustomDashedTopBottomCell(BaseColor.Black, 4f, 4f), PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                //    creditInviceTable.AddCell(new PdfPCell(new Phrase("Due Amount", fontArial9Bold)) { Border = 0, CellEvent = new CustomDashedTopBottomCell(BaseColor.Black, 4f, 4f), PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                //    foreach (var item in creditSalesInvoices)
                //    {
                //        creditInviceTable.SetWidths(widthsCellsCreditInviceTable);
                //        creditInviceTable.AddCell(new PdfPCell(new Phrase(item.InvoiceDate.ToString("dd/MM/yyyy"), fontArial9Bold)) { Border = 0, PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                //        creditInviceTable.AddCell(new PdfPCell(new Phrase(item.InvoiceNo, fontArial9Bold)) { Border = 0, PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                //        creditInviceTable.AddCell(new PdfPCell(new Phrase(item.NetTotal.ToString("#,##0.00"), fontArial9Bold)) { Border = 0, PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                //        creditInviceTable.AddCell(new PdfPCell(new Phrase(item.AvailableReceivable.ToString("#,##0.00"), fontArial9Bold)) { Border = 0, PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                //    }
                //    creditInviceTable.AddCell(new PdfPCell(new Phrase("Total Due (Including this invoice): ", fontArial9Bold)) { Colspan = 3, Border = 0, CellEvent = new CustomDashedTopBottomCell(BaseColor.Black, 4f, 0f), PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                //    creditInviceTable.AddCell(new PdfPCell(new Phrase(creditSalesInvoices.Sum(x => x.AvailableReceivable).ToString("#,##0.00"), fontArial9Bold)) { Border = 0, CellEvent = new CustomDashedTopBottomCell(BaseColor.Black, 4f, 0f), PaddingLeft = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                //    var creditInvoiceDetailsCell = GetPdfCell("", fontArial8);
                //    creditInvoiceDetailsCell.Colspan = 3;
                //    creditInvoiceDetailsCell.Border = 0;
                //    creditInvoiceDetailsCell.PaddingLeft = 5f;
                //    creditInvoiceDetailsCell.PaddingTop = 3f;
                //    creditInvoiceDetailsCell.PaddingBottom = 3f;
                //    creditInvoiceDetailsCell.HorizontalAlignment = 0;
                //    creditInvoiceDetailsCell.AddElement(creditInviceTable);
                //    calculationTable.AddCell(creditInvoiceDetailsCell);
                //    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 2, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                //    document.Add(calculationTable);
                //    document.Add(spaceTable);
                //}
                //else
                //{
                //    document.Add(calculationTable);
                //    document.Add(spaceTable);
                //}

                //line2
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 60.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                Paragraph line3 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 60.0F, BaseColor.Black, Element.ALIGN_RIGHT, 4.5F)));

                PdfPTable descriptionTable = new(3);
                float[] widthsCellsDescriptionTable = new float[] { 10f, 55f, 38f };
                descriptionTable.WidthPercentage = 100;
                descriptionTable.SetWidths(widthsCellsDescriptionTable);

                descriptionTable.AddCell(new PdfPCell(new Phrase("Warranty: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                descriptionTable.AddCell(new PdfPCell(new Phrase("Hereby give the warranty that products sold under this invoice don't contravene provisions of section 18 of the drug Act 1940 & Bangladesh drug Ordiance 1982 ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                descriptionTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                descriptionTable.AddCell(new PdfPCell(new Phrase("Note: ", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                descriptionTable.AddCell(new PdfPCell(new Phrase("PRODUCTS SOLD ARE RETURNABLE AS PER THE CONDITIONS", fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                descriptionTable.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                descriptionTable.AddCell(new PdfPCell(new Phrase("")) { Colspan = 2, Border = 0, HorizontalAlignment = 1, Padding = 0 });
                descriptionTable.AddCell(new PdfPCell(new Phrase(line3)) { Border = 0, HorizontalAlignment = 2, Padding = 0 });
                descriptionTable.AddCell(new PdfPCell(new Phrase("For" + " " + tenantData!.Name, fontArial8Bold)) { Colspan = 3, Border = 0, Padding = 0, HorizontalAlignment = 2 });

                document.Add(descriptionTable);
                document.Add(spaceTable);
                document.Add(spaceTable);
                document.Add(spaceTable);
                document.Add(spaceTable);



                // User Signature
                PdfPTable tableUsers = new(3);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Received By", fontArial9)) { Border = 0, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Delivery Officer", fontArial9)) { Border = 0, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Invoice Officer", fontArial9)) { Border = 0, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();
        }

        public async Task PrintSaleInvoiceSecondaryReportToPdfAsync(MemoryStream stream, Guid saleInvoiceId, TenantViewModel tenantData, string userName, string headerText)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            Rectangle rectangle = new(PageSize.A5);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(10f, 10f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 10.0F)));

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
            headerPage.AddCell(new PdfPCell(new Phrase("BIN No: " + tenantData?.Binno, fontArial9)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Sale Invoice", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Sales Invoice
            // =============
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
                decimal? offerDiscount = salesInvoice.OfferDiscount;
                decimal? otherDiscount = salesInvoice.OtherDiscount;
                String? invoiceDate = DateTimeHelper.UtcToLocal(salesInvoice.InvoiceDate).ToString("dd/MM/yyyy");
                String? orderDate = "";
                if (salesInvoice.OrderDate.HasValue)
                {
                    orderDate = DateTimeHelper.UtcToLocal(salesInvoice.OrderDate.Value).ToString("dd/MM/yyyy");
                }
                decimal? subtotal = salesInvoice.Subtotal;
                decimal? total = salesInvoice.NetTotal;
                decimal? transportationCost = salesInvoice.TransportationCost;
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
                float[] widthscellsTableSaleOrder = new float[] { 18f, 40f, 18f, 24f };
                tableSalesInvoice.SetWidths(widthscellsTableSaleOrder);
                tableSalesInvoice.WidthPercentage = 100;
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Customer Code: ", fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerCode, fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Invoice No: ", fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(invoiceNo, fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Name: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerName, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Invoice Date: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(invoiceDate, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Address: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerAddress, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Challan No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(deliveryNoteNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(customerContactNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Sale Order No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(saleOrderNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Transport: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(transport, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Sale Order Date:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(orderDate, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Depot:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(storeName, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSalesInvoice.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableSalesInvoice);

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
                    PdfPTable tableSalesInvoiceItems = new(11);
                    float[] widthsCellsSalesInvoiceItems = new float[] { 4f, 11f, 13f, 6f, 19f, 5f, 7f, 6f, 7f, 7f, 11f };
                    tableSalesInvoiceItems.SetWidths(widthsCellsSalesInvoiceItems);
                    tableSalesInvoiceItems.WidthPercentage = 100;
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8)) { Colspan = 6, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8)) { Colspan = 3, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Rate & Amount", fontArial8)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Delivery Date", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Delivery Place", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Bag Weight", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Bag / PCS", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Qty / KG", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("TP / KG", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("TK", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });

                    int sl = 0;
                    foreach (var salesInvoiceItem in salesInvoiceItems)
                    {
                        invoiceDiscount += salesInvoiceItem.InvoiceDiscountPerUnit * salesInvoiceItem.Quantity;
                        cashDiscount += salesInvoiceItem.CashDiscountPerUnit * salesInvoiceItem.Quantity;
                        specialDiscount += salesInvoiceItem.SpecialDiscountPerUnit * salesInvoiceItem.Quantity;
                        sl++;
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.DeliveryDate?.ToString("dd/MM/yyyy"), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.DeliveryPlace, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.BagWeight.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.PrimaryQuantity.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Quantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Rate.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    }
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 7, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItems.Sum(x => x.PrimaryQuantity).ToString(), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItems.Sum(x => x.Quantity).ToString("#,##0"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableSalesInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItems.Sum(x => x.Amount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    document.Add(tableSalesInvoiceItems);
                }
                // ==============
                // Calculation
                // ==============

                PdfPTable calculationTable = new(5);
                float[] widthsCellsCalculationTable = new float[] { 15f, 16f, 33f, 20f, 16f };
                calculationTable.WidthPercentage = 100;
                calculationTable.SetWidths(widthsCellsCalculationTable);
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("TP Value: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(subtotal?.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Invoice Discount: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(invoiceDiscount.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Cash Discount: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(cashDiscount.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Special Discount: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(specialDiscount.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Offer Discount: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(offerDiscount?.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Other Discount: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(otherDiscount?.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Transportation Cost: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(transportationCost?.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Depo Charge: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(depoCharge?.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("Balance Due: ", fontArial8)) { Border = 0, PaddingTop = 2f, PaddingLeft = 5, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase((balance + total)?.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 2 });
                calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase("Total Payable: ", fontArial8)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                calculationTable.AddCell(new PdfPCell(new Phrase(total?.ToString("#,##0.00"), fontArial8)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                calculationTable.AddCell(new PdfPCell(new Phrase("*Vat Note: This is vat exemted product. By: SRO No-176-AIN/2022/176-VAT", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 10f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
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
                tableUsers.AddCell(new PdfPCell(new Phrase("Disclaimer: This is a computer generated Sale Invoice. No signature is required. If you have any query, Please call this number: 01313-019140", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 14f, PaddingBottom = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();
        }
    }

    public class RoundedRectangleCell : IPdfPCellEvent
    {
        private readonly BaseColor _borderColor;
        private readonly float _radius;

        public RoundedRectangleCell(BaseColor borderColor, float radius)
        {
            _borderColor = borderColor;
            _radius = radius;
        }

        public void CellLayout(PdfPCell cell, Rectangle position, PdfContentByte[] canvases)
        {
            PdfContentByte canvas = canvases[PdfPTable.BACKGROUNDCANVAS];

            // Draw rounded rectangle
            canvas.RoundRectangle(
                position.Left,
                position.Bottom,
                position.Width,
                position.Height,
                _radius
            );

            // Set border color and stroke
            canvas.SetColorStroke(_borderColor);
            canvas.SetLineWidth(1f); // Adjust border width as needed
            canvas.Stroke();
        }
    }

    public class CustomDashedTopBottomCell : IPdfPCellEvent
    {
        private readonly BaseColor _borderColor;
        private readonly float _topDashLength;
        private readonly float _bottomDashLength;

        public CustomDashedTopBottomCell(BaseColor borderColor, float topDashLength, float bottomDashLength)
        {
            _borderColor = borderColor;
            _topDashLength = topDashLength;
            _bottomDashLength = bottomDashLength;
        }

        public void CellLayout(PdfPCell cell, Rectangle position, PdfContentByte[] canvases)
        {
            PdfContentByte canvas = canvases[PdfPTable.LINECANVAS];

            // Set line properties
            canvas.SetLineWidth(1f);
            canvas.SetColorStroke(_borderColor);

            // Clear any previous line dash settings
            canvas.SetLineDash(0);

            // Draw top border (dashed) if specified
            if (_topDashLength > 0)
            {
                canvas.SetLineDash(_topDashLength, _topDashLength); // Dash pattern
                canvas.MoveTo(position.Left, position.Top); // Move to top left
                canvas.LineTo(position.Right, position.Top); // Draw to top right
                canvas.Stroke();
            }

            // Reset dash settings
            canvas.SetLineDash(0);

            // Draw bottom border (dashed) if specified
            if (_bottomDashLength > 0)
            {
                canvas.SetLineDash(_bottomDashLength, _bottomDashLength); // Dash pattern
                canvas.MoveTo(position.Left, position.Bottom); // Move to bottom left
                canvas.LineTo(position.Right, position.Bottom); // Draw to bottom right
                canvas.Stroke();
            }

            // Reset dash settings for safety
            canvas.SetLineDash(0);
        }
    }
}
