using System;
using System.Collections.Generic;

namespace ProductsDB.Models
{
    public class Market
    {
        public string MarketId { get; set; }

        public List<PriceDetail> PriceDetails { get; set; }
    }
}
