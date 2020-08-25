using System;
using System.Collections.Generic;
using System.Linq;
using ProductsDB.Data;
using ProductsDB.Models;

namespace ProductsDB.Utils
{
    public static class OptimisedPriceCalculator
    {
        private static List<OptimisedPrice> OptimisedPrices;

        public static List<OptimisedPrice> Calculate(List<PriceDetail> ProductPrices)
        {
            // list to hold optimised items
            OptimisedPrices = new List<OptimisedPrice>();
            
            // previous and current items for comparesment
            PriceDetail previous = null;
            PriceDetail current = null;
            // product's main price for country-currency
            decimal basePrice = 0;


            // start of the list
            current = ProductPrices[0];
            // convert first item to optimised
            DateTime untilDT = (DateTime)(current.ValidUntil is null ? ProductPrices[1].ValidFrom : current.ValidUntil);

            var firstItem = CreateOptimisedPriceItem(
                current.MarketId,
                current.CurrencyCode,
                current.CatalogEntryCode,
                current.ValidFrom,
                untilDT,
                current.UnitPrice
                );

            OptimisedPrices.Add(firstItem);

            basePrice = current.UnitPrice;
            current.ValidUntil = untilDT;
            previous = current;
            

            // go through PriceDetail list
            for (int i = 1; i < ProductPrices.Count; i++) 
            {
                // get the item to compare
                current = ProductPrices[i];

                if (current.MarketId != previous.MarketId || current.CurrencyCode != previous.CurrencyCode)
                {
                    // new market-currency block started
                    // 'close' previous block with base price
                    var item = CreateOptimisedPriceItem(
                        previous.MarketId,
                        previous.CurrencyCode,
                        previous.CatalogEntryCode,
                        (DateTime)previous.ValidUntil,
                        (DateTime?)null,
                        basePrice
                        );

                    OptimisedPrices.Add(item);

                    basePrice = current.UnitPrice;
                    if (i != ProductPrices.Count - 1)
                    {
                        DateTime until = (DateTime)(current.ValidUntil is null ? ProductPrices[i + 1].ValidFrom : current.ValidUntil);
                        current.ValidUntil = until;
                    }

                    previous = current;
                }

                else
                {
                    // compare validity time periods

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

            return OptimisedPrices;
        }

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
