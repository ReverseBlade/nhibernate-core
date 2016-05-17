#if NET_4_5
using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.Unionsubclass
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class UnionSubclassFixture : TestCase
	{
		[Test]
		public async Task UnionSubclassCollectionAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					Location mel = new Location("Earth");
					await (s.SaveAsync(mel));
					Human gavin = new Human();
					gavin.Identity = "gavin";
					gavin.Sex = 'M';
					gavin.Location = mel;
					mel.AddBeing(gavin);
					gavin.Info.Add("bar");
					gavin.Info.Add("y");
					await (t.CommitAsync());
				}

				s.Close();
			}

			using (ISession s = OpenSession())
			{
				using (ITransaction t = s.BeginTransaction())
				{
					Human gavin = (Human)await (s.CreateCriteria(typeof (Human)).UniqueResultAsync());
					Assert.AreEqual(gavin.Info.Count, 2);
					await (s.DeleteAsync(gavin));
					await (s.DeleteAsync(gavin.Location));
					await (t.CommitAsync());
				}

				s.Close();
			}
		}

		[Test]
		public async Task UnionSubclassFetchModeAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Location mel = new Location("Earth");
			await (s.SaveAsync(mel));
			Human gavin = new Human();
			gavin.Identity = "gavin";
			gavin.Sex = 'M';
			gavin.Location = mel;
			mel.AddBeing(gavin);
			Human max = new Human();
			max.Identity = "max";
			max.Sex = 'M';
			max.Location = mel;
			mel.AddBeing(gavin);
			await (s.FlushAsync());
			s.Clear();
			IList list = s.CreateCriteria(typeof (Human)).SetFetchMode("location", FetchMode.Join).SetFetchMode("location.beings", FetchMode.Join).List();
			for (int i = 0; i < list.Count; i++)
			{
				Human h = (Human)list[i];
				Assert.IsTrue(NHibernateUtil.IsInitialized(h.Location));
				Assert.IsTrue(NHibernateUtil.IsInitialized(h.Location.Beings));
				await (s.DeleteAsync(h));
			}

			await (s.DeleteAsync(await (s.GetAsync<Location>(mel.Id))));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task UnionSubclassOneToManyAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Location mel = new Location("Melbourne, Australia");
			Location mars = new Location("Mars");
			await (s.SaveAsync(mel));
			await (s.SaveAsync(mars));
			Human gavin = new Human();
			gavin.Identity = "gavin";
			gavin.Sex = 'M';
			gavin.Location = mel;
			mel.AddBeing(gavin);
			Alien x23y4 = new Alien();
			x23y4.Identity = "x23y4$$hu%3";
			x23y4.Location = mars;
			x23y4.Species = "martian";
			mars.AddBeing(x23y4);
			Alien yy3dk = new Alien();
			yy3dk.Identity = "yy3dk&*!!!";
			yy3dk.Location = mars;
			yy3dk.Species = "martian";
			mars.AddBeing(yy3dk);
			Hive hive = new Hive();
			hive.Location = mars;
			hive.Members.Add(x23y4);
			x23y4.Hive = hive;
			hive.Members.Add(yy3dk);
			yy3dk.Hive = hive;
			await (s.PersistAsync(hive));
			yy3dk.Hivemates.Add(x23y4);
			x23y4.Hivemates.Add(yy3dk);
			await (s.FlushAsync());
			s.Clear();
			hive = (Hive)await (s.CreateQuery("from Hive h").UniqueResultAsync());
			Assert.IsFalse(NHibernateUtil.IsInitialized(hive.Members));
			Assert.AreEqual(2, hive.Members.Count);
			s.Clear();
			hive = (Hive)await (s.CreateQuery("from Hive h left join fetch h.location left join fetch h.members").UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(hive.Members));
			Assert.AreEqual(2, hive.Members.Count);
			s.Clear();
			x23y4 = (Alien)await (s.CreateQuery("from Alien a left join fetch a.hivemates where a.identity like 'x%'").UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(x23y4.Hivemates));
			Assert.AreEqual(1, x23y4.Hivemates.Count);
			s.Clear();
			x23y4 = (Alien)await (s.CreateQuery("from Alien a where a.identity like 'x%'").UniqueResultAsync());
			Assert.IsFalse(NHibernateUtil.IsInitialized(x23y4.Hivemates));
			Assert.AreEqual(1, x23y4.Hivemates.Count);
			s.Clear();
			x23y4 = (Alien)s.CreateCriteria(typeof (Alien)).AddOrder(Order.Asc("identity")).List()[0];
			await (s.DeleteAsync(x23y4.Hive));
			await (s.DeleteAsync(await (s.GetAsync<Location>(mel.Id))));
			await (s.DeleteAsync(await (s.GetAsync<Location>(mars.Id))));
			Assert.IsTrue(s.CreateQuery("from Being").List().Count == 0);
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task UnionSubclassManyToOneAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			Location mel = new Location("Melbourne, Australia");
			Location mars = new Location("Mars");
			await (s.SaveAsync(mel));
			await (s.SaveAsync(mars));
			Human gavin = new Human();
			gavin.Identity = "gavin";
			gavin.Sex = 'M';
			gavin.Location = mel;
			mel.AddBeing(gavin);
			Alien x23y4 = new Alien();
			x23y4.Identity = "x23y4$$hu%3";
			x23y4.Location = mars;
			x23y4.Species = "martian";
			mars.AddBeing(x23y4);
			Hive hive = new Hive();
			hive.Location = mars;
			hive.Members.Add(x23y4);
			x23y4.Hive = hive;
			await (s.PersistAsync(hive));
			Thing thing = new Thing();
			thing.Description = "some thing";
			thing.Owner = gavin;
			gavin.Things.Add(thing);
			await (s.SaveAsync(thing));
			await (s.FlushAsync());
			s.Clear();
			thing = (Thing)await (s.CreateQuery("from Thing t left join fetch t.owner").UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(thing.Owner));
			Assert.AreEqual("gavin", thing.Owner.Identity);
			s.Clear();
			thing = (Thing)await (s.CreateQuery("select t from Thing t left join t.owner where t.owner.identity='gavin'").UniqueResultAsync());
			Assert.IsFalse(NHibernateUtil.IsInitialized(thing.Owner));
			Assert.AreEqual("gavin", thing.Owner.Identity);
			s.Clear();
			gavin = (Human)await (s.CreateQuery("from Human h left join fetch h.things").UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(gavin.Things));
			Assert.AreEqual("some thing", ((Thing)gavin.Things[0]).Description);
			s.Clear();
			Assert.AreEqual(2, s.CreateQuery("from Being b left join fetch b.things").List().Count);
			s.Clear();
			gavin = (Human)await (s.CreateQuery("from Being b join fetch b.things").UniqueResultAsync());
			Assert.IsTrue(NHibernateUtil.IsInitialized(gavin.Things));
			Assert.AreEqual("some thing", ((Thing)gavin.Things[0]).Description);
			s.Clear();
			gavin = (Human)await (s.CreateQuery("select h from Human h join h.things t where t.description='some thing'").UniqueResultAsync());
			Assert.IsFalse(NHibernateUtil.IsInitialized(gavin.Things));
			Assert.AreEqual("some thing", ((Thing)gavin.Things[0]).Description);
			s.Clear();
			gavin = (Human)await (s.CreateQuery("select b from Being b join b.things t where t.description='some thing'").UniqueResultAsync());
			Assert.IsFalse(NHibernateUtil.IsInitialized(gavin.Things));
			Assert.AreEqual("some thing", ((Thing)gavin.Things[0]).Description);
			s.Clear();
			thing = await (s.GetAsync<Thing>(thing.Id));
			Assert.IsFalse(NHibernateUtil.IsInitialized(thing.Owner));
			Assert.AreEqual("gavin", thing.Owner.Identity);
			thing.Owner.Things.Remove(thing);
			thing.Owner = x23y4;
			x23y4.Things.Add(thing);
			await (s.FlushAsync());
			s.Clear();
			thing = await (s.GetAsync<Thing>(thing.Id));
			Assert.IsFalse(NHibernateUtil.IsInitialized(thing.Owner));
			Assert.AreEqual("x23y4$$hu%3", thing.Owner.Identity);
			await (s.DeleteAsync(thing));
			x23y4 = (Alien)await (s.CreateCriteria(typeof (Alien)).UniqueResultAsync());
			await (s.DeleteAsync(x23y4.Hive));
			await (s.DeleteAsync(await (s.GetAsync<Location>(mel.Id))));
			await (s.DeleteAsync(await (s.GetAsync<Location>(mars.Id))));
			Assert.AreEqual(0, s.CreateQuery("from Being").List().Count);
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task UnionSubclassAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					Location mel = new Location("Melbourne, Australia");
					Location atl = new Location("Atlanta, GA");
					Location mars = new Location("Mars");
					await (s.SaveAsync(mel));
					await (s.SaveAsync(atl));
					await (s.SaveAsync(mars));
					Human gavin = new Human();
					gavin.Identity = "gavin";
					gavin.Sex = 'M';
					gavin.Location = mel;
					mel.AddBeing(gavin);
					Alien x23y4 = new Alien();
					x23y4.Identity = "x23y4$$hu%3";
					x23y4.Location = mars;
					x23y4.Species = "martian";
					mars.AddBeing(x23y4);
					Hive hive = new Hive();
					hive.Location = mars;
					hive.Members.Add(x23y4);
					x23y4.Hive = hive;
					await (s.PersistAsync(hive));
					Assert.AreEqual(2, s.CreateQuery("from Being").List().Count);
					Assert.AreEqual(1, s.CreateQuery("from Being b where b.class = Alien").List().Count);
					Assert.AreEqual(1, s.CreateQuery("from Alien").List().Count);
					s.Clear();
					IList<Being> beings = s.CreateQuery("from Being b left join fetch b.location").List<Being>();
					foreach (Being b in beings)
					{
						Assert.IsTrue(NHibernateUtil.IsInitialized(b.Location));
						Assert.IsNotNull(b.Location.Name);
						Assert.IsNotNull(b.Identity);
						Assert.IsNotNull(b.Species);
					}

					Assert.AreEqual(2, beings.Count);
					s.Clear();
					beings = s.CreateQuery("from Being").List<Being>();
					foreach (Being b in beings)
					{
						Assert.IsFalse(NHibernateUtil.IsInitialized(b.Location));
						Assert.IsNotNull(b.Location.Name);
						Assert.IsNotNull(b.Identity);
						Assert.IsNotNull(b.Species);
					}

					Assert.AreEqual(2, beings.Count);
					s.Clear();
					IList<Location> locations = s.CreateQuery("from Location").List<Location>();
					int count = 0;
					foreach (Location l in locations)
					{
						Assert.IsNotNull(l.Name);
						foreach (Being o in l.Beings)
						{
							count++;
							Assert.AreSame(o.Location, l);
						}
					}

					Assert.AreEqual(2, count);
					Assert.AreEqual(3, locations.Count);
					s.Clear();
					locations = s.CreateQuery("from Location loc left join fetch loc.beings").List<Location>();
					count = 0;
					foreach (Location l in locations)
					{
						Assert.IsNotNull(l.Name);
						foreach (Being o in l.Beings)
						{
							count++;
							Assert.AreSame(o.Location, l);
						}
					}

					Assert.AreEqual(2, count);
					Assert.AreEqual(3, locations.Count);
					s.Clear();
					gavin = await (s.GetAsync<Human>(gavin.Id));
					atl = await (s.GetAsync<Location>(atl.Id));
					atl.AddBeing(gavin);
					Assert.AreEqual(1, s.CreateQuery("from Human h where h.location.name like '%GA'").List().Count);
					await (s.DeleteAsync(gavin));
					x23y4 = (Alien)await (s.CreateCriteria(typeof (Alien)).UniqueResultAsync());
					await (s.DeleteAsync(x23y4.Hive));
					Assert.AreEqual(0, s.CreateQuery("from Being").List().Count);
					await (t.CommitAsync());
					s.Close();
				}

			using (ISession s = OpenSession())
				using (ITransaction t = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Location"));
					await (t.CommitAsync());
				}
		}

		[Test]
		public async Task NestedUnionedSubclassesAsync()
		{
			ISession s;
			ITransaction tx;
			s = OpenSession();
			tx = s.BeginTransaction();
			Location mel = new Location("Earth");
			Human marcf = new Human();
			marcf.Identity = "marc";
			marcf.Sex = 'M';
			mel.AddBeing(marcf);
			Employee steve = new Employee();
			steve.Identity = "steve";
			steve.Sex = 'M';
			steve.Salary = 0;
			mel.AddBeing(steve);
			await (s.PersistAsync(mel));
			await (tx.CommitAsync());
			s.Close();
			s = OpenSession();
			tx = s.BeginTransaction();
			IQuery q = s.CreateQuery("from Being h where h.identity = :name1 or h.identity = :name2");
			q.SetString("name1", "marc");
			q.SetString("name2", "steve");
			IList result = q.List();
			Assert.AreEqual(2, result.Count);
			await (s.DeleteAsync(result[0]));
			await (s.DeleteAsync(result[1]));
			await (s.DeleteAsync(((Human)result[0]).Location));
			await (tx.CommitAsync());
			s.Close();
		}
	}
}
#endif
