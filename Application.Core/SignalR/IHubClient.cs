namespace Application.Core.SignalR
{
    public interface IHubClient
    {
        Task BroadcastMessage();
    }
}
