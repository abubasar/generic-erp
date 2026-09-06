namespace Application.Core.Interfaces
{
    public interface ISmsService
    {
        Task Send(string mobileNumber, string message);
    }
}
