
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Accounts.Pdf;
using Application.Services.Services.Accounts.PdfAccount;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Accounts;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly string cacheKey_accounts = "accounts";
        private readonly string cacheKey_ParentAccounts = "parent-accounts";
        private readonly string cacheKey_ControlAccounts = "control-accounts";
        private readonly string cacheKey_ControlAccountsExcludingCustomers = "control-accounts-excluding-customers";
        private readonly string cacheKey_ControlAccountsExcludingSuppliers = "control-accounts-excluding-suppliers";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountService _accountService;
        private readonly IAccountPdfService _accountPdfService;
        private readonly IAccountReportPdfService _accountReportPdfService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        private readonly ITenantService _tenantService;

        public AccountController(IAccountService accountService,
           IWorkContext workContext,
           IUnitOfWork unitOfWork, IMapper mapper, IAccountPdfService accountPdfService, IAccountReportPdfService accountReportPdfService,
           IConfigurationPdfService configurationPdfService, ICacheService cacheService, ITenantService tenantService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _accountService = accountService;
            _mapper = mapper;
            _accountPdfService = accountPdfService;
            _accountReportPdfService = accountReportPdfService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
            _tenantService = tenantService;
        }

        //crud api
        [Authorize(Permissions.Accounts.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<AccountViewModel>, int>>> Search(AccountRequestModel request)
        {
            return await Result<Tuple<List<AccountViewModel>, int>>.SuccessAsync(await _accountService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Accounts.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<AccountViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey_accounts, out Tuple<List<AccountViewModel>, int> cachedList))
            {
                cachedList = await _accountService.SearchAsync(new AccountRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey_accounts, cachedList);
            }
            return await Result<Tuple<List<AccountViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }

        [Authorize(Permissions.Accounts.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] AccountCreationDto accountCreationDto)
        {
            if (await _accountService.AccountExists(accountCreationDto.Name ?? "")) return await Result<string>.FailAsync("", "Account's Name Already Exists.");
            var accountId = await _accountService.AddAsync(accountCreationDto);
            _cacheService.Remove(cacheKey_accounts);
            _cacheService.Remove(cacheKey_ParentAccounts);
            _cacheService.Remove(cacheKey_ControlAccounts);
            _cacheService.Remove(cacheKey_ControlAccountsExcludingCustomers);
            _cacheService.Remove(cacheKey_ControlAccountsExcludingSuppliers);
            return await Result<Guid>.SuccessAsync(accountId, "Account Added Successfully");

        }

        [Authorize(Permissions.Accounts.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] AccountUpdateDto accountUpdateDto)
        {
            var accountId = await _accountService.UpdateAsync(accountUpdateDto);
            _cacheService.Remove(cacheKey_accounts);
            _cacheService.Remove(cacheKey_ParentAccounts);
            _cacheService.Remove(cacheKey_ControlAccounts);
            _cacheService.Remove(cacheKey_ControlAccountsExcludingCustomers);
            _cacheService.Remove(cacheKey_ControlAccountsExcludingSuppliers);
            return await Result<Guid>.SuccessAsync(accountId, "Account Updated Successfully");


        }

        [Authorize(Permissions.Accounts.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var accountId = await _accountService.DeleteAsync(id);
            _cacheService.Remove(cacheKey_accounts);
            _cacheService.Remove(cacheKey_ParentAccounts);
            _cacheService.Remove(cacheKey_ControlAccounts);
            _cacheService.Remove(cacheKey_ControlAccountsExcludingCustomers);
            _cacheService.Remove(cacheKey_ControlAccountsExcludingSuppliers);
            return await Result<Guid>.SuccessAsync(accountId, "Account Deleted Successfully");
        }

        //others api
        [Authorize(Permissions.Accounts.View)]
        [HttpGet("chart-of-accounts")]
        public async Task<IActionResult> ChartOfAccount(Guid? rootAccountHeadId = null, int level = 0, int levelsToLoad = 8)
        {
            var accountHeadsList = await _unitOfWork.Repository<Account>().TableNoTracking().Include(x => x.AccountType).ToListAsync();
            var result = await _accountService.PrepareChartOfAccount(accountHeadsList, rootAccountHeadId, level, levelsToLoad);
            return Ok(result.OrderBy(x => x.Code));
        }

        [HttpGet("parent-accounts")]
        public async Task<Result> GetParentAccount()
        {
            if (!_cacheService.TryGet(cacheKey_ParentAccounts, out IList<AccountDropdownDto> cachedList))
            {
                cachedList = await _accountService.PrepareAllParentAccounts();
                _cacheService.Set(cacheKey_ParentAccounts, cachedList);
            }
            return await Result<IList<AccountDropdownDto>>.SuccessAsync(cachedList, "Parent Accounts Found");
        }

        [HttpGet("control-accounts")]
        public async Task<Result> GetControlAccounts()
        {
            if (!_cacheService.TryGet(cacheKey_ControlAccounts, out IList<AccountDropdownDto> cachedList))
            {
                cachedList = await _accountService.PrepareAllControlAccounts();
                _cacheService.Set(cacheKey_ControlAccounts, cachedList);
            }
            return await Result<IList<AccountDropdownDto>>.SuccessAsync(cachedList, "Control Accounts Found");
        }

        [HttpGet("control-accounts-excluding-customers")]
        public async Task<Result> GetControlAccountsExcludingCustomers()
        {
            if (!_cacheService.TryGet(cacheKey_ControlAccountsExcludingCustomers, out IList<AccountDropdownDto> cachedList))
            {
                cachedList = await _accountService.PrepareAllControlAccountsExcludingCustomers();
                _cacheService.Set(cacheKey_ControlAccountsExcludingCustomers, cachedList);
            }
            return await Result<IList<AccountDropdownDto>>.SuccessAsync(cachedList, "Control Accounts Excluding Customers Found");
        }

        [HttpGet("control-accounts-excluding-suppliers")]
        public async Task<Result> GetControlAccountsExcludingSuppliers()
        {
            if (!_cacheService.TryGet(cacheKey_ControlAccountsExcludingSuppliers, out IList<AccountDropdownDto> cachedList))
            {
                cachedList = await _accountService.PrepareAllControlAccountsExcludingSuppliers();
                _cacheService.Set(cacheKey_ControlAccountsExcludingSuppliers, cachedList);
            }
            return await Result<IList<AccountDropdownDto>>.SuccessAsync(cachedList, "Control Accounts Excluding Suppliers Found");
        }

        [HttpGet("getControlAccounts/{parentId}")]
        public async Task<Result> GetAllControlAccountsByParentId(Guid parentId)
        {
            var cacheKey = "for-web-user-" + parentId;
            if (!_cacheService.TryGet(cacheKey, out IList<ControlAccount> cachedList))
            {
                cachedList = await _accountService.GetAllControlAccountsByAccountId(parentId);
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<IList<ControlAccount>>.SuccessAsync(cachedList, "Accounts Found");
        }

        [HttpGet("getControlAccountsForAppUser/{parentId}")]
        public async Task<Result> GetAllControlAccountsByParentIdForAppUser(Guid parentId)
        {
            var cacheKey = "for-app-user-" + parentId;
            if (!_cacheService.TryGet(cacheKey, out IList<ControlAccount> cachedList))
            {
                cachedList = await _accountService.GetAllControlAccountsByAccountIdForAppUser(parentId);
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<IList<ControlAccount>>.SuccessAsync(cachedList, "Accounts Found");
        }

        [Authorize(Permissions.AccountsModuleReports.General_Ledger)]
        [HttpPost]
        [Route("subsidiary-ledger-print")]
        public virtual async Task<IActionResult> SubsidiaryLedgerReportPrint(SubsidiaryLedgerRequestModel requestModel)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var headerText = "";
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                    headerText = "Account Statement for the period of " + requestModel.FromDate.Value.ToString("dd/MM/yyyy") + " to " + requestModel.ToDate.Value.ToString("dd/MM/yyyy");
                }
                else headerText = "General Ledger";
                var account = await _unitOfWork.Repository<Account>().FindAsync(requestModel.AccountId);
                var userName = _workContext.GetUserName() ?? "";
                var list = await _accountService.SubsidiaryLedger(requestModel.AccountId, requestModel.FromDate, requestModel.ToDate);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    //_accountPdfService.PrintSubsidiaryLedgerReportToPdf(stream, list, "Subsidiary Ledger(" + account?.Name + ")", requestModel.FromDate, requestModel.ToDate);
                    var tables = await _accountReportPdfService.PrintSubsidiaryLedgerReportToPdfAsync(stream, list, headerText, account);

                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.PurchaseModuleReports.Supplier_Ledger_Product_Wise)]
        [HttpPost]
        [Route("supplier-ledger-product-wise-print")]
        public virtual async Task<IActionResult> SupplierLedgerProductWiseReportPrint(SubsidiaryLedgerRequestModel requestModel)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                var account = await _unitOfWork.Repository<Account>().FindAsync(requestModel.AccountId);
                var costCenterName = "ALL";
                if (requestModel.CostCenterId.HasValue)
                {
                    var costCenter = await _unitOfWork.Repository<CostCenter>().FindAsync(requestModel.CostCenterId.Value);
                    costCenterName = costCenter.Name;
                }
                var userName = _workContext.GetUserName() ?? "";
                var list = await _accountService.SupplierLedgerProductWise(requestModel.AccountId, requestModel.CostCenterId, requestModel.FromDate, requestModel.ToDate);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var reportTitle = "Supplier Ledger : Product Wise";
                    var table = await _accountReportPdfService.SupplierLedgerProductWiseReportToPdfAsync(stream, list, reportTitle, account, costCenterName, requestModel.FromDate, requestModel.ToDate);
                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                            requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, reportTitle);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);

                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.SalesModuleReports.Customer_Ledger_Product_Wise)]
        [HttpPost]
        [Route("customer-ledger-product-wise-print")]
        public virtual async Task<IActionResult> CustomerLedgerProductWiseReportPrint(SubsidiaryLedgerRequestModel requestModel)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                var account = await _unitOfWork.Repository<Account>().FindAsync(requestModel.AccountId);
                var costCenterName = "ALL";
                if (requestModel.CostCenterId.HasValue)
                {
                    var costCenter = await _unitOfWork.Repository<CostCenter>().FindAsync(requestModel.CostCenterId.Value);
                    costCenterName = costCenter.Name;
                }
                var userName = _workContext.GetUserName() ?? "";
                var list = await _accountService.CustomerLedgerProductWise(requestModel.AccountId, requestModel.CostCenterId, requestModel.FromDate, requestModel.ToDate);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Customer Ledger : Product Wise";
                    var tables = await _accountReportPdfService.PrintCustomerLedgerProductWiseReportToPdfAsync(stream, list, account, headerText, costCenterName, requestModel.FromDate, requestModel.ToDate);

                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                                requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.AccountsModuleReports.Account_Ledger_Day_Wise)]
        [HttpPost]
        [Route("day-wise-account-ledger-print")]
        public virtual async Task<IActionResult> DayWiseAccountLedgerReportPrint(SubsidiaryLedgerRequestModel requestModel)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                var account = await _unitOfWork.Repository<Account>().FindAsync(requestModel.AccountId);
                var costCenterName = "ALL";
                if (requestModel.CostCenterId.HasValue)
                {
                    var costCenter = await _unitOfWork.Repository<CostCenter>().FindAsync(requestModel.CostCenterId.Value);
                    costCenterName = costCenter.Name;
                }
                var userName = _workContext.GetUserName() ?? "";
                var list = await _accountService.SubsidiaryLedger(requestModel.AccountId, requestModel.FromDate, requestModel.ToDate, requestModel.CostCenterId);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Account Statement for the period of " + requestModel.FromDate!.Value.ToString("dd/MM/yyyy") + " to " + requestModel.ToDate!.Value.ToString("dd/MM/yyyy") + ",   Cost Center: " + costCenterName;
                    var tables = await _accountReportPdfService.PrintDayWiseAccountLedgerReportToPdfAsync(stream, list, account, costCenterName, requestModel.FromDate, requestModel.ToDate);

                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.AccountsModuleReports.Financial_Statement)]
        [HttpPost]
        [Route("trial-balance-print")]
        public virtual async Task<IActionResult> TrialBalanceReportPrint(AccountReportRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                var userName = _workContext.GetUserName() ?? "";
                var list = await _accountService.TrialBalanceReportPrint(requestModel);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _accountPdfService.PrintTrailBalanceReportToPdfAsync(stream, list, "Trial Balance ", requestModel.FromDate, requestModel.ToDate);
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                }
                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.AccountsModuleReports.Financial_Statement)]
        [HttpPost("balance-sheet-print")]
        public async Task<IActionResult> PrintBalanceSheetReport(AccountReportRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.BalanceSheet(requestModel.Level, requestModel.LevelsToLoad, requestModel.FromDate, requestModel.ToDate, requestModel.FinancialYearId);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _accountPdfService.PrintBalanceSheetReportToPdfAsync(stream, result, "Balance Sheet", requestModel.FromDate, requestModel.ToDate);
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                }
                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }

        [Authorize(Permissions.AccountsModuleReports.Financial_Statement)]
        [HttpPost("income-statement-print")]
        public async Task<IActionResult> PrintIncomeStatementReport(AccountReportRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                var userName = _workContext.GetUserName() ?? "";
                var isHeads = await _accountService.IncomeStatement(requestModel.Level, requestModel.LevelsToLoad, requestModel.FromDate, requestModel.ToDate, requestModel.FinancialYearId);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _accountPdfService.PrintIncomeStatementReportToPdfAsync(stream, isHeads, "Income Statement ", requestModel.FromDate, requestModel.ToDate);
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(AccountRequestModel request)
        {
            try
            {
                request.Page = -1;
                var userName = _workContext.GetUserName() ?? "";
                var list = await _accountService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name", "Account Type", "Level", "Parent Name", "Controll Account" };
                List<float> columnWidths = new List<float> { 10f, 30f, 20f, 8f, 20f, 12f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var account in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        account?.Name,
                        account?.AccountTypeName,
                        account?.Level,
                        ParrentName = account?.ParentName ?? "",
                        ControllAccount = account!.IsControlAccount ? "Yes" : "No"

                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Account List");
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.AccountsModuleReports.Supplier_Transaction_Report)]
        [HttpPost("supplier-transaction-ledger-print")]
        public async Task<IActionResult> PrintSupplierTransactionLedgerReport(AccountReportRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                int numberOfColumns = 10;
                List<TableHeader> headers = new List<TableHeader>
            {
                new TableHeader { Text = "SL", ColSpan = 1, RowSpan = 3 },
                new TableHeader { Text = "Supplier", ColSpan = 2, RowSpan = 1 },
                new TableHeader { Text = "Previous Due", ColSpan = 1, RowSpan = 3 },
                new TableHeader { Text = "Transaction", ColSpan = 4, RowSpan = 1 },
                new TableHeader { Text = "Balance", ColSpan = 2, RowSpan = 1 },
                new TableHeader { Text = "Code", ColSpan = 1, RowSpan = 2 },
                new TableHeader { Text = "Name", ColSpan = 1, RowSpan = 2 },
                new TableHeader { Text = "Net Purchase", ColSpan = 2, RowSpan = 1 },
                new TableHeader { Text = "Payment", ColSpan = 2, RowSpan = 1 },
                new TableHeader { Text = "Due", ColSpan = 1, RowSpan = 2 },
                new TableHeader { Text = "Advance", ColSpan = 1, RowSpan = 2},
                new TableHeader { Text = "Qty", ColSpan = 1, RowSpan = 1 },
                new TableHeader { Text = "Amount", ColSpan = 1, RowSpan = 1 },
                new TableHeader { Text = "Paid", ColSpan = 1, RowSpan = 1},
                new TableHeader { Text = "Adjustment", ColSpan = 1, RowSpan = 1}
            };
                float[] columnWidths = new float[] { 4f, 8f, 29f, 9f, 9f, 9f, 9f, 9f, 9f, 9f };
                List<Func<SupplierTransactionLedgerViewModel, decimal>> sumProperties = new List<Func<SupplierTransactionLedgerViewModel, decimal>>
                    {
                        x => x.OpeningDue,
                        x => x.ThisPeriodPurchaseQty,
                        x => x.ThisPeriodPurchaseValue,
                        x => x.ThisPeriodPayment,
                        x => x.ThisPeriodAdjustment,
                        x => x.Due,
                        x => x.Advance
                    };
                var result = await _accountService.PrepareSupplierTransactionLedger(requestModel.FromDate, requestModel.ToDate);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Supplier Transaction : Report";
                    ///var tables = await _accountPdfService.PrintSupplierTransactionLedgerReportToPdfAsync(stream, result, headerText, requestModel.FromDate, requestModel.ToDate);
                    var tables = await _accountPdfService.PrintReportToPdfAsync<SupplierTransactionLedgerViewModel>(numberOfColumns, stream, headers, columnWidths.ToList(), result, headerText, requestModel.FromDate!.Value.AddDays(-1), requestModel.ToDate, sumProperties, true);
                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }

            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }

        [Authorize(Permissions.AccountsModuleReports.Customer_Transaction_Report)]
        [HttpPost("customer-transaction-ledger-print")]
        public async Task<IActionResult> PrintCustomerTransactionLedgerReport(CustomerTransactionRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCustomerTransactionLedger(requestModel);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Customer Transaction : Report";
                    var tables = await _accountPdfService.PrintCustomerTransactionLedgerReportToPdfAsync(stream, result, headerText, requestModel);
                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Customer_Wise_Sales_And_Collection_Report)]
        [HttpPost]
        [Route("primary-customer-wise-sales-and-collection-report-print")]
        public async Task<IActionResult> PrintCustomerWiseSalesAndCollectionReport(SalesAndCollectionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCustomerWiseSalesAndCollection(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Customer Wise Sales & Collection : Report";
                    var tables = await _accountPdfService.PrintCustomerWiseSalesAndCollectionReportToPdfAsync(stream, result, headerText, request);
                    if (request.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_National_Wise_Sales_Collection_And_Due_Report)]
        [HttpPost]
        [Route("primary-national-wise-sales-collection-and-due-report-print")]
        public async Task<IActionResult> PrintNationalWiseSalesCollectionAndDueReport(SalesAndCollectionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCustomerWiseSalesAndCollection(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "National Wise Sales, Collection & Due : Report";
                    var tables = await _accountPdfService.PrintNationalWiseSalesCollectionAndDueReportToPdfAsync(stream, result, headerText, request);
                    if (request.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }


        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Report)]
        [HttpPost]
        [Route("primary-marketing-officer-wise-sales-collection-and-due-report-print")]
        public async Task<IActionResult> PrintMarketingOfficerWiseSalesCollectionAndDueReport(SalesAndCollectionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }

                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCustomerWiseSalesAndCollection(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "MO Wise Sales, Collection & Due : Report";
                    var tables = await _accountPdfService.PrintMarketingOfficerWiseSalesCollectionAndDueReportToPdfAsync(stream, result, headerText, request);
                    if (request.ReportType == 2)
                    {
                        Guid? tenantId = _workContext.GetTenantId();
                        var tenantData = await _tenantService.GetByIdAsync(tenantId);
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Details_Report)]
        [HttpPost]
        [Route("primary-marketing-officer-wise-sales-collection-and-due-details-report-print")]
        public async Task<IActionResult> PrintMarketingOfficerWiseSalesCollectionAndDueDetailsReport(SalesAndCollectionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }

                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCustomerWiseSalesAndCollection(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "MO Wise Sales, Collection & Due : Report (Customer Wise)";
                    var tables = await _accountPdfService.PrintMarketingOfficerWiseSalesCollectionAndDueDetailsReportToPdfAsync(stream, result, headerText, request);
                    if (request.ReportType == 2)
                    {
                        Guid? tenantId = _workContext.GetTenantId();
                        var tenantData = await _tenantService.GetByIdAsync(tenantId);
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Report_Short)]
        [HttpPost]
        [Route("primary-marketing-officer-wise-sales-collection-and-due-report-short-print")]
        public async Task<IActionResult> PrintMarketingOfficerWiseSalesCollectionAndDueReportShort(SalesAndCollectionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }

                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCustomerWiseSalesAndCollection(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "MO Wise Sales, Collection & Due : Report (Short)";
                    var tables = await _accountPdfService.PrintMarketingOfficerWiseSalesCollectionAndDueReportShortToPdfAsync(stream, result, headerText, request);
                    if (request.ReportType == 2)
                    {
                        Guid? tenantId = _workContext.GetTenantId();
                        var tenantData = await _tenantService.GetByIdAsync(tenantId);
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Month_Wise_Sales_Collection_And_Due_Report)]
        [HttpPost]
        [Route("primary-month-wise-sales-collection-and-due-report-print")]
        public async Task<IActionResult> PrintMonthWiseSalesCollectionAndDueReport(MarketingOfficerYearlySalesAndCollectionRequestModel request)
        {
            try
            {
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareYearlyCustomerWiseSalesAndCollection(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Month Wise Sales, Collection & Due : Report";
                    var tables = await _accountPdfService.PrintMonthlySalesCollectionAndDueReportToPdfAsync(stream, result, headerText, request);
                    if (request.ReportType == 2)
                    {
                        Guid? tenantId = _workContext.GetTenantId();
                        var tenantData = await _tenantService.GetByIdAsync(tenantId);
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1", null, null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }


        [Authorize(Permissions.AccountsModuleReports.Daily_Transaction_Report)]
        [HttpPost("cash-bank-transaction-ledger-print")]
        public async Task<IActionResult> PrintCashBankTransactionLedgerReport(AccountReportRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCashBankTransactionLedger(requestModel.FromDate, requestModel.ToDate);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Daily Transaction : Report";
                    var tables = await _accountPdfService.PrintDailyTransactionLedgerReportToPdfAsync(stream, result, headerText, requestModel.FromDate, requestModel.ToDate);
                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }

        [Authorize(Permissions.AccountsModuleReports.Cash_Bank_Balance_Report)]
        [HttpPost("cash-bank-balance-print")]
        public async Task<IActionResult> PrintCashBankBalanceReport(AccountReportRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCashBankTransactionLedger(requestModel.FromDate, requestModel.ToDate);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Cash & Bank Balance : Report";
                    var tables = await _accountPdfService.PrintCashBankBalanceReportToPdfAsync(stream, result, headerText, requestModel.FromDate, requestModel.ToDate);
                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }

        [Authorize(Permissions.AccountsModuleReports.Daily_Transaction_Detail_Report)]
        [HttpPost("cash-bank-transaction-detail-ledger-print")]
        public async Task<IActionResult> PrintCashBankTransactionDetailLedgerReport(AccountReportRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCashBankTransactionDetailLedger(requestModel.FromDate, requestModel.ToDate);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _accountPdfService.PrintDailyTransactionDetailLedgerReportToPdfAsync(stream, result, "Daily Transaction Detail : Report", requestModel.FromDate, requestModel.ToDate);
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }

        [Authorize(Permissions.AccountsModuleReports.Cash_Book_Report)]
        [HttpPost("cash-book-transaction-detail-ledger-print")]
        public async Task<IActionResult> PrintCashBookTransactionDetailLedgerReport(CashBookReportRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }

                var account = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == requestModel.CashBookAccountId);
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _accountService.PrepareCashBookTransactionDetailLedger(requestModel);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Cash Book";
                    var tables = await _accountPdfService.PrintCashBookTransactionDetailLedgerReportToPdfAsync(stream, result, headerText, requestModel);
                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }

        }
    }
}
