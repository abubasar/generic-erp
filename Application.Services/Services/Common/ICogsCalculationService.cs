using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Common
{
    public interface ICogsCalculationService
    {
        public (decimal cogs, List<Stock> stocks) CalculateCOGS(List<int> transactionTypes, Product product, Guid storeId, decimal outQuantity);
    }
}
