namespace Application.Services.Services.Common
{
    public interface IImportManager
    {
        void ImportRawMaterialsFromXlsx(Stream stream);
        void ImportFinishedGoodsFromXlsx(Stream stream);
        void ImportCashAndBankOpeningBalanceFromXlsx(Stream stream);
        void ImportCustomerOpeningBalanceFromXlsx(Stream stream);
        void ImportSupplierOpeningBalanceFromXlsx(Stream stream);
    }
}
