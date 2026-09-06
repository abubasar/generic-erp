using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.PurchaseOrder;
using Application.Services.Dtos.Purchase.PurchaseRequisition;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.SearchRequestModels.Report;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Report.Purchase;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Purchase.Pdf
{
    public class PurchasePdfService : IPurchasePdfService
    {
        private readonly DataContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;

        public PurchasePdfService(DataContext context, IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
        }

        public async Task<PdfPTable> AddHeaderAsync(string reportTitleName, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, DateTime? fromDate, DateTime? toDate)
        {
            Guid? tenantId = _workContext.GetTenantId();
            TenantViewModel tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase($"{tenantData?.ContactNo}, Email : {tenantData?.Email}", fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitleName, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (fromDate.HasValue && toDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + fromDate!.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + toDate!.Value.ToString("dd/MM/yyyy"), fontArial10)) { Border = 0, PaddingTop = 3f, PaddingBottom = 4f, HorizontalAlignment = 0 });
            }
            return headerPage;
        }

        public async Task<PdfPTable> AddHeaderAsync(string reportTitleName, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitleName, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            return headerPage;
        }

        protected virtual Font GetFont()
        {
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pdf", Path.GetFileName("RobotoRegular.ttf"));
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 10, Font.NORMAL);
            return font;
        }

        protected virtual PdfPCell GetPdfCell(object? text, Font font)
        {
            return new PdfPCell(new Phrase(text?.ToString(), font));
        }

        public byte[] AddFooter(byte[] pdf)
        {
            MemoryStream ms = new MemoryStream();
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(pdf);
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // we retrieve the size of the first page
            Rectangle psize = reader.GetPageSize(1);

            // step 1: creation of a document-object
            Document document = new Document(psize, 50, 50, 50, 50);
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3: we open the document

            document.Open();
            // step 4: we add content
            PdfContentByte cb = writer.DirectContent;

            int p = 0;
            // Console.WriteLine("There are " + n + " pages in the document.");
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                document.NewPage();
                p++;

                PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                cb.AddTemplate(importedPage, 0, 0);

                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                //cb.BeginText();
                //cb.SetFontAndSize(bf, 8);

                //cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Md. Ariful Islam", 30, 48, 0);
                //cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Md. Ariful Islam", 265, 48, 0);
                //cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Md. Ariful Islam", 570, 48, 0);
                //cb.EndText();

                cb.MoveTo(30, 42);
                cb.LineTo(120, 42);
                cb.Stroke();

                cb.MoveTo(265, 42);
                cb.LineTo(355, 42);
                cb.Stroke();

                cb.MoveTo(480, 42);
                cb.LineTo(570, 42);
                cb.Stroke();

                cb.BeginText();
                cb.SetFontAndSize(bf, 10);

                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Prepared By", 30, 30, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Checked By", 265, 30, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Approved By", 570, 30, 0);
                cb.EndText();

                cb.BeginText();
                cb.SetFontAndSize(bf, 9);

                cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Printing Date : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), 30, 10, 0);
                cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Powered By: BUTS", 570, 10, 0);
                cb.EndText();
            }
            // step 5: we close the document
            document.Close();
            return ms.ToArray();
        }

        protected virtual async Task<PdfPTable> PrintPurchaseOrderHeaderAsync(string reportHeader)
        {
            Guid? tenantId = _workContext.GetTenantId();
            TenantViewModel tenantData = await _tenantService.GetByIdAsync(tenantId);

            //font
            Font font10 = FontFactory.GetFont("Arial", 10);
            Font font14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            //header
            var numColumns = 1;
            var headerTable = new PdfPTable(1)
            {
                WidthPercentage = 100f
                // RunDirection = GetDirection(lang)
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 100 } }
            };

            headerTable.SetWidths(widths[numColumns]);

            headerTable.DefaultCell.Border = Rectangle.NO_BORDER;
            if (tenantData!.BusinessType == (int)BusinessType.Primary)
            {
                var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", Path.GetFileName(tenantData.Logo!)));
                logo.ScaleToFit(100f, 500f);
                logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                logo.IndentationLeft = 9f;
                logo.IndentationRight = 9f;
                logo.SpacingBefore = 5f;
                logo.SpacingAfter = 2f;
                //Item

                var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
                logocell.AddElement(logo);
                //logocell.Colspan = 2;
                headerTable.AddCell(logocell);
            }
            if (tenantData!.BusinessType == (int)BusinessType.Secondary)
            {
                var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tenantData.Logo!));
                logo.ScaleToFit(100f, 500f);
                logo.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
                logo.IndentationLeft = 9f;
                logo.IndentationRight = 9f;
                logo.SpacingBefore = 5f;
                logo.SpacingAfter = 2f;
                //Item

                var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
                logocell.AddElement(logo);
                //logocell.Colspan = 2;
                headerTable.AddCell(logocell);
            }


            var cellHeader = GetPdfCell("", font10);
            cellHeader.Phrase.Add(new Phrase(tenantData.Name, font14Bold));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(tenantData.Address, font10));
            cellHeader.Phrase.Add(new Phrase(Environment.NewLine));
            cellHeader.Phrase.Add(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, font10));
            cellHeader.HorizontalAlignment = Element.ALIGN_CENTER;
            cellHeader.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(cellHeader);

            return headerTable;
        }

        public PdfPTable PrintPurchaseRequisitionHeader(TenantViewModel tenantData)
        {
            //reportTitleFont
            var reportTitleFont = GetFont();
            reportTitleFont.SetFamily("Times New Roman");
            reportTitleFont.Size = 10;
            reportTitleFont.SetStyle(Font.NORMAL);
            reportTitleFont.Color = BaseColor.Black;

            //headerTitlefonts
            var headerTitleFont = GetFont();
            headerTitleFont.SetFamily("Times New Roman");
            headerTitleFont.Size = 14;
            headerTitleFont.SetStyle(Font.BOLD);
            headerTitleFont.Color = BaseColor.Black;
            //header
            var numColumns = 1;
            var headerTable = new PdfPTable(1)
            {
                WidthPercentage = 100f
                // RunDirection = GetDirection(lang)
            };

            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 100 } }
            };

            headerTable.SetWidths(widths[numColumns]);

            var logo = Image.GetInstance(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", tenantData.Logo!));
            logo.ScaleToFit(50f, 100f);
            logo.Alignment = iTextSharp.text.Image.ALIGN_LEFT;

            var logocell = new PdfPCell { Border = Rectangle.NO_BORDER };
            logocell.AddElement(logo);

            var cellHeader1 = GetPdfCell(tenantData.Name, headerTitleFont);
            cellHeader1.Border = Rectangle.NO_BORDER;
            var cellHeader2 = GetPdfCell(tenantData.Address, reportTitleFont);
            cellHeader2.Border = Rectangle.NO_BORDER;
            var cellHeader3 = GetPdfCell($"{tenantData.Email}/{tenantData.ContactNo}", reportTitleFont);
            cellHeader3.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(logocell);
            headerTable.AddCell(cellHeader1);
            headerTable.AddCell(cellHeader2);
            headerTable.AddCell(cellHeader3);

            return headerTable;
        }

        protected virtual PdfPTable PrintPurchaseRequisitionSubHeader(PurchaseRequisition purchaseRequisition, Account? supplier)
        {
            //font
            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 11;
            font.SetStyle(Font.NORMAL);
            font.Color = BaseColor.Black;

            //fontBold12
            var fontBold12 = GetFont();
            fontBold12.SetFamily("Times New Roman");
            fontBold12.Size = 12;
            fontBold12.SetStyle(Font.BOLD);
            fontBold12.Color = BaseColor.Black;

            //fontBold12
            var fontNormal12 = GetFont();
            fontNormal12.SetFamily("Times New Roman");
            fontNormal12.Size = 12;
            fontNormal12.SetStyle(Font.NORMAL);
            fontNormal12.Color = BaseColor.Black;

            // Report
            var numColumns = 1;
            var invoiceInfoTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f,
            };
            invoiceInfoTable.SpacingBefore = 12f;
            invoiceInfoTable.SpacingAfter = 12f;
            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 100 } }
            };
            invoiceInfoTable.SetWidths(widths[numColumns]);

            var cell1 = GetPdfCell("Request For Quotation " + purchaseRequisition.RequisitionNo, fontBold12);
            cell1.Border = Rectangle.NO_BORDER;
            cell1.Phrase.Add(new Phrase(Environment.NewLine));
            cell1.Phrase.Add(new Phrase(Environment.NewLine));

            var cell2 = GetPdfCell("To", font);
            cell2.Border = Rectangle.NO_BORDER;
            var cell3 = GetPdfCell("      " + supplier?.Name, font);
            cell3.Border = Rectangle.NO_BORDER;
            var cell4 = GetPdfCell("      " + supplier?.Address + ".", font);
            cell4.Border = Rectangle.NO_BORDER;
            cell4.Phrase.Add(new Phrase(Environment.NewLine));
            cell4.Phrase.Add(new Phrase(Environment.NewLine));

            var cell5 = GetPdfCell("Kind Attn:   Mr/Ms. ,", fontNormal12);
            cell5.Border = Rectangle.NO_BORDER;
            cell5.Phrase.Add(new Phrase(Environment.NewLine));
            cell5.HorizontalAlignment = Element.ALIGN_CENTER;

            var cell6 = GetPdfCell("      We request you to kindly provide your competitive Quote for the following items at the earliest.", font);
            cell6.Border = Rectangle.NO_BORDER;

            invoiceInfoTable.AddCell(cell1);
            invoiceInfoTable.AddCell(cell2);
            invoiceInfoTable.AddCell(cell3);
            invoiceInfoTable.AddCell(cell4);
            invoiceInfoTable.AddCell(cell5);
            invoiceInfoTable.AddCell(cell6);
            return invoiceInfoTable;
        }

        protected virtual PdfPTable PrintcellPurchaseItemTable(PurchaseRequisition purchaseRequisition, List<PurchaseRequisitionView> purchaseRequisitionDetails)
        {
            //font
            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 10;
            font.SetStyle(Font.NORMAL);
            font.Color = BaseColor.Black;

            //font
            var titleFont = GetFont();
            titleFont.SetFamily("Times New Roman");
            titleFont.Size = 10;
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;

            var numColumns = 5;
            var purchaseItemTable = new PdfPTable(numColumns)
            {
                WidthPercentage = 100f
            };

            var widthsOfPurchaseItemTable = new Dictionary<int, int[]>
            {
               { numColumns, new[] { 10,40,20,15,15 } }
            };
            purchaseItemTable.SetWidths(widthsOfPurchaseItemTable[numColumns]);

            var cellPurchaseItemTable = GetPdfCell("SL", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            cellPurchaseItemTable = GetPdfCell("Description", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            cellPurchaseItemTable = GetPdfCell("Unit", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            cellPurchaseItemTable = GetPdfCell("Qty", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            cellPurchaseItemTable = GetPdfCell("Required By Date", titleFont);
            cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPurchaseItemTable);

            var r = 0;
            foreach (var ReqDetail in purchaseRequisitionDetails)
            {
                r++;

                cellPurchaseItemTable = GetPdfCell(r, font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseItemTable.AddCell(cellPurchaseItemTable);

                cellPurchaseItemTable = GetPdfCell(ReqDetail.Description ?? "", font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseItemTable.AddCell(cellPurchaseItemTable);

                cellPurchaseItemTable = GetPdfCell(ReqDetail.Unit ?? "", font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseItemTable.AddCell(cellPurchaseItemTable);

                cellPurchaseItemTable = GetPdfCell(ReqDetail.Quantity, font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemTable.AddCell(cellPurchaseItemTable);

                cellPurchaseItemTable = GetPdfCell(purchaseRequisition.ExpectedDeliveryDate?.ToString("dd/MM/yyyy"), font);
                cellPurchaseItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemTable.AddCell(cellPurchaseItemTable);

            }

            return purchaseItemTable;
        }

        protected virtual PdfPTable PrintTermAndConditionTable(PurchaseRequisition purchaseRequisition, TenantViewModel tenantData)
        {
            //font
            var font = GetFont();
            font.SetFamily("Times New Roman");
            font.Size = 9;
            font.SetStyle(Font.NORMAL);
            font.Color = BaseColor.Black;

            //font
            var titleFont = GetFont();
            titleFont.SetFamily("Times New Roman");
            titleFont.Size = 11;
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;

            var numColumns = 3;
            var termAndConditionTable = new PdfPTable(numColumns)
            {
                WidthPercentage = 100f
            };
            termAndConditionTable.SpacingBefore = 12f;
            termAndConditionTable.SpacingAfter = 12f;

            var widths = new Dictionary<int, int[]>
            {
               { numColumns, new[] { 10,30,60 } }
            };
            termAndConditionTable.SetWidths(widths[numColumns]);

            var cellTermAndConditionTable = GetPdfCell("Terms & Conditions :", titleFont);
            cellTermAndConditionTable.Colspan = 3;
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("1", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Payment Terms :", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell(purchaseRequisition.PaymentMode == 1 ? (purchaseRequisition.PaymentTermInDays + " days") : Enum.GetName(typeof(Core.Enums.PaymentMode), purchaseRequisition.PaymentMode) ?? "", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("2", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Transportation :", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell(Enum.GetName(typeof(Transport), purchaseRequisition.Transport)?.Replace("_", " ") ?? "", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("3", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Quotation Deadline :", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Supplier must send quotation within 5 days of receipt of Request for Quotation.", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("4", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Product Details :", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("With item price, also provide more details on products.", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("5", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Quotation Comparison :", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell($"{tenantData.Name} will also approach other suppliers in the market for taking quotations. Your Quotation will be due to evaluation and comparison with other quotations.", font);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);
            return termAndConditionTable;
        }

        public void PrintPurchaseRequisitionInvoiceToPdf(MemoryStream stream, Account? supplier, PurchaseRequisition purchaseRequisition, List<PurchaseRequisitionView> purchaseRequisitionDetails, TenantViewModel tenantData)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchaseRequisition == null)
                throw new ArgumentNullException(nameof(purchaseRequisition));
            if (supplier == null)
                throw new ArgumentNullException(nameof(supplier));

            var pageSize = PageSize.A4;
            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //table
            var headerTable = PrintPurchaseRequisitionHeader(tenantData);
            var subHeaderTable = PrintPurchaseRequisitionSubHeader(purchaseRequisition, supplier);
            var purchaseItemTable = PrintcellPurchaseItemTable(purchaseRequisition, purchaseRequisitionDetails);
            var termAndConditionTable = PrintTermAndConditionTable(purchaseRequisition, tenantData);

            doc.SetMargins(10.0f, 10.0f, 0, 0);
            doc.Add(headerTable);
            doc.Add(subHeaderTable);
            doc.Add(purchaseItemTable);
            doc.Add(termAndConditionTable);
            doc.Close();
        }

        public async Task PrintPurchaseOrderInvoiceToPdf(MemoryStream stream, PurchaseOrder purchaseOrder, List<PurchaseOrderView> purchaseOrderDetails)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchaseOrder == null)
                throw new ArgumentNullException(nameof(purchaseOrder));

            var pageSize = PageSize.A4;
            var doc = new Document(pageSize);

            var pdfWriter = PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //addressFont
            var addressFont = GetFont();
            addressFont.SetFamily("Times New Roman");
            addressFont.Size = 6;
            addressFont.SetStyle(Font.NORMAL);
            addressFont.Color = BaseColor.Black;

            //Tenant info
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            //table
            var headerTable = await PrintPurchaseOrderHeaderAsync("Purchase Order");
            var subHeaderTable = PrintPurchaseOrderSubHeader(purchaseOrder);
            var purchaseOrderItemTable = PrintPurchaseOrderItemTable(purchaseOrder, purchaseOrderDetails);
            var termAndConditionTable = PrintPOTermAndConditionTable(purchaseOrder, tenantData);
            var footerTable = PrintPOFooterTable(purchaseOrder.CreatedBy, purchaseOrder.CheckedBy, purchaseOrder.ApprovedBy);

            doc.SetMargins(16.0f, 16.0f, 0, 0);
            doc.Add(headerTable);
            doc.Add(subHeaderTable);
            doc.Add(purchaseOrderItemTable);
            doc.Add(termAndConditionTable);
            doc.Add(footerTable);
            doc.Close();
        }

        protected virtual PdfPTable PrintPOFooterTable(string createdBy, string checkedBy, string approvedBy)
        {
            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            //Line
            Paragraph line2 = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
            // ==============
            // User Signature
            // ==============
            PdfPTable poFooterTable = new(3);
            float[] widthsCellsPoFooterTable = new float[] { 100f, 100f, 100f };
            poFooterTable.WidthPercentage = 100;
            poFooterTable.SpacingBefore = 10f;
            poFooterTable.SetWidths(widthsCellsPoFooterTable);
            poFooterTable.AddCell(new PdfPCell(new Phrase(createdBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            poFooterTable.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            poFooterTable.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            poFooterTable.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            poFooterTable.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            poFooterTable.AddCell(new PdfPCell(new Phrase(line2)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            poFooterTable.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            poFooterTable.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            poFooterTable.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            poFooterTable.AddCell(new PdfPCell(new Phrase("Disclaimer: This is a computer generated Purchase Order. No signature is required. If you have any query,", fontArial8)) { Colspan = 3, Border = 0, PaddingTop = 14f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            poFooterTable.AddCell(new PdfPCell(new Phrase("Please call this number: 01313-019147", fontArial8)) { Colspan = 3, Border = 0, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });

            poFooterTable.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
            poFooterTable.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial9)) { Colspan = 3, Border = 0, PaddingTop = 1f, HorizontalAlignment = 1 });
            return poFooterTable;
        }

        protected virtual PdfPTable PrintPOTermAndConditionTable(PurchaseOrder purchaseOrder, TenantViewModel tenantData)
        {
            //font
            Font font9 = FontFactory.GetFont("Arial", 9);
            Font font11Bold = FontFactory.GetFont("Arial", 11, Font.BOLD);

            var numColumns = 3;
            var termAndConditionTable = new PdfPTable(numColumns)
            {
                // RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            termAndConditionTable.SpacingBefore = 12f;
            termAndConditionTable.SpacingAfter = 4f;

            var widths = new Dictionary<int, int[]>
            {
               { numColumns, new[] { 10,25,65 } }
            };
            termAndConditionTable.SetWidths(widths[numColumns]);

            var cellTermAndConditionTable = GetPdfCell("Terms & Conditions :", font11Bold);
            cellTermAndConditionTable.Colspan = 3;
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("1", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_CENTER;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Terms of Delivery :", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell(purchaseOrder.DeliveryTermInDays + " days from date of issuance of Purchase Order.", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("2", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_CENTER;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Quality & Quantity :", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Quality & quantity of the delivered goods must be approved & certified by the assigned authority.", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("3", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_CENTER;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Important Clause :", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Buyer reserves the right to change/cancel the part/full order without assigning any reason.", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("4", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_CENTER;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Terms of Payment :", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Final Payment will be made after " + purchaseOrder.PaymentTermInDays + " Days of the completion of full delivery and submission of authorized challan and bill/ invoice.", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("5", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_CENTER;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Weight, Transport Cost & Reject goods :", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            var transportName = Enum.GetName(typeof(Transport), purchaseOrder.Transport);

            if (purchaseOrder.Transport == (int)Transport.AP_FEED)
            {
                transportName = tenantData.Name;
            }
            else
            {
                transportName = Enum.GetName(typeof(Transport), purchaseOrder.Transport);
            }

            transportName = transportName?.Replace("_", " ");

            cellTermAndConditionTable = GetPdfCell("Net weight will be finalized at factory, Transport cost will be paid by " + transportName + ". Supplier need to receive the goods back, if delivered goods are rejected. Supplier will be required to take return of the rejected goods at their own costs.", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("6", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_CENTER;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Partial Delivery :", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell(purchaseOrder.IsPartialDelivery == true ? "Yes" : "No", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("7", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_CENTER;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Variance :", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("+/- " + purchaseOrder.WeightVariance + " %", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("8", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_CENTER;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell("Other Conditions :", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            cellTermAndConditionTable = GetPdfCell(purchaseOrder.TermAndCondition ?? "", font9);
            cellTermAndConditionTable.BorderColor = BaseColor.LightGray;
            cellTermAndConditionTable.HorizontalAlignment = Element.ALIGN_LEFT;
            termAndConditionTable.AddCell(cellTermAndConditionTable);

            return termAndConditionTable;
        }

        protected virtual PdfPTable PrintPurchaseOrderItemTable(PurchaseOrder purchaseOrder, List<PurchaseOrderView> purchaseOrderDetails)
        {
            //font
            Font font9 = FontFactory.GetFont("Arial", 9);
            Font font9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font titleFont = FontFactory.GetFont("Arial", 10, Font.BOLD);

            var numColumns = 6;
            var purchaseItemTable = new PdfPTable(numColumns)
            {
                // RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            purchaseItemTable.SpacingBefore = 4f;
            purchaseItemTable.SpacingAfter = 4f;

            var widths = new Dictionary<int, int[]>
            {
               { numColumns, new[] { 10,40,10,10,15,15 } }
            };
            purchaseItemTable.SetWidths(widths[numColumns]);

            var cellPOItemTable = GetPdfCell("SL", titleFont);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell("Description", titleFont);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell("Unit", titleFont);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell("Qty", titleFont);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell("Rate", titleFont);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell("Amount", titleFont);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
            purchaseItemTable.AddCell(cellPOItemTable);

            var r = 0;
            foreach (var OrderDetail in purchaseOrderDetails)
            {
                r++;

                cellPOItemTable = GetPdfCell(r, font9);
                cellPOItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseItemTable.AddCell(cellPOItemTable);

                cellPOItemTable = GetPdfCell(OrderDetail.Description ?? "", font9);
                cellPOItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
                purchaseItemTable.AddCell(cellPOItemTable);

                cellPOItemTable = GetPdfCell(OrderDetail.Unit ?? "", font9);
                cellPOItemTable.HorizontalAlignment = Element.ALIGN_CENTER;
                purchaseItemTable.AddCell(cellPOItemTable);

                cellPOItemTable = GetPdfCell(OrderDetail.Quantity.ToString("#,##0"), font9);
                cellPOItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemTable.AddCell(cellPOItemTable);

                cellPOItemTable = GetPdfCell(OrderDetail.Rate.ToString("#,##0.000"), font9);
                cellPOItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemTable.AddCell(cellPOItemTable);

                cellPOItemTable = GetPdfCell(OrderDetail.Amount.ToString("#,##0.00"), font9);
                cellPOItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                purchaseItemTable.AddCell(cellPOItemTable);

            }
            cellPOItemTable = GetPdfCell("Total : ", font9Bold);
            cellPOItemTable.Colspan = 3;
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell(purchaseOrderDetails.Sum(x => x.Quantity).ToString("#,##0"), font9Bold);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell("", font9Bold);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell(purchaseOrderDetails.Sum(x => x.Amount).ToString("#,##0.00"), font9Bold);
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_RIGHT;
            purchaseItemTable.AddCell(cellPOItemTable);

            cellPOItemTable = GetPdfCell("Amount In Words :  " + NumberToWords.ConvertAmount(purchaseOrderDetails.Sum(x => x.Amount)), font9);
            cellPOItemTable.Colspan = 6;
            cellPOItemTable.HorizontalAlignment = Element.ALIGN_LEFT;
            cellPOItemTable.Border = Rectangle.NO_BORDER;
            purchaseItemTable.AddCell(cellPOItemTable);

            return purchaseItemTable;
        }

        protected virtual PdfPTable PrintPurchaseOrderSubHeader(PurchaseOrder purchaseOrder)
        {
            //Font
            Font font10 = FontFactory.GetFont("Arial", 10);
            Font font9 = FontFactory.GetFont("Arial", 9);
            Font font10Bold = FontFactory.GetFont("Arial", 10, Font.BOLD);
            Font font12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);

            // Report
            var numColumns = 4;
            var invoiceInfoTable = new PdfPTable(numColumns)
            {
                //  RunDirection = GetDirection(lang),
                WidthPercentage = 100f,
            };
            invoiceInfoTable.SpacingBefore = 6f;
            invoiceInfoTable.SpacingAfter = 4f;
            var widths = new Dictionary<int, int[]>
            {
                { numColumns, new[] { 16,44,18,22 } }
            };
            invoiceInfoTable.SetWidths(widths[numColumns]);

            var cellInvoiceInfo = GetPdfCell("Purchase Order", font12Bold);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_CENTER;
            cellInvoiceInfo.Colspan = 4;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell(" ", font12Bold);
            cellInvoiceInfo.Colspan = 4;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("Supplier", font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell(": " + purchaseOrder.Supplier.Name, font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("P.O. No.", font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell(": " + purchaseOrder.Ponumber, font10Bold);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("Address", font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell(": " + purchaseOrder.Supplier.Address, font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("P.O. Date", font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell(": " + purchaseOrder.Podate.ToString("dd/MM/yyyy"), font10Bold);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("Delivery Place", font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell(": " + purchaseOrder?.DeliveryPlace?.Name, font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("Payment Mode", font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell(": " + Enum.GetName(typeof(Core.Enums.PaymentMode), purchaseOrder!.PaymentMode) ?? "", font10Bold);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("Subject", font10);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell(": Work Order", font10Bold);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            cellInvoiceInfo.Colspan = 3;
            invoiceInfoTable.AddCell(cellInvoiceInfo);

            cellInvoiceInfo = GetPdfCell("      We are pleased to place order upon you, Ref# " + purchaseOrder.ReferenceNo + " to supply us the following items as per terms and conditions laid down:", font9);
            cellInvoiceInfo.HorizontalAlignment = Element.ALIGN_LEFT;
            cellInvoiceInfo.Border = Rectangle.NO_BORDER;
            cellInvoiceInfo.Colspan = 4;
            invoiceInfoTable.AddCell(cellInvoiceInfo);


            return invoiceInfoTable;
        }

        public async Task<PdfPTable> PrintSupplierWisePurchaseItemReportToPdfAsync(MemoryStream stream, List<PurchaseItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fromDate, toDate);
            var supplierWiseItemPurchaseData = SupplierWiseItemPurchaseDataTable(list, fontArial9, fontArial8, fontArial8Bold);
            document.Add(headerTable);
            document.Add(supplierWiseItemPurchaseData);

            document.Close();
            return supplierWiseItemPurchaseData;
        }

        public PdfPTable SupplierWiseItemPurchaseDataTable(List<PurchaseItemViewModel> list, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            var supplierData = list.GroupBy(x => x.SupplierName).OrderBy(x => x.Key).ToList();

            // swpi = supplierWisePurchaseItem
            PdfPTable swpiData = new(9);
            float[] widthsCellsHeaderPage = new float[] { 4f, 8f, 9f, 15f, 26f, 11f, 10f, 7f, 12f };
            swpiData.SetWidths(widthsCellsHeaderPage);
            swpiData.WidthPercentage = 100;
            swpiData.AddCell(new PdfPCell(new Phrase("", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            swpiData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            swpiData.AddCell(new PdfPCell(new Phrase("Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            swpiData.AddCell(new PdfPCell(new Phrase("Store", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            swpiData.AddCell(new PdfPCell(new Phrase("Description", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            swpiData.AddCell(new PdfPCell(new Phrase("PO Number", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            swpiData.AddCell(new PdfPCell(new Phrase("Qty", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            swpiData.AddCell(new PdfPCell(new Phrase("Rate", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            swpiData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            swpiData.HeaderRows = 1;
            foreach (var supplier in supplierData)
            {
                var productData = supplier.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();
                swpiData.AddCell(new PdfPCell(new Phrase(supplier.Key!.ToString(), fontArial8Bold)) { Colspan = 9, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var product in productData)
                {
                    var sl = 0;
                    swpiData.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    swpiData.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    swpiData.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    swpiData.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    swpiData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    foreach (var item in product)
                    {
                        sl++;
                        swpiData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        swpiData.AddCell(new PdfPCell(new Phrase(item?.BillNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        swpiData.AddCell(new PdfPCell(new Phrase(item?.Date.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        swpiData.AddCell(new PdfPCell(new Phrase(item?.StoreName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        swpiData.AddCell(new PdfPCell(new Phrase(item?.Description, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        swpiData.AddCell(new PdfPCell(new Phrase(item?.Pono, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        swpiData.AddCell(new PdfPCell(new Phrase(item?.Quantity.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        swpiData.AddCell(new PdfPCell(new Phrase(item?.Rate.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        swpiData.AddCell(new PdfPCell(new Phrase(item?.Value.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    swpiData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial8Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    swpiData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Quantity).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    swpiData.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    swpiData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Value).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                swpiData.AddCell(new PdfPCell(new Phrase(supplier.Key + " Total : ", fontArial8Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                swpiData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.Quantity).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                swpiData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                swpiData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.Value).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            swpiData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            swpiData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Quantity).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            swpiData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            swpiData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Value).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return swpiData;
        }

        public async Task<PdfPTable> PrintPurchaseItemWiseSupplierReportToPdfAsync(MemoryStream stream, List<PurchaseItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fromDate, toDate);
            var purchaseItemWiseSupplierData = PurchaseItemWiseSupplierDataTable(list, fontArial9, fontArial8, fontArial8Bold);
            document.Add(headerTable);
            document.Add(purchaseItemWiseSupplierData);

            document.Close();
            return purchaseItemWiseSupplierData;
        }

        public PdfPTable PurchaseItemWiseSupplierDataTable(List<PurchaseItemViewModel> list, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            var productData = list.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();

            // piwsData = PurchaseItemWiseSupplier
            PdfPTable piwsData = new(9);
            float[] widthsCellsHeaderPage = new float[] { 4f, 8f, 9f, 15f, 26f, 11f, 10f, 7f, 12f };
            piwsData.SetWidths(widthsCellsHeaderPage);
            piwsData.WidthPercentage = 100;
            piwsData.AddCell(new PdfPCell(new Phrase("", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            piwsData.AddCell(new PdfPCell(new Phrase("Bill No", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            piwsData.AddCell(new PdfPCell(new Phrase("Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            piwsData.AddCell(new PdfPCell(new Phrase("Store", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            piwsData.AddCell(new PdfPCell(new Phrase("Description", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            piwsData.AddCell(new PdfPCell(new Phrase("PO Number", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            piwsData.AddCell(new PdfPCell(new Phrase("Qty", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            piwsData.AddCell(new PdfPCell(new Phrase("Rate", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            piwsData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            piwsData.HeaderRows = 1;
            foreach (var product in productData)
            {
                var supplierData = product.GroupBy(x => x.SupplierName).OrderBy(x => x.Key).ToList();
                piwsData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial8Bold)) { Colspan = 9, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var supplier in supplierData)
                {
                    var sl = 0;
                    piwsData.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    piwsData.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    piwsData.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    piwsData.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    piwsData.AddCell(new PdfPCell(new Phrase(supplier.Key!.ToString(), fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    foreach (var item in supplier)
                    {
                        sl++;
                        piwsData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        piwsData.AddCell(new PdfPCell(new Phrase(item?.BillNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        piwsData.AddCell(new PdfPCell(new Phrase(item?.Date.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        piwsData.AddCell(new PdfPCell(new Phrase(item?.StoreName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        piwsData.AddCell(new PdfPCell(new Phrase(item?.Description, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        piwsData.AddCell(new PdfPCell(new Phrase(item?.Pono, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        piwsData.AddCell(new PdfPCell(new Phrase(item?.Quantity.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        piwsData.AddCell(new PdfPCell(new Phrase(item?.Rate.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        piwsData.AddCell(new PdfPCell(new Phrase(item?.Value.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    piwsData.AddCell(new PdfPCell(new Phrase(supplier.Key + " Total : ", fontArial8Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    piwsData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.Quantity).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    piwsData.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    piwsData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.Value).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                piwsData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial8Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                piwsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Quantity).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                piwsData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                piwsData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Value).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            piwsData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            piwsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Quantity).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            piwsData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            piwsData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Value).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return piwsData;
        }

        public async Task PrintPurchaseOrderSummaryReportToPdf(MemoryStream stream, List<PurchaseOrderViewModel> purchaseOrderViewModels, string reportTitle, bool isDetails, PurchaseOrderRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchaseOrderViewModels == null)
                throw new ArgumentNullException(nameof(purchaseOrderViewModels));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 25f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Gray = FontFactory.GetFont("Arial", 7, BaseColor.Gray);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddPurchaseOrderHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, request);
            var purchaseOrderData = PurchaseOrderDataTable(purchaseOrderViewModels, isDetails, fontArial8, fontArial7, fontArial7Bold, fontArial7Gray);
            document.Add(headerTable);
            document.Add(purchaseOrderData);

            document.Close();
        }

        public PdfPTable PurchaseOrderDataTable(List<PurchaseOrderViewModel> purchaseOrderViewModels, bool isDetails, Font fontArial8, Font fontArial7, Font fontArial7Bold, Font fontArial7Gray)
        {
            PdfPTable purchaseOrderData = new(16);
            float[] widthsCellsHeaderPage = new float[] { 4f, 8f, 6f, 8f, 15f, 6f, 5f, 6f, 4f, 6f, 5f, 5f, 5f, 8f, 5f, 5f };
            purchaseOrderData.SetWidths(widthsCellsHeaderPage);
            purchaseOrderData.WidthPercentage = 100;
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("PO NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("PO Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Store", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Supplier", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Delivery Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Product Origin", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Packaging Type", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Expiry Time", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Transport", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Payment Term In Days", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Delivery Term In Days", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Payment Mode", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Total", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Weight Variance (%)", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Partial Delivery", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            purchaseOrderData.HeaderRows = 1;
            var sl = 0;
            foreach (var item in purchaseOrderViewModels)
            {
                sl++;
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.Ponumber, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.Podate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.Supplier?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.DeliveryDate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.ProductOrigin, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.PackagingType, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.ExpiryTime, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.TransportName?.Replace('_', ' '), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.PaymentTermInDays.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.DeliveryTermInDays.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.PaymentModeName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.Total.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.WeightVariance.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseOrderData.AddCell(new PdfPCell(new Phrase(item!.IsPartialDelivery ? "Yes" : "No", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(5);
                    float[] widthsCellsProductDetailsTable = new float[] { 8f, 50f, 15f, 12f, 15f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Quantity", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Rate", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item.PurchaseOrderDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Quantity.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Rate.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    purchaseOrderData.AddCell(new PdfPCell(new Phrase("Purchase Order Details", fontArial7Gray)) { Colspan = 5, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial8);
                    productDetailsCell2.Colspan = 11;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    purchaseOrderData.AddCell(productDetailsCell2);
                }
            }
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("Total:", fontArial7Bold)) { Colspan = 13, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase(purchaseOrderViewModels.Sum(x => x.Total).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            purchaseOrderData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return purchaseOrderData;
        }

        public async Task<PdfPTable> AddPurchaseOrderHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, PurchaseOrderRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });


            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial10)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.SupplierId.HasValue)
            {
                var supplierName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.SupplierId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Supplier: " + supplierName, fontArial10)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 4f, PaddingBottom = 4f, HorizontalAlignment = 1 });
            return headerPage;
        }

        public async Task PrintGoodsReceiveNoteReportToPdfAsync(MemoryStream stream, List<GoodsReceiveNoteViewModel> goodsReceiveNoteViewModels, string reportTitle, bool isDetails, GoodsReceiveNoteRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (goodsReceiveNoteViewModels == null)
                throw new ArgumentNullException(nameof(goodsReceiveNoteViewModels));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 25f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Gray = FontFactory.GetFont("Arial", 7, BaseColor.Gray);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddGoodsReceiveNoteHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);
            var goodsReceiveNoteData = GoodsReceiveNoteDataTable(goodsReceiveNoteViewModels, isDetails, fontArial8, fontArial7, fontArial7Bold, fontArial7Gray);
            document.Add(headerTable);
            document.Add(goodsReceiveNoteData);

            document.Close();
        }

        public PdfPTable GoodsReceiveNoteDataTable(List<GoodsReceiveNoteViewModel> goodsReceiveNoteViewModels, bool isDetails, Font fontArial8, Font fontArial7, Font fontArial7Bold, Font fontArial7Gray)
        {
            PdfPTable goodsReceiveNoteData = new(14);
            float[] widthsCellsHeaderPage = new float[] { 3f, 8f, 5f, 7f, 9f, 15f, 5f, 5f, 7f, 7f, 7f, 8f, 6f, 8f };
            goodsReceiveNoteData.SetWidths(widthsCellsHeaderPage);
            goodsReceiveNoteData.WidthPercentage = 100;
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("GRN NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("GRN Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("PO NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Store", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Supplier", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Challan NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Challan Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Truck NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Driver Name", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Driver Contact NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Subtotal", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Transport. Cost", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Total", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            goodsReceiveNoteData.HeaderRows = 1;
            var sl = 0;
            foreach (var item in goodsReceiveNoteViewModels)
            {
                sl++;
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.Grnno, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.Grndate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.Ponumber, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.Supplier?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.ChallanNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.ChallanDate?.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.TruckNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.DriverName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.DriverContactNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.Subtotal.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.TransportationCost.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(item?.Total.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(5);
                    float[] widthsCellsProductDetailsTable = new float[] { 8f, 50f, 15f, 12f, 15f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("GRN Quantity", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Rate", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item?.GoodsReceiveNoteDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Grnquantity.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Rate.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Goods Receive Note Details", fontArial7Gray)) { Colspan = 5, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial8);
                    productDetailsCell2.Colspan = 11;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    goodsReceiveNoteData.AddCell(productDetailsCell2);
                }
            }

            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase("Total:", fontArial7Bold)) { Colspan = 11, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(goodsReceiveNoteViewModels.Sum(x => x.Subtotal).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(goodsReceiveNoteViewModels.Sum(x => x.TransportationCost).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            goodsReceiveNoteData.AddCell(new PdfPCell(new Phrase(goodsReceiveNoteViewModels.Sum(x => x.Total).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            return goodsReceiveNoteData;
        }

        public async Task<PdfPTable> AddGoodsReceiveNoteHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, GoodsReceiveNoteRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.SupplierId.HasValue)
            {
                var supplierName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.SupplierId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Supplier: " + supplierName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        public async Task PrintPurchaseInvoiceReportToPdfAsync(MemoryStream stream, List<PurchaseInvoiceViewModel> purchaseInvoiceViewModels, string reportTitle, bool isDetails, PurchaseInvoiceRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchaseInvoiceViewModels == null)
                throw new ArgumentNullException(nameof(purchaseInvoiceViewModels));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 25f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Gray = FontFactory.GetFont("Arial", 7, BaseColor.Gray);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddPurchaseInvoiceHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);
            var purchaseInvoiceData = PurchaseInvoiceDataTable(purchaseInvoiceViewModels, isDetails, fontArial8, fontArial7, fontArial7Bold, fontArial7Gray);
            document.Add(headerTable);
            document.Add(purchaseInvoiceData);

            document.Close();
        }
        public PdfPTable PurchaseInvoiceDataTable(List<PurchaseInvoiceViewModel> purchaseInvoiceViewModels, bool isDetails, Font fontArial8, Font fontArial7, Font fontArial7Bold, Font fontArial7Gray)
        {
            PdfPTable purchaseInvoiceData = new(16);
            float[] widthsCellsHeaderPage = new float[] { 3f, 8f, 6f, 10f, 9f, 8f, 12f, 7f, 6f, 6f, 8f, 6f, 8f, 8f, 8f, 8f };
            purchaseInvoiceData.SetWidths(widthsCellsHeaderPage);
            purchaseInvoiceData.WidthPercentage = 100;
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("SL", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Invoice NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Invoice Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("GRN NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("PO NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Store", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Supplier", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Supplier Invoice NO", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Supplier Invoice Date", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Payment Term In Days", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Subtotal", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Discount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Total", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Advance Payment Amount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Total GRN Adjustment Amount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Net Payable", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BackgroundColor = BaseColor.LightGray });

            purchaseInvoiceData.HeaderRows = 1;
            var sl = 0;
            foreach (var item in purchaseInvoiceViewModels)
            {
                sl++;
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.PurchaseInvoiceNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.InvoiceDate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Grnno?.Replace(",", "\n"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Ponumber, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Supplier?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.SupplierInvoiceNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.SupplierInvoiceDate?.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.PaymentTermInDays.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Subtotal.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Discount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Total.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.AdvancePaymentAmount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.TotalGrnAdjustmentAmount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(item?.NetPayable.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(6);
                    float[] widthsCellsProductDetailsTable = new float[] { 6f, 42f, 8f, 15f, 10f, 19f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("GRN Quantity", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Rate", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item?.PurchaseInvoiceDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Grnquantity.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Rate.ToString(), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial7Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Purchase Invoice Details", fontArial7Gray)) { Colspan = 6, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial8);
                    productDetailsCell2.Colspan = 10;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    purchaseInvoiceData.AddCell(productDetailsCell2);
                }
            }
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial7Bold)) { Colspan = 10, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT, });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(purchaseInvoiceViewModels.Sum(x => x.Subtotal).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT, });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(purchaseInvoiceViewModels.Sum(x => x.Discount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT, });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(purchaseInvoiceViewModels.Sum(x => x.Total).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT, });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(purchaseInvoiceViewModels.Sum(x => x.AdvancePaymentAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT, });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(purchaseInvoiceViewModels.Sum(x => x.TotalGrnAdjustmentAmount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT, });
            purchaseInvoiceData.AddCell(new PdfPCell(new Phrase(purchaseInvoiceViewModels.Sum(x => x.NetPayable).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT, });
            return purchaseInvoiceData;
        }

        public async Task<PdfPTable> AddPurchaseInvoiceHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, PurchaseInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.SupplierId.HasValue)
            {
                var supplierName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.SupplierId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Supplier: " + supplierName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        public async Task PrintPurchaseReturnReportToPdf(MemoryStream stream, List<PurchaseReturnViewModel> purchaseReturnViewModels, string reportTitle, bool isDetails, PurchaseReturnRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (purchaseReturnViewModels == null)
                throw new ArgumentNullException(nameof(purchaseReturnViewModels));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 25f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddPurchaseReturnHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);
            var purchaseReturnData = PurchaseReturnDataTable(purchaseReturnViewModels, isDetails, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);
            document.Add(headerTable);
            document.Add(purchaseReturnData);

            document.Close();
        }

        public PdfPTable PurchaseReturnDataTable(List<PurchaseReturnViewModel> purchaseReturnViewModels, bool isDetails, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable purchaseReturnData = new(9);
            float[] widthsCellsHeaderPage = new float[] { 5f, 10f, 10f, 9f, 10f, 11f, 25f, 20f, 10f };
            purchaseReturnData.SetWidths(widthsCellsHeaderPage);
            purchaseReturnData.WidthPercentage = 100;
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("Purchase Return NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("Purchase Return Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("Reference NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("GRN NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("Store", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("Supplier", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("Remark", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("Total", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            purchaseReturnData.HeaderRows = 1;
            var sl = 0;
            foreach (var item in purchaseReturnViewModels)
            {
                sl++;
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(item?.PurchaseReturnNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(item?.PurchaseReturnDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(item?.ReferenceNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(item?.Grnno, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(item?.Store?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(item?.Supplier?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(item?.Remark, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                purchaseReturnData.AddCell(new PdfPCell(new Phrase(item?.TotalAmount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                if (isDetails)
                {
                    PdfPTable productDetailsTable = new(6);
                    float[] widthsCellsProductDetailsTable = new float[] { 8f, 40f, 10f, 15f, 12f, 15f };
                    productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                    productDetailsTable.WidthPercentage = 100;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.SpacingAfter = 0;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Return Quantity", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Rate", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    var miniSL = 0;
                    foreach (var miniItem in item?.PurchaseReturnDetails!)
                    {
                        miniSL++;
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.MeasurementUnit?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Quantity.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Rate.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    purchaseReturnData.AddCell(new PdfPCell(new Phrase("Purchase Return Details", fontArial8Gray)) { Colspan = 5, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var productDetailsCell2 = GetPdfCell("", fontArial9);
                    productDetailsCell2.Colspan = 4;
                    productDetailsCell2.Padding = 0;
                    productDetailsCell2.AddElement(productDetailsTable);
                    purchaseReturnData.AddCell(productDetailsCell2);
                }
            }
            purchaseReturnData.AddCell(new PdfPCell(new Phrase("Total: ", fontArial8Bold)) { Colspan = 8, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            purchaseReturnData.AddCell(new PdfPCell(new Phrase(purchaseReturnViewModels.Sum(x => x.TotalAmount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return purchaseReturnData;
        }

        public async Task<PdfPTable> AddPurchaseReturnHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, PurchaseReturnRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.SupplierId.HasValue)
            {
                var supplierName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.SupplierId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Supplier: " + supplierName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        public async Task<PdfPTable> PrintPurchaseInvoiceItemReportToPdf(MemoryStream stream, List<PurchaseInvoiceItemViewModel> list, string reportTitle, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fromDate, toDate);
            var purchaseInvoiceItemDataTable = PurchaseInvoiceItemDataTable(list, fontArial9, fontArial8, fontArial8Bold);
            var footer = FooterForPreparedCheckedApproved(fontArial8, fontArial9Bold);
            var spaceTable = SpaceTable(fontArial9);
            document.Add(headerTable);
            document.Add(purchaseInvoiceItemDataTable);
            document.Add(spaceTable);
            document.Add(footer);

            document.Close();

            return purchaseInvoiceItemDataTable;
        }

        public PdfPTable PurchaseInvoiceItemDataTable(List<PurchaseInvoiceItemViewModel> list, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            var productTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();

            PdfPTable purchaseInvoiceItemData = new(5);
            float[] widthsCellsHeaderPage = new float[] { 10f, 45f, 15f, 15f, 15f };
            purchaseInvoiceItemData.SetWidths(widthsCellsHeaderPage);
            purchaseInvoiceItemData.WidthPercentage = 100;
            purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase("Item Name", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase("Rate", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase("Value", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

            purchaseInvoiceItemData.HeaderRows = 1;
            foreach (var productType in productTypeData)
            {
                purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(productType.Key!.ToString(), fontArial8Bold)) { Colspan = 5, HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 5f });
                foreach (var product in productType)
                {
                    purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(product.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(product.ProductName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(product.Quantity.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(product.Rate.ToString("#,##0.000000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(product.Value.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                }
                purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.Value).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            purchaseInvoiceItemData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Value).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return purchaseInvoiceItemData;
        }

        public async Task<PdfPTable> PrintPurchaseReportToPdfAsync(MemoryStream stream, PurchaseReportViewModel list, string reportTitle, DateTime? fromDate, DateTime? toDate)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);


            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fromDate, toDate);
            var purchaseReportDataTable = PurchaseReportDataTable(list, fontArial9, fontArial9Bold);
            document.Add(headerTable);
            document.Add(purchaseReportDataTable);

            document.Close();

            return purchaseReportDataTable;
        }

        public PdfPTable PurchaseReportDataTable(PurchaseReportViewModel list, Font fontArial9, Font fontArial9Bold)
        {
            var netPurchase = list.TotalPurchase + list.TotalFreight - list.TotalPurchaseRetun - list.TotalPurchaseDiscount - list.TotalPOPriceAdjustment;
            PdfPTable purchaseReportData = new(2);
            float[] widthsCellsHeaderPage = new float[] { 50f, 50f };
            purchaseReportData.SetWidths(widthsCellsHeaderPage);
            purchaseReportData.WidthPercentage = 100;
            purchaseReportData.AddCell(new PdfPCell(new Phrase("Particular", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase("Purchase", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase(list.TotalPurchase.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase("Add: Freight In", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase(list.TotalFreight.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase("Less: Purchase Return", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase(list.TotalPurchaseRetun.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase("Less: Purchase Discount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase(list.TotalPurchaseDiscount.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase("Less: PO Price Adjustment After GRN", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase(list.TotalPOPriceAdjustment.ToString("#,##0.00"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase("Net Purchase", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
            purchaseReportData.AddCell(new PdfPCell(new Phrase(netPurchase.ToString("#,##0.00"), fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return purchaseReportData;
        }

        public PdfPTable FooterForPreparedCheckedApproved(Font fontArial8, Font fontArial9Bold)
        {
            //Line
            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
            // ==============
            // User Signature
            // ==============
            PdfPTable tableUsers = new(3);
            float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f };
            tableUsers.WidthPercentage = 100;
            tableUsers.SetWidths(widthsCellsTableUsers);
            tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Prepared By", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Checked By", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Authorised By", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            return tableUsers;
        }

        public PdfPTable SpaceTable(Font fontArial9)
        {
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 5f });
            return spaceTable;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintGrnSupplierItemWiseReportToPdfAsync(MemoryStream stream, List<GrnItemViewModel> list, string reportTitle, GoodsReceiveNoteRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddSubHeader(fontArial9, request.FromDate, request.ToDate, request.SupplierId, request.ProductId, request.StoreId, request.Ponumber);
            var dataTable = GrnSupplierItemWiseDataTable(list, fontArial8, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable GrnSupplierItemWiseDataTable(List<GrnItemViewModel> list, Font fontArial8, Font fontArial7, Font fontArial7Bold)
        {
            var supplierData = list.GroupBy(x => x.SupplierName).OrderBy(x => x.Key).ToList();

            // swpi = supplierWisePurchaseItem
            PdfPTable grnSupplierItemWiseData = new(15);
            float[] widthsCellsHeaderPage = new float[] { 3f, 10f, 7f, 17f, 13f, 9f, 7f, 5f, 9f, 6f, 8f, 6f, 8f, 9f, 9f };
            grnSupplierItemWiseData.SetWidths(widthsCellsHeaderPage);
            grnSupplierItemWiseData.WidthPercentage = 100;
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("GRN No", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("GRN Date", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Store", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Description", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("PO Number", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Transp.", fontArial8)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Landed Cost", fontArial8)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Rate (Including All)", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Amount (Including All)", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            grnSupplierItemWiseData.HeaderRows = 2;
            foreach (var supplier in supplierData)
            {
                var productData = supplier.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(supplier.Key!.ToString(), fontArial7Bold)) { Colspan = 15, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var product in productData)
                {
                    var sl = 0;
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("SL", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial7Bold)) { Colspan = 11, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    foreach (var item in product)
                    {
                        sl++;
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.GrnNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.GrnDate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.StoreName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.Description, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.Pono, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.GrnQuantity.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.Rate.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.Amount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.TransportationRate.ToString("#,##0.00000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase((item?.GrnQuantity * item?.TransportationRate)?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(item?.AdditionalLandedCostRate.ToString("#,##0.00000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase((item?.GrnQuantity * item?.AdditionalLandedCostRate)?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase((item?.Rate + item?.TransportationRate + item?.AdditionalLandedCostRate)?.ToString("#,##0.000000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase((item?.Amount + (item?.GrnQuantity * item?.TransportationRate) + (item?.GrnQuantity * item?.AdditionalLandedCostRate))?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial7Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.GrnQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(" ", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Amount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.GrnQuantity * x.TransportationRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.GrnQuantity * x.AdditionalLandedCostRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Amount + (x.GrnQuantity * x.TransportationRate) + (x.GrnQuantity * x.AdditionalLandedCostRate)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(supplier.Key + " Total : ", fontArial7Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.GrnQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.Amount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.GrnQuantity * x.TransportationRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.GrnQuantity * x.AdditionalLandedCostRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.Amount + (x.GrnQuantity * x.TransportationRate) + (x.GrnQuantity * x.AdditionalLandedCostRate)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.GrnQuantity).ToString("#,##0"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Amount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.GrnQuantity * x.TransportationRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.GrnQuantity * x.AdditionalLandedCostRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnSupplierItemWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Amount + (x.GrnQuantity * x.TransportationRate) + (x.GrnQuantity * x.AdditionalLandedCostRate)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return grnSupplierItemWiseData;
        }

        private PdfPTable AddSubHeader(Font fontArial10, DateTime? fromDate, DateTime? toDate, Guid? supplierId = null, Guid? productId = null, Guid? storeId = null, string? ponumber = null)
        {
            PdfPTable subHeaderPage = new(2);
            float[] widthsCellsHeaderPage = new float[] { 10f, 90f };
            subHeaderPage.SetWidths(widthsCellsHeaderPage);
            subHeaderPage.WidthPercentage = 100;
            subHeaderPage.SpacingAfter = 3;
            if (fromDate.HasValue && toDate.HasValue)
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Date :        " + fromDate.Value.ToString("dd/MM/yyyy") + " to " + toDate.Value.ToString("dd/MM/yyyy"), fontArial10)) { Colspan = 2, Border = 0, HorizontalAlignment = 0 });
            }
            if (supplierId.HasValue)
            {
                var supplierName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == supplierId.Value)?.Name;
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Supplier :  " + supplierName, fontArial10)) { Colspan = 2, Border = 0, HorizontalAlignment = 0 });
            }
            if (productId.HasValue)
            {
                var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == productId.Value)?.Name;
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Product :   " + productName, fontArial10)) { Colspan = 2, Border = 0, HorizontalAlignment = 0 });
            }
            if (storeId.HasValue)
            {
                var storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == storeId.Value)?.Name;
                subHeaderPage.AddCell(new PdfPCell(new Phrase("Store :      " + storeName, fontArial10)) { Colspan = 2, Border = 0, HorizontalAlignment = 0 });
            }
            if (!string.IsNullOrWhiteSpace(ponumber))
            {
                subHeaderPage.AddCell(new PdfPCell(new Phrase("PO No   :  " + ponumber, fontArial10)) { Colspan = 2, Border = 0, HorizontalAlignment = 0 });
            }
            return subHeaderPage;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintGrnItemSupplierWiseReportToPdfAsync(MemoryStream stream, List<GrnItemViewModel> list, string reportTitle, GoodsReceiveNoteRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====

            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddSubHeader(fontArial10, request.FromDate, request.ToDate, request.SupplierId, request.ProductId, request.StoreId, request.Ponumber);
            var dataTable = GrnItemSupplierWiseDataTable(list, fontArial8, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable GrnItemSupplierWiseDataTable(List<GrnItemViewModel> list, Font fontArial8, Font fontArial7, Font fontArial7Bold)
        {
            var productData = list.GroupBy(x => x.ProductName).OrderBy(x => x.Key).ToList();

            PdfPTable grnItemSupplierWiseData = new(15);
            float[] widthsCellsHeaderPage = new float[] { 3f, 10f, 6f, 16f, 12f, 9f, 8f, 5f, 9f, 6f, 8f, 6f, 8f, 8f, 9f };
            grnItemSupplierWiseData.SetWidths(widthsCellsHeaderPage);
            grnItemSupplierWiseData.WidthPercentage = 100;
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("GRN No", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("GRN Date", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Store", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Description", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("PO Number", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Transp.", fontArial8)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Landed Cost", fontArial8)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Rate ( Including All )", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Amount (Including All)", fontArial8)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Rate", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Amount", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            grnItemSupplierWiseData.HeaderRows = 2;
            foreach (var product in productData)
            {
                var supplierData = product.GroupBy(x => x.SupplierName).OrderBy(x => x.Key).ToList();
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(product.Key!.ToString(), fontArial7Bold)) { Colspan = 15, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var supplier in supplierData)
                {
                    var sl = 0;
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("SL", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(" ", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(supplier.Key!.ToString(), fontArial7Bold)) { Colspan = 11, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                    foreach (var item in supplier)
                    {
                        sl++;
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.GrnNo, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.GrnDate.ToString("dd/MM/yyy"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.StoreName, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.Description, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.Pono, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.GrnQuantity.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.Rate.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.Amount.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.TransportationRate.ToString("#,##0.00000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase((item?.GrnQuantity * item?.TransportationRate)?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(item?.AdditionalLandedCostRate.ToString("#,##0.00000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase((item?.GrnQuantity * item?.AdditionalLandedCostRate)?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase((item?.Rate + item?.TransportationRate + item?.AdditionalLandedCostRate)?.ToString("#,##0.0000"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                        grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase((item?.Amount + (item?.GrnQuantity * item?.TransportationRate) + (item?.GrnQuantity * item?.AdditionalLandedCostRate))?.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(supplier.Key + " Total : ", fontArial7Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.GrnQuantity).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(" ", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.Amount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.GrnQuantity * x.TransportationRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.GrnQuantity * x.AdditionalLandedCostRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(supplier.Sum(x => x.Amount + (x.GrnQuantity * x.TransportationRate) + (x.GrnQuantity * x.AdditionalLandedCostRate)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(product.Key + " Total : ", fontArial7Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.GrnQuantity).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Amount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.GrnQuantity * x.TransportationRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.GrnQuantity * x.AdditionalLandedCostRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(product.Sum(x => x.Amount + (x.GrnQuantity * x.TransportationRate) + (x.GrnQuantity * x.AdditionalLandedCostRate)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial7Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.GrnQuantity).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Amount).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.GrnQuantity * x.TransportationRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.GrnQuantity * x.AdditionalLandedCostRate).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase("", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnItemSupplierWiseData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Amount + (x.GrnQuantity * x.TransportationRate) + (x.GrnQuantity * x.AdditionalLandedCostRate)).ToString("#,##0.00"), fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return grnItemSupplierWiseData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintGrnTotalQuantityValueAveragePriceReportToPdfAsync(MemoryStream stream, List<GrnItemSummaryViewModel> list, string reportTitle, GoodsReceiveNoteRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            var headerTable = await AddHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddSubHeader(fontArial10, request.FromDate, request.ToDate, request.SupplierId, request.ProductId, request.StoreId);
            var dataTable = GrnTotalQuantityValueAveragePriceDataTable(list, fontArial9, fontArial8, fontArial8Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        private PdfPTable GrnTotalQuantityValueAveragePriceDataTable(List<GrnItemSummaryViewModel> list, Font fontArial9, Font fontArial8, Font fontArial8Bold)
        {
            var productTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();

            PdfPTable grnTotalQuantityValueAveragePriceData = new(9);
            float[] widthsCellsHeaderPage = new float[] { 8f, 30f, 15f, 14f, 15f, 12f, 12f, 15f, 15f };
            grnTotalQuantityValueAveragePriceData.SetWidths(widthsCellsHeaderPage);
            grnTotalQuantityValueAveragePriceData.WidthPercentage = 100;
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Item Name", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Rate", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Transp. Rate", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Transp. Cost", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Rate (Including Transp. Rate)", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Amount (Including Transp. Cost)", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            grnTotalQuantityValueAveragePriceData.HeaderRows = 1;
            foreach (var productType in productTypeData)
            {
                grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(productType.Key!.ToString(), fontArial8Bold)) { Colspan = 9, HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 5f });
                foreach (var product in productType)
                {
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(product.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(product.ProductName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(product.GrnQuantity.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(product.Rate.ToString("#,##0.000000"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(product.Amount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(product.TransportationRate.ToString("#,##0.000000"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(product.TransportationCost.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase((product.Rate + product.TransportationRate).ToString("#,##0.000000"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                    grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase((product.Amount + product.TransportationCost).ToString("#,##0.00"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                }
                grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(productType.Key + " Total : ", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.Amount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.TransportationCost).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(productType.Sum(x => x.Amount + x.TransportationCost).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            }
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("Grand Total : ", fontArial8Bold)) { Colspan = 4, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Amount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.TransportationCost).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase("", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            grnTotalQuantityValueAveragePriceData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Amount + x.TransportationCost).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
            return grnTotalQuantityValueAveragePriceData;
        }

        public async Task PrintLCCostEntryReportToPdf(MemoryStream stream, List<LCCostEntryViewModel> lCCostEntryViewModels, string reportTitle, bool isDetails, LCCostEntryRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (lCCostEntryViewModels == null)
                throw new ArgumentNullException(nameof(lCCostEntryViewModels));

            //InitializeDocument
            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;

            document.Open();

            //Fonts
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            //Add header table
            var headerTable = await AddLcCostEntryHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var lcCostEntryData = LcCostEntryDataTable(lCCostEntryViewModels, isDetails, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(lcCostEntryData);
            document.Close();
        }

        private async Task<PdfPTable> AddLcCostEntryHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, LCCostEntryRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.CostCenterId.HasValue)
            {
                var customerName = _unitOfWork.Repository<CostCenter>().TableNoTracking().FirstOrDefault(x => x.Id == request.CostCenterId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Cost Center: " + customerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }

            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });

            return headerPage;
        }


        private PdfPTable LcCostEntryDataTable(List<LCCostEntryViewModel> lCCostEntryViewModels, bool isDetails, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable lcCostEntryData = new(8);
            float[] widthCellsHeaderPage = new float[] { 8f, 15f, 12f, 15f, 10f, 12f, 18f, 10f };
            lcCostEntryData.SetWidths(widthCellsHeaderPage);
            lcCostEntryData.WidthPercentage = 100;
            lcCostEntryData.HeaderRows = 1;

            lcCostEntryData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryData.AddCell(new PdfPCell(new Phrase("PO NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryData.AddCell(new PdfPCell(new Phrase("LC NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryData.AddCell(new PdfPCell(new Phrase("Entry Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryData.AddCell(new PdfPCell(new Phrase("Cost Center", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryData.AddCell(new PdfPCell(new Phrase("Remarks", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryData.AddCell(new PdfPCell(new Phrase("Total", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in lCCostEntryViewModels)
            {
                sl++;
                lcCostEntryData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryData.AddCell(new PdfPCell(new Phrase(item?.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryData.AddCell(new PdfPCell(new Phrase(item?.Ponumber, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryData.AddCell(new PdfPCell(new Phrase(item?.LcNumber, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryData.AddCell(new PdfPCell(new Phrase(item?.EntryDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryData.AddCell(new PdfPCell(new Phrase(item?.CostCenter?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                lcCostEntryData.AddCell(new PdfPCell(new Phrase(item?.Remark, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryData.AddCell(new PdfPCell(new Phrase(item?.Total.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (isDetails)
                {
                    PdfPTable lcCostEntryDetailsTable = new(5);
                    float[] widthsCellslcCostEntryDetailsTable = new float[] { 7f, 28f, 42f, 10f, 13f };
                    lcCostEntryDetailsTable.SetWidths(widthsCellslcCostEntryDetailsTable);
                    lcCostEntryDetailsTable.WidthPercentage = 100;
                    lcCostEntryDetailsTable.SpacingAfter = 0;
                    lcCostEntryDetailsTable.SpacingAfter = 0;

                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("Debit Account", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("Credit Account", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("Is Landed Cost", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

                    var miniSL = 0;
                    foreach (var miniItem in item?.LccostEntryDetails!)
                    {
                        miniSL++;
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.DebitAccount?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.CreditAccount?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.IsIncludedWithinLandedCost == true ? "Yes" : "No", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    lcCostEntryData.AddCell(new PdfPCell(new Phrase("LC Cost Entry Details", fontArial8Gray)) { Colspan = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var lcCostEntryDetailsCell2 = GetPdfCell("", fontArial9);
                    lcCostEntryDetailsCell2.Colspan = 6;
                    lcCostEntryDetailsCell2.Padding = 0;
                    lcCostEntryDetailsCell2.AddElement(lcCostEntryDetailsTable);
                    lcCostEntryData.AddCell(lcCostEntryDetailsCell2);
                }
            }

            lcCostEntryData.AddCell(new PdfPCell(new Phrase("Total:", fontArial8Bold)) { Colspan = 7, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            lcCostEntryData.AddCell(new PdfPCell(new Phrase(lCCostEntryViewModels.Sum(x => x.Total).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return lcCostEntryData;
        }

        public async Task PrintLcAdjustmentReportToPdf(MemoryStream stream, List<LcAdjustmentViewModel> lcAdjustmentViewModels, string reportTitle, bool isDetails, LcAdjustmentRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (lcAdjustmentViewModels == null)
                throw new ArgumentNullException(nameof(lcAdjustmentViewModels));

            //InitializeDocument
            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;

            document.Open();

            //Fonts
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            //Add header table
            var headerTable = await AddLcAdjustmentHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var lcAdjustmentData = LcAdjustmentDataTable(lcAdjustmentViewModels, isDetails, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(lcAdjustmentData);
            document.Close();
        }



        private async Task<PdfPTable> AddLcAdjustmentHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, LcAdjustmentRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }
            if (request.CostCenterId.HasValue)
            {
                var customerName = _unitOfWork.Repository<CostCenter>().TableNoTracking().FirstOrDefault(x => x.Id == request.CostCenterId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Cost Center: " + customerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }

            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });

            return headerPage;
        }

        private PdfPTable LcAdjustmentDataTable(List<LcAdjustmentViewModel> lcAdjustmentViewModels, bool isDetails, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable lcAdjustmentData = new(8);
            float[] widthCellsHeaderPage = new float[] { 6f, 11f, 14f, 12f, 12f, 25f, 10f, 10f };
            lcAdjustmentData.SetWidths(widthCellsHeaderPage);
            lcAdjustmentData.WidthPercentage = 100;
            lcAdjustmentData.HeaderRows = 1;

            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("Purchase Invoice NO", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("Adjustment Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("Cost Center", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("Remarks", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("Invoice Total", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("LC Margin Total", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in lcAdjustmentViewModels)
            {
                sl++;
                lcAdjustmentData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.Code, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.PurchaseInvoiceNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.AdjustmentDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.CostCenter?.Name, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                lcAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.Remark, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.InvoiceTotal.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                lcAdjustmentData.AddCell(new PdfPCell(new Phrase(item?.LcMarginTotal.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });

                if (isDetails)
                {
                    PdfPTable lcCostEntryDetailsTable = new(5);
                    float[] widthsCellslcCostEntryDetailsTable = new float[] { 7f, 24f, 48f, 8f, 12f };
                    lcCostEntryDetailsTable.SetWidths(widthsCellslcCostEntryDetailsTable);
                    lcCostEntryDetailsTable.WidthPercentage = 100;
                    lcCostEntryDetailsTable.SpacingAfter = 0;
                    lcCostEntryDetailsTable.SpacingAfter = 0;

                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("Account", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("Account Tree", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("Post Type", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase("Amount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

                    var miniSL = 0;
                    foreach (var miniItem in item?.LcAdjustmentDetails!)
                    {
                        miniSL++;
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Account?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.AccountDescription, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(Enum.GetName(typeof(PostType), miniItem!.PostType)?.Replace("_", " "), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                        lcCostEntryDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Amount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    }

                    lcAdjustmentData.AddCell(new PdfPCell(new Phrase("LC Adjustment Details", fontArial8Gray)) { Colspan = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                    var lcCostEntryDetailsCell2 = GetPdfCell("", fontArial9);
                    lcCostEntryDetailsCell2.Colspan = 6;
                    lcCostEntryDetailsCell2.Padding = 0;
                    lcCostEntryDetailsCell2.AddElement(lcCostEntryDetailsTable);
                    lcAdjustmentData.AddCell(lcCostEntryDetailsCell2);
                }
            }

            lcAdjustmentData.AddCell(new PdfPCell(new Phrase("Total:", fontArial8Bold)) { Colspan = 6, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase(lcAdjustmentViewModels.Sum(x => x.InvoiceTotal).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            lcAdjustmentData.AddCell(new PdfPCell(new Phrase(lcAdjustmentViewModels.Sum(x => x.LcMarginTotal).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return lcAdjustmentData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintLcCostEntryAgainstPurchaseOrderReportToPdf(MemoryStream stream, List<LcCostEntryDetailAgainstPoViewModel> list, string headerText, string lcnumber, string ponumber)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            //InitializeDocument
            Rectangle rectangle = new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;

            document.Open();

            //Fonts
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 8, BaseColor.Gray);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial12Bold = FontFactory.GetFont("Arial", 12, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            //Add header table
            var headerTable = await AddLcCostEntryAgainstPurchaseOrderHeaderAsync(headerText, fontArial13Bold, fontArial12Bold, fontArial10, fontArial9);
            var filterTable = AddLcCostEntryAgainstPurchaseOrderSubHeaderAsync(fontArial10, lcnumber, ponumber);
            //Add data table
            var dataTable = lcCostEntryListAgainstPurchaseOrderDataTable(list, fontArial9, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Close();
            return (filterTable, dataTable);
        }

        private async Task<PdfPTable> AddLcCostEntryAgainstPurchaseOrderHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial12Bold, Font fontArial10, Font fontArial9)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData!.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.ContactNo + ", Email : " + tenantData?.Email, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitle, fontArial12Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial12Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        public PdfPTable AddLcCostEntryAgainstPurchaseOrderSubHeaderAsync(Font fontArial10, string? lcnumber, string? ponumber)
        {
            PdfPTable subHeaderPage = new(1);
            float[] widthsCellsSubHeaderPage = new float[] { 100f };
            subHeaderPage.SetWidths(widthsCellsSubHeaderPage);
            subHeaderPage.WidthPercentage = 100;

            subHeaderPage.AddCell(new PdfPCell(new Phrase("LC Number: " + lcnumber, fontArial10)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
            subHeaderPage.AddCell(new PdfPCell(new Phrase("Purchase Order No: " + ponumber, fontArial10)) { Border = 0, PaddingTop = 2f, HorizontalAlignment = 0 });
            subHeaderPage.AddCell(new PdfPCell(new Phrase("", fontArial10)) { Border = 0, PaddingTop = 3f, HorizontalAlignment = 0 });
            return subHeaderPage;
        }

        private PdfPTable lcCostEntryListAgainstPurchaseOrderDataTable(List<LcCostEntryDetailAgainstPoViewModel> list, Font fontArial9, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable lcCostEntryListAgainstPurchaseOrderData = new(8);
            float[] widthCellsHeaderPage = new float[] { 6f, 10f, 18f, 35f, 50f, 15f, 16f, 28f };
            lcCostEntryListAgainstPurchaseOrderData.SetWidths(widthCellsHeaderPage);
            lcCostEntryListAgainstPurchaseOrderData.WidthPercentage = 100;
            lcCostEntryListAgainstPurchaseOrderData.HeaderRows = 1;

            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("SL", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("Date", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("Code", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("Debit Account", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("Credit Account", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("Amount", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("Is Landed Cost", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("Status", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });

            var sl = 0;
            foreach (var item in list)
            {
                sl++;
                lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(item.EntryDate?.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(item.LcCostEntryNo, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.DebitAccountName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.CreditAccountName, fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(item?.Amount.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(item!.IsIncludedWithinLandedCost ? "Yes" : "No", fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(Enum.GetName(typeof(Core.Enums.LCCostEntryStatus), item.Status)?.Replace("_", " "), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
            }

            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase("Total : ", fontArial8Bold)) { Colspan = 5, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(list.Sum(x => x.Amount).ToString("#,##0.00"), fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
            lcCostEntryListAgainstPurchaseOrderData.AddCell(new PdfPCell(new Phrase(" ", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

            return lcCostEntryListAgainstPurchaseOrderData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductsWithLatestPurchasePriceAsyncReportToPdfAsync(MemoryStream stream, List<ProductWithLatestPriceViewModel> list, string headerText, ProductWithLatestPriceRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            //InitializeDocument
            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;

            document.Open();

            //Fonts
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial8 = FontFactory.GetFont("Arial", 8);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            //Add header table
            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddProductsWithLatestPurchasePriceSubHeaderAsync(fontArial9, request);
            //Add data table
            var dataTable = ProductsWithLatestPurchasePriceDataTable(list, fontArial9Bold, fontArial8);

            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);
            document.Close();
            return (filterTable, dataTable);
        }
        private PdfPTable AddProductsWithLatestPurchasePriceSubHeaderAsync(Font fontArial9, ProductWithLatestPriceRequestModel request)
        {
            PdfPTable subHeaderTable = new(2);
            float[] widthsCellsHeaderTable = new float[] { 10f, 90f };
            subHeaderTable.SetWidths(widthsCellsHeaderTable);
            subHeaderTable.WidthPercentage = 100;
            subHeaderTable.SpacingAfter = 3;
            if (request.ToDate.HasValue)
            {
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Date :                " + request.ToDate?.ToString("dd/MM/yyyy"), fontArial9)) { Colspan = 2, Border = 0, HorizontalAlignment = 0 });
            }
            if (request.ProductTypeId.HasValue)
            {
                var productTypeName = _unitOfWork.Repository<ProductType>().TableNoTracking().FirstOrDefault(x => x.Id == request.ProductTypeId.Value)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Product Type :  " + productTypeName, fontArial9)) { Colspan = 2, Border = 0, HorizontalAlignment = 0 });
            }
            if (request.ProductId.HasValue)
            {
                var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == request.ProductId.Value)?.Name;
                subHeaderTable.AddCell(new PdfPCell(new Phrase("Product :           " + productName, fontArial9)) { Colspan = 2, Border = 0, HorizontalAlignment = 0 });
            }
            return subHeaderTable;
        }
        private PdfPTable ProductsWithLatestPurchasePriceDataTable(List<ProductWithLatestPriceViewModel> list, Font fontArial9Bold, Font fontArial8)
        {
            var productTypeData = list.GroupBy(x => x.ProductTypeName).OrderBy(x => x.Key).ToList();
            PdfPTable productsWithLatestPurchasePriceData = new(8);
            float[] widthsCellsHeaderPage = new float[] { 5f, 8f, 25f, 6f, 8f, 8f, 10f, 10f };
            productsWithLatestPurchasePriceData.SetWidths(widthsCellsHeaderPage);
            productsWithLatestPurchasePriceData.WidthPercentage = 100;
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("S/L", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Last Received Date", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Product Name", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Unit", fontArial9Bold)) { Rowspan = 2, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Last Purchase Rate", fontArial9Bold)) { Colspan = 2, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Quantity", fontArial9Bold)) { Colspan = 2, PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Supplier Rate", fontArial9Bold)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Net Rate", fontArial9Bold)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Received", fontArial9Bold)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase("Balance", fontArial9Bold)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            var sl = 0;
            productsWithLatestPurchasePriceData.HeaderRows = 2;
            foreach (var productType in productTypeData)
            {
                productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(productType.Key!.ToString(), fontArial8)) { Colspan = 8, HorizontalAlignment = 1, PaddingTop = 4f, PaddingBottom = 5f });
                foreach (var item in productType)
                {
                    sl++;
                    productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                    productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(item.GrnDate?.ToString("dd/MM/yyyy"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                    productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(item.ProductName, fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 0 });
                    productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(item.MeasurementUnitName, fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 1 });
                    productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(item.GrnRate.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                    productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(item.LatestRate.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                    productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(item.GrnQuantity.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                    productsWithLatestPurchasePriceData.AddCell(new PdfPCell(new Phrase(item.BalanceQuantity.ToString("#,##0.00"), fontArial8)) { PaddingTop = 3f, PaddingBottom = 3f, HorizontalAlignment = 2 });
                }
            }

            return productsWithLatestPurchasePriceData;

        }
    }
}
