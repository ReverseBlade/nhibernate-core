#if NET_4_5
using NHibernate.Dialect;
using NHibernate.SqlCommand;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.DialectTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Ingres9DialectFixtureAsync
	{
		[Test]
		public void GetLimitString()
		{
			var d = new Ingres9Dialect();
			var str = d.GetLimitString(new SqlString("SELECT * FROM fish"), null, null);
			Assert.That(str.ToString(), Is.EqualTo("SELECT * FROM fish"));
			str = d.GetLimitString(new SqlString("SELECT * FROM fish"), null, new SqlString("10"));
			Assert.That(str.ToString(), Is.EqualTo("SELECT * FROM fish fetch first 10 rows only"));
			str = d.GetLimitString(new SqlString("SELECT * FROM fish"), new SqlString("23"), null);
			Assert.That(str.ToString(), Is.EqualTo("SELECT * FROM fish offset 23"));
			str = d.GetLimitString(new SqlString("SELECT * FROM fish"), new SqlString("5"), new SqlString("15"));
			Assert.That(str.ToString(), Is.EqualTo("SELECT * FROM fish offset 5 fetch next 15 rows only"));
		}
	}
}
#endif
