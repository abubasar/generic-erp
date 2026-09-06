
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.ProductType;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.ProductTypes;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductTypeService _productTypeService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public ProductTypeController(IProductTypeService productTypeService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _productTypeService = productTypeService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.ProductTypes.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ProductTypeViewModel>, int>>> Search(ProductTypeRequestModel request)
        {
            return await Result<Tuple<List<ProductTypeViewModel>, int>>.SuccessAsync(await _productTypeService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.ProductTypes.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ProductTypeCreationDto productTypeCreationDto)
        {
            var productTypeId = await _productTypeService.AddAsync(productTypeCreationDto);
            return await Result<Guid>.SuccessAsync(productTypeId, "ProductType Added Successfully");
        }
        [Authorize(Permissions.ProductTypes.Edit)]
        [HttpPost("/api/productType/update")]
        public virtual async Task<Result> Put([FromBody] ProductTypeUpdateDto productTypeUpdateDto)
        {
            var productTypeId = await _productTypeService.UpdateAsync(productTypeUpdateDto);
            return await Result<Guid>.SuccessAsync(productTypeId, "ProductType Updated Successfully");
        }
        [Authorize(Permissions.ProductTypes.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var productTypeId = await _productTypeService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(productTypeId, "ProductType Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(ProductTypeRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _productTypeService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name", "Inventory Type" };
                List<float> columnWidths = new List<float> { 10f, 45f, 45f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var productType in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        productType?.Name,
                        InventoryType = productType?.InventoryType?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Product Type List");
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
