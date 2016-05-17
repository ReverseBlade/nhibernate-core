#if NET_4_5
using System;
using System.Collections;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;
using System.Threading.Tasks;

namespace NHibernate.Test.DriverTest
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Sql2008DateTime2Test : TestCase
	{
		[Test]
		public async Task CrudAsync()
		{
			var expectedMoment = new DateTime(1848, 6, 1, 12, 00, 00, 123);
			var expectedLapse = new TimeSpan((DateTime.Now - expectedMoment).Ticks);
			object savedId;
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					savedId = await (s.SaveAsync(new EntityForMs2008{DateTimeProp = expectedMoment, TimeSpanProp = expectedLapse, }));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					var m = await (s.GetAsync<EntityForMs2008>(savedId));
					Assert.That(m.DateTimeProp, Is.EqualTo(expectedMoment));
					Assert.That(m.TimeSpanProp, Is.EqualTo(expectedLapse));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					s.CreateQuery("delete from EntityForMs2008").ExecuteUpdate();
					await (t.CommitAsync());
				}
		}
	}
}
#endif
