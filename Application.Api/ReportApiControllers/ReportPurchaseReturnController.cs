using Application.Api.Attributes;
using Application.Core.Common;
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
    public class ReportPurchaseReturnController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportPurchaseReturnController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{purchaseReturnId}")]
        public async Task<IActionResult> PurchaseReturns(Guid purchaseReturnId)
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
            headerPage.AddCell(new PdfPCell(new Phrase("Purchase Return", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            var userName = _workContext.GetUserName() ?? "";
            var purchaseReturn = await _unitOfWork.Repository<PurchaseReturn>().TableNoTracking().Include(x => x.Supplier).Include(x => x.Store).Where(d => d.Id == purchaseReturnId).FirstOrDefaultAsync();

            if (purchaseReturn is not null)
            {
                String? purchaseReturnNo = purchaseReturn.PurchaseReturnNo;
                String? grnNo = purchaseReturn.Grnno;
                String? supplierCode = purchaseReturn.Supplier.Code;
                String? supplierName = purchaseReturn.Supplier.Name;
                String? supplierAddress = purchaseReturn.Supplier.Address;
                String? supplierContactNo = purchaseReturn.Supplier.ContactNo;
                String? storeName = purchaseReturn.Store.Name;
                String? purchaseReturnDate = purchaseReturn.PurchaseReturnDate.ToLocal().ToString("dd/MM/yyyy");
                String? remarks = purchaseReturn.Remark;
                String? preparedBy = purchaseReturn.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(purchaseReturn.CheckedBy))
                    checkedBy = purchaseReturn.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(purchaseReturn.ApprovedBy))
                    approvedBy = purchaseReturn.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tablePurchaseReturn = new(4);
                float[] widthscellsTablePurchaseReturn = new float[] { 15f, 40f, 20f, 25f };
                tablePurchaseReturn.SetWidths(widthscellsTablePurchaseReturn);
                tablePurchaseReturn.WidthPercentage = 100;
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("Supplier Code: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(supplierCode, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("Purchase Return No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(purchaseReturnNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("Name: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(supplierName, fontArial9Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("GRN No: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(grnNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("Address: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(supplierAddress, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("Purchase Return Date: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(purchaseReturnDate, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(supplierContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("Depot: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(storeName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tablePurchaseReturn.AddCell(new PdfPCell(new Phrase(remarks, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tablePurchaseReturn);
                document.Add(spaceTable);

                // ======================
                // Get Purchase Return Item
                // ======================
                var purchaseReturnItems = _unitOfWork.Repository<PurchaseReturnDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.PurchaseReturnId == purchaseReturnId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.Quantity,
                                           d.Rate,
                                           d.Amount
                                       });

                if (purchaseReturnItems.Any())
                {
                    PdfPTable tablePurchaseReturnItems = new(7);
                    float[] widthsCellsPurchaseItems = new float[] { 5f, 8f, 40f, 8f, 12f, 12f, 15f };
                    tablePurchaseReturnItems.SetWidths(widthsCellsPurchaseItems);
                    tablePurchaseReturnItems.WidthPercentage = 100;
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("Description", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("Rate", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });

                    int sl = 0;
                    foreach (var item in purchaseReturnItems)
                    {
                        sl++;
                        tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial9)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                        tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                        tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(item.Rate.ToString(), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                        tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("#,##0.00"), fontArial9)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    }
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial9Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(purchaseReturnItems.Sum(x => x.Quantity).ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase("", fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                    tablePurchaseReturnItems.AddCell(new PdfPCell(new Phrase(purchaseReturnItems.Sum(x => x.Amount).ToString("#,##0.00"), fontArial9Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });

                    document.Add(tablePurchaseReturnItems);

                    document.Add(spaceTable);

                    //NumberToWords Table
                    PdfPTable tableNumberToWords = new(2);
                    float[] widthsCellsTableNumberToWords = new float[] { 17f, 83f };
                    tableNumberToWords.WidthPercentage = 100;
                    tableNumberToWords.SetWidths(widthsCellsTableNumberToWords);
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial9)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(purchaseReturnItems.Sum(x => x.Amount)), fontArial9Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableNumberToWords);
                    document.Add(spaceTable);
                    document.Add(spaceTable);
                }
                //Line
                Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 75.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
                // ==============
                // User Signature
                // ==============
                PdfPTable tableUsers = new(4);
                float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
                tableUsers.WidthPercentage = 100;
                tableUsers.SetWidths(widthsCellsTableUsers);
                tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Authorized by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
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
