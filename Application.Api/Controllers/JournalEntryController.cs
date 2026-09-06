using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.JournalEntry;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.JournalEntries;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JournalEntryController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJournalEntryService _journalEntryService;
        public JournalEntryController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IJournalEntryService journalEntryService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _journalEntryService = journalEntryService;
        }
        [Authorize(Permissions.JournalEntries.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<JournalEntryViewModel>, int>>> Search(JournalEntryRequestModel request)
        {
            return await Result<Tuple<List<JournalEntryViewModel>, int>>.SuccessAsync(await _journalEntryService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.JournalEntries.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<JournalEntryViewModel>> GetById(Guid id)
        {
            return await Result<JournalEntryViewModel>.SuccessAsync(await _journalEntryService.GetByIdAsync(id), "Result Found");
        }
        [Authorize(Permissions.JournalEntries.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _journalEntryService.GetPendingCheckedCountAsync(), "Success");
        }
        [Authorize(Permissions.JournalEntries.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] JournalEntryCreationDto journalEntryCreationDto)
        {
            var journalEntryId = await _journalEntryService.AddAsync(journalEntryCreationDto);
            return await Result<Guid>.SuccessAsync(journalEntryId, "Journal Entry Added Successfully");
        }
        [Authorize(Permissions.JournalEntries.Edit)]
        [HttpPost("/api/journalEntry/update")]
        public virtual async Task<Result> Put([FromBody] JournalEntryUpdateDto journalEntryUpdateDto)
        {
            var journalEntryId = await _journalEntryService.UpdateAsync(journalEntryUpdateDto);
            return await Result<Guid>.SuccessAsync(journalEntryId, "Journal Entry Updated Successfully");
        }
        [Authorize(Permissions.JournalEntries.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var journalEntryId = await _journalEntryService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(journalEntryId, "Journal Entry Deleted Successfully");
        }
        [Authorize(Permissions.JournalEntries.Check)]
        [HttpPost("/api/journalEntry/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _journalEntryService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)JournalEntryStatus.Checked, "Journal Entry Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }
        [Authorize(Permissions.JournalEntries.Approve)]
        [HttpPost("/api/journalEntry/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _journalEntryService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)JournalEntryStatus.Approved, "Journal Entry Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }
        [Authorize(Permissions.JournalEntries.Unpost)]
        [HttpPost("/api/journalEntry/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _journalEntryService.UnpostAsync(id, fromStatus), "Journal Entry Unposted Successfully");
        }
    }
}
