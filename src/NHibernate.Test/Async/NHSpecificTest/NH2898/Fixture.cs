﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.Intercept;
using NHibernate.Properties;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2898
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void OnSetUp()
		{
			cfg.Properties[Environment.CacheProvider] = typeof (BinaryFormatterCacheProvider).AssemblyQualifiedName;
			cfg.Properties[Environment.UseQueryCache] = "true";
			sessions = (ISessionFactoryImplementor) cfg.BuildSessionFactory();

			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				for (var i = 0; i < 5; i++)
				{
					var obj = new ItemWithLazyProperty
								  {
									  Id = i + 1,
									  Name = "Name #" + i,
									  Description = "Description #" + i,
								  };
					session.Save(obj);
				}

				tx.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			{
				session.Delete("from ItemWithLazyProperty");
				session.Flush();
			}
		}

		[Test]
		public async Task SecondLevelCacheWithCriteriaQueriesAsync()
		{
			using (var session = OpenSession())
			{
				var list = await (session.CreateCriteria(typeof (ItemWithLazyProperty))
					.Add(Restrictions.Gt("Id", 2))
					.SetCacheable(true)
					.ListAsync());
				Assert.AreEqual(3, list.Count);

				using (var cmd = session.Connection.CreateCommand())
				{
					cmd.CommandText = "DELETE FROM ItemWithLazyProperty";
					await (cmd.ExecuteNonQueryAsync(CancellationToken.None));
				}
			}

			using (var session = OpenSession())
			{
				//should bring from cache
				var list = await (session.CreateCriteria(typeof (ItemWithLazyProperty))
					.Add(Restrictions.Gt("Id", 2))
					.SetCacheable(true)
					.ListAsync());
				Assert.AreEqual(3, list.Count);
			}
		}

		[Test]
		public async Task SecondLevelCacheWithHqlQueriesAsync()
		{
			using (var session = OpenSession())
			{
				var list = await (session.CreateQuery("from ItemWithLazyProperty i where i.Id > 2")
					.SetCacheable(true)
					.ListAsync());
				Assert.AreEqual(3, list.Count);

				using (var cmd = session.Connection.CreateCommand())
				{
					cmd.CommandText = "DELETE FROM ItemWithLazyProperty";
					await (cmd.ExecuteNonQueryAsync(CancellationToken.None));
				}
			}

			using (var session = OpenSession())
			{
				//should bring from cache
				var list = await (session.CreateQuery("from ItemWithLazyProperty i where i.Id > 2")
					.SetCacheable(true)
					.ListAsync());
				Assert.AreEqual(3, list.Count);
			}
		}
	}
}