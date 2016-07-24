#if NET_4_5
using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class LinqQuerySamplesAsync : LinqTestCaseAsync
	{
		[Test]
		public void GroupTwoQueriesAndSum()
		{
			//NH-3534
			var queryWithAggregation =
				from o1 in db.Orders
				from o2 in db.Orders
				where o1.Customer.CustomerId == o2.Customer.CustomerId && o1.OrderDate == o2.OrderDate
				group o1 by new
				{
				o1.Customer.CustomerId, o1.OrderDate
				}

					into g
					select new
					{
					CustomerId = g.Key.CustomerId, LastOrderDate = g.Max(x => x.OrderDate)}

			;
			var result = queryWithAggregation.ToList();
			Assert.IsNotNull(result);
			Assert.IsNotEmpty(result);
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses WHERE to filter for Customers in London.")]
		public void DLinq1()
		{
			IOrderedQueryable<Customer> q =
				from c in db.Customers
				where c.Address.City == "London"
				orderby c.CustomerId
				select c;
			AssertByIds(q, new[]{"AROUT", "BSBEV", "CONSH", "EASTC", "NORTS", "SEVES"}, x => x.CustomerId);
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses WHERE to filter for Customers in London and then Madrid to ensure that the parameterization is working.")]
		public void DLinq1b()
		{
			string city = "London";
			IOrderedQueryable<Customer> q =
				from c in db.Customers
				where c.Address.City == city
				orderby c.CustomerId
				select c;
			AssertByIds(q, new[]{"AROUT", "BSBEV", "CONSH", "EASTC", "NORTS", "SEVES"}, x => x.CustomerId);
			city = "Madrid";
			q =
				from c in db.Customers
				where c.Address.City == city
				orderby c.CustomerId
				select c;
			AssertByIds(q, new[]{"BOLID", "FISSA", "ROMEY"}, x => x.CustomerId);
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses SELECT and anonymous types to return a sequence of just the Customers' contact names and phone numbers.")]
		public void DLinq10()
		{
			var q =
				from c in db.Customers
				select new
				{
				c.ContactName, c.Address.PhoneNumber
				}

			;
			var items = q.ToList();
			Assert.AreEqual(91, items.Count);
			items.Each(x =>
			{
				Assert.IsNotNull(x.ContactName);
				Assert.IsNotNull(x.PhoneNumber);
			}

			);
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses SELECT and anonymous types to return " + "a sequence of just the Employees' names and phone numbers, " + "with the FirstName and LastName fields combined into a single field, 'Name', " + "and the HomePhone field renamed to Phone in the resulting sequence.")]
		public void DLinq11()
		{
			var q =
				from e in db.Employees
				select new
				{
				Name = e.FirstName + " " + e.LastName, Phone = e.Address.PhoneNumber
				}

			;
			var items = q.ToList();
			Assert.AreEqual(9, items.Count);
			items.Each(x =>
			{
				Assert.IsNotNull(x.Name);
				Assert.IsNotNull(x.Phone);
			}

			);
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses SELECT and anonymous types to return " + "a sequence of all Products' IDs and a calculated value " + "called HalfPrice which is set to the Product's UnitPrice " + "divided by 2.")]
		public void DLinq12()
		{
			var q =
				from p in db.Products
				select new
				{
				p.ProductId, p.UnitPrice, HalfPrice = p.UnitPrice / 2
				}

			;
			foreach (var item in q)
			{
				Assert.AreEqual((item.UnitPrice / 2), item.HalfPrice);
			}
		}

		[Category("SELECT/DISTINCT")]
		[Test]
		public void DLinq12b()
		{
			var q =
				from p in db.Products
				select new
				{
				p.ProductId, p.UnitPrice, HalfPrice = p.UnitPrice * 2
				}

			;
			foreach (var item in q)
			{
				Assert.IsTrue(item.UnitPrice * 2 == item.HalfPrice);
			}
		}

		[Category("SELECT/DISTINCT")]
		[Test]
		public void DLinq12c()
		{
			var q =
				from p in db.Products
				select new
				{
				p.ProductId, p.UnitPrice, HalfPrice = p.UnitPrice + 2
				}

			;
			foreach (var item in q)
			{
				Assert.IsTrue(item.UnitPrice + 2 == item.HalfPrice);
			}
		}

		[Category("SELECT/DISTINCT")]
		[Test]
		public void DLinq12d()
		{
			var q =
				from p in db.Products
				select new
				{
				p.ProductId, p.UnitPrice, HalfPrice = p.UnitPrice - 2
				}

			;
			foreach (var item in q)
			{
				Assert.IsTrue(item.UnitPrice - 2 == item.HalfPrice);
			}
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses SELECT and a conditional statment to return a sequence of product " + " name and product availability.")]
		public async Task DLinq13Async()
		{
			var q =
				from p in db.Products
				select new
				{
				p.Name, Availability = p.UnitsInStock - p.UnitsOnOrder < 0 ? "Out Of Stock" : "In Stock"
				}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses SELECT and a known type to return a sequence of employees' names.")]
		public async Task DLinq14Async()
		{
			IQueryable<Name> q =
				from e in db.Employees
				select new Name{FirstName = e.FirstName, LastName = e.LastName};
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses SELECT and WHERE to return a sequence of " + "just the London Customers' contact names.")]
		public async Task DLinq15Async()
		{
			IQueryable<string> q =
				from c in db.Customers
				where c.Address.City == "London"
				select c.ContactName;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses SELECT and anonymous types to return " + "a shaped subset of the data about Customers.")]
		public async Task DLinq16Async()
		{
			var q =
				from c in db.Customers
				select new
				{
				c.CustomerId, CompanyInfo = new
				{
				c.CompanyName, c.Address.City, c.Address.Country
				}

				, ContactInfo = new
				{
				c.ContactName, c.ContactTitle
				}

				, Count = c.Orders.Count()}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses nested queries to return a sequence of " + "all orders containing their OrderId, a subsequence of the " + "items in the order where there is a discount, and the money " + "saved if shipping is not included.")]
		[Ignore("TODO - nested select")]
		public async Task DLinq17Async()
		{
			using (ISession s = OpenSession())
			{
				/////////////
				///// Flattened Select
				/////////////
				//// In HQL select, get all the data that's needed
				//var dbOrders =
				//    s.CreateQuery("select o.OrderId, od, o.Freight from Order o join o.OrderLines od").List<object[]>();
				//// Now group by the items in the parent select, grouping the items in the child select (note lookups on object[], ala SelectClauseVisitor)
				//// Note the casts to get the types correct.  Need to check if SelectClauseVisitor handles that, but think it does
				//var a = from o in dbOrders
				//        group new { OrderLine = (OrderLine)o[1], Freight = (Decimal?)o[2] } by new { OrderId = (int) o[0] }
				//            into g
				//            select
				//            // Select the parent items,  and the child items in a nested select
				//            new { g.Key.OrderId, DiscountedProducts = from e in g select new { e.OrderLine, FreeShippingDiscount = e.Freight } };
				//a.ToList();
				/////////////
				///// Nested Select
				/////////////
				//var dbOrders2 = s.CreateQuery("select o.OrderId from Order o").List<int>();
				//var q2 = from o in dbOrders2
				//         select new
				//                    {
				//                        OrderId = o,
				//                        DiscountedProducts =
				//                             from subO in db.Orders
				//                                 where subO.OrderId == o
				//                                 from orderLine in subO.OrderLines
				//                                 select new { orderLine, FreeShippingDiscount = subO.Freight }
				//                    };
				//q2.ToList();
				///////////
				///// Batching Select
				///////////
				var dbOrders3 = await (s.CreateQuery("select o.OrderId from Order o").ListAsync<int>());
				//var q3 = dbOrders3.SubQueryBatcher(orderId => orderId,
				//                                   ids => from subO in db.Orders.ToList()  // Note that ToList is just because current group by code is incorrent in our linq provider
				//                                          where ids.Contains(subO.OrderId)
				//                                          from orderLine in subO.OrderLines
				//                                          group new {orderLine, FreeShippingDiscount = subO.Freight}
				//                                             by subO.OrderId
				//                                          into g
				//                                             select g
				//                                   )
				//                                   .Select((input, index) => new
				//                                    {
				//                                         OrderId = input.Item,
				//                                         DiscountedProducts = input.Batcher.GetData(index)
				//                    });
				// This is what we want:
				//var q3 = dbOrders3.SubQueryBatcher(orderId => orderId,
				//                                   ids => db.Orders
				//                                       .Where(o => ids.Contains(o.OrderId))
				//                                       .Select(o => new {o.OrderId, o.OrderLines, o.Freight}).ToList()
				//                                       .GroupBy(k => k.OrderId, e => new { e.OrderLines, FreeShippingDiscount = e.Freight})
				//                                   )
				//                                   .Select((input, index) => new
				//                                    {
				//                                         OrderId = input.Item,
				//                                         DiscountedProducts = input.Batcher.GetData(index)
				//                    });
				// This is what we're using since our provider can't yet handle the in or the group by clauses correctly (note the ToList and the Where clause moving to get us into Linq to Objects world)
				var q3 = dbOrders3.SubQueryBatcher(orderId => orderId, ids => (
					from o in db.Orders
					from ol in o.OrderLines
					select new
					{
					OrderLines = ol, FreeShippingDiscount = o.Freight, o.OrderId
					}

				).ToList().Where(o => ids.Contains(o.OrderId)).GroupBy(k => k.OrderId, e => new
				{
				e.OrderLines, e.FreeShippingDiscount
				}

				)).Select((input, index) => new
				{
				OrderId = input.Item, DiscountedProducts = input.Batcher.GetData(index)}

				);
				foreach (var x in q3)
				{
					Console.WriteLine(x.OrderId);
					foreach (var y in x.DiscountedProducts)
					{
						Console.WriteLine(y.FreeShippingDiscount);
					}
				}

				q3.ToList();
			}

			var q =
				from o in db.Orders
				select new
				{
				o.OrderId, DiscountedProducts =
					from od in o.OrderLines
					//                                    from od in o.OrderLines.Cast<OrderLine>()
					where od.Discount > 0.0m
					select od, FreeShippingDiscount = o.Freight
				}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses nested queries to return a sequence of " + "all orders containing their OrderId, a subsequence of the " + "items in the order where there is a discount, and the money " + "saved if shipping is not included.")]
		public async Task DLinq17bAsync()
		{
			var q =
				from o in db.Orders
				select new
				{
				o.OrderId, DiscountedProducts =
					from od in o.OrderLines
					where od.Discount > 0.0m
					select new
					{
					od.Quantity, od.UnitPrice
					}

				, FreeShippingDiscount = o.Freight
				}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses nested queries to return a sequence of " + "all orders containing their OrderId, a subsequence of the " + "items in the order where there is a discount, and the money " + "saved if shipping is not included.")]
		[Ignore("TODO - nested select")]
		public async Task DLinq17cAsync()
		{
			var q =
				from o in db.Orders
				select new
				{
				o.OrderId, DiscountedProducts =
					from od in o.OrderLines
					//                    from od in o.OrderLines.Cast<OrderLine>()
					where od.Discount > 0.0m
					orderby od.Discount descending
					select od, FreeShippingDiscount = o.Freight
				}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses Distinct to select a sequence of the unique cities " + "that have Customers.")]
		public async Task DLinq18Async()
		{
			IQueryable<string> q = (
				from c in db.Customers
				select c.Address.City).Distinct();
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Count to find the number of Customers in the database.")]
		public void DLinq19()
		{
			int q = db.Customers.Count();
			Console.WriteLine(q);
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses WHERE to filter for Employees hired during or after 1994.")]
		public void DLinq2()
		{
			IQueryable<Employee> q =
				from e in db.Employees
				where e.HireDate >= new DateTime(1994, 1, 1)select e;
			AssertByIds(q, new[]{7, 8, 9}, x => x.EmployeeId);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Count to find the number of Products in the database " + "that are not discontinued.")]
		public void DLinq20()
		{
			int q = db.Products.Count(p => !p.Discontinued);
			Console.WriteLine(q);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Sum to find the total freight over all Orders.")]
		public void DLinq21()
		{
			decimal ? q = db.Orders.Select(o => o.Freight).Sum();
			Console.WriteLine(q);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Sum to find the total number of units on order over all Products.")]
		public void DLinq22()
		{
			int ? q = db.Products.Sum(p => p.UnitsOnOrder);
			Console.WriteLine(q);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Min to find the lowest unit price of any Product.")]
		public void DLinq23()
		{
			decimal ? q = db.Products.Select(p => p.UnitPrice).Min();
			Console.WriteLine(q);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Min to find the lowest freight of any Order.")]
		public void DLinq24()
		{
			decimal ? q = db.Orders.Min(o => o.Freight);
			Console.WriteLine(q);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Min to find the Products that have the lowest unit price " + "in each category.")]
		[Ignore("TODO nested aggregating group by")]
		public async Task DLinq25Async()
		{
			using (var session = OpenSession())
			{
				var output = (await (session.CreateQuery("select p.Category.CategoryId, p from Product p where p.UnitPrice = (select min(p2.UnitPrice) from Product p2 where p.Category.CategoryId = p2.Category.CategoryId)").ListAsync<object[]>())).GroupBy(input => input[0]).Select(input => new
				{
				CategoryId = (int)input.Key, CheapestProducts =
					from g in input
					select (Product)g[1]
				}

				);
			}

			/*
			 * From g, only using g.Key, min(UnitPrice), g
			 *  - g.Key is fine
			 *  - min(UnitPrice) is fine
			 *  - g is the problem.  Can't just issue a single select since it's non-aggregating
			 *    However, don't want to loose the aggregate; need that processed in the DB
			 * 
			 * To get additional information over and above g.Key and any aggregates, need a where clause against the aggregate:
			 * 
			 * select xxx, yyy, zzz from Product p where p.UnitPrice = (select min(p2.UnitPrice) from Product p2)
			 * 
			 * the outer where comes from the inner where in the queryModel:
			 *
			 * where p2.UnitPrice == g.Min(p3 => p3.UnitPrice)
			 * 
			 * also need additional constraints on the aggregate to fulfil the groupby requirements:
			 * 
			 * where p.Category.CategoryId = p2.Category.CategoryId
			 * 
			 * so join the inner select to the outer select using the group by criteria
			 * 
			 * finally, need to do some client-side processing to get the "shape" correct
			 * 
			 */
			var categories =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					CategoryId = g.Key, CheapestProducts = (IEnumerable<Product>)(
						from p2 in g
						where p2.UnitPrice == g.Min(p3 => p3.UnitPrice)select p2)}

			;
			Console.WriteLine(await (ObjectDumper.WriteAsync(categories, 1)));
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Max to find the latest hire date of any Employee.")]
		public void DLinq26()
		{
			DateTime? q = db.Employees.Select(e => e.HireDate).Max();
			Console.WriteLine(q);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Max to find the most units in stock of any Product.")]
		public void DLinq27()
		{
			int ? q = db.Products.Max(p => p.UnitsInStock);
			Console.WriteLine(q);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Max to find the Products that have the highest unit price " + "in each category.")]
		[Ignore("TODO nested aggregating group by")]
		public async Task DLinq28Async()
		{
			var categories =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, MostExpensiveProducts =
						from p2 in g
						where p2.UnitPrice == g.Max(p3 => p3.UnitPrice)select p2
					}

			;
			await (ObjectDumper.WriteAsync(categories, 1));
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Average to find the average freight of all Orders.")]
		public void DLinq29()
		{
			decimal ? q = db.Orders.Select(o => o.Freight).Average();
			Console.WriteLine(q);
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses WHERE to filter for Products that have stock below their reorder level and are not discontinued.")]
		public void DLinq3()
		{
			IQueryable<Product> q =
				from p in db.Products
				where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
				select p;
			AssertByIds(q, new[]{2, 3, 11, 21, 30, 31, 32, 37, 43, 45, 48, 49, 56, 64, 66, 68, 70, 74, }, x => x.ProductId);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Average to find the average unit price of all Products.")]
		public void DLinq30()
		{
			decimal ? q = db.Products.Average(p => p.UnitPrice);
			Console.WriteLine(q);
		}

		[Category("COUNT/SUM/MIN/MAX/AVG")]
		[Test(Description = "This sample uses Average to find the Products that have unit price higher than " + "the average unit price of the category for each category.")]
		[Ignore("TODO nested aggregating group by")]
		public async Task DLinq31Async()
		{
			var categories =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, ExpensiveProducts =
						from p2 in g
						where p2.UnitPrice > g.Average(p3 => p3.UnitPrice)select p2
					}

			;
			await (ObjectDumper.WriteAsync(categories, 1));
		}

		[Category("ORDER BY")]
		[Test(Description = "This sample uses orderby to sort Employees by hire date.")]
		public async Task DLinq36Async()
		{
			IOrderedQueryable<Employee> q =
				from e in db.Employees
				orderby e.HireDate
				select e;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("ORDER BY")]
		[Test(Description = "This sample uses where and orderby to sort Orders " + "shipped to London by freight.")]
		public async Task DLinq37Async()
		{
			IOrderedQueryable<Order> q =
				from o in db.Orders
				where o.ShippingAddress.City == "London"
				orderby o.Freight
				select o;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("ORDER BY")]
		[Test(Description = "This sample uses orderby to sort Products " + "by unit price from highest to lowest.")]
		public async Task DLinq38Async()
		{
			IOrderedQueryable<Product> q =
				from p in db.Products
				orderby p.UnitPrice descending
				select p;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("ORDER BY")]
		[Test(Description = "This sample uses a compound orderby to sort Customers " + "by city and then contact name.")]
		public async Task DLinq39Async()
		{
			IOrderedQueryable<Customer> q =
				from c in db.Customers
				orderby c.Address.City, c.ContactName
				select c;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses WHERE to filter for Products that have stock below their reorder level and are not discontinued.")]
		public void DLinq3b()
		{
			IQueryable<Product> q =
				from p in db.Products
				where p.UnitsInStock <= p.ReorderLevel && p.Discontinued == false
				select p;
			AssertByIds(q, new[]{2, 3, 11, 21, 30, 31, 32, 37, 43, 45, 48, 49, 56, 64, 66, 68, 70, 74, }, x => x.ProductId);
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses WHERE to filter out Products that are either UnitPrice is greater than 10 or is discontinued.")]
		public void DLinq4()
		{
			IQueryable<Product> q =
				from p in db.Products
				where p.UnitPrice > 10m || p.Discontinued
				select p;
			AssertByIds(q, new[]{1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15, 16, 17, 18, 20, 22, 24, 25, 26, 27, 28, 29, 30, 31, 32, 34, 35, 36, 37, 38, 39, 40, 42, 43, 44, 46, 48, 49, 50, 51, 53, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 76, 77, }, x => x.ProductId);
		}

		[Category("ORDER BY")]
		[Test(Description = "This sample uses orderby to sort Orders from EmployeeId 1 " + "by ship-to country, and then by freight from highest to lowest.")]
		public async Task DLinq40Async()
		{
			IOrderedQueryable<Order> q =
				from o in db.Orders
				where o.Employee.EmployeeId == 1
				orderby o.ShippingAddress.Country, o.Freight descending
				select o;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("ORDER BY")]
		[Test(Description = "This sample uses Orderby, Max and Group By to find the Products that have " + "the highest unit price in each category, and sorts the group by category id.")]
		[Ignore("TODO nested aggregating group by")]
		public async Task DLinq41Async()
		{
			var categories =
				from p in db.Products
				group p by p.Category.CategoryId into g
					orderby g.Key
					select new
					{
					g.Key, MostExpensiveProducts =
						from p2 in g
						where p2.UnitPrice == g.Max(p3 => p3.UnitPrice)select p2
					}

			;
			await (ObjectDumper.WriteAsync(categories, 1));
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses group by to partition Products by " + "CategoryId.")]
		public async Task DLinq42Async()
		{
			IQueryable<IGrouping<int, Product>> q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select g;
			await (ObjectDumper.WriteAsync(q, 1));
			foreach (var o in q)
			{
				Console.WriteLine("\n{0}\n", o);
				foreach (Product p in o)
				{
					await (ObjectDumper.WriteAsync(p));
				}
			}
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses group by and Max " + "to find the maximum unit price for each CategoryId.")]
		public async Task DLinq43Async()
		{
			var q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, MaxPrice = g.Max(p => p.UnitPrice)}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses group by and Min " + "to find the minimum unit price for each CategoryId.")]
		public async Task DLinq44Async()
		{
			var q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, MinPrice = g.Min(p => p.UnitPrice)}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses group by and Average " + "to find the average UnitPrice for each CategoryId.")]
		public async Task DLinq45Async()
		{
			var q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, AveragePrice = g.Average(p2 => p2.UnitPrice)}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses group by and Sum " + "to find the total UnitPrice for each CategoryId.")]
		public async Task DLinq46Async()
		{
			var q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, TotalPrice = g.Sum(p => p.UnitPrice)}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses group by and Count " + "to find the number of Products in each CategoryId.")]
		public async Task DLinq47Async()
		{
			var q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, NumProducts = g.Count()}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses group by and Count " + "to find the number of Products in each CategoryId " + "that are discontinued.")]
		public async Task DLinq48Async()
		{
			var q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, NumProducts = g.Count(p => p.Discontinued)}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses group by and Count " + "to find the number of Products in each CategoryId " + "that are not discontinued.")]
		public async Task DLinq48bAsync()
		{
			var q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					select new
					{
					g.Key, NumProducts = g.Count(p => !p.Discontinued)}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses a where clause after a group by clause " + "to find all categories that have at least 10 products.")]
		public async Task DLinq49Async()
		{
			var q =
				from p in db.Products
				group p by p.Category.CategoryId into g
					where g.Count() >= 10
					select new
					{
					g.Key, ProductCount = g.Count()}

			;
			await (ObjectDumper.WriteAsync(q, 1));
		}

		[Category("WHERE")]
		[Test(Description = "This sample calls WHERE twice to filter out Products that UnitPrice is greater than 10" + " and is discontinued.")]
		public void DLinq5()
		{
			IEnumerable<Product> q = db.Products.Where(p => p.UnitPrice > 10m).Where(p => p.Discontinued);
			AssertByIds(q, new[]{5, 9, 17, 28, 29, 42, 53, }, x => x.ProductId);
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses Group By to group products by CategoryId and SupplierId.")]
		public async Task DLinq50Async()
		{
			//var prods = db.Products.ToList();
			var categories =
				from p in db.Products
				group p by new
				{
				p.Category.CategoryId, p.Supplier.SupplierId
				}

					into g
					select new
					{
					g.Key, g
					}

			;
			var nhOutput = await (ObjectDumper.WriteAsync(categories, 1));
			var categories2 =
				from p in db.Products.ToList()group p by new
				{
				p.Category.CategoryId, p.Supplier.SupplierId
				}

					into g
					select new
					{
					g.Key, g
					}

			;
			string linq2ObjectsOutput = await (ObjectDumper.WriteAsync(categories2, 1));
			Assert.AreEqual(nhOutput, linq2ObjectsOutput);
		}

		[Category("GROUP BY/HAVING")]
		[Test(Description = "This sample uses Group By to return two sequences of products. " + "The first sequence contains products with unit price " + "greater than 10. The second sequence contains products " + "with unit price less than or equal to 10.")]
		public async Task DLinq51Async()
		{
			var categories =
				from p in db.Products
				group p by new
				{
				Criterion = p.UnitPrice > 10
				}

					into g
					select g;
			await (ObjectDumper.WriteAsync(categories, 1));
		}

		[Category("EXISTS/IN/ANY/ALL")]
		[Test(Description = "This sample uses Any to return only Customers that have no Orders.")]
		public async Task DLinq52Async()
		{
			IQueryable<Customer> q =
				from c in db.Customers
				where !c.Orders.Any()//                where !c.Orders.Cast<Order>().Any()
				select c;
			await (ObjectDumper.WriteAsync(q));
			foreach (Customer c in q)
				Assert.IsTrue(!c.Orders.Cast<Order>().Any());
		}

		[Category("EXISTS/IN/ANY/ALL")]
		[Test(Description = "This sample uses Any to return only Customers that have Orders.")]
		public async Task DLinq52bAsync()
		{
			IQueryable<Customer> q =
				from c in db.Customers
				where c.Orders.Any()//                where c.Orders.Cast<Order>().Any()
				select c;
			await (ObjectDumper.WriteAsync(q));
			foreach (Customer c in q)
				Assert.IsTrue(c.Orders.Cast<Order>().Any());
		}

		[Category("EXISTS/IN/ANY/ALL")]
		[Test(Description = "This sample uses Any to return only Categories that have " + "at least one Discontinued product.")]
		public async Task DLinq53Async()
		{
			IQueryable<ProductCategory> q =
				from c in db.Categories
				where c.Products.Any(p => p.Discontinued)//                where c.Products.Cast<Product>().Any(p => p.Discontinued)
				select c;
			await (ObjectDumper.WriteAsync(q));
			foreach (ProductCategory c in q)
				Assert.IsTrue(c.Products.Cast<Product>().Any(p => p.Discontinued));
		}

		[Category("EXISTS/IN/ANY/ALL")]
		[Test(Description = "This sample uses Any to return only Categories that have " + "zero Discontinued products.")]
		public async Task DLinq53bAsync()
		{
			IQueryable<ProductCategory> q =
				from c in db.Categories
				where c.Products.Any(p => !p.Discontinued)//                where c.Products.Cast<Product>().Any(p => !p.Discontinued)
				select c;
			await (ObjectDumper.WriteAsync(q));
			foreach (ProductCategory c in q)
				Assert.IsTrue(c.Products.Cast<Product>().Any(p => !p.Discontinued));
		}

		[Category("EXISTS/IN/ANY/ALL")]
		[Test(Description = "This sample uses Any to return only Categories that does not have " + "at least one Discontinued product.")]
		public async Task DLinq53cAsync()
		{
			IQueryable<ProductCategory> q =
				from c in db.Categories
				where !c.Products.Any(p => p.Discontinued)//                where !c.Products.Cast<Product>().Any(p => p.Discontinued)
				select c;
			await (ObjectDumper.WriteAsync(q));
			foreach (ProductCategory c in q)
				Assert.IsTrue(!c.Products.Cast<Product>().Any(p => p.Discontinued));
		}

		[Category("EXISTS/IN/ANY/ALL")]
		[Test(Description = "This sample uses Any to return only Categories that does not have " + "any Discontinued products.")]
		public async Task DLinq53dAsync()
		{
			IQueryable<ProductCategory> q =
				from c in db.Categories
				where !c.Products.Any(p => !p.Discontinued)//                where !c.Products.Cast<Product>().Any(p => !p.Discontinued)
				select c;
			await (ObjectDumper.WriteAsync(q));
			foreach (ProductCategory c in q)
				Assert.IsTrue(!c.Products.Cast<Product>().Any(p => !p.Discontinued));
		}

		[Category("EXISTS/IN/ANY/ALL")]
		[Test(Description = "This sample uses All to return Customers whom all of their orders " + "have been shipped to their own city or whom have no orders.")]
		public async Task DLinq54Async()
		{
			IQueryable<Customer> q =
				from c in db.Customers
				where c.Orders.All(o => o.ShippingAddress.City == c.Address.City)//                where c.Orders.Cast<Order>().All(o => o.ShippingAddress.City == c.Address.City)
				select c;
			await (ObjectDumper.WriteAsync(q));
			foreach (Customer c in q)
			{
				Customer customer = c;
				Assert.IsTrue(c.Orders.Cast<Order>().All(o => o.ShippingAddress.City == customer.Address.City));
			}
		}

		[Category("UNION ALL/UNION/INTERSECT")]
		[Test(Description = "This sample uses Concat to return a sequence of all Customer and Employee " + "phone/fax numbers.")]
		[Ignore("TODO set operations")]
		public async Task DLinq55Async()
		{
			IQueryable<string> q = (
				from c in db.Customers
				select c.Address.PhoneNumber).Concat(
				from c in db.Customers
				select c.Address.Fax).Concat(
				from e in db.Employees
				select e.Address.PhoneNumber);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("UNION ALL/UNION/INTERSECT")]
		[Test(Description = "This sample uses Concat to return a sequence of all Customer and Employee " + "name and phone number mappings.")]
		[Ignore("TODO set operations")]
		public async Task DLinq56Async()
		{
			var q = (
				from c in db.Customers
				select new
				{
				Name = c.CompanyName, Phone = c.Address.PhoneNumber
				}

			).Concat(
				from e in db.Employees
				select new
				{
				Name = e.FirstName + " " + e.LastName, Phone = e.Address.PhoneNumber
				}

			);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("UNION ALL/UNION/INTERSECT")]
		[Test(Description = "This sample uses Union to return a sequence of all countries that either " + "Customers or Employees are in.")]
		[Ignore("TODO set operations")]
		public async Task DLinq57Async()
		{
			IQueryable<string> q = (
				from c in db.Customers
				select c.Address.Country).Union(
				from e in db.Employees
				select e.Address.Country);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("UNION ALL/UNION/INTERSECT")]
		[Test(Description = "This sample uses Intersect to return a sequence of all countries that both " + "Customers and Employees live in.")]
		[Ignore("TODO set operations")]
		public async Task DLinq58Async()
		{
			IQueryable<string> q = (
				from c in db.Customers
				select c.Address.Country).Intersect(
				from e in db.Employees
				select e.Address.Country);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("UNION ALL/UNION/INTERSECT")]
		[Test(Description = "This sample uses Except to return a sequence of all countries that " + "Customers live in but no Employees live in.")]
		[Ignore("TODO set operations")]
		public async Task DLinq59Async()
		{
			IQueryable<string> q = (
				from c in db.Customers
				select c.Address.Country).Except(
				from e in db.Employees
				select e.Address.Country);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses First to select the first Shipper in the table.")]
		public void DLinq6()
		{
			Shipper shipper = db.Shippers.First();
			Assert.AreEqual(1, shipper.ShipperId);
		}

		[Category("TOP/BOTTOM")]
		[Test(Description = "This sample uses Take to select the first 5 Employees hired.")]
		public async Task DLinq60Async()
		{
			IQueryable<Employee> q = (
				from e in db.Employees
				orderby e.HireDate
				select e).Take(5);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("TOP/BOTTOM")]
		[Test(Description = "This sample uses Skip to select all but the 10 most expensive Products.")]
		public async Task DLinq61Async()
		{
			IQueryable<Product> q = (
				from p in db.Products
				orderby p.UnitPrice descending
				select p).Skip(10);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("Paging")]
		[Test(Description = "This sample uses the Skip and Take operators to do paging by " + "skipping the first 50 records and then returning the next 10, thereby " + "providing the data for page 6 of the Products table.")]
		public async Task DLinq62Async()
		{
			IQueryable<Customer> q = (
				from c in db.Customers
				orderby c.ContactName
				select c).Skip(50).Take(10);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("Paging")]
		[Test(Description = "This sample uses a where clause and the Take operator to do paging by, " + "first filtering to get only the ProductIds above 50 (the last ProductId " + "from page 5), then ordering by ProductId, and finally taking the first 10 results, " + "thereby providing the data for page 6 of the Products table.  " + "Note that this method only works when ordering by a unique key.")]
		public async Task DLinq63Async()
		{
			IQueryable<Product> q = (
				from p in db.Products
				where p.ProductId > 50
				orderby p.ProductId
				select p).Take(10);
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses First to select the single Customer with CustomerId 'BONAP'.")]
		public void DLinq7()
		{
			Customer cust = db.Customers.First(c => c.CustomerId == "BONAP");
			Assert.AreEqual("BONAP", cust.CustomerId);
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses First to select an Order with freight greater than 10.00.")]
		public void DLinq8()
		{
			Order ord = db.Orders.First(o => o.Freight > 10.00M);
			Assert.Greater(ord.Freight, 10.00M);
		}

		[Category("SELECT/DISTINCT")]
		[Test(Description = "This sample uses SELECT to return a sequence of just the Customers' contact names.")]
		public void DLinq9()
		{
			IQueryable<string> q =
				from c in db.Customers
				select c.ContactName;
			IList<string> items = q.ToList();
			Assert.AreEqual(91, items.Count);
			items.Each(Assert.IsNotNull);
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "from clause to select all orders for customers in London.")]
		public async Task DLinqJoin1Async()
		{
			IQueryable<Order> q =
				from c in db.Customers
				from o in c.Orders
				//                from o in c.Orders.Cast<Order>()
				where c.Address.City == "London"
				select o;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample shows how to construct a join where one side is nullable and the other isn't.")]
		public async Task DLinqJoin10Async()
		{
			var q =
				from o in db.Orders
				join e in db.Employees on o.Employee.EmployeeId equals (int ? )e.EmployeeId into emps
				from e in emps
				select new
				{
				o.OrderId, e.FirstName
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "from clause to select all orders for customers in London.")]
		public async Task DLinqJoin1aAsync()
		{
			var q =
				from c in db.Customers
				from o in c.Orders
				//                from o in c.Orders.Cast<Order>()
				where c.Address.City == "London"
				select new
				{
				o.OrderDate, o.ShippingAddress.Region
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "from clause to select all orders for customers in London.")]
		public async Task DLinqJoin1bAsync()
		{
			var q =
				from c in db.Customers
				from o in c.Orders
				//                from o in c.Orders.Cast<Order>()
				where c.Address.City == "London"
				select new
				{
				c.Address.City, o.OrderDate, o.ShippingAddress.Region
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "from clause to select all orders for customers.")]
		public async Task DLinqJoin1cAsync()
		{
			IQueryable<Order> q =
				from c in db.Customers
				from o in c.Orders
				//                from o in c.Orders.Cast<Order>()
				select o;
			List<Order> list = q.ToList();
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "from clause to select all orders for customers.")]
		public async Task DLinqJoin1dAsync()
		{
			IQueryable<DateTime? > q =
				from c in db.Customers
				//                from o in c.Orders.Cast<Order>()
				from o in c.Orders
				select o.OrderDate;
			List<DateTime? > list = q.ToList();
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "from clause to select all orders for customers.")]
		public async Task DLinqJoin1eAsync()
		{
			IQueryable<Customer> q =
				from c in db.Customers
				from o in c.Orders
				//                from o in c.Orders.Cast<Order>()
				select c;
			List<Customer> list = q.ToList();
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "where clause to filter for Products whose Supplier is in the USA " + "that are out of stock.")]
		public async Task DLinqJoin2Async()
		{
			IQueryable<Product> q =
				from p in db.Products
				where p.Supplier.Address.Country == "USA" && p.UnitsInStock == 0
				select p;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "from clause to filter for employees in Seattle, " + "and also list their territories.")]
		public async Task DLinqJoin3Async()
		{
			var q =
				from e in db.Employees
				from et in e.Territories
				//                from et in e.Territories.Cast<Territory>()
				where e.Address.City == "Seattle"
				select new
				{
				e.FirstName, e.LastName, et.Region.Description
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample uses foreign key navigation in the " + "select clause to filter for pairs of employees where " + "one employee reports to the other and where " + "both employees are from the same City.")]
		public async Task DLinqJoin4Async()
		{
			var q =
				from e1 in db.Employees
				from e2 in e1.Subordinates
				//                from e2 in e1.Subordinates.Cast<Employee>()
				where e1.Address.City == e2.Address.City
				select new
				{
				FirstName1 = e1.FirstName, LastName1 = e1.LastName, FirstName2 = e2.FirstName, LastName2 = e2.LastName, e1.Address.City
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample explictly joins two tables and projects results from both tables using a group join.")]
		public async Task DLinqJoin5Async()
		{
			var q =
				from c in db.Customers
				join o in db.Orders on c.CustomerId equals o.Customer.CustomerId into orders
				select new
				{
				c.ContactName, OrderCount = orders.Average(x => x.Freight)}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample explictly joins two tables and projects results from both tables.")]
		public async Task DLinqJoin5aAsync()
		{
			var q =
				from c in db.Customers
				join o in db.Orders on c.CustomerId equals o.Customer.CustomerId
				select new
				{
				c.ContactName, o.OrderId
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample explictly joins two tables and projects results from both tables using a group join.")]
		public async Task DLinqJoin5bAsync()
		{
			var q =
				from c in db.Customers
				join o in db.Orders on c.CustomerId equals o.Customer.CustomerId
				group new
				{
				c, o
				}

				by c.ContactName into g
					select new
					{
					ContactName = g.Key, OrderCount = g.Average(i => i.o.Freight)}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample explictly joins two tables with a composite key and projects results from both tables.")]
		public async Task DLinqJoin5cAsync()
		{
			var q =
				from c in db.Customers
				join o in db.Orders on new
				{
				c.CustomerId
				}

				equals new
				{
				o.Customer.CustomerId
				}

				select new
				{
				c.ContactName, o.OrderId
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample explictly joins three tables and projects results from each of them.")]
		public async Task DLinqJoin6Async()
		{
			var q =
				from c in db.Customers
				join o in db.Orders on c.CustomerId equals o.Customer.CustomerId into ords
				join e in db.Employees on c.Address.City equals e.Address.City into emps
				select new
				{
				c.ContactName, ords = ords.Count(), emps = emps.Count()}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample shows how to get LEFT OUTER JOIN by using DefaultIfEmpty(). The DefaultIfEmpty() method returns null when there is no Order for the Employee.")]
		[Ignore("TODO left outer join")]
		public async Task DLinqJoin7Async()
		{
			var q =
				from e in db.Employees
				join o in db.Orders on e equals o.Employee into ords
				from o in ords.DefaultIfEmpty()select new
				{
				e.FirstName, e.LastName, Order = o
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample projects a 'let' expression resulting from a join.")]
		public async Task DLinqJoin8Async()
		{
			var q =
				from c in db.Customers
				join o in db.Orders on c.CustomerId equals o.Customer.CustomerId into ords
				let z = c.Address.City + c.Address.Country
				from o in ords
				select new
				{
				c.ContactName, o.OrderId, z
				}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("JOIN")]
		[Test(Description = "This sample shows a group join with a composite key.")]
		public void DLinqJoin9()
		{
			var expected = (
				from o in db.Orders.ToList()from p in db.Products.ToList()join d in db.OrderLines.ToList()on new
				{
				o.OrderId, p.ProductId
				}

				equals new
				{
				d.Order.OrderId, d.Product.ProductId
				}

				into details
				from d in details
				select new
				{
				o.OrderId, p.ProductId, d.UnitPrice
				}

			).ToList();
			var actual = (
				from o in db.Orders
				from p in db.Products
				join d in db.OrderLines on new
				{
				o.OrderId, p.ProductId
				}

				equals new
				{
				d.Order.OrderId, d.Product.ProductId
				}

				into details
				from d in details
				select new
				{
				o.OrderId, p.ProductId, d.UnitPrice
				}

			).ToList();
			Assert.AreEqual(expected.Count, actual.Count);
		}

		[Category("JOIN")]
		[Test(Description = "This sample shows a join which is then grouped")]
		public async Task DLinqJoin9bAsync()
		{
			var q =
				from c in db.Customers
				join o in db.Orders on c.CustomerId equals o.Customer.CustomerId
				group o by c into x
					select new
					{
					CustomerName = x.Key.ContactName, Order = x
					}

			;
			await (ObjectDumper.WriteAsync(q));
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses WHERE to filter for orders with shipping date equals to null.")]
		public void DLinq2B()
		{
			IQueryable<Order> q =
				from o in db.Orders
				where o.ShippingDate == null
				select o;
			AssertByIds(q, new[]{11008, 11019, 11039, 11040, 11045, 11051, 11054, 11058, 11059, 11061, 11062, 11065, 11068, 11070, 11071, 11072, 11073, 11074, 11075, 11076, 11077}, x => x.OrderId);
		}

		[Category("WHERE")]
		[Test(Description = "This sample uses WHERE to filter for orders with shipping date not equals to null.")]
		public void DLinq2C()
		{
			var q = (
				from o in db.Orders
				where o.ShippingDate != null
				select o.OrderId).ToArray();
			var withNullShippingDate = new[]{11008, 11019, 11039, 11040, 11045, 11051, 11054, 11058, 11059, 11061, 11062, 11065, 11068, 11070, 11071, 11072, 11073, 11074, 11075, 11076, 11077};
			Assert.AreEqual(809, q.Length);
			Assert.That(!q.Any(orderid => withNullShippingDate.Contains(orderid)));
		}
	}
}
#endif
