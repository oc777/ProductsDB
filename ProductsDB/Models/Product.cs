using System;
using System.Collections.Generic;

namespace ProductsDB.Models
{
    public class Product
    {
        public string CatalogEntryCode { get; set; }

        public List<PriceDetail> PriceDetails { get; set; }
    }
}
