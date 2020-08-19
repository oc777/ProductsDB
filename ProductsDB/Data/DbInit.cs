using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using ProductsDB.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductsDB.Data
{
    public static class DbInit
    {

        //public static void Init(AppDbContext context)
        public static void Init()
        {
            var context = new AppDbContext();

            // delete & create DB schema
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // seed db if empty
            if (context.Markets.Any())
            {
                // DB has been seeded
                return;
            }

            Console.WriteLine("tada");

            var CsvPath = Path.Combine("Data", "SeedData", "price_detail.csv");
            List<PriceDetail> prices = new List<PriceDetail>();
            List<Product> products = new List<Product>();
            List<Market> markets = new List<Market>();

            // parse file using CSVHelper
            using (var stream = File.OpenRead(CsvPath))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Configuration.Delimiter = "\t";
                csv.Configuration.TypeConverterOptionsCache.GetOptions<DateTime?>().NullValues.Add("NULL");

                prices = csv.GetRecords<PriceDetail>().ToList();

                // go to the begining of stream
                stream.Position = 0;
                products = csv.GetRecords<Product>().ToList();

                stream.Position = 0;
                markets = csv.GetRecords<Market>().ToList();
            }

            Console.WriteLine(prices.Count);
            Console.WriteLine(products.Count);
            Console.WriteLine(markets.Count);

            // bulk insert into tables
            context.BulkInsert(products, options =>
            {
                // skip duplicates
                options.InsertIfNotExists = true;
                // suppose to speed up the process a bit?
                options.AutoMapOutputDirection = false;
            });

            context.BulkInsert(markets, options =>
            {
                options.InsertIfNotExists = true;
                options.AutoMapOutputDirection = false;
            });

            context.BulkInsert(prices, options =>
            {
                // insery the PK as in csv file
                options.InsertKeepIdentity = true;
                options.AutoMapOutputDirection = false;
            });

        }

    }
}
