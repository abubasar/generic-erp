using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Core.Common
{
   public class VatCalculator
    {
        public static  decimal CalculateVat(decimal vatPercentage, decimal salePrice)
        {
            var perc = 1 + (vatPercentage / 100);
            var a = (salePrice / perc);
            return (salePrice - a);
        }
    }
}
