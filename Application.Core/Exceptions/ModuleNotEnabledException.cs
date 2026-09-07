namespace Application.Core.Exceptions
{
    /// <summary>
    /// A platform module required by the endpoint is not switched on for the
    /// current tenant. Maps to HTTP 403 — "not on your plan".
    /// </summary>
    public class ModuleNotEnabledException : Exception
    {
        public ModuleNotEnabledException(string message) : base(message)
        {
        }
    }
}
