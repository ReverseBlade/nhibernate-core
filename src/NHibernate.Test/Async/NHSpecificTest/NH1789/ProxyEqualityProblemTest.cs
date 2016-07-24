#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Proxy;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH1789
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ProxyEqualityProblemTestAsync : BugTestCaseAsync
	{
		protected override async Task OnSetUpAsync()
		{
			await (base.OnSetUpAsync());
			using (ISession session = OpenSession())
			{
				ICat cat1 = new Cat("Marcel", 1);
				ICat cat2 = new Cat("Maurice", 2);
				await (session.SaveAsync(cat1));
				await (session.SaveAsync(cat2));
				await (session.FlushAsync());
			}
		}

		protected override async Task OnTearDownAsync()
		{
			await (base.OnTearDownAsync());
			using (ISession session = OpenSession())
			{
				string hql = "from System.Object";
				await (session.DeleteAsync(hql));
				await (session.FlushAsync());
			}
		}

		/// <summary>
		/// This test fails: when comparing a proxy with a non-proxy, I want the proxy to use the Equals() method on DomainObject to check for equality.
		/// It doesn't do it, so the equality fails.
		/// </summary>
		[Test]
		public async Task TestProxyEqualityProblemAsync()
		{
			using (ISession session = OpenSession())
			{
				//We load a proxy version of Maurice
				var mauriceProxy = await (session.LoadAsync<ICat>((long)2));
				Assert.IsTrue(mauriceProxy is INHibernateProxy, "The proxy should be of type INHibernateProxy");
				//From it's proxy, we get a non-proxied (concrete?) version
				var mauriceNonProxy = await (DomainObject.UnProxyAsync<ICat>(mauriceProxy));
				Assert.IsTrue(!(mauriceNonProxy is INHibernateProxy), "The non-proxy shouldn't be of type INHibernateProxy");
				//We check if the name and ID matches (as they should be because they are the same entity!)
				Assert.AreEqual(mauriceProxy.Name, mauriceNonProxy.Name, "The two objects should have the same name");
				Assert.AreEqual(mauriceProxy.ID, mauriceNonProxy.ID, "The two objects should have the same ID");
				//Now here's the problem:
				//When calling Equals() on the non-proxy, everything works (calling the overriden Equals() method on DomainObject as it should be)
				Assert.IsTrue(mauriceNonProxy.Equals(mauriceProxy), "The two instances should be declared equal");
				//But when calling it on the proxy, it doesn't, and they return a false for the equality, and that's a bug IMHO
				bool condition = mauriceProxy.Equals(mauriceNonProxy);
				Assert.IsTrue(condition, "The two instances should be declared equal");
			}
		}

		/// <summary>
		/// Here, instead of querying an ICat, we query a Cat directly: everything works, and DomainObject.Equals() is properly called on the proxy.
		/// </summary>
		[Test]
		public async Task TestProxyEqualityWhereItDoesWorkAsync()
		{
			using (ISession session = OpenSession())
			{
				//We load a proxy version of Maurice
				var mauriceProxy = await (session.LoadAsync<Cat>((long)2));
				Assert.IsTrue(mauriceProxy is INHibernateProxy, "The proxy should be of type INHibernateProxy");
				//From it's proxy, we get a non-proxied (concrete?) version
				var mauriceNonProxy = await (DomainObject.UnProxyAsync<Cat>(mauriceProxy));
				Assert.IsTrue(!(mauriceNonProxy is INHibernateProxy), "The non-proxy shouldn't be of type INHibernateProxy");
				//We check if the name and ID matches (as they should be because they are the same entity!)
				Assert.AreEqual(mauriceProxy.Name, mauriceNonProxy.Name, "The two objects should have the same name");
				Assert.AreEqual(mauriceProxy.ID, mauriceNonProxy.ID, "The two objects should have the same ID");
				//Because we queried a concrete class (Cat instead of ICat), here it works both ways:
				Assert.IsTrue(mauriceNonProxy.Equals(mauriceProxy), "The two instances should be declared equal");
				Assert.IsTrue(mauriceProxy.Equals(mauriceNonProxy), "The two instances should be declared equal");
			}
		}

		/// <summary>
		/// That's how I discovered something was wrong: here my object is not found in the collection, even if it's there.
		/// </summary>
		[Test] //, Ignore("To investigate. When run with the whole tests suit it fail...probably something related with the ProxyCache.")]
		public async Task TestTheProblemWithCollectionAsync()
		{
			using (ISession session = OpenSession())
			{
				//As before, we load a proxy, a non-proxy of the same entity, and checks everything is correct:
				var mauriceProxy = await (session.LoadAsync<ICat>((long)2));
				Assert.IsTrue(mauriceProxy is INHibernateProxy, "The proxy should be of type INHibernateProxy");
				var mauriceNonProxy = await (DomainObject.UnProxyAsync<ICat>(mauriceProxy));
				Assert.IsTrue(!(mauriceNonProxy is INHibernateProxy), "The non-proxy shouldn't be of type INHibernateProxy");
				Assert.AreEqual(mauriceProxy.Name, mauriceNonProxy.Name, "The two objects should have the same name");
				Assert.AreEqual(mauriceProxy.ID, mauriceNonProxy.ID, "The two objects should have the same ID");
				//Ok now we add the proxy version into a collection:
				var collection = new List<ICat>{mauriceProxy};
				//The proxy should be able to find itself:
				Assert.IsTrue(collection.Contains(mauriceProxy), "The proxy should be present in the collection");
				//Now, the non-proxy should also be able to find itself in the collection, using the Equals() on DomainObject...
				Assert.IsTrue(collection.Contains(mauriceNonProxy), "The proxy should be present in the collection");
			}
		}
	}
}
#endif
