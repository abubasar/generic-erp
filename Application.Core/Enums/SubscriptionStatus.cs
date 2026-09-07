namespace Application.Core.Enums
{
    public enum SubscriptionStatus
    {
        /// <summary>No subscription row resolved (unauthenticated, or not yet provisioned).</summary>
        None = 0,
        Trial = 1,
        Active = 2,
        PastDue = 3,
        Suspended = 4,
        Cancelled = 5,
    }
}
