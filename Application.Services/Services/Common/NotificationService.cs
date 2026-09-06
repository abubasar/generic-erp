using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Common
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        public NotificationService(IUnitOfWork unitOfWork, IHubContext<BroadcastHub, IHubClient> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task<List<Notification>> GetNotificationsAsync(Guid userId)
        {
            var notifications = await _unitOfWork.Repository<Notification>()
                .TableNoTracking().Where(x => !x.Deleted && x.UserId == userId).ToListAsync();
            return notifications;
        }

        public async Task SendNotificationAsync(IUnitOfWork unitOfWork, Guid guidId, string route, string permission, string message, int status = 0)
        {
            //uncomment for activate notification
            //var roleClaims = await _unitOfWork.Repository<RoleClaim>().TableNoTracking()
            //    .Where(x => x.Value == permission).ToListAsync();
            //if (roleClaims.Any())
            //{
            //    foreach (var roleClaim in roleClaims)
            //    {
            //        var users = await unitOfWork.Repository<User>().TableNoTracking()
            //            .Where(x => x.RoleId == roleClaim.RoleId).ToListAsync();
            //        foreach (var user in users)
            //        {
            //            var notification = new Notification
            //            {
            //                Id = Guid.NewGuid(),
            //                GuidId = guidId,
            //                Text = message,
            //                UserId = user.Id,
            //                Route = route,
            //                Status = status
            //            };
            //            await unitOfWork.Repository<Notification>().AddAsync(notification);
            //        }

            //    }
            //}
            //add this line when uncomment above code
            await Task.Run(() => { });
        }

        public async Task RemoveNotificationAsync(IUnitOfWork unitOfWork, Guid guidId)
        {
            var notification = await unitOfWork.Repository<Notification>().
                     TableNoTracking().FirstOrDefaultAsync(x => x.GuidId == guidId);
            if (notification is not null)
            {
                notification.Deleted = true;
                await _unitOfWork.Repository<Notification>().UpdateAsync(notification);
            }
        }


    }
}
