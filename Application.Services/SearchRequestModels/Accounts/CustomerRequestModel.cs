using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class CustomerRequestModel : BaseRequestModel<Account>
    {
        public Guid? CustomerRegionId { get; set; }
        public Guid? CustomerZoneId { get; set; }
        public Guid? CustomerAreaId { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public override Expression<Func<Account, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword) || x.Code.Contains(Keyword);
            if (CustomerRegionId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerRegionId == CustomerRegionId.Value);
            if (CustomerZoneId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerZoneId == CustomerZoneId.Value);
            if (CustomerAreaId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerAreaId == CustomerAreaId.Value);
            if (CustomerTerritoryId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerTerritoryId == CustomerTerritoryId.Value);
            if (CustomerMarketingOfficerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerMarketingOfficerId == CustomerMarketingOfficerId.Value);
            return ExpressionObject;
        }
        public override IQueryable<Account> IncludeParents(IQueryable<Account> queryable)
        {
            return queryable.Include(x => x.BankAccounts).Include(x => x.CustomerMarketingOfficer);
        }
    }
}