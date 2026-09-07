namespace Application.Core.Interfaces
{
    /// <summary>
    /// Marks an entity as belonging to a single tenant. Every <see cref="ITenantScoped"/>
    /// entity gets an automatic EF Core query filter (named "TenantFilter") on
    /// <see cref="TenantId"/>, and <see cref="IUnitOfWork"/> stamps / verifies the
    /// value on save. Do not read or write a scoped entity without going through
    /// the repository.
    /// </summary>
    public interface ITenantScoped
    {
        Guid TenantId { get; set; }
    }
}
