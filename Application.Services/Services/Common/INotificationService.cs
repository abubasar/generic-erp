using Application.Core.Entities;
using Application.Core.Interfaces;

namespace Application.Services.Services.Common
{
    public interface INotificationService
    {
        Task SendNotificationAsync(IUnitOfWork unitOfWork, Guid guidId, string route, string permission, string message, int status = 0);
        Task RemoveNotificationAsync(IUnitOfWork unitOfWork, Guid guidId);
        Task<List<Notification>> GetNotificationsAsync(Guid userId);


    }
}
