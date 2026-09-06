using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.ViewModels.Configuration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Configuration.ProductAudits
{
    public class ProductAuditService : IProductAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductAuditService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductAuditViewModel>> GetProductAuditData(ProductAuditRequestModel request)
        {
            var queryable = _unitOfWork.Repository<ProductAudit>().TableNoTracking().Include(x => x.InventoryType).Include(x => x.ProductType).Include(x => x.Generic).Include(x => x.Category)
                .Include(x => x.PackSize).Include(x => x.Country).Include(x => x.MeasurementUnit).Include(x => x.Manufacturer).AsQueryable();
            if (request.FromDate.HasValue && request.ToDate.HasValue) queryable = queryable.Where(d => d.ActionDate!.Value.Date >= request.FromDate.Value.Date && d.ActionDate.Value.Date <= request.ToDate.Value);
            if (request.ProductId.HasValue) queryable = queryable.Where(d => d.ProductId == request.ProductId.Value);
            var productAudits = await queryable.Select(d => new ProductAuditViewModel
            {
                Id = d.Id,
                ProductId = d.ProductId,
                Code = d.Code,
                Name = d.Name,
                InventoryTypeId = d.InventoryTypeId,
                InventoryTypeName = d.InventoryType.Name,
                ProductTypeId = d.ProductTypeId,
                ProductTypeName = d.ProductType.Name,
                GenericId = d.GenericId,
                GenericName = d.Generic.Name,
                Composition = d.Composition,
                CategoryId = d.CategoryId,
                CategoryName = d.Category.Name,
                PackSizeId = d.PackSizeId,
                PackSizeName = d.PackSize.Name,
                CountryId = d.CountryId,
                CountryName = d.Country.Name,
                IsPurchaseProduct = d.IsPurchaseProduct,
                IsSaleProduct = d.IsSaleProduct,
                MeasurementUnitId = d.MeasurementUnitId,
                MeasurementUnitName = d.MeasurementUnit.Name,
                Mrp = d.Mrp,
                SalePrice = d.SalePrice,
                PurchasePrice = d.PurchasePrice,
                AlertQuantity = d.AlertQuantity,
                LastPurchaseRate = d.LastPurchaseRate,
                BagWeight = d.BagWeight,
                VatPercentage = d.VatPercentage,
                ManufacturerId = d.ManufacturerId,
                ManufacturerName = d.Manufacturer.Name,
                CreatedOn = d.CreatedOn,
                UpdatedOn = d.UpdatedOn,
                CreatedBy = d.CreatedBy,
                UpdatedBy = d.UpdatedBy,
                DeletedHistory = d.DeletedHistory,
                TenantId = d.TenantId,
                ActionType = d.ActionType,
                ActionDate = d.ActionDate
            }).OrderBy(x => x.Name).ThenBy(x => x.ActionDate).ToListAsync();

            return productAudits;
        }
    }
}
