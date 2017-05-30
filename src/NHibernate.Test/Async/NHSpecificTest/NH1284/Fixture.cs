﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1284
{
	using System.Threading.Tasks;
	[TestFixture, Ignore("Not supported yet.")]
	public class FixtureAsync : BugTestCase
	{
		[Test]
		public async Task EmptyValueTypeComponentAsync()
		{
			Person jimmy;
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				Person p = new Person("Jimmy Hendrix");
				await (s.SaveAsync(p));
				await (tx.CommitAsync());
			}

			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				jimmy = (Person)await (s.GetAsync(typeof(Person), "Jimmy Hendrix"));
				await (tx.CommitAsync());
			}
			Assert.IsFalse(jimmy.Address.HasValue);

			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				await (s.DeleteAsync("from Person"));
				await (tx.CommitAsync());
			}
		}
	}
}