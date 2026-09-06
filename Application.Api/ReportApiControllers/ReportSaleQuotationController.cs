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
    public class ReportSaleQuotationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportSaleQuotationController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{saleQuotationId}")]
        public async Task<IActionResult> SaleQuotations(Guid saleQuotationId)
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
            headerPage.AddCell(new PdfPCell(new Phrase("Sale Quotation", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Sale Quotation
            // =============
            var saleQuotation = _unitOfWork.Repository<SaleQuotation>().TableNoTracking().Include(x => x.Customer).Where(d => d.Id == saleQuotationId).FirstOrDefault();

            if (saleQuotation is not null)
            {
                String? customerCode = saleQuotation.Customer.Code;
                String? customerName = saleQuotation.Customer.Name;
                String? address = saleQuotation.Customer.Address;
                String? contactNo = saleQuotation.Customer.ContactNo;
                String? email = saleQuotation.Customer.Email;
                String? quotationNo = saleQuotation.QuotationNo;
                String? quotationDate = saleQuotation.QuotationDate.ToString("dd-MMM-yyyy");
                String? referenceNo = saleQuotation.ReferenceNo;
                String? expiryDate = saleQuotation.ExpiryDate.ToString("dd-MMM-yyyy");
                String? termAndCondition = saleQuotation.TermAndCondition;
                decimal? subtotal = saleQuotation.Subtotal;
                decimal? discount = saleQuotation.Discount;
                decimal? total = saleQuotation.Total;
                bool? isMailSent = saleQuotation.IsMailSent;
                String? createdOn = saleQuotation.CreatedOn.ToString("dd-MMM-yyyy");
                String? remarks = saleQuotation.Remark;
                String? preparedBy = saleQuotation.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(saleQuotation.CheckedBy))
                    checkedBy = saleQuotation.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(saleQuotation.ApprovedBy))
                    approvedBy = saleQuotation.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableSaleQuotation = new(4);
                float[] widthscellsTableSaleQuotation = new float[] { 60f, 130f, 60f, 130f };
                tableSaleQuotation.SetWidths(widthscellsTableSaleQuotation);
                tableSaleQuotation.WidthPercentage = 100;
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Quotation No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(quotationNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Quotation Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(quotationDate, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Reference No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(referenceNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Expiray Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(expiryDate, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Quote To: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(customerName, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Address: " + address, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Contact No: " + contactNo, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Email: " + email, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleQuotation.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableSaleQuotation);

                // ======================
                // Get Production Item
                // ======================
                var saleQuotationItems = _unitOfWork.Repository<SaleQuotationDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.SaleQuotationId == saleQuotationId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           InventoryType = d.Product.InventoryType.Name,
                                           ProductType = d.Product.ProductType.Name,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.BagWeight,
                                           d.PrimaryQuantity,
                                           d.Quantity,
                                           d.Rate,
                                           d.DiscountPerUnit,
                                           d.DiscountAmount,
                                           d.Amount,
                                       });

                if (saleQuotationItems.Any())
                {
                    PdfPTable tableSaleQuotationItems = new(11);
                    float[] widthsCellsSaleQuotationItems = new float[] { 5f, 7f, 20f, 8f, 8f, 7f, 7f, 8f, 10f, 10f, 10f };
                    tableSaleQuotationItems.SetWidths(widthsCellsSaleQuotationItems);
                    tableSaleQuotationItems.WidthPercentage = 100;
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial9Bold)) { Colspan = 5, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Quantity/Rate", fontArial9Bold)) { Colspan = 6, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Bag Size", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Bag Qty", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Discount P/U", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Discount Amount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var item in saleQuotationItems)
                    {
                        sl++;
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.BagWeight.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.PrimaryQuantity.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("N3"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.DiscountPerUnit.ToString("N3"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.DiscountAmount.ToString("N3"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("N3"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { Colspan = 11, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Subtotal: ", fontArial8Bold)) { Colspan = 9, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(subtotal.ToString(), fontArial8Bold)) { Colspan = 2, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Discount: ", fontArial8Bold)) { Colspan = 9, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(discount.ToString(), fontArial8Bold)) { Colspan = 2, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 9, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableSaleQuotationItems.AddCell(new PdfPCell(new Phrase(total.ToString(), fontArial8Bold)) { Colspan = 2, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableSaleQuotationItems);

                    document.Add(spaceTable);
                }

                PdfPTable tableConditions = new(2);
                float[] widthsCellsTableConditions = new float[] { 10f, 90f };
                tableConditions.WidthPercentage = 60;
                tableConditions.HorizontalAlignment = Element.ALIGN_LEFT;
                tableConditions.SetWidths(widthsCellsTableConditions);
                tableConditions.AddCell(new PdfPCell(new Phrase("Term & Condition", fontArial8Bold)) { Colspan = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableConditions.AddCell(new PdfPCell(new Phrase("1", fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableConditions.AddCell(new PdfPCell(new Phrase(termAndCondition, fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                document.Add(tableConditions);
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
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
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
