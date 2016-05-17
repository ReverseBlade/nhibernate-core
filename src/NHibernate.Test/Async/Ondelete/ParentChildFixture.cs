#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Stat;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Ondelete
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ParentChildFixture : TestCase
	{
		[Test]
		public async Task ParentChildCascadeAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Parent p = new Parent("foo");
			p.Children.Add(new Child(p, "foo1"));
			p.Children.Add(new Child(p, "foo2"));
			Parent q = new Parent("bar");
			q.Children.Add(new Child(q, "bar1"));
			q.Children.Add(new Child(q, "bar2"));
			await (s.SaveAsync(p));
			await (s.SaveAsync(q));
			await (t.CommitAsync());
			s.Close();
			IStatistics statistics = sessions.Statistics;
			statistics.Clear();
			s = OpenSession();
			t = s.BeginTransaction();
			IList<Parent> l = s.CreateQuery("from Parent").List<Parent>();
			p = l[0];
			q = l[1];
			statistics.Clear();
			await (s.DeleteAsync(p));
			await (s.DeleteAsync(q));
			await (t.CommitAsync());
			Assert.AreEqual(2, statistics.PrepareStatementCount);
			Assert.AreEqual(6, statistics.EntityDeleteCount);
			t = s.BeginTransaction();
			IList names = s.CreateQuery("from Parent p").List();
			Assert.AreEqual(0, names.Count);
			await (t.CommitAsync());
			s.Close();
		}
	}
}
#endif
