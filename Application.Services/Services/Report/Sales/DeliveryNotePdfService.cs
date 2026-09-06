using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.ViewModels.Configuration;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.EntityFrameworkCore;
using Application.Core.Extensions;

namespace Application.Services.Services.Report.Sales
{
    public class DeliveryNotePdfService : IDeliveryNotePdfService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeliveryNotePdfService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task PrintDeliveryNotePrimaryReportToPdfAsync(MemoryStream stream, Guid deliveryNoteId, TenantViewModel tenantData, string userName, string headerText)
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
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 10.0F)));

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
            headerPage.AddCell(new PdfPCell(new Phrase(headerText, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Delivery Note
            // =============
            var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d => d.Id == deliveryNoteId).FirstOrDefaultAsync();

            if (deliveryNote is not null)
            {
                String? customerCode = deliveryNote.Customer.Code;
                String? customerName = deliveryNote.Customer.Name;
                String? address = deliveryNote.Customer.Address;
                String? contactNo = deliveryNote.Customer.ContactNo;
                String? email = deliveryNote.Customer.Email;
                String? storeName = deliveryNote.Store.Name;
                String? saleOrderNo = deliveryNote.SaleOrderNo;
                String? deliveryNoteNo = deliveryNote.DeliveryNoteNo;
                String? deliveryDate = deliveryNote.DeliveryDate.ToLocal().ToString("dd/MM/yyyy");
                String? deliveryPlace = deliveryNote.DeliveryPlace;
                String? transport = Enum.GetName(typeof(Transport), deliveryNote.Transport)?.Replace("_", " ");
                String? referenceNo = deliveryNote.ReferenceNo;
                String? truckNo = deliveryNote.TruckNo;
                String? driverName = deliveryNote.DriverName;
                String? driverContactNo = deliveryNote.DriverContactNo;
                String? createdOn = deliveryNote.CreatedOn.ToLocal().ToString("dd/MM/yyyy");
                String? remarks = deliveryNote.Remark;
                String? preparedBy = deliveryNote.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(deliveryNote.CheckedBy))
                    checkedBy = deliveryNote.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(deliveryNote.ApprovedBy))
                    approvedBy = deliveryNote.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableDeliveryNote = new(4);
                float[] widthscellsTableDeliveryNote = new float[] { 19f, 39f, 19f, 23f };
                tableDeliveryNote.SetWidths(widthscellsTableDeliveryNote);
                tableDeliveryNote.WidthPercentage = 100;
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Delivery Note No: ", fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNoteNo, fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Sale Order No: ", fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(saleOrderNo, fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Name: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(customerName, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Reference No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(referenceNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Address: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(address, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Delivery Date: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryDate, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(contactNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Store: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(storeName, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Email: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(email, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Truck No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(truckNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Delivery Place: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryPlace, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Driver Name: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(driverName, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Transport: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(transport, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Driver Contact No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(driverContactNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableDeliveryNote);

                // ======================
                // Get Delivery Note Item
                // ======================
                var deliveryNoteItems = _unitOfWork.Repository<DeliveryNoteDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.DeliveryNoteId == deliveryNoteId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.BagWeight,
                                           d.DeliveryPrimaryQuantity,
                                           d.DeliveryQuantity,
                                       }).OrderBy(x => x.ProductName);

                if (deliveryNoteItems.Any())
                {
                    PdfPTable tableDeliveryNoteItems = new(5);
                    float[] widthsCellsDeliveryNoteItems = new float[] { 7f, 10f, 37f, 10f, 12f };
                    tableDeliveryNoteItems.SetWidths(widthsCellsDeliveryNoteItems);
                    tableDeliveryNoteItems.WidthPercentage = 100;
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8)) { Rowspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, VerticalAlignment = Element.ALIGN_MIDDLE });

                    int sl = 0;
                    foreach (var item in deliveryNoteItems)
                    {
                        sl++;
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.DeliveryQuantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    }
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(deliveryNoteItems.Sum(x => x.DeliveryQuantity).ToString("#,##0"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });

                    document.Add(tableDeliveryNoteItems);

                    document.Add(spaceTable);

                    //NumberToWords Table
                    PdfPTable tableNumberToWords = new(2);
                    float[] widthsCellsTableNumberToWords = new float[] { 26f, 74f };
                    tableNumberToWords.WidthPercentage = 100;
                    tableNumberToWords.SetWidths(widthsCellsTableNumberToWords);
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase("Total Quantity In Words: ", fontArial8Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(deliveryNoteItems.Sum(x => x.DeliveryQuantity)), fontArial8)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                    document.Add(tableNumberToWords);
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
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Disclaimer: This is a computer generated Delivery Note. No signature is required. If you have any query, Please call this number: 01313-019140", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 14f, PaddingBottom = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();
        }

        public async Task PrintDeliveryNoteSecondaryReportToPdfAsync(MemoryStream stream, Guid deliveryNoteId, TenantViewModel tenantData, string userName, string headerText)
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
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 10.0F)));

            // ===========
            // Header Page
            // ===========
            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase("Mobile: +8801313019130"+ ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(headerText, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 1f, HorizontalAlignment = Element.ALIGN_CENTER });
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
            // Delivery Note
            // =============
            var deliveryNote = await _unitOfWork.Repository<DeliveryNote>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d => d.Id == deliveryNoteId).FirstOrDefaultAsync();

            if (deliveryNote is not null)
            {
                String? customerCode = deliveryNote.Customer.Code;
                String? customerName = deliveryNote.Customer.Name;
                String? address = deliveryNote.Customer.Address;
                String? contactNo = deliveryNote.Customer.ContactNo;
                String? email = deliveryNote.Customer.Email;
                String? storeName = deliveryNote.Store.Name;
                String? saleOrderNo = deliveryNote.SaleOrderNo;
                String? deliveryNoteNo = deliveryNote.DeliveryNoteNo;
                String? deliveryDate = deliveryNote.DeliveryDate.ToLocal().ToString("dd/MM/yyyy");
                String? deliveryPlace = deliveryNote.DeliveryPlace;
                String? transport = Enum.GetName(typeof(Transport), deliveryNote.Transport)?.Replace("_", " ");
                String? referenceNo = deliveryNote.ReferenceNo;
                String? truckNo = deliveryNote.TruckNo;
                String? driverName = deliveryNote.DriverName;
                String? driverContactNo = deliveryNote.DriverContactNo;
                String? createdOn = deliveryNote.CreatedOn.ToLocal().ToString("dd/MM/yyyy");
                String? remarks = deliveryNote.Remark;
                String? preparedBy = deliveryNote.CreatedBy;
                String? checkedBy;
                if (!string.IsNullOrEmpty(deliveryNote.CheckedBy))
                    checkedBy = deliveryNote.CheckedBy;
                else
                    checkedBy = "";
                String? approvedBy;
                if (!string.IsNullOrEmpty(deliveryNote.ApprovedBy))
                    approvedBy = deliveryNote.ApprovedBy;
                else
                    approvedBy = "";

                PdfPTable tableDeliveryNote = new(4);
                float[] widthscellsTableDeliveryNote = new float[] { 19f, 39f, 19f, 23f };
                tableDeliveryNote.SetWidths(widthscellsTableDeliveryNote);
                tableDeliveryNote.WidthPercentage = 100;
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Challan No: ", fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryNoteNo, fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Sale Order No: ", fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(saleOrderNo, fontArial8)) { Border = 0, PaddingTop = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Customer Name: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(customerName, fontArial8Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Reference No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(referenceNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Address: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(address, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Delivery Date: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryDate, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Mobile: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(contactNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Store Location: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(storeName, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Email: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(email, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Vehicle No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(truckNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Delivery Place: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(deliveryPlace, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Driver Name: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(driverName, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Transportation By: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(transport, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Driver Contact No: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(driverContactNo, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase("Remarks: ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
                tableDeliveryNote.AddCell(new PdfPCell(new Phrase(remarks, fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingBottom = 10, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

                document.Add(tableDeliveryNote);

                // ======================
                // Get Delivery Note Item
                // ======================
                var deliveryNoteItems = _unitOfWork.Repository<DeliveryNoteDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.DeliveryNoteId == deliveryNoteId).Select(d => new
                                       {
                                           d.Id,
                                           ProductCode = d.Product.Code,
                                           ProductName = d.Product.Name,
                                           UnitName = d.Product.MeasurementUnit.Name,
                                           d.BagWeight,
                                           d.DeliveryPrimaryQuantity,
                                           d.DeliveryQuantity,
                                       }).OrderBy(x=>x.ProductName);

                if (deliveryNoteItems.Any())
                {
                    PdfPTable tableDeliveryNoteItems = new(7);
                    float[] widthsCellsDeliveryNoteItems = new float[] { 7f, 10f, 37f, 10f, 12f, 12f, 12f };
                    tableDeliveryNoteItems.SetWidths(widthsCellsDeliveryNoteItems);
                    tableDeliveryNoteItems.WidthPercentage = 100;
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8)) { Colspan = 5, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Code", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Name", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Unit", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Bag Size", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Bag Qty", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });

                    int sl = 0;
                    foreach (var item in deliveryNoteItems)
                    {
                        sl++;
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.ProductCode, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.UnitName, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.BagWeight.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.DeliveryPrimaryQuantity.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                        tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(item.DeliveryQuantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    }
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(deliveryNoteItems.Sum(x => x.DeliveryPrimaryQuantity).ToString(), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });
                    tableDeliveryNoteItems.AddCell(new PdfPCell(new Phrase(deliveryNoteItems.Sum(x => x.DeliveryQuantity).ToString("#,##0"), fontArial8Bold)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 2f, PaddingRight = 2f });

                    document.Add(tableDeliveryNoteItems);

                    document.Add(spaceTable);

                    //NumberToWords Table
                    PdfPTable tableNumberToWords = new(2);
                    float[] widthsCellsTableNumberToWords = new float[] { 31f, 69f };
                    tableNumberToWords.WidthPercentage = 100;
                    tableNumberToWords.SetWidths(widthsCellsTableNumberToWords);
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase("Total Quantity (KG) In Words: ", fontArial8Bold)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    tableNumberToWords.AddCell(new PdfPCell(new Phrase(NumberToWords.ConvertAmount(deliveryNoteItems.Sum(x => x.DeliveryQuantity)), fontArial8)) { Border = 0, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                    document.Add(tableNumberToWords);
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
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Prepared By", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Delivered By", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Received By", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                tableUsers.AddCell(new PdfPCell(new Phrase("Disclaimer: This is a computer generated Delivery Note. If you have any query, Please call this number: 01313-019140", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 14f, PaddingBottom = 2f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
                document.Add(tableUsers);
            }

            document.Close();
        }
    }
}
