using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class SkuModel : PageModel
    {
        public AppDbContext _context;
        public string ProductId { get; set; }
        public bool ShowProduct { get; set; }
        //=> !string.IsNullOrEmpty(ProductId);

        public IQueryable<PriceDetail> ProductPrices { get; set; }
        public List<OptimisedPrice> OptimisedPrices { get; set; }

        private readonly ILogger<SkuModel> _logger;

        public SkuModel(ILogger<SkuModel> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void OnGet(string id)
        {
            ProductId = id;
            ShowProduct = _context.PriceDetails.Any(e => e.CatalogEntryCode == id);

            if (ShowProduct)
            {
                // get price details for the product and sort by market->currency->date
                ProductPrices = _context.PriceDetails.AsNoTracking()
                    .Where(p => p.CatalogEntryCode == ProductId)
                    .OrderBy(p => p.MarketId)
                    .ThenBy(p => p.CurrencyCode)
                    .ThenBy(p => p.ValidUntil);

                calculateOptimisedPrices(ProductPrices.ToList());
            }
           
        }

        private void calculateOptimisedPrices(List<PriceDetail> prices)
        {
            // list to hold optimised items
            OptimisedPrices = new List<OptimisedPrice>();
            // index for traversing PriceDetail list
            int i = 0;
            // previous and current items for comparesment
            PriceDetail previous = null;
            PriceDetail current = null;
            // product's main price for country-currency
            decimal basePrice = 0;

            // go through PriceDetail list
            while (i < prices.Count())
            {
                // get the item to compare
                current = prices[i];


                if (previous is null)
                {
                    // start of the list
                    // convert first item to optimised
                    DateTime until = (DateTime)(current.ValidUntil is null ? prices[i + 1].ValidFrom : current.ValidUntil);

                    var item = CreateOptimisedPriceItem(
                        current.MarketId,
                        current.CurrencyCode,
                        current.CatalogEntryCode,
                        current.ValidFrom,
                        until,
                        current.UnitPrice
                        );

                    OptimisedPrices.Add(item);

                    basePrice = current.UnitPrice;
                    current.ValidUntil = until;
                    previous = current;
                    i++;
                }

                else if (current.MarketId != previous.MarketId || current.CurrencyCode != previous.CurrencyCode)
                {
                    // new market-currency block started
                    // 'close' previous block with base price
                    var item = CreateOptimisedPriceItem(
                        previous.MarketId,
                        previous.CurrencyCode,
                        previous.CatalogEntryCode,
                        (DateTime) previous.ValidUntil,
                        (DateTime?)null,
                        basePrice
                        );

                    OptimisedPrices.Add(item);

                    basePrice = current.UnitPrice;
                    if (i != prices.Count()-1)
                    {
                        DateTime until = (DateTime)(current.ValidUntil is null ? prices[i + 1].ValidFrom : current.ValidUntil);
                        current.ValidUntil = until;
                    }
                    
                    previous = current;
                    i++;
                }
                 
                else
                {
                    // manage dates that are null
                    //var previousUntill = (DateTime)(current.ValidUntil is null ? prices[i + 1].ValidFrom : current.ValidUntil);
                    //var crentUntill = (DateTime)(current.ValidUntil is null ? prices[i + 1].ValidFrom : current.ValidUntil);
                    // compare validity time periods
                    //if (DateTime.Compare(current.ValidFrom, previous.ValidUntil) < 0)

                    var compare = DateTime.Compare((DateTime)previous.ValidUntil, current.ValidFrom);

                    if (compare == 0) 
                    {
                        // dates are the same
                        // convert item into optimised 'as is'
                        var item = CreateOptimisedPriceItem(
                            previous.MarketId,
                            previous.CurrencyCode,
                            previous.CatalogEntryCode,
                            previous.ValidFrom,
                            (DateTime)previous.ValidUntil,
                            basePrice
                            );

                        OptimisedPrices.Add(item);
                        previous = current;
                    }

                    if (compare < 0)
                    {
                        // there is a gap between two periods
                        // fill it with 'base price'

                        var itemPrevious = CreateOptimisedPriceItem(
                            previous.MarketId,
                            previous.CurrencyCode,
                            previous.CatalogEntryCode,
                            previous.ValidFrom,
                            (DateTime)previous.ValidUntil,
                            previous.UnitPrice
                            );

                        OptimisedPrices.Add(itemPrevious);

                        var itemGap = CreateOptimisedPriceItem(
                            previous.MarketId,
                            previous.CurrencyCode,
                            previous.CatalogEntryCode,
                            (DateTime)previous.ValidUntil,
                            current.ValidFrom,
                            basePrice
                            );

                        OptimisedPrices.Add(itemGap);

                        previous = current;
                    }

                    if (compare > 0)
                    {
                        // periods are overlaping
                        var compareT = DateTime.Compare((DateTime)previous.ValidUntil, (DateTime)current.ValidUntil);

                        if (compareT < 0 || compareT == 0)
                        {
                            // previous 'until' is within current range
                            // OR both end at the same time

                            // prev price < curr price ?
                            // make prev range 'longer'
                            if (previous.UnitPrice < current.UnitPrice)
                            {
                                previous.ValidUntil = current.ValidUntil;
                            }


                            // prev price > curr price ?
                            // cut prev range
                            if (previous.UnitPrice > current.UnitPrice)
                            { 
                                var item = CreateOptimisedPriceItem(
                                    previous.MarketId,
                                    previous.CurrencyCode,
                                    previous.CatalogEntryCode,
                                    previous.ValidFrom,
                                    current.ValidFrom,
                                    previous.UnitPrice
                                    );

                                OptimisedPrices.Add(item);
                                previous = current;
                            }


                        }

                        if (compareT > 0)
                        {
                            // current range is inside previous range

                            // prev price < curr price ?
                            // disregard current range

                            // prev price > curr price ?
                            // create three ranges: previous, overlap, current
                            // assign curr price to overlap range
                            if (previous.UnitPrice > current.UnitPrice)
                            {
                                var itemOne = CreateOptimisedPriceItem(
                                    previous.MarketId,
                                    previous.CurrencyCode,
                                    previous.CatalogEntryCode,
                                    previous.ValidFrom,
                                    current.ValidFrom,
                                    previous.UnitPrice
                                    );
                                OptimisedPrices.Add(itemOne);

                                var itemTwo = CreateOptimisedPriceItem(
                                    current.MarketId,
                                    current.CurrencyCode,
                                    current.CatalogEntryCode,
                                    current.ValidFrom,
                                    current.ValidUntil,
                                    current.UnitPrice
                                    );
                                OptimisedPrices.Add(itemTwo);

                                var itemThree = CreateOptimisedPriceItem(
                                    previous.MarketId,
                                    previous.CurrencyCode,
                                    previous.CatalogEntryCode,
                                    (DateTime)current.ValidUntil,
                                    previous.ValidUntil,
                                    previous.UnitPrice
                                    );
                                OptimisedPrices.Add(itemThree);

                                previous.ValidFrom = (DateTime)current.ValidUntil;
                            }
                            
                        }

                    }

                    i++;
                }



            }

            // Parse final items
            if (!(previous.ValidUntil is null))
            {
                var preFinalItem = CreateOptimisedPriceItem(
                    previous.MarketId,
                    previous.CurrencyCode,
                    previous.CatalogEntryCode,
                    previous.ValidFrom,
                    previous.ValidUntil,
                    previous.UnitPrice
                    );
                OptimisedPrices.Add(preFinalItem);

                var finalItem = CreateOptimisedPriceItem(
                    previous.MarketId,
                    previous.CurrencyCode,
                    previous.CatalogEntryCode,
                    (DateTime)previous.ValidUntil,
                    (DateTime?)null,
                    basePrice
                    );
                OptimisedPrices.Add(finalItem);
            }
            else
            {
                var preFinalItem = CreateOptimisedPriceItem(
                    previous.MarketId,
                    previous.CurrencyCode,
                    previous.CatalogEntryCode,
                    previous.ValidFrom,
                    (DateTime?)null,
                    previous.UnitPrice
                    );
                OptimisedPrices.Add(preFinalItem);
            }


        }

        private OptimisedPrice CreateOptimisedPriceItem(string market, string currency, string id, DateTime from, DateTime? till, decimal price)
        {
            OptimisedPrice item = new OptimisedPrice();
            item.MarketId = market;
            item.CurrencyCode = currency;
            item.CatalogEntryCode = id;
            item.ValidFrom = from;
            item.ValidUntil = till;
            item.UnitPrice = price;

            return item;
        }
    }
}
