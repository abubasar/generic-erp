using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Configuration
{
    public class ProductAuditViewModel
    {
        public Guid ProductId { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? PropertyName { get; set; }

        public Guid InventoryTypeId { get; set; }

        public string? InventoryTypeName { get; set; }

        public Guid ProductTypeId { get; set; }

        public string? ProductTypeName { get; set; }

        public Guid? GenericId { get; set; }

        public string? GenericName { get; set; }

        public string? Composition { get; set; }

        public Guid? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public Guid? PackSizeId { get; set; }

        public string? PackSizeName { get; set; }

        public Guid? CountryId { get; set; }

        public string? CountryName { get; set; }

        public bool IsPurchaseProduct { get; set; }

        public bool IsSaleProduct { get; set; }

        public Guid MeasurementUnitId { get; set; }

        public string? MeasurementUnitName { get; set; }

        public decimal Mrp { get; set; }

        public decimal SalePrice { get; set; }

        public decimal PurchasePrice { get; set; }

        public int AlertQuantity { get; set; }

        public decimal LastPurchaseRate { get; set; }

        public int BagWeight { get; set; }

        public decimal VatPercentage { get; set; }

        public Guid? ManufacturerId { get; set; }

        public string? ManufacturerName { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public bool DeletedHistory { get; set; }

        public Guid TenantId { get; set; }

        public string? ActionType { get; set; }

        public DateTime? ActionDate { get; set; }

        public int Id { get; set; }

        public virtual CategoryViewModel? Category { get; set; }

        public virtual CountryViewModel? Country { get; set; }

        public virtual GenericViewModel? Generic { get; set; }

        public virtual InventoryTypeViewModel? InventoryType { get; set; }

        public virtual ManufacturerViewModel? Manufacturer { get; set; }

        public virtual MeasurementUnitViewModel? MeasurementUnit { get; set; }

        public virtual PackSizeViewModel? PackSize { get; set; }

        public virtual ProductTypeViewModel? ProductType { get; set; }
    }
}
