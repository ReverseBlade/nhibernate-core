#if NET_4_5
using System.Collections.Generic;
using System.Data.Common;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1405
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		[Test]
		public async Task BugAsync()
		{
			string[] populate = new string[]{"insert into PPDM_COLUMN ( SYSTEM_ID, TABLE_NAME, COLUMN_NAME, CONTROL_COLUMN ) values ( 'SYSTEM', 'TABLE', 'COLUMN1', null )", "insert into PPDM_COLUMN ( SYSTEM_ID, TABLE_NAME, COLUMN_NAME, CONTROL_COLUMN ) values ( 'SYSTEM', 'TABLE', 'COLUMN2', 'COLUMN1' )", "insert into PPDM_COLUMN ( SYSTEM_ID, TABLE_NAME, COLUMN_NAME, CONTROL_COLUMN ) values ( 'SYSTEM', 'TABLE', 'COLUMN3', 'COLUMN2' )"};
			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					foreach (string sql in populate)
					{
						DbCommand cmd = session.Connection.CreateCommand();
						cmd.CommandText = sql;
						tx.Enlist(cmd);
						await (cmd.ExecuteNonQueryAsync());
					}

					await (tx.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					IQuery query = session.CreateQuery("from Column");
					IList<Column> columns = await (query.ListAsync<Column>());
					Assert.AreEqual(3, columns.Count);
					foreach (Column column in columns)
					{
						Assert.IsNotNull(column.ColumnName, "Column.ColumnName should not be null.");
						Assert.IsFalse((null != column.ControlColumn) && (null == column.ControlColumn.ColumnName), "Column's control column's ColumnName should not be null.");
					}

					await (tx.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction tx = session.BeginTransaction())
				{
					DbCommand cmd = session.Connection.CreateCommand();
					cmd.CommandText = "DELETE FROM PPDM_COLUMN";
					tx.Enlist(cmd);
					await (cmd.ExecuteNonQueryAsync());
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
