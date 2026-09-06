
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.InventoryType;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.InventoryTypes;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryTypeController : ControllerBase
    {
        private readonly string cacheKey = "inventory-types";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryTypeService _inventoryTypeService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public InventoryTypeController(IInventoryTypeService inventoryTypeService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _inventoryTypeService = inventoryTypeService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }
        [Authorize(Permissions.InventoryTypes.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<InventoryTypeViewModel>, int>>> Search(InventoryTypeRequestModel request)
        {
            return await Result<Tuple<List<InventoryTypeViewModel>, int>>.SuccessAsync(await _inventoryTypeService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.InventoryTypes.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<InventoryTypeViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<InventoryTypeViewModel>, int> cachedList))
            {
                cachedList = await _inventoryTypeService.SearchAsync(new InventoryTypeRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<InventoryTypeViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.InventoryTypes.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] InventoryTypeCreationDto inventoryTypeCreationDto)
        {
            var inventoryTypeId = await _inventoryTypeService.AddAsync(inventoryTypeCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(inventoryTypeId, "InventoryType Added Successfully");
        }
        [Authorize(Permissions.InventoryTypes.Edit)]
        [HttpPost("/api/inventoryType/update")]
        public virtual async Task<Result> Put([FromBody] InventoryTypeUpdateDto inventoryTypeUpdateDto)
        {
            var inventoryTypeId = await _inventoryTypeService.UpdateAsync(inventoryTypeUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(inventoryTypeId, "InventoryType Updated Successfully");
        }
        [Authorize(Permissions.InventoryTypes.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var inventoryTypeId = await _inventoryTypeService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(inventoryTypeId, "InventoryType Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(InventoryTypeRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _inventoryTypeService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var inventoryType in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        inventoryType?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Inventory Type List");
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
