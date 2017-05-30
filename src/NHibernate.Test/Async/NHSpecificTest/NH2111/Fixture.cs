﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2111
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void OnTearDown()
		{
			using( ISession s = sessions.OpenSession() )
			{
				s.Delete( "from A" );
				s.Flush();
			}
		}

		[Test]
		public async Task SyncRootOnLazyLoadAsync()
		{
			A a = new A();
			a.Name = "first generic type";
			a.LazyItems = new List<string>();
			a.LazyItems.Add("first string");
			a.LazyItems.Add("second string");
			a.LazyItems.Add("third string");

			ISession s = OpenSession();
			await (s.SaveOrUpdateAsync(a));
			await (s.FlushAsync());
			s.Close();

			Assert.IsNotNull(((ICollection) a.LazyItems).SyncRoot);
			Assert.AreEqual("first string", a.LazyItems[0]);

			s = OpenSession();
			a = (A)await (s.LoadAsync(typeof(A), a.Id));

			Assert.IsNotNull(((ICollection) a.LazyItems).SyncRoot);
			Assert.AreEqual("first string", a.LazyItems[0]);

			s.Close();
		}
	}
}