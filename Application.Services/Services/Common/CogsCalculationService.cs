using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Common
{
    public class CogsCalculationService:ICogsCalculationService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CogsCalculationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public (decimal cogs, List<Stock> stocks) CalculateCOGS(List<int> transactionTypes, Product product, Guid storeId, decimal outQuantity)
        {
            List<Stock> stocks = new();
            var neededItems = outQuantity;
            var itemsCalculated = 0.0M;
            var currentIndex = 0;
            var COGS_total = 0.0M;
            var stockList = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => transactionTypes.Contains(x.TransactonType) && x.StoreId == storeId && x.ProductId == product.Id && x.AvailableQty != 0)
                     .OrderBy(x => x.StockInDate).ToArray();
            if (!stockList.Any()) throw new NotFoundResultException($"{product.Name} is not available in stock.");
            var availableStock = stockList.Sum(x => x.AvailableQty);
            if (availableStock < neededItems) throw new BadRequestException($"Available stock quantity for {product.Name} is {availableStock.ToString("#,##0.000")} and needed quantity is {neededItems.ToString("#,##0.000")}");
            while (neededItems != itemsCalculated)
            {
                var stock = stockList[currentIndex];
                if (stock.AvailableQty > (neededItems - itemsCalculated))
                {
                    var used = (neededItems - itemsCalculated);
                    COGS_total = COGS_total + used * (stock.InRate + stock.InTransportCost);
                    stock.AvailableQty = stock.AvailableQty - used;
                    itemsCalculated = itemsCalculated + used;
                }
                else if (stock.AvailableQty == (neededItems - itemsCalculated))
                {
                    COGS_total = COGS_total + stock.AvailableQty * (stock.InRate + stock.InTransportCost);
                    itemsCalculated += stock.AvailableQty;
                    stock.AvailableQty = 0;
                }
                else if (stock.AvailableQty < (neededItems - itemsCalculated))
                {
                    COGS_total = COGS_total + stock.AvailableQty * (stock.InRate + stock.InTransportCost);
                    itemsCalculated += stock.AvailableQty;
                    stock.AvailableQty = 0;
                }

                stocks.Add(stock);
                currentIndex++;
            }

            return (COGS_total, stocks);
        }
    }
}
