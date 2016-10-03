﻿#if NET_4_5
using System.Collections;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.TypedManyToOne
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class TypedManyToOneTestAsync : TestCaseAsync
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
				return new[]{"TypedManyToOne.Customer.hbm.xml"};
			}
		}

		[Test]
		public async Task TestCreateQueryAsync()
		{
			var cust = new Customer();
			cust.CustomerId = "abc123";
			cust.Name = "Matt";
			var ship = new Address();
			ship.Street = "peachtree rd";
			ship.State = "GA";
			ship.City = "ATL";
			ship.Zip = "30326";
			ship.AddressId = new AddressId("SHIPPING", "xyz123");
			ship.Customer = cust;
			var bill = new Address();
			bill.Street = "peachtree rd";
			bill.State = "GA";
			bill.City = "ATL";
			bill.Zip = "30326";
			bill.AddressId = new AddressId("BILLING", "xyz123");
			bill.Customer = cust;
			cust.BillingAddress = bill;
			cust.ShippingAddress = ship;
			using (ISession s = sessions.OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.PersistAsync(cust));
					await (t.CommitAsync());
				}

			using (ISession s = sessions.OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					IList results = await (s.CreateQuery("from Customer cust left join fetch cust.BillingAddress where cust.CustomerId='abc123'").ListAsync());
					//IList results = s.CreateQuery("from Customer cust left join fetch cust.BillingAddress left join fetch cust.ShippingAddress").List();
					cust = (Customer)results[0];
					Assert.That(NHibernateUtil.IsInitialized(cust.ShippingAddress), Is.False);
					Assert.That(NHibernateUtil.IsInitialized(cust.BillingAddress), Is.True);
					Assert.That(cust.BillingAddress.Zip, Is.EqualTo("30326"));
					Assert.That(cust.ShippingAddress.Zip, Is.EqualTo("30326"));
					Assert.That(cust.BillingAddress.AddressId.Type, Is.EqualTo("BILLING"));
					Assert.That(cust.ShippingAddress.AddressId.Type, Is.EqualTo("SHIPPING"));
					await (t.CommitAsync());
				}

			using (ISession s = sessions.OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.SaveOrUpdateAsync(cust));
					ship = cust.ShippingAddress;
					cust.ShippingAddress = null;
					await (s.DeleteAsync("ShippingAddress", ship));
					await (s.FlushAsync());
					Assert.That(await (s.GetAsync("ShippingAddress", ship.AddressId)), Is.Null);
					await (s.DeleteAsync(cust));
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task TestCreateQueryNullAsync()
		{
			var cust = new Customer();
			cust.CustomerId = "xyz123";
			cust.Name = "Matt";
			using (ISession s = sessions.OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.PersistAsync(cust));
					await (t.CommitAsync());
				}

			using (ISession s = sessions.OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					IList results = await (s.CreateQuery("from Customer cust left join fetch cust.BillingAddress where cust.CustomerId='xyz123'").ListAsync());
					//IList results = s.CreateQuery("from Customer cust left join fetch cust.BillingAddress left join fetch cust.ShippingAddress").List();
					cust = (Customer)results[0];
					Assert.That(cust.ShippingAddress, Is.Null);
					Assert.That(cust.BillingAddress, Is.Null);
					await (s.DeleteAsync(cust));
					await (t.CommitAsync());
				}
		}
	}
}
#endif
