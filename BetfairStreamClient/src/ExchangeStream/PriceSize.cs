using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.ExchangeStream
{
    public class PriceSize
    {
        public double Price { get; }
        public double Size { get; }

        public PriceSize(double price, double size)
        {            
            Price = price;
            Size = size;
        }
    }
}
