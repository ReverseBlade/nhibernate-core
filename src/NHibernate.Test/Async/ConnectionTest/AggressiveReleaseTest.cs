#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using NHibernate.Cfg;
using NHibernate.Util;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;
using System.Threading.Tasks;

namespace NHibernate.Test.ConnectionTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class AggressiveReleaseTestAsync : ConnectionManagementTestCaseAsync
	{
		protected override async Task ConfigureAsync(Configuration cfg)
		{
			await (base.ConfigureAsync(cfg));
			cfg.SetProperty(Environment.ReleaseConnections, "after_transaction");
			//cfg.SetProperty(Environment.ConnectionProvider, typeof(DummyConnectionProvider).AssemblyQualifiedName);
			//cfg.SetProperty(Environment.GenerateStatistics, "true");
			cfg.SetProperty(Environment.BatchSize, "0");
		}

		protected override ISession GetSessionUnderTest()
		{
			return OpenSession();
		}

		protected void Reconnect(ISession session)
		{
			session.Reconnect();
		}

		protected override void Prepare()
		{
		//DummyTransactionManager.INSTANCE.Begin();
		}

		protected override void Done()
		{
		//DummyTransactionManager.INSTANCE.Commit();
		}

		// Some additional tests specifically for the aggressive-Release functionality...
		[Test]
		public async Task SerializationOnAfterStatementAggressiveReleaseAsync()
		{
			Prepare();
			ISession s = GetSessionUnderTest();
			Silly silly = new Silly("silly");
			await (s.SaveAsync(silly));
			// this should cause the CM to obtain a connection, and then Release it
			await (s.FlushAsync());
			// We should be able to serialize the session at this point...
			SerializationHelper.Serialize(s);
			await (s.DeleteAsync(silly));
			await (s.FlushAsync());
			await (ReleaseAsync(s));
			Done();
		}

		[Test]
		public async Task SerializationFailsOnAfterStatementAggressiveReleaseWithOpenResourcesAsync()
		{
			Prepare();
			ISession s = GetSessionUnderTest();
			Silly silly = new Silly("silly");
			await (s.SaveAsync(silly));
			// this should cause the CM to obtain a connection, and then Release it
			await (s.FlushAsync());
			// both scroll() and iterate() cause the batcher to hold on
			// to resources, which should make aggresive-Release not Release
			// the connection (and thus cause serialization to fail)
			IEnumerable en = await (s.CreateQuery("from Silly").EnumerableAsync());
			try
			{
				SerializationHelper.Serialize(s);
				Assert.Fail("Serialization allowed on connected session; or aggressive Release released connection with open resources");
			}
			catch (InvalidOperationException)
			{
			// expected behavior
			}

			// Closing the ScrollableResults does currently force the batcher to
			// aggressively Release the connection
			NHibernateUtil.Close(en);
			SerializationHelper.Serialize(s);
			await (s.DeleteAsync(silly));
			await (s.FlushAsync());
			await (ReleaseAsync(s));
			Done();
		}

		[Test]
		public async Task QueryIterationAsync()
		{
			Prepare();
			ISession s = GetSessionUnderTest();
			Silly silly = new Silly("silly");
			await (s.SaveAsync(silly));
			await (s.FlushAsync());
			IEnumerable en = await (s.CreateQuery("from Silly").EnumerableAsync());
			IEnumerator itr = en.GetEnumerator();
			Assert.IsTrue(itr.MoveNext());
			Silly silly2 = (Silly)itr.Current;
			Assert.AreEqual(silly, silly2);
			NHibernateUtil.Close(itr);
			itr = (await (s.CreateQuery("from Silly").EnumerableAsync())).GetEnumerator();
			IEnumerator itr2 = (await (s.CreateQuery("from Silly where name = 'silly'").EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(itr.MoveNext());
			Assert.AreEqual(silly, itr.Current);
			Assert.IsTrue(itr2.MoveNext());
			Assert.AreEqual(silly, itr2.Current);
			NHibernateUtil.Close(itr);
			NHibernateUtil.Close(itr2);
			await (s.DeleteAsync(silly));
			await (s.FlushAsync());
			await (ReleaseAsync(s));
			Done();
		}

		//[Test]
		//public void QueryScrolling()
		//{
		//    Prepare();
		//    ISession s = GetSessionUnderTest();
		//    Silly silly = new Silly("silly");
		//    s.Save(silly);
		//    s.Flush();
		//    ScrollableResults sr = s.CreateQuery("from Silly").scroll();
		//    Assert.IsTrue(sr.next());
		//    Silly silly2 = (Silly) sr.get(0);
		//    Assert.AreEqual(silly, silly2);
		//    sr.Close();
		//    sr = s.CreateQuery("from Silly").Scroll();
		//    ScrollableResults sr2 = s.CreateQuery("from Silly where name = 'silly'").Scroll();
		//    Assert.IsTrue(sr.next());
		//    Assert.AreEqual(silly, sr.get(0));
		//    Assert.IsTrue(sr2.next());
		//    Assert.AreEqual(silly, sr2.get(0));
		//    sr.Close();
		//    sr2.Close();
		//    s.Delete(silly);
		//    s.Flush();
		//    Release(s);
		//    Done();
		//}
		[Test]
		public async Task SuppliedConnectionAsync()
		{
			Prepare();
			DbConnection originalConnection = await (sessions.ConnectionProvider.GetConnectionAsync());
			ISession session = sessions.OpenSession(originalConnection);
			Silly silly = new Silly("silly");
			await (session.SaveAsync(silly));
			// this will cause the connection manager to cycle through the aggressive Release logic;
			// it should not Release the connection since we explicitly suplied it ourselves.
			await (session.FlushAsync());
			Assert.IsTrue(originalConnection == session.Connection, "Different connections");
			await (session.DeleteAsync(silly));
			await (session.FlushAsync());
			await (ReleaseAsync(session));
			originalConnection.Close();
			Done();
		}

		// TODO
		//[Test]
		//public void BorrowedConnections()
		//{
		//    Prepare();
		//    ISession s = GetSessionUnderTest();
		//    DbConnection conn = s.Connection;
		//    Assert.IsTrue(((SessionImpl) s).ConnectionManager.HasBorrowedConnection);
		//    conn.Close();
		//    Assert.IsFalse(((SessionImpl) s).ConnectionManager.HasBorrowedConnection);
		//    Release(s);
		//    Done();
		//}
		[Test]
		public async Task ConnectionMaintanenceDuringFlushAsync()
		{
			Prepare();
			ISession s = GetSessionUnderTest();
			s.BeginTransaction();
			IList<Silly> entities = new List<Silly>();
			for (int i = 0; i < 10; i++)
			{
				Other other = new Other("other-" + i);
				Silly silly = new Silly("silly-" + i, other);
				entities.Add(silly);
				await (s.SaveAsync(silly));
			}

			await (s.FlushAsync());
			foreach (Silly silly in entities)
			{
				silly.Name = "new-" + silly.Name;
				silly.Other.Name = "new-" + silly.Other.Name;
			}

			//			long initialCount = sessions.Statistics.getConnectCount();
			await (s.FlushAsync());
			//			Assert.AreEqual(initialCount + 1, sessions.Statistics.getConnectCount(), "connection not maintained through Flush");
			await (s.DeleteAsync("from Silly"));
			await (s.DeleteAsync("from Other"));
			await (s.Transaction.CommitAsync());
			await (ReleaseAsync(s));
			Done();
		}
	}
}
#endif
