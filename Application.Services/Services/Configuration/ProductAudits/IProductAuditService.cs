using Application.Services.SearchRequestModels.Configuration;
using Application.Services.ViewModels.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Configuration.ProductAudits
{
    public interface IProductAuditService
    {
        Task<List<ProductAuditViewModel>> GetProductAuditData(ProductAuditRequestModel request);
    }
}
