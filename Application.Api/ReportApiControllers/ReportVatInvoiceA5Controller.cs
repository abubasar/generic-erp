using Application.Api.Attributes;
using Application.Core.Entities;
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
    public class ReportVatInvoiceA5Controller : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportVatInvoiceA5Controller(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{saleVatInvoiceId}")]
        public async Task<IActionResult> Sales(Guid saleVatInvoiceId)
        {
            // ==============================
            // PDF Settings and Customization
            // ==============================
            MemoryStream workStream = new MemoryStream();
            Rectangle rectangle = new Rectangle(PageSize.A5);
            Document document = new Document(rectangle, 72, 72, 72, 72);
            document.SetMargins(14f, 14f, 20f, 20f);
            PdfWriter.GetInstance(document, workStream).CloseStream = false;

            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial11 = FontFactory.GetFont("Arial", 11);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial6 = FontFactory.GetFont("Arial", 6);
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);

            // ===========
            // Header Page
            // ===========

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName("govtLogo.png")));
            // Set the size of the image if needed
            logo.ScaleToFit(44, 110);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 0f;
            logo.SpacingAfter = 0f;

            PdfPTable header1Page = new PdfPTable(3);
            float[] widthsCellsHeaderPage1 = new float[] { 20f, 60f, 20f };
            header1Page.SetWidths(widthsCellsHeaderPage1);
            header1Page.WidthPercentage = 100;
            PdfPCell logoCell = new PdfPCell(logo);
            logoCell.Border = Rectangle.NO_BORDER;
            logoCell.HorizontalAlignment = Element.ALIGN_CENTER;
            logoCell.Rowspan = 3;
            header1Page.AddCell(logoCell);
            header1Page.AddCell(new PdfPCell(new Phrase("Government of the people's Republic of Bangladesh", fontArial9)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("First Copy", fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("National Board of Revenue", fontArial11Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("Mushak-6.3", fontArial11Bold)) { HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            header1Page.AddCell(new PdfPCell(new Phrase("VAT Invoice", fontArial11Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("", fontArial11Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("[See Clauses (C) and (f) of Sub-Rule (1) of Rule 40]", fontArial9Bold)) { Colspan = 3, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            document.Add(header1Page);

            // Space
            PdfPTable spaceTable = new(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 5f });

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Sales Invoice
            var salesVatInvoice = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d =>
                                d.Id == saleVatInvoiceId).FirstOrDefaultAsync();

            if (salesVatInvoice is not null)
            {
                String? customerCode = salesVatInvoice.Customer.Code;
                String? customerName = salesVatInvoice.Customer.Name;
                String? customerAddress = salesVatInvoice.Customer.Address;
                String? customerContactNo = salesVatInvoice.Customer.ContactNo;
                String? invoiceNo = salesVatInvoice.InvoiceNo;
                String? invoiceDate = salesVatInvoice.InvoiceDate.ToLocal().ToString("dd/MM/yyyy");
                String? createdOn = salesVatInvoice.CreatedOn.ToLocal().ToString("hh:mm:ss tt");
                //var deliveryPlace = salesVatInvoice.SaleInvoiceDetails.FirstOrDefault()?.DeliveryPlace;

                // ======================
                // Get Sales Invoice Item
                // ======================
                var salesVatInvoiceItems = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.SaleInvoiceId == saleVatInvoiceId).Select(d => new
                                       {
                                           Id = d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           Quantity = d.Quantity,
                                           NetRate = d.NetRate,
                                           NetAmount = d.Quantity * d.NetRate,
                                           d.DeliveryPlace
                                       });

                // Extract unique DeliveryPlace values
                var uniqueDeliveryPlaces = salesVatInvoiceItems
                    .Select(item => item.DeliveryPlace)
                    .Distinct();

                // Join the unique values into a comma-separated string
                string deliveryPlace = string.Join(", ", uniqueDeliveryPlaces);

                PdfPTable proprietorInfo = new(2);
                float[] widthsCellsProprietorInfo = new float[] { 50f, 50f };
                proprietorInfo.SetWidths(widthsCellsProprietorInfo);
                proprietorInfo.WidthPercentage = 100;
                proprietorInfo.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 2, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                proprietorInfo.AddCell(new PdfPCell(new Phrase("Name of Registered Person:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                proprietorInfo.AddCell(new PdfPCell(new Phrase(tenantData.Name, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                proprietorInfo.AddCell(new PdfPCell(new Phrase("Bin of Registered Person:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                proprietorInfo.AddCell(new PdfPCell(new Phrase(tenantData.Binno, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                proprietorInfo.AddCell(new PdfPCell(new Phrase("Address in the Invoice Issued:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });
                proprietorInfo.AddCell(new PdfPCell(new Phrase(tenantData.Address, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                document.Add(proprietorInfo);

                PdfPTable supplierSalesInfo = new(4);
                float[] widthsCellsSupplierSalesInfo = new float[] { 22f, 45f, 15f, 18f };
                supplierSalesInfo.SetWidths(widthsCellsSupplierSalesInfo);
                supplierSalesInfo.WidthPercentage = 100;
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("Name of Purchaser:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 2 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase(customerName, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("Invoice No:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 1f, PaddingRight = 2f, HorizontalAlignment = 2 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase(invoiceNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("Bin of Purchaser (Applicable Where):", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 2 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("Date of Issue:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 1f, PaddingRight = 2f, HorizontalAlignment = 2 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase(invoiceDate, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("Address of Purchaser:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 2 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase(customerAddress, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("Time of Issue:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 1f, PaddingRight = 2f, HorizontalAlignment = 2 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase(createdOn, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                if (!string.IsNullOrEmpty(customerContactNo))
                {
                    supplierSalesInfo.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                    supplierSalesInfo.AddCell(new PdfPCell(new Phrase(customerContactNo, fontArial8)) { Colspan = 3, Border = 0, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                }
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("Destination of Supply:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 2 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase(deliveryPlace, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("Nature of Vehicle and Number:", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 2 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                supplierSalesInfo.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 2f, PaddingRight = 2f, HorizontalAlignment = 0 });
                document.Add(supplierSalesInfo);

                if (salesVatInvoiceItems.Any())
                {
                    Paragraph paragraph = new Paragraph();

                    //paragraph.Add("Total Price (Taka)");
                    paragraph.Add(new Chunk("Unit Price", fontArial7Bold));

                    // Create a Chunk for the superscript "1"
                    Chunk superscriptChunk = new Chunk("1", fontArial6).SetTextRise(2);
                    paragraph.Add(superscriptChunk);

                    paragraph.Add(new Chunk(" (Taka)", fontArial7Bold));

                    PdfPTable tableVatInvoiceItems = new(12);
                    float[] widthsCellsVatInvoiceItems = new float[] { 4f, 6f, 12f, 7f, 9f, 7f, 11f, 8f, 8f, 8f, 9f, 11f };
                    tableVatInvoiceItems.SetWidths(widthsCellsVatInvoiceItems);
                    tableVatInvoiceItems.WidthPercentage = 100;
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Goods/Service Description (Applicable Where with Brand Name)", fontArial7Bold)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit of Supply", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Paragraph(paragraph)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Total Price (Taka)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Rate of Supplementary Duty", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Amount of Supplementary Duty (Taka)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Value Added Tax Rate/ Specific Tax (Taka)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Value Added Tax Rate/ Specific Tax Amount", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Total Value Including all Duty & Tax", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(1)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(2)", fontArial7Bold)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(3)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(4)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(5)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(6)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(7)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(8)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(9)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(10)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("(11)", fontArial7Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });

                    int sl = 0;
                    foreach (var salesInvoiceItem in salesVatInvoiceItems)
                    {
                        sl++;
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductCode, fontArial7)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductName, fontArial7)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.UnitName, fontArial7)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Quantity.ToString("#,##0.00"), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.NetRate.ToString("#,##0.00"), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.NetAmount.ToString("#,##0.00"), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.NetAmount.ToString("#,##0.00"), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    }
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Total : ", fontArial7Bold)) { Colspan = 6, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesVatInvoiceItems.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial7Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial7Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial7Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial7Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial7Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesVatInvoiceItems.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial7Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    document.Add(tableVatInvoiceItems);
                }
            }
            PdfPTable footerTable = new(2);
            float[] widthCellsFooterTable = new float[] { 55f, 45f };
            footerTable.SetWidths(widthCellsFooterTable);
            footerTable.WidthPercentage = 100;
            footerTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            footerTable.AddCell(new PdfPCell(new Phrase("Name of the Authorized Responsible Person of the Company :", fontArial8)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            footerTable.AddCell(new PdfPCell(new Phrase("Designation :", fontArial8)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            //PdfPCell logoSeal = new PdfPCell(logo2);
            //logoSeal.Border = Rectangle.NO_BORDER;
            //logoSeal.HorizontalAlignment = Element.ALIGN_CENTER;
            //logoSeal.Rowspan = 3;
            //footerTable.AddCell(logoSeal);
            footerTable.AddCell(new PdfPCell(new Phrase("Signature :", fontArial8)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            document.Add(footerTable);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 31.0F, BaseColor.Black, Element.ALIGN_LEFT, 4.5F)));
            Paragraph footer = new Paragraph();
            // Create a Chunk for the superscript "1"
            Chunk superscriptChunkForFooter = new Chunk("1 ", fontArial6).SetTextRise(3);
            // Add the superscript Chunk to the paragraph
            footer.Add(superscriptChunkForFooter);
            //paragraph.Add("Total Price (Taka)");
            footer.Add(new Chunk("Price Excluding All Kind of VAT\";", fontArial7Bold));

            PdfPTable footer2Table = new(2);
            float[] widthCellsFooter2Table = new float[] { 70f, 30f };
            footer2Table.SetWidths(widthCellsFooter2Table);
            footer2Table.WidthPercentage = 100;

            footer2Table.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 2, Border = 0, PaddingTop = 50f });
            footer2Table.AddCell(new PdfPCell(new Phrase(line)) { Colspan = 2, Border = 0, HorizontalAlignment = 0, Padding = 0 });
            footer2Table.AddCell(new PdfPCell(new Paragraph(footer)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 0f });
            footer2Table.AddCell(new PdfPCell(new Phrase("Seal", fontArial8)) { Colspan = 2, Border = 0, HorizontalAlignment = 0, PaddingTop = 0f });
            document.Add(footer2Table);

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}
