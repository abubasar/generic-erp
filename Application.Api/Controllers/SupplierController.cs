
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Application.Services.Services.Accounts.Suppliers;
using Application.Services.ViewModels.Accounts;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Api.Attributes;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly string cacheKey = "suppliers";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISupplierService _supplierService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public SupplierController(ISupplierService supplierService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _supplierService = supplierService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }
        [Authorize(Permissions.Suppliers.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<SupplierViewModel>, int>>> Search(SupplierRequestModel request)
        {
            return await Result<Tuple<List<SupplierViewModel>, int>>.SuccessAsync(await _supplierService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Suppliers.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<SupplierViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<SupplierViewModel>, int> cachedList))
            {
                cachedList = await _supplierService.SearchAsync(new SupplierRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<SupplierViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.Suppliers.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] SupplierCreationDto supplierCreationDto)
        {
            if (await _supplierService.SupplierExists(supplierCreationDto.Name ?? "")) return await Result<string>.FailAsync("", "Supplier Already Exists.");
            var supplierId = await _supplierService.AddAsync(supplierCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(supplierId, "Supplier Added Successfully");
        }
        [Authorize(Permissions.Suppliers.Edit)]
        [HttpPost("/api/supplier/update")]
        public virtual async Task<Result> Put([FromBody] SupplierUpdateDto supplierUpdateDto)
        {
            var supplierId = await _supplierService.UpdateAsync(supplierUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(supplierId, "Supplier Updated Successfully");
        }
        [Authorize(Permissions.Suppliers.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var supplierId = await _supplierService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(supplierId, "Supplier Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(SupplierRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.OrderBy = "Name";
                request.IsAscending = true;
                var list = await _supplierService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintSupplierReportToPdfAsync(stream, list.Item1.ToList(), "Supplier List");
                    bytes = stream.ToArray();
                    // bytes = _pdfService.AddPageNumbers(bytes);
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }
    }
}
