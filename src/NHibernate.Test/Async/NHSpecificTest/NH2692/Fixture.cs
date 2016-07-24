#if NET_4_5
using System;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.NHSpecificTest.NH2692
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		[Test]
		public void QueryingParentWhichHasChildren()
		{
			using (var session = OpenSession())
				using (session.BeginTransaction())
				{
					var result = session.Query<Parent>().Where(x => x.ChildComponents.Any()).ToList();
					Assert.That(result, Has.Count.EqualTo(1));
				}
		}

		[Test]
		public Task QueryingChildrenComponentsAsync()
		{
			try
			{
				Assert.Throws<Exception>(() =>
				{
					using (var session = OpenSession())
						using (session.BeginTransaction())
						{
							var result = session.Query<Parent>().SelectMany(x => x.ChildComponents).ToList();
							Assert.That(result, Has.Count.EqualTo(1));
						}
				}

				, KnownBug.Issue("NH-2692"));
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		[Test]
		public async Task QueryingChildrenComponentsHqlAsync()
		{
			Assert.ThrowsAsync<Exception>(async () =>
			{
				using (var session = OpenSession())
					using (session.BeginTransaction())
					{
						var result = await (session.CreateQuery("select c from Parent as p join p.ChildComponents as c").ListAsync<ChildComponent>());
						Assert.That(result, Has.Count.EqualTo(1));
					}
			}

			, KnownBug.Issue("NH-2692"));
		}

		protected override async Task OnSetUpAsync()
		{
			using (var session = OpenSession())
				using (var tx = session.BeginTransaction())
				{
					var parent1 = new Parent();
					var child1 = new ChildComponent{Parent = parent1, SomeBool = true, SomeString = "something"};
					parent1.ChildComponents.Add(child1);
					var parent2 = new Parent();
					await (session.SaveAsync(parent1));
					await (session.SaveAsync(parent2));
					await (tx.CommitAsync());
				}
		}

		protected override async Task OnTearDownAsync()
		{
			using (var session = OpenSession())
				using (var tx = session.BeginTransaction())
				{
					await (session.DeleteAsync("from Parent"));
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
