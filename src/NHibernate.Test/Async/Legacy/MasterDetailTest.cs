#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Dialect;
using NHibernate.DomainModel;
using NHibernate.Engine;
using NHibernate.Criterion;
using NHibernate.Mapping;
using NUnit.Framework;
using Single = NHibernate.DomainModel.Single;
using System.Threading.Tasks;

namespace NHibernate.Test.Legacy
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class MasterDetailTestAsync : TestCaseAsync
	{
		protected override IList Mappings
		{
			get
			{
				return new string[]{"MasterDetail.hbm.xml", "Custom.hbm.xml", "Category.hbm.xml", "INameable.hbm.xml", "SingleSeveral.hbm.xml", "WZ.hbm.xml", "UpDown.hbm.xml", "Eye.hbm.xml", "MN.hbm.xml"};
			}
		}

		[Test]
		public async Task ParentChildrenAsync()
		{
			ISession session = OpenSession();
			M parent = new M();
			N n = new N();
			n.String = "0";
			parent.AddChildren(n);
			await (session.SaveOrUpdateAsync(parent));
			parent.RemoveChildren(n);
			await (session.FlushAsync());
			session.Close();
			session = OpenSession();
			Assert.AreEqual(0, (await (session.CreateQuery("from N").ListAsync())).Count);
			await (session.DeleteAsync("from M"));
			await (session.FlushAsync());
			session.Close();
		}

		[Test]
		public async Task OuterJoinAsync()
		{
			ISession s = OpenSession();
			Eye e = new Eye();
			e.Name = "Eye Eye";
			Jay jay = new Jay(e);
			e.Jay = jay;
			await (s.SaveOrUpdateAsync(e));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			e = (Eye)await (s.CreateCriteria(typeof (Eye)).UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(e.Jay));
			Assert.IsTrue(NHibernateUtil.IsInitialized(e.Jays));
			s.Close();
			s = OpenSession();
			jay = (Jay)await (s.CreateQuery("select new Jay(eye) from Eye eye").UniqueResultAsync());
			Assert.AreEqual("Eye Eye", jay.Eye.Name);
			await (s.DeleteAsync(jay.Eye));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public void Meta()
		{
			PersistentClass clazz = cfg.GetClassMapping(typeof (Master));
			MetaAttribute meta = clazz.GetMetaAttribute("foo");
			Assert.AreEqual("foo", meta.Value);
			meta = clazz.GetProperty("Name").GetMetaAttribute("bar");
			Assert.IsTrue(meta.IsMultiValued);
		}

		[Test]
		public async Task CopyAsync()
		{
			Category catWA = new Category();
			catWA.Name = "HSQL workaround";
			Category cat = new Category();
			cat.Name = "foo";
			Category subCatBar = new Category();
			subCatBar.Name = "bar";
			Category subCatBaz = new Category();
			subCatBaz.Name = "baz";
			cat.Subcategories.Add(subCatBar);
			cat.Subcategories.Add(subCatBaz);
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(catWA));
				await (s.SaveAsync(cat));
				await (s.FlushAsync());
			}

			cat.Name = "new foo";
			subCatBar.Name = "new bar";
			cat.Subcategories.Remove(subCatBaz);
			Category newCat = new Category();
			newCat.Name = "new";
			cat.Subcategories.Add(newCat);
			Category newSubCat = new Category();
			newSubCat.Name = "new sub";
			newCat.Subcategories.Add(newSubCat);
			Category copiedCat;
			using (ISession s = OpenSession())
			{
				copiedCat = s.Merge(cat);
				await (s.FlushAsync());
			}

			Assert.IsFalse(copiedCat == cat);
			Assert.IsTrue(cat.Subcategories.Contains(newCat));
			using (ISession s = OpenSession())
			{
				cat = (Category)await (s.CreateQuery("from Category cat where cat.Name='new foo'").UniqueResultAsync());
				newSubCat = (Category)await (s.CreateQuery("from Category cat left join fetch cat.Subcategories where cat.Name='new sub'").UniqueResultAsync());
				Assert.AreEqual("new sub", newSubCat.Name);
			}

			newSubCat.Subcategories.Add(cat);
			cat.Name = "new new foo";
			using (ISession s = OpenSession())
			{
				newSubCat = s.Merge(newSubCat);
				Assert.IsTrue(newSubCat.Name.Equals("new sub"));
				Assert.AreEqual(1, newSubCat.Subcategories.Count);
				cat = (Category)newSubCat.Subcategories[0];
				Assert.AreEqual("new new foo", cat.Name);
				newSubCat.Subcategories.Remove(cat);
				await (s.DeleteAsync(cat));
				await (s.DeleteAsync(subCatBaz));
				await (s.DeleteAsync(catWA));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task CopyCascadeAsync()
		{
			ISession s = OpenSession();
			Category child = new Category();
			child.Name = "child";
			await (s.SaveAsync(child));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			Category parent = new Category();
			parent.Name = "parent";
			parent.Subcategories.Add(child);
			child.Name = "child2";
			// Save parent and cascade update detached child
			Category persistentParent = s.Merge(parent);
			Assert.IsTrue(persistentParent.Subcategories.Count == 1);
			Assert.AreEqual(((Category)persistentParent.Subcategories[0]).Name, "child2");
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.DeleteAsync(persistentParent));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task NotNullDiscriminatorAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Up up = new Up();
			up.Id1 = "foo";
			up.Id2 = 1231;
			Down down = new Down();
			down.Id1 = "foo";
			down.Id2 = 3211;
			down.Value = 123123121;
			await (s.SaveAsync(up));
			await (s.SaveAsync(down));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IList list = await (s.CreateQuery("from Up up order by up.Id2 asc").ListAsync());
			Assert.IsTrue(list.Count == 2);
			Assert.IsFalse(list[0] is Down);
			Assert.IsTrue(list[1] is Down);
			list = await (s.CreateQuery("from Down down").ListAsync());
			Assert.IsTrue(list.Count == 1);
			Assert.IsTrue(list[0] is Down);
			await (s.DeleteAsync("from Up up"));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task SelfManyToOneAsync()
		{
			// add a check to not run if HSQLDialect
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Master m = new Master();
			m.OtherMaster = m;
			await (s.SaveAsync(m));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IEnumerator enumer = (await (s.CreateQuery("from m in class Master").EnumerableAsync())).GetEnumerator();
			enumer.MoveNext();
			m = (Master)enumer.Current;
			Assert.AreSame(m, m.OtherMaster);
			if (Dialect is MySQLDialect)
			{
				m.OtherMaster = null;
				await (s.FlushAsync());
			}

			await (s.DeleteAsync(m));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ExampleTestAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Master m = new Master();
			m.Name = "name";
			m.X = 5;
			m.OtherMaster = m;
			await (s.SaveAsync(m));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			Master m1 = (Master)await (s.CreateCriteria(typeof (Master)).Add(Example.Create(m).EnableLike().IgnoreCase()).UniqueResultAsync());
			Assert.AreSame(m1.OtherMaster, m1);
			m1 = (Master)await (s.CreateCriteria(typeof (Master)).Add(Expression.Eq("Name", "foobar")).UniqueResultAsync());
			Assert.IsNull(m1);
			m1 = (Master)await (s.CreateCriteria(typeof (Master)).Add(Example.Create(m)).CreateCriteria("OtherMaster").Add(Example.Create(m).ExcludeZeroes()).UniqueResultAsync());
			Assert.AreSame(m1.OtherMaster, m1);
			Master m2 = (Master)await (s.CreateCriteria(typeof (Master)).Add(Example.Create(m).ExcludeNone()).UniqueResultAsync());
			Assert.AreSame(m1, m2);
			m.Name = null;
			m2 = (Master)await (s.CreateCriteria(typeof (Master)).Add(Example.Create(m).ExcludeNone()).UniqueResultAsync());
			Assert.IsNull(m2);
			if (Dialect is MySQLDialect)
			{
				m1.OtherMaster = null;
				await (s.FlushAsync());
			}

			await (s.DeleteAsync(m1));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task NonLazyBidirectionalAsync()
		{
			Single sin = new Single();
			sin.Id = "asfdfds";
			sin.String = "adsa asdfasd";
			Several sev = new Several();
			sev.Id = "asdfasdfasd";
			sev.String = "asd ddd";
			sin.Several.Add(sev);
			sev.Single = sin;
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.SaveAsync(sin));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					sin = (Single)await (s.LoadAsync(typeof (Single), sin));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					sev = (Several)await (s.LoadAsync(typeof (Several), sev));
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.CreateQuery("from s in class Several").ListAsync());
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.CreateQuery("from s in class Single").ListAsync());
					await (t.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Single"));
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task CollectionQueryAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			const string path = "m.Details";
			if (Dialect.SupportsSubSelects)
			{
				await (s.CreateQuery("FROM m IN CLASS Master WHERE NOT EXISTS ( FROM " + path + " d WHERE NOT d.I=5 )").EnumerableAsync());
				await (s.CreateQuery("FROM m IN CLASS Master WHERE NOT 5 IN ( SELECT d.I FROM d IN  " + path + " )").EnumerableAsync());
			}

			await (s.CreateQuery("SELECT m FROM m in CLASS NHibernate.DomainModel.Master join m.Details d WHERE d.I=5").EnumerableAsync());
			await (s.CreateQuery("SELECT m FROM m in CLASS NHibernate.DomainModel.Master join m.Details d WHERE d.I=5").ListAsync());
			await (s.CreateQuery("SELECT m.id FROM m IN CLASS NHibernate.DomainModel.Master join m.Details d WHERE d.I=5").ListAsync());
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task MasterDetailAsync()
		{
			//if( dialect is Dialect.HSQLDialect ) return;
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Master master = new Master();
			Assert.IsNotNull(await (s.SaveAsync(master)), "save returned native id");
			object mid = s.GetIdentifier(master);
			Detail d1 = new Detail();
			d1.Master = master;
			object did = await (s.SaveAsync(d1));
			Detail d2 = new Detail();
			d2.I = 12;
			d2.Master = master;
			Assert.IsNotNull(await (s.SaveAsync(d2)), "generated id returned");
			master.AddDetail(d1);
			master.AddDetail(d2);
			if (Dialect.SupportsSubSelects)
			{
				string hql = "from d in class NHibernate.DomainModel.Detail, m in class NHibernate.DomainModel.Master " + "where m = d.Master and m.Outgoing.size = 0 and m.Incoming.size = 0";
				Assert.AreEqual(2, (await (s.CreateQuery(hql).ListAsync())).Count, "query");
			}

			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			master = (Master)await (s.LoadAsync(typeof (Master), mid));
			IEnumerator enumer = master.Details.GetEnumerator();
			int i = 0;
			while (enumer.MoveNext())
			{
				Detail d = (Detail)enumer.Current;
				Assert.AreSame(master, d.Master, "master-detail");
				i++;
			}

			Assert.AreEqual(2, i, "master-detail count");
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			Assert.AreEqual(2, (await (s.CreateQuery("select elements(master.Details) from Master master").ListAsync())).Count);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IList list = await (s.CreateQuery("from Master m left join fetch m.Details").ListAsync());
			Master m = (Master)list[0];
			Assert.IsTrue(NHibernateUtil.IsInitialized(m.Details), "joined fetch should initialize collection");
			Assert.AreEqual(2, m.Details.Count);
			list = await (s.CreateQuery("from Detail d inner join fetch d.Master").ListAsync());
			Detail dt = (Detail)list[0];
			object dtid = s.GetIdentifier(dt);
			Assert.AreSame(m, dt.Master);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			list = await (s.CreateQuery("select m from Master m1, Master m left join fetch m.Details where m.Name=m1.Name").ListAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(((Master)list[0]).Details));
			dt = (Detail)await (s.LoadAsync(typeof (Detail), dtid));
			Assert.IsTrue(((Master)list[0]).Details.Contains(dt));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			list = await (s.CreateQuery("select m, m1.Name from Master m1, Master m left join fetch m.Details where m.Name=m1.Name").ListAsync());
			Master masterFromHql = (Master)((object[])list[0])[0];
			Assert.IsTrue(NHibernateUtil.IsInitialized(masterFromHql.Details));
			dt = (Detail)await (s.LoadAsync(typeof (Detail), dtid));
			Assert.IsTrue(masterFromHql.Details.Contains(dt));
			// This line is commentend in H3.2 tests because it work in the classic parser
			// even if it as no sense ('join fetch' where the 'select' is a scalar)
			// The AST check the case with an Exception
			//list = s.CreateQuery("select m.id from Master m inner join fetch m.Details").List();
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			Detail dd = (Detail)await (s.LoadAsync(typeof (Detail), did));
			master = dd.Master;
			Assert.IsTrue(master.Details.Contains(dd), "detail-master");
			Assert.AreEqual(2, (await ((await (s.CreateFilterAsync(master.Details, "order by this.I desc"))).ListAsync())).Count);
			Assert.AreEqual(2, (await ((await (s.CreateFilterAsync(master.Details, "select this where this.id > -1"))).ListAsync())).Count);
			IQuery q = await (s.CreateFilterAsync(master.Details, "where this.id > :id"));
			q.SetInt32("id", -1);
			Assert.AreEqual(2, (await (q.ListAsync())).Count);
			q = await (s.CreateFilterAsync(master.Details, "where this.id > :id1 and this.id < :id2"));
			q.SetInt32("id1", -1);
			q.SetInt32("id2", 99999999);
			Assert.AreEqual(2, (await (q.ListAsync())).Count);
			q.SetInt32("id2", -1);
			Assert.AreEqual(0, (await (q.ListAsync())).Count);
			q = await (s.CreateFilterAsync(master.Details, "where this.id in (:ids)"));
			q.SetParameterList("ids", new[]{did, (long)-1});
			Assert.AreEqual(1, (await (q.ListAsync())).Count);
			Assert.IsTrue((await (q.EnumerableAsync())).GetEnumerator().MoveNext());
			Assert.AreEqual(2, (await ((await (s.CreateFilterAsync(master.Details, "where this.id > -1"))).ListAsync())).Count);
			Assert.AreEqual(2, (await ((await (s.CreateFilterAsync(master.Details, "select this.Master where this.id > -1"))).ListAsync())).Count);
			Assert.AreEqual(2, (await ((await (s.CreateFilterAsync(master.Details, "select m from m in class Master where this.id > -1 and this.Master=m"))).ListAsync())).Count);
			Assert.AreEqual(0, (await ((await (s.CreateFilterAsync(master.Incoming, "where this.id > -1 and this.Name is not null"))).ListAsync())).Count);
			IQuery filter = await (s.CreateFilterAsync(master.Details, "select max(this.I)"));
			enumer = (await (filter.EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.IsTrue(enumer.Current is Int32);
			filter = await (s.CreateFilterAsync(master.Details, "select max(this.I) group by this.id"));
			enumer = (await (filter.EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.IsTrue(enumer.Current is Int32);
			filter = await (s.CreateFilterAsync(master.Details, "select count(*)"));
			enumer = (await (filter.EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.IsTrue(enumer.Current is Int64);
			Assert.AreEqual(2, (await ((await (s.CreateFilterAsync(master.Details, "select this.Master"))).ListAsync())).Count);
			IQuery f = await (s.CreateFilterAsync(master.Details, "select max(this.I) where this.I < :top and this.I>=:bottom"));
			f.SetInt32("top", 100);
			f.SetInt32("bottom", 0);
			enumer = (await (f.EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.AreEqual(12, enumer.Current);
			f.SetInt32("top", 2);
			enumer = (await (f.EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.AreEqual(0, enumer.Current);
			f = await (s.CreateFilterAsync(master.Details, "select max(this.I) where this.I not in (:list)"));
			f.SetParameterList("list", new List<int>{-666, 22, 0});
			enumer = (await (f.EnumerableAsync())).GetEnumerator();
			Assert.IsTrue(enumer.MoveNext());
			Assert.AreEqual(12, enumer.Current);
			i = 0;
			foreach (Detail d in master.Details)
			{
				Assert.AreSame(master, d.Master, "master-detail");
				await (s.DeleteAsync(d));
				i++;
			}

			Assert.AreEqual(2, i, "master-detail");
			await (s.DeleteAsync(master));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task IncomingOutgoingAsync()
		{
			//if HSQLDialect skip test
			ISession s = OpenSession();
			Master master1 = new Master();
			Master master2 = new Master();
			Master master3 = new Master();
			await (s.SaveAsync(master1));
			await (s.SaveAsync(master2));
			await (s.SaveAsync(master3));
			master1.AddIncoming(master2);
			master2.AddOutgoing(master1);
			master1.AddIncoming(master3);
			master3.AddOutgoing(master1);
			object m1id = s.GetIdentifier(master1);
			Assert.AreEqual(2, (await ((await (s.CreateFilterAsync(master1.Incoming, "where this.id > 0 and this.Name is not null"))).ListAsync())).Count);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			master1 = (Master)await (s.LoadAsync(typeof (Master), m1id));
			int i = 0;
			foreach (Master m in master1.Incoming)
			{
				Assert.AreEqual(1, m.Outgoing.Count, "outgoing");
				Assert.IsTrue(m.Outgoing.Contains(master1), "outgoing");
				await (s.DeleteAsync(m));
				i++;
			}

			Assert.AreEqual(2, i, "incoming-outgoing");
			await (s.DeleteAsync(master1));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CascadingAsync()
		{
			//HSQLDialect return;
			ISession s = OpenSession();
			Detail d1 = new Detail();
			Detail d2 = new Detail();
			d2.I = 22;
			Master m = new Master();
			Master m0 = new Master();
			object m0id = await (s.SaveAsync(m0));
			m0.AddDetail(d1);
			m0.AddDetail(d2);
			d1.Master = m0;
			d2.Master = m0;
			m.MoreDetails.Add(d1);
			m.MoreDetails.Add(d2);
			object mid = await (s.SaveAsync(m));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			m = (Master)await (s.LoadAsync(typeof (Master), mid));
			Assert.AreEqual(2, m.MoreDetails.Count, "cascade save");
			IEnumerator enumer = m.MoreDetails.GetEnumerator();
			enumer.MoveNext();
			Assert.AreEqual(2, ((Detail)enumer.Current).Master.Details.Count, "cascade save");
			await (s.DeleteAsync(m));
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Master), m0id))));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task NamedQueryAsync()
		{
			ISession s = OpenSession();
			IQuery q = s.GetNamedQuery("all_details");
			await (q.ListAsync());
			s.Close();
		}

		[Test]
		public async Task SerializationAsync()
		{
			ISession s = OpenSession();
			Master m = new Master();
			Detail d1 = new Detail();
			Detail d2 = new Detail();
			object mid = await (s.SaveAsync(m));
			d1.Master = (m);
			d2.Master = (m);
			m.AddDetail(d1);
			m.AddDetail(d2);
			if (Dialect is MsSql2000Dialect)
			{
				await (s.SaveAsync(d1));
			}
			else
			{
				await (s.SaveAsync(d1, 666L));
			}

			await (s.FlushAsync());
			s.Disconnect();
			MemoryStream stream = new MemoryStream();
			BinaryFormatter f = new BinaryFormatter();
			f.Serialize(stream, s);
			stream.Position = 0;
			Console.WriteLine(stream.Length);
			s = (ISession)f.Deserialize(stream);
			stream.Close();
			s.Reconnect();
			Master m2 = (Master)await (s.LoadAsync(typeof (Master), mid));
			Assert.IsTrue(m2.Details.Count == 2, "serialized state");
			foreach (Detail d in m2.Details)
			{
				Assert.IsTrue(d.Master == m2, "deserialization");
				try
				{
					s.GetIdentifier(d);
					await (s.DeleteAsync(d));
				}
				catch (Exception)
				{
				// Why are we ignoring exceptions here? /Oskar 2014-08-24
				}
			}

			await (s.DeleteAsync(m2));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			mid = await (s.SaveAsync(new Master()));
			object mid2 = await (s.SaveAsync(new Master()));
			await (s.FlushAsync());
			s.Disconnect();
			stream = new MemoryStream();
			f.Serialize(stream, s);
			stream.Position = 0;
			s = (ISession)f.Deserialize(stream);
			stream.Close();
			s.Reconnect();
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Master), mid))));
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Master), mid2))));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			string db = s.Connection.Database; //force session to grab a connection
			try
			{
				stream = new MemoryStream();
				f.Serialize(stream, s);
			}
			catch (Exception e)
			{
				Assert.IsTrue(e is InvalidOperationException, "illegal state");
				s.Close();
				return;
			}
			finally
			{
				stream.Close();
			}

			Assert.IsTrue(false, "serialization should have failed");
		}

		[Test]
		public async Task UpdateLazyCollectionsAsync()
		{
			// if (dialect is HSQLDialect) return;
			ISession s = OpenSession();
			Master m = new Master();
			Detail d1 = new Detail();
			Detail d2 = new Detail();
			d2.X = 14;
			object mid = await (s.SaveAsync(m));
			// s.Flush();  commented out in h2.0.3 also
			d1.Master = m;
			d2.Master = m;
			m.AddDetail(d1);
			m.AddDetail(d2);
			if (Dialect is MsSql2000Dialect)
			{
				await (s.SaveAsync(d1));
				await (s.SaveAsync(d2));
			}
			else
			{
				await (s.SaveAsync(d1, 666L));
				await (s.SaveAsync(d2, 667L));
			}

			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			m = (Master)await (s.LoadAsync(typeof (Master), mid));
			s.Close();
			m.Name = "New Name";
			s = OpenSession();
			await (s.UpdateAsync(m, mid));
			IEnumerator enumer = m.Details.GetEnumerator();
			int i = 0;
			while (enumer.MoveNext())
			{
				Assert.IsNotNull(enumer.Current);
				i++;
			}

			Assert.AreEqual(2, i);
			enumer = m.Details.GetEnumerator();
			while (enumer.MoveNext())
			{
				await (s.DeleteAsync(enumer.Current));
			}

			await (s.DeleteAsync(m));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task MultiLevelCascadeAsync()
		{
			//if( dialect is Dialect.HSQLDialect ) return;
			object mid;
			object m0id;
			using (ISession s = OpenSession())
			{
				Detail detail = new Detail();
				SubDetail subdetail = new SubDetail();
				Master m = new Master();
				Master m0 = new Master();
				m0id = await (s.SaveAsync(m0));
				m0.AddDetail(detail);
				detail.Master = m0;
				m.MoreDetails.Add(detail);
				detail.SubDetails = new HashSet<SubDetail>{subdetail};
				mid = await (s.SaveAsync(m));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				Master m = (Master)await (s.LoadAsync(typeof (Master), mid));
				IEnumerator enumer = m.MoreDetails.GetEnumerator();
				enumer.MoveNext();
				Assert.IsTrue(((Detail)enumer.Current).SubDetails.Count != 0);
				await (s.DeleteAsync(m));
				await (s.DeleteAsync(await (s.LoadAsync(typeof (Master), m0id))));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task MixNativeAssignedAsync()
		{
			// if HSQLDialect then skip test
			ISession s = OpenSession();
			Category c = new Category();
			c.Name = "NAME";
			Assignable assn = new Assignable();
			assn.Id = "i.d.";
			assn.Categories = new List<Category>{c};
			c.Assignable = assn;
			await (s.SaveAsync(assn));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			await (s.DeleteAsync(assn));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionReplaceOnUpdateAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Category c = new Category();
			IList<Category> list = new List<Category>();
			c.Subcategories = list;
			list.Add(new Category());
			await (s.SaveAsync(c));
			await (t.CommitAsync());
			s.Close();
			c.Subcategories = list;
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(c));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Category)await (s.LoadAsync(typeof (Category), c.Id, LockMode.Upgrade));
			IList<Category> list2 = c.Subcategories;
			await (t.CommitAsync());
			s.Close();
			Assert.IsFalse(NHibernateUtil.IsInitialized(c.Subcategories));
			c.Subcategories = list2;
			s = OpenSession();
			t = s.BeginTransaction();
			await (s.UpdateAsync(c));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Category)await (s.LoadAsync(typeof (Category), c.Id, LockMode.Upgrade));
			Assert.AreEqual(1, c.Subcategories.Count);
			await (s.DeleteAsync(c));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionReplace2Async()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Category c = new Category();
			IList<Category> list = new List<Category>();
			c.Subcategories = list;
			list.Add(new Category());
			Category c2 = new Category();
			await (s.SaveAsync(c2));
			await (s.SaveAsync(c));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Category)await (s.LoadAsync(typeof (Category), c.Id, LockMode.Upgrade));
			IList<Category> list2 = c.Subcategories;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c2 = (Category)await (s.LoadAsync(typeof (Category), c2.Id, LockMode.Upgrade));
			c2.Subcategories = list2;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c2 = (Category)await (s.LoadAsync(typeof (Category), c2.Id, LockMode.Upgrade));
			Assert.AreEqual(1, c2.Subcategories.Count);
			await (s.DeleteAsync(c2));
			await (s.DeleteAsync(await (s.LoadAsync(typeof (Category), c.Id))));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CollectionReplaceAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Category c = new Category();
			IList<Category> list = new List<Category>();
			c.Subcategories = list;
			list.Add(new Category());
			await (s.SaveAsync(c));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Category)await (s.LoadAsync(typeof (Category), c.Id, LockMode.Upgrade));
			c.Subcategories = list;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Category)await (s.LoadAsync(typeof (Category), c.Id, LockMode.Upgrade));
			IList<Category> list2 = c.Subcategories;
			await (t.CommitAsync());
			s.Close();
			Assert.IsFalse(NHibernateUtil.IsInitialized(c.Subcategories));
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Category)await (s.LoadAsync(typeof (Category), c.Id, LockMode.Upgrade));
			c.Subcategories = list2;
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			c = (Category)await (s.LoadAsync(typeof (Category), c.Id, LockMode.Upgrade));
			Assert.AreEqual(1, c.Subcategories.Count);
			await (s.DeleteAsync(c));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task CategoriesAsync()
		{
			Category c = new Category();
			c.Name = Category.RootCategory;
			Category c1 = new Category();
			Category c2 = new Category();
			Category c3 = new Category();
			c.Subcategories.Add(c1);
			c.Subcategories.Add(c2);
			c2.Subcategories.Add(null);
			c2.Subcategories.Add(c3);
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(c));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				c = (Category)await (s.LoadAsync(typeof (Category), c.Id));
				Assert.IsNotNull(c.Subcategories[0]);
				Assert.IsNotNull(c.Subcategories[1]);
				IList<Category> list = ((Category)c.Subcategories[1]).Subcategories;
				Assert.IsNull(list[0]);
				Assert.IsNotNull(list[1]);
				IEnumerator enumer = (await (s.CreateQuery("from c in class Category where c.Name = NHibernate.DomainModel.Category.RootCategory").EnumerableAsync())).GetEnumerator();
				Assert.IsTrue(enumer.MoveNext());
			}

			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync("from Category"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task CollectionRefreshAsync()
		{
			ISession s = OpenSession();
			Category c = new Category();
			IList<Category> list = new List<Category>();
			c.Subcategories = list;
			list.Add(new Category());
			c.Name = "root";
			object id = await (s.SaveAsync(c));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			c = (Category)await (s.LoadAsync(typeof (Category), id));
			await (s.RefreshAsync(c));
			await (s.FlushAsync());
			Assert.AreEqual(1, c.Subcategories.Count);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			c = (Category)await (s.LoadAsync(typeof (Category), id));
			Assert.AreEqual(1, c.Subcategories.Count);
			// modify the collection in another session
			ISession s2 = OpenSession();
			Category c2 = (Category)await (s2.LoadAsync(typeof (Category), id));
			c2.Subcategories.Add(new Category());
			await (s2.FlushAsync());
			s2.Close();
			// now lets refresh the collection and see if it picks up 
			// the new objects
			await (s.RefreshAsync(c));
			Assert.AreEqual(2, c.Subcategories.Count, "should have gotten the addition from s2");
			await (s.DeleteAsync(c));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CachedCollectionRefreshAsync()
		{
			ISession s = OpenSession();
			Category c = new Category();
			IList<Category> list = new List<Category>();
			c.Subcategories = list;
			list.Add(new Category());
			c.Name = "root";
			object id = await (s.SaveAsync(c));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			c = (Category)await (s.LoadAsync(typeof (Category), id));
			await (NHibernateUtil.InitializeAsync(c.Subcategories));
			ISession ss = OpenSession();
			Category c2 = (Category)await (ss.LoadAsync(typeof (Category), id));
			await (ss.DeleteAsync(c2.Subcategories[0]));
			c2.Subcategories.Clear();
			await (ss.FlushAsync());
			ss.Close();
			await (s.RefreshAsync(c));
			Assert.AreEqual(0, c.Subcategories.Count);
			ss = OpenSession();
			c2 = (Category)await (ss.LoadAsync(typeof (Category), id));
			c2.Subcategories.Add(new Category());
			c2.Subcategories.Add(new Category());
			await (ss.FlushAsync());
			ss.Close();
			await (s.RefreshAsync(c));
			Assert.AreEqual(2, c.Subcategories.Count);
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			c = (Category)await (s.LoadAsync(typeof (Category), id));
			Assert.AreEqual(2, c.Subcategories.Count);
			await (s.DeleteAsync(c));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task CustomPersisterAsync()
		{
			ISession s = OpenSession();
			Custom c = new Custom();
			c.Name = "foo";
			c.Id = "100";
			string id = (string)await (s.SaveAsync(c));
			Assert.AreSame(c, await (s.LoadAsync(typeof (Custom), id)));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			c = (Custom)await (s.LoadAsync(typeof (Custom), id));
			Assert.AreEqual("foo", c.Name);
			c.Name = "bar";
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			c = (Custom)await (s.LoadAsync(typeof (Custom), id));
			Assert.AreEqual("bar", c.Name);
			await (s.DeleteAsync(c));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			bool none = false;
			try
			{
				await (s.LoadAsync(typeof (Custom), id));
			}
			catch (ObjectNotFoundException onfe)
			{
				Assert.IsNotNull(onfe); //getting ride of 'onfe' is never used compile warning
				none = true;
			}

			Assert.IsTrue(none);
			s.Close();
		}

		[Test]
		public async Task InterfaceAsync()
		{
			ISession s = OpenSession();
			object id = await (s.SaveAsync(new BasicNameable()));
			await (s.FlushAsync());
			s.Close();
			s = OpenSession();
			INameable n = (INameable)await (s.LoadAsync(typeof (INameable), id));
			await (s.DeleteAsync(n));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task NoUpdatedManyToOneAsync()
		{
			W w1 = new W();
			W w2 = new W();
			Z z = new Z();
			z.W = w1;
			using (ISession s = OpenSession())
			{
				await (s.SaveAsync(z));
				await (s.FlushAsync());
				z.W = w2;
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				await (s.UpdateAsync(z));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				await (s.DeleteAsync(z));
				await (s.DeleteAsync(w1));
				await (s.DeleteAsync(w2));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task QueuedBagAddsAsync()
		{
			ISession s = OpenSession();
			Assignable a = new Assignable();
			a.Id = "foo";
			a.Categories = new List<Category>();
			Category c = new Category();
			c.Assignable = a;
			a.Categories.Add(c);
			await (s.SaveAsync(a));
			await (s.FlushAsync());
			s.Close();
			sessions.EvictCollection("NHibernate.DomainModel.Assignable.Categories");
			s = OpenSession();
			a = (Assignable)await (s.GetAsync(typeof (Assignable), "foo"));
			c = new Category();
			c.Assignable = a;
			a.Categories.Add(c);
			Assert.IsFalse(NHibernateUtil.IsInitialized(a.Categories));
			Assert.AreEqual(2, a.Categories.Count);
			await (s.FlushAsync());
			s.Close();
			sessions.EvictCollection("NHibernate.DomainModel.Assignable.Categories");
			s = OpenSession();
			a = (Assignable)await (s.GetAsync(typeof (Assignable), "foo"));
			c = new Category();
			c.Assignable = a;
			a.Categories.Add(c);
			Assert.IsFalse(NHibernateUtil.IsInitialized(a.Categories));
			await (s.FlushAsync());
			Assert.IsFalse(NHibernateUtil.IsInitialized(a.Categories));
			Assert.AreEqual(3, a.Categories.Count);
			s.Close();
			sessions.EvictCollection("NHibernate.DomainModel.Assignable.Categories");
			s = OpenSession();
			a = (Assignable)await (s.GetAsync(typeof (Assignable), "foo"));
			Assert.AreEqual(3, a.Categories.Count);
			await (s.DeleteAsync(a));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task PolymorphicCriteriaAsync()
		{
			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();
			Category f = new Category();
			Single b = new Single();
			b.Id = "asdfa";
			b.String = "asdfasdf";
			await (s.SaveAsync(f));
			await (s.SaveAsync(b));
			IList list = await (s.CreateCriteria(typeof (object)).ListAsync());
			Assert.AreEqual(2, list.Count);
			Assert.IsTrue(list.Contains(f));
			Assert.IsTrue(list.Contains(b));
			await (s.DeleteAsync(f));
			await (s.DeleteAsync(b));
			await (txn.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task ToStringWithNoIdentifierAsync()
		{
			await (NHibernateUtil.Entity(typeof (Master)).ToLoggableStringAsync(new Master(), (ISessionFactoryImplementor)sessions));
		}

		[Test]
		public async Task NH741Async()
		{
			using (ISession s = OpenSession())
			{
				await (s.CreateQuery("select m from Master m order by m.id + :param").SetParameter("param", 10, NHibernateUtil.Int32).ListAsync());
			}
		}
	}
}
#endif
