using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Services.Dtos.Configuration.Territory;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.Territories;
using Application.Services.ViewModels.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerritoryController : ControllerBase
    {
        private readonly ITerritoryService _territoryService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public TerritoryController(ITerritoryService territoryService, IConfigurationPdfService configurationPdfService)
        {
            _territoryService = territoryService;
            _configurationPdfService = configurationPdfService;
        }

        [Authorize(PrimaryPermissions.Territories.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<TerritoryViewModel>, int>>> Search(TerritoryRequestModel request)
        {
            return await Result<Tuple<List<TerritoryViewModel>, int>>.SuccessAsync(await _territoryService.SearchAsync(request), "Result Found");
        }

        [Authorize(PrimaryPermissions.Territories.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] TerritoryCreationDto territoryCreationDto)
        {
            var territoryId = await _territoryService.AddAsync(territoryCreationDto);
            return await Result<Guid>.SuccessAsync(territoryId, "Territory Added Successfully");
        }

        [Authorize(PrimaryPermissions.Territories.Edit)]
        [HttpPost("/api/Territory/update")]
        public virtual async Task<Result> Put([FromBody] TerritoryUpdateDto territoryUpdateDto)
        {
            var territoryId = await _territoryService.UpdateAsync(territoryUpdateDto);
            return await Result<Guid>.SuccessAsync(territoryId, "Territory Updated Successfully");
        }

        [Authorize(PrimaryPermissions.Territories.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var territoryId = await _territoryService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(territoryId, "Territory Deleted Successfully");
        }

        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(TerritoryRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _territoryService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name", "Area" };
                List<float> columnWidths = new List<float> { 10f, 45f, 45f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var territory in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        territory?.Name,
                        Area = territory?.Area?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Territory List");
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
