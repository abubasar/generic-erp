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
            var hasTenantId = Guid.TryParse(_contextAccessor.HttpContext?.Items["TenantId"]?.ToString(), out Guid tenantId);
            if (username is null || !hasTenantId) throw new UnauthorizationException("Unauthorized");
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
                switch (entityEntry?.State)
                {
                    case EntityState.Added:
                        entityEntry.Property("TenantId").CurrentValue = tenantId;
                        entityEntry.Property("CreatedOn").CurrentValue = DateTime.UtcNow;
                        entityEntry.Property("CreatedBy").CurrentValue = username;
                        entityEntry.Property("UpdatedOn").CurrentValue = DateTime.UtcNow;
                        entityEntry.Property("UpdatedBy").CurrentValue = username;
                        eventLog.EventDescription = "Added";
                        break;
                    case EntityState.Modified:
                        entityEntry.Property("TenantId").CurrentValue = tenantId;
                        entityEntry.Property("UpdatedOn").CurrentValue = DateTime.UtcNow;
                        entityEntry.Property("UpdatedBy").CurrentValue = username;
                        eventLog.OldValues = JsonConvert.SerializeObject(entityEntry.GetDatabaseValues()?.ToObject());
                        eventLog.EventDescription = "Modified";
                        break;
                    case EntityState.Deleted:
                        if (hardDeleteEntityTypes.Contains(entityEntry.Entity.GetType()))
                        {
                            _context.Remove(entityEntry.Entity);
                        }
                        else
                        {
                            entityEntry.State = EntityState.Modified; // Soft delete by marking as modified
                            entityEntry.Property("UpdatedOn").CurrentValue = DateTime.UtcNow;
                            entityEntry.Property("UpdatedBy").CurrentValue = username;
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
    }
}




