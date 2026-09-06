using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.ProductCostSetup;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.ProductCostSetups;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCostSetupController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductCostSetupService _productCostSetupService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public ProductCostSetupController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IProductCostSetupService productCostSetupService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _productCostSetupService = productCostSetupService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.ProductCostSetups.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ProductCostSetupViewModel>, int>>> Search(ProductCostSetupRequestModel request)
        {
            return await Result<Tuple<List<ProductCostSetupViewModel>, int>>.SuccessAsync(await _productCostSetupService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.ProductCostSetups.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ProductCostSetupCreationDto productCostSetupCreationDto)
        {
            var productCostSetupId = await _productCostSetupService.AddAsync(productCostSetupCreationDto);
            return await Result<Guid>.SuccessAsync(productCostSetupId, "Product Cost Setup Added Successfully");
        }
        [Authorize(Permissions.ProductCostSetups.Edit)]
        [HttpPost("/api/productCostSetup/update")]
        public virtual async Task<Result> Put([FromBody] ProductCostSetupUpdateDto productCostSetupUpdateDto)
        {
            var productCostSetupId = await _productCostSetupService.UpdateAsync(productCostSetupUpdateDto);
            return await Result<Guid>.SuccessAsync(productCostSetupId, "Product Cost Setup Updated Successfully");
        }
        [Authorize(Permissions.ProductCostSetups.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var productCostSetupId = await _productCostSetupService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(productCostSetupId, "Product Cost Setup Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(ProductCostSetupRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _productCostSetupService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Produt Name", "Direct Expense", "Factory Overhead" };
                List<float> columnWidths = new List<float> { 10f, 40f, 25f, 25f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var productCostSetup in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        productCostSetup?.Product?.Name,
                        productCostSetup?.DirectExpense,
                        productCostSetup?.FactoryOverhead
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Product Cost Setup List");
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
