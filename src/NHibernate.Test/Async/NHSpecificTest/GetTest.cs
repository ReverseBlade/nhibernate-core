﻿#if NET_4_5
using System.Collections;
using NHibernate.DomainModel;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class GetTestAsync : TestCaseAsync
	{
		protected override IList Mappings
		{
			get
			{
				// have to use classes with proxies to test difference
				// between Get() and Load()
				return new string[]{"ABCProxy.hbm.xml"};
			}
		}

		protected override async Task OnTearDownAsync()
		{
			using (ISession s = sessions.OpenSession())
			{
				await (s.DeleteAsync("from A"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task GetVsLoadAsync()
		{
			A a = new A("name");
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(a));
			}

			using (ISession s = OpenSession())
			{
				A loadedA = (A)await (s.LoadAsync(typeof (A), a.Id));
				Assert.IsFalse(NHibernateUtil.IsInitialized(loadedA), "Load should not initialize the object");
				Assert.IsNotNull(await (s.LoadAsync(typeof (A), (a.Id + 1))), "Loading non-existent object should not return null");
			}

			using (ISession s = OpenSession())
			{
				A gotA = (A)await (s.GetAsync(typeof (A), a.Id));
				Assert.IsTrue(NHibernateUtil.IsInitialized(gotA), "Get should initialize the object");
				Assert.IsNull(await (s.GetAsync(typeof (A), (a.Id + 1))), "Getting non-existent object should return null");
			}
		}

		[Test]
		public async Task GetAndModifyAsync()
		{
			A a = new A("name");
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(a));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				a = await (s.GetAsync(typeof (A), a.Id)) as A;
				a.Name = "modified";
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				a = await (s.GetAsync(typeof (A), a.Id)) as A;
				Assert.AreEqual("modified", a.Name, "the name was modified");
			}
		}

		[Test]
		public async Task GetAfterLoadAsync()
		{
			long id;
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					A a = new A("name");
					id = (long)await (s.SaveAsync(a));
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					A loadedA = (A)await (s.LoadAsync(typeof (A), id));
					Assert.IsFalse(NHibernateUtil.IsInitialized(loadedA));
					A gotA = (A)await (s.GetAsync(typeof (A), id));
					Assert.IsTrue(NHibernateUtil.IsInitialized(gotA));
					Assert.AreSame(loadedA, gotA);
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					A loadedNonExistentA = (A)await (s.LoadAsync(typeof (A), -id));
					Assert.IsFalse(NHibernateUtil.IsInitialized(loadedNonExistentA));
					// changed behavior because NH-1252
					Assert.IsNull(await (s.GetAsync(typeof (A), -id)));
					await (tx.CommitAsync());
				}
		}
	}
}
#endif
