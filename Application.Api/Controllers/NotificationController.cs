using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        public NotificationController(IWorkContext workContext, INotificationService notificationService)
        {
            _workContext = workContext;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<Result<List<Notification>>> GetAll()
        {
            var list = await _notificationService.GetNotificationsAsync(Guid.Parse(_workContext.GetUserId()));
            return await Result<List<Notification>>.SuccessAsync(list, "Result Found");
        }
    }
}
