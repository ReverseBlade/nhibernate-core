#if NET_4_5
using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1293
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : BugTestCase
	{
		[Test]
		public async Task Criteria_Does_Not_Equal_To_HQLAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					Customer c = new Customer("Somebody");
					c.Category = new Category("User");
					await (s.SaveAsync(c.Category));
					c.IsActive = true;
					c.Category.IsActive = false; // this cause diff in query results
					await (s.SaveAsync(c));
					await (tx.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.DisableFilter("onlyActive");
				IFilter fltr = s.EnableFilter("onlyActive");
				if (Dialect is PostgreSQLDialect)
					fltr.SetParameter("activeFlag", true);
				else
					fltr.SetParameter("activeFlag", 1);
				// with HQL, Category.IsActive=true filter applied, result count=2
				IQuery hqlQuery = s.CreateQuery("from Customer c join c.Category cat where cat.Name = ?");
				hqlQuery.SetParameter(0, "User"); // note using positional parameters because of NH-1490
				IList<Customer> hqlResult = hqlQuery.List<Customer>();
				Console.WriteLine(hqlResult.Count);
				// with ICriteria, no Category.IsActive filter applied, result count=1
				ICriteria criteria = s.CreateCriteria(typeof (Customer), "cust").CreateCriteria("Category", "cat");
				criteria.Add(Restrictions.Eq("cat.Name", "User"));
				IList<Customer> criteriaResult = criteria.List<Customer>();
				Console.WriteLine(criteriaResult.Count);
				Assert.That(hqlResult.Count == criteriaResult.Count);
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Customer"));
					await (s.DeleteAsync("from Category"));
					await (tx.CommitAsync());
				}
			}
		}
	}
}
#endif
