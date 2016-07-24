#if NET_4_5
using System;
using System.Collections.Generic;
using NUnit.Framework;
using NHibernate.Criterion;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1938
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		protected override bool AppliesTo(NHibernate.Dialect.Dialect dialect)
		{
			// Database needs to be case-sensitive
			return (dialect is NHibernate.Dialect.Oracle10gDialect);
		}

		[Test]
		public async Task Can_Query_By_Example_Case_InsensitiveAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.SaveAsync(new Person()
					{Name = "John Smith"}));
					Person examplePerson = new Person()
					{Name = "oHn"};
					IList<Person> matchingPeople;
					matchingPeople = await (s.CreateCriteria<Person>().Add(Example.Create(examplePerson).EnableLike(MatchMode.Anywhere).IgnoreCase()).ListAsync<Person>());
					Assert.That(matchingPeople.Count, Is.EqualTo(1));
					matchingPeople = await (s.CreateCriteria<Person>().Add(Example.Create(examplePerson).EnableLike(MatchMode.Anywhere)).ListAsync<Person>());
					Assert.That(matchingPeople.Count, Is.EqualTo(0));
					t.Rollback();
				}
		}
	}
}
#endif
