using Application.Services.Dtos.Purchase.LcAdjustment;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.ViewModels.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Core.Entities;
using Application.Services.Services.Common;

namespace Application.Services.Services.Purchase.LcAdjustments
{
    public interface ILcAdjustmentService : IBaseService<LcAdjustment, LcAdjustmentCreationDto, LcAdjustmentUpdateDto, LcAdjustmentRequestModel, LcAdjustmentViewModel>
    {
        Task<LcAdjustmentViewModel> GetByIdAsync(Guid id);
        new Task<Guid> AddAsync(LcAdjustmentCreationDto lcAdjustmentCreationDto);
        new Task<Guid> UpdateAsync(LcAdjustmentUpdateDto lcAdjustmentUpdateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
    }
}
