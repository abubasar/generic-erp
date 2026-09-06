using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Product;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Application.Services.Services.Configuration.Products
{
    public class ProductService : BaseService<Product, ProductCreationDto, ProductUpdateDto, ProductRequestModel, ProductViewModel>, IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }


        public override async Task<Guid> AddAsync(ProductCreationDto productCreationDto)
        {
            string pattern = "[^a-zA-Z0-9]"; // Matches any character that is not a letter or digit
            productCreationDto.Name = productCreationDto.Name.ToUpper();
            var model = _mapper.Map<Product>(productCreationDto);
            var count = _unitOfWork.Repository<Product>().TableNoTracking().IgnoreQueryFilters().Count() + 1;
            model.Id = Guid.NewGuid();
            model.Code = count.ToString().PadLeft(4, '0');
            model.PropertyName = Regex.Replace(productCreationDto.Name, pattern, "");
            await _unitOfWork.Repository<Product>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;

        }
        public async Task<bool> ProductExists(string name)
        {
            if (await _unitOfWork.Repository<Product>().TableNoTracking().AnyAsync(x => x.Name.ToLower() == name.ToLower())) //TableWithoutTenant()
                return true;
            return false;
        }


    }
}
