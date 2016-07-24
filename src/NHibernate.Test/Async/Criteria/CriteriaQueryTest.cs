#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Dialect;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Criteria
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class CriteriaQueryTestAsync : TestCaseAsync
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
				return new string[]{"Criteria.Enrolment.hbm.xml", "Criteria.Animal.hbm.xml", "Criteria.MaterialResource.hbm.xml"};
			}
		}

		[Test]
		public async Task EscapeCharacterAsync()
		{
			Course c1 = new Course();
			c1.CourseCode = "course-1";
			c1.Description = "%1";
			Course c2 = new Course();
			c2.CourseCode = "course-2";
			c2.Description = "%2";
			Course c3 = new Course();
			c3.CourseCode = "course-3";
			c3.Description = "control";
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.SaveAsync(c1));
					await (session.SaveAsync(c2));
					await (session.SaveAsync(c3));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
			{
				// finds all courses which have a description equal to '%1'
				Course example = new Course();
				example.Description = "&%1";
				IList result = await (session.CreateCriteria(typeof (Course)).Add(Example.Create(example).IgnoreCase().EnableLike().SetEscapeCharacter('&')).ListAsync());
				Assert.AreEqual(1, result.Count);
			}

			using (ISession session = OpenSession())
			{
				// finds all courses which contain '%' as the first char in the description
				Course example = new Course();
				example.Description = "&%%";
				IList result = await (session.CreateCriteria(typeof (Course)).Add(Example.Create(example).IgnoreCase().EnableLike().SetEscapeCharacter('&')).ListAsync());
				Assert.AreEqual(2, result.Count);
			}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.DeleteAsync(c1));
					await (session.DeleteAsync(c2));
					await (session.DeleteAsync(c3));
					await (t.CommitAsync());
				}
		}

		[Test, Ignore("ScrollableResults not implemented")]
		public async Task ScrollCriteriaAsync()
		{
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			await (session.SaveAsync(course));
			await (session.FlushAsync());
			session.Clear();
			//IScrollableResults sr = session.CreateCriteria(typeof(Course)).Scroll();
			//Assert.IsTrue( sr.Next() );
			//course = (Course) sr[0];
			Assert.IsNotNull(course);
			//sr.Close();
			await (session.DeleteAsync(course));
			await (t.CommitAsync());
			session.Close();
		}

		[Test]
		public async Task AllowToSetLimitOnSubqueriesAsync()
		{
			using (ISession session = OpenSession())
			{
				DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("StudentNumber").Eq(232L)).SetMaxResults(1).AddOrder(Order.Asc("Name")).SetProjection(Property.ForName("Name"));
				await (session.CreateCriteria(typeof (Student)).Add(Subqueries.PropertyEq("Name", dc)).ListAsync());
			}
		}

		[Test]
		public async Task TestSubcriteriaBeingNullAsync()
		{
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Course hibernateCourse = new Course();
			hibernateCourse.CourseCode = "HIB";
			hibernateCourse.Description = "Hibernate Training";
			await (session.SaveAsync(hibernateCourse));
			DetachedCriteria subcriteria = DetachedCriteria.For<Enrolment>("e");
			subcriteria.Add(Expression.EqProperty("e.CourseCode", "c.CourseCode"));
			subcriteria.SetProjection(Projections.Avg("Semester"));
			DetachedCriteria criteria = DetachedCriteria.For<Course>("c");
			criteria.SetProjection(Projections.Count("id"));
			criteria.Add(Expression.Or(Subqueries.Le(5, subcriteria), Subqueries.IsNull(subcriteria)));
			object o = await (criteria.GetExecutableCriteria(session).UniqueResultAsync());
			Assert.AreEqual(1, o);
			await (session.DeleteAsync(hibernateCourse));
			await (t.CommitAsync());
			session.Close();
		}

		[Test]
		public async Task SubselectAsync()
		{
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			await (session.SaveAsync(course));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 232;
			await (session.SaveAsync(gavin));
			Enrolment enrolment2 = new Enrolment();
			enrolment2.Course = course;
			enrolment2.CourseCode = course.CourseCode;
			enrolment2.Semester = 3;
			enrolment2.Year = 1998;
			enrolment2.Student = gavin;
			enrolment2.StudentNumber = gavin.StudentNumber;
			gavin.Enrolments.Add(enrolment2);
			await (session.SaveAsync(enrolment2));
			DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("StudentNumber").Eq(232L)).SetProjection(Property.ForName("Name"));
			if (TestDialect.SupportsOperatorAll)
			{
				await (session.CreateCriteria(typeof (Student)).Add(Subqueries.PropertyEqAll("Name", dc)).ListAsync());
			}

			await (session.CreateCriteria(typeof (Student)).Add(Subqueries.Exists(dc)).ListAsync());
			if (TestDialect.SupportsOperatorAll)
			{
				await (session.CreateCriteria(typeof (Student)).Add(Property.ForName("Name").EqAll(dc)).ListAsync());
			}

			await (session.CreateCriteria(typeof (Student)).Add(Subqueries.In("Gavin King", dc)).ListAsync());
			DetachedCriteria dc2 = DetachedCriteria.For(typeof (Student), "st").Add(Property.ForName("st.StudentNumber").EqProperty("e.StudentNumber")).SetProjection(Property.ForName("Name"));
			await (session.CreateCriteria(typeof (Enrolment), "e").Add(Subqueries.Eq("Gavin King", dc2)).ListAsync());
			DetachedCriteria dc3 = DetachedCriteria.For(typeof (Student), "st").CreateCriteria("Enrolments").CreateCriteria("Course").Add(Property.ForName("Description").Eq("Hibernate Training")).SetProjection(Property.ForName("st.Name"));
			await (session.CreateCriteria(typeof (Enrolment), "e").Add(Subqueries.Eq("Gavin King", dc3)).ListAsync());
			DetachedCriteria courseCriteria = DetachedCriteria.For(typeof (Course)).Add(Property.ForName("Description").Eq("Hibernate Training")).SetProjection(Projections.Property("CourseCode"));
			DetachedCriteria enrolmentCriteria = DetachedCriteria.For(typeof (Enrolment)).Add(Property.ForName("CourseCode").Eq(courseCriteria)).SetProjection(Projections.Property("CourseCode"));
			DetachedCriteria studentCriteria = DetachedCriteria.For(typeof (Student)).Add(Subqueries.Exists(enrolmentCriteria));
			object result = await (studentCriteria.GetExecutableCriteria(session).UniqueResultAsync());
			Assert.AreSame(gavin, result);
			await (session.DeleteAsync(enrolment2));
			await (session.DeleteAsync(gavin));
			await (session.DeleteAsync(course));
			await (t.CommitAsync());
			session.Close();
		}

		[Test]
		public async Task SubselectWithComponentAsync()
		{
			Course course = null;
			Student gavin = null;
			DetachedCriteria dc = null;
			CityState odessaWa = null;
			Enrolment enrolment2 = null;
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					course = new Course();
					course.CourseCode = "HIB";
					course.Description = "Hibernate Training";
					await (session.SaveAsync(course));
					odessaWa = new CityState("Odessa", "WA");
					gavin = new Student();
					gavin.Name = "Gavin King";
					gavin.StudentNumber = 232;
					gavin.CityState = odessaWa;
					await (session.SaveAsync(gavin));
					enrolment2 = new Enrolment();
					enrolment2.Course = course;
					enrolment2.CourseCode = course.CourseCode;
					enrolment2.Semester = 3;
					enrolment2.Year = 1998;
					enrolment2.Student = gavin;
					enrolment2.StudentNumber = gavin.StudentNumber;
					gavin.Enrolments.Add(enrolment2);
					await (session.PersistAsync(enrolment2));
					dc = DetachedCriteria.For<Student>().Add(Property.ForName("CityState").Eq(odessaWa)).SetProjection(Property.ForName("CityState"));
					await (session.CreateCriteria<Student>().Add(Subqueries.Exists(dc)).ListAsync());
					await (t.CommitAsync());
				}

			if (TestDialect.SupportsOperatorAll)
			{
				using (ISession session = OpenSession())
					using (ITransaction t = session.BeginTransaction())
					{
						try
						{
							await (session.CreateCriteria<Student>().Add(Subqueries.PropertyEqAll("CityState", dc)).ListAsync());
							Assert.Fail("should have failed because cannot compare subquery results with multiple columns");
						}
						catch (QueryException)
						{
						// expected
						}

						t.Rollback();
					}
			}

			if (TestDialect.SupportsOperatorAll)
			{
				using (ISession session = OpenSession())
					using (ITransaction t = session.BeginTransaction())
					{
						try
						{
							await (session.CreateCriteria<Student>().Add(Property.ForName("CityState").EqAll(dc)).ListAsync());
							Assert.Fail("should have failed because cannot compare subquery results with multiple columns");
						}
						catch (QueryException)
						{
						// expected
						}
						finally
						{
							t.Rollback();
						}
					}
			}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					try
					{
						await (session.CreateCriteria<Student>().Add(Subqueries.In(odessaWa, dc)).ListAsync());
						Assert.Fail("should have failed because cannot compare subquery results with multiple columns");
					}
					catch (NHibernate.Exceptions.GenericADOException)
					{
					// expected
					}
					finally
					{
						t.Rollback();
					}
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					DetachedCriteria dc2 = DetachedCriteria.For<Student>("st1").Add(Property.ForName("st1.CityState").EqProperty("st2.CityState")).SetProjection(Property.ForName("CityState"));
					try
					{
						await (session.CreateCriteria<Student>("st2").Add(Subqueries.Eq(odessaWa, dc2)).ListAsync());
						Assert.Fail("should have failed because cannot compare subquery results with multiple columns");
					}
					catch (NHibernate.Exceptions.GenericADOException)
					{
					// expected
					}
					finally
					{
						t.Rollback();
					}
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					DetachedCriteria dc3 = DetachedCriteria.For<Student>("st").CreateCriteria("Enrolments").CreateCriteria("Course").Add(Property.ForName("Description").Eq("Hibernate Training")).SetProjection(Property.ForName("st.CityState"));
					try
					{
						await (session.CreateCriteria<Enrolment>("e").Add(Subqueries.Eq(odessaWa, dc3)).ListAsync());
						Assert.Fail("should have failed because cannot compare subquery results with multiple columns");
					}
					catch (NHibernate.Exceptions.GenericADOException)
					{
					// expected
					}
					finally
					{
						t.Rollback();
					}
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.DeleteAsync(enrolment2));
					await (session.DeleteAsync(gavin));
					await (session.DeleteAsync(course));
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task CloningCriteriaAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			// HQL: from Animal a where a.mother.class = Reptile
			ICriteria c = s.CreateCriteria(typeof (Animal), "a").CreateAlias("mother", "m").Add(Property.ForName("m.class").Eq(typeof (Reptile)));
			ICriteria cloned = CriteriaTransformer.Clone(c);
			await (cloned.ListAsync());
			t.Rollback();
			s.Close();
		}

		[Test]
		public async Task CloningCriteria_AddCount_RemoveOrderingAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			// HQL: from Animal a where a.mother.class = Reptile
			ICriteria c = s.CreateCriteria(typeof (Animal), "a").CreateAlias("mother", "m").Add(Property.ForName("m.class").Eq(typeof (Reptile))).AddOrder(Order.Asc("a.bodyWeight"));
			ICriteria cloned = CriteriaTransformer.TransformToRowCount(c);
			await (cloned.ListAsync());
			t.Rollback();
			s.Close();
		}

		[Test]
		public async Task DetachedCriteriaTestAsync()
		{
			DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("Name").Eq("Gavin King")).AddOrder(Order.Asc("StudentNumber")).SetProjection(Property.ForName("StudentNumber"));
			byte[] bytes = SerializationHelper.Serialize(dc);
			dc = (DetachedCriteria)SerializationHelper.Deserialize(bytes);
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 232;
			Student bizarroGavin = new Student();
			bizarroGavin.Name = "Gavin King";
			bizarroGavin.StudentNumber = 666;
			await (session.SaveAsync(bizarroGavin));
			await (session.SaveAsync(gavin));
			IList result = await (dc.GetExecutableCriteria(session).SetMaxResults(3).ListAsync());
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(232L, result[0]);
			Assert.AreEqual(666L, result[1]);
			await (session.DeleteAsync(gavin));
			await (session.DeleteAsync(bizarroGavin));
			await (t.CommitAsync());
			session.Close();
		}

		[Test]
		public async Task SubqueryPaginationOnlyWithFirstAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.SaveAsync(new Student{Name = "Mengano", StudentNumber = 232}));
					await (session.SaveAsync(new Student{Name = "Ayende", StudentNumber = 999}));
					await (session.SaveAsync(new Student{Name = "Fabio", StudentNumber = 123}));
					await (session.SaveAsync(new Student{Name = "Merlo", StudentNumber = 456}));
					await (session.SaveAsync(new Student{Name = "Fulano", StudentNumber = 0}));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("StudentNumber").Gt(0L)).SetFirstResult(1).AddOrder(Order.Asc("StudentNumber")).SetProjection(Property.ForName("Name"));
					var result = await (session.CreateCriteria(typeof (Student)).Add(Subqueries.PropertyIn("Name", dc)).ListAsync<Student>());
					Assert.That(result.Count, Is.EqualTo(3));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.CreateQuery("delete from Student").ExecuteUpdateAsync());
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task SubqueryPaginationAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.SaveAsync(new Student{Name = "Mengano", StudentNumber = 232}));
					await (session.SaveAsync(new Student{Name = "Ayende", StudentNumber = 999}));
					await (session.SaveAsync(new Student{Name = "Fabio", StudentNumber = 123}));
					await (session.SaveAsync(new Student{Name = "Merlo", StudentNumber = 456}));
					await (session.SaveAsync(new Student{Name = "Fulano", StudentNumber = 0}));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("StudentNumber").Gt(200L)).SetMaxResults(2).SetFirstResult(1).AddOrder(Order.Asc("StudentNumber")).SetProjection(Property.ForName("Name"));
					var result = await (session.CreateCriteria(typeof (Student)).Add(Subqueries.PropertyIn("Name", dc)).AddOrder(Order.Asc("StudentNumber")).ListAsync<Student>());
					Assert.That(result.Count, Is.EqualTo(2));
					Assert.That(result[0].StudentNumber, Is.EqualTo(456));
					Assert.That(result[1].StudentNumber, Is.EqualTo(999));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.CreateQuery("delete from Student").ExecuteUpdateAsync());
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task SimplePaginationAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.SaveAsync(new Student{Name = "Mengano", StudentNumber = 232}));
					await (session.SaveAsync(new Student{Name = "Ayende", StudentNumber = 999}));
					await (session.SaveAsync(new Student{Name = "Fabio", StudentNumber = 123}));
					await (session.SaveAsync(new Student{Name = "Merlo", StudentNumber = 456}));
					await (session.SaveAsync(new Student{Name = "Fulano", StudentNumber = 0}));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					var result = await (session.CreateCriteria<Student>().Add(Restrictions.Gt("StudentNumber", 0L)).AddOrder(Order.Asc("StudentNumber")).SetFirstResult(1).SetMaxResults(2).ListAsync<Student>());
					Assert.That(result.Count, Is.EqualTo(2));
					Assert.That(result[0].StudentNumber, Is.EqualTo(232));
					Assert.That(result[1].StudentNumber, Is.EqualTo(456));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.CreateQuery("delete from Student").ExecuteUpdateAsync());
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task SimplePaginationOnlyWithFirstAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.SaveAsync(new Student{Name = "Mengano", StudentNumber = 232}));
					await (session.SaveAsync(new Student{Name = "Ayende", StudentNumber = 999}));
					await (session.SaveAsync(new Student{Name = "Fabio", StudentNumber = 123}));
					await (session.SaveAsync(new Student{Name = "Merlo", StudentNumber = 456}));
					await (session.SaveAsync(new Student{Name = "Fulano", StudentNumber = 0}));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					var result = await (session.CreateCriteria<Student>().Add(Restrictions.Gt("StudentNumber", 0L)).AddOrder(Order.Asc("StudentNumber")).SetFirstResult(1).ListAsync<Student>());
					Assert.That(result.Count, Is.EqualTo(3));
					Assert.That(result[0].StudentNumber, Is.EqualTo(232));
					Assert.That(result[1].StudentNumber, Is.EqualTo(456));
					Assert.That(result[2].StudentNumber, Is.EqualTo(999));
					await (t.CommitAsync());
				}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.CreateQuery("delete from Student").ExecuteUpdateAsync());
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task CloningDetachedCriteriaTestAsync()
		{
			DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("Name").Eq("Gavin King")).SetProjection(Property.ForName("StudentNumber"));
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 232;
			Student bizarroGavin = new Student();
			bizarroGavin.Name = "Gavin King";
			bizarroGavin.StudentNumber = 666;
			await (session.SaveAsync(bizarroGavin));
			await (session.SaveAsync(gavin));
			IList result = await (CriteriaTransformer.Clone(dc).AddOrder(Order.Asc("StudentNumber")).GetExecutableCriteria(session).SetMaxResults(3).ListAsync());
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(232L, result[0]);
			Assert.AreEqual(666L, result[1]);
			int count = (int)await (CriteriaTransformer.Clone(dc).SetProjection(Projections.RowCount()).GetExecutableCriteria(session).UniqueResultAsync());
			Assert.AreEqual(2, count);
			await (session.DeleteAsync(gavin));
			await (session.DeleteAsync(bizarroGavin));
			await (t.CommitAsync());
			session.Close();
		}

		[Test]
		public async Task ProjectionCacheAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			await (s.SaveAsync(course));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 666;
			await (s.SaveAsync(gavin));
			Student xam = new Student();
			xam.Name = "Max Rydahl Andersen";
			xam.StudentNumber = 101;
			await (s.SaveAsync(xam));
			Enrolment enrolment1 = new Enrolment();
			enrolment1.Course = course;
			enrolment1.CourseCode = course.CourseCode;
			enrolment1.Semester = 1;
			enrolment1.Year = 1999;
			enrolment1.Student = xam;
			enrolment1.StudentNumber = xam.StudentNumber;
			xam.Enrolments.Add(enrolment1);
			await (s.SaveAsync(enrolment1));
			Enrolment enrolment2 = new Enrolment();
			enrolment2.Course = course;
			enrolment2.CourseCode = course.CourseCode;
			enrolment2.Semester = 3;
			enrolment2.Year = 1998;
			enrolment2.Student = gavin;
			enrolment2.StudentNumber = gavin.StudentNumber;
			gavin.Enrolments.Add(enrolment2);
			await (s.SaveAsync(enrolment2));
			IList list = await (s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "s").CreateAlias("Course", "c").Add(Expression.IsNotEmpty("s.Enrolments")).SetProjection(Projections.ProjectionList().Add(Projections.Property("s.Name")).Add(Projections.Property("c.Description"))).SetCacheable(true).ListAsync());
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual(2, ((object[])list[0]).Length);
			Assert.AreEqual(2, ((object[])list[1]).Length);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "s").CreateAlias("Course", "c").Add(Expression.IsNotEmpty("s.Enrolments")).SetProjection(Projections.ProjectionList().Add(Projections.Property("s.Name")).Add(Projections.Property("c.Description"))).SetCacheable(true).ListAsync());
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual(2, ((object[])list[0]).Length);
			Assert.AreEqual(2, ((object[])list[1]).Length);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "s").CreateAlias("Course", "c").Add(Expression.IsNotEmpty("s.Enrolments")).SetProjection(Projections.ProjectionList().Add(Projections.Property("s.Name")).Add(Projections.Property("c.Description"))).SetCacheable(true).ListAsync());
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual(2, ((object[])list[0]).Length);
			Assert.AreEqual(2, ((object[])list[1]).Length);
			await (s.DeleteAsync(enrolment1));
			await (s.DeleteAsync(enrolment2));
			await (s.DeleteAsync(course));
			await (s.DeleteAsync(gavin));
			await (s.DeleteAsync(xam));
			await (t.CommitAsync());
			s.Close();
		}

		[Test, Ignore("Not supported.")]
		public async Task NH_1155_ShouldNotLoadAllChildrenInPagedSubSelectAsync()
		{
			if (this.Dialect.GetType().Equals((typeof (MsSql2000Dialect))))
				Assert.Ignore("This is not fixed for SQL 2000 Dialect");
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					Course course = new Course();
					course.CourseCode = "HIB";
					course.Description = "Hibernate Training";
					await (s.SaveAsync(course));
					Student gavin = new Student();
					gavin.Name = "Gavin King";
					gavin.StudentNumber = 667;
					await (s.SaveAsync(gavin));
					Student ayende = new Student();
					ayende.Name = "Ayende Rahien";
					ayende.StudentNumber = 1337;
					await (s.SaveAsync(ayende));
					Student xam = new Student();
					xam.Name = "Max Rydahl Andersen";
					xam.StudentNumber = 101;
					await (s.SaveAsync(xam));
					Enrolment enrolment = new Enrolment();
					enrolment.Course = course;
					enrolment.CourseCode = course.CourseCode;
					enrolment.Semester = 1;
					enrolment.Year = 1999;
					enrolment.Student = xam;
					enrolment.StudentNumber = xam.StudentNumber;
					xam.Enrolments.Add(enrolment);
					await (s.SaveAsync(enrolment));
					enrolment = new Enrolment();
					enrolment.Course = course;
					enrolment.CourseCode = course.CourseCode;
					enrolment.Semester = 3;
					enrolment.Year = 1998;
					enrolment.Student = ayende;
					enrolment.StudentNumber = ayende.StudentNumber;
					ayende.Enrolments.Add(enrolment);
					await (s.SaveAsync(enrolment));
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
			{
				IList<Student> list = await (s.CreateCriteria(typeof (Student)).SetFirstResult(1).SetMaxResults(10).AddOrder(Order.Asc("StudentNumber")).ListAsync<Student>());
				foreach (Student student in list)
				{
					foreach (Enrolment enrolment in student.Enrolments)
					{
						await (NHibernateUtil.InitializeAsync(enrolment));
					}
				}

				Enrolment key = new Enrolment();
				key.CourseCode = "HIB";
				key.StudentNumber = 101; // xam
				//since we didn't load xam's entrollments before (skipped by orderring)
				//it should not be already loaded
				Enrolment shouldNotBeLoaded = (Enrolment)await (s.LoadAsync(typeof (Enrolment), key));
				Assert.IsFalse(NHibernateUtil.IsInitialized(shouldNotBeLoaded));
			}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Enrolment"));
					await (s.DeleteAsync("from Student"));
					await (s.DeleteAsync("from Course"));
					await (tx.CommitAsync());
				}
		}

		[Test]
		public async Task ProjectionsTestAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			await (s.SaveAsync(course));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 667;
			await (s.SaveAsync(gavin));
			Student xam = new Student();
			xam.Name = "Max Rydahl Andersen";
			xam.StudentNumber = 101;
			await (s.SaveAsync(xam));
			Enrolment enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 1;
			enrolment.Year = 1999;
			enrolment.Student = xam;
			enrolment.StudentNumber = xam.StudentNumber;
			xam.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 3;
			enrolment.Year = 1998;
			enrolment.Student = gavin;
			enrolment.StudentNumber = gavin.StudentNumber;
			gavin.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			//s.flush();
			int count = (int)await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.Count("StudentNumber").SetDistinct()).UniqueResultAsync());
			Assert.AreEqual(2, count);
			object obj = await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(Projections.Count("StudentNumber")).Add(Projections.Max("StudentNumber")).Add(Projections.Min("StudentNumber")).Add(Projections.Avg("StudentNumber"))).UniqueResultAsync());
			object[] result = (object[])obj;
			Assert.AreEqual(2, result[0]);
			Assert.AreEqual(667L, result[1]);
			Assert.AreEqual(101L, result[2]);
			Assert.AreEqual(384.0D, (Double)result[3], 0.01D);
			IList resultWithMaps = await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.Distinct(Projections.ProjectionList().Add(Projections.Property("StudentNumber"), "stNumber").Add(Projections.Property("CourseCode"), "cCode"))).Add(Expression.Gt("StudentNumber", 665L)).Add(Expression.Lt("StudentNumber", 668L)).AddOrder(Order.Asc("stNumber")).SetResultTransformer(CriteriaSpecification.AliasToEntityMap).ListAsync());
			Assert.AreEqual(1, resultWithMaps.Count);
			IDictionary m1 = (IDictionary)resultWithMaps[0];
			Assert.AreEqual(667L, m1["stNumber"]);
			Assert.AreEqual(course.CourseCode, m1["cCode"]);
			resultWithMaps = await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.Property("StudentNumber").As("stNumber")).AddOrder(Order.Desc("stNumber")).SetResultTransformer(CriteriaSpecification.AliasToEntityMap).ListAsync());
			Assert.AreEqual(2, resultWithMaps.Count);
			IDictionary m0 = (IDictionary)resultWithMaps[0];
			m1 = (IDictionary)resultWithMaps[1];
			Assert.AreEqual(101L, m1["stNumber"]);
			Assert.AreEqual(667L, m0["stNumber"]);
			IList resultWithAliasedBean = await (s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Projections.Property("st.Name"), "studentName").Add(Projections.Property("co.Description"), "courseDescription")).AddOrder(Order.Desc("studentName")).SetResultTransformer(Transformers.AliasToBean(typeof (StudentDTO))).ListAsync());
			Assert.AreEqual(2, resultWithAliasedBean.Count);
			StudentDTO dto = (StudentDTO)resultWithAliasedBean[0];
			Assert.IsNotNull(dto.Description);
			Assert.IsNotNull(dto.Name);
			await (s.CreateCriteria(typeof (Student)).Add(Expression.Like("Name", "Gavin", MatchMode.Start)).AddOrder(Order.Asc("Name")).CreateCriteria("Enrolments", "e").AddOrder(Order.Desc("Year")).AddOrder(Order.Desc("Semester")).CreateCriteria("Course", "c").AddOrder(Order.Asc("Description")).SetProjection(Projections.ProjectionList().Add(Projections.Property("this.Name")).Add(Projections.Property("e.Year")).Add(Projections.Property("e.Semester")).Add(Projections.Property("c.CourseCode")).Add(Projections.Property("c.Description"))).UniqueResultAsync());
			ProjectionList p1 = Projections.ProjectionList().Add(Projections.Count("StudentNumber")).Add(Projections.Max("StudentNumber")).Add(Projections.RowCount());
			ProjectionList p2 = Projections.ProjectionList().Add(Projections.Min("StudentNumber")).Add(Projections.Avg("StudentNumber")).Add(Projections.SqlProjection("1 as constOne, count(*) as countStar", new String[]{"constOne", "countStar"}, new IType[]{NHibernateUtil.Int32, NHibernateUtil.Int32}));
			object[] array = (object[])await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(p1).Add(p2)).UniqueResultAsync());
			Assert.AreEqual(7, array.Length);
			ProjectionList pp1 = Projections.ProjectionList().Add(Projections.RowCountInt64());
			object r = await (s.CreateCriteria(typeof (Enrolment)).SetProjection(pp1).UniqueResultAsync());
			Assert.AreEqual(typeof (Int64), r.GetType());
			IList list = await (s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Projections.GroupProperty("co.CourseCode")).Add(Projections.Count("st.StudentNumber").SetDistinct()).Add(Projections.GroupProperty("Year"))).ListAsync());
			Assert.AreEqual(2, list.Count);
			object g = await (s.CreateCriteria(typeof (Student)).Add(Expression.IdEq(667L)).SetFetchMode("enrolments", FetchMode.Join)//.setFetchMode("enrolments.course", FetchMode.JOIN) //TODO: would love to make that work...
			.UniqueResultAsync());
			Assert.AreSame(gavin, g);
			await (s.DeleteAsync(gavin));
			await (s.DeleteAsync(xam));
			await (s.DeleteAsync(course));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CloningProjectionsTestAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			await (s.SaveAsync(course));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 667;
			await (s.SaveAsync(gavin));
			Student xam = new Student();
			xam.Name = "Max Rydahl Andersen";
			xam.StudentNumber = 101;
			await (s.SaveAsync(xam));
			Enrolment enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 1;
			enrolment.Year = 1999;
			enrolment.Student = xam;
			enrolment.StudentNumber = xam.StudentNumber;
			xam.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 3;
			enrolment.Year = 1998;
			enrolment.Student = gavin;
			enrolment.StudentNumber = gavin.StudentNumber;
			gavin.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			//s.flush();
			ICriteria criteriaToBeCloned = s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.Count("StudentNumber").SetDistinct());
			int count = (int)await (CriteriaTransformer.Clone(criteriaToBeCloned).UniqueResultAsync());
			Assert.AreEqual(2, count);
			ICriteria criteriaToClone = s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(Projections.Count("StudentNumber")).Add(Projections.Max("StudentNumber")).Add(Projections.Min("StudentNumber")).Add(Projections.Avg("StudentNumber")));
			object obj = await (CriteriaTransformer.Clone(criteriaToClone).UniqueResultAsync());
			object[] result = (object[])obj;
			Assert.AreEqual(2, result[0]);
			Assert.AreEqual(667L, result[1]);
			Assert.AreEqual(101L, result[2]);
			Assert.AreEqual(384.0D, (Double)result[3], 0.01D);
			ICriteria criteriaToClone2 = s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.Distinct(Projections.ProjectionList().Add(Projections.Property("StudentNumber"), "stNumber").Add(Projections.Property("CourseCode"), "cCode"))).Add(Expression.Gt("StudentNumber", 665L)).Add(Expression.Lt("StudentNumber", 668L)).AddOrder(Order.Asc("stNumber")).SetResultTransformer(CriteriaSpecification.AliasToEntityMap);
			IList resultWithMaps = await (CriteriaTransformer.Clone(criteriaToClone2).ListAsync());
			Assert.AreEqual(1, resultWithMaps.Count);
			IDictionary m1 = (IDictionary)resultWithMaps[0];
			Assert.AreEqual(667L, m1["stNumber"]);
			Assert.AreEqual(course.CourseCode, m1["cCode"]);
			ICriteria criteria = s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.Property("StudentNumber").As("stNumber")).AddOrder(Order.Desc("stNumber")).SetResultTransformer(CriteriaSpecification.AliasToEntityMap);
			resultWithMaps = await (CriteriaTransformer.Clone(criteria).ListAsync());
			Assert.AreEqual(2, resultWithMaps.Count);
			IDictionary m0 = (IDictionary)resultWithMaps[0];
			m1 = (IDictionary)resultWithMaps[1];
			Assert.AreEqual(101L, m1["stNumber"]);
			Assert.AreEqual(667L, m0["stNumber"]);
			ICriteria criteriaToClone3 = s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Projections.Property("st.Name"), "studentName").Add(Projections.Property("co.Description"), "courseDescription")).AddOrder(Order.Desc("studentName")).SetResultTransformer(Transformers.AliasToBean(typeof (StudentDTO)));
			IList resultWithAliasedBean = await (CriteriaTransformer.Clone(criteriaToClone3).ListAsync());
			Assert.AreEqual(2, resultWithAliasedBean.Count);
			StudentDTO dto = (StudentDTO)resultWithAliasedBean[0];
			Assert.IsNotNull(dto.Description);
			Assert.IsNotNull(dto.Name);
			ICriteria complexCriteriaToBeCloned = s.CreateCriteria(typeof (Student)).Add(Expression.Like("Name", "Gavin", MatchMode.Start)).AddOrder(Order.Asc("Name")).CreateCriteria("Enrolments", "e").AddOrder(Order.Desc("Year")).AddOrder(Order.Desc("Semester")).CreateCriteria("Course", "c").AddOrder(Order.Asc("Description")).SetProjection(Projections.ProjectionList().Add(Projections.Property("this.Name")).Add(Projections.Property("e.Year")).Add(Projections.Property("e.Semester")).Add(Projections.Property("c.CourseCode")).Add(Projections.Property("c.Description")));
			await (CriteriaTransformer.Clone(complexCriteriaToBeCloned).UniqueResultAsync());
			ProjectionList p1 = Projections.ProjectionList().Add(Projections.Count("StudentNumber")).Add(Projections.Max("StudentNumber")).Add(Projections.RowCount());
			ProjectionList p2 = Projections.ProjectionList().Add(Projections.Min("StudentNumber")).Add(Projections.Avg("StudentNumber")).Add(Projections.SqlProjection("1 as constOne, count(*) as countStar", new String[]{"constOne", "countStar"}, new IType[]{NHibernateUtil.Int32, NHibernateUtil.Int32}));
			object[] array = (object[])await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(p1).Add(p2))).UniqueResultAsync());
			Assert.AreEqual(7, array.Length);
			ICriteria criteriaToClone5 = s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Projections.GroupProperty("co.CourseCode")).Add(Projections.Count("st.StudentNumber").SetDistinct()).Add(Projections.GroupProperty("Year")));
			IList list = await (CriteriaTransformer.Clone(criteriaToClone5).ListAsync());
			Assert.AreEqual(2, list.Count);
			ICriteria criteriaToClone6 = s.CreateCriteria(typeof (Student)).Add(Expression.IdEq(667L)).SetFetchMode("enrolments", FetchMode.Join);
			object g = await (CriteriaTransformer.Clone(criteriaToClone6).UniqueResultAsync());
			Assert.AreSame(gavin, g);
			await (s.DeleteAsync(gavin));
			await (s.DeleteAsync(xam));
			await (s.DeleteAsync(course));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ProjectionsUsingPropertyAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			course.CourseMeetings.Add(new CourseMeeting(course, "Monday", 1, "1313 Mockingbird Lane"));
			await (s.SaveAsync(course));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 667;
			CityState odessaWa = new CityState("Odessa", "WA");
			gavin.CityState = odessaWa;
			gavin.PreferredCourse = course;
			await (s.SaveAsync(gavin));
			Student xam = new Student();
			xam.Name = "Max Rydahl Andersen";
			xam.StudentNumber = 101;
			await (s.SaveAsync(xam));
			Enrolment enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 1;
			enrolment.Year = 1999;
			enrolment.Student = xam;
			enrolment.StudentNumber = xam.StudentNumber;
			xam.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 3;
			enrolment.Year = 1998;
			enrolment.Student = gavin;
			enrolment.StudentNumber = gavin.StudentNumber;
			gavin.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			await (s.FlushAsync());
			// Subtest #1
			IList resultList = await (s.CreateCriteria<Enrolment>().SetProjection(Projections.ProjectionList().Add(Property.ForName("Student"), "student").Add(Property.ForName("Course"), "course").Add(Property.ForName("Semester"), "semester").Add(Property.ForName("Year"), "year")).ListAsync());
			Assert.That(resultList.Count, Is.EqualTo(2));
			foreach (object[] objects in resultList)
			{
				Assert.That(objects.Length, Is.EqualTo(4));
				Assert.That(objects[0], Is.InstanceOf<Student>());
				Assert.That(objects[1], Is.InstanceOf<Course>());
				Assert.That(objects[2], Is.InstanceOf<short>());
				Assert.That(objects[3], Is.InstanceOf<short>());
			}

			// Subtest #2
			resultList = await (s.CreateCriteria<Student>().SetProjection(Projections.ProjectionList().Add(Projections.Id().As("StudentNumber")).Add(Property.ForName("Name"), "name").Add(Property.ForName("CityState"), "cityState").Add(Property.ForName("PreferredCourse"), "preferredCourse")).ListAsync());
			Assert.That(resultList.Count, Is.EqualTo(2));
			foreach (object[] objects in resultList)
			{
				Assert.That(objects.Length, Is.EqualTo(4));
				Assert.That(objects[0], Is.InstanceOf<long>());
				Assert.That(objects[1], Is.InstanceOf<string>());
				if ("Gavin King".Equals(objects[1]))
				{
					Assert.That(objects[2], Is.InstanceOf<CityState>());
					Assert.That(objects[3], Is.InstanceOf<Course>());
				}
				else
				{
					Assert.That(objects[2], Is.Null);
					Assert.That(objects[3], Is.Null);
				}
			}

			// Subtest #3
			resultList = await (s.CreateCriteria<Student>().Add(Restrictions.Eq("Name", "Gavin King")).SetProjection(Projections.ProjectionList().Add(Projections.Id().As("StudentNumber")).Add(Property.ForName("Name"), "name").Add(Property.ForName("CityState"), "cityState").Add(Property.ForName("PreferredCourse"), "preferredCourse")).ListAsync());
			Assert.That(resultList.Count, Is.EqualTo(1));
			// Subtest #4
			object[] aResult = (object[])await (s.CreateCriteria<Student>().Add(Restrictions.IdEq(667L)).SetProjection(Projections.ProjectionList().Add(Projections.Id().As("StudentNumber")).Add(Property.ForName("Name"), "name").Add(Property.ForName("CityState"), "cityState").Add(Property.ForName("PreferredCourse"), "preferredCourse")).UniqueResultAsync());
			Assert.That(aResult, Is.Not.Null);
			Assert.That(aResult.Length, Is.EqualTo(4));
			Assert.That(aResult[0], Is.InstanceOf<long>());
			Assert.That(aResult[1], Is.InstanceOf<string>());
			Assert.That(aResult[2], Is.InstanceOf<CityState>());
			Assert.That(aResult[3], Is.InstanceOf<Course>());
			// Subtest #5
			int count = (int)await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Property.ForName("StudentNumber").Count().SetDistinct()).UniqueResultAsync());
			Assert.AreEqual(2, count);
			// Subtest #6
			object obj = await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(Property.ForName("StudentNumber").Count()).Add(Property.ForName("StudentNumber").Max()).Add(Property.ForName("StudentNumber").Min()).Add(Property.ForName("StudentNumber").Avg())).UniqueResultAsync());
			object[] result = (object[])obj;
			Assert.AreEqual(2, result[0]);
			Assert.AreEqual(667L, result[1]);
			Assert.AreEqual(101L, result[2]);
			Assert.AreEqual(384.0D, (double)result[3], 0.01D);
			// Subtest #7
			await (s.CreateCriteria(typeof (Enrolment)).Add(Property.ForName("StudentNumber").Gt(665L)).Add(Property.ForName("StudentNumber").Lt(668L)).Add(Property.ForName("CourseCode").Like("HIB", MatchMode.Start)).Add(Property.ForName("Year").Eq((short)1999)).AddOrder(Property.ForName("StudentNumber").Asc()).UniqueResultAsync());
			// Subtest #8
			IList resultWithMaps = await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(Property.ForName("StudentNumber").As("stNumber")).Add(Property.ForName("CourseCode").As("cCode"))).Add(Property.ForName("StudentNumber").Gt(665L)).Add(Property.ForName("StudentNumber").Lt(668L)).AddOrder(Property.ForName("StudentNumber").Asc()).SetResultTransformer(CriteriaSpecification.AliasToEntityMap).ListAsync());
			Assert.AreEqual(1, resultWithMaps.Count);
			IDictionary m1 = (IDictionary)resultWithMaps[0];
			Assert.AreEqual(667L, m1["stNumber"]);
			Assert.AreEqual(course.CourseCode, m1["cCode"]);
			// Subtest #9
			resultWithMaps = await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Property.ForName("StudentNumber").As("stNumber")).AddOrder(Order.Desc("stNumber")).SetResultTransformer(CriteriaSpecification.AliasToEntityMap).ListAsync());
			Assert.AreEqual(2, resultWithMaps.Count);
			IDictionary m0 = (IDictionary)resultWithMaps[0];
			m1 = (IDictionary)resultWithMaps[1];
			Assert.AreEqual(101L, m1["stNumber"]);
			Assert.AreEqual(667L, m0["stNumber"]);
			// Subtest #10
			IList resultWithAliasedBean = await (s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Property.ForName("st.Name").As("studentName")).Add(Property.ForName("co.Description").As("courseDescription"))).AddOrder(Order.Desc("studentName")).SetResultTransformer(Transformers.AliasToBean(typeof (StudentDTO))).ListAsync());
			Assert.AreEqual(2, resultWithAliasedBean.Count);
			// Subtest #11
			StudentDTO dto = (StudentDTO)resultWithAliasedBean[0];
			Assert.IsNotNull(dto.Description);
			Assert.IsNotNull(dto.Name);
			// Subtest #12
			CourseMeeting courseMeetingDto = await (s.CreateCriteria<CourseMeeting>().SetProjection(Projections.ProjectionList().Add(Property.ForName("Id").As("id")).Add(Property.ForName("Course").As("course"))).AddOrder(Order.Desc("id")).SetResultTransformer(Transformers.AliasToBean<CourseMeeting>()).UniqueResultAsync<CourseMeeting>());
			Assert.That(courseMeetingDto.Id, Is.Not.Null);
			Assert.That(courseMeetingDto.Id.CourseCode, Is.EqualTo(course.CourseCode));
			Assert.That(courseMeetingDto.Id.Day, Is.EqualTo("Monday"));
			Assert.That(courseMeetingDto.Id.Location, Is.EqualTo("1313 Mockingbird Lane"));
			Assert.That(courseMeetingDto.Id.Period, Is.EqualTo(1));
			Assert.That(courseMeetingDto.Course.Description, Is.EqualTo(course.Description));
			// Subtest #13
			await (s.CreateCriteria(typeof (Student)).Add(Expression.Like("Name", "Gavin", MatchMode.Start)).AddOrder(Order.Asc("Name")).CreateCriteria("Enrolments", "e").AddOrder(Order.Desc("Year")).AddOrder(Order.Desc("Semester")).CreateCriteria("Course", "c").AddOrder(Order.Asc("Description")).SetProjection(Projections.ProjectionList().Add(Property.ForName("this.Name")).Add(Property.ForName("e.Year")).Add(Property.ForName("e.Semester")).Add(Property.ForName("c.CourseCode")).Add(Property.ForName("c.Description"))).UniqueResultAsync());
			// Subtest #14
			ProjectionList p1 = Projections.ProjectionList().Add(Property.ForName("StudentNumber").Count()).Add(Property.ForName("StudentNumber").Max()).Add(Projections.RowCount());
			ProjectionList p2 = Projections.ProjectionList().Add(Property.ForName("StudentNumber").Min()).Add(Property.ForName("StudentNumber").Avg()).Add(Projections.SqlProjection("1 as constOne, count(*) as countStar", new String[]{"constOne", "countStar"}, new IType[]{NHibernateUtil.Int32, NHibernateUtil.Int32}));
			object[] array = (object[])await (s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(p1).Add(p2)).UniqueResultAsync());
			Assert.AreEqual(7, array.Length);
			// Subtest #15
			IList list = await (s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Property.ForName("co.CourseCode").Group()).Add(Property.ForName("st.StudentNumber").Count().SetDistinct()).Add(Property.ForName("Year").Group())).ListAsync());
			Assert.AreEqual(2, list.Count);
			// Subtest #16
			list = await (s.CreateCriteria<Enrolment>().CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Property.ForName("co.CourseCode").Group().As("courseCode")).Add(Property.ForName("st.StudentNumber").Count().SetDistinct().As("studentNumber")).Add(Property.ForName("Year").Group())).AddOrder(Order.Asc("courseCode")).AddOrder(Order.Asc("studentNumber")).ListAsync());
			Assert.That(list.Count, Is.EqualTo(2));
			// Subtest #17
			list = await (s.CreateCriteria<Enrolment>().CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Property.ForName("co.CourseCode").Group().As("cCode")).Add(Property.ForName("st.StudentNumber").Count().SetDistinct().As("stNumber")).Add(Property.ForName("Year").Group())).AddOrder(Order.Asc("cCode")).AddOrder(Order.Asc("stNumber")).ListAsync());
			Assert.That(list.Count, Is.EqualTo(2));
			await (s.DeleteAsync(gavin));
			await (s.DeleteAsync(xam));
			await (s.DeleteAsync(course));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task DistinctProjectionsOfComponentsAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			await (s.SaveAsync(course));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 667;
			gavin.CityState = new CityState("Odessa", "WA");
			;
			await (s.SaveAsync(gavin));
			Student xam = new Student();
			xam.Name = "Max Rydahl Andersen";
			xam.StudentNumber = 101;
			xam.PreferredCourse = course;
			xam.CityState = new CityState("Odessa", "WA");
			;
			await (s.SaveAsync(xam));
			Enrolment enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 1;
			enrolment.Year = 1999;
			enrolment.Student = xam;
			enrolment.StudentNumber = xam.StudentNumber;
			xam.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 3;
			enrolment.Year = 1998;
			enrolment.Student = gavin;
			enrolment.StudentNumber = gavin.StudentNumber;
			gavin.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			await (s.FlushAsync());
			object result = await (s.CreateCriteria<Student>().SetProjection(Projections.Distinct(Property.ForName("CityState"))).UniqueResultAsync());
			Assert.That(result, Is.InstanceOf<CityState>());
			Assert.That(((CityState)result).City, Is.EqualTo("Odessa"));
			Assert.That(((CityState)result).State, Is.EqualTo("WA"));
			result = await (s.CreateCriteria<Student>().SetProjection(Projections.Distinct(Property.ForName("CityState").As("cityState"))).AddOrder(Order.Asc("cityState")).UniqueResultAsync());
			Assert.That(result, Is.InstanceOf<CityState>());
			Assert.That(((CityState)result).City, Is.EqualTo("Odessa"));
			Assert.That(((CityState)result).State, Is.EqualTo("WA"));
			result = await (s.CreateCriteria<Student>().SetProjection(Projections.Count("CityState.City")).UniqueResultAsync());
			Assert.That(result, Is.EqualTo(2));
			result = await (s.CreateCriteria<Student>().SetProjection(Projections.CountDistinct("CityState.City")).UniqueResultAsync());
			Assert.That(result, Is.EqualTo(1));
			await (t.CommitAsync());
			s.Close();
			//			s = OpenSession();
			//			t = s.BeginTransaction();
			//			try
			//			{
			//				result = s.CreateCriteria<Student>()
			//					.SetProjection(Projections.Count("CityState"))
			//					.UniqueResult();
			//				
			//				if (!Dialect.SupportsTupleCounts)
			//				{
			//					fail( "expected SQLGrammarException" );
			//				}
			//				
			//				Assert.That((long)result, Is.EqualTo(1L));
			//			}
			//			catch (NHibernate.Exceptions.SQLGrammarException ex)
			//			{
			//				throw ex;
			//				if (!Dialect.SupportsTupleCounts)
			//				{
			//					// expected
			//				}
			//				else 
			//				{
			//					throw ex;
			//				}
			//			}
			//			finally
			//			{
			//				t.Rollback();
			//				s.Close();
			//			}
			//			s = OpenSession();
			//			t = s.BeginTransaction();
			//			try 
			//			{
			//				result = s.CreateCriteria<Student>()
			//					.SetProjection(Projections.CountDistinct("CityState"))
			//					.UniqueResult();
			//				
			//				if (!Dialect.SupportsTupleDistinctCounts)
			//				{
			//					fail("expected SQLGrammarException");
			//				}
			//				
			//				Assert.That((long)result, Is.EqualTo(1L));
			//			}
			//			catch (NHibernate.Exceptions.SQLGrammarException ex)
			//			{
			//				throw ex;
			//				if (Dialect.SupportsTupleDistinctCounts)
			//				{
			//					// expected
			//				}
			//				else 
			//				{
			//					throw ex;
			//				}
			//			}
			//			finally
			//			{
			//				t.Rollback();
			//				s.Close();
			//			}
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.DeleteAsync(gavin));
			await (s.DeleteAsync(xam));
			await (s.DeleteAsync(course));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task GroupByComponentAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			await (s.SaveAsync(course));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 667;
			gavin.CityState = new CityState("Odessa", "WA");
			;
			await (s.SaveAsync(gavin));
			Student xam = new Student();
			xam.Name = "Max Rydahl Andersen";
			xam.StudentNumber = 101;
			xam.PreferredCourse = course;
			xam.CityState = new CityState("Odessa", "WA");
			;
			await (s.SaveAsync(xam));
			Enrolment enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 1;
			enrolment.Year = 1999;
			enrolment.Student = xam;
			enrolment.StudentNumber = xam.StudentNumber;
			xam.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 3;
			enrolment.Year = 1998;
			enrolment.Student = gavin;
			enrolment.StudentNumber = gavin.StudentNumber;
			gavin.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			await (s.FlushAsync());
			object result = await (s.CreateCriteria<Student>().SetProjection(Projections.GroupProperty("CityState")).UniqueResultAsync());
			Assert.That(result, Is.InstanceOf<CityState>());
			Assert.That(((CityState)result).City, Is.EqualTo("Odessa"));
			Assert.That(((CityState)result).State, Is.EqualTo("WA"));
			result = await (s.CreateCriteria<Student>("st").SetProjection(Projections.GroupProperty("st.CityState")).UniqueResultAsync());
			Assert.That(result, Is.InstanceOf<CityState>());
			Assert.That(((CityState)result).City, Is.EqualTo("Odessa"));
			Assert.That(((CityState)result).State, Is.EqualTo("WA"));
			result = await (s.CreateCriteria<Student>("st").SetProjection(Projections.GroupProperty("st.CityState")).AddOrder(Order.Asc("CityState")).UniqueResultAsync());
			Assert.That(result, Is.InstanceOf<CityState>());
			Assert.That(((CityState)result).City, Is.EqualTo("Odessa"));
			Assert.That(((CityState)result).State, Is.EqualTo("WA"));
			result = await (s.CreateCriteria<Student>("st").SetProjection(Projections.GroupProperty("st.CityState").As("cityState")).AddOrder(Order.Asc("cityState")).UniqueResultAsync());
			Assert.That(result, Is.InstanceOf<CityState>());
			Assert.That(((CityState)result).City, Is.EqualTo("Odessa"));
			Assert.That(((CityState)result).State, Is.EqualTo("WA"));
			result = await (s.CreateCriteria<Student>("st").SetProjection(Projections.GroupProperty("st.CityState").As("cityState")).AddOrder(Order.Asc("cityState")).UniqueResultAsync());
			Assert.That(result, Is.InstanceOf<CityState>());
			Assert.That(((CityState)result).City, Is.EqualTo("Odessa"));
			Assert.That(((CityState)result).State, Is.EqualTo("WA"));
			result = await (s.CreateCriteria<Student>("st").SetProjection(Projections.GroupProperty("st.CityState").As("cityState")).Add(Restrictions.Eq("st.CityState", new CityState("Odessa", "WA"))).AddOrder(Order.Asc("cityState")).UniqueResultAsync());
			Assert.That(result, Is.InstanceOf<CityState>());
			Assert.That(((CityState)result).City, Is.EqualTo("Odessa"));
			Assert.That(((CityState)result).State, Is.EqualTo("WA"));
			IList list = await (s.CreateCriteria<Enrolment>().CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Property.ForName("co.CourseCode").Group()).Add(Property.ForName("st.CityState").Group()).Add(Property.ForName("Year").Group())).ListAsync());
			await (s.DeleteAsync(gavin));
			await (s.DeleteAsync(xam));
			await (s.DeleteAsync(course));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CloningProjectionsUsingPropertyAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Course course = new Course();
			course.CourseCode = "HIB";
			course.Description = "Hibernate Training";
			await (s.SaveAsync(course));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 667;
			await (s.SaveAsync(gavin));
			Student xam = new Student();
			xam.Name = "Max Rydahl Andersen";
			xam.StudentNumber = 101;
			await (s.SaveAsync(xam));
			Enrolment enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 1;
			enrolment.Year = 1999;
			enrolment.Student = xam;
			enrolment.StudentNumber = xam.StudentNumber;
			xam.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			enrolment = new Enrolment();
			enrolment.Course = course;
			enrolment.CourseCode = course.CourseCode;
			enrolment.Semester = 3;
			enrolment.Year = 1998;
			enrolment.Student = gavin;
			enrolment.StudentNumber = gavin.StudentNumber;
			gavin.Enrolments.Add(enrolment);
			await (s.SaveAsync(enrolment));
			await (s.FlushAsync());
			int count = (int)await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).SetProjection(Property.ForName("StudentNumber").Count().SetDistinct())).UniqueResultAsync());
			Assert.AreEqual(2, count);
			object obj = await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(Property.ForName("StudentNumber").Count()).Add(Property.ForName("StudentNumber").Max()).Add(Property.ForName("StudentNumber").Min()).Add(Property.ForName("StudentNumber").Avg()))).UniqueResultAsync());
			object[] result = (object[])obj;
			Assert.AreEqual(2, result[0]);
			Assert.AreEqual(667L, result[1]);
			Assert.AreEqual(101L, result[2]);
			Assert.AreEqual(384.0D, (double)result[3], 0.01D);
			await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).Add(Property.ForName("StudentNumber").Gt(665L)).Add(Property.ForName("StudentNumber").Lt(668L)).Add(Property.ForName("CourseCode").Like("HIB", MatchMode.Start)).Add(Property.ForName("Year").Eq((short)1999)).AddOrder(Property.ForName("StudentNumber").Asc())).UniqueResultAsync());
			ICriteria clonedCriteriaProjection = CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(Property.ForName("StudentNumber").As("stNumber")).Add(Property.ForName("CourseCode").As("cCode"))).Add(Property.ForName("StudentNumber").Gt(665L)).Add(Property.ForName("StudentNumber").Lt(668L)).AddOrder(Property.ForName("StudentNumber").Asc()).SetResultTransformer(CriteriaSpecification.AliasToEntityMap));
			IList resultWithMaps = await (clonedCriteriaProjection.ListAsync());
			Assert.AreEqual(1, resultWithMaps.Count);
			IDictionary m1 = (IDictionary)resultWithMaps[0];
			Assert.AreEqual(667L, m1["stNumber"]);
			Assert.AreEqual(course.CourseCode, m1["cCode"]);
			resultWithMaps = await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).SetProjection(Property.ForName("StudentNumber").As("stNumber")).AddOrder(Order.Desc("stNumber")).SetResultTransformer(CriteriaSpecification.AliasToEntityMap)).ListAsync());
			Assert.AreEqual(2, resultWithMaps.Count);
			IDictionary m0 = (IDictionary)resultWithMaps[0];
			m1 = (IDictionary)resultWithMaps[1];
			Assert.AreEqual(101L, m1["stNumber"]);
			Assert.AreEqual(667L, m0["stNumber"]);
			IList resultWithAliasedBean = await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Property.ForName("st.Name").As("studentName")).Add(Property.ForName("co.Description").As("courseDescription"))).AddOrder(Order.Desc("studentName")).SetResultTransformer(Transformers.AliasToBean(typeof (StudentDTO)))).ListAsync());
			Assert.AreEqual(2, resultWithAliasedBean.Count);
			StudentDTO dto = (StudentDTO)resultWithAliasedBean[0];
			Assert.IsNotNull(dto.Description);
			Assert.IsNotNull(dto.Name);
			ICriteria complexCriteriaWithProjections = s.CreateCriteria(typeof (Student)).Add(Expression.Like("Name", "Gavin", MatchMode.Start)).AddOrder(Order.Asc("Name")).CreateCriteria("Enrolments", "e").AddOrder(Order.Desc("Year")).AddOrder(Order.Desc("Semester")).CreateCriteria("Course", "c").AddOrder(Order.Asc("Description")).SetProjection(Projections.ProjectionList().Add(Property.ForName("this.Name")).Add(Property.ForName("e.Year")).Add(Property.ForName("e.Semester")).Add(Property.ForName("c.CourseCode")).Add(Property.ForName("c.Description")));
			await (CriteriaTransformer.Clone(complexCriteriaWithProjections).UniqueResultAsync());
			ProjectionList p1 = Projections.ProjectionList().Add(Property.ForName("StudentNumber").Count()).Add(Property.ForName("StudentNumber").Max()).Add(Projections.RowCount());
			ProjectionList p2 = Projections.ProjectionList().Add(Property.ForName("StudentNumber").Min()).Add(Property.ForName("StudentNumber").Avg()).Add(Projections.SqlProjection("1 as constOne, count(*) as countStar", new String[]{"constOne", "countStar"}, new IType[]{NHibernateUtil.Int32, NHibernateUtil.Int32}));
			object[] array = (object[])await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).SetProjection(Projections.ProjectionList().Add(p1).Add(p2))).UniqueResultAsync());
			Assert.AreEqual(7, array.Length);
			IList list = await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Enrolment)).CreateAlias("Student", "st").CreateAlias("Course", "co").SetProjection(Projections.ProjectionList().Add(Property.ForName("co.CourseCode").Group()).Add(Property.ForName("st.StudentNumber").Count().SetDistinct()).Add(Property.ForName("Year").Group()))).ListAsync());
			Assert.AreEqual(2, list.Count);
			await (s.DeleteAsync(gavin));
			await (s.DeleteAsync(xam));
			await (s.DeleteAsync(course));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task RestrictionOnSubclassCollectionAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.CreateCriteria(typeof (Reptile)).Add(Expression.IsEmpty("offspring")).ListAsync());
			await (s.CreateCriteria(typeof (Reptile)).Add(Expression.IsNotEmpty("offspring")).ListAsync());
			t.Rollback();
			s.Close();
		}

		[Test]
		public async Task ClassPropertyAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			// HQL: from Animal a where a.mother.class = Reptile
			ICriteria c = s.CreateCriteria<Animal>("a").CreateAlias("mother", "m").Add(Property.ForName("m.class").Eq(typeof (Reptile)));
			await (c.ListAsync());
			t.Rollback();
			s.Close();
		}

		[Test]
		public async Task ProjectedIdAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.CreateCriteria(typeof (Course)).SetProjection(Projections.Property("CourseCode")).ListAsync());
			await (s.CreateCriteria(typeof (Course)).SetProjection(Projections.Id()).ListAsync());
			t.Rollback();
			s.Close();
		}

		[Test]
		public async Task ProjectedEmbeddedCompositeIdAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Course course = new Course();
					course.CourseCode = "HIB";
					course.Description = "Hibernate Training";
					await (s.SaveAsync(course));
					Student gavin = new Student();
					gavin.Name = "Gavin King";
					gavin.StudentNumber = 667;
					await (s.SaveAsync(gavin));
					Student xam = new Student();
					xam.Name = "Max Rydahl Andersen";
					xam.StudentNumber = 101;
					await (s.SaveAsync(xam));
					Enrolment enrolment = new Enrolment();
					enrolment.Course = course;
					enrolment.CourseCode = course.CourseCode;
					enrolment.Semester = 1;
					enrolment.Year = 1999;
					enrolment.Student = xam;
					enrolment.StudentNumber = xam.StudentNumber;
					gavin.Enrolments.Add(enrolment);
					await (s.SaveAsync(enrolment));
					enrolment = new Enrolment();
					enrolment.Course = course;
					enrolment.CourseCode = course.CourseCode;
					enrolment.Semester = 3;
					enrolment.Year = 1998;
					enrolment.Student = gavin;
					enrolment.StudentNumber = gavin.StudentNumber;
					gavin.Enrolments.Add(enrolment);
					await (s.SaveAsync(enrolment));
					await (s.FlushAsync());
					IList enrolments = (IList)await (s.CreateCriteria<Enrolment>().SetProjection(Projections.Id()).ListAsync());
					t.Rollback();
				}
		}

		[Test]
		public async Task ProjectedListIncludesEmbeddedCompositeIdAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Course course = new Course();
					course.CourseCode = "HIB";
					course.Description = "Hibernate Training";
					await (s.SaveAsync(course));
					Student gavin = new Student();
					gavin.Name = "Gavin King";
					gavin.StudentNumber = 667;
					await (s.SaveAsync(gavin));
					Student xam = new Student();
					xam.Name = "Max Rydahl Andersen";
					xam.StudentNumber = 101;
					await (s.SaveAsync(xam));
					Enrolment enrolment = new Enrolment();
					enrolment.Course = course;
					enrolment.CourseCode = course.CourseCode;
					enrolment.Semester = 1;
					enrolment.Year = 1999;
					enrolment.Student = xam;
					enrolment.StudentNumber = xam.StudentNumber;
					gavin.Enrolments.Add(enrolment);
					await (s.SaveAsync(enrolment));
					enrolment = new Enrolment();
					enrolment.Course = course;
					enrolment.CourseCode = course.CourseCode;
					enrolment.Semester = 3;
					enrolment.Year = 1998;
					enrolment.Student = gavin;
					enrolment.StudentNumber = gavin.StudentNumber;
					gavin.Enrolments.Add(enrolment);
					await (s.SaveAsync(enrolment));
					await (s.FlushAsync());
					IList data = (IList)await (s.CreateCriteria<Enrolment>().SetProjection(Projections.ProjectionList().Add(Projections.Property("Semester")).Add(Projections.Property("Year")).Add(Projections.Id())).ListAsync());
					t.Rollback();
				}
		}

		[Test]
		public async Task ProjectedCompositeIdAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Course course = new Course();
					course.CourseCode = "HIB";
					course.Description = "Hibernate Training";
					course.CourseMeetings.Add(new CourseMeeting(course, "Monday", 1, "1313 Mockingbird Lane"));
					await (s.SaveAsync(course));
					await (s.FlushAsync());
					IList data = (IList)await (s.CreateCriteria<CourseMeeting>().SetProjection(Projections.Id()).ListAsync());
					t.Rollback();
				}
		}

		[Test]
		public async Task ProjectedCompositeIdWithAliasAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Course course = new Course();
					course.CourseCode = "HIB";
					course.Description = "Hibernate Training";
					course.CourseMeetings.Add(new CourseMeeting(course, "Monday", 1, "1313 Mockingbird Lane"));
					await (s.SaveAsync(course));
					await (s.FlushAsync());
					IList data = (IList)await (s.CreateCriteria<CourseMeeting>().SetProjection(Projections.Id().As("id")).ListAsync());
					t.Rollback();
				}
		}

		[Test]
		public async Task ProjectedComponentAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Student gaith = new Student();
					gaith.Name = "Gaith Bell";
					gaith.StudentNumber = 123;
					gaith.CityState = new CityState("Chicago", "Illinois");
					await (s.SaveAsync(gaith));
					await (s.FlushAsync());
					IList cityStates = (IList)await (s.CreateCriteria<Student>().SetProjection(Projections.Property("CityState")).ListAsync());
					t.Rollback();
				}
		}

		[Test]
		public async Task ProjectedListIncludesComponentAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Student gaith = new Student();
					gaith.Name = "Gaith Bell";
					gaith.StudentNumber = 123;
					gaith.CityState = new CityState("Chicago", "Illinois");
					await (s.SaveAsync(gaith));
					await (s.FlushAsync());
					IList data = (IList)await (s.CreateCriteria<Student>().SetProjection(Projections.ProjectionList().Add(Projections.Property("CityState")).Add(Projections.Property("Name"))).ListAsync());
					t.Rollback();
				}
		}

		[Test]
		public async Task CloningProjectedIdAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.CreateCriteria<Course>().SetProjection(Projections.Property("CourseCode")).ListAsync());
			await (CriteriaTransformer.Clone(s.CreateCriteria(typeof (Course)).SetProjection(Projections.Id())).ListAsync());
			t.Rollback();
			s.Close();
		}

		[Test]
		public async Task CloningSubcriteriaJoinTypesAsync()
		{
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Course courseA = new Course();
			courseA.CourseCode = "HIB-A";
			courseA.Description = "Hibernate Training A";
			await (session.SaveAsync(courseA));
			Course courseB = new Course();
			courseB.CourseCode = "HIB-B";
			courseB.Description = "Hibernate Training B";
			await (session.SaveAsync(courseB));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 232;
			gavin.PreferredCourse = courseA;
			await (session.SaveAsync(gavin));
			Student leonardo = new Student();
			leonardo.Name = "Leonardo Quijano";
			leonardo.StudentNumber = 233;
			leonardo.PreferredCourse = courseB;
			await (session.SaveAsync(leonardo));
			Student johnDoe = new Student();
			johnDoe.Name = "John Doe";
			johnDoe.StudentNumber = 235;
			johnDoe.PreferredCourse = null;
			await (session.SaveAsync(johnDoe));
			ICriteria criteria = session.CreateCriteria(typeof (Student)).SetProjection(Property.ForName("PreferredCourse.CourseCode")).CreateCriteria("PreferredCourse", JoinType.LeftOuterJoin).AddOrder(Order.Asc("CourseCode"));
			IList result = await (CriteriaTransformer.Clone(criteria).ListAsync());
			Assert.AreEqual(3, result.Count);
			t.Rollback();
			session.Dispose();
		}

		[Test]
		public async Task SubcriteriaJoinTypesAsync()
		{
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Course courseA = new Course();
			courseA.CourseCode = "HIB-A";
			courseA.Description = "Hibernate Training A";
			await (session.SaveAsync(courseA));
			Course courseB = new Course();
			courseB.CourseCode = "HIB-B";
			courseB.Description = "Hibernate Training B";
			await (session.SaveAsync(courseB));
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 232;
			gavin.PreferredCourse = courseA;
			await (session.SaveAsync(gavin));
			Student leonardo = new Student();
			leonardo.Name = "Leonardo Quijano";
			leonardo.StudentNumber = 233;
			leonardo.PreferredCourse = courseB;
			await (session.SaveAsync(leonardo));
			Student johnDoe = new Student();
			johnDoe.Name = "John Doe";
			johnDoe.StudentNumber = 235;
			johnDoe.PreferredCourse = null;
			await (session.SaveAsync(johnDoe));
			IList result = await (session.CreateCriteria(typeof (Student)).SetProjection(Property.ForName("PreferredCourse.CourseCode")).CreateCriteria("PreferredCourse", JoinType.LeftOuterJoin).AddOrder(Order.Asc("CourseCode")).ListAsync());
			Assert.AreEqual(3, result.Count);
			// can't be sure of NULL comparison ordering aside from they should
			// either come first or last
			if (result[0] == null)
			{
				Assert.AreEqual("HIB-A", result[1]);
				Assert.AreEqual("HIB-B", result[2]);
			}
			else
			{
				Assert.IsNull(result[2]);
				Assert.AreEqual("HIB-A", result[0]);
				Assert.AreEqual("HIB-B", result[1]);
			}

			result = await (session.CreateCriteria(typeof (Student)).SetFetchMode("PreferredCourse", FetchMode.Join).CreateCriteria("PreferredCourse", JoinType.LeftOuterJoin).AddOrder(Order.Asc("CourseCode")).ListAsync());
			Assert.AreEqual(3, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNotNull(result[1]);
			Assert.IsNotNull(result[2]);
			result = await (session.CreateCriteria(typeof (Student)).SetFetchMode("PreferredCourse", FetchMode.Join).CreateAlias("PreferredCourse", "pc", JoinType.LeftOuterJoin).AddOrder(Order.Asc("pc.CourseCode")).ListAsync());
			Assert.AreEqual(3, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNotNull(result[1]);
			Assert.IsNotNull(result[2]);
			await (session.DeleteAsync(gavin));
			await (session.DeleteAsync(leonardo));
			await (session.DeleteAsync(johnDoe));
			await (session.DeleteAsync(courseA));
			await (session.DeleteAsync(courseB));
			await (t.CommitAsync());
			session.Close();
		}

		[Test]
		public async Task TypeMismatchAsync()
		{
			using (ISession session = OpenSession())
			{
				Assert.ThrowsAsync<QueryException>(async () => await (session.CreateCriteria(typeof (Enrolment)).Add(Expression.Eq("Student", 10)) // Type mismatch!
				.ListAsync()));
			}
		}

		[Test]
		public async Task PropertySubClassDiscriminatorAsync()
		{
			using (ISession s = OpenSession())
			{
				MaterialUnitable bo1 = new MaterialUnitable();
				bo1.Description = "Seal";
				MaterialUnitable bo2 = new MaterialUnitable();
				bo2.Description = "Meter";
				MaterialUnitable dv = new DeviceDef();
				dv.Description = "Printer";
				await (s.SaveAsync(bo1));
				await (s.SaveAsync(bo2));
				await (s.SaveAsync(dv));
				await (s.FlushAsync());
				MaterialUnit mu = new MaterialUnit(bo1, "S1");
				await (s.SaveAsync(mu));
				mu = new MaterialUnit(bo1, "S2");
				await (s.SaveAsync(mu));
				mu = new MaterialUnit(bo2, "M1");
				await (s.SaveAsync(mu));
				mu = new MaterialUnit(dv, "D1");
				await (s.SaveAsync(mu));
				await (s.FlushAsync());
			}

			using (ISession session = OpenSession())
			{
				IList l = await (session.CreateCriteria(typeof (MaterialUnit), "mu").CreateAlias("mu.Material", "ma").Add(Property.ForName("ma.class").Eq(typeof (MaterialUnitable))).ListAsync());
				Assert.AreEqual(3, l.Count);
			}

			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync("from MaterialUnit"));
				await (s.DeleteAsync("from MaterialResource"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public void CriteriaInspection()
		{
			using (ISession session = OpenSession())
			{
				ICriteria criteria = session.CreateCriteria(typeof (MaterialUnit), "mu").CreateAlias("mu.Material", "ma");
				Assert.IsNotNull(criteria.GetCriteriaByAlias("ma"));
				Assert.AreEqual("ma", criteria.GetCriteriaByPath("mu.Material").Alias);
				Assert.AreEqual(criteria, criteria.GetCriteriaByAlias("mu"));
				Assert.AreEqual(criteria.CreateCriteria("fooBar"), criteria.GetCriteriaByPath("fooBar"));
			}
		}

		[Test]
		public void DetachedCriteriaInspection()
		{
			DetachedCriteria criteria = DetachedCriteria.For(typeof (Student)).CreateAlias("mu.Material", "ma");
			Assert.IsNotNull(criteria.GetCriteriaByAlias("ma"));
			Assert.AreEqual("ma", criteria.GetCriteriaByPath("mu.Material").Alias);
			Assert.IsNull(criteria.GetCriteriaByPath("fooBar"));
			Assert.IsNull(criteria.GetCriteriaByAlias("fooBar"));
		}

		[Test]
		public async Task SameColumnAndAliasNamesAsync()
		{
			DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("Name").Eq("Gavin King")).AddOrder(Order.Asc("StudentNumber")).SetProjection(Projections.ProjectionList().Add(Projections.Property("StudentNumber"), "StudentNumber").Add(Projections.Property("Name"), "Name"));
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 232;
			Student bizarroGavin = new Student();
			bizarroGavin.Name = "Gavin King";
			bizarroGavin.StudentNumber = 666;
			await (session.SaveAsync(bizarroGavin));
			await (session.SaveAsync(gavin));
			IList result = await (dc.GetExecutableCriteria(session).SetMaxResults(3).ListAsync());
			Assert.AreEqual(2, result.Count);
			await (session.DeleteAsync(gavin));
			await (session.DeleteAsync(bizarroGavin));
			await (t.CommitAsync());
			session.Close();
		}

		[Test]
		public async Task SameColumnAndAliasNamesResultTransformerAsync()
		{
			DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).SetProjection(Projections.ProjectionList().Add(Projections.Property("StudentNumber"), "StudentNumber").Add(Projections.Property("Name"), "Name")).SetResultTransformer(new AliasToBeanResultTransformer(typeof (Student))).Add(Property.ForName("Name").Eq("Gavin King")).AddOrder(Order.Asc("StudentNumber"));
			ISession session = OpenSession();
			ITransaction t = session.BeginTransaction();
			Student gavin = new Student();
			gavin.Name = "Gavin King";
			gavin.StudentNumber = 232;
			Student bizarroGavin = new Student();
			bizarroGavin.Name = "Gavin King";
			bizarroGavin.StudentNumber = 666;
			await (session.SaveAsync(bizarroGavin));
			await (session.SaveAsync(gavin));
			IList result = await (dc.GetExecutableCriteria(session).SetMaxResults(3).ListAsync());
			Assert.That(result.Count, Is.EqualTo(2));
			Assert.That(result[0], Is.InstanceOf(typeof (Student)));
			Assert.That(result[1], Is.InstanceOf(typeof (Student)));
			await (session.DeleteAsync(gavin));
			await (session.DeleteAsync(bizarroGavin));
			await (t.CommitAsync());
			session.Close();
		}

		[Test]
		public async Task CacheDetachedCriteriaAsync()
		{
			using (ISession session = OpenSession())
			{
				bool current = sessions.Statistics.IsStatisticsEnabled;
				sessions.Statistics.IsStatisticsEnabled = true;
				sessions.Statistics.Clear();
				DetachedCriteria dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("Name").Eq("Gavin King")).SetProjection(Property.ForName("StudentNumber")).SetCacheable(true);
				Assert.That(sessions.Statistics.QueryCacheMissCount, Is.EqualTo(0));
				Assert.That(sessions.Statistics.QueryCacheHitCount, Is.EqualTo(0));
				await (dc.GetExecutableCriteria(session).ListAsync());
				Assert.That(sessions.Statistics.QueryCacheMissCount, Is.EqualTo(1));
				dc = DetachedCriteria.For(typeof (Student)).Add(Property.ForName("Name").Eq("Gavin King")).SetProjection(Property.ForName("StudentNumber")).SetCacheable(true);
				await (dc.GetExecutableCriteria(session).ListAsync());
				Assert.That(sessions.Statistics.QueryCacheMissCount, Is.EqualTo(1));
				Assert.That(sessions.Statistics.QueryCacheHitCount, Is.EqualTo(1));
				sessions.Statistics.IsStatisticsEnabled = false;
			}
		}

		[Test]
		public async Task PropertyWithFormulaAndPagingTestAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			ICriteria crit = s.CreateCriteria(typeof (Animal)).SetFirstResult(0).SetMaxResults(1).AddOrder(new Order("bodyWeight", true));
			await (crit.ListAsync<Animal>());
			t.Rollback();
			s.Close();
		}

		[Test]
		public async Task SqlExpressionWithParametersAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					ICriteria c = session.CreateCriteria(typeof (Student));
					c.Add(Expression.Eq("StudentNumber", (long)232));
					c.Add(Expression.Sql("2 = ?", 1, NHibernateUtil.Int32));
					Student gavin = new Student();
					gavin.Name = "Gavin King";
					gavin.StudentNumber = 232;
					await (session.SaveAsync(gavin));
					IList result = await (c.ListAsync());
					Assert.AreEqual(0, result.Count);
					await (session.DeleteAsync(gavin));
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task ParametersInCountExpressionAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					ICriteria criteria = session.CreateCriteria(typeof (Student), "c");
					DetachedCriteria subselect = DetachedCriteria.For(typeof (Enrolment));
					subselect.Add(Expression.Eq("Year", (short)2008));
					subselect.SetProjection(Projections.Distinct(Projections.Property("StudentNumber")));
					criteria.Add(Subqueries.PropertyNotIn("StudentNumber", subselect));
					ICriteria rowCount = CriteriaTransformer.TransformToRowCount(criteria);
					// IMPORTANT: The problem is executing BOTH queries at the same time... not just one
					await (criteria.ListAsync());
					await (rowCount.ListAsync());
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task TransformToRowCountTestAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					ICriteria crit = s.CreateCriteria(typeof (Student));
					ICriteria subCriterium = crit.CreateCriteria("PreferredCourse");
					subCriterium.Add(Property.ForName("CourseCode").Eq("PREFFERED_CODE"));
					ICriteria countCriteria = CriteriaTransformer.TransformToRowCount(crit);
					await (countCriteria.ListAsync());
					t.Rollback();
				}
		}

		[Test]
		public async Task OrderProjectionTestAsync()
		{
			using (ISession session = this.OpenSession())
			{
				ICriteria criteria = session.CreateCriteria(typeof (Student), "c");
				criteria.AddOrder(Order.Asc(Projections.Conditional(Restrictions.Eq("StudentNumber", (long)1), Projections.Constant(0), Projections.Constant(1))));
				await (criteria.ListAsync());
			}
		}

		[Test]
		public async Task OrderProjectionAliasedTestAsync()
		{
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					Course courseA = new Course();
					courseA.CourseCode = "HIB-A";
					courseA.Description = "Hibernate Training A";
					await (session.SaveAsync(courseA));
					Student gavin = new Student();
					gavin.Name = "Gavin King";
					gavin.StudentNumber = 232;
					gavin.PreferredCourse = courseA;
					await (session.SaveAsync(gavin));
					Student leonardo = new Student();
					leonardo.Name = "Leonardo Quijano";
					leonardo.StudentNumber = 233;
					leonardo.PreferredCourse = courseA;
					await (session.SaveAsync(leonardo));
					Student johnDoe = new Student();
					johnDoe.Name = "John Doe";
					johnDoe.StudentNumber = 235;
					johnDoe.PreferredCourse = null;
					await (session.SaveAsync(johnDoe));
					IProjection conditional = Projections.Conditional(Restrictions.Eq("Name", "Gavin King"), Projections.Constant("Name"), Projections.Constant("AnotherName"));
					ICriteria criteria = session.CreateCriteria(typeof (Student));
					criteria.SetMaxResults(1);
					criteria.SetFirstResult(1);
					IList result = await (criteria.SetProjection(Projections.Alias(conditional, "CheckName")).AddOrder(Order.Asc("CheckName")).ListAsync());
					await (session.DeleteAsync(gavin));
					await (session.DeleteAsync(leonardo));
					await (session.DeleteAsync(johnDoe));
					await (session.DeleteAsync(courseA));
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task LikeProjectionTestAsync()
		{
			Student john = new Student{Name = "John"};
			using (ISession session = this.OpenSession())
			{
				await (session.SaveAsync(john));
				await (session.FlushAsync());
			}

			using (ISession session = this.OpenSession())
			{
				ICriteria criteria = session.CreateCriteria(typeof (Student), "c");
				criteria.Add(new LikeExpression(Projections.Property("Name"), "John", MatchMode.Anywhere));
				Assert.AreEqual(1, (await (criteria.ListAsync())).Count);
			}

			using (ISession session = this.OpenSession())
			{
				ICriteria criteria = session.CreateCriteria(typeof (Student), "c");
				criteria.Add(new LikeExpression("Name", "John"));
				Assert.AreEqual(1, (await (criteria.ListAsync())).Count);
			}

			using (ISession session = this.OpenSession())
			{
				await (session.DeleteAsync(john));
				await (session.FlushAsync());
			}
		}

		[Test]
		public async Task LikeProjectionUsingRestrictionsTestAsync()
		{
			using (ISession session = this.OpenSession())
			{
				ICriteria criteria = session.CreateCriteria(typeof (Student), "c");
				criteria.Add(Restrictions.Like(Projections.Constant("Name"), "John", MatchMode.Anywhere));
				await (criteria.ListAsync());
			}
		}

		[Test]
		public async Task InsensitiveLikeProjectionUsingRestrictionsTestAsync()
		{
			using (ISession session = this.OpenSession())
			{
				ICriteria criteria = session.CreateCriteria(typeof (Student), "c");
				criteria.Add(Restrictions.InsensitiveLike(Projections.Constant("Name"), "John", MatchMode.Anywhere));
				await (criteria.ListAsync());
			}
		}

		[Test]
		public async Task AliasJoinCriterionAsync()
		{
			using (ISession session = this.OpenSession())
			{
				using (ITransaction t = session.BeginTransaction())
				{
					Course courseA = new Course();
					courseA.CourseCode = "HIB-A";
					courseA.Description = "Hibernate Training A";
					await (session.PersistAsync(courseA));
					Course courseB = new Course();
					courseB.CourseCode = "HIB-B";
					courseB.Description = "Hibernate Training B";
					await (session.PersistAsync(courseB));
					Student gavin = new Student();
					gavin.Name = "Gavin King";
					gavin.StudentNumber = 232;
					gavin.PreferredCourse = courseA;
					await (session.PersistAsync(gavin));
					Student leonardo = new Student();
					leonardo.Name = "Leonardo Quijano";
					leonardo.StudentNumber = 233;
					leonardo.PreferredCourse = courseB;
					await (session.PersistAsync(leonardo));
					Student johnDoe = new Student();
					johnDoe.Name = "John Doe";
					johnDoe.StudentNumber = 235;
					johnDoe.PreferredCourse = null;
					await (session.PersistAsync(johnDoe));
					// test == on one value exists
					IList<string> result = await (session.CreateCriteria<Student>().CreateAlias("PreferredCourse", "pc", JoinType.LeftOuterJoin, Restrictions.Eq("pc.CourseCode", "HIB-A")).SetProjection(Property.ForName("pc.CourseCode")).AddOrder(Order.Asc("pc.CourseCode")).ListAsync<string>());
					// can't be sure of NULL comparison ordering aside from they should
					// either come first or last
					if (result[0] == null)
					{
						Assert.IsNull(result[1]);
						Assert.AreEqual("HIB-A", result[2]);
					}
					else
					{
						Assert.IsNull(result[2]);
						Assert.IsNull(result[1]);
						Assert.AreEqual("HIB-A", result[0]);
					}

					// test == on non existent value
					result = await (session.CreateCriteria<Student>().CreateAlias("PreferredCourse", "pc", JoinType.LeftOuterJoin, Restrictions.Eq("pc.CourseCode", "HIB-R")).SetProjection(Property.ForName("pc.CourseCode")).AddOrder(Order.Asc("pc.CourseCode")).ListAsync<string>());
					Assert.AreEqual(3, result.Count);
					Assert.IsNull(result[2]);
					Assert.IsNull(result[1]);
					Assert.IsNull(result[0]);
					// test != on one existing value
					result = await (session.CreateCriteria<Student>().CreateAlias("PreferredCourse", "pc", JoinType.LeftOuterJoin, Restrictions.Not(Restrictions.Eq("pc.CourseCode", "HIB-A"))).SetProjection(Property.ForName("pc.CourseCode")).AddOrder(Order.Asc("pc.CourseCode")).ListAsync<string>());
					Assert.AreEqual(3, result.Count);
					// can't be sure of NULL comparison ordering aside from they should
					// either come first or last
					if (result[0] == null)
					{
						Assert.IsNull(result[1]);
						Assert.AreEqual("HIB-B", result[2]);
					}
					else
					{
						Assert.AreEqual("HIB-B", result[0]);
						Assert.IsNull(result[1]);
						Assert.IsNull(result[2]);
					}

					// test != on one existing value (using clone)
					var criteria = session.CreateCriteria<Student>().CreateAlias("PreferredCourse", "pc", JoinType.LeftOuterJoin, Restrictions.Not(Restrictions.Eq("pc.CourseCode", "HIB-A"))).SetProjection(Property.ForName("pc.CourseCode")).AddOrder(Order.Asc("pc.CourseCode"));
					var clonedCriteria = CriteriaTransformer.Clone(criteria);
					result = await (clonedCriteria.ListAsync<string>());
					Assert.AreEqual(3, result.Count);
					// can't be sure of NULL comparison ordering aside from they should
					// either come first or last
					if (result[0] == null)
					{
						Assert.IsNull(result[1]);
						Assert.AreEqual("HIB-B", result[2]);
					}
					else
					{
						Assert.AreEqual("HIB-B", result[0]);
						Assert.IsNull(result[1]);
						Assert.IsNull(result[2]);
					}

					await (session.DeleteAsync(gavin));
					await (session.DeleteAsync(leonardo));
					await (session.DeleteAsync(johnDoe));
					await (session.DeleteAsync(courseA));
					await (session.DeleteAsync(courseB));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task IgnoreCaseAsync()
		{
			//SqlServer collation set to Latin1_General_BIN
			//when database created to validate this test
			Course c1 = new Course();
			c1.CourseCode = "course-1";
			c1.Description = "Advanced NHibernate";
			Course c2 = new Course();
			c2.CourseCode = "course-2";
			c2.Description = "advanced csharp";
			Course c3 = new Course();
			c3.CourseCode = "course-3";
			c3.Description = "advanced UnitTesting";
			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.SaveAsync(c1));
					await (session.SaveAsync(c2));
					await (session.SaveAsync(c3));
					await (t.CommitAsync());
				}

			// this particular selection is commented out if collation is not Latin1_General_BIN
			//using (ISession session = OpenSession())
			//{
			//    // order the courses in binary order - assumes collation Latin1_General_BIN
			//    IList result =
			//        session.CreateCriteria(typeof(Course)).AddOrder(Order.Asc("Description")).List();
			//    Assert.AreEqual(3, result.Count);
			//    Course firstResult = (Course)result[0];
			//    Assert.IsTrue(firstResult.Description.Contains("Advanced NHibernate"), "Description should have 'Advanced NHibernate', but has " + firstResult.Description);
			//}
			using (ISession session = OpenSession())
			{
				// order the courses after all descriptions have been converted to lower case
				IList result = await (session.CreateCriteria(typeof (Course)).AddOrder(Order.Asc("Description").IgnoreCase()).ListAsync());
				Assert.AreEqual(3, result.Count);
				Course firstResult = (Course)result[0];
				Assert.IsTrue(firstResult.Description.Contains("advanced csharp"), "Description should have 'advanced csharp', but has " + firstResult.Description);
			}

			using (ISession session = OpenSession())
				using (ITransaction t = session.BeginTransaction())
				{
					await (session.DeleteAsync(c1));
					await (session.DeleteAsync(c2));
					await (session.DeleteAsync(c3));
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task CanSetLockModeOnDetachedCriteriaAsync()
		{
			//NH-3710
			var dc = DetachedCriteria.For(typeof (Student)).SetLockMode(LockMode.Upgrade);
			using (var session = OpenSession())
				using (var tx = session.BeginTransaction())
				{
					await (session.SaveAsync(new Student{Name = "Ricardo Peres", StudentNumber = 666, CityState = new CityState("Coimbra", "Portugal")}));
					await (session.FlushAsync());
					var ec = dc.GetExecutableCriteria(session);
					var countExec = CriteriaTransformer.TransformToRowCount(ec);
					var countRes = await (countExec.UniqueResultAsync());
					Assert.AreEqual(countRes, 1);
				}
		}
	}
}
#endif
