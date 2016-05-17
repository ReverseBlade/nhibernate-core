#if NET_4_5
using System.Collections;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Cascade.OneToOneCascadeDelete.Pk.Bidirectional
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public abstract partial class DeleteOneToOneOrphansTest : TestCase
	{
		[Test]
		public async Task TestOrphanedWhileManagedAsync()
		{
			long empId;
			using (var s = OpenSession())
				using (var tx = s.BeginTransaction())
				{
					var empInfoList = s.CreateQuery("from EmployeeInfo").List<EmployeeInfo>();
					Assert.AreEqual(1, empInfoList.Count);
					var empList = s.CreateQuery("from Employee").List<Employee>();
					Assert.AreEqual(1, empList.Count);
					var emp = empList[0];
					Assert.NotNull(emp.Info);
					empId = emp.Id;
					emp.Info = null;
					await (tx.CommitAsync());
				}

			using (var s = OpenSession())
				using (var tx = s.BeginTransaction())
				{
					var emp = await (s.GetAsync<Employee>(empId));
					Assert.IsNull(emp.Info);
					var empInfoList = s.CreateQuery("from EmployeeInfo").List<EmployeeInfo>();
					Assert.AreEqual(0, empInfoList.Count);
					var empList = s.CreateQuery("from Employee").List<Employee>();
					Assert.AreEqual(1, empList.Count);
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
