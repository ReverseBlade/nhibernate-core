#if NET_4_5
using System;
using NHibernate.Cfg;
using NUnit.Framework;
using NHibernate.Cfg.Loquacious;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.NHSpecificTest.NH2228
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		public partial class ParentWithTwoChildrenScenario : IDisposable
		{
			private readonly ISessionFactory factory;
			private readonly int parentId;
			public ParentWithTwoChildrenScenario(ISessionFactory factory)
			{
				this.factory = factory;
				var parent = new Parent();
				parent.Children.Add(new Child{Description = "Child1", Parent = parent});
				parent.Children.Add(new Child{Description = "Child2", Parent = parent});
				using (var s = factory.OpenSession())
				{
					parentId = (int)s.Save(parent);
					s.Flush();
				}
			}

			public int ParentId
			{
				get
				{
					return parentId;
				}
			}

			public void Dispose()
			{
				using (var s = factory.OpenSession())
				{
					s.Delete("from Parent");
					s.Flush();
				}
			}
		}

		protected override Task ConfigureAsync(NHibernate.Cfg.Configuration configuration)
		{
			try
			{
				// needed to be sure of StaleStateException that the user has reported in the issue
				configuration.DataBaseIntegration(x => x.BatchSize = 1);
				return TaskHelper.CompletedTask;
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		[Test]
		public async Task WhenStaleObjectStateThenMessageContainsEntityAsync()
		{
			using (var scenario = new ParentWithTwoChildrenScenario(Sfi))
			{
				using (var client1 = OpenSession())
				{
					var parentFromClient1 = await (client1.GetAsync<Parent>(scenario.ParentId));
					await (NHibernateUtil.InitializeAsync(parentFromClient1.Children));
					var firstChildId = parentFromClient1.Children[0].Id;
					await (DeleteChildUsingAnotherSessionAsync(firstChildId));
					using (var tx1 = client1.BeginTransaction())
					{
						parentFromClient1.Children[0].Description = "Modified info";
						var expectedException = Assert.ThrowsAsync<StaleObjectStateException>(async () => await (tx1.CommitAsync()));
						Assert.That(expectedException.EntityName, Is.EqualTo(typeof (Child).FullName));
						Assert.That(expectedException.Identifier, Is.EqualTo(firstChildId));
					}
				}
			}
		}

		private async Task DeleteChildUsingAnotherSessionAsync(int childIdToDelete)
		{
			using (var client2 = Sfi.OpenStatelessSession())
				using (var tx2 = client2.BeginTransaction())
				{
					await (client2.CreateQuery("delete from Child c where c.Id = :pChildId").SetInt32("pChildId", childIdToDelete).ExecuteUpdateAsync());
					await (tx2.CommitAsync());
				}
		}
	}
}
#endif
