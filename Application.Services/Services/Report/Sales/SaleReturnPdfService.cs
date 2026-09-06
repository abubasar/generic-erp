using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Report.Sales
{
    public class SaleReturnPdfService : ISaleReturnPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;

        public SaleReturnPdfService(IUnitOfWork unitOfWork, IWorkContext workContext)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
        }

        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }

        public async Task PrintSaleReturnPrimaryReportToPdfAsync(MemoryStream stream, Guid saleReturnId, TenantViewModel tenantData, string userName, string headerText)
        {

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            // ==============================
            // PDF Settings and Customization
            // ==============================
            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;

            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);



            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));


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
            headerPage.AddCell(new PdfPCell(new Phrase("Sale Return", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Stock Transfer
            // =============
            var saleReturn = await _unitOfWork.Repository<SaleReturn>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d => d.Id == saleReturnId).FirstOrDefaultAsync();

            if (saleReturn is not null)
            {
                String? saleReturnNo = saleReturn.SaleReturnNo;
                String? deliveryNoteNo = saleReturn.DeliveryNoteNo;
                String? customerCode = saleReturn.Customer.Code;
                String? customerName = saleReturn.Customer.Name;
                String? customerAddress = saleReturn.Customer.Address;
                String? customerContactNo = saleReturn.Customer.ContactNo;
                String? storeName = saleReturn.Store.Name;
                String? saleReturnDate = saleReturn.SaleReturnDate.ToLocal().ToString("dd/MM/yyyy");
                String? remarks = saleReturn.Remark;
                decimal subtotal = saleReturn.Subtotal;
                decimal totalDiscountAmount = saleReturn.TotalPercentageDiscountAmount;
                decimal otherDiscount = saleReturn.OtherDiscount;
                decimal total = saleReturn.Total;
                String? preparedBy = saleReturn.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(saleReturn.CheckedBy))
                    checkedBy = saleReturn.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(saleReturn.ApprovedBy))
                    approvedBy = saleReturn.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableSaleReturn = new(4);
                float[] widthscellsTableSaleReturn = new float[] { 15f, 40f, 16f, 29f };
                tableSaleReturn.SetWidths(widthscellsTableSaleReturn);
                tableSaleReturn.WidthPercentage = 100;
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Customer Code: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(customerCode, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Sale Return No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(saleReturnNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Name: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(customerName, fontArial9Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                //tableSaleReturn.AddCell(new PdfPCell(new Phrase("Delivery Note No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                //tableSaleReturn.AddCell(new PdfPCell(new Phrase(deliveryNoteNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Address: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(customerAddress, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Date: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(saleReturnDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(customerContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Depot: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(storeName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(remarks, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableSaleReturn);
                document.Add(spaceTable);

                // ======================
                // Get Stock Transfer Item
                // ======================
                var saleReturnItems = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.SaleReturnId == saleReturnId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           Unit = d.Product.MeasurementUnit.Name,
                                           PackSize = d.Product.PackSize.Name,
                                           d.ReturnPrimaryQuantity,
                                           d.ReturnPrimaryBonusQuantity,
                                           d.Rate,
                                           d.DiscountPercentage,
                                           d.PercentageDiscountAmount,
                                           d.Amount
                                       });

                if (saleReturnItems.Any())
                {
                    PdfPTable tableSaleReturnItems = new(11);
                    float[] widthsCellsStockTransferItems = new float[] { 5f, 6f, 20f, 8f, 8f, 8f, 8f, 8f, 10f, 10f, 10f };
                    tableSaleReturnItems.SetWidths(widthsCellsStockTransferItems);
                    tableSaleReturnItems.WidthPercentage = 100;
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Sale Return", fontArial8)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Rate & Amount", fontArial8)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Return Quantity", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Bouns Quantity", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Rate", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Distount(%)", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Discount Amount", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var item in saleReturnItems)
                    {
                        sl++;
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.PackSize, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.Unit, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.ReturnPrimaryQuantity.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.ReturnPrimaryBonusQuantity.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.DiscountPercentage.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.PercentageDiscountAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    }
                    // Calculation
                    PdfPTable calculationTable = new(5);
                    float[] widthsCellsCalculationTable = new float[] { 15f, 16f, 33f, 20f, 16f };
                    calculationTable.WidthPercentage = 100;
                    calculationTable.SetWidths(widthsCellsCalculationTable);
                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("Subtotal: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase(subtotal.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("Discount Amount: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase(totalDiscountAmount.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("Other Discount: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase(otherDiscount.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase(total.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                    document.Add(tableSaleReturnItems);
                    //document.Add(spaceTable);

                    document.Add(calculationTable);
                    document.Add(spaceTable);

                    //NumberToWords Table
                    PdfPTable tableNumberToWords = new(2);
                    float[] widthsCellsTableNumberToWords = new float[] { 17f, 83f };
                    tableNumberToWords.WidthPercentage = 100;
                    tableNumberToWords.SetWidths(widthsCellsTableNumberToWords);
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial9)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(total), fontArial9Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableNumberToWords);
                    document.Add(spaceTable);
                    document.Add(spaceTable);
                }
                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 75.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new(5);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Customer", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Authorized by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();
        }

        public async Task PrintSaleReturnSecondaryReportToPdfAsync(MemoryStream stream, Guid saleReturnId, TenantViewModel tenantData, string userName, string headerText)
        {

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            // ==============================
            // PDF Settings and Customization
            // ==============================
            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;

            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);



            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));


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
            headerPage.AddCell(new PdfPCell(new Phrase("Sale Return", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Stock Transfer
            // =============
            var saleReturn = await _unitOfWork.Repository<SaleReturn>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d => d.Id == saleReturnId).FirstOrDefaultAsync();

            if (saleReturn is not null)
            {
                String? saleReturnNo = saleReturn.SaleReturnNo;
                String? deliveryNoteNo = saleReturn.DeliveryNoteNo;
                String? customerCode = saleReturn.Customer.Code;
                String? customerName = saleReturn.Customer.Name;
                String? customerAddress = saleReturn.Customer.Address;
                String? customerContactNo = saleReturn.Customer.ContactNo;
                String? storeName = saleReturn.Store.Name;
                String? saleReturnDate = saleReturn.SaleReturnDate.ToLocal().ToString("dd/MM/yyyy");
                String? remarks = saleReturn.Remark;
                decimal subtotal = saleReturn.Subtotal;
                decimal otherDiscount = saleReturn.OtherDiscount;
                decimal total = saleReturn.Total;
                String? preparedBy = saleReturn.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(saleReturn.CheckedBy))
                    checkedBy = saleReturn.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(saleReturn.ApprovedBy))
                    approvedBy = saleReturn.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableSaleReturn = new(4);
                float[] widthscellsTableSaleReturn = new float[] { 15f, 40f, 16f, 29f };
                tableSaleReturn.SetWidths(widthscellsTableSaleReturn);
                tableSaleReturn.WidthPercentage = 100;
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Customer Code: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(customerCode, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Sale Return No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(saleReturnNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Name: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(customerName, fontArial9Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Delivery Note No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(deliveryNoteNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Address: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(customerAddress, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Date: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(saleReturnDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(customerContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Depot: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(storeName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableSaleReturn.AddCell(new PdfPCell(new Phrase(remarks, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableSaleReturn);
                document.Add(spaceTable);

                // ======================
                // Get Stock Transfer Item
                // ======================
                var saleReturnItems = _unitOfWork.Repository<SaleReturnDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.SaleReturnId == saleReturnId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.ReturnPrimaryQuantity,
                                           d.ReturnQuantity,
                                           d.Rate,
                                           d.Amount
                                       });

                if (saleReturnItems.Any())
                {
                    PdfPTable tableSaleReturnItems = new(8);
                    float[] widthsCellsStockTransferItems = new float[] { 5f, 6f, 30f, 6f, 10f, 10f, 8f, 15f };
                    tableSaleReturnItems.SetWidths(widthsCellsStockTransferItems);
                    tableSaleReturnItems.WidthPercentage = 100;
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial9)) { Colspan = 3, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Sale Return", fontArial9)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Rate & Amount", fontArial9)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Name", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Bag", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Rate", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableSaleReturnItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var item in saleReturnItems)
                    {
                        sl++;
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.ReturnPrimaryQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.ReturnQuantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.Rate.ToString(), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tableSaleReturnItems.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    }

                    // Calculation
                    PdfPTable calculationTable = new(5);
                    float[] widthsCellsCalculationTable = new float[] { 15f, 16f, 33f, 20f, 16f };
                    calculationTable.WidthPercentage = 100;
                    calculationTable.SetWidths(widthsCellsCalculationTable);
                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("Subtotal: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase(subtotal.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("Other Discount: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase(otherDiscount.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });

                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
                    calculationTable.AddCell(new PdfPCell(new Phrase(total.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 2f, PaddingRight = 5f, HorizontalAlignment = 2 });
                    calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Colspan = 5, Border = 0, PaddingTop = 5f, PaddingRight = 5f, HorizontalAlignment = 2 });

                    document.Add(tableSaleReturnItems);
                    //document.Add(spaceTable);

                    document.Add(calculationTable);
                    document.Add(spaceTable);

                    //NumberToWords Table
                    PdfPTable tableNumberToWords = new(2);
                    float[] widthsCellsTableNumberToWords = new float[] { 17f, 83f };
                    tableNumberToWords.WidthPercentage = 100;
                    tableNumberToWords.SetWidths(widthsCellsTableNumberToWords);
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial9)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(saleReturnItems.Sum(x => x.Amount)), fontArial9Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableNumberToWords);
                    document.Add(spaceTable);
                    document.Add(spaceTable);
                }
                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 75.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new(5);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Customer", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Authorized by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();
        }
    }
}
