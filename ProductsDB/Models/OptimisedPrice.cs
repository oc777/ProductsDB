using System;

namespace ProductsDB.Models
{
    public class OptimisedPrice
    {
        public string CatalogEntryCode { get; set; }
        public string MarketId { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
