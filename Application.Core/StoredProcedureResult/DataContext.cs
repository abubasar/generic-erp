using Application.Core.StoredProcedureResult;
using Microsoft.EntityFrameworkCore;

namespace Application.Core.Data
{
    public partial class DataContext : DbContext
    {
        public virtual DbSet<SPSupplierLedgerResult> SPSupplierLedgerResults { get; set; }
    }
}
