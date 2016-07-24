#if NET_4_5
using System;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2470
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Class1_Class2_TestsAsync : BugTestCaseAsync
	{
		public async Task CleanAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					await (s.DeleteAsync("from Class1"));
					await (s.DeleteAsync("from Class2"));
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task Test0Async()
		{
			Class1 c1;
			Class2 c2;
			Class1DTO class1dto;
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					c1 = new Class1();
					c2 = new Class2();
					c1.AddClass2(c2);
					await (s.SaveAsync(c2));
					await (s.SaveAsync(c1));
					class1dto = new Class1DTO{ID = c1.ID, EntityVersion = c1.EntityVersion};
					class1dto.Class2Ary = c1.Class2List.Select(cl2 => new Class2DTO{ID = cl2.ID, EntityVersion = cl2.EntityVersion}).ToArray();
					await (tx.CommitAsync());
				}

				Assert.AreEqual(1, c1.EntityVersion);
				Assert.AreEqual(1, c2.EntityVersion);
				Assert.AreEqual(1, class1dto.EntityVersion);
			}

			Assert.AreEqual(1, c1.EntityVersion);
			Assert.AreEqual(1, c2.EntityVersion);
			Assert.AreEqual(1, class1dto.EntityVersion);
			await (CleanAsync());
		}

		[Test]
		public async Task Test1Async()
		{
			Class1 c1;
			Class2 c2;
			Class1DTO dto;
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					c1 = new Class1();
					c2 = new Class2();
					c1.AddClass2(c2);
					await (s.SaveAsync(c2));
					await (s.SaveAsync(c1));
					await (tx.CommitAsync());
				}

				using (ITransaction tx = s.BeginTransaction())
				{
					// NH-2470 If inverse="true" in Class1.hbm.xml you must add the next line to prevent erroneous version increment
					// s.Refresh(c1, LockMode.None);
					dto = new Class1DTO{ID = c1.ID, EntityVersion = c1.EntityVersion};
					await (tx.CommitAsync());
				}

				Assert.AreEqual(1, c1.EntityVersion);
				Assert.AreEqual(1, c2.EntityVersion);
				Assert.AreEqual(1, dto.EntityVersion);
			}

			Assert.AreEqual(1, c1.EntityVersion);
			Assert.AreEqual(1, c2.EntityVersion);
			Assert.AreEqual(1, dto.EntityVersion);
			await (CleanAsync());
		}

		[Test]
		public async Task Test2Async()
		{
			Class1 c1;
			Class2 c2;
			Class1DTO dto;
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					c1 = new Class1();
					c2 = new Class2();
					c1.AddClass2(c2);
					await (s.SaveAsync(c2));
					await (s.SaveAsync(c1));
					dto = new Class1DTO{ID = c1.ID, EntityVersion = c1.EntityVersion};
					await (tx.CommitAsync());
				}

				Assert.AreEqual(1, c1.EntityVersion);
				Assert.AreEqual(1, c2.EntityVersion);
				Assert.AreEqual(c1.ID, dto.ID);
				Assert.AreEqual(1, dto.EntityVersion);
			}

			Assert.AreEqual(1, c1.EntityVersion);
			Assert.AreEqual(1, c2.EntityVersion);
			Assert.AreEqual(c1.ID, dto.ID);
			Assert.AreEqual(1, dto.EntityVersion);
			await (CleanAsync());
		}

		[Test]
		public async Task Test3Async()
		{
			Class1 c1;
			Class2 c2;
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					c1 = new Class1();
					c2 = new Class2();
					c1.AddClass2(c2);
					await (s.SaveAsync(c2));
					await (s.SaveAsync(c1));
					await (tx.CommitAsync());
				}

				Assert.AreEqual(1, c1.EntityVersion);
				Assert.AreEqual(1, c2.EntityVersion);
				using (ITransaction tx = s.BeginTransaction())
				{
					// NH-2470 If inverse="true" in Class1.hbm.xml you must add the next line to prevent erroneous version increment
					// s.Refresh(c1, LockMode.None);
					var class1dto = new Class1DTO{ID = c1.ID, EntityVersion = c1.EntityVersion};
					class1dto.Class2Ary = c1.Class2List.Select(cl2 => new Class2DTO{ID = cl2.ID, EntityVersion = cl2.EntityVersion}).ToArray();
					await (tx.CommitAsync());
				}

				Assert.AreEqual(1, c1.EntityVersion);
				Assert.AreEqual(1, c2.EntityVersion);
			}

			Assert.AreEqual(1, c1.EntityVersion);
			Assert.AreEqual(1, c2.EntityVersion);
			await (CleanAsync());
		}

		[Test]
		public async Task Test4Async()
		{
			Guid c1id;
			int c1ev, c2ev;
			Class1DTO class1dto;
			using (ISession s = OpenSession())
			{
				Class1 c1;
				using (ITransaction tx = s.BeginTransaction())
				{
					Class2 c2;
					c1 = new Class1();
					c2 = new Class2();
					c1.AddClass2(c2);
					await (s.SaveAsync(c2));
					await (s.SaveAsync(c1));
					await (tx.CommitAsync());
					c1id = c1.ID;
					c1ev = c1.EntityVersion;
					c2ev = c2.EntityVersion;
				}

				Assert.AreEqual(1, c1ev);
				Assert.AreEqual(1, c2ev);
				using (ITransaction tx = s.BeginTransaction())
				{
					// NH-2470 If inverse="true" in Class1.hbm.xml you must add the next line to prevent erroneous version increment
					// s.Refresh(c1);
					class1dto = new Class1DTO{ID = c1.ID, EntityVersion = c1.EntityVersion};
					class1dto.Class2Ary = c1.Class2List.Select(cl2 => new Class2DTO{ID = cl2.ID, EntityVersion = cl2.EntityVersion}).ToArray();
					await (tx.CommitAsync());
					c1ev = c1.EntityVersion;
					Assert.AreEqual(1, c1ev);
				}

				Assert.AreEqual(1, c1.EntityVersion);
			}

			Assert.AreEqual(1, c1ev);
			Assert.AreEqual(1, class1dto.EntityVersion);
			Assert.IsTrue(class1dto.Class2Ary.All(cl2 => cl2.EntityVersion == 1));
			Class1 cl1;
			using (ISession s = OpenSession())
			{
				cl1 = await (s.GetAsync<Class1>(c1id));
			}

			Assert.AreEqual(1, cl1.EntityVersion);
			await (CleanAsync());
		}

		[Test]
		public async Task Test5Async()
		{
			Guid c1id;
			using (ISession s = OpenSession())
			{
				Class1 c1;
				Class2 c2;
				using (ITransaction tx = s.BeginTransaction())
				{
					c1 = new Class1();
					c2 = new Class2();
					c1.AddClass2(c2);
					await (s.SaveAsync(c2));
					await (s.SaveAsync(c1));
					await (tx.CommitAsync());
				}

				Assert.AreEqual(1, c1.EntityVersion);
				Assert.AreEqual(1, c2.EntityVersion);
				c1id = c1.ID;
			}

			// NH-2470 Since we're using a totally separate session, this test always passes.
			Class1 cl1;
			Class1DTO class1dto;
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					cl1 = await (s.GetAsync<Class1>(c1id));
					class1dto = new Class1DTO{ID = cl1.ID, EntityVersion = cl1.EntityVersion};
					class1dto.Class2Ary = cl1.Class2List.Select(cl2 => new Class2DTO{ID = cl2.ID, EntityVersion = cl2.EntityVersion}).ToArray();
					await (tx.CommitAsync());
				}

				Assert.AreEqual(1, cl1.EntityVersion);
			}

			Assert.AreEqual(1, cl1.EntityVersion);
			Assert.IsTrue(class1dto.Class2Ary.All(cl2 => cl2.EntityVersion == 1));
			await (CleanAsync());
		}
	}
}
#endif
