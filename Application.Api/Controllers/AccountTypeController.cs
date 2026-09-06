
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountType;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.AccountTypes;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Accounts;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountTypeController : ControllerBase
    {
        private readonly string cacheKey = "accountTypes";
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountTypeService _accountTypeService;
        private readonly IConfigurationPdfService _configurationPdfService;
        private readonly ICacheService _cacheService;
        public AccountTypeController(IAccountTypeService accountTypeService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper, IConfigurationPdfService configurationPdfService, ICacheService cacheService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _accountTypeService = accountTypeService;
            _mapper = mapper;
            _configurationPdfService = configurationPdfService;
            _cacheService = cacheService;
        }
        [Authorize(Permissions.AccountTypes.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<AccountTypeViewModel>, int>>> Search(AccountTypeRequestModel request)
        {
            return await Result<Tuple<List<AccountTypeViewModel>, int>>.SuccessAsync(await _accountTypeService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.AccountTypes.View)]
        [Route("all")]
        [HttpPost]
        public async Task<Result<Tuple<List<AccountTypeViewModel>, int>>> GetAll()
        {
            if (!_cacheService.TryGet(cacheKey, out Tuple<List<AccountTypeViewModel>, int> cachedList))
            {
                cachedList = await _accountTypeService.SearchAsync(new AccountTypeRequestModel { Page = -1, OrderBy = "Name", IsAscending = true });
                _cacheService.Set(cacheKey, cachedList);
            }
            return await Result<Tuple<List<AccountTypeViewModel>, int>>.SuccessAsync(cachedList, "Result Found");
        }
        [Authorize(Permissions.AccountTypes.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] AccountTypeCreationDto accountTypeCreationDto)
        {
            var accountTypeId = await _accountTypeService.AddAsync(accountTypeCreationDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(accountTypeId, "AccountType Added Successfully");

        }
        [Authorize(Permissions.AccountTypes.Edit)]
        [HttpPost("/api/accountType/update")]
        public virtual async Task<Result> Put([FromBody] AccountTypeUpdateDto accountTypeUpdateDto)
        {
            var accountTypeId = await _accountTypeService.UpdateAsync(accountTypeUpdateDto);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(accountTypeId, "AccountType Updated Successfully");


        }
        [Authorize(Permissions.AccountTypes.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var accountTypeId = await _accountTypeService.DeleteAsync(id);
            _cacheService.Remove(cacheKey);
            return await Result<Guid>.SuccessAsync(accountTypeId, "AccountType Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(AccountTypeRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _accountTypeService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var accountType in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        accountType?.Name

                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Account Type List");
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
