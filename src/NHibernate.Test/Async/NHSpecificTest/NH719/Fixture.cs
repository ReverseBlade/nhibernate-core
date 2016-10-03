﻿#if NET_4_5
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH719
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		public override string BugNumber
		{
			get
			{
				return "NH719";
			}
		}

		[Test]
		public async Task CacheLoadTestAsync()
		{
			//Instantiate and setup associations (all needed to generate the error);
			A a = new A(1, "aaa");
			B b = new B(2, "bbb");
			NotCached notCached = new NotCached(1, b);
			Cached cached = new Cached(1, a);
			try
			{
				using (ISession session = sessions.OpenSession())
				{
					await (session.SaveAsync(a));
					await (session.SaveAsync(b));
					await (session.SaveAsync(notCached));
					await (session.SaveAsync(cached));
					await (session.FlushAsync());
				}

				using (ISession session = sessions.OpenSession())
				{
					// runs OK, since it's not cached
					NotCached nc = (NotCached)await (session.LoadAsync(typeof (NotCached), 1));
					Assert.AreEqual("bbb", ((B)nc.Owner).Foo);
					// 1st run OK, not yet in cache
					Cached ca = (Cached)await (session.LoadAsync(typeof (Cached), 1));
					Assert.AreEqual("aaa", ((A)ca.Owner).Foo);
				}

				// 2nd run fails, when data is read from the cache
				using (ISession session = sessions.OpenSession())
				{
					// runs OK, since it's not cached
					NotCached nc = (NotCached)await (session.LoadAsync(typeof (NotCached), 1));
					Assert.AreEqual("bbb", ((B)nc.Owner).Foo);
					// 2nd run fails, when loaded from in cache
					Cached ca = (Cached)await (session.LoadAsync(typeof (Cached), 1));
					Assert.AreEqual("aaa", ((A)ca.Owner).Foo);
				}
			}
			finally
			{
				using (ISession session = OpenSession())
				{
					await (session.DeleteAsync(notCached));
					await (session.DeleteAsync(cached));
					await (session.DeleteAsync(a));
					await (session.DeleteAsync(b));
					await (session.FlushAsync());
				}
			}
		}
	}
}
#endif
