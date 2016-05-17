#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Criterion;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Immutable.EntityWithMutableCollection
{
	/// <summary>
	/// Hibernate tests ported from trunk revision 19910 (July 8, 2010)
	/// </summary>
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public abstract partial class AbstractEntityWithManyToManyTest : TestCase
	{
		[Test]
		public async Task UpdatePropertyAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			p.AddContract(new Contract(null, "gail", "phone"));
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			p.Description = "new plan";
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			Contract c = p.Contracts.First();
			c.CustomerName = "yogi";
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(1));
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task CreateWithNonEmptyManyToManyCollectionOfNewAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			p.AddContract(new Contract(null, "gail", "phone"));
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			Contract c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(1));
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task CreateWithNonEmptyManyToManyCollectionOfExistingAsync()
		{
			ClearCounts();
			Contract c = new Contract(null, "gail", "phone");
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(c));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(0);
			ClearCounts();
			Plan p = new Plan("plan");
			p.AddContract(c);
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.SaveAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(isContractVersioned ? 1 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(1));
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task AddNewManyToManyElementToPersistentEntityAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.GetAsync<Plan>(p.Id));
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			p.AddContract(new Contract(null, "gail", "phone"));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(isContractVersioned ? 1 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			Contract c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(1));
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task AddExistingManyToManyElementToPersistentEntityAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (s.PersistAsync(c));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.GetAsync<Plan>(p.Id));
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			c = await (s.GetAsync<Contract>(c.Id));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(0));
			}

			p.AddContract(c);
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(0);
			AssertUpdateCount(isContractVersioned && isPlanVersioned ? 2 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task CreateWithEmptyManyToManyCollectionUpdateWithExistingElementAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (s.PersistAsync(c));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			p.AddContract(c);
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(0);
			AssertUpdateCount(isContractVersioned && isPlanVersioned ? 2 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task CreateWithNonEmptyManyToManyCollectionUpdateWithNewElementAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			p.AddContract(c);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			Contract newC = new Contract(null, "sherman", "telepathy");
			p.AddContract(newC);
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(isContractVersioned ? 1 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(2));
			foreach (Contract aContract in p.Contracts)
			{
				if (aContract.Id == c.Id)
				{
					Assert.That(aContract.CustomerName, Is.EqualTo("gail"));
				}
				else if (aContract.Id == newC.Id)
				{
					Assert.That(aContract.CustomerName, Is.EqualTo("sherman"));
				}
				else
				{
					Assert.Fail("unknown contract");
				}

				if (isPlanContractsBidirectional)
				{
					Assert.That(aContract.Plans.First(), Is.SameAs(p));
				}
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(3);
		}

		[Test]
		public async Task CreateWithEmptyManyToManyCollectionMergeWithExistingElementAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (s.PersistAsync(c));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			p.AddContract(c);
			s = OpenSession();
			t = s.BeginTransaction();
			p = (Plan)s.Merge(p);
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(0);
			AssertUpdateCount(isContractVersioned && isPlanVersioned ? 2 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task CreateWithNonEmptyManyToManyCollectionMergeWithNewElementAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			p.AddContract(c);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			Contract newC = new Contract(null, "yogi", "mail");
			p.AddContract(newC);
			s = OpenSession();
			t = s.BeginTransaction();
			p = (Plan)s.Merge(p);
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(isContractVersioned && isPlanVersioned ? 1 : 0); // NH-specific: Hibernate issues a separate UPDATE for the version number
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(2));
			foreach (Contract aContract in p.Contracts)
			{
				if (aContract.Id == c.Id)
				{
					Assert.That(aContract.CustomerName, Is.EqualTo("gail"));
				}
				else if (!aContract.CustomerName.Equals(newC.CustomerName))
				{
					Assert.Fail("unknown contract:" + aContract.CustomerName);
				}

				if (isPlanContractsBidirectional)
				{
					Assert.That(aContract.Plans.First(), Is.SameAs(p));
				}
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(3);
		}

		[Test]
		public async Task RemoveManyToManyElementUsingUpdateAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			p.AddContract(c);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			p.RemoveContract(c);
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(0));
			}

			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(isContractVersioned ? 1 : 0);
			AssertDeleteCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			if (isPlanContractsInverse)
			{
				Assert.That(p.Contracts.Count, Is.EqualTo(1));
				c = p.Contracts.First();
				Assert.That(c.CustomerName, Is.EqualTo("gail"));
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}
			else
			{
				Assert.That(p.Contracts.Count, Is.EqualTo(0));
				c = await (s.CreateCriteria<Contract>().UniqueResultAsync<Contract>());
				if (isPlanContractsBidirectional)
				{
					Assert.That(c.Plans.Count, Is.EqualTo(0));
				}

				await (s.DeleteAsync(c));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task RemoveManyToManyElementUsingUpdateBothSidesAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			p.AddContract(c);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			p.RemoveContract(c);
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(0));
			}

			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(p));
			await (s.UpdateAsync(c));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(isContractVersioned && isPlanVersioned ? 2 : 0);
			AssertDeleteCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			c = await (s.CreateCriteria<Contract>().UniqueResultAsync<Contract>());
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(0));
			}

			await (s.DeleteAsync(c));
			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task RemoveManyToManyElementUsingMergeAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			p.AddContract(c);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			p.RemoveContract(c);
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(0));
			}

			s = OpenSession();
			t = s.BeginTransaction();
			p = (Plan)s.Merge(p);
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(isContractVersioned ? 1 : 0);
			AssertDeleteCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			if (isPlanContractsInverse)
			{
				Assert.That(p.Contracts.Count, Is.EqualTo(1));
				c = p.Contracts.First();
				Assert.That(c.CustomerName, Is.EqualTo("gail"));
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}
			else
			{
				Assert.That(p.Contracts.Count, Is.EqualTo(0));
				c = await (s.CreateCriteria<Contract>().UniqueResultAsync<Contract>());
				if (isPlanContractsBidirectional)
				{
					Assert.That(c.Plans.Count, Is.EqualTo(0));
				}

				await (s.DeleteAsync(c));
			}

			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task RemoveManyToManyElementUsingMergeBothSidesAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			p.AddContract(c);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			p.RemoveContract(c);
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(0));
			}

			s = OpenSession();
			t = s.BeginTransaction();
			p = (Plan)s.Merge(p);
			c = (Contract)s.Merge(c);
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(isContractVersioned && isPlanVersioned ? 2 : 0);
			AssertDeleteCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			c = await (s.CreateCriteria<Contract>().UniqueResultAsync<Contract>());
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(0));
			}

			await (s.DeleteAsync(c));
			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(2);
		}

		[Test]
		public async Task DeleteManyToManyElementAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			p.AddContract(c);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(p));
			p.RemoveContract(c);
			await (s.DeleteAsync(c));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(isContractVersioned ? 1 : 0);
			AssertDeleteCount(1);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			c = await (s.CreateCriteria<Contract>().UniqueResultAsync<Contract>());
			Assert.That(c, Is.Null);
			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(1);
		}

		[Test]
		public async Task RemoveManyToManyElementByDeleteAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			Contract c = new Contract(null, "gail", "phone");
			p.AddContract(c);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			p.RemoveContract(c);
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.Count, Is.EqualTo(0));
			}

			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(p));
			await (s.DeleteAsync(c));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(isPlanVersioned ? 1 : 0);
			AssertDeleteCount(1);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(1);
		}

		[Test]
		public async Task ManyToManyCollectionOptimisticLockingWithMergeAsync()
		{
			ClearCounts();
			Plan pOrig = new Plan("plan");
			Contract cOrig = new Contract(null, "gail", "phone");
			pOrig.AddContract(cOrig);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(pOrig));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			Plan p = await (s.GetAsync<Plan>(pOrig.Id));
			Contract newC = new Contract(null, "sherman", "note");
			p.AddContract(newC);
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(isContractVersioned ? 1 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			pOrig.RemoveContract(cOrig);
			try
			{
				s.Merge(pOrig);
				Assert.That(isContractVersioned, Is.False);
			}
			catch (StaleObjectStateException)
			{
				Assert.That(isContractVersioned, Is.True);
			}
			finally
			{
				t.Rollback();
			}

			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			await (s.DeleteAsync(p));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(3);
		}

		[Test]
		public async Task ManyToManyCollectionOptimisticLockingWithUpdateAsync()
		{
			ClearCounts();
			Plan pOrig = new Plan("plan");
			Contract cOrig = new Contract(null, "gail", "phone");
			pOrig.AddContract(cOrig);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(pOrig));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			Plan p = await (s.GetAsync<Plan>(pOrig.Id));
			Contract newC = new Contract(null, "yogi", "pawprint");
			p.AddContract(newC);
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(isContractVersioned ? 1 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			pOrig.RemoveContract(cOrig);
			await (s.UpdateAsync(pOrig));
			try
			{
				await (t.CommitAsync());
				Assert.That(isContractVersioned, Is.False);
			}
			catch (StaleObjectStateException)
			{
				Assert.That(isContractVersioned, Is.True);
				t.Rollback();
			}

			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			await (s.DeleteAsync(p));
			s.CreateQuery("delete from Contract").ExecuteUpdate();
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync<long>()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task MoveManyToManyElementToNewEntityCollectionAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			p.AddContract(new Contract(null, "gail", "phone"));
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(2);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			Contract c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			p.RemoveContract(c);
			Plan p2 = new Plan("new plan");
			p2.AddContract(c);
			await (s.SaveAsync(p2));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(1);
			AssertUpdateCount(isPlanVersioned && isContractVersioned ? 2 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().Add(Restrictions.IdEq(p.Id)).UniqueResultAsync<Plan>());
			p2 = await (s.CreateCriteria<Plan>().Add(Restrictions.IdEq(p2.Id)).UniqueResultAsync<Plan>());
			/*
			if (isPlanContractsInverse) {
				Assert.That(p.Contracts.Count, Is.EqualTo(1));
				c = p.Contracts.First();
				Assert.That(c.CustomerName, Is.EqualTo("gail"));
				if (isPlanContractsBidirectional) {
					Assert.That(c.Plans.First(), Is.SameAs(p));
				}
				assertEquals( 0, p2.getContracts().size() );
			}
			else {
			*/
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			Assert.That(p2.Contracts.Count, Is.EqualTo(1));
			c = p2.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.First(), Is.SameAs(p2));
			}

			//}
			await (s.DeleteAsync(p));
			await (s.DeleteAsync(p2));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(3);
		}

		[Test]
		public async Task MoveManyToManyElementToExistingEntityCollectionAsync()
		{
			ClearCounts();
			Plan p = new Plan("plan");
			p.AddContract(new Contract(null, "gail", "phone"));
			Plan p2 = new Plan("plan2");
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			await (s.PersistAsync(p));
			await (s.PersistAsync(p2));
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(3);
			AssertUpdateCount(0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().Add(Restrictions.IdEq(p.Id)).UniqueResultAsync<Plan>());
			Assert.That(p.Contracts.Count, Is.EqualTo(1));
			Contract c = p.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.First(), Is.SameAs(p));
			}

			p.RemoveContract(c);
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(0);
			AssertUpdateCount(isPlanVersioned && isContractVersioned ? 2 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p2 = await (s.CreateCriteria<Plan>().Add(Restrictions.IdEq(p2.Id)).UniqueResultAsync<Plan>());
			c = await (s.CreateCriteria<Contract>().Add(Restrictions.IdEq(c.Id)).UniqueResultAsync<Contract>());
			p2.AddContract(c);
			await (t.CommitAsync());
			s.Close();
			AssertInsertCount(0);
			AssertUpdateCount(isPlanVersioned && isContractVersioned ? 2 : 0);
			ClearCounts();
			s = OpenSession();
			t = s.BeginTransaction();
			p = await (s.CreateCriteria<Plan>().Add(Restrictions.IdEq(p.Id)).UniqueResultAsync<Plan>());
			p2 = await (s.CreateCriteria<Plan>().Add(Restrictions.IdEq(p2.Id)).UniqueResultAsync<Plan>());
			/*
			if (isPlanContractsInverse) {
				Assert.That(p.Contracts.Count, Is.EqualTo(1));
				c = p.Contracts.First();
				Assert.That(c.CustomerName, Is.EqualTo("gail"));
				if (isPlanContractsBidirectional) {
					Assert.That(c.Plans.First(), Is.SameAs(p));
				}
				assertEquals( 0, p2.getContracts().size() );
			}
			else {
			*/
			Assert.That(p.Contracts.Count, Is.EqualTo(0));
			Assert.That(p2.Contracts.Count, Is.EqualTo(1));
			c = p2.Contracts.First();
			Assert.That(c.CustomerName, Is.EqualTo("gail"));
			if (isPlanContractsBidirectional)
			{
				Assert.That(c.Plans.First(), Is.SameAs(p2));
			}

			//}
			await (s.DeleteAsync(p));
			await (s.DeleteAsync(p2));
			Assert.That(await (s.CreateCriteria<Plan>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			Assert.That(await (s.CreateCriteria<Contract>().SetProjection(Projections.RowCountInt64()).UniqueResultAsync()), Is.EqualTo(0L));
			await (t.CommitAsync());
			s.Close();
			AssertUpdateCount(0);
			AssertDeleteCount(3);
		}
	}
}
#endif
