#if NET_4_5
using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2565
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		private class TaskSavedScenario : IDisposable
		{
			private readonly ISessionFactory factory;
			private readonly Guid taskId;
			public TaskSavedScenario(ISessionFactory factory)
			{
				this.factory = factory;
				var activity = new TaskActivity{Name = "Say Hello!"};
				var task = new Task{Description = "Nice to do", Activity = activity};
				using (var s = factory.OpenSession())
					using (var tx = s.BeginTransaction())
					{
						s.Persist(task);
						taskId = task.Id;
						tx.Commit();
					}
			}

			public Guid TaskId
			{
				get
				{
					return taskId;
				}
			}

			public void Dispose()
			{
				using (var s = factory.OpenSession())
					using (var tx = s.BeginTransaction())
					{
						s.Delete(s.Get<Task>(taskId));
						tx.Commit();
					}
			}
		}

		[Test]
		public async System.Threading.Tasks.Task WhenUseLoadThenCanUsePersistToModifyAsync()
		{
			using (var scenario = new TaskSavedScenario(Sfi))
			{
				using (var s = OpenSession())
					using (var tx = s.BeginTransaction())
					{
						var task = await (s.LoadAsync<Task>(scenario.TaskId));
						task.Description = "Could be something nice";
						await (s.PersistAsync(task));
						Assert.That(async () => await (s.PersistAsync(task)), Throws.Nothing);
						await (tx.CommitAsync());
					}
			}
		}

		[Test]
		public async System.Threading.Tasks.Task WhenUseGetThenCanUsePersistToModifyAsync()
		{
			using (var scenario = new TaskSavedScenario(Sfi))
			{
				using (var s = OpenSession())
					using (var tx = s.BeginTransaction())
					{
						var task = await (s.GetAsync<Task>(scenario.TaskId));
						task.Description = "Could be something nice";
						Assert.That(async () => await (s.PersistAsync(task)), Throws.Nothing);
						await (tx.CommitAsync());
					}
			}
		}
	}
}
#endif
