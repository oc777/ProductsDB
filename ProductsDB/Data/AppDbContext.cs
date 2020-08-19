using System;
using Microsoft.EntityFrameworkCore;
using ProductsDB.Models;

namespace ProductsDB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public DbSet<Market> Markets { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PriceDetail> PriceDetails { get; set; }
    }
}
