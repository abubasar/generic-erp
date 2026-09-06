using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class ProductRequestModel : BaseRequestModel<Product>
    {
        public int ReportType { get; set; }
        public Guid? ProductTypeId { get; set; }
        public Guid? InventoryTypeId { get; set; }
        public Guid? GenericId { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? PackSizeId { get; set; }
        public bool IsPurchaseProduct { get; set; }
        public bool IsSaleProduct { get; set; }
        public List<Guid> InventoryTypeIds { get; set; } = new();
        public override Expression<Func<Product, bool>> GetExpression()
        {

            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.StartsWith(Keyword) || x.Code.Contains(Keyword);
            if (InventoryTypeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.InventoryTypeId == InventoryTypeId);
            if (GenericId.HasValue) ExpressionObject = ExpressionObject.And(x => x.GenericId == GenericId);
            if (CountryId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CountryId == CountryId);
            if (CategoryId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CategoryId == CategoryId);
            if (PackSizeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.PackSizeId == PackSizeId);
            if (ProductTypeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.ProductTypeId == ProductTypeId);
            if (IsPurchaseProduct) ExpressionObject = ExpressionObject.And(x => x.IsPurchaseProduct == IsPurchaseProduct);
            if (IsSaleProduct) ExpressionObject = ExpressionObject.And(x => x.IsSaleProduct == IsSaleProduct);
            if (InventoryTypeIds.Any()) ExpressionObject = ExpressionObject.And(x => InventoryTypeIds.Contains(x.InventoryTypeId));
            return ExpressionObject;
        }

        public override IQueryable<Product> IncludeParents(IQueryable<Product> queryable)
        {
            return queryable.Include(x => x.InventoryType).Include(x => x.ProductType).Include(x => x.MeasurementUnit).Include(x => x.Generic).Include(x => x.Manufacturer).Include(x => x.Country).Include(x => x.Category).Include(x => x.PackSize);
        }
    }
}
