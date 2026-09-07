using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Core.Extensions;
using Application.Core.Constants;
using Application.Api.Attributes;

namespace Application.Api.ReportApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("report")]
    public class ReportVatInvoiceForPrePrintedPdfWriterController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        public ReportVatInvoiceForPrePrintedPdfWriterController(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.SaleInvoices.Vat_Invoice_For_Pre_Printed_Document)]
        [HttpGet("{saleVatInvoiceId}")]
        public async Task<IActionResult> VatSalesInvoice(Guid saleVatInvoiceId)
        {
            MemoryStream workStream = new();
            Rectangle rectangle = new(PageSize.Letter);
            Document document = new(rectangle, 0, 0, 0, 0);
            PdfWriter writer = PdfWriter.GetInstance(document, workStream);
            writer.CloseStream = false;

            document.Open();

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            // Sales Invoice
            var salesVatInvoice = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Include(x => x.Customer).Include(x => x.Store).Where(d =>
                                d.Id == saleVatInvoiceId).FirstOrDefaultAsync();

            // ======================
            // Get Sales Invoice Item
            // ======================
            var salesVatInvoiceItems = _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(d =>
                                   d.SaleInvoiceId == saleVatInvoiceId).Select(d => new
                                   {
                                       d.Id,
                                       ProductCode = d.Product.Code,
                                       ProductName = d.Product.Name,
                                       UnitName = d.Product.MeasurementUnit.Name,
                                       d.Quantity,
                                       d.NetRate,
                                       NetAmount = d.Quantity * d.NetRate,
                                       d.DeliveryPlace
                                   }).OrderBy(x => x.ProductName);

            // Extract unique DeliveryPlace values
            var uniqueDeliveryPlaces = salesVatInvoiceItems
                .Select(item => item.DeliveryPlace)
                .Distinct();

            // Join the unique values into a comma-separated string
            string deliveryPlace = string.Join(", ", uniqueDeliveryPlaces);


            PdfContentByte canvas = writer.DirectContent;

            // Set font and size
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            canvas.SetFontAndSize(baseFont, 9);

            canvas.BeginText();

            canvas.ShowTextAligned(Element.ALIGN_LEFT, tenantData.Name, 370, 750, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, tenantData.Binno, 370, 730, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, tenantData.Address, 370, 710, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, salesVatInvoice!.Customer.Name, 130, 692, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, "", 130, 672, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, salesVatInvoice!.Customer.Address, 130, 659, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, "", 475, 657, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, deliveryPlace, 130, 637, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, salesVatInvoice!.InvoiceDate.ToLocal().ToString("dd/MM/yyyy"), 475, 637, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, "", 130, 617, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, salesVatInvoice.CreatedOn.ToLocal().ToString("hh:mm:ss tt"), 475, 617, 0);

            canvas.SetFontAndSize(baseFont, 8);
            int sl = 0;
            int height = 485;
            foreach (var salesInvoiceItem in salesVatInvoiceItems)
            {
                sl++;
                canvas.ShowTextAligned(Element.ALIGN_LEFT, sl.ToString(), 10, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_LEFT, salesInvoiceItem?.ProductName, 25, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_LEFT, salesInvoiceItem?.UnitName, 180, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_RIGHT, salesInvoiceItem?.Quantity.ToString("#,##0.00"), 245, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_RIGHT, salesInvoiceItem?.NetRate.ToString("#,##0.00"), 305, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_RIGHT, salesInvoiceItem?.NetAmount.ToString("#,##0.00"), 375, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_RIGHT, "0.00", 400, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_RIGHT, "0.00", 445, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_RIGHT, "0.00", 475, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_RIGHT, "0.00", 540, height, 0);
                canvas.ShowTextAligned(Element.ALIGN_RIGHT, salesInvoiceItem?.NetAmount.ToString("#,##0.00"), 610, height, 0);
                height -= 20;
            }
            canvas.ShowTextAligned(Element.ALIGN_RIGHT, salesVatInvoiceItems.Sum(x => x.NetAmount).ToString("#,##0.00"), 375, 155, 0);
            canvas.ShowTextAligned(Element.ALIGN_RIGHT, "0.00", 400, 155, 0);
            canvas.ShowTextAligned(Element.ALIGN_RIGHT, "0.00", 445, 155, 0);
            canvas.ShowTextAligned(Element.ALIGN_RIGHT, "0.00", 475, 155, 0);
            canvas.ShowTextAligned(Element.ALIGN_RIGHT, "0.00", 540, 155, 0);
            canvas.ShowTextAligned(Element.ALIGN_RIGHT, salesVatInvoiceItems.Sum(x => x.NetAmount).ToString("#,##0.00"), 610, 155, 0);

            canvas.SetFontAndSize(baseFont, 9);

            canvas.ShowTextAligned(Element.ALIGN_LEFT, "Amanullah Khandakar", 250, 131, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, "Junior Officer (Accounts & VAT)", 250, 114, 0);
            canvas.ShowTextAligned(Element.ALIGN_LEFT, "TK. " + salesVatInvoiceItems.Sum(x => x.NetAmount).ToString("#,##0.00"), 180, 60, 0);
            canvas.EndText();

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }
    }
}
