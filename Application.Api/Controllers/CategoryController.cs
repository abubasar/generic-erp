using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Services.Dtos.Configuration.Category;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Categories;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly string cacheKey = "categories";
        private readonly ICategoryService _categoryService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;

        public CategoryController(ICategoryService categoryService, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _categoryService = categoryService;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }

        [Authorize(PrimaryPermissions.Categories.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<CategoryViewModel>, int>>> Search(CategoryRequestModel request)
        {
            return await Result<Tuple<List<CategoryViewModel>, int>>.SuccessAsync(await _categoryService.SearchAsync(request), "Result Found");
        }

        [Authorize(PrimaryPermissions.Categories.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<CategoryViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<CategoryViewModel>, int> cacheList))
            {
                cacheList = await _categoryService.SearchAsync(new CategoryRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cacheList);
            }
            return await Result<Tuple<List<CategoryViewModel>, int>>.SuccessAsync(cacheList, "Result Found");
        }

        [Authorize(PrimaryPermissions.Categories.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] CategoryCreationDto categoryCreationDto)
        {
            var categoryId = await _categoryService.AddAsync(categoryCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(categoryId, "Category Added Successfully");
        }

        [Authorize(PrimaryPermissions.Categories.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] CategoryUpdateDto categoryUpdateDto)
        {
            var categoryId = await _categoryService.UpdateAsync(categoryUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(categoryId, "Category Updated Successfully");
        }

        [Authorize(PrimaryPermissions.Categories.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var categoryId = await _categoryService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(categoryId, "Category Deleted Successfullly");
        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(CategoryRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _categoryService.SearchAsync(request);
                //Headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                //Create a list of data with properties mapped of header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var category in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        category?.Name
                    });
                }

                byte[] bytes;
                using (var ms = new MemoryStream())
                {

                    await _configurationPdfService.PrintReportToPdfAsync(ms, headers, columnWidths, tableData, "Category List");
                    bytes = ms.ToArray();
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
