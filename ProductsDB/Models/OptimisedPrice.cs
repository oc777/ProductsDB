using System;
using System.ComponentModel.DataAnnotations;

namespace ProductsDB.Models
{
    public class OptimisedPrice
    {
        public string CatalogEntryCode { get; set; }
        public string MarketId { get; set; }
        public string CurrencyCode { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime ValidFrom { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public DateTime? ValidUntil { get; set; }
        [DisplayFormat(DataFormatString = "{0:#.00}")]
        public decimal UnitPrice { get; set; }
    }
}
