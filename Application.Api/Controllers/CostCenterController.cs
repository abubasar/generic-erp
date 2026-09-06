using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.CostCenter;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.CostCenters;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CostCenterController : ControllerBase
    {
        private readonly string cacheKey = "cost-centers";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICostCenterService _costCenterService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public CostCenterController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork,
            ICostCenterService costCenterService, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _costCenterService = costCenterService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }
        [Authorize(Permissions.CostCenters.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<CostCenterViewModel>, int>>> Search(CostCenterRequestModel request)
        {
            return await Result<Tuple<List<CostCenterViewModel>, int>>.SuccessAsync(await _costCenterService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.CostCenters.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<CostCenterViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<CostCenterViewModel>, int> cachedList))
            {
                cachedList = await _costCenterService.SearchAsync(new CostCenterRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<CostCenterViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.CostCenters.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] CostCenterCreationDto costCenterCreationDto)
        {
            var costCenterId = await _costCenterService.AddAsync(costCenterCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(costCenterId, "Cost Center Added Successfully");

        }
        [Authorize(Permissions.CostCenters.Edit)]
        [HttpPost("/api/costCenter/update")]
        public virtual async Task<Result> Put([FromBody] CostCenterUpdateDto costCenterUpdateDto)
        {
            var costCenterId = await _costCenterService.UpdateAsync(costCenterUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(costCenterId, "Cost Center Updated Successfully");
        }
        [Authorize(Permissions.CostCenters.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var costCenterId = await _costCenterService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(costCenterId, "Cost Center Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(CostCenterRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _costCenterService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var costCenter in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        costCenter?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Cost Center List");
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
