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
    public class ReportCustomerWiseProductDiscountController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportCustomerWiseProductDiscountController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{customerWiseProductDiscountId}")]
        public async Task<IActionResult> CustomerWiseProductDiscounts(Guid customerWiseProductDiscountId)
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
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
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
            headerPage.AddCell(new PdfPCell(new Phrase("Customer Wise Product Discount", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Customer Wise Product Discount
            // =============
            var customerWiseProductDiscount = _unitOfWork.Repository<CustomerWiseProductDiscount>().TableNoTracking().Include(x => x.Customer).Where(d => d.Id == customerWiseProductDiscountId).FirstOrDefault();

            if (customerWiseProductDiscount is not null)
            {
                String? customerCode = customerWiseProductDiscount.Customer.Code;
                String? customerName = customerWiseProductDiscount.Customer.Name;
                String? address = customerWiseProductDiscount.Customer.Address;
                String? contactNo = customerWiseProductDiscount.Customer.ContactNo;
                String? email = customerWiseProductDiscount.Customer.Email;
                String? applicableDate = customerWiseProductDiscount.ApplicableDate.ToString("dd-MMM-yyyy");
                String? preparedBy = customerWiseProductDiscount.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(customerWiseProductDiscount.CheckedBy))
                    checkedBy = customerWiseProductDiscount.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(customerWiseProductDiscount.ApprovedBy))
                    approvedBy = customerWiseProductDiscount.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableCustomerWiseProductDiscount = new(4);
                float[] widthsCellsTableCustomerWiseProductDiscount = new float[] { 60f, 130f, 60f, 130f };
                tableCustomerWiseProductDiscount.SetWidths(widthsCellsTableCustomerWiseProductDiscount);
                tableCustomerWiseProductDiscount.WidthPercentage = 100;
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase("Customer Code: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase(customerCode, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase("Applicable Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase(applicableDate, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase("Customer Name: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase(customerName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase("Email: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase(email, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase("Address: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase(address, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase("Contct No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableCustomerWiseProductDiscount.AddCell(new PdfPCell(new Phrase(contactNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableCustomerWiseProductDiscount);
                document.Add(spaceTable);

                // ======================
                // Get Customer Wise Product Discount Item
                // ======================
                //cwpd = customer wise product discount
                var cwpdItems = _unitOfWork.Repository<CustomerWiseProductDiscountDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.CustomerWiseProductDiscountId == customerWiseProductDiscountId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.SalePrice,
                                           d.InvoiceDiscount,
                                           d.MonthlyDiscount,
                                           d.YearlyDiscount,
                                           d.CashDiscount,
                                           d.TargetDiscount,
                                           d.SpecialDiscount,
                                           TotalDiscount = d.InvoiceDiscount + d.CashDiscount + d.SpecialDiscount + d.MonthlyDiscount + d.YearlyDiscount + d.TargetDiscount
                                       }).OrderBy(x => x.ProductCode);

                if (cwpdItems.Any())
                {
                    PdfPTable tableCWPDItems = new(12);
                    float[] widthsCellsCWPDItems = new float[] { 4f, 7f, 24f, 5f, 7f, 9f, 9f, 9f, 9f, 9f, 9f, 9f };
                    tableCWPDItems.SetWidths(widthsCellsCWPDItems);
                    tableCWPDItems.WidthPercentage = 100;
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Invoice Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Cash Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Special Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Monthly Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Yearly Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Target Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableCWPDItems.AddCell(new PdfPCell(new Phrase("Total Discount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var item in cwpdItems)
                    {
                        sl++;
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial7)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial7)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial7)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.SalePrice.ToString(), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.InvoiceDiscount.ToString(), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.CashDiscount.ToString(), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.SpecialDiscount.ToString(), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.MonthlyDiscount.ToString(), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.YearlyDiscount.ToString(), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.TargetDiscount.ToString(), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableCWPDItems.AddCell(new PdfPCell(new Phrase(item.TotalDiscount.ToString(), fontArial7)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    }
                    document.Add(tableCWPDItems);
                    document.Add(spaceTable);
                    document.Add(spaceTable);
                }

                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
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
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });

                tableUsers.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 1f, HorizontalAlignment = 1 });
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
