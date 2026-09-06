
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Store;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.Stores;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : ControllerBase
    {
        private readonly string cacheKey = "stores";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStoreService _storeService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public StoreController(IStoreService storeService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _storeService = storeService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }
        [Authorize(Permissions.Stores.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<StoreViewModel>, int>>> Search(StoreRequestModel request)
        {
            return await Result<Tuple<List<StoreViewModel>, int>>.SuccessAsync(await _storeService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Stores.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<StoreViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<StoreViewModel>, int> cachedList))
            {
                cachedList = await _storeService.SearchAsync(new StoreRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<StoreViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.Stores.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] StoreCreationDto storeCreationDto)
        {
            var storeId = await _storeService.AddAsync(storeCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(storeId, "Store Added Successfully");

        }
        [Authorize(Permissions.Stores.Edit)]
        [HttpPost("/api/store/update")]
        public virtual async Task<Result> Put([FromBody] StoreUpdateDto storeUpdateDto)
        {
            var storeId = await _storeService.UpdateAsync(storeUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(storeId, "Store Updated Successfully");


        }
        [Authorize(Permissions.Stores.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var storeId = await _storeService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(storeId, "Store Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(StoreRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _storeService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name", "Inventory Type" };
                List<float> columnWidths = new List<float> { 10f, 45f, 45f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var store in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        store?.Name,
                        InventoryType = store?.InventoryType?.Name
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Store List");
                    bytes = stream.ToArray();
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
