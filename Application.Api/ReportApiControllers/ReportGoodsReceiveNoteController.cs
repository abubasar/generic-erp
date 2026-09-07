using Application.Api.Attributes;
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
    public class ReportGoodsReceiveNoteController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public ReportGoodsReceiveNoteController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [HttpGet("{goodsReceiveNoteId}")]
        public async Task<IActionResult> GoodsReceiveNotes(Guid goodsReceiveNoteId)
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
            headerPage.AddCell(new PdfPCell(new Phrase("Goods Receive Note", fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            var goodsReceiveNote = _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking().Include(x => x.Supplier).Include(x => x.Store).Where(d => d.Id == goodsReceiveNoteId).FirstOrDefault();

            if (goodsReceiveNote is not null)
            {
                String? supplierCode = goodsReceiveNote.Supplier.Code;
                String? supplierName = goodsReceiveNote.Supplier.Name;
                String? address = goodsReceiveNote.Supplier.Address;
                String? contactNo = goodsReceiveNote.Supplier.ContactNo;
                String? email = goodsReceiveNote.Supplier.Email;
                String? storeName = goodsReceiveNote.Store.Name;
                String? poNumber = goodsReceiveNote.Ponumber;
                String? grnNo = goodsReceiveNote.Grnno;
                String? challanNo = goodsReceiveNote.ChallanNo;
                String? grnDate = goodsReceiveNote.Grndate.ToString("dd-MMM-yyyy");
                String? challanDate = goodsReceiveNote.ChallanDate?.ToString("dd-MMM-yyyy");
                String? truckNo = goodsReceiveNote.TruckNo;
                String? driverName = goodsReceiveNote.DriverName;
                String? driverContactNo = goodsReceiveNote.DriverContactNo;
                decimal? subtotal = goodsReceiveNote.Subtotal;
                decimal? discount = goodsReceiveNote.Discount;
                decimal? transportationCost = goodsReceiveNote.TransportationCost;
                decimal? total = goodsReceiveNote.Total;
                String? createdOn = goodsReceiveNote.CreatedOn.ToString("dd-MMM-yyyy");
                String? remarks = goodsReceiveNote.Remark;
                String? preparedBy = goodsReceiveNote.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(goodsReceiveNote.CheckedBy))
                    checkedBy = goodsReceiveNote.CheckedBy;
                else
                    checkedBy = "";
                String? verifiedBy;
                if (!string.IsNullOrEmpty(goodsReceiveNote.ApprovedBy))
                    verifiedBy = goodsReceiveNote.ApprovedBy;
                else
                    verifiedBy = "";

                PdfPTable tableGoodsReceiveNote = new(4);
                float[] widthscellsTableGoodsReceiveNote = new float[] { 60f, 130f, 60f, 130f };
                tableGoodsReceiveNote.SetWidths(widthscellsTableGoodsReceiveNote);
                tableGoodsReceiveNote.WidthPercentage = 100;
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Supplier: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(supplierName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("GRN No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(grnNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Address: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(address, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("GRN Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(grnDate, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(contactNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Challan No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(challanNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("PO No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(poNumber, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Challan Date: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(challanDate, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Store: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(storeName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Truck No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(truckNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Driver Name: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(driverName, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Driver Contact No: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(driverContactNo, fontArial8)) { Border = 0, PaddingTop = 5f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial8Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableGoodsReceiveNote.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 5f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableGoodsReceiveNote);

                // ======================
                // Get Goods Receive Note Item
                // ======================
                var grnItems = _unitOfWork.Repository<GoodsReceiveNoteDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.GoodsReceiveNoteId == goodsReceiveNoteId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.Poquantity,
                                           d.Grnquantity,
                                           d.Rate,
                                           d.Amount,
                                       });

                if (grnItems.Any())
                {
                    PdfPTable tableGRNItems = new(6);
                    float[] widthsCellsDeliveryNoteItems = new float[] { 7f, 10f, 40f, 14f, 14f, 15f };
                    tableGRNItems.SetWidths(widthsCellsDeliveryNoteItems);
                    tableGRNItems.WidthPercentage = 100;
                    tableGRNItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableGRNItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableGRNItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableGRNItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableGRNItems.AddCell(new PdfPCell(new Phrase("PO Qty", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableGRNItems.AddCell(new PdfPCell(new Phrase("GRN Qty", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    //tableGRNItems.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    //tableGRNItems.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var item in grnItems)
                    {
                        sl++;
                        tableGRNItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableGRNItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableGRNItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableGRNItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableGRNItems.AddCell(new PdfPCell(new Phrase(item.Poquantity.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        tableGRNItems.AddCell(new PdfPCell(new Phrase(item.Grnquantity.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        //tableGRNItems.AddCell(new PdfPCell(new Phrase(item.Rate.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                        //tableGRNItems.AddCell(new PdfPCell(new Phrase(item.Amount.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    }
                    tableGRNItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableGRNItems.AddCell(new PdfPCell(new Phrase(grnItems.Sum(x => x.Grnquantity).ToString(), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    //tableGRNItems.AddCell(new PdfPCell(new Phrase("Subtotal: ", fontArial8Bold)) { Colspan = 7, Border = 0, HorizontalAlignment = 2, PaddingTop = 8f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    //tableGRNItems.AddCell(new PdfPCell(new Phrase(subtotal.ToString(), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 8f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    //tableGRNItems.AddCell(new PdfPCell(new Phrase("Transportation Cost: ", fontArial8Bold)) { Colspan = 7, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    //tableGRNItems.AddCell(new PdfPCell(new Phrase(transportationCost.ToString(), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    //tableGRNItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 7, Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    //tableGRNItems.AddCell(new PdfPCell(new Phrase(total.ToString(), fontArial8Bold)) { Border = 0, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    document.Add(tableGRNItems);

                    document.Add(spaceTable);

                    //NumberToWords Table
                    //PdfPTable tableNumberToWords = new(2);
                    //float[] widthsCellsTableNumberToWords = new float[] { 20f, 80f };
                    //tableNumberToWords.WidthPercentage = 100;
                    //tableNumberToWords.SetWidths(widthsCellsTableNumberToWords);
                    //tableNumberToWords.AddCell(new PdfPCell(new Phrase("Amount In Words: ", fontArial8Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    //tableNumberToWords.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(total ?? 0), fontArial8)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    //document.Add(tableNumberToWords);
                    //document.Add(spaceTable);

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
