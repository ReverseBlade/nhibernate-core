#if NET_4_5
using System;
using NHibernate.DomainModel;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Util;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace NHibernate.Test.ExpressionTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class InExpressionFixtureAsync : BaseExpressionFixture
	{
		[Test]
		public async Task InSqlStringTestAsync()
		{
			ISession session = factory.OpenSession();
			ICriterion inExpression = Expression.In("Count", new int[]{3, 4, 5});
			CreateObjects(typeof (Simple), session);
			SqlString sqlString = await (inExpression.ToSqlStringAsync(criteria, criteriaQuery, new CollectionHelper.EmptyMapClass<string, IFilter>()));
			string expectedSql = "sql_alias.count_ in (?, ?, ?)";
			CompareSqlStrings(sqlString, expectedSql, 3);
			session.Close();
		}

		[Test]
		public async Task InEmptyListAsync()
		{
			ISession session = factory.OpenSession();
			InExpression expression = new InExpression("Count", new object[0]);
			CreateObjects(typeof (Simple), session);
			SqlString sql = await (expression.ToSqlStringAsync(criteria, criteriaQuery, new CollectionHelper.EmptyMapClass<string, IFilter>()));
			Assert.AreEqual("1=0", sql.ToString());
			session.Close();
		}

		[Test]
		public async Task InSqlFunctionTestAsync()
		{
			using (var session = factory.OpenSession())
			{
				CreateObjects(typeof (Simple), session);
				var inExpression = Restrictions.In(Projections.SqlFunction("substring", NHibernateUtil.String, Projections.Property("Name"), Projections.Constant(1), Projections.Constant(1)), new object[]{"A", "B"});
				var sql = await (inExpression.ToSqlStringAsync(criteria, criteriaQuery, new CollectionHelper.EmptyMapClass<string, IFilter>()));
				// Allow some dialectal differences in function name and parameter style.
				Assert.That(sql.ToString(), Is.StringStarting("substring(sql_alias.Name").Or.StringStarting("substr(sql_alias.Name"));
				Assert.That(sql.ToString(), Is.StringEnding(") in (?, ?)"));
				// Ensure no parameters are duplicated.
				var parameters = criteriaQuery.CollectedParameters.ToList();
				Assert.That(parameters.Count, Is.EqualTo(4));
				Assert.That(parameters[0].Value, Is.EqualTo(1));
				Assert.That(parameters[1].Value, Is.EqualTo(1));
				Assert.That(parameters[2].Value, Is.EqualTo("A"));
				Assert.That(parameters[3].Value, Is.EqualTo("B"));
			}
		}
	}
}
#endif
