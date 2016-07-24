#if NET_4_5
using System;
using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.DriverTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class NullReferenceFixtureAsync : TestCaseAsync
	{
		protected override IList Mappings
		{
			get
			{
				return new string[]{"Simple.hbm.xml"};
			}
		}

		/// <summary>
		/// No value is assigned for the given named parameter
		/// </summary>
		/// <remarks>
		/// Was giving a NullReferenceException in NHibernate.Driver.DriverBase.GenerateParameter
		/// at the line "dbParam.DbType = parameter.SqlType.DbType;" because "parameter" existed
		/// but all properties were null.
		/// </remarks>
		/// TODO: I think this fixture is redundant now due to the QueryTest fixtures, just mark it so that it catches the new exception type for now
		[Test]
		public async Task NoParameterNameNullReferenceAsync()
		{
			ISession s = null;
			try
			{
				s = OpenSession();
				IQuery q = s.CreateQuery("from Simple s where s.Name = :missing");
				Assert.IsNotNull(q);
				await (q.ListAsync());
			}
			catch (QueryException ae)
			{
				Assert.IsTrue(ae.Message.StartsWith("Not all named parameters have been set"));
			}
			finally
			{
				s.Close();
			}
		}

		/// <summary>
		/// query succeeds when the parameter is assigned a value
		/// </summary>
		[Test]
		public async Task NamedParameterAssignedNoErrorAsync()
		{
			ISession s = null;
			try
			{
				s = OpenSession();
				Console.WriteLine("about to run query");
				IQuery q = s.CreateQuery("from Simple s where s.Name = :missing");
				Assert.IsNotNull(q);
				q.SetString("missing", "nhibernate");
				IList list = await (q.ListAsync());
				Assert.IsNotNull(list);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				throw;
			}
			finally
			{
				s.Close();
			}
		}
	}
}
#endif
