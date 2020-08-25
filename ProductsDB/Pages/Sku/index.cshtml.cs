using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductsDB.Data;
using ProductsDB.Models;

namespace ProductsDB.Pages
{
    public class SkuIndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public AppDbContext _context;
        public IQueryable<Product> Products { get; set; }

        public SkuIndexModel(ILogger<IndexModel> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet()
        {
            // show list with 10 products
            Products = _context.Products.AsNoTracking().Take(10);
        }
    }
}
