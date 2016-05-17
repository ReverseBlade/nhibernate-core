#if NET_4_5
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1301
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : BugTestCase
	{
		[Test]
		public async Task TestAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					ClassA a = new ClassA();
					a.BCollection.Add(new ClassB());
					await (s.SaveAsync(a));
					await (s.FlushAsync());
					s.Clear();
					//dont know if proxy should be able to refresh
					//so I eager/join load here just to show it doesn't work anyhow...
					ClassA loaded = s.CreateCriteria(typeof (ClassA)).SetFetchMode("BCollection", FetchMode.Join).List<ClassA>()[0];
					Assert.AreEqual(1, a.BCollection.Count);
					loaded.BCollection.RemoveAt(0);
					Assert.AreEqual(0, loaded.BCollection.Count);
					s.Refresh(loaded);
					Assert.AreEqual(1, loaded.BCollection.Count);
					await (s.DeleteAsync(loaded));
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
