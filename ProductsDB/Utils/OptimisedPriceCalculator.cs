using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProductsDB.Data;
using ProductsDB.Models;

namespace ProductsDB.Utils
{
    public static class OptimisedPriceCalculator
    {
        public static List<OptimisedPrice> GetList(string ProductId)
        {
            // list of PriceDetails for given product
            List<PriceDetail> ProductPrices = GetPriceDetails(ProductId);
            // list with optimised price timeline
            List<OptimisedPrice> OptimisedPrices = CreateOptimisedList(ProductPrices);

            return OptimisedPrices;
        }


        // Gets a list of all Price Details for given product
        private static List<PriceDetail> GetPriceDetails(string ProductId)
        {
            // list of PriceDetails for given product
            List<PriceDetail> ProductPrices = new List<PriceDetail>();

            // get price details and sort by market->currency->date
            using (var context = new AppDbContext())
            {
                ProductPrices = context.PriceDetails.AsNoTracking()
                    .Where(p => p.CatalogEntryCode == ProductId)
                    .OrderBy(p => p.MarketId)
                    .ThenBy(p => p.CurrencyCode)
                    .ThenBy(p => p.ValidUntil)
                    .ToList();
            }

            return ProductPrices;
        }


        // Creats and returns optimised list of product's price history
        private static List<OptimisedPrice> CreateOptimisedList(List<PriceDetail> prices)
        {
            List<OptimisedPrice> OptimisedPrices = new List<OptimisedPrice>();

            // group prices by market-currency
            var grouped = prices.GroupBy(p => new { p.MarketId, p.CurrencyCode });

            foreach (var group in grouped)
            {
                var groupLength = group.Count();

                // previous and current items for comparesment
                PriceDetail previous = group.ElementAt(0);
                PriceDetail current = null;

                // base price for each group
                var basePrice = previous.UnitPrice;
                

                if (groupLength == 1)
                {
                    // previous block contains single record
                    var item = CreateOptimisedPriceItem(
                        previous.MarketId,
                        previous.CurrencyCode,
                        previous.CatalogEntryCode,
                        previous.ValidFrom,
                        (DateTime?)null,
                        previous.UnitPrice
                        );

                    OptimisedPrices.Add(item);
                }
                else
                {

                    for (int i = 1; i < groupLength; i++)
                    {
                        current = group.ElementAt(i);

                        if (current.UnitPrice > basePrice)
                        {
                            // price is higher than base
                            break;
                        }

                        // compare validity time periods
                        DateTime previousUntill = (DateTime)(previous.ValidUntil is null ? current.ValidFrom : previous.ValidUntil);
                        previous.ValidUntil = previousUntill;
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
                                previous.UnitPrice
                                );

                            OptimisedPrices.Add(item);
                            previous = current;
                        }

                        else if (compare < 0)
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

                        else
                        {
                            // periods are overlaping
                            var compareT = DateTime.Compare((DateTime)previous.ValidUntil, (DateTime)current.ValidUntil);

                            if (compareT <= 0)
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
                    }

                    DateTime finalFrom = (DateTime)(previous.ValidUntil is null ? previous.ValidFrom : previous.ValidUntil);

                    var finalItem = CreateOptimisedPriceItem(
                        previous.MarketId,
                        previous.CurrencyCode,
                        previous.CatalogEntryCode,
                        finalFrom,
                        (DateTime?)null,
                        basePrice
                        );

                    OptimisedPrices.Add(finalItem);

                }

            }

            return OptimisedPrices;
        }


        // Creates and returns new instance of OptimisedPrice
        private static OptimisedPrice CreateOptimisedPriceItem(string market, string currency, string id, DateTime from, DateTime? till, decimal price)
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
