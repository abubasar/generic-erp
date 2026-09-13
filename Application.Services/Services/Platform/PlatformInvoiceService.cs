using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Entities.Platform;
using Application.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Platform
{
    public interface IPlatformInvoiceService
    {
        Task<IReadOnlyList<PlatformInvoiceDto>> ListAsync(Guid tenantId);
        Task<PlatformInvoiceDto> CreateAsync(Guid tenantId, CreateInvoiceRequest request);
        Task<PlatformInvoiceDto> MarkPaidAsync(Guid invoiceId);
        Task<PlatformInvoiceDto> VoidAsync(Guid invoiceId);
    }

    /// <summary>
    /// Manual invoicing — a platform admin records what was billed and later marks it
    /// paid. No automated generation, no line items; see PlatformInvoice's own doc
    /// comment for why this is deliberately not the Phase 4 TenantInvoice.
    /// </summary>
    public sealed class PlatformInvoiceService : IPlatformInvoiceService
    {
        private readonly DataContext _db;
        private readonly IPlatformAuditWriter _audit;

        public PlatformInvoiceService(DataContext db, IPlatformAuditWriter audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task<IReadOnlyList<PlatformInvoiceDto>> ListAsync(Guid tenantId)
        {
            var invoices = await _db.PlatformInvoices.Where(i => i.TenantId == tenantId)
                .OrderByDescending(i => i.IssuedOn)
                .ToListAsync();
            return invoices.Select(ToDto).ToList();
        }

        public async Task<PlatformInvoiceDto> CreateAsync(Guid tenantId, CreateInvoiceRequest request)
        {
            if (!await _db.Set<Tenant>().IgnoreQueryFilters().AnyAsync(t => t.Id == tenantId && !t.Deleted))
                throw new BadRequestException("Tenant not found.");
            if (request.Amount <= 0)
                throw new BadRequestException("Amount must be greater than zero.");
            if (request.PeriodEnd < request.PeriodStart)
                throw new BadRequestException("Period end must be on or after period start.");

            var now = DateTime.UtcNow;
            var invoice = new PlatformInvoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Number = await GenerateNumberAsync(now),
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                Amount = request.Amount,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BDT" : request.Currency.Trim().ToUpperInvariant(),
                Status = "Unpaid",
                IssuedOn = now,
                DueOn = request.DueOn,
                Note = request.Note,
            };
            _db.PlatformInvoices.Add(invoice);

            _audit.Add("invoice.create", tenantId, $"number={invoice.Number}; amount={invoice.Amount} {invoice.Currency}");
            await _db.SaveChangesAsync();
            return ToDto(invoice);
        }

        public async Task<PlatformInvoiceDto> MarkPaidAsync(Guid invoiceId)
        {
            var invoice = await Find(invoiceId);
            if (invoice.Status == "Void")
                throw new BadRequestException("Cannot mark a void invoice as paid.");

            invoice.Status = "Paid";
            invoice.PaidOn = DateTime.UtcNow;

            _audit.Add("invoice.paid", invoice.TenantId, $"number={invoice.Number}");
            await _db.SaveChangesAsync();
            return ToDto(invoice);
        }

        public async Task<PlatformInvoiceDto> VoidAsync(Guid invoiceId)
        {
            var invoice = await Find(invoiceId);
            if (invoice.Status == "Paid")
                throw new BadRequestException("Cannot void a paid invoice.");

            invoice.Status = "Void";

            _audit.Add("invoice.void", invoice.TenantId, $"number={invoice.Number}");
            await _db.SaveChangesAsync();
            return ToDto(invoice);
        }

        private async Task<PlatformInvoice> Find(Guid invoiceId) =>
            await _db.PlatformInvoices.FirstOrDefaultAsync(i => i.Id == invoiceId)
                ?? throw new BadRequestException("Invoice not found.");

        private async Task<string> GenerateNumberAsync(DateTime now)
        {
            var year = now.Year;
            var prefix = $"INV-{year}-";
            var n = await _db.PlatformInvoices.CountAsync(i => i.Number.StartsWith(prefix)) + 1;
            var number = $"{prefix}{n:0000}";
            // Extremely unlikely collision (concurrent create in the same instant) — retry linearly.
            while (await _db.PlatformInvoices.AnyAsync(i => i.Number == number))
                number = $"{prefix}{++n:0000}";
            return number;
        }

        private static PlatformInvoiceDto ToDto(PlatformInvoice i) => new(
            i.Id, i.TenantId, i.Number, i.PeriodStart, i.PeriodEnd,
            i.Amount, i.Currency, i.Status, i.IssuedOn, i.DueOn, i.PaidOn, i.Note);
    }
}
