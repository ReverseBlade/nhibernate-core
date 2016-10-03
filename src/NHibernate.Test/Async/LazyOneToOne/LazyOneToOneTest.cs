﻿#if NET_4_5
using System;
using System.Collections;
using NHibernate.Intercept;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;
using System.Threading.Tasks;

namespace NHibernate.Test.LazyOneToOne
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class LazyOneToOneTestAsync : TestCaseAsync
	{
		protected override IList Mappings
		{
			get
			{
				return new[]{"LazyOneToOne.Person.hbm.xml"};
			}
		}

		protected override string MappingsAssembly
		{
			get
			{
				return "NHibernate.Test";
			}
		}

		//protected override bool AppliesTo(Dialect.Dialect dialect)
		//{
		//  // this test work only with Field interception (NH-1618)
		//  return FieldInterceptionHelper.IsInstrumented( new Person() );
		//}
		protected override void Configure(Cfg.Configuration configuration)
		{
			configuration.SetProperty(Environment.MaxFetchDepth, "2");
			configuration.SetProperty(Environment.UseSecondLevelCache, "false");
		}

		protected override string CacheConcurrencyStrategy
		{
			get
			{
				return null;
			}
		}

		[Test]
		public async Task LazyAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			var p = new Person{Name = "Gavin"};
			var p2 = new Person{Name = "Emmanuel"};
			var e = new Employee(p);
			new Employment(e, "JBoss");
			var old = new Employment(e, "IFA")
			{EndDate = DateTime.Today};
			await (s.PersistAsync(p));
			await (s.PersistAsync(p2));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateQuery("from Person where name='Gavin'").UniqueResultAsync<Person>());
			Assert.That(!await (NHibernateUtil.IsPropertyInitializedAsync(p, "Employee")));
			Assert.That(p.Employee.Person, Is.SameAs(p));
			Assert.That(NHibernateUtil.IsInitialized(p.Employee.Employments));
			Assert.That(p.Employee.Employments.Count, Is.EqualTo(1));
			p2 = await (s.CreateQuery("from Person where name='Emmanuel'").UniqueResultAsync<Person>());
			Assert.That(p2.Employee, Is.Null);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.GetAsync<Person>("Gavin"));
			Assert.That(!await (NHibernateUtil.IsPropertyInitializedAsync(p, "Employee")));
			Assert.That(p.Employee.Person, Is.SameAs(p));
			Assert.That(NHibernateUtil.IsInitialized(p.Employee.Employments));
			Assert.That(p.Employee.Employments.Count, Is.EqualTo(1));
			p2 = await (s.GetAsync<Person>("Emmanuel"));
			Assert.That(p2.Employee, Is.Null);
			await (s.DeleteAsync(p2));
			await (s.DeleteAsync(old));
			await (s.DeleteAsync(p));
			await (t.CommitAsync());
			s.Close();
		}
	}
}
#endif
