﻿#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using log4net;
using NHibernate.Dialect;
using NHibernate.Dialect.Function;
using NHibernate.DomainModel;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Legacy
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class SQLFunctionsTestAsync : TestCaseAsync
	{
		private static readonly ILog log = LogManager.GetLogger(typeof (SQLFunctionsTestAsync));
		protected override IList Mappings
		{
			get
			{
				return new string[]{"Simple.hbm.xml", "Blobber.hbm.xml", "Broken.hbm.xml"};
			}
		}

		[Test]
		public async Task DialectSQLFunctionsAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			IEnumerator iter = (await (s.CreateQuery("select max(s.Count) from s in class Simple").EnumerableAsync())).GetEnumerator();
			if (Dialect is MySQLDialect // Added two dialects below for NH
			|| Dialect is MsSql2000Dialect || Dialect is PostgreSQLDialect)
			{
				Assert.IsTrue(iter.MoveNext());
				Assert.IsNull(iter.Current);
			}

			Simple simple = new Simple();
			simple.Name = "Simple Dialect Function Test";
			simple.Address = "Simple Address";
			simple.Pay = 45.8f;
			simple.Count = 2;
			await (s.SaveAsync(simple, 10L));
			// Test to make sure allocating an specified object operates correctly.
			Assert.AreEqual(1, (await (s.CreateQuery("select new S(s.Count, s.Address) from s in class Simple").ListAsync())).Count);
			// Quick check the base dialect functions operate correctly
			Assert.AreEqual(1, (await (s.CreateQuery("select max(s.Count) from s in class Simple").ListAsync())).Count);
			Assert.AreEqual(1, (await (s.CreateQuery("select count(*) from s in class Simple").ListAsync())).Count);
			if (Dialect is Oracle8iDialect)
			{
				// Check Oracle Dialect mix of dialect functions - no args (no parenthesis and single arg functions
				IList rset = await (s.CreateQuery("select s.Name, sysdate, trunc(s.Pay), round(s.Pay) from s in class Simple").ListAsync());
				object[] row = (object[])rset[0];
				Assert.IsNotNull(row[0], "Name string should have been returned");
				Assert.IsNotNull(row[1], "Todays Date should have been returned");
				Assert.AreEqual(45f, row[2], "trunc(45.8) result was incorrect");
				Assert.AreEqual(46f, row[3], "round(45.8) result was incorrect");
				simple.Pay = -45.8f;
				await (s.UpdateAsync(simple));
				// Test type conversions while using nested functions (Float to Int).
				rset = await (s.CreateQuery("select abs(round(s.Pay)) from s in class Simple").ListAsync());
				Assert.AreEqual(46f, rset[0], "abs(round(-45.8)) result was incorrect");
				rset = await (s.CreateQuery("select left('abc', 2), right('abc', 2) from s in class Simple").ListAsync());
				row = (object[])rset[0];
				Assert.AreEqual("ab", row[0], "Left function is broken.");
				Assert.AreEqual("bc", row[1], "Right function is broken.");
				// Test a larger depth 3 function example - Not a useful combo other than for testing
				Assert.AreEqual(1, (await (s.CreateQuery("select trunc(round(length('A'))) from s in class Simple").ListAsync())).Count);
			// NOTE: In Oracle this will fail as the translator will expect two columns in return
			//Assert.AreEqual(1,
			//                s.CreateQuery("select trunc(round(sysdate)) from s in class Simple").List().Count);
			// Test the oracle standard NVL funtion as a test of multi-param functions...
			// NOTE: commented out for NH, since Pay is a value type and will never be null
			//simple.Pay = null;
			//s.Update( simple );
			//Assert.AreEqual( 0,
			//	s.CreateQuery("select MOD( NVL(s.Pay, 5000), 2 ) from Simple as s where s.id = 10").List()[0] );
			}

			// NOTE: Commented out for NHibernate, no HSQL dialect.
			//if ( (getDialect() is HSQLDialect) ) 
			//{
			//	// Test the hsql standard MOD funtion as a test of multi-param functions...
			//	Integer value = (Integer) s.find("select MOD(s.count, 2) from Simple as s where s.id = 10" ).get(0);
			//	assertTrue( 0 == value.intValue() );
			//}
			await (s.DeleteAsync(simple));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task SetPropertiesAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Simple simple = new Simple();
			simple.Name = "Simple 1";
			await (s.SaveAsync(simple, (long)10));
			IQuery q = s.CreateQuery("from s in class Simple where s.Name=:Name and s.Count=:Count");
			q.SetProperties(simple);
			Assert.AreEqual(simple, (await (q.ListAsync()))[0]);
			await (s.DeleteAsync(simple));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task BrokenAsync()
		{
			if (Dialect is Oracle8iDialect)
				Assert.Ignore("Does not apply to " + typeof (Oracle8iDialect).FullName);
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Broken b = new Fixed();
			b.Id = 123;
			b.OtherId = "foobar";
			await (s.SaveAsync(b));
			await (s.FlushAsync());
			b.Timestamp = DateTime.Now;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(b));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			b = (Broken)await (s.LoadAsync(typeof (Broken), b));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.DeleteAsync(b));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task NothingToUpdateAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Simple simple = new Simple();
			simple.Name = "Simple 1";
			await (s.SaveAsync(simple, (long)10));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(simple, (long)10));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(simple, (long)10));
			await (s.DeleteAsync(simple));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CachedQueryAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Simple simple = new Simple();
			simple.Name = "Simple 1";
			await (s.SaveAsync(simple, 10L));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IQuery q = s.CreateQuery("from Simple s where s.Name=?");
			q.SetCacheable(true);
			q.SetString(0, "Simple 1");
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			q = s.CreateQuery("from Simple s where s.Name=:name");
			q.SetCacheable(true);
			q.SetString("name", "Simple 1");
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			simple = (Simple)(await (q.ListAsync()))[0];
			q.SetString("name", "Simple 2");
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			simple.Name = "Simple 2";
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			q = s.CreateQuery("from Simple s where s.Name=:name");
			q.SetString("name", "Simple 2");
			q.SetCacheable(true);
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(simple, 10L));
			await (s.DeleteAsync(simple));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			q = s.CreateQuery("from Simple s where s.Name=?");
			q.SetCacheable(true);
			q.SetString(0, "Simple 1");
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
		}

		private string LocateAppropriateDialectFunctionNameForAliasTest()
		{
			foreach (KeyValuePair<string, ISQLFunction> de in Dialect.Functions)
			{
				ISQLFunction function = de.Value;
				if (!function.HasArguments && !function.HasParenthesesIfNoArguments)
				{
					return de.Key;
				}
			}

			return null;
		}

		[Test]
		public async Task SQLFunctionAsAliasAsync()
		{
			string functionName = LocateAppropriateDialectFunctionNameForAliasTest();
			if (functionName == null)
			{
				log.Info("Dialect does not list any no-arg functions");
				return;
			}

			log.Info("Using function named [" + functionName + "] for 'function as alias' test");
			string query = "select " + functionName + " from Simple as " + functionName + " where " + functionName + ".id = 10";
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Simple simple = new Simple();
			simple.Name = "Simple 1";
			await (s.SaveAsync(simple, 10L));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IList result = await (s.CreateQuery(query).ListAsync());
			Assert.IsTrue(result[0] is Simple, "Unexpected result type [" + result[0].GetType().Name + "]");
			await (s.DeleteAsync(result[0]));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CachedQueryOnInsertAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Simple simple = new Simple();
			simple.Name = "Simple 1";
			await (s.SaveAsync(simple, 10L));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IQuery q = s.CreateQuery("from Simple s");
			IList list = await (q.SetCacheable(true).ListAsync());
			Assert.AreEqual(1, list.Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			q = s.CreateQuery("from Simple s");
			list = await (q.SetCacheable(true).ListAsync());
			Assert.AreEqual(1, list.Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			Simple simple2 = new Simple();
			simple2.Count = 133;
			await (s.SaveAsync(simple2, 12L));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			q = s.CreateQuery("from Simple s");
			list = await (q.SetCacheable(true).ListAsync());
			Assert.AreEqual(2, list.Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			q = s.CreateQuery("from Simple s");
			list = await (q.SetCacheable(true).ListAsync());
			Assert.AreEqual(2, list.Count);
			foreach (object obj in list)
			{
				await (s.DeleteAsync(obj));
			}

			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CachedQueryRegionAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Simple simple = new Simple();
			simple.Name = "Simple 1";
			await (s.SaveAsync(simple, 10L));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IQuery q = s.CreateQuery("from Simple s where s.Name=?");
			q.SetCacheRegion("foo");
			q.SetCacheable(true);
			q.SetString(0, "Simple 1");
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			q = s.CreateQuery("from Simple s where s.Name=:name");
			q.SetCacheRegion("foo");
			q.SetCacheable(true);
			q.SetString("name", "Simple 1");
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			simple = (Simple)(await (q.ListAsync()))[0];
			q.SetString("name", "Simple 2");
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			simple.Name = "Simple 2";
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(simple, 10L));
			await (s.DeleteAsync(simple));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			q = s.CreateQuery("from Simple s where s.Name=?");
			q.SetCacheRegion("foo");
			q.SetCacheable(true);
			q.SetString(0, "Simple 1");
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task SQLFunctionsAsync()
		{
			using (ISession s = OpenSession())
			{
				ITransaction t = s.BeginTransaction();
				Simple simple = new Simple();
				simple.Name = "Simple 1";
				await (s.SaveAsync(simple, (long)10));
				if (Dialect is DB2Dialect)
				{
					await (s.CreateQuery("from s in class Simple where repeat('foo', 3) = 'foofoofoo'").ListAsync());
					await (s.CreateQuery("from s in class Simple where repeat(s.Name, 3) = 'foofoofoo'").ListAsync());
					await (s.CreateQuery("from s in class Simple where repeat( lower(s.Name), 3 + (1-1) / 2) = 'foofoofoo'").ListAsync());
				}

				Assert.AreEqual(1, (await (s.CreateQuery("from s in class Simple where upper(s.Name) = 'SIMPLE 1'").ListAsync())).Count);
				Assert.AreEqual(1, (await (s.CreateQuery("from s in class Simple where not( upper(s.Name)='yada' or 1=2 or 'foo'='bar' or not('foo'='foo') or 'foo' like 'bar')").ListAsync())).Count);
				if (!(Dialect is MySQLDialect) && !(Dialect is MsSql2000Dialect))
				{
					// Dialect.MckoiDialect and Dialect.InterbaseDialect also included
					// My Sql has a funny concatenation operator
					Assert.AreEqual(1, (await (s.CreateQuery("from s in class Simple where lower(s.Name || ' foo')='simple 1 foo'").ListAsync())).Count);
				}

				if ((Dialect is MsSql2000Dialect))
				{
					Assert.AreEqual(1, (await (s.CreateQuery("from s in class Simple where lower( s.Name + ' foo' ) = 'simple 1 foo'").ListAsync())).Count);
				}

				/*
				 * TODO: uncomment if MckoiDialect is ever implemented 
				if( (dialect is Dialect.MckoiDialect) ) 
				{
					Assert.AreEqual( 1, s.CreateQuery("from s in class Simple where lower( concat(s.Name, ' foo') ) = 'simple 1 foo'").List().Count );
				}
				*/
				Simple other = new Simple();
				other.Name = "Simple 2";
				other.Count = 12;
				simple.Other = other;
				await (s.SaveAsync(other, (long)20));
				Assert.AreEqual(1, (await (s.CreateQuery("from s in class Simple where upper( s.Other.Name )='SIMPLE 2'").ListAsync())).Count);
				Assert.AreEqual(0, (await (s.CreateQuery("from s in class Simple where not (upper(s.Other.Name)='SIMPLE 2')").ListAsync())).Count);
				Assert.AreEqual(1, (await (s.CreateQuery("select distinct s from s in class Simple where ( ( s.Other.Count + 3) = (15*2)/2 and s.Count = 69) or ( (s.Other.Count + 2) / 7 ) = 2").ListAsync())).Count);
				Assert.AreEqual(1, (await (s.CreateQuery("select s from s in class Simple where ( ( s.Other.Count + 3) = (15*2)/2 and s.Count = 69) or ( (s.Other.Count + 2) / 7 ) = 2 order by s.Other.Count").ListAsync())).Count);
				Simple min = new Simple();
				min.Count = -1;
				await (s.SaveAsync(min, (long)30));
				if (Dialect.SupportsSubSelects && TestDialect.SupportsOperatorSome)
				{
					Assert.AreEqual(2, (await (s.CreateQuery("from s in class Simple where s.Count > ( select min(sim.Count) from sim in class NHibernate.DomainModel.Simple )").ListAsync())).Count);
					await (t.CommitAsync());
					t = s.BeginTransaction();
					Assert.AreEqual(2, (await (s.CreateQuery("from s in class Simple where s = some( select sim from sim in class NHibernate.DomainModel.Simple where sim.Count>=0) and s.Count >= 0").ListAsync())).Count);
					Assert.AreEqual(1, (await (s.CreateQuery("from s in class Simple where s = some( select sim from sim in class NHibernate.DomainModel.Simple where sim.Other.Count=s.Other.Count ) and s.Other.Count > 0").ListAsync())).Count);
				}

				IEnumerator enumer = (await (s.CreateQuery("select sum(s.Count) from s in class Simple group by s.Count having sum(s.Count) > 10 ").EnumerableAsync())).GetEnumerator();
				Assert.IsTrue(enumer.MoveNext());
				Assert.AreEqual(12, (Int64)enumer.Current); // changed cast from Int32 to Int64 (H3.2)
				Assert.IsFalse(enumer.MoveNext());
				if (Dialect.SupportsSubSelects)
				{
					enumer = (await (s.CreateQuery("select s.Count from s in class Simple group by s.Count having s.Count = 12").EnumerableAsync())).GetEnumerator();
					Assert.IsTrue(enumer.MoveNext());
				}

				enumer = (await (s.CreateQuery("select s.id, s.Count, count(t), max(t.Date) from s in class Simple, t in class Simple where s.Count = t.Count group by s.id, s.Count order by s.Count").EnumerableAsync())).GetEnumerator();
				IQuery q = s.CreateQuery("from s in class Simple");
				q.SetMaxResults(10);
				Assert.AreEqual(3, (await (q.ListAsync())).Count);
				q = s.CreateQuery("from s in class Simple");
				q.SetMaxResults(1);
				Assert.AreEqual(1, (await (q.ListAsync())).Count);
				q = s.CreateQuery("from s in class Simple");
				Assert.AreEqual(3, (await (q.ListAsync())).Count);
				q = s.CreateQuery("from s in class Simple where s.Name = ?");
				q.SetString(0, "Simple 1");
				Assert.AreEqual(1, (await (q.ListAsync())).Count);
				q = s.CreateQuery("from s in class Simple where s.Name = ? and upper(s.Name) = ?");
				q.SetString(1, "SIMPLE 1");
				q.SetString(0, "Simple 1");
				q.SetFirstResult(0);
				Assert.IsTrue((await (q.EnumerableAsync())).GetEnumerator().MoveNext());
				q = s.CreateQuery("from s in class Simple where s.Name = :foo and upper(s.Name) = :bar or s.Count=:count or s.Count=:count + 1");
				q.SetParameter("bar", "SIMPLE 1");
				q.SetString("foo", "Simple 1");
				q.SetInt32("count", 69);
				q.SetFirstResult(0);
				Assert.IsTrue((await (q.EnumerableAsync())).GetEnumerator().MoveNext());
				q = s.CreateQuery("select s.id from s in class Simple");
				q.SetFirstResult(1);
				q.SetMaxResults(2);
				IEnumerable enumerable = await (q.EnumerableAsync());
				int i = 0;
				foreach (object obj in enumerable)
				{
					Assert.IsTrue(obj is Int64);
					i++;
				}

				Assert.AreEqual(2, i);
				q = s.CreateQuery("select all s, s.Other from s in class Simple where s = :s");
				q.SetParameter("s", simple);
				Assert.AreEqual(1, (await (q.ListAsync())).Count);
				q = s.CreateQuery("from s in class Simple where s.Name in (:name_list) and s.Count > :count");
				IList list = new ArrayList(2);
				list.Add("Simple 1");
				list.Add("foo");
				q.SetParameterList("name_list", list);
				q.SetParameter("count", (int)-1);
				Assert.AreEqual(1, (await (q.ListAsync())).Count);
				await (s.DeleteAsync(other));
				await (s.DeleteAsync(simple));
				await (s.DeleteAsync(min));
				await (t.CommitAsync());
			}
		}
	}
}
#endif
