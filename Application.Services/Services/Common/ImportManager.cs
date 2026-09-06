using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using OfficeOpenXml;

namespace Application.Services.Services.Common
{
    public class ImportManager : IImportManager
    {
        private readonly DataContext _context;
        private readonly IWorkContext _workContext;
        public ImportManager(DataContext context, IWorkContext workContext)
        {
            _context = context;
            _workContext = workContext;

        }
        private IList<PropertyByName<T>> GetPropertiesByExcelCells<T>(ExcelWorksheet worksheet)
        {
            var properties = new List<PropertyByName<T>>();
            var poz = 1;
            while (true)
            {
                try
                {
                    var cell = worksheet.Cells[1, poz];

                    if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            return properties;
        }
        public virtual void ImportRawMaterialsFromXlsx(Stream stream)
        {
            var userId = _workContext.GetUserId();
            using var xlPackage = new ExcelPackage(stream);
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");
            var properties = GetPropertiesByExcelCells<Stock>(worksheet);
            var manager = new PropertyManager<Stock>(properties);
            var iRow = 2;
            var totalRawMaterialAmount = 0m;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));
                if (allColumnsAreEmpty)
                    break;
                manager.ReadFromXlsx(worksheet, iRow);
                var qty = manager.GetProperty("Quantity").DecimalValue;
                var rate = manager.GetProperty("Rate").DecimalValue;
                var product = _context.Products.Where(x => Convert.ToInt16(x.Code) == manager.GetProperty("Code").IntValue).FirstOrDefault();
                if (product is null) throw new Exception("Product Not Found");
                if (qty > 0m)
                {
                    _context.Stocks.Add(new Stock
                    {
                        Id = Guid.NewGuid(),
                        TransactonType = (int)TransactonType.Purchase,
                        InventoryTypeId = product.InventoryTypeId,
                        ProductTypeId = product.ProductTypeId,
                        ProductId = product.Id,
                        StoreId = "4EB95ED5-34C1-404C-ADAC-8DB161AE1922".ToGuid(),//raw material store
                        AvailableQty = qty,
                        InQty = qty,
                        InRate = rate,
                        BatchNo = "OS",
                        TransactionId = Guid.NewGuid(),
                        StockInDate = DateTime.Now.AddDays(-1),
                        Remark = "Opening Raw Material",
                        CreatedOn = DateTime.Now.AddDays(-1),
                        UpdatedOn = DateTime.Now.AddDays(-1),
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                        TenantId = product.TenantId,
                    });


                }
                iRow++;
                totalRawMaterialAmount += (qty * rate);
            }
            //owners capital credit
            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
                Vnumber = "OMG",
                Description = "Opening Raw Material",
                AccountId = "3B07A7D4-4428-431E-9D32-EDA39BEB5991".ToGuid(),//owners capital
                Debit = 0m,
                Credit = totalRawMaterialAmount,
                IsOpeningBalance = true,
                TransactionDate = DateTime.Now.AddDays(-1),
                CreatedOn = DateTime.Now.AddDays(-1),
                UpdatedOn = DateTime.Now.AddDays(-1),
                CreatedBy = userId,
                UpdatedBy = userId,
                FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                TenantId = "DC6A03D8-3B41-4739-B304-C38839BBFB75".ToGuid()//ap feed mill tenant id
            });
            //raw materials inventory debit
            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
                Vnumber = "OMG",
                Description = "Opening Raw Material",
                AccountId = AccountHeadConstants.RawMaterialsInventory.ToGuid(),
                Debit = totalRawMaterialAmount,
                Credit = 0m,
                IsOpeningBalance = true,
                TransactionDate = DateTime.Now.AddDays(-1),
                CreatedOn = DateTime.Now.AddDays(-1),
                UpdatedOn = DateTime.Now.AddDays(-1),
                CreatedBy = userId,
                UpdatedBy = userId,
                FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                TenantId = "DC6A03D8-3B41-4739-B304-C38839BBFB75".ToGuid()//ap feed mill tenant id
            });
            _context.SaveChanges();
        }
        public virtual void ImportFinishedGoodsFromXlsx(Stream stream)
        {
            var userId = _workContext.GetUserId();
            using var xlPackage = new ExcelPackage(stream);
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");
            var properties = GetPropertiesByExcelCells<Stock>(worksheet);
            var manager = new PropertyManager<Stock>(properties);
            var iRow = 2;
            var totalFinishedGoodsAmount = 0m;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));
                if (allColumnsAreEmpty)
                    break;
                manager.ReadFromXlsx(worksheet, iRow);
                var qty = manager.GetProperty("Quantity").DecimalValue;
                var rate = manager.GetProperty("Rate").DecimalValue;
                var product = _context.Products.Where(x => Convert.ToInt16(x.Code) == manager.GetProperty("Code").IntValue).FirstOrDefault();
                if (product is null) throw new Exception("Product Not Found");
                if (qty > 0m)
                {
                    _context.Stocks.Add(new Stock
                    {
                        Id = Guid.NewGuid(),
                        TransactonType = (int)TransactonType.Production,
                        InventoryTypeId = product.InventoryTypeId,
                        ProductTypeId = product.ProductTypeId,
                        ProductId = product.Id,
                        StoreId = "03A389C2-EEBE-4DAD-9FEF-A89AD3B1C736".ToGuid(),//finished good store
                        AvailableQty = qty,
                        InQty = qty,
                        InRate = rate,
                        BatchNo = "OS",
                        TransactionId = Guid.NewGuid(),
                        StockInDate = DateTime.Now.AddDays(-1),
                        Remark = "Opening Finished Good",
                        CreatedOn = DateTime.Now.AddDays(-1),
                        UpdatedOn = DateTime.Now.AddDays(-1),
                        CreatedBy = userId,
                        UpdatedBy = userId,
                        FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                        TenantId = product.TenantId
                    });
                }
                iRow++;
                totalFinishedGoodsAmount += (qty * rate);
            }
            //owners capital credit
            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
                Vnumber = "OFG",
                Description = "Opening Finished Good",
                AccountId = "3B07A7D4-4428-431E-9D32-EDA39BEB5991".ToGuid(),//owners capital
                Debit = 0m,
                Credit = totalFinishedGoodsAmount,
                IsOpeningBalance = true,
                TransactionDate = DateTime.Now.AddDays(-1),
                CreatedOn = DateTime.Now.AddDays(-1),
                UpdatedOn = DateTime.Now.AddDays(-1),
                CreatedBy = userId,
                UpdatedBy = userId,
                FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                TenantId = "DC6A03D8-3B41-4739-B304-C38839BBFB75".ToGuid()//ap feed mill tenant id
            });
            //finished Goods inventory debit
            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
                Vnumber = "OFG",
                Description = "Opening Finished Good",
                AccountId = AccountHeadConstants.FinishedGoodsInventory.ToGuid(),
                Debit = totalFinishedGoodsAmount,
                Credit = 0m,
                IsOpeningBalance = true,
                TransactionDate = DateTime.Now.AddDays(-1),
                CreatedOn = DateTime.Now.AddDays(-1),
                UpdatedOn = DateTime.Now.AddDays(-1),
                CreatedBy = userId,
                UpdatedBy = userId,
                FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                TenantId = "DC6A03D8-3B41-4739-B304-C38839BBFB75".ToGuid()//ap feed mill tenant id
            });
            _context.SaveChanges();
        }
        public virtual void ImportCashAndBankOpeningBalanceFromXlsx(Stream stream)
        {
            var userId = _workContext.GetUserId();
            using var xlPackage = new ExcelPackage(stream);
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");
            var properties = GetPropertiesByExcelCells<Stock>(worksheet);
            var manager = new PropertyManager<Stock>(properties);
            var iRow = 2;
            var totalDebit = 0m;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));
                if (allColumnsAreEmpty)
                    break;
                manager.ReadFromXlsx(worksheet, iRow);
                var debit = manager.GetProperty("Debit").DecimalValue;
                var account = _context.Accounts.Where(x => x.Code == manager.GetProperty("Code").StringValue).FirstOrDefault();
                if (account is null) throw new Exception("Account Not Found");
                if (debit == 0m) continue;
                _context.Transactions.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),
                    Vnumber = "OBB",
                    Description = "Opening Bank Balance",
                    AccountId = account.Id,
                    Debit = debit,
                    Credit = 0,
                    IsOpeningBalance = true,
                    TransactionDate = DateTime.Now.AddDays(-1),
                    CreatedOn = DateTime.Now.AddDays(-1),
                    UpdatedOn = DateTime.Now.AddDays(-1),
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                    TenantId = account.TenantId
                });
                iRow++;
                totalDebit += debit;
            }
            //owners capital credit
            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
                Vnumber = "OBB",
                Description = "Opening Bank Balance",
                AccountId = "3B07A7D4-4428-431E-9D32-EDA39BEB5991".ToGuid(),//owners capital
                Debit = 0m,
                Credit = totalDebit,
                IsOpeningBalance = true,
                TransactionDate = DateTime.Now.AddDays(-1),
                CreatedOn = DateTime.Now.AddDays(-1),
                UpdatedOn = DateTime.Now.AddDays(-1),
                CreatedBy = userId,
                UpdatedBy = userId,
                FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                TenantId = "DC6A03D8-3B41-4739-B304-C38839BBFB75".ToGuid()//ap feed mill tenant id
            });
            _context.SaveChanges();
        }
        //public virtual void ImportCashAndBankOpeningBalanceFromXlsx(Stream stream)
        //{
        //    var userId = _workContext.GetUserId();
        //    using var xlPackage = new ExcelPackage(stream);
        //    var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
        //    if (worksheet == null)
        //        throw new Exception("No worksheet found");
        //    var properties = GetPropertiesByExcelCells<Stock>(worksheet);
        //    var manager = new PropertyManager<Stock>(properties);
        //    var iRow = 2;
        //    var totalDebit = 0m;
        //    while (true)
        //    {
        //        var allColumnsAreEmpty = manager.GetProperties
        //            .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
        //            .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));
        //        if (allColumnsAreEmpty)
        //            break;
        //        manager.ReadFromXlsx(worksheet, iRow);
        //        var debit = manager.GetProperty("Debit").DecimalValue;
        //        var account = _context.Accounts.Where(x => x.Code == manager.GetProperty("Code").StringValue).FirstOrDefault();
        //        if (account is null) throw new Exception("Account Not Found");
        //        if (debit == 0m) continue;
        //        _context.Transactions.Add(new Transaction
        //        {
        //            Id = Guid.NewGuid(),
        //            CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),
        //            Vnumber = "OCB",
        //            Description = "Opening Cash Bank Balance",
        //            AccountId = account.Id,
        //            Debit = debit,
        //            Credit = 0,
        //            IsOpeningBalance = true,
        //            TransactionDate = DateTime.Now.AddDays(-1),
        //            CreatedOn = DateTime.Now.AddDays(-1),
        //            UpdatedOn = DateTime.Now.AddDays(-1),
        //            CreatedBy = userId,
        //            UpdatedBy = userId,
        //            FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
        //            TenantId = account.TenantId
        //        });
        //        iRow++;
        //        totalDebit += debit;
        //    }
        //    //owners capital credit
        //    _context.Transactions.Add(new Transaction
        //    {
        //        Id = Guid.NewGuid(),
        //        CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
        //        Vnumber = "OCB",
        //        Description = "Opening Cash Bank Balance",
        //        AccountId = "3B07A7D4-4428-431E-9D32-EDA39BEB5991".ToGuid(),//owners capital
        //        Debit = 0m,
        //        Credit = totalDebit,
        //        IsOpeningBalance = true,
        //        TransactionDate = DateTime.Now.AddDays(-1),
        //        CreatedOn = DateTime.Now.AddDays(-1),
        //        UpdatedOn = DateTime.Now.AddDays(-1),
        //        CreatedBy = userId,
        //        UpdatedBy = userId,
        //        FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
        //        TenantId = "DC6A03D8-3B41-4739-B304-C38839BBFB75".ToGuid()//ap feed mill tenant id
        //    });
        //    _context.SaveChanges();
        //}

        public virtual void ImportCustomerOpeningBalanceFromXlsx(Stream stream)
        {
            var userId = _workContext.GetUserId();
            using var xlPackage = new ExcelPackage(stream);
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");
            var properties = GetPropertiesByExcelCells<Stock>(worksheet);
            var manager = new PropertyManager<Stock>(properties);
            var iRow = 2;
            var totalDebit = 0m;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));
                if (allColumnsAreEmpty)
                    break;
                manager.ReadFromXlsx(worksheet, iRow);
                var debit = manager.GetProperty("Debit").DecimalValue;
                var account = _context.Accounts.Where(x => x.Code == manager.GetProperty("Code").StringValue).FirstOrDefault();
                if (account is null) throw new Exception("Account Not Found");
                if (debit == 0m) continue;
                _context.Transactions.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),
                    Vnumber = "OCB",
                    Description = "Opening Customer Balance",
                    AccountId = account.Id,
                    Debit = debit,
                    Credit = 0,
                    IsOpeningBalance = true,
                    TransactionDate = DateTime.Now.AddDays(-1),
                    CreatedOn = DateTime.Now.AddDays(-1),
                    UpdatedOn = DateTime.Now.AddDays(-1),
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                    TenantId = account.TenantId
                });
                iRow++;
                totalDebit += debit;
            }
            //owners capital credit
            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
                Vnumber = "OCB",
                Description = "Opening Customer Balance",
                AccountId = "3B07A7D4-4428-431E-9D32-EDA39BEB5991".ToGuid(),//owners capital
                Debit = 0m,
                Credit = totalDebit,
                IsOpeningBalance = true,
                TransactionDate = DateTime.Now.AddDays(-1),
                CreatedOn = DateTime.Now.AddDays(-1),
                UpdatedOn = DateTime.Now.AddDays(-1),
                CreatedBy = userId,
                UpdatedBy = userId,
                FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                TenantId = "DC6A03D8-3B41-4739-B304-C38839BBFB75".ToGuid()//ap feed mill tenant id
            });
            _context.SaveChanges();
        }
        public virtual void ImportSupplierOpeningBalanceFromXlsx(Stream stream)
        {
            var userId = _workContext.GetUserId();
            using var xlPackage = new ExcelPackage(stream);
            var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new Exception("No worksheet found");
            var properties = GetPropertiesByExcelCells<Stock>(worksheet);
            var manager = new PropertyManager<Stock>(properties);
            var iRow = 2;
            var totalCredit = 0m;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Cells[iRow, property.PropertyOrderPosition])
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));
                if (allColumnsAreEmpty)
                    break;
                manager.ReadFromXlsx(worksheet, iRow);
                var credit = manager.GetProperty("Credit").DecimalValue;
                var account = _context.Accounts.Where(x => x.Code == manager.GetProperty("Code").StringValue).FirstOrDefault();
                if (account is null) throw new Exception("Account Not Found");
                if (credit == 0m) continue;
                _context.Transactions.Add(new Transaction
                {
                    Id = Guid.NewGuid(),
                    CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),
                    Vnumber = "OSB",
                    Description = "Opening Supplier Balance",
                    AccountId = account.Id,
                    Debit = 0m,
                    Credit = credit,
                    IsOpeningBalance = true,
                    TransactionDate = DateTime.Now.AddDays(-1),
                    CreatedOn = DateTime.Now.AddDays(-1),
                    UpdatedOn = DateTime.Now.AddDays(-1),
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                    TenantId = account.TenantId
                });
                iRow++;
                totalCredit += credit;
            }
            //owners capital debit
            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
                Vnumber = "OSB",
                Description = "Opening Supplier Balance",
                AccountId = "3B07A7D4-4428-431E-9D32-EDA39BEB5991".ToGuid(),//owners capital
                Debit = totalCredit,
                Credit = 0,
                IsOpeningBalance = true,
                TransactionDate = DateTime.Now.AddDays(-1),
                CreatedOn = DateTime.Now.AddDays(-1),
                UpdatedOn = DateTime.Now.AddDays(-1),
                CreatedBy = userId,
                UpdatedBy = userId,
                FinancialYearId = "D6503850-F14B-4540-8C75-00240EF49481".ToGuid(),
                TenantId = "DC6A03D8-3B41-4739-B304-C38839BBFB75".ToGuid()//ap feed mill tenant id
            });
            _context.SaveChanges();
        }

    }
}
