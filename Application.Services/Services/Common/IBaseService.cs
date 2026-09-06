using Application.Core.Common;


namespace Application.Services.Services.Common
{
    public interface IBaseService<TEntity, TCreationDto, TUpdateDto, TRequestModel, TViewModel> where TRequestModel : BaseRequestModel<TEntity> where TEntity : class
    {
        Task<Tuple<List<TViewModel>, int>> SearchAsync(TRequestModel request);
        Task<Guid> AddAsync(TCreationDto creationDto);
        Task<Guid> UpdateAsync(TUpdateDto updateDto);
        Task<Guid> DeleteAsync(Guid id);
    }
}
