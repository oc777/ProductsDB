using System;
using Microsoft.EntityFrameworkCore;
using ProductsDB.Models;
using ProductsDB;

namespace ProductsDB.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        //public AppDbContext()
        {
        }

        public DbSet<Market> Markets { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PriceDetail> PriceDetails { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    // TODO: put connection string in config
        //    optionsBuilder.UseSqlite("Data Source=products.db");
        //    base.OnConfiguring(optionsBuilder);
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Market>().ToTable("Market");
            modelBuilder.Entity<Product>().ToTable("Product");
            modelBuilder.Entity<PriceDetail>().ToTable("PriceDetail");
        }
    }
}
