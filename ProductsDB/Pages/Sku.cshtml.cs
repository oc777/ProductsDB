using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ProductsDB.Pages
{
    public class SkuModel : PageModel
    {
        public string ProductId { get; set; }

        public bool ShowProduct => !string.IsNullOrEmpty(ProductId);

        private readonly ILogger<SkuModel> _logger;

        public SkuModel(ILogger<SkuModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string id)
        {
            ProductId = id;
        }
    }
}
