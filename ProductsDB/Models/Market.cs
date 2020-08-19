using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductsDB.Models
{
    public class Market
    {
        [Key]
        public string MarketId { get; set; }

        public List<PriceDetail> PriceDetails { get; set; }
    }
}
