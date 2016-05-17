#if NET_4_5
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH995
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : BugTestCase
	{
		[Test]
		public async Task TestAsync()
		{
			int a_id;
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					// Create an A and save it
					ClassA a = new ClassA();
					a.Name = "a1";
					await (s.SaveAsync(a));
					// Create a B and save it
					ClassB b = new ClassB();
					b.Id = new ClassBId("bbb", a);
					b.SomeProp = "Some property";
					await (s.SaveAsync(b));
					// Create a C and save it
					ClassC c = new ClassC();
					c.B = b;
					await (s.SaveAsync(c));
					await (tx.CommitAsync());
					a_id = a.Id;
				}

			// Clear the cache
			sessions.Evict(typeof (ClassA));
			sessions.Evict(typeof (ClassB));
			sessions.Evict(typeof (ClassC));
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					// Load a so we can use it to load b
					ClassA a = await (s.GetAsync<ClassA>(a_id));
					// Load b so b will be in cache
					ClassB b = await (s.GetAsync<ClassB>(new ClassBId("bbb", a)));
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					using (SqlLogSpy sqlLogSpy = new SqlLogSpy())
					{
						IList<ClassC> c_list = s.CreateCriteria(typeof (ClassC)).List<ClassC>();
						// make sure we initialize B
						NHibernateUtil.Initialize(c_list[0].B);
						Assert.AreEqual(1, sqlLogSpy.Appender.GetEvents().Length, "Only one SQL should have been issued");
					}

					await (tx.CommitAsync());
				}
		}
	}
}
#endif
