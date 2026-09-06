using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Store;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Configuration.Stores
{
    public class StoreService : BaseService<Store, StoreCreationDto, StoreUpdateDto, StoreRequestModel, StoreViewModel>, IStoreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;

        public StoreService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }


        public override async Task<Guid> AddAsync(StoreCreationDto StoreCreationDto)
        {
            var model = _mapper.Map<Store>(StoreCreationDto);
            var count = _unitOfWork.Repository<Store>().TableNoTracking().IgnoreQueryFilters().Count() + 1;
            model.Id = Guid.NewGuid();
            model.Code = count.ToString().PadLeft(3, '0');
            await _unitOfWork.Repository<Store>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }
    }
}
