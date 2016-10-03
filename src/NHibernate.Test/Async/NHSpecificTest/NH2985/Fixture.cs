﻿#if NET_4_5
using System.Collections.Generic;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2985
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		protected override async Task OnTearDownAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from WebImage"));
					await (s.DeleteAsync("from ClassA"));
					await (tx.CommitAsync());
				}
		}

		[Test]
		public async Task TestAsync()
		{
			Guid a_id;
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					// Create an A and save it
					ClassA a = new ClassA();
					a.Name = "a1";
					await (s.SaveAsync(a));
					a_id = a.Id;
					a.Childs = new List<WebImage>();
					a.Childs.Add(new WebImage()
					{ImageUrl = "http://blabla/bla1.jpg", ImageData = new byte[]{11}});
					a.Childs.Add(new WebImage()
					{ImageUrl = "http://blabla/bla2.jpg", ImageData = new byte[]{13}});
					await (tx.CommitAsync());
				}

			// Clear the cache
			sessions.Evict(typeof (ClassA));
			sessions.Evict(typeof (WebImage));
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					// Load a so we can use it to load b
					ClassA a = await (s.GetAsync<ClassA>(a_id));
					Assert.That(a.Childs, Has.Count.EqualTo(2));
					var firstElement = a.Childs[0];
					//I'm expect an object to be equal to itself
					Assert.That(firstElement.Equals(firstElement));
					//expect a list to contain the first element
					Assert.That(a.Childs.Contains(a.Childs[0]));
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
