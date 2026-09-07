namespace Application.Services.Services.Platform
{
    /// <summary>Input to a provisioning run. Password is generated when omitted and returned once.</summary>
    public sealed record ProvisionRequest(string? OwnerUsername, string? OwnerEmail, string? OwnerPassword);

    public sealed record ProvisioningStepStatus(string StepKey, string Status, string? Error, int Attempts, DateTime? CompletedOn);

    public sealed record ProvisioningResult(
        Guid TenantId,
        bool Complete,
        string? OwnerUsername,
        string? OwnerTempPassword,
        IReadOnlyList<ProvisioningStepStatus> Steps);
}
