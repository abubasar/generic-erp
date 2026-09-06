using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Application.Services.ViewModels.Configuration;
using Application.Core.Enums;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Report.Sales
{
    public class SaleOrderPdfService : ISaleOrderPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public SaleOrderPdfService(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }
        public Paragraph CreateSmallLineSeparator()
        {
            //Small Line
            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 65.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
            return line;
        }

        public Paragraph CreateLineSeparator()
        {
            //Large Line
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_LEFT, 4.5F)));
            return line;
        }

        public PdfPTable SpaceTable(Font fontArial13Bold)
        {
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial13Bold)) { Border = 0 });
            return spaceTable;
        }
        public PdfPTable AddHeaderAsync(TenantViewModel tenantData, string headerText, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10)
        {
            PdfPTable headerTable = new(1);
            float[] widthsCellsHeaderTable = new float[] { 100f };
            headerTable.SetWidths(widthsCellsHeaderTable);
            headerTable.WidthPercentage = 100;

            headerTable.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerTable.AddCell(new PdfPCell(new Phrase(headerText, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            return headerTable;
        }

        public async Task PrintSaleOrderSecondaryReportToPdfAsync(MemoryStream stream, SaleOrder? salesOrder, TenantViewModel tenantData, string headerText)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (salesOrder == null)
                throw new ArgumentNullException(nameof(salesOrder));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(30f, 30f, 30f, 30f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            string? preparedBy = salesOrder.CreatedBy;
            string? checkedBy;
            if (!string.IsNullOrEmpty(salesOrder.CheckedBy))
                checkedBy = salesOrder.CheckedBy;
            else
                checkedBy = "";
            string? approvedBy;
            if (!string.IsNullOrEmpty(salesOrder.ApprovedBy))
                approvedBy = salesOrder.ApprovedBy;
            else
                approvedBy = "";

            var headerTable = AddHeaderAsync(tenantData, headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var saleOrderSubHeaderTable = AddSaleOrderSubHeader(salesOrder, fontArial9, fontArial9Bold);
            var saleOrderSecondaryDataTable = await AddSaleOrderSecondaryDataTable(salesOrder, fontArial8, fontArial8Bold, fontArial9, fontArial9Bold);
            var calculationTable = AddSaleOrderSecondaryDataSubTable(salesOrder, fontArial8, fontArial8Bold, fontArial9, fontArial9Bold);
            var footerTable = AddFooterTable(preparedBy, checkedBy, approvedBy, fontArial9);


            var line1 = CreateLineSeparator();
            var spaceTable = SpaceTable(fontArial8);
            document.Add(headerTable);
            document.Add(line1);
            document.Add(saleOrderSubHeaderTable);
            document.Add(spaceTable);
            document.Add(saleOrderSecondaryDataTable);
            document.Add(spaceTable);
            document.Add(calculationTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(footerTable);

            document.Close();
        }

        private PdfPTable AddSaleOrderPrimarySubHeader(SaleOrder salesOrder, Font fontArial9, Font fontArial9Bold)
        {
            var marketingOfficerName = "";
            var marketingOfficerContactNo = "";
            var marketingOfficerDesignationName = "";
            if (salesOrder!.CustomerMarketingOfficerId.HasValue)
            {
                var marketingOfficer = _unitOfWork.Repository<Employee>().TableNoTracking().Include(x => x.Designation).FirstOrDefault(x => x.Id == salesOrder.CustomerMarketingOfficerId.Value);
                marketingOfficerName = marketingOfficer?.FullName;
                marketingOfficerContactNo = marketingOfficer?.ContactNo;
                marketingOfficerDesignationName = marketingOfficer?.Designation?.Name;
            };
            PdfPTable saleOrderTable = new PdfPTable(4);
            float[] widthscellsSaleOrderTable = new float[] { 15f, 43f, 15f, 27f };
            saleOrderTable.SetWidths(widthscellsSaleOrderTable);
            saleOrderTable.WidthPercentage = 100;
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Customer Code:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Customer.Code, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Sale Order No:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.SaleOrderNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Name:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Customer.Name, fontArial9Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Date:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.OrderDate.ToLocal().ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Address:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Customer.Address, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Delivery Date:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.DeliveryDate.ToLocal().ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Mobile:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Customer.ContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Name:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(marketingOfficerName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Depot:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Store.Name, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Designation:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(marketingOfficerDesignationName, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Remark, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Territory:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder?.CustomerTerritory?.Name, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Colspan = 2, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Mobile:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(marketingOfficerContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

            return saleOrderTable;
        }

        private PdfPTable AddSaleOrderSubHeader(SaleOrder salesOrder, Font fontArial9, Font fontArial9Bold)
        {
            PdfPTable saleOrderTable = new PdfPTable(4);
            float[] widthscellsSaleOrderTable = new float[] { 15f, 43f, 15f, 27f };
            saleOrderTable.SetWidths(widthscellsSaleOrderTable);
            saleOrderTable.WidthPercentage = 100;
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Customer Code:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Customer.Code, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Sale Order No:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.SaleOrderNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Name:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Customer.Name, fontArial9Bold)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Date:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.OrderDate.ToLocal().ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Address:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Customer.Address, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Delivery Date:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.DeliveryDate.ToLocal().ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Mobile:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Customer.ContactNo, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Depot:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Store.Name, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Transport:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(Enum.GetName(typeof(Transport), salesOrder.Transport)?.Replace("_", " "), fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase("Remarks:", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });
            saleOrderTable.AddCell(new PdfPCell(new Phrase(salesOrder.Remark, fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 3f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 0 });

            return saleOrderTable;
        }

        private async Task<PdfPTable> AddSaleOrderSecondaryDataTable(SaleOrder salesOrder, Font fontArial8, Font fontArial8Bold, Font fontArial9, Font fontArial9Bold)
        {
            var salesOrderItems = await _unitOfWork.Repository<SaleOrderDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                       d.SaleOrderId == salesOrder.Id).ToListAsync();

            PdfPTable saleOrderItemsTable = new PdfPTable(12);
            float[] widthsCellsSaleOrderItemsTable = new float[] { 4f, 7f, 20f, 5f, 6f, 6f, 6f, 7f, 8f, 8f, 8f, 13f };
            saleOrderItemsTable.SetWidths(widthsCellsSaleOrderItemsTable);
            saleOrderItemsTable.WidthPercentage = 100;
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Product Description", fontArial9)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9)) { Colspan = 3, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Rate & Discount", fontArial9)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Name", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Bag Weight", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Bag Qty", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Qty", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Rate", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Discount (P.U)", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Offer Discount (P.U)", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Discount Amount", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("TK", fontArial9)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f });

            int sl = 0;

            foreach (var item in salesOrderItems)
            {
                sl++;
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.Product.Code, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.Product.Name, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.Product.MeasurementUnit.Name, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.BagWeight.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.PrimaryQuantity.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.Rate.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.DiscountPerUnit.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.OfferDiscountPerUnit.ToString(), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.DiscountAmount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            }
            return saleOrderItemsTable;
        }

        private PdfPTable AddSaleOrderSecondaryDataSubTable(SaleOrder salesOrder, Font fontArial8, Font fontArial8Bold, Font fontArial9, Font fontArial9Bold)
        {
            PdfPTable calculationTable = new PdfPTable(3);
            float[] widthsCellsCalculationTable = new float[] { 69f, 16f, 15f };
            calculationTable.WidthPercentage = 100;
            calculationTable.SetWidths(widthsCellsCalculationTable);
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Subtotal:", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.Subtotal.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Discount:", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.Discount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Offer Discount:", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.OfferDiscount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Other Discount:", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.OtherDiscount.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Net Sale:", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.Total.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Transportation Cost:", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.TransportationCost.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Depo Charge:", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.DepoCharge.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Total Payable:", fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.NetTotal.ToString("#,##0.00"), fontArial9)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            return calculationTable;
        }

        private PdfPTable AddSaleOrderPrimaryDataSubTable(SaleOrder salesOrder, Font fontArial8, Font fontArial8Bold, Font fontArial9, Font fontArial9Bold)
        {
            decimal? totalVat = 0M;
            foreach (var item in salesOrder.SaleOrderDetails)
            {
                var perc = 1 + (item?.VatPercentage / 100);
                var basePrice = (item?.Rate / perc);
                var totalVatPerItem = item?.Quantity * (item?.Rate - basePrice);
                totalVat += totalVatPerItem;
            }
            PdfPTable calculationTable = new PdfPTable(3);
            float[] widthsCellsCalculationTable = new float[] { 65f, 20f, 15f };
            calculationTable.WidthPercentage = 100;
            calculationTable.SetWidths(widthsCellsCalculationTable);
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Gross TP:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase((salesOrder.Subtotal - totalVat)?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Vat on TP:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(totalVat?.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Discount on TP:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.TotalPercentageDiscountAmount.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Other Discount on TP:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.OtherDiscount.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            calculationTable.AddCell(new PdfPCell(new Phrase("Net Payable:", fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            calculationTable.AddCell(new PdfPCell(new Phrase(salesOrder.NetTotal.ToString("#,##0.00"), fontArial8Bold)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 2 });
            return calculationTable;
        }

        private PdfPTable AddFooterTable(string preparedBy, string checkedBy, string approvedBy, Font fontArial9)
        {
            var line2 = CreateSmallLineSeparator();
            PdfPTable footerTable = new(3);
            float[] widthsCellsFooterTable = new float[] { 100f, 100f, 100f };
            footerTable.WidthPercentage = 100;
            footerTable.SetWidths(widthsCellsFooterTable);
            footerTable.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            footerTable.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            footerTable.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            footerTable.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            footerTable.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            footerTable.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            footerTable.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            footerTable.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            footerTable.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            return footerTable;
        }

        public async Task PrintSaleOrderPrimaryReportToPdfAsync(MemoryStream stream, SaleOrder? salesOrder, TenantViewModel tenantData, string headerText)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (salesOrder == null)
                throw new ArgumentNullException(nameof(salesOrder));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            PdfWriter.GetInstance(document, stream).CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            string? preparedBy = salesOrder.CreatedBy;
            string? checkedBy;
            if (!string.IsNullOrEmpty(salesOrder.CheckedBy))
                checkedBy = salesOrder.CheckedBy;
            else
                checkedBy = "";
            string? approvedBy;
            if (!string.IsNullOrEmpty(salesOrder.ApprovedBy))
                approvedBy = salesOrder.ApprovedBy;
            else
                approvedBy = "";

            var headerTable = AddHeaderAsync(tenantData, headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var saleOrderSubHeaderTable = AddSaleOrderPrimarySubHeader(salesOrder, fontArial9, fontArial9Bold);
            var saleOrderSecondaryDataTable = await AddSaleOrderPrimaryDataTable(salesOrder, fontArial8, fontArial8Bold, fontArial9, fontArial9Bold);
            var calculationTable = AddSaleOrderPrimaryDataSubTable(salesOrder, fontArial8, fontArial8Bold, fontArial9, fontArial9Bold);
            var footerTable = AddFooterTable(preparedBy, checkedBy, approvedBy, fontArial9);


            var line1 = CreateLineSeparator();
            var spaceTable = SpaceTable(fontArial8);
            document.Add(headerTable);
            document.Add(line1);
            document.Add(saleOrderSubHeaderTable);
            document.Add(spaceTable);
            document.Add(saleOrderSecondaryDataTable);
            document.Add(spaceTable);
            document.Add(calculationTable);
            document.Add(spaceTable);
            document.Add(spaceTable);
            document.Add(footerTable);

            document.Close();
        }

        private async Task<PdfPTable> AddSaleOrderPrimaryDataTable(SaleOrder salesOrder, Font fontArial8, Font fontArial8Bold, Font fontArial9, Font fontArial9Bold)
        {
            var salesOrderItems = await _unitOfWork.Repository<SaleOrderDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.PackSize).Where(d =>
                                       d.SaleOrderId == salesOrder.Id).ToListAsync();

            PdfPTable saleOrderItemsTable = new PdfPTable(13);
            float[] widthsCellsSalesInvoiceItems = new float[] { 4f, 5f, 18f, 6f, 8f, 6f, 8f, 8f, 9f, 10f, 8f, 10f, 10f };
            saleOrderItemsTable.SetWidths(widthsCellsSalesInvoiceItems);
            saleOrderItemsTable.WidthPercentage = 100;
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Product Description", fontArial8Bold)) { Colspan = 4, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { Colspan = 2, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Rate & Amount", fontArial8Bold)) { Colspan = 7, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Bonus", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Unit Price", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Unit Vat", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Unit Price with Vat", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Total Vat", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Discount (%)", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Total Price", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });
            saleOrderItemsTable.AddCell(new PdfPCell(new Phrase("Net Amount", fontArial8Bold)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 4f, VerticalAlignment = Element.ALIGN_MIDDLE });

            int sl = 0;

            foreach (var item in salesOrderItems)
            {
                var perc = 1 + (item.VatPercentage / 100);
                var basePrice = (item.Rate / perc);
                var totalVatPerItem = item?.Quantity * (item?.Rate - basePrice);
                sl++;
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item?.Product.Code, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item?.Product.Name, fontArial8)) { HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item?.Product.PackSize.Name, fontArial8)) { HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item?.Quantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item?.BonusQuantity.ToString("#,##0"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(basePrice.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase((item?.Rate - basePrice)?.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item?.Rate.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(totalVatPerItem?.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item?.DiscountPercentage.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase((item?.Amount - totalVatPerItem)?.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
                saleOrderItemsTable.AddCell(new PdfPCell(new Phrase(item?.Amount.ToString("#,##0.00"), fontArial8)) { HorizontalAlignment = 2, PaddingTop = 3f, PaddingBottom = 3f });
            }
            return saleOrderItemsTable;
        }
    }
}
