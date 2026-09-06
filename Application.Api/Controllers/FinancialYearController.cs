using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.FinancialYear;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.FinancialYears;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialYearController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFinancialYearService _financialYearService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public FinancialYearController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IFinancialYearService financialYearService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _financialYearService = financialYearService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.FinancialYears.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<FinancialYearViewModel>, int>>> Search(FinancialYearRequestModel request)
        {
            return await Result<Tuple<List<FinancialYearViewModel>, int>>.SuccessAsync(await _financialYearService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.FinancialYears.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] FinancialYearCreationDto financialYearCreationDto)
        {
            var financialYearId = await _financialYearService.AddAsync(financialYearCreationDto);
            return await Result<Guid>.SuccessAsync(financialYearId, "Financial Year Added Successfully");
        }
        [Authorize(Permissions.FinancialYears.Edit)]
        [HttpPost("/api/financialYear/update")]
        public virtual async Task<Result> Put([FromBody] FinancialYearUpdateDto financialYearUpdateDto)
        {
            var financialYearId = await _financialYearService.UpdateAsync(financialYearUpdateDto);
            return await Result<Guid>.SuccessAsync(financialYearId, "Financial Year Updated Successfully");
        }
        [Authorize(Permissions.FinancialYears.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var financialYearId = await _financialYearService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(financialYearId, "Financial Year Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(FinancialYearRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _financialYearService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name", "Start Date", "End Date", "Active" };
                List<float> columnWidths = new List<float> { 10f, 30f, 25f, 25f, 10f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var financialYear in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        financialYear?.Name,
                        StartDate = financialYear?.StartDate.ToString("dd/MM/yyyy"),
                        EndDate = financialYear?.EndDate.ToString("dd/MM/yyyy"),
                        Active = financialYear!.IsActive ? "Yes" : "No",
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Financial Year List");
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
