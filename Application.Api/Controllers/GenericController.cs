using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Generic;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Generics;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenericController : ControllerBase
    {
        private readonly string _cacheKey = "generics";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericServicecs _genericServicecs;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;

        public GenericController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IGenericServicecs genericServicecs, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _genericServicecs = genericServicecs;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }

        [Authorize(Permissions.Generics.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<GenericViewModel>, int>>> Search(GenericRequestModel request)
        {
            return await Result<Tuple<List<GenericViewModel>, int>>.SuccessAsync(await _genericServicecs.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Generics.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<GenericViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(_cacheKey, out Tuple<List<GenericViewModel>, int> cacheList))
            {
                cacheList = await _genericServicecs.SearchAsync(new GenericRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(_cacheKey, cacheList);
            }
            return await Result<Tuple<List<GenericViewModel>, int>>.SuccessAsync(cacheList, "Result Found");
        }

        [Authorize(Permissions.Generics.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] GenericCreationDto genericCreationDto)
        {
            var genericId = await _genericServicecs.AddAsync(genericCreationDto);
            _cacheService.Remove(_cacheKey);
            return await Result<Guid>.SuccessAsync(genericId, "Generic Added Successfully");
        }

        [Authorize(Permissions.Generics.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] GenericUpdateDto genericUpdateDto)
        {
            var genericId = await _genericServicecs.UpdateAsync(genericUpdateDto);
            _cacheService.Remove(_cacheKey);
            return await Result<Guid>.SuccessAsync(genericId, "Generic Updated Successfully");
        }

        [Authorize(Permissions.Generics.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var genericId = await _genericServicecs.DeleteAsync(id);
            _cacheService.Remove(_cacheKey);
            return await Result<Guid>.SuccessAsync(genericId, "Generic Deleted Successfully");
        }
    
        [Route("print")]
        [HttpPost]
        public virtual async Task<IActionResult> Print(GenericRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _genericServicecs.SearchAsync(request);

                //Header and Column Widths
                List<string> headers = new List<string> { "SL", "Name", "Product Type" };
                List<float> columnWidths = new List<float> { 10f, 45f, 45f };
                //Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var generic in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        generic?.Name,
                        ProductType = generic?.ProductType?.Name,
                    });
                }

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(ms, headers, columnWidths, tableData, "Generic List");
                    bytes = ms.ToArray();
                }
                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }

        }
    }
}
