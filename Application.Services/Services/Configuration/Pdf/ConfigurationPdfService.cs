using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Extensions;
using Application.Core.Industry;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Configuration.Pdf
{
    public class ConfigurationPdfService : IConfigurationPdfService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        private readonly IIndustryProfile _industry;

        public ConfigurationPdfService(IUnitOfWork unitOfWork, IWorkContext workContext, ITenantService tenantService, IIndustryProfile industry)
        {
            _unitOfWork = unitOfWork;
            _workContext = workContext;
            _tenantService = tenantService;
            _industry = industry;
        }
        protected virtual PdfPCell GetPdfCell(object text, Font font)
        {
            return new PdfPCell(new Phrase(text.ToString(), font));
        }
        public Paragraph CreateSmallLineSeparator()
        {
            //Small Line
            Paragraph line = new(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 70.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
            return line;
        }
        public Paragraph CreateLineSeparator()
        {
            //Line
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.Black, Element.ALIGN_CENTER, 4.5F)));
            return line;
        }
        public PdfPTable SpaceTable(Font fontArial9)
        {
            // =====
            // Space
            // =====
            PdfPTable spaceTable = new PdfPTable(1);
            float[] widthCellsSpaceTable = new float[] { 100f };
            spaceTable.SetWidths(widthCellsSpaceTable);
            spaceTable.WidthPercentage = 100;
            spaceTable.AddCell(new PdfPCell(new Phrase(" ", fontArial9)) { Border = 0, PaddingTop = 5f });

            return spaceTable;
        }

        public PdfPTable AddFooter(string preparedBy, string checkedBy, string approvedBy, Font fontArial8, Font fontArial9Bold)
        {
            var line = CreateSmallLineSeparator();
            PdfPTable tableUsers = new(4);
            float[] widthsCellsTableUsers = new float[] { 100f, 100f, 100f, 100f };
            tableUsers.WidthPercentage = 100;
            tableUsers.SetWidths(widthsCellsTableUsers);
            tableUsers.AddCell(new PdfPCell(new Phrase(preparedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(checkedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(approvedBy, fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(" ", fontArial8)) { Border = 0, PaddingTop = 3f, PaddingBottom = 0f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase(line)) { Border = 0, HorizontalAlignment = 1, Padding = 0 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Prepared by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Checked by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Approved by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Authorized by", fontArial9Bold)) { Border = 0, PaddingTop = 0f, PaddingBottom = 7f, PaddingLeft = 5f, PaddingRight = 5f, HorizontalAlignment = 1 });

            tableUsers.AddCell(new PdfPCell(new Phrase("Software By: Bangladesh Unique Technology Services, www.butsbd.com", fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 5f, HorizontalAlignment = 1 });
            tableUsers.AddCell(new PdfPCell(new Phrase("Printed " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), fontArial8)) { Colspan = 4, Border = 0, PaddingTop = 1f, HorizontalAlignment = 1 });

            return tableUsers;
        }

        public async Task PrintReportToPdfAsync(MemoryStream stream, List<string> headers, List<float> columnWidths, List<object> tableData, string reportTitleName, bool isLandscape = false)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, isLandscape);
            //fonts
            Font tableHeaderFont = FontFactory.GetFont("Arial", 9, Font.BOLD);
            Font tableDataFont = FontFactory.GetFont("Arial", 9);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(reportTitleName, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            // Add the table
            pdfGenerator.AddTable(tableData, headers, columnWidths, tableHeaderFont, tableDataFont);

            // Close the PDF document
            pdfGenerator.Close();
        }

        public async Task PrintSupplierReportToPdfAsync(MemoryStream stream, List<SupplierViewModel> supplierViewModels, string reportTitleName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (supplierViewModels == null)
                throw new ArgumentNullException(nameof(supplierViewModels));

            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font tableHeaderFont = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font tableDataFont = FontFactory.GetFont("Arial", 8);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(reportTitleName, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            // Add the table
            AddSupplierTable(pdfGenerator, supplierViewModels, tableHeaderFont, tableDataFont);
            // Close the PDF document
            pdfGenerator.Close();
        }

        public async Task PrintCustomerReportToPdfAsync(MemoryStream stream, List<CustomerViewModel> customerViewModels, string reportTitleName, CustomerRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (customerViewModels == null)
                throw new ArgumentNullException(nameof(customerViewModels));
            PdfGenerator pdfGenerator = new PdfGenerator(stream, true);
            //fonts
            Font tableHeaderFont = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font tableDataFont = FontFactory.GetFont("Arial", 8);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);

            //Company Information
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            // Add the header
            pdfGenerator.AddHeader(reportTitleName, tenantData.Name!, tenantData.Address!, tenantData.ContactNo!, tenantData.Email!);
            // Add Customer Filter Table
            pdfGenerator.AddTable(AddCustomerFilteringTable(fontArial9, request));
            // Add the table
            AddCustomerTable(pdfGenerator, customerViewModels, tableHeaderFont, tableDataFont);
            // Close the PDF document
            pdfGenerator.Close();
        }

        public PdfPTable AddCustomerFilteringTable(Font font, CustomerRequestModel request)
        {
            PdfPTable customerFilteringTable = new(1);
            float[] widthsCellsCustomerFilteringTable = new float[] { 100f };
            customerFilteringTable.SetWidths(widthsCellsCustomerFilteringTable);
            customerFilteringTable.WidthPercentage = 100;

            if (request.CustomerMarketingOfficerId.HasValue)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId)?.FullName;
                customerFilteringTable.AddCell(new PdfPCell(new Phrase("Marketing Officer : " + marketingOfficerName, font)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (request.CustomerRegionId.HasValue)
            {
                var customerRegionName = _unitOfWork.Repository<Region>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerRegionId)?.Name;
                customerFilteringTable.AddCell(new PdfPCell(new Phrase("Region :                 " + customerRegionName, font)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (request.CustomerZoneId.HasValue)
            {
                var customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)?.Name;
                customerFilteringTable.AddCell(new PdfPCell(new Phrase("Zone :                    " + customerZoneName, font)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (request.CustomerAreaId.HasValue)
            {
                var customerAreaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerAreaId)?.Name;
                customerFilteringTable.AddCell(new PdfPCell(new Phrase("Area :                     " + customerAreaName, font)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }

            return customerFilteringTable;
        }

        public void AddCustomerTable(PdfGenerator pdfGenerator, List<CustomerViewModel> customerViewModels, Font tableHeaderFont, Font tableDataFont)
        {
            PdfPTable customerTable = new(14) { WidthPercentage = 100, SpacingBefore = 10f, SpacingAfter = 10f };
            float[] widthsCellsCustomerTable = new float[] { 3f, 7f, 10f, 8f, 7f, 8f, 10f, 7f, 8f, 7f, 8f, 7f, 5f, 6f };
            customerTable.SetWidths(widthsCellsCustomerTable);
            List<string> headers = new List<string> { "SL", "Code", "Name", "Owner's Name", "Contact No", "Email", "Address", "Trade License", "Contact Person Name", "Contact Person Contact No", "Marketing Officer", "Credit Limit", "Credit Days", "Target Qty" };
            foreach (var header in headers)
            {
                customerTable.AddCell(pdfGenerator.GetHeaderCell(header, tableHeaderFont, Element.ALIGN_CENTER));
            }

            int sl = 0;
            customerTable.HeaderRows = 1;
            foreach (var item in customerViewModels)
            {
                sl++;
                var marketingOfficer = _unitOfWork.Repository<Employee>().TableNoTracking().Where(d => d.Id == item.CustomerMarketingOfficerId).FirstOrDefault();
                var marketingOfficerName = "";
                if (marketingOfficer is not null)
                {
                    marketingOfficerName = marketingOfficer.FirstName + " " + marketingOfficer.LastName;
                }
                customerTable.AddCell(pdfGenerator.GetTableCell(sl.ToString(), tableDataFont, Element.ALIGN_CENTER));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.Code, tableDataFont, Element.ALIGN_CENTER));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.Name, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.OwnersName, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.ContactNo, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.Email, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.Address, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.TradeLicense, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.ContactPersonName, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.ContactPersonContactNo, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(marketingOfficerName, tableDataFont, Element.ALIGN_LEFT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.CustomerCreditLimit.ToString(), tableDataFont, Element.ALIGN_RIGHT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.CustomerCreditDays, tableDataFont, Element.ALIGN_RIGHT));
                customerTable.AddCell(pdfGenerator.GetTableCell(item.CustomerTargetQuantity, tableDataFont, Element.ALIGN_RIGHT));
                if (item.BankAccounts is not null && item!.BankAccounts!.Any())
                {
                    PdfPTable bankDetailsTable = new(6);
                    float[] widthsCellsBankDetailsTable = new float[] { 6f, 20f, 17f, 15f, 17f, 25f };
                    bankDetailsTable.SetWidths(widthsCellsBankDetailsTable);
                    bankDetailsTable.WidthPercentage = 100;
                    headers = new List<string> { "SL", "Name", "Account No", "Routing No", "Bank Name", "Branch Name" };
                    foreach (var header in headers)
                    {
                        bankDetailsTable.AddCell(pdfGenerator.GetHeaderCell(header, tableHeaderFont, Element.ALIGN_CENTER));
                    }
                    var miniSL = 0;
                    foreach (var miniItem in item.BankAccounts!)
                    {
                        miniSL++;
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniSL.ToString(), tableDataFont, Element.ALIGN_RIGHT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.Name, tableDataFont, Element.ALIGN_LEFT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.AccNo, tableDataFont, Element.ALIGN_LEFT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.RoutingNo, tableDataFont, Element.ALIGN_LEFT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.BankName, tableDataFont, Element.ALIGN_LEFT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.BranchName, tableDataFont, Element.ALIGN_LEFT));

                    }
                    var bankDetailsCell1 = pdfGenerator.GetHeaderCell("Bank Details", tableHeaderFont, Element.ALIGN_CENTER);
                    bankDetailsCell1.Colspan = 4;
                    customerTable.AddCell(bankDetailsCell1);
                    var bankDetailsCell2 = pdfGenerator.GetTableCell("", tableDataFont, Element.ALIGN_LEFT);
                    bankDetailsCell2.Colspan = 10;
                    bankDetailsCell2.AddElement(bankDetailsTable);
                    customerTable.AddCell(bankDetailsCell2);
                }

            }

            pdfGenerator.AddTable(customerTable);
        }

        public void AddSupplierTable(PdfGenerator pdfGenerator, List<SupplierViewModel> supplierViewModels, Font tableHeaderFont, Font tableDataFont)
        {
            PdfPTable supplierTable = new(12) { WidthPercentage = 100, SpacingBefore = 10f, SpacingAfter = 10f };
            float[] widthsCellsSupplierTable = new float[] { 3f, 7f, 13f, 10f, 7f, 10f, 13f, 7f, 9f, 7f, 8f, 7f };
            supplierTable.SetWidths(widthsCellsSupplierTable);
            List<string> headers = new List<string> { "SL", "Code", "Name", "Owner's Name", "Contact No", "Email", "Address", "Trade License", "Contact Person Name", "Contact Person Contact No", "Contact Person Email", "Contact Person Designation" };
            foreach (var header in headers)
            {
                supplierTable.AddCell(pdfGenerator.GetHeaderCell(header, tableHeaderFont, Element.ALIGN_CENTER));
            }

            int sl = 0;
            supplierTable.HeaderRows = 1;
            foreach (var item in supplierViewModels)
            {
                sl++;
                supplierTable.AddCell(pdfGenerator.GetTableCell(sl.ToString(), tableDataFont, Element.ALIGN_CENTER));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.Code, tableDataFont, Element.ALIGN_CENTER));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.Name, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.OwnersName, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.ContactNo, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.Email, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.Address, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.TradeLicense, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.ContactPersonName, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.ContactPersonContactNo, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.ContactPersonEmail, tableDataFont, Element.ALIGN_LEFT));
                supplierTable.AddCell(pdfGenerator.GetTableCell(item.ContactPersonDesignation, tableDataFont, Element.ALIGN_LEFT));

                if (item.BankAccounts is not null && item!.BankAccounts!.Any())
                {
                    PdfPTable bankDetailsTable = new(6) { WidthPercentage = 100 };
                    float[] widthsCellsBankDetailsTable = new float[] { 6f, 20f, 17f, 15f, 17f, 25f };
                    bankDetailsTable.SetWidths(widthsCellsBankDetailsTable);
                    headers = new List<string> { "SL", "Name", "Account No", "Routing No", "Bank Name", "Branch Name" };
                    foreach (var header in headers)
                    {
                        bankDetailsTable.AddCell(pdfGenerator.GetHeaderCell(header, tableHeaderFont, Element.ALIGN_CENTER));
                    }
                    var miniSL = 0;
                    foreach (var miniItem in item.BankAccounts!)
                    {
                        miniSL++;
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniSL.ToString(), tableDataFont, Element.ALIGN_RIGHT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.Name, tableDataFont, Element.ALIGN_LEFT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.AccNo, tableDataFont, Element.ALIGN_LEFT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.RoutingNo, tableDataFont, Element.ALIGN_LEFT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.BankName, tableDataFont, Element.ALIGN_LEFT));
                        bankDetailsTable.AddCell(pdfGenerator.GetTableCell(miniItem.BranchName, tableDataFont, Element.ALIGN_LEFT));

                    }
                    var bankDetailsCell1 = pdfGenerator.GetHeaderCell("Bank Details", tableHeaderFont, Element.ALIGN_CENTER);
                    bankDetailsCell1.Colspan = 4;
                    supplierTable.AddCell(bankDetailsCell1);
                    var bankDetailsCell2 = pdfGenerator.GetTableCell("", tableDataFont, Element.ALIGN_LEFT);
                    bankDetailsCell2.Colspan = 8;
                    bankDetailsCell2.AddElement(bankDetailsTable);
                    supplierTable.AddCell(bankDetailsCell2);
                }

            }

            pdfGenerator.AddTable(supplierTable);
        }

        public async Task PrintCustomerWiseProductDiscountReportToPdf(MemoryStream stream, List<CustomerWiseProductDiscountViewModel> customerWiseProductDiscountViewModels, string reportTitle, CustomerWiseProductDiscountRequestModel request)
        {
            //ValidateParameters
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (customerWiseProductDiscountViewModels == null)
                throw new ArgumentNullException(nameof(customerWiseProductDiscountViewModels));

            //InitializeDocument
            Rectangle rectangle = new(PageSize.A4);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;

            document.Open();

            //Fonts
            Font fontArial8 = FontFactory.GetFont("Arial", 7);
            Font fontArial8Gray = FontFactory.GetFont("Arial", 7);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            //Add header table
            var headerTable = await AddCustomerWiseProductDiscountHeaderAsync(reportTitle, fontArial13Bold, fontArial14Bold, fontArial10, fontArial9, request);

            //Add data table
            var customerWiseProductDiscountData = CustomerWiseProductDiscountDataTable(customerWiseProductDiscountViewModels, fontArial8, fontArial8Bold, fontArial8Gray);

            document.Add(headerTable);
            document.Add(customerWiseProductDiscountData);

            document.Close();
        }

        public async Task<PdfPTable> AddCustomerWiseProductDiscountHeaderAsync(string reportTitle, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10, Font fontArial9, CustomerWiseProductDiscountRequestModel request)
        {
            //Company Information
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
                headerPage.AddCell(new PdfPCell(new Phrase("Date : " + request.FromDate.Value.AddDays(1).ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.AddDays(1).ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }

            if (request.CustomerMarketingOfficerId.HasValue)
            {
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value)?.FullName;
                headerPage.AddCell(new PdfPCell(new Phrase("Marketing Officer : " + marketingOfficerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }

            if (request.CustomerId.HasValue)
            {
                var customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Customer : " + customerName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }

            if (request.CustomerZoneId.HasValue)
            {
                var zoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Zone : " + zoneName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }

            if (request.CustomerAreaId.HasValue)
            {
                var areaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerAreaId.Value)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Area : " + areaName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });
            }

            headerPage.AddCell(new PdfPCell(new Phrase("Discount Status : " + (request.IsActive ? "Active" : "Inactive"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 0 });


            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial14Bold)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = 1 });
            return headerPage;
        }

        public PdfPTable CustomerWiseProductDiscountDataTable(List<CustomerWiseProductDiscountViewModel> customerWiseProductDiscountViewModels, Font fontArial8, Font fontArial8Bold, Font fontArial8Gray)
        {
            PdfPTable saleInvoiceData = new(4);
            float[] widthCellsHeaderPage = new float[] { 8f, 12f, 55f, 25f };
            saleInvoiceData.SetWidths(widthCellsHeaderPage);
            saleInvoiceData.WidthPercentage = 100;
            saleInvoiceData.HeaderRows = 1;

            saleInvoiceData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Customer", fontArial8Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            saleInvoiceData.AddCell(new PdfPCell(new Phrase("Applicable Date", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, BackgroundColor = BaseColor.LightGray });
            var sl = 0;

            foreach (var item in customerWiseProductDiscountViewModels.OrderBy(x => x.Customer?.Name))
            {
                sl++;
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.Customer?.Name, fontArial8)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                saleInvoiceData.AddCell(new PdfPCell(new Phrase(item?.ApplicableDate.ToString("dd/MM/yyy"), fontArial8)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });

                PdfPTable productDetailsTable = new(8);
                float[] widthsCellsProductDetailsTable = new float[] { 6f, 40f, 9f, 9f, 9f, 9f, 9f, 9f };
                productDetailsTable.SetWidths(widthsCellsProductDetailsTable);
                productDetailsTable.WidthPercentage = 100;
                productDetailsTable.SpacingAfter = 0;
                productDetailsTable.SpacingAfter = 0;
                productDetailsTable.AddCell(new PdfPCell(new Phrase("SL", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productDetailsTable.AddCell(new PdfPCell(new Phrase("Product", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productDetailsTable.AddCell(new PdfPCell(new Phrase("Invoice Discount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productDetailsTable.AddCell(new PdfPCell(new Phrase("Cash Discount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productDetailsTable.AddCell(new PdfPCell(new Phrase("Special Discount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productDetailsTable.AddCell(new PdfPCell(new Phrase("Monthly Discount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productDetailsTable.AddCell(new PdfPCell(new Phrase("Yearly Discount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                productDetailsTable.AddCell(new PdfPCell(new Phrase("Target Discount", fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                var miniSL = 0;
                foreach (var miniItem in item!.CustomerWiseProductDiscountDetails!.OrderBy(x => x.Product!.Name))
                {
                    miniSL++;
                    productDetailsTable.AddCell(new PdfPCell(new Phrase(miniSL.ToString(), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.Product?.Name, fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.InvoiceDiscount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.CashDiscount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.SpecialDiscount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.MonthlyDiscount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.YearlyDiscount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productDetailsTable.AddCell(new PdfPCell(new Phrase(miniItem?.TargetDiscount.ToString("#,##0.00"), fontArial8Gray)) { BorderColor = BaseColor.Gray, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                }

                saleInvoiceData.AddCell(new PdfPCell(new Phrase("Discount Details", fontArial8Gray)) { Colspan = 2, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                var productDetailsCell2 = GetPdfCell("", fontArial8);
                productDetailsCell2.Colspan = 2;
                productDetailsCell2.Padding = 0;
                productDetailsCell2.AddElement(productDetailsTable);
                saleInvoiceData.AddCell(productDetailsCell2);
            }

            return saleInvoiceData;
        }

        public async Task<PdfPTable> AddHeaderAsync(string reportTitleName, Font fontArial13Bold, Font fontArial14Bold, Font fontArial10)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);

            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Name, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(tenantData?.Address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase($"{tenantData?.ContactNo}, Email : {tenantData?.Email}", fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitleName, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = 1 });

            return headerPage;
        }

        public PdfPTable FilteringDataTable(Font fontArial9, ProductRequestModel request)
        {
            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 3f });

            headerPage.AddCell(new PdfPCell(new Phrase("Date: " + DateTime.Now.ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_RIGHT });
            if (request.InventoryTypeId.HasValue)
            {
                var inventoryTypeName = _unitOfWork.Repository<InventoryType>().TableNoTracking().FirstOrDefault(x => x.Id == request.InventoryTypeId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Inventory Type : " + inventoryTypeName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (request.CategoryId.HasValue)
            {
                var categoryName = _unitOfWork.Repository<Category>().TableNoTracking().FirstOrDefault(x => x.Id == request.CategoryId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Category : " + categoryName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (request.IsPurchaseProduct)
            {
                //var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == request.ProductId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Item For : " + "Purchase", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            if (request.IsSaleProduct)
            {
                //var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == request.ProductId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Item For : " + "Sale", fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 3f });

            return headerPage;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductPriceListReportToPdf(MemoryStream stream, List<ProductViewModel> list, string headerText, ProductRequestModel request)
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
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial7Bold = FontFactory.GetFont("Arial", 7, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = FilteringDataTable(fontArial9, request);
            var dataTable = PrimaryProductPriceListDataTable(list, fontArial7, fontArial7Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }

        public PdfPTable PrimaryProductPriceListDataTable(List<ProductViewModel> list, Font fontArial7, Font fontArial7Bold)
        {
            var productData = list.GroupBy(x => new { x.Category!.Name }).ToList();
            //var productData = list;

            PdfPTable salesItemWiseCustomerData = new(6);
            float[] widthsCellsHeaderPage = new float[] { 5f, 12f, 8f, 6f, 6f, 6f };
            salesItemWiseCustomerData.SetWidths(widthsCellsHeaderPage);
            salesItemWiseCustomerData.WidthPercentage = 100;
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("SL", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BorderWidthBottom = 0 });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Product Name", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Carton Size", fontArial7Bold)) { Rowspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("Unit Price (Tk)", fontArial7Bold)) { Colspan = 2, PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("TP", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("MRP", fontArial7Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            salesItemWiseCustomerData.HeaderRows = 2;

            foreach (var product in productData)
            {
                var sl = 0;
                salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(product.Key.Name, fontArial7Bold)) { Colspan = 6, HorizontalAlignment = 1, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var item in product)
                {
                    sl++;
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item.PackSize!.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase("", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item.SalePrice.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                    salesItemWiseCustomerData.AddCell(new PdfPCell(new Phrase(item.Mrp.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });

                }
            }
            return salesItemWiseCustomerData;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductAuditReportToPdfAsync(MemoryStream stream, List<ProductAuditViewModel> list, string headerText, ProductAuditRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            Rectangle rectangle = new(PageSize.A0);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // Fonts
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial9Bold = FontFactory.GetFont("Arial", 9, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = AddFilteringDataTable(fontArial9, request);
            var dataTable = ProductChangeHistoryDataTable(list, fontArial9, fontArial9Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();

            return (filterTable, dataTable);
        }

        public PdfPTable ProductChangeHistoryDataTable(List<ProductAuditViewModel> list, Font fontArial9, Font fontArial9Bold)
        {
            PdfPTable ProductChangeHistoryData = new(25);
            float[] widthsCellsHeaderPage = new float[] { 5, 9f, 8f, 6f, 22f, 8f, 10, 15f, 10f, 30f, 50f, 15f, 10, 10f, 10f, 8f, 8f, 8f, 10, 10f, 9f, 9f, 15f, 15f, 10f };
            ProductChangeHistoryData.SetWidths(widthsCellsHeaderPage);
            ProductChangeHistoryData.WidthPercentage = 100;
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("SL", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Action date", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Action Type", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Code", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Product", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Measurement Unit", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Inventory Type", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Product Type", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Generic", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Composition", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Category", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Country", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Is Purchase Product", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Is Sale Product", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("MRP", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Trade Price", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Purchase Price", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Last Purchase Price", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Vat Percentage", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Created On", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Updated On", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Created By", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Updated By", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
            ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase("Is Deleted", fontArial9Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

            ProductChangeHistoryData.HeaderRows = 1;
            var sl = 0;
            foreach (var item in list)
            {
                sl++;
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.ActionDate?.ToString("dd/MM/yyyy"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.ActionType, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.PackSizeName, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.MeasurementUnitName, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.InventoryTypeName, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.ProductTypeName, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.GenericName, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.Composition, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.CategoryName, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.CountryName, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.IsPurchaseProduct ? "Yes" : "No", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.IsSaleProduct ? "Yes" : "No", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.Mrp.ToString(), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.SalePrice.ToString(), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.PurchasePrice.ToString(), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.LastPurchaseRate.ToString(), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.VatPercentage.ToString(), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_RIGHT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.CreatedOn.ToString("dd/MM/yyyy"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.UpdatedOn.ToString("dd/MM/yyyy"), fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.CreatedBy, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.UpdatedBy, fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_LEFT });
                ProductChangeHistoryData.AddCell(new PdfPCell(new Phrase(item.DeletedHistory ? "Yes" : "No", fontArial9)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = Element.ALIGN_CENTER });
            }
            return ProductChangeHistoryData;
        }

        public PdfPTable AddFilteringDataTable(Font fontArial9, ProductAuditRequestModel request)
        {
            PdfPTable headerPage = new(1);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 3f });
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                headerPage.AddCell(new PdfPCell(new Phrase("Date: " + request.FromDate.Value.ToString("dd/MM/yyyy") + " to " + request.ToDate.Value.ToString("dd/MM/yyyy"), fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 4f, HorizontalAlignment = 0 });
            }
            if (request.ProductId.HasValue)
            {
                var productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == request.ProductId)?.Name;
                headerPage.AddCell(new PdfPCell(new Phrase("Product : " + productName, fontArial9)) { Border = 0, PaddingTop = 2f, PaddingBottom = 2f, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            headerPage.AddCell(new PdfPCell(new Phrase("", fontArial9)) { Border = 0, PaddingTop = 3f, PaddingBottom = 3f });

            return headerPage;
        }

        public async Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductListReportToPdf(MemoryStream stream, List<ProductViewModel> itemList, string headerText, ProductRequestModel request)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (itemList == null)
                throw new ArgumentNullException(nameof(itemList));

            Rectangle rectangle = _industry.Reports.CompactLayout ? new(PageSize.A4) : new(PageSize.A4.Height, PageSize.A4.Width);
            Document document = new(rectangle, 72, 72, 72, 72);
            document.SetMargins(20f, 20f, 20f, 20f);
            var pdfWriter = PdfWriter.GetInstance(document, stream);
            pdfWriter.CloseStream = false;
            document.Open();

            // =====
            // Fonts
            // =====
            Font fontArial7 = FontFactory.GetFont("Arial", 7);
            Font fontArial8Bold = FontFactory.GetFont("Arial", 8, Font.BOLD);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial9 = FontFactory.GetFont("Arial", 9);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);

            var headerTable = await AddHeaderAsync(headerText, fontArial13Bold, fontArial14Bold, fontArial10);
            var filterTable = FilteringDataTable(fontArial9, request);
            var dataTable = PrimaryProductListDataTable(itemList, fontArial7, fontArial8Bold);
            document.Add(headerTable);
            document.Add(filterTable);
            document.Add(dataTable);

            document.Close();
            return (filterTable, dataTable);
        }
        public PdfPTable PrimaryProductListDataTable(List<ProductViewModel> list, Font fontArial7, Font fontArial8Bold)
        {
            if (_industry.Reports.CompactLayout)
            {
                PdfPTable productListData = new(13);
                float[] widthsCellsHeaderPage = new float[] { 4f, 5f, 18f, 12f, 12f, 6f, 5f, 6f, 6f, 6f, 6f, 4f, 4f };
                productListData.SetWidths(widthsCellsHeaderPage);
                productListData.WidthPercentage = 100;
                productListData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BorderWidthBottom = 0 });
                productListData.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Product Type", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Inventory Type", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Bag Weight", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Pur. Price", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("MRP", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Alert Qty", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("For Pur.", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("For Sale", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

                productListData.HeaderRows = 1;
                var sl = 0;
                foreach (var item in list)
                {
                    sl++;
                    productListData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.ProductType?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.InventoryType?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase((item.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid()) ? item.BagWeight.ToString() : "N/A", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.MeasurementUnit?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.PurchasePrice.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.SalePrice.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Mrp.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.AlertQuantity.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.IsPurchaseProduct ? "Yes" : "No", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.IsSaleProduct ? "Yes" : "No", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                }
                return productListData;
            }
            else
            {
                PdfPTable productListData = new(16);
                float[] widthsCellsHeaderPage = new float[] { 4f, 5f, 18f, 15f, 16f, 11f, 9f, 7f, 6f, 5f, 6f, 5f, 5f, 5f, 4f, 4f };
                productListData.SetWidths(widthsCellsHeaderPage);
                productListData.WidthPercentage = 100;
                productListData.AddCell(new PdfPCell(new Phrase("SL", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE, BorderWidthBottom = 0 });
                productListData.AddCell(new PdfPCell(new Phrase("Code", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Name", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Category", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Generic", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Product Type", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Inventory Type", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Country", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Pack Size", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Unit", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Pur. Price", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("TP", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("MRP", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("Alert Qty", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("For Pur.", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });
                productListData.AddCell(new PdfPCell(new Phrase("For Sale", fontArial8Bold)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1, VerticalAlignment = Element.ALIGN_MIDDLE });

                productListData.HeaderRows = 1;
                var sl = 0;
                foreach (var item in list)
                {
                    sl++;
                    productListData.AddCell(new PdfPCell(new Phrase(sl.ToString(), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Code, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Generic?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Category?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.ProductType?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.InventoryType?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Country?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 0 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.PackSize?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.MeasurementUnit?.Name, fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.PurchasePrice.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.SalePrice.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.Mrp.ToString("#,##0.00"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.AlertQuantity.ToString("#,##0"), fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 2 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.IsPurchaseProduct ? "Yes" : "No", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                    productListData.AddCell(new PdfPCell(new Phrase(item.IsSaleProduct ? "Yes" : "No", fontArial7)) { PaddingTop = 3, PaddingBottom = 3, HorizontalAlignment = 1 });
                }
                return productListData;
            }
        }
    }
}
