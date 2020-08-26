# Products DB

Application for showing optimised history of product prices flactuation.

Solution is a web app built with C#, ASP.NET Core 3.1 and Entity Framework 3.1.

SQLite is used for persisting the data due to being lighweight, thus easy to ship with the application.

(The database is under version control)


## Architecture

Since the application is a simple web app that has a singular functionality, 
it was decided to implement it in one layer
using a Razor Pages template, 
rather than a more complex layered architecture.

## DB creation

On the app startup, if the datebase has not been created yet or is empty,
the DB initialisation process is initiated.

Related code is located in `ProductsDB.Data.DbInit.cs`

Using `CSV Helper` library, the data from `price_detail.csv` is parsed and saved to DB using EF models
and EF Extentions library that enables bulk inserts.

Altogether, three tables are created and populated: `Market`, `Product` and their relation table `PriceDetail`.

## Optimised price history

The method responsible for creating the Optimised price history is located in `ProductsDB.Utils`.

## TODO

- tests