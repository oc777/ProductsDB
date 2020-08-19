using System;
using System.Collections.Generic;

namespace ProductsDB.Models
{
    public class PriceDetail
    {
        public int PriceValueId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public string CatalogEntryCode { get; set; }
        public Product Product { get; set; }

        public string MarketId { get; set; }
        public Market Market { get; set; }

        public string CurrencyCode { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
