﻿#if NET_4_5
using System;
using System.Collections;
using NHibernate.Criterion;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Criteria
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class DetachedCriteriaSerializableAsync : TestCaseAsync
	{
		protected override string MappingsAssembly
		{
			get
			{
				return "NHibernate.Test";
			}
		}

		protected override IList Mappings
		{
			get
			{
				return new string[]{"Criteria.Enrolment.hbm.xml"};
			}
		}

		private async Task SerializeAndListAsync(DetachedCriteria dc)
		{
			byte[] bytes = SerializationHelper.Serialize(dc);
			DetachedCriteria dcs = (DetachedCriteria)SerializationHelper.Deserialize(bytes);
			using (ISession s = OpenSession())
			{
				await (dcs.GetExecutableCriteria(s).ListAsync());
			}
		}

		[Test]
		public async Task DetachedCriteriaItSelfAsync()
		{
			DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Expression.Eq("Name", "Gavin King"));
			await (SerializeAndListAsync(dc));
		}

		[Test]
		public async Task ExecutableCriteriaAsync()
		{
			// All query below don't have sense, are only to test if all needed classes are serializable
			// Basic criterion
			DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Expression.Between("Name", "aaaaa", "zzzzz")).Add(Expression.EqProperty("Name", "Name")).Add(Expression.Ge("Name", "a")).Add(Expression.GeProperty("Name", "Name")).Add(Expression.Gt("Name", "z")).Add(Expression.GtProperty("Name", "Name")).Add(Expression.IdEq(1)).Add(Expression.In("Name", new string[]{"Gavin", "Ralph"})).Add(Expression.InsensitiveLike("Name", "GAVIN")).Add(Expression.IsEmpty("Enrolments")).Add(Expression.IsNotEmpty("Enrolments")).Add(Expression.IsNotNull("PreferredCourse")).Add(Expression.IsNull("PreferredCourse")).Add(Expression.Le("Name", "a")).Add(Expression.LeProperty("Name", "Name")).Add(Expression.Lt("Name", "z")).Add(Expression.LtProperty("Name", "Name")).Add(Expression.Like("Name", "G%")).Add(Expression.Not(Expression.Eq("Name", "Ralph"))).Add(Expression.NotEqProperty("Name", "Name")).AddOrder(Order.Asc("StudentNumber")).SetProjection(Property.ForName("StudentNumber"));
			await (SerializeAndListAsync(dc));
			// Like match modes
			dc = DetachedCriteria.For(typeof (Student)).Add(Expression.Like("Name", "Gavin", MatchMode.Anywhere)).Add(Expression.Like("Name", "Gavin", MatchMode.End)).Add(Expression.Like("Name", "Gavin", MatchMode.Exact)).Add(Expression.Like("Name", "Gavin", MatchMode.Start));
			await (SerializeAndListAsync(dc));
			// Logical Expression
			dc = DetachedCriteria.For(typeof (Student)).Add(Expression.Or(Expression.Eq("Name", "Ralph"), Expression.Eq("Name", "Gavin"))).Add(Expression.And(Expression.Gt("StudentNumber", 1L), Expression.Lt("StudentNumber", 10L)));
			await (SerializeAndListAsync(dc));
			// Projections
			dc = DetachedCriteria.For(typeof (Enrolment)).SetProjection(Projections.Distinct(Projections.ProjectionList().Add(Projections.Property("StudentNumber"), "stNumber").Add(Projections.Property("CourseCode"), "cCode"))).Add(Expression.Lt("StudentNumber", 668L));
			await (SerializeAndListAsync(dc));
			dc = DetachedCriteria.For(typeof (Enrolment)).SetProjection(Projections.Count("StudentNumber").SetDistinct());
			await (SerializeAndListAsync(dc));
			dc = DetachedCriteria.For(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(Projections.Count("StudentNumber")).Add(Projections.Max("StudentNumber")).Add(Projections.Min("StudentNumber")).Add(Projections.Avg("StudentNumber")));
			await (SerializeAndListAsync(dc));
			// Junctions
			dc = DetachedCriteria.For(typeof (Student)).Add(Expression.Conjunction().Add(Expression.Eq("Name", "Ralph")).Add(Expression.Eq("StudentNumber", 1L))).Add(Expression.Disjunction().Add(Expression.Eq("Name", "Ralph")).Add(Expression.Eq("Name", "Gavin")));
			await (SerializeAndListAsync(dc));
			// Subquery
			dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("StudentNumber").Eq(232L)).SetProjection(Property.ForName("Name"));
			DetachedCriteria dcs = DetachedCriteria.For(typeof (Student)).Add(Subqueries.PropertyEqAll("Name", dc));
			await (SerializeAndListAsync(dc));
			// SQLCriterion
			dc = DetachedCriteria.For(typeof (Student)).Add(Expression.Sql("{alias}.Name = 'Gavin'"));
			await (SerializeAndListAsync(dc));
			// SQLProjection
			dc = DetachedCriteria.For(typeof (Enrolment)).SetProjection(Projections.SqlProjection("1 as constOne, count(*) as countStar", new String[]{"constOne", "countStar"}, new IType[]{NHibernateUtil.Int32, NHibernateUtil.Int32}));
			await (SerializeAndListAsync(dc));
			dc = DetachedCriteria.For(typeof (Student)).SetProjection(Projections.SqlGroupProjection("COUNT({alias}.studentId), {alias}.preferredCourseCode", "{alias}.preferredCourseCode", new string[]{"studentsOfCourse", "CourseCode"}, new IType[]{NHibernateUtil.Int32, NHibernateUtil.Int32}));
			await (SerializeAndListAsync(dc));
			// Result transformers
			dc = DetachedCriteria.For(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Projections.Property("st.Name"), "studentName").Add(Projections.Property("co.Description"), "courseDescription")).AddOrder(Order.Desc("studentName")).SetResultTransformer(Transformers.AliasToBean(typeof (StudentDTO)));
			await (SerializeAndListAsync(dc));
		}
	}
}
#endif
