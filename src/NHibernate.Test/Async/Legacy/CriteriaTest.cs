#if NET_4_5
using System;
using System.Collections;
using NHibernate.DomainModel;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Legacy
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class CriteriaTestAsync : TestCaseAsync
	{
		protected override IList Mappings
		{
			get
			{
				return new string[]{"Simple.hbm.xml", "MasterDetail.hbm.xml"};
			}
		}

		[Test]
		public async Task SimpleSelectTestAsync()
		{
			// create the objects to search on
			long simple1Key = 15;
			Simple simple1 = new Simple();
			simple1.Address = "Street 12";
			simple1.Date = DateTime.Now;
			simple1.Name = "For Criteria Test";
			simple1.Count = 16;
			long notSimple1Key = 17;
			Simple notSimple1 = new Simple();
			notSimple1.Address = "Street 123";
			notSimple1.Date = DateTime.Now;
			notSimple1.Name = "Don't be found";
			notSimple1.Count = 18;
			using (ISession s1 = OpenSession())
				using (ITransaction t1 = s1.BeginTransaction())
				{
					await (s1.SaveAsync(notSimple1, notSimple1Key));
					await (s1.SaveAsync(simple1, simple1Key));
					await (t1.CommitAsync());
				}

			using (ISession s2 = OpenSession())
				using (ITransaction t2 = s2.BeginTransaction())
				{
					IList results2 = await (s2.CreateCriteria(typeof (Simple)).Add(Expression.Eq("Address", "Street 12")).ListAsync());
					Assert.AreEqual(1, results2.Count);
					Simple simple2 = (Simple)results2[0];
					Assert.IsNotNull(simple2, "Unable to load object");
					Assert.AreEqual(simple1.Count, simple2.Count, "Load failed");
					Assert.AreEqual(simple1.Name, simple2.Name, "Load failed");
					Assert.AreEqual(simple1.Address, simple2.Address, "Load failed");
					Assert.AreEqual(simple1.Date.ToString(), simple2.Date.ToString(), "Load failed");
					await (s2.DeleteAsync("from Simple"));
					await (t2.CommitAsync());
				}
		}

		[Test]
		public async Task SimpleDateCriteriaAsync()
		{
			Simple s1 = new Simple();
			s1.Address = "blah";
			s1.Count = 1;
			s1.Date = new DateTime(2004, 01, 01);
			Simple s2 = new Simple();
			s2.Address = "blah";
			s2.Count = 2;
			s2.Date = new DateTime(2006, 01, 01);
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(s1, 1L));
				await (s.SaveAsync(s2, 2L));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				IList results = await (s.CreateCriteria(typeof (Simple)).Add(Expression.Gt("Date", new DateTime(2005, 01, 01))).AddOrder(Order.Asc("Date")).ListAsync());
				Assert.AreEqual(1, results.Count, "one gt from 2005");
				Simple simple = (Simple)results[0];
				Assert.IsTrue(simple.Date > new DateTime(2005, 01, 01), "should have returned dates after 2005");
				results = await (s.CreateCriteria(typeof (Simple)).Add(Expression.Lt("Date", new DateTime(2005, 01, 01))).AddOrder(Order.Asc("Date")).ListAsync());
				Assert.AreEqual(1, results.Count, "one lt than 2005");
				simple = (Simple)results[0];
				Assert.IsTrue(simple.Date < new DateTime(2005, 01, 01), "should be less than 2005");
				await (s.DeleteAsync("from Simple"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task CriteriaTypeMismatchAsync()
		{
			using (ISession s = OpenSession())
			{
				Assert.ThrowsAsync<QueryException>(async () => await (s.CreateCriteria(typeof (Master)).Add(Expression.Like("Details", "SomeString")).ListAsync()));
			}
		}

		[Test]
		public async Task CriteriaManyToOneEqualsAsync()
		{
			using (ISession s = OpenSession())
			{
				Master master = new Master();
				await (s.SaveAsync(master));
				await (s.CreateCriteria(typeof (Detail)).Add(Expression.Eq("Master", master)).ListAsync());
				await (s.DeleteAsync(master));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task CriteriaCompositePropertyAsync()
		{
			using (ISession s = OpenSession())
			{
				Assert.ThrowsAsync<QueryException>(async () => await (s.CreateCriteria(typeof (Master)).Add(Expression.Eq("Details.I", 10)).ListAsync()));
			}
		}

		[Test]
		public async Task CriteriaLeftOuterJoinAsync()
		{
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(new Master()));
				await (s.FlushAsync());
				Assert.AreEqual(1, (await (s.CreateCriteria(typeof (Master)).CreateAlias("Details", "detail", JoinType.LeftOuterJoin).SetFetchMode("Details", FetchMode.Join).ListAsync())).Count);
				await (s.DeleteAsync("from Master"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public void Criteria_can_get_query_entity_type()
		{
			using (ISession s = OpenSession())
			{
				Assert.AreEqual(typeof (Master), s.CreateCriteria(typeof (Master)).GetRootEntityTypeIfAvailable());
			}
		}

		[Test]
		public void DetachedCriteria_can_get_query_entity_type()
		{
			Assert.AreEqual(typeof (Master), DetachedCriteria.For<Master>().GetRootEntityTypeIfAvailable());
		}
	}
}
#endif
