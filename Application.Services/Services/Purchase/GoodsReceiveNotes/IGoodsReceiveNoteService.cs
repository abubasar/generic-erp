using Application.Core.Entities;
using Application.Services.Dtos.Purchase.GoodsReceiveNote;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.SearchRequestModels.Report;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.GoodsReceiveNote;
using Application.Services.ViewModels.Report.Purchase;

namespace Application.Services.Services.Purchase.GoodsReceiveNotes
{
    public interface IGoodsReceiveNoteService : IBaseService<GoodsReceiveNote, GoodsReceiveNoteCreationDto, GoodsReceiveNoteUpdateDto, GoodsReceiveNoteRequestModel, GoodsReceiveNoteViewModel>
    {
        Task<GoodsReceiveNoteAggregatorModel> PrepareGoodsReceiveNoteAggregatorModel(GoodsReceiveNoteRequestModel request);
        Task<GoodsReceiveNoteViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(GoodsReceiveNoteCreationDto creationDto);
        new Task<Guid> UpdateAsync(GoodsReceiveNoteUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<List<GrnItemViewModel>> GrnSupplierItemWiseSearchAsync(GoodsReceiveNoteRequestModel request);
        Task<List<GrnItemSummaryViewModel>> GrnItemSearchAsync(GoodsReceiveNoteRequestModel request);
        Task<List<ProductWithLatestPriceViewModel>> GetProductsWithLatestPurchasePriceAsync(ProductWithLatestPriceRequestModel request);
    }
}
