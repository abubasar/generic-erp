using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Accounts.Pdf;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Report.Stocks;
using Application.Services.ViewModels.Accounts.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CogsController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly IAccountService _accountService;
        private readonly IAccountPdfService _accountPdfService;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;
        public CogsController(IStockService stockService, IAccountService accountService, IAccountPdfService accountPdfService, IWorkContext workContext, IUnitOfWork unitOfWork, ITenantService tenantService)
        {
            _stockService = stockService;
            _accountService = accountService;
            _accountPdfService = accountPdfService;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.AccountsModuleReports.CogsCalculation_Report)]
        [HttpPost("cogs-calculation-print")]
        public async Task<IActionResult> PrintCogsCalculationReport(CogsCalculationRequestModel request)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }

                var financialYearId = _unitOfWork.Repository<FinancialYear>().TableNoTracking().First(x => x.Id == request.FinancialYearId).Id;

                //request.InventoryTypeId = InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid();
                //var rawMaterials = await _stockService.PrepareStockLedgerTest(request);
                //var rmOpeningValue = rawMaterials.Sum(x => x.OpeningValue);
                //var rmClosingValue = rawMaterials.Sum(x => x.ClosingValue);
                request.AccountId = AccountHeadConstants.RawMaterialsInventory.ToGuid();
                var rawMaterialsInventoryAccount = await _accountService.SubsidiaryLedger(request.AccountId, request.FromDate, request.ToDate);
                var rmOpeningValue = rawMaterialsInventoryAccount.Select(x => x.Balance).First();
                var rmClosingValue = rawMaterialsInventoryAccount.Select(x => x.Balance).Last();

                request.AccountId = AccountHeadConstants.FinishedGoodsInventory.ToGuid();
                var finishedGoods = await _accountService.SubsidiaryLedger(request.AccountId, request.FromDate, request.ToDate);
                var fgOpeningValue = finishedGoods.Select(x => x.Balance).First();
                var fgClosingValue = finishedGoods.Select(x => x.Balance).Last();
                //request.InventoryTypeId = InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid();
                //var finishedGoods = await _stockService.PrepareStockLedgerTest(request);
                //var fgOpeningValue = finishedGoods.Sum(x => x.OpeningValue);
                //var fgClosingValue = finishedGoods.Sum(x => x.ClosingValue);

                request.AccountId = AccountHeadConstants.PurchaseClearingAccount.ToGuid();
                var purchaseClearingAccount = await _accountService.SubsidiaryLedger(request.AccountId, request.FromDate, request.ToDate);
                var totalCreditInPurchaseClearingAccount = purchaseClearingAccount.Sum(x => x.Credit);

                request.AccountId = AccountHeadConstants.LCCostClearingAccount.ToGuid();
                var lcCostClearingAccount = await _accountService.SubsidiaryLedger(request.AccountId, request.FromDate, request.ToDate);
                var totalCreditInLcCostClearingAccount = lcCostClearingAccount.Sum(x => x.Credit);

                request.AccountId = AccountHeadConstants.PurchasePriceVariance.ToGuid();
                var purchasePriceVariance = await _accountService.SubsidiaryLedger(request.AccountId, request.FromDate, request.ToDate);
                var purchasePriceVarianceDuringThisPeriod = purchasePriceVariance.Last().Balance - purchasePriceVariance.First().Balance;

                request.AccountId = AccountHeadConstants.PurchaseReturnClearingAccount.ToGuid();
                var purchaseReturnClearingAccount = await _accountService.SubsidiaryLedger(request.AccountId, request.FromDate, request.ToDate);
                var balanceInPurchaseReturnClearingAccount = purchaseReturnClearingAccount.Sum(x => x.Debit);

                request.AccountId = AccountHeadConstants.PurchaseDiscount.ToGuid();
                var purchaseDiscount = await _accountService.SubsidiaryLedger(request.AccountId, request.FromDate, request.ToDate);
                var totalCreditInPurchaseDiscount = purchaseDiscount.Sum(x => x.Credit);

                request.AccountId = AccountHeadConstants.GainLossInventoryVariance.ToGuid();
                var gainLossInventoryVariance = await _accountService.SubsidiaryLedger(request.AccountId, request.FromDate, request.ToDate);
                var totalBalanceInGainLossInventoryVariance = gainLossInventoryVariance.Sum(x => x.Debit)-gainLossInventoryVariance.Sum(x => x.Credit);

                var workInProgressInventory = await _accountService.CalculateBalanceByAccountId(Guid.Parse(AccountHeadConstants.WorkInProgressInventory), Guid.Parse(AccountTypeConstants.CurrentAsset), financialYearId, request.FromDate, request.ToDate);

                var factoryOverhead = await _accountService.CalculateBalanceByAccountId(Guid.Parse(AccountHeadConstants.FactoryOverhead), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, request.FromDate, request.ToDate);

                var directExpenses = await _accountService.CalculateBalanceByAccountId(Guid.Parse(AccountHeadConstants.DirectExpenses), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, request.FromDate, request.ToDate);

                var factoryOverhead_Standard = await _accountService.CalculateBalanceByAccountId(Guid.Parse(AccountHeadConstants.FactoryOverhead_Standard), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, request.FromDate, request.ToDate);

                var directExpenses_Standard = await _accountService.CalculateBalanceByAccountId(Guid.Parse(AccountHeadConstants.DirectExpenses_Standard), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, request.FromDate, request.ToDate);

                var cogs = await _accountService.CalculateBalanceByAccountId(Guid.Parse(AccountHeadConstants.CostOfGoodsSold), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, request.FromDate, request.ToDate);

                var desiredGrn = _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking().Where(x => x.Status >= (int)GRNStatus.Approved).AsQueryable();
                desiredGrn = desiredGrn.Where(d => d.Grndate.Date >= request.FromDate!.Value.Date && d.Grndate.Date <= request.ToDate!.Value.Date);
                var grnCost = desiredGrn.Sum(x => x.TransportationCost);


                var result = new CogsViewModel()
                {
                    RawMaterialsOpeningValue = rmOpeningValue,
                    RawMaterialsClosingValue = rmClosingValue,
                    FinishedGoodsOpeningValue = fgOpeningValue,
                    FinishedGoodsClosingValue = fgClosingValue,
                    TotalCreditInPurchaseClearingAccount = totalCreditInPurchaseClearingAccount,
                    PurchasePriceVarianceDuringThisPeriod = purchasePriceVarianceDuringThisPeriod,
                    TotalCreditInLcCostClearingAccount = totalCreditInLcCostClearingAccount,
                    BalanceInPurchaseReturnClearingAccount = balanceInPurchaseReturnClearingAccount,
                    TotalCreditInPurchaseDiscount = totalCreditInPurchaseDiscount,
                    WorkInProgressInventory = workInProgressInventory,
                    FactoryOverhead = factoryOverhead - factoryOverhead_Standard,
                    DirectExpenses = directExpenses - directExpenses_Standard,
                    Cogs = cogs,
                    TransportationCost = grnCost,
                    GainLossInventoryVariance = totalBalanceInGainLossInventoryVariance,
                };


                var userName = _workContext.GetUserName() ?? "";

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "COGS Calculation";
                    var tables = await _accountPdfService.PrintCogsCalculationReportToPdfAsync(stream, result, headerText, request);

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
    }
}
