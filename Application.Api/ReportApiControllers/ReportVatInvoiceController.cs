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
    public class ReportVatInvoiceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportVatInvoiceController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
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
            Rectangle rectangle = new Rectangle(PageSize.A4);
            Document document = new Document(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 30f, 30f);
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
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);

            // ===========
            // Header Page
            // ===========

            //var logo2 = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName("")));
            //// Set the size of the image if needed
            //logo2.ScaleToFit(44, 110);
            //logo2.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            //logo2.IndentationLeft = 9f;
            //logo2.IndentationRight = 9f;
            //logo2.SpacingBefore = 0f;
            //logo2.SpacingAfter = 0f;

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName("govtLogo.png")));
            // Set the size of the image if needed
            logo.ScaleToFit(44, 110);
            logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            logo.IndentationLeft = 9f;
            logo.IndentationRight = 9f;
            logo.SpacingBefore = 0f;
            logo.SpacingAfter = 0f;

            PdfPTable header1Page = new PdfPTable(3);
            float[] widthsCellsHeaderPage1 = new float[] { 14f, 74f, 14f };
            header1Page.SetWidths(widthsCellsHeaderPage1);
            header1Page.WidthPercentage = 100;
            PdfPCell logoCell = new PdfPCell(logo);
            logoCell.Border = Rectangle.NO_BORDER;
            logoCell.HorizontalAlignment = Element.ALIGN_CENTER;
            logoCell.Rowspan = 3;
            header1Page.AddCell(logoCell);
            header1Page.AddCell(new PdfPCell(new Phrase("Government of the people's Republic of Bangladesh", fontArial9)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("Mushak-6.3", fontArial12Bold)) { Rowspan = 3, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("National Board of Revenue", fontArial11Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("VAT Invoice", fontArial11Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            header1Page.AddCell(new PdfPCell(new Phrase("(See clause ( C ) and (f) Sub-Rule (1) of Rule 40)", fontArial9)) { Colspan = 3, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            document.Add(header1Page);

            // =====
            // Space
            // =====
            PdfPTable spaceTable = new(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial10Bold)) { Border = 0, PaddingTop = 5f });

            //document.Add(line);

            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // =============
            // Sales Invoice
            // =============
            var salesVatInvoice = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d =>
                                d.Id == saleVatInvoiceId).FirstOrDefaultAsync();

            if (salesVatInvoice is not null)
            {
                String? customerCode = salesVatInvoice.Customer.Code;
                String? customerName = salesVatInvoice.Customer.Name;
                String? customerAddress = salesVatInvoice.Customer.Address;
                String? customerContactNo = salesVatInvoice.Customer.ContactNo;
                String? invoiceNo = salesVatInvoice.InvoiceNo;
                String? invoiceDate = DateTimeHelper.UtcToLocal(salesVatInvoice.InvoiceDate).ToString("dd/MM/yyyy");
                var deliveryPlace = salesVatInvoice.SaleInvoiceDetails.FirstOrDefault()?.DeliveryPlace;

                PdfPTable tableVatInvoice = new(4);
                float[] widthscellsTableVatInvoice = new float[] { 25f, 30f, 15f, 30f };
                tableVatInvoice.SetWidths(widthscellsTableVatInvoice);
                tableVatInvoice.WidthPercentage = 100;
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("Name of Registered Person:", fontArial9)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(tenantData.Name, fontArial9Bold)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("Invoice No:", fontArial9)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(invoiceNo, fontArial9)) { Border = 0, PaddingTop = 6f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("Bin of Registered Person:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(tenantData.Binno, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("Date of Issue:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(invoiceDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("Address in the Invoice Issued:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(tenantData.Address, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("Name of Purchaser:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(customerName, fontArial9Bold)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(customerAddress, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(customerContactNo, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("Bin of Purchaser:", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("Destination of Supply:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase(deliveryPlace, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableVatInvoice.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 4, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableVatInvoice);

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
                                           NetAmount = d.Quantity * d.NetRate
                                       });

                if (salesVatInvoiceItems.Any())
                {
                    PdfPTable tableVatInvoiceItems = new(11);
                    float[] widthsCellsVatInvoiceItems = new float[] { 4f, 5f, 23f, 7f, 8f, 7f, 11f, 6f, 10f, 8f, 11f };
                    tableVatInvoiceItems.SetWidths(widthsCellsVatInvoiceItems);
                    tableVatInvoiceItems.WidthPercentage = 100;
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Details of Supply", fontArial8Bold)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit of Supply", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Unit Price (Taka)", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Total Price (Taka)", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Amt of SD", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Rate of VAT / Exact VAT (Taka)", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Amount of VAT (Taka)", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Total Value Including All Duty & Taxes", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });

                    int sl = 0;
                    foreach (var salesInvoiceItem in salesVatInvoiceItems)
                    {
                        sl++;
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.Quantity.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.NetRate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.NetAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesInvoiceItem?.NetAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    }
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesVatInvoiceItems.Sum(x => x.Quantity).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesVatInvoiceItems.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase("0.00", fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableVatInvoiceItems.AddCell(new PdfPCell(new Phrase(salesVatInvoiceItems.Sum(x => x.NetAmount).ToString("#,##0.00"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 4f, PaddingLeft = 2f, PaddingRight = 2f });
                    document.Add(tableVatInvoiceItems);
                }
            }
            PdfPTable footerTable = new(2);
            float[] widthCellsFooterTable = new float[] { 55f, 45f };
            footerTable.SetWidths(widthCellsFooterTable);
            footerTable.WidthPercentage = 100;
            footerTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            footerTable.AddCell(new PdfPCell(new Phrase("Name of the Authorized Responsible Person of the Company ::", fontArial9)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            footerTable.AddCell(new PdfPCell(new Phrase("Designation", fontArial9)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            //PdfPCell logoSeal = new PdfPCell(logo2);
            //logoSeal.Border = Rectangle.NO_BORDER;
            //logoSeal.HorizontalAlignment = Element.ALIGN_CENTER;
            //logoSeal.Rowspan = 3;
            //footerTable.AddCell(logoSeal);
            footerTable.AddCell(new PdfPCell(new Phrase("Signature", fontArial9)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            footerTable.AddCell(new PdfPCell(new Phrase("Seal", fontArial9)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            footerTable.AddCell(new PdfPCell(new Phrase("1. Price Including All Kind of VAT", fontArial9Bold)) { Colspan = 2, Border = 0, PaddingTop = 5f });
            document.Add(footerTable);

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}
