﻿#if NET_4_5
using System.Collections.Generic;
using System.Text;
using NHibernate.Criterion;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1990
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		protected override async Task OnSetUpAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					for (int i = 0; i < 10; i++)
					{
						var feed = new NewsFeed{Url = string.Format("Feed{0}Uri", i), Title = string.Format("Feed{0}", i), Status = (i % 2 == 0 ? 1 : 2)};
						await (s.SaveAsync(feed));
						for (int j = 0; j < 8; j++)
						{
							var item = new NewsItem{Title = string.Format("Feed{0}Item{1}", i, j), Status = (j % 2 == 0 ? 1 : 2), Feed = feed};
							await (s.SaveAsync(item));
						}
					}

					await (tx.CommitAsync());
				}
			}
		}

		protected override async Task OnTearDownAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.DeleteAsync(string.Format("from {0}", typeof (NewsItem).Name)));
					await (s.DeleteAsync(string.Format("from {0}", typeof (NewsFeed).Name)));
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task FetchingBySubqueryFilterParametersAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IFilter filter = s.EnableFilter("StatusFilter");
					filter.SetParameter("Status", 1);
					ICriteria criteria = s.CreateCriteria(typeof (NewsFeed), "NewsFeed");
					IList<NewsFeed> feeds = await (criteria.ListAsync<NewsFeed>());
					Assert.That(feeds.Count, Is.EqualTo(5));
					foreach (NewsFeed feed in feeds)
					{
						Assert.That(feed.Items.Count, Is.EqualTo(4));
					}

					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task FetchingBySubqueryFilterParametersAndPositionalParametersAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IFilter filter = s.EnableFilter("StatusFilter");
					filter.SetParameter("Status", 1);
					ICriteria criteria = s.CreateCriteria(typeof (NewsFeed), "NewsFeed");
					criteria.Add(Restrictions.In("Url", new[]{"Feed2Uri", "Feed4Uri", "Feed8Uri"}));
					IList<NewsFeed> feeds = await (criteria.ListAsync<NewsFeed>());
					Assert.That(feeds.Count, Is.EqualTo(3));
					foreach (NewsFeed feed in feeds)
					{
						Assert.That(feed.Items.Count, Is.EqualTo(4));
					}

					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task FetchingBySubqueryFilterParametersAndPositionalParametersAndNamedParametersAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IFilter filter = s.EnableFilter("StatusFilter");
					filter.SetParameter("Status", 1);
					var hql = new StringBuilder();
					hql.AppendLine("from NewsFeed");
					hql.AppendLine("where (Url = ? or Url = ?) and Title in (:TitleList) ");
					IQuery query = s.CreateQuery(hql.ToString());
					query.SetString(0, "Feed4Uri");
					query.SetString(1, "Feed8Uri");
					query.SetParameterList("TitleList", new[]{"Feed2", "Feed4", "Feed8"});
					IList<NewsFeed> feeds = await (query.ListAsync<NewsFeed>());
					Assert.That(feeds.Count, Is.EqualTo(2));
					foreach (NewsFeed feed in feeds)
					{
						Assert.That(feed.Items.Count, Is.EqualTo(4));
					}

					await (tx.CommitAsync());
				}
			}
		}
	}
}
#endif
