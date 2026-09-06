using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.EmailAccount;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.EmailAccounts;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailAccountController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IConfigurationPdfService _configurationPdfService;

        public EmailAccountController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IEmailAccountService emailAccountService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _emailAccountService = emailAccountService;
            _configurationPdfService = configurationPdfService;
        }

        [Authorize(Permissions.EmailAccounts.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<EmailAccountViewModel>, int>>> Search(EmailAccountRequestModel emailAccountRequestModel)
        {
            return await Result<Tuple<List<EmailAccountViewModel>, int>>.SuccessAsync(await _emailAccountService.SearchAsync(emailAccountRequestModel), "Result Found");
        }
        [Authorize(Permissions.EmailAccounts.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] EmailAccountCreationDto emailAccountCreationDto)
        {
            var emailAccountId = await _emailAccountService.AddAsync(emailAccountCreationDto);
            return await Result<Guid>.SuccessAsync(emailAccountId, "Email Account Added Successfully");
        }
        [Authorize(Permissions.EmailAccounts.Edit)]
        [HttpPost("/api/emailAccount/update")]
        public virtual async Task<Result> Put([FromBody] EmailAccountUpdateDto emailAccountUpdateDto)
        {
            var emailAccountId = await _emailAccountService.UpdateAsync(emailAccountUpdateDto);
            return await Result<Guid>.SuccessAsync(emailAccountId, "Email Account Updated Successfully");
        }
        [Authorize(Permissions.EmailAccounts.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var emailAccountId = await _emailAccountService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(emailAccountId, "Email Account Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(EmailAccountRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _emailAccountService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Display Name", "Email", "User Name", "Host", "Port", "Enable SSL", "Default Email Account" };
                List<float> columnWidths = new List<float> { 5f, 15f, 21f, 20f, 19f, 5f, 7f, 8f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var emailAccount in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        emailAccount?.DisplayName,
                        emailAccount?.Email,
                        emailAccount?.Username,
                        emailAccount?.Host,
                        emailAccount?.Port,
                        IsEnableSSL = emailAccount!.EnableSsl ? "Yes" : "No",
                        IsDefaultEmailAccount = emailAccount!.IsDefaultEmailAccount ? "Yes" : "No",
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Email Account List");
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
