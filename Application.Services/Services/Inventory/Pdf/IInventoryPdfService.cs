using Application.Services.SearchRequestModels.Purchase;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.Services.Inventory.Pdf
{
    public interface IInventoryPdfService
    {
        Task PrintStockAdjustmentReportToPdfAsync(MemoryStream stream, List<StockAdjustmentViewModel> stockAdjustmentViewModels, string reportTitle, bool isDetails, StockAdjustmentRequestModel request);
    }
}
