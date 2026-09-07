using Application.Core.Common;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Application.Infrastructure

{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        public UnitOfWork(DataContext context, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _contextAccessor = contextAccessor;
        }
        public async Task<bool> SaveChangesAsync()
        {
            bool returnValue = true;
            try
            {
                await OnBeforeSaveChangesAsync();
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                //Log Exception Handling message                      
                returnValue = false;
                throw;
            }

            return returnValue;
        }
        public async Task<bool> RefreshTokenSaveChangesAsync()
        {
            bool returnValue = true;
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                }

                catch (Exception)
                {
                    //Log Exception Handling message                      
                    returnValue = false;
                    await dbContextTransaction.RollbackAsync();
                    throw;

                }
            }

            return returnValue;
        }

        public async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public IBaseRepository<T> Repository<T>() where T : class
        {
            return new BaseRepository<T>(_context);
        }



        private async Task OnBeforeSaveChangesAsync()
        {
            _context.ChangeTracker.DetectChanges();
            var username = _contextAccessor.HttpContext?.Items["UserName"]?.ToString();
            var tenantId = TenantScope.CurrentTenantId;
            if (username is null || tenantId == Guid.Empty) throw new UnauthorizationException("Unauthorized");
            var entries = _context.ChangeTracker.Entries().Where(e =>
            e.State == EntityState.Added
           || e.State == EntityState.Modified
           || e.State == EntityState.Deleted);
            var auditEntries = new List<Application.Core.Entities.EventLog>();
            // List of entity types to hard delete
            var hardDeleteEntityTypes = new List<Type>
                {
                    typeof(Transaction),
                    typeof(Stock),
                    typeof(SupplierTransactionAgainstPo),
                    typeof(Notification),
                    typeof(RoleClaim),
                    typeof(PurchaseInvoiceSupplierPaymentMapping),
                    typeof(ReceivePaymentAgainstSaleSaleInvoiceMapping),
                    typeof(PurchaseRequisitionPurchaseOrderMapping)
                    // Add other entity types as needed
                };
            foreach (var entityEntry in entries)
            {
                var hasEntityId = Guid.TryParse(entityEntry?.Property("Id")?.CurrentValue?.ToString(), out Guid entityId);
                var eventLog = new Application.Core.Entities.EventLog();
                eventLog.Id = Guid.NewGuid();
                eventLog.TableName = entityEntry?.Entity.GetType().Name;
                eventLog.CreatedOn = DateTime.UtcNow;
                eventLog.CreatedBy = username;
                eventLog.TenantId = tenantId;
                eventLog.EntityId = entityId;
                eventLog.NewValues = JsonConvert.SerializeObject(entityEntry?.CurrentValues?.ToObject());
                var isTenantScoped = entityEntry?.Entity is ITenantScoped;
                // A row already carrying the shared sentinel (the CoA skeleton) is
                // owned by no tenant — never re-stamp it or guard it as cross-tenant.
                var isSharedRow = isTenantScoped
                    && entityEntry!.Entity is ITenantSharable
                    && (Guid?)entityEntry.Property("TenantId").CurrentValue == TenancyConstants.SystemTenantId;
                switch (entityEntry?.State)
                {
                    case EntityState.Added:
                        if (isTenantScoped && !isSharedRow)
                            entityEntry.Property("TenantId").CurrentValue = tenantId;
                        SetIfPresent(entityEntry, "CreatedOn", DateTime.UtcNow);
                        SetIfPresent(entityEntry, "CreatedBy", username);
                        SetIfPresent(entityEntry, "UpdatedOn", DateTime.UtcNow);
                        SetIfPresent(entityEntry, "UpdatedBy", username);
                        eventLog.EventDescription = "Added";
                        break;
                    case EntityState.Modified:
                        {
                            var dbValues = entityEntry.GetDatabaseValues();
                            GuardTenantOwnership(isTenantScoped, dbValues, tenantId, entityEntry.Entity.GetType().Name);
                            if (isTenantScoped && !isSharedRow)
                                entityEntry.Property("TenantId").CurrentValue = tenantId;
                            SetIfPresent(entityEntry, "UpdatedOn", DateTime.UtcNow);
                            SetIfPresent(entityEntry, "UpdatedBy", username);
                            eventLog.OldValues = JsonConvert.SerializeObject(dbValues?.ToObject());
                            eventLog.EventDescription = "Modified";
                            break;
                        }
                    case EntityState.Deleted:
                        // OriginalValues (not a DB round-trip): deletes come from the
                        // tenant-filtered TableNoTracking(), so this is already the DB value.
                        GuardTenantOwnership(isTenantScoped, entityEntry.OriginalValues, tenantId, entityEntry.Entity.GetType().Name);
                        if (hardDeleteEntityTypes.Contains(entityEntry.Entity.GetType()))
                        {
                            _context.Remove(entityEntry.Entity);
                        }
                        else
                        {
                            entityEntry.State = EntityState.Modified; // Soft delete by marking as modified
                            SetIfPresent(entityEntry, "UpdatedOn", DateTime.UtcNow);
                            SetIfPresent(entityEntry, "UpdatedBy", username);
                            entityEntry.Property("Deleted").CurrentValue = true; // Set Deleted to true for deleted entities
                            eventLog.OldValues = JsonConvert.SerializeObject(entityEntry.OriginalValues.ToObject());
                            eventLog.EventDescription = "Deleted";
                        }
                        break;
                }
                auditEntries.Add(eventLog);
            }
            await _context.EventLogs.AddRangeAsync(auditEntries);
        }

        /// <summary>Sets an audit property only if the entity actually has it (Setting, Log, ProductAudit and EventLog do not).</summary>
        private static void SetIfPresent(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string property, object? value)
        {
            if (entry.Metadata.FindProperty(property) is not null)
                entry.Property(property).CurrentValue = value;
        }

        /// <summary>
        /// Fail closed on cross-tenant writes: an ITenantScoped row being modified or
        /// deleted must already belong to the current tenant. Catches update-by-Id /
        /// Attach against another tenant's row (which the read query filter cannot stop).
        /// </summary>
        private static void GuardTenantOwnership(bool isTenantScoped, Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues? databaseValues, Guid currentTenantId, string entityName)
        {
            if (!isTenantScoped || databaseValues is null) return;
            var storedTenantId = databaseValues.GetValue<Guid>("TenantId");
            // Guid.Empty = unstamped; SystemTenantId = shared CoA skeleton (owned by no tenant).
            if (storedTenantId == Guid.Empty || storedTenantId == TenancyConstants.SystemTenantId) return;
            if (storedTenantId != currentTenantId)
                throw new UnauthorizationException($"Cross-tenant write blocked on {entityName}.");
        }
    }
}




