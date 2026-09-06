using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.DiscountProductWise;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Sale;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Configuration.DiscountProductWises
{
    public class DiscountProductWiseService : BaseService<DiscountProductWise, DiscountProductWiseCreationDto, DiscountProductWiseUpdateDto, DiscountProductWiseRequestModel, DiscountProductWiseViewModel>, IDiscountProductWiseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;

        public DiscountProductWiseService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }

        public new async Task<AddUpdateResponseModel> AddAsync(DiscountProductWiseCreationDto discountProductWiseCreationDto)
        {
            discountProductWiseCreationDto.StartDate = discountProductWiseCreationDto.StartDate.ToLocal();
            discountProductWiseCreationDto.EndDate = discountProductWiseCreationDto.EndDate.ToLocal();
            var discountProductWise = _mapper.Map<DiscountProductWise>(discountProductWiseCreationDto);
            discountProductWise.Id = Guid.NewGuid();
            foreach (var item in discountProductWise.DiscountProductWiseDetails)
            {
                item.Id = Guid.NewGuid();
                item.DiscountProductWiseId = discountProductWise.Id;
                await _unitOfWork.Repository<DiscountProductWiseDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<DiscountProductWise>().AddAsync(discountProductWise);
            await _unitOfWork.SaveChangesAsync();

            return new AddUpdateResponseModel { Id = discountProductWise.Id };
        }

        public new async Task<AddUpdateResponseModel> UpdateAsync(DiscountProductWiseUpdateDto discountProductWiseUpdateDto)
        {
            var dbDiscountProductWise = await _unitOfWork.Repository<DiscountProductWise>().FindAsync(discountProductWiseUpdateDto.Id);
            if (discountProductWiseUpdateDto.StartDate.Equals(dbDiscountProductWise.StartDate) == false)
            {
                discountProductWiseUpdateDto.StartDate = discountProductWiseUpdateDto.StartDate.ToLocal();
            }
            if (discountProductWiseUpdateDto.EndDate.Equals(dbDiscountProductWise.EndDate) == false)
            {
                discountProductWiseUpdateDto.EndDate = discountProductWiseUpdateDto.EndDate.ToLocal();
            }
            var discountProductWise = _mapper.Map(discountProductWiseUpdateDto, dbDiscountProductWise);
            await _unitOfWork.Repository<DiscountProductWise>().UpdateAsync(discountProductWise);
            foreach (var item in discountProductWise.DiscountProductWiseDetails)
            {
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                    item.DiscountProductWiseId = discountProductWise.Id;
                    await _unitOfWork.Repository<DiscountProductWiseDetail>().AddAsync(item);
                }
                else
                {
                    await _unitOfWork.Repository<DiscountProductWiseDetail>().UpdateAsync(item);
                }
            }
            //Delete for DiscountProductWiseDetails
            if (!string.IsNullOrEmpty(discountProductWiseUpdateDto.DeletedDiscountProductWiseDetailIds))
            {
                foreach (var id in discountProductWiseUpdateDto.DeletedDiscountProductWiseDetailIds.Split(',').Where(x => x != ""))
                {
                    var discountProductWiseDetail = await _unitOfWork.Repository<DiscountProductWiseDetail>().FindAsync(new Guid(id));
                    discountProductWiseDetail.Deleted = true;
                    await _unitOfWork.Repository<DiscountProductWiseDetail>().UpdateAsync(discountProductWiseDetail);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return new AddUpdateResponseModel { Id = discountProductWise.Id };
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var discountProductWise = await _unitOfWork.Repository<DiscountProductWise>().FindAsync(x => x.Id == id, x => x.Include(x => x.DiscountProductWiseDetails));
            if (discountProductWise == null) throw new NotFoundResultException("Discount Product Wise Not Found With this Id");
            foreach (var item in discountProductWise.DiscountProductWiseDetails.ToList())
            {
                item.Deleted = true;
                await _unitOfWork.Repository<DiscountProductWiseDetail>().UpdateAsync(item);
            }
            discountProductWise.Deleted = true;
            await _unitOfWork.Repository<DiscountProductWise>().UpdateAsync(discountProductWise);
            await _unitOfWork.SaveChangesAsync();
            return discountProductWise.Id;
        }

        public async Task<DiscountProductWiseByDateViewModel> GetDiscountProductWiseByProductId(DiscountProductWiseByProductIdRequestModel request)
        {
            var discountProductWise = await _unitOfWork.Repository<DiscountProductWise>()
                                                        .TableNoTracking().Include(x => x.DiscountProductWiseDetails).SingleOrDefaultAsync(x => x.StartDate <= request.checkDate!.Value.ToLocal().Date && request.checkDate!.Value.ToLocal().Date <= x.EndDate && x.IsActive);
            if (discountProductWise is not null)
            {
                var discountDetail = await _unitOfWork.Repository<DiscountProductWiseDetail>()
                                                  .TableNoTracking()
                                                  .SingleAsync(x => x.DiscountProductWiseId == discountProductWise.Id
                                                  && x.ProductId == request.productId);
                return new DiscountProductWiseByDateViewModel
                {
                    DiscountProductWiseId = discountProductWise.Id,
                    DiscountAmountPerKg = discountDetail.DiscountAmountPerKg,
                };
            }
            return new DiscountProductWiseByDateViewModel
            {
                DiscountProductWiseId = null,
                DiscountAmountPerKg = 0,
            };
        }

        public async Task<List<ActiveDiscountProductWiseByDateViewModel>> GetActiveDiscountProductWiseByDate(DiscountProductWiseByDateRequestModel request)
        {
            List<ActiveDiscountProductWiseByDateViewModel> result = new();
            var discountProductWise = await _unitOfWork.Repository<DiscountProductWise>().TableNoTracking().Include(x => x.DiscountProductWiseDetails).SingleOrDefaultAsync(x => x.StartDate <= request.checkDate!.Value.ToLocal().Date && request.checkDate!.Value.ToLocal().Date <= x.EndDate && x.IsActive);
            if (discountProductWise is not null)
            {
                var queryable = _unitOfWork.Repository<DiscountProductWiseDetail>().TableNoTracking().Where(x => x.DiscountProductWiseId == discountProductWise.Id).AsQueryable();
                result = await queryable.Select(d => new ActiveDiscountProductWiseByDateViewModel
                {
                    DiscountProductWiseId = discountProductWise.Id,
                    ProductId = d.ProductId,
                    DiscountAmountPerKg = d.DiscountAmountPerKg
                }).ToListAsync();
            }
            return result;
        }
    }
}
