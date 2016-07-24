#if NET_4_5
using System;
using System.Collections;
using log4net;
using NUnit.Framework;

namespace NHibernate.Test.Stateless.Fetching
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class StatelessSessionFetchingTestAsync : TestCaseAsync
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (StatelessSessionFetchingTestAsync));
		protected override string MappingsAssembly
		{
			get
			{
				return "NHibernate.Test";
			}
		}

		protected override IList Mappings
		{
			get
			{
				return new[]{"Stateless.Fetching.Mappings.hbm.xml"};
			}
		}

		[Test]
		public async System.Threading.Tasks.Task DynamicFetchAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					DateTime now = DateTime.Now;
					User me = new User("me");
					User you = new User("you");
					Resource yourClock = new Resource("clock", you);
					Task task = new Task(me, "clean", yourClock, now); // :)
					await (s.SaveAsync(me));
					await (s.SaveAsync(you));
					await (s.SaveAsync(yourClock));
					await (s.SaveAsync(task));
					await (tx.CommitAsync());
				}

			using (IStatelessSession ss = sessions.OpenStatelessSession())
				using (ITransaction tx = ss.BeginTransaction())
				{
					ss.BeginTransaction();
					Task taskRef = (Task)await (ss.CreateQuery("from Task t join fetch t.Resource join fetch t.User").UniqueResultAsync());
					Assert.That(taskRef, Is.Not.Null);
					Assert.That(NHibernateUtil.IsInitialized(taskRef), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(taskRef.User), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(taskRef.Resource), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(taskRef.Resource.Owner), Is.False);
					await (tx.CommitAsync());
				}

			await (cleanupAsync());
		}

		private async System.Threading.Tasks.Task cleanupAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					s.BeginTransaction();
					await (s.CreateQuery("delete Task").ExecuteUpdateAsync());
					await (s.CreateQuery("delete Resource").ExecuteUpdateAsync());
					await (s.CreateQuery("delete User").ExecuteUpdateAsync());
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
