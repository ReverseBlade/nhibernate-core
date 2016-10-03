﻿#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Proxy;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;
using NUnit.Framework;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace NHibernate.Test.ReadOnly
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class ReadOnlySessionTestAsync : TestCaseAsync
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
				var mappings = new List<string>{"ReadOnly.DataPoint.hbm.xml"};
				if (TextHolder.SupportedForDialect(Dialect))
					mappings.Add("ReadOnly.TextHolder.hbm.xml");
				return mappings;
			}
		}

		[Test]
		public async Task ReadOnlyOnProxiesAsync()
		{
			DataPoint dp = null;
			long dpId = -1;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				s.BeginTransaction();
				dp = new DataPoint();
				dp.X = 0.1M;
				dp.Y = (decimal)System.Math.Cos((double)dp.X);
				dp.Description = "original";
				await (s.SaveAsync(dp));
				dpId = dp.Id;
				await (s.Transaction.CommitAsync());
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				s.BeginTransaction();
				s.DefaultReadOnly = true;
				Assert.That(s.DefaultReadOnly, Is.True);
				dp = (DataPoint)await (s.LoadAsync<DataPoint>(dpId));
				s.DefaultReadOnly = false;
				Assert.That(NHibernateUtil.IsInitialized(dp), Is.False, "was initialized");
				Assert.That(s.IsReadOnly(dp), Is.True);
				Assert.That(NHibernateUtil.IsInitialized(dp), Is.False, "was initialized during isReadOnly");
				dp.Description = "changed";
				Assert.That(NHibernateUtil.IsInitialized(dp), Is.True, "was not initialized during mod");
				Assert.That(dp.Description, Is.StringMatching("changed"), "desc not changed in memory");
				await (s.FlushAsync());
				await (s.Transaction.CommitAsync());
			}

			using (ISession s = OpenSession())
			{
				s.BeginTransaction();
				IList list = await (s.CreateQuery("from DataPoint where description = 'changed'").ListAsync());
				Assert.That(list.Count, Is.EqualTo(0), "change written to database");
				await (s.CreateQuery("delete from DataPoint").ExecuteUpdateAsync());
				await (s.Transaction.CommitAsync());
			}
		}

		[Test]
		public async Task ReadOnlySessionDefaultQueryIterateAsync()
		{
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					for (int i = 0; i < 100; i++)
					{
						DataPoint dp = new DataPoint();
						dp.X = 0.1M * i;
						dp.Y = (decimal)System.Math.Cos((double)dp.X);
						await (s.SaveAsync(dp));
					}

					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = true;
					IEnumerable enumerable = await (s.CreateQuery("from DataPoint dp order by dp.X asc").EnumerableAsync());
					s.DefaultReadOnly = false;
					int i = 0;
					foreach (DataPoint dp in enumerable)
					{
						if (++i == 50)
						{
							await (s.SetReadOnlyAsync(dp, false));
						}

						dp.Description = "done!";
					}

					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					try
					{
						IList single = await (s.CreateQuery("from DataPoint where Description = 'done!'").ListAsync());
						Assert.That(single.Count, Is.EqualTo(1));
					}
					finally
					{
						// cleanup
						await (s.CreateQuery("delete from DataPoint").ExecuteUpdateAsync());
					}

					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ReadOnlySessionModifiableQueryIterateAsync()
		{
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					for (int i = 0; i < 100; i++)
					{
						DataPoint dp = new DataPoint();
						dp.X = 0.1M * i;
						dp.Y = (decimal)System.Math.Cos((double)dp.X);
						await (s.SaveAsync(dp));
					}

					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = true;
					IEnumerable enumerable = await (s.CreateQuery("from DataPoint dp order by dp.X asc").SetReadOnly(false).EnumerableAsync());
					int i = 0;
					foreach (DataPoint dp in enumerable)
					{
						if (++i == 50)
						{
							await (s.SetReadOnlyAsync(dp, true));
						}

						dp.Description = "done!";
					}

					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					try
					{
						IList single = await (s.CreateQuery("from DataPoint where Description = 'done!'").ListAsync());
						Assert.That(single.Count, Is.EqualTo(99));
					}
					finally
					{
						// cleanup
						await (s.CreateQuery("delete from DataPoint").ExecuteUpdateAsync());
					}

					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ModifiableSessionReadOnlyQueryIterateAsync()
		{
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					for (int i = 0; i < 100; i++)
					{
						DataPoint dp = new DataPoint();
						dp.X = 0.1M * i;
						dp.Y = (decimal)System.Math.Cos((double)dp.X);
						await (s.SaveAsync(dp));
					}

					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					Assert.That(s.DefaultReadOnly, Is.False);
					IEnumerable enumerable = await (s.CreateQuery("from DataPoint dp order by dp.X asc").SetReadOnly(true).EnumerableAsync());
					int i = 0;
					foreach (DataPoint dp in enumerable)
					{
						if (++i == 50)
						{
							await (s.SetReadOnlyAsync(dp, false));
						}

						dp.Description = "done!";
					}

					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					try
					{
						IList single = await (s.CreateQuery("from DataPoint where Description = 'done!'").ListAsync());
						Assert.That(single.Count, Is.EqualTo(1));
					}
					finally
					{
						// cleanup
						await (s.CreateQuery("delete from DataPoint").ExecuteUpdateAsync());
					}

					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ModifiableSessionDefaultQueryReadOnlySessionIterateAsync()
		{
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					for (int i = 0; i < 100; i++)
					{
						DataPoint dp = new DataPoint();
						dp.X = 0.1M * i;
						dp.Y = (decimal)System.Math.Cos((double)dp.X);
						await (s.SaveAsync(dp));
					}

					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = false;
					IQuery query = s.CreateQuery("from DataPoint dp order by dp.X asc");
					s.DefaultReadOnly = true;
					IEnumerable enumerable = await (query.EnumerableAsync());
					s.DefaultReadOnly = false;
					int i = 0;
					foreach (DataPoint dp in enumerable)
					{
						if (++i == 50)
						{
							await (s.SetReadOnlyAsync(dp, false));
						}

						dp.Description = "done!";
					}

					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					try
					{
						IList single = await (s.CreateQuery("from DataPoint where Description = 'done!'").ListAsync());
						Assert.That(single.Count, Is.EqualTo(1));
					}
					finally
					{
						// cleanup
						await (s.CreateQuery("delete from DataPoint").ExecuteUpdateAsync());
					}

					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task QueryReadOnlyIterateAsync()
		{
			long lastDataPointId = 0;
			int nExpectedChanges = 0;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					DataPoint dp = null;
					for (int i = 0; i < 100; i++)
					{
						dp = new DataPoint();
						dp.X = 0.1M * i;
						dp.Y = (decimal)System.Math.Cos((double)dp.X);
						await (s.SaveAsync(dp));
					}

					await (t.CommitAsync());
					lastDataPointId = dp.Id;
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = false;
					IQuery query = s.CreateQuery("from DataPoint dp order by dp.X asc");
					Assert.That(query.IsReadOnly, Is.False);
					s.DefaultReadOnly = true;
					Assert.That(query.IsReadOnly, Is.True);
					s.DefaultReadOnly = false;
					Assert.That(query.IsReadOnly, Is.False);
					query.SetReadOnly(true);
					Assert.That(query.IsReadOnly, Is.True);
					s.DefaultReadOnly = true;
					Assert.That(query.IsReadOnly, Is.True);
					s.DefaultReadOnly = false;
					Assert.That(query.IsReadOnly, Is.True);
					query.SetReadOnly(false);
					Assert.That(query.IsReadOnly, Is.False);
					s.DefaultReadOnly = true;
					Assert.That(query.IsReadOnly, Is.False);
					query.SetReadOnly(true);
					Assert.That(query.IsReadOnly, Is.True);
					s.DefaultReadOnly = false;
					Assert.That(s.DefaultReadOnly, Is.False);
					IEnumerator<DataPoint> it = (await (query.EnumerableAsync<DataPoint>())).GetEnumerator();
					Assert.That(query.IsReadOnly, Is.True);
					DataPoint dpLast = await (s.GetAsync<DataPoint>(lastDataPointId));
					Assert.That(s.IsReadOnly(dpLast), Is.False);
					query.SetReadOnly(false);
					Assert.That(query.IsReadOnly, Is.False);
					Assert.That(s.DefaultReadOnly, Is.False);
					int i = 0;
					while (it.MoveNext())
					{
						Assert.That(s.DefaultReadOnly, Is.False);
						DataPoint dp = it.Current;
						Assert.That(s.DefaultReadOnly, Is.False);
						if (dp.Id == dpLast.Id)
						{
							//dpLast existed in the session before executing the read-only query
							Assert.That(s.IsReadOnly(dp), Is.False);
						}
						else
						{
							Assert.That(s.IsReadOnly(dp), Is.True);
						}

						if (++i == 50)
						{
							await (s.SetReadOnlyAsync(dp, false));
							nExpectedChanges = (dp == dpLast ? 1 : 2);
						}

						dp.Description = "done!";
					}

					Assert.That(s.DefaultReadOnly, Is.False);
					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					try
					{
						IList single = await (s.CreateQuery("from DataPoint where Description = 'done!'").ListAsync());
						Assert.That(single.Count, Is.EqualTo(nExpectedChanges));
					}
					finally
					{
						// cleanup
						await (s.CreateQuery("delete from DataPoint").ExecuteUpdateAsync());
					}

					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task QueryModifiableIterateAsync()
		{
			long lastDataPointId = 0;
			int nExpectedChanges = 0;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					DataPoint dp = null;
					for (int i = 0; i < 100; i++)
					{
						dp = new DataPoint();
						dp.X = 0.1M * i;
						dp.Y = (decimal)System.Math.Cos((double)dp.X);
						await (s.SaveAsync(dp));
					}

					await (t.CommitAsync());
					lastDataPointId = dp.Id;
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = true;
					IQuery query = s.CreateQuery("from DataPoint dp order by dp.X asc");
					Assert.That(query.IsReadOnly, Is.True);
					s.DefaultReadOnly = false;
					Assert.That(query.IsReadOnly, Is.False);
					s.DefaultReadOnly = true;
					Assert.That(query.IsReadOnly, Is.True);
					query.SetReadOnly(false);
					Assert.That(query.IsReadOnly, Is.False);
					s.DefaultReadOnly = false;
					Assert.That(query.IsReadOnly, Is.False);
					s.DefaultReadOnly = true;
					Assert.That(query.IsReadOnly, Is.False);
					query.SetReadOnly(true);
					Assert.That(query.IsReadOnly, Is.True);
					s.DefaultReadOnly = false;
					Assert.That(query.IsReadOnly, Is.True);
					query.SetReadOnly(false);
					Assert.That(query.IsReadOnly, Is.False);
					s.DefaultReadOnly = true;
					Assert.That(s.DefaultReadOnly, Is.True);
					IEnumerator<DataPoint> it = (await (query.EnumerableAsync<DataPoint>())).GetEnumerator();
					Assert.That(query.IsReadOnly, Is.False);
					DataPoint dpLast = await (s.GetAsync<DataPoint>(lastDataPointId));
					Assert.That(s.IsReadOnly(dpLast), Is.True);
					query.SetReadOnly(true);
					Assert.That(query.IsReadOnly, Is.True);
					Assert.That(s.DefaultReadOnly, Is.True);
					int i = 0;
					while (it.MoveNext())
					{
						Assert.That(s.DefaultReadOnly, Is.True);
						DataPoint dp = it.Current;
						Assert.That(s.DefaultReadOnly, Is.True);
						if (dp.Id == dpLast.Id)
						{
							//dpLast existed in the session before executing the read-only query
							Assert.That(s.IsReadOnly(dp), Is.True);
						}
						else
						{
							Assert.That(s.IsReadOnly(dp), Is.False);
						}

						if (++i == 50)
						{
							await (s.SetReadOnlyAsync(dp, true));
							nExpectedChanges = (dp == dpLast ? 99 : 98);
						}

						dp.Description = "done!";
					}

					Assert.That(s.DefaultReadOnly, Is.True);
					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					try
					{
						IList list = await (s.CreateQuery("from DataPoint where Description = 'done!'").ListAsync());
						Assert.That(list.Count, Is.EqualTo(nExpectedChanges));
					}
					finally
					{
						// cleanup
						await (s.CreateQuery("delete from DataPoint").ExecuteUpdateAsync());
					}

					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ReadOnlyRefreshAsync()
		{
			DataPoint dp = null;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = new DataPoint();
					dp.Description = "original";
					dp.X = 0.1M;
					dp.Y = (decimal)System.Math.Cos((double)dp.X);
					await (s.SaveAsync(dp));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				s.DefaultReadOnly = true;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = await (s.GetAsync<DataPoint>(dp.Id));
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(dp.Description, Is.EqualTo("original"));
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					await (s.RefreshAsync(dp));
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(dp.Description, Is.EqualTo("original"));
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					s.DefaultReadOnly = false;
					await (s.RefreshAsync(dp));
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(dp.Description, Is.EqualTo("original"));
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					dp = await (s.GetAsync<DataPoint>(dp.Id));
					Assert.That(dp.Description, Is.EqualTo("original"));
					await (s.DeleteAsync(dp));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ReadOnlyRefreshDetachedAsync()
		{
			DataPoint dp = null;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = new DataPoint();
					dp.Description = "original";
					dp.X = 0.1M;
					dp.Y = (decimal)System.Math.Cos((double)dp.X);
					await (s.SaveAsync(dp));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = false;
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					await (s.RefreshAsync(dp));
					Assert.That(dp.Description, Is.EqualTo("original"));
					Assert.That(s.IsReadOnly(dp), Is.False);
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					await (s.EvictAsync(dp));
					await (s.RefreshAsync(dp));
					Assert.That(dp.Description, Is.EqualTo("original"));
					Assert.That(s.IsReadOnly(dp), Is.False);
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					s.DefaultReadOnly = true;
					await (s.EvictAsync(dp));
					await (s.RefreshAsync(dp));
					Assert.That(dp.Description, Is.EqualTo("original"));
					Assert.That(s.IsReadOnly(dp), Is.True);
					dp.Description = "changed";
					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					dp = await (s.GetAsync<DataPoint>(dp.Id));
					Assert.That(dp.Description, Is.EqualTo("original"));
					await (s.DeleteAsync(dp));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ReadOnlyProxyRefreshAsync()
		{
			DataPoint dp = null;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = new DataPoint();
					dp.Description = "original";
					dp.X = 0.1M;
					dp.Y = (decimal)System.Math.Cos((double)dp.X);
					await (s.SaveAsync(dp));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = true;
					dp = await (s.LoadAsync<DataPoint>(dp.Id));
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(dp), Is.False);
					await (s.RefreshAsync(dp));
					Assert.That(NHibernateUtil.IsInitialized(dp), Is.False);
					Assert.That(s.IsReadOnly(dp), Is.True);
					s.DefaultReadOnly = false;
					await (s.RefreshAsync(dp));
					Assert.That(NHibernateUtil.IsInitialized(dp), Is.False);
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(dp.Description, Is.EqualTo("original"));
					Assert.That(NHibernateUtil.IsInitialized(dp), Is.True);
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(s.IsReadOnly(await (((INHibernateProxy)dp).HibernateLazyInitializer.GetImplementationAsync())), Is.True);
					await (s.RefreshAsync(dp));
					Assert.That(dp.Description, Is.EqualTo("original"));
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(s.IsReadOnly(await (((INHibernateProxy)dp).HibernateLazyInitializer.GetImplementationAsync())), Is.True);
					s.DefaultReadOnly = true;
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					await (s.RefreshAsync(dp));
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(s.IsReadOnly(await (((INHibernateProxy)dp).HibernateLazyInitializer.GetImplementationAsync())), Is.True);
					Assert.That(dp.Description, Is.EqualTo("original"));
					dp.Description = "changed";
					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					dp = await (s.GetAsync<DataPoint>(dp.Id));
					Assert.That(dp.Description, Is.EqualTo("original"));
					await (s.DeleteAsync(dp));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ReadOnlyProxyRefreshDetachedAsync()
		{
			DataPoint dp = null;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = new DataPoint();
					dp.Description = "original";
					dp.X = 0.1M;
					dp.Y = (decimal)System.Math.Cos((double)dp.X);
					await (s.SaveAsync(dp));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = true;
					dp = await (s.LoadAsync<DataPoint>(dp.Id));
					Assert.That(NHibernateUtil.IsInitialized(dp), Is.False);
					Assert.That(s.IsReadOnly(dp), Is.True);
					await (s.EvictAsync(dp));
					await (s.RefreshAsync(dp));
					Assert.That(NHibernateUtil.IsInitialized(dp), Is.False);
					s.DefaultReadOnly = false;
					Assert.That(s.IsReadOnly(dp), Is.True);
					await (s.EvictAsync(dp));
					await (s.RefreshAsync(dp));
					Assert.That(NHibernateUtil.IsInitialized(dp), Is.False);
					Assert.That(s.IsReadOnly(dp), Is.False);
					Assert.That(s.IsReadOnly(await (((INHibernateProxy)dp).HibernateLazyInitializer.GetImplementationAsync())), Is.False);
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					Assert.That(NHibernateUtil.IsInitialized(dp), Is.True);
					await (s.EvictAsync(dp));
					await (s.RefreshAsync(dp));
					Assert.That(dp.Description, Is.EqualTo("original"));
					Assert.That(s.IsReadOnly(dp), Is.False);
					Assert.That(s.IsReadOnly(await (((INHibernateProxy)dp).HibernateLazyInitializer.GetImplementationAsync())), Is.False);
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					s.DefaultReadOnly = true;
					await (s.EvictAsync(dp));
					await (s.RefreshAsync(dp));
					Assert.That(dp.Description, Is.EqualTo("original"));
					Assert.That(s.IsReadOnly(dp), Is.True);
					Assert.That(s.IsReadOnly(await (((INHibernateProxy)dp).HibernateLazyInitializer.GetImplementationAsync())), Is.True);
					dp.Description = "changed";
					Assert.That(dp.Description, Is.EqualTo("changed"));
					await (t.CommitAsync());
				}

				s.Clear();
				using (ITransaction t = s.BeginTransaction())
				{
					dp = await (s.GetAsync<DataPoint>(dp.Id));
					Assert.That(dp.Description, Is.EqualTo("original"));
					await (s.DeleteAsync(dp));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ReadOnlyDeleteAsync()
		{
			DataPoint dp = null;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = new DataPoint();
					dp.X = 0.1M;
					dp.Y = (decimal)System.Math.Cos((double)dp.X);
					await (s.SaveAsync(dp));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.DefaultReadOnly = true;
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = await (s.GetAsync<DataPoint>(dp.Id));
					s.DefaultReadOnly = false;
					Assert.That(s.IsReadOnly(dp), Is.True);
					await (s.DeleteAsync(dp));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					IList list = await (s.CreateQuery("from DataPoint where id=" + dp.Id).ListAsync());
					Assert.That(list.Count, Is.EqualTo(0));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ReadOnlyGetModifyAndDeleteAsync()
		{
			DataPoint dp = null;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = new DataPoint();
					dp.X = 0.1M;
					dp.Y = (decimal)System.Math.Cos((double)dp.X);
					await (s.SaveAsync(dp));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				s.DefaultReadOnly = true;
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = await (s.GetAsync<DataPoint>(dp.Id));
					s.DefaultReadOnly = true;
					dp.Description = "a DataPoint";
					await (s.DeleteAsync(dp));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					IList list = await (s.CreateQuery("from DataPoint where id=" + dp.Id).ListAsync());
					Assert.That(list.Count, Is.EqualTo(0));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task ReadOnlyOnTextTypeAsync()
		{
			if (!TextHolder.SupportedForDialect(Dialect))
				Assert.Ignore("Dialect doesn't support the 'text' data type.");
			string origText = "some huge text string";
			string newText = "some even bigger text string";
			long id = 0;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					TextHolder holder = new TextHolder(origText);
					await (s.SaveAsync(holder));
					id = holder.Id;
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = true;
					s.CacheMode = CacheMode.Ignore;
					TextHolder holder = await (s.GetAsync<TextHolder>(id));
					s.DefaultReadOnly = false;
					holder.TheText = newText;
					await (s.FlushAsync());
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					TextHolder holder = await (s.GetAsync<TextHolder>(id));
					Assert.That(holder.TheText, Is.EqualTo(origText), "change written to database");
					await (s.DeleteAsync(holder));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task MergeWithReadOnlyEntityAsync()
		{
			DataPoint dp = null;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = new DataPoint();
					dp.X = 0.1M;
					dp.Y = (decimal)System.Math.Cos((double)dp.X);
					await (s.SaveAsync(dp));
					await (t.CommitAsync());
				}
			}

			dp.Description = "description";
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = true;
					DataPoint dpManaged = await (s.GetAsync<DataPoint>(dp.Id));
					DataPoint dpMerged = (DataPoint)s.Merge(dp);
					Assert.That(dpManaged, Is.SameAs(dpMerged));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					DataPoint dpManaged = await (s.GetAsync<DataPoint>(dp.Id));
					Assert.That(dpManaged.Description, Is.Null);
					await (s.DeleteAsync(dpManaged));
					await (t.CommitAsync());
				}
			}
		}

		[Test]
		public async Task MergeWithReadOnlyProxyAsync()
		{
			DataPoint dp = null;
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					dp = new DataPoint();
					dp.X = 0.1M;
					dp.Y = (decimal)System.Math.Cos((double)dp.X);
					await (s.SaveAsync(dp));
					await (t.CommitAsync());
				}
			}

			dp.Description = "description";
			using (ISession s = OpenSession())
			{
				s.CacheMode = CacheMode.Ignore;
				using (ITransaction t = s.BeginTransaction())
				{
					s.DefaultReadOnly = true;
					DataPoint dpProxy = await (s.LoadAsync<DataPoint>(dp.Id));
					Assert.That(s.IsReadOnly(dpProxy), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(dpProxy), Is.False);
					await (s.EvictAsync(dpProxy));
					dpProxy = (DataPoint)s.Merge(dpProxy);
					Assert.That(s.IsReadOnly(dpProxy), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(dpProxy), Is.False);
					dpProxy = (DataPoint)s.Merge(dp);
					Assert.That(s.IsReadOnly(dpProxy), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(dpProxy), Is.True);
					Assert.That(dpProxy.Description, Is.EqualTo("description"));
					await (s.EvictAsync(dpProxy));
					dpProxy = (DataPoint)s.Merge(dpProxy);
					Assert.That(s.IsReadOnly(dpProxy), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(dpProxy), Is.True);
					Assert.That(dpProxy.Description, Is.EqualTo("description"));
					dpProxy.Description = null;
					dpProxy = (DataPoint)s.Merge(dp);
					Assert.That(s.IsReadOnly(dpProxy), Is.True);
					Assert.That(NHibernateUtil.IsInitialized(dpProxy), Is.True);
					Assert.That(dpProxy.Description, Is.EqualTo("description"));
					await (t.CommitAsync());
				}
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					dp = await (s.GetAsync<DataPoint>(dp.Id));
					Assert.That(dp.Description, Is.Null);
					await (s.DeleteAsync(dp));
					await (t.CommitAsync());
				}
			}
		}
	}
}
#endif
