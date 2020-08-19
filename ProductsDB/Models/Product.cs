using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductsDB.Models
{
    public class Product
    {
        [Key]
        public string CatalogEntryCode { get; set; }

        public List<PriceDetail> PriceDetails { get; set; }
    }
}
