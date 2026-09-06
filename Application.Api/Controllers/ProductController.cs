using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.ExcelHelper;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Product;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.Products;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly string cacheKey = "products";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        private readonly ITenantService _tenantService;
        public ProductController(IProductService productService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService, ICacheService cacheService, ITenantService tenantService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _productService = productService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
            _tenantService = tenantService;
        }
        [Authorize(Permissions.Products.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ProductViewModel>, int>>> Search(ProductRequestModel request)
        {
            return await Result<Tuple<List<ProductViewModel>, int>>.SuccessAsync(await _productService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.Products.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<ProductViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<ProductViewModel>, int> cachedList))
            {
                cachedList = await _productService.SearchAsync(new ProductRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<ProductViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.Products.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ProductCreationDto productCreationDto)
        {
            if (await _productService.ProductExists(productCreationDto.Name)) return await Result<string>.FailAsync("", "Product already Exists.");
            var productId = await _productService.AddAsync(productCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(productId, "Product Added Successfully");

        }
        [Authorize(Permissions.Products.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] ProductUpdateDto productUpdateDto)
        {
            var productId = await _productService.UpdateAsync(productUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(productId, "Product Updated Successfully");


        }
        [Authorize(Permissions.Products.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var productId = await _productService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(productId, "Product Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(ProductRequestModel request)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                request.Page = -1;
                var list = await _productService.SearchAsync(request);
                var itemList = list.Item1.OrderBy(x => x.Code).ToList();
                var userName = _workContext.GetUserName() ?? "";
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Product List";
                    var tables = await _configurationPdfService.PrintProductListReportToPdf(stream, itemList, tenantData.BusinessType, headerText, request);
                    if (request.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1", null, null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }
        [Authorize(PrimaryPermissions.PrimaryInventoryModuleReports.Primary_Product_Price_List_Report)]
        [HttpPost]
        [Route("print-product-price-list")]
        public virtual async Task<IActionResult> PrintProductPriceList(ProductRequestModel request)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                request.Page = -1;
                var userName = _workContext.GetUserName() ?? "";
                var list = await _productService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Product Price List";
                    var tables = await _configurationPdfService.PrintProductPriceListReportToPdf(stream, list.Item1, headerText, request);
                    if (request.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1", null, null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

    }
}
