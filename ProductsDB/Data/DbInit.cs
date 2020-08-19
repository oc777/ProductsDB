using System;
using System.Linq;

namespace ProductsDB.Data
{
    public static class DbInit
    {
        
        public static void Init(AppDbContext context)
        {
            //var context = new AppDbContext();

            // delete & create DB schema
            // context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // seed db if empty
            if (context.Markets.Any())
            {
                return;
            }

            Console.WriteLine("tada");
        }

    }
}
