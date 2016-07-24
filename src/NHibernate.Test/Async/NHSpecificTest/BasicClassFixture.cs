#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.DomainModel.NHSpecific;
using NHibernate.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using NHibernate.Util;

namespace NHibernate.Test.NHSpecificTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class BasicClassFixtureAsync : TestCaseAsync
	{
		protected override IList Mappings
		{
			get
			{
				return new string[]{"NHSpecific.BasicClass.hbm.xml"};
			}
		}

		/// <summary>
		/// This is a test for <a href = "http://nhibernate.jira.com/browse/NH-134">NH-134</a>.
		/// </summary>
		/// <remarks>
		/// It checks to make sure that NHibernate can use the correct accessor to get the
		/// type="" attribute through reflection.
		/// </remarks>
		[Test]
		public async Task TestPrivateFieldAccessAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					BasicClass bc = new BasicClass();
					if (!TestDialect.SupportsNullCharactersInUtfStrings)
						bc.CharacterProperty = 'a';
					bc.Id = 1;
					bc.ValueOfPrivateField = 5;
					await (s.SaveAsync(bc));
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					BasicClass bc = (BasicClass)await (s.LoadAsync(typeof (BasicClass), 1));
					Assert.AreEqual(5, bc.ValueOfPrivateField, "private field accessor");
					await (s.DeleteAsync(bc));
					await (tx.CommitAsync());
				}
		}

		[Test]
		public async Task CachingAsync()
		{
			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					BasicClass bc = new BasicClass();
					if (!TestDialect.SupportsNullCharactersInUtfStrings)
						bc.CharacterProperty = 'a';
					bc.Id = 1;
					await (s.SaveAsync(bc));
					await (tx.CommitAsync());
				}

			using (ISession s = OpenSession())
				using (ITransaction tx = s.BeginTransaction())
				{
					BasicClass bc = (BasicClass)await (s.LoadAsync(typeof (BasicClass), 1));
					await (s.DeleteAsync(bc));
					await (tx.CommitAsync());
				}

			{
				int maxIndex = 2;
				ISession[] s = new ISession[maxIndex];
				ITransaction[] t = new ITransaction[maxIndex];
				BasicClass[] bc = new BasicClass[maxIndex];
				int index = 0;
				int id = 1;
				bc[index] = await (InsertBasicClassAsync(id));
				index++;
				// make sure the previous insert went through
				s[index] = OpenSession();
				t[index] = s[index].BeginTransaction();
				bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
				Assert.IsNotNull(bc[index]);
				AssertPropertiesEqual(bc[index - 1], bc[index]);
				// VERIFY DELETE
				await (s[index].DeleteAsync(bc[index]));
				await (t[index].CommitAsync());
				s[index].Close();
				await (AssertDeleteAsync(id));
			}
		}

		[Test]
		public async Task TestCRUDAsync()
		{
			int maxIndex = 22;
			ISession[] s = new ISession[maxIndex];
			ITransaction[] t = new ITransaction[maxIndex];
			BasicClass[] bc = new BasicClass[maxIndex];
			int index = 0;
			int id = 1;
			bc[index] = await (InsertBasicClassAsync(id));
			index++;
			// make sure the previous insert went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			Assert.IsNotNull(bc[index]);
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].Int32Array[1] = 15;
			bc[index].StringBag[0] = "Replaced Spot 0";
			bc[index].StringArray[2] = "Replaced Spot 2";
			bc[index].StringList[0] = "Replaced Spot 0";
			bc[index].StringMap["keyZero"] = "Replaced Key 0";
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].CharacterProperty = 'b';
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// update a property to make sure it picks up that it is dirty
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].ClassProperty = typeof (String);
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// 12 = french
			bc[index].CultureInfoProperty = new CultureInfo(12);
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].DateTimeProperty = DateTime.Parse("2004-02-15 08:00:00 PM");
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].Int16Property = Int16.MinValue;
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].Int32Property = Int32.MinValue;
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].Int64Property = Int64.MinValue;
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].SingleProperty = bc[index].SingleProperty * -1;
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].StringProperty = "new string property";
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].TicksProperty = DateTime.Now;
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].TrueFalseProperty = false;
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// make sure the previous updates went through
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].YesNoProperty = false;
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// VERIFY PREVIOUS UPDATE & PERFORM DELETE
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// test the delete method
			await (s[index].DeleteAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			await (AssertDeleteAsync(id));
		}

		[Test]
		public async Task TestArrayCRUDAsync()
		{
			int maxIndex = 4;
			ISession[] s = new ISession[maxIndex];
			ITransaction[] t = new ITransaction[maxIndex];
			BasicClass[] bc = new BasicClass[maxIndex];
			int index = 0;
			int id = 1;
			bc[index] = await (InsertBasicClassAsync(id));
			index++;
			// modify the array so it is updated - should not be recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].StringArray[0] = "modified string 0";
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// change the array to a new array so it is recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].StringArray = new string[]{"string one", "string two"};
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// VERIFY PREVIOUS UPDATE & PERFORM DELETE
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// test the delete method
			await (s[index].DeleteAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			await (AssertDeleteAsync(id));
		}

		[Test]
		public async Task TestPrimitiveArrayCRUDAsync()
		{
			int maxIndex = 4;
			ISession[] s = new ISession[maxIndex];
			ITransaction[] t = new ITransaction[maxIndex];
			BasicClass[] bc = new BasicClass[maxIndex];
			int index = 0;
			int id = 1;
			bc[index] = await (InsertBasicClassAsync(id));
			index++;
			// modify the array so it is updated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			for (int i = 0; i < bc[index].Int32Array.Length; i++)
			{
				bc[index].Int32Array[i] = i + 1;
			}

			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// modify the array to a new array so it is recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].Int32Array = new int[]{1, 2, 3, 4, 5, 6};
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// VERIFY PREVIOUS UPDATE & PERFORM DELETE
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// test the delete method
			await (s[index].DeleteAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			await (AssertDeleteAsync(id));
		}

		[Test]
		public async Task TestMapCRUDAsync()
		{
			int maxIndex = 4;
			ISession[] s = new ISession[maxIndex];
			ITransaction[] t = new ITransaction[maxIndex];
			BasicClass[] bc = new BasicClass[maxIndex];
			int index = 0;
			int id = 1;
			bc[index] = await (InsertBasicClassAsync(id));
			index++;
			// modify the array so it is updated - should not be recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// remove the last one and update another
			bc[index].StringMap.Remove("keyOne");
			bc[index].StringMap["keyTwo"] = "modified string two";
			bc[index].StringMap["keyThree"] = "added key three";
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// change the List to a new List so it is recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].StringMap = new Dictionary<string, string>();
			bc[index].StringMap.Add("keyZero", "new list zero");
			bc[index].StringMap.Add("keyOne", "new list one");
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// VERIFY PREVIOUS UPDATE & PERFORM DELETE
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// test the delete method
			await (s[index].DeleteAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			await (AssertDeleteAsync(id));
		}

		[Test]
		public async Task TestSetCRUDAsync()
		{
			int maxIndex = 4;
			ISession[] s = new ISession[maxIndex];
			ITransaction[] t = new ITransaction[maxIndex];
			BasicClass[] bc = new BasicClass[maxIndex];
			int index = 0;
			int id = 1;
			bc[index] = await (InsertBasicClassAsync(id));
			index++;
			// modify the array so it is updated - should not be recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// remove the last one and add another
			bc[index].StringSet.Remove("zero");
			bc[index].StringSet.Add("two");
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// change the List to a new List so it is recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].StringSet = new HashSet<string>{"zero", "one"};
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// VERIFY PREVIOUS UPDATE & PERFORM DELETE
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// test the delete method
			await (s[index].DeleteAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			await (AssertDeleteAsync(id));
		}

		[Test]
		public async Task TestBagCRUDAsync()
		{
			int maxIndex = 5;
			ISession[] s = new ISession[maxIndex];
			ITransaction[] t = new ITransaction[maxIndex];
			BasicClass[] bc = new BasicClass[maxIndex];
			int index = 0;
			int id = 1;
			bc[index] = await (InsertBasicClassAsync(id));
			index++;
			// modify the bag so it is updated - should not be recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// remove the last one and update another
			bc[index].StringBag.RemoveAt(bc[index].StringBag.Count - 1);
			bc[index].StringBag[1] = "modified string 1";
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// add an item to the list
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// remove the last one and update another
			bc[index].StringBag.Add("inserted into the bag");
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// change the List to a new List so it is recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].StringBag = new List<string>();
			bc[index].StringBag.Add("new bag zero");
			bc[index].StringBag.Add("new bag one");
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// VERIFY PREVIOUS UPDATE & PERFORM DELETE
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// test the delete method
			await (s[index].DeleteAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			await (AssertDeleteAsync(id));
		}

		[Test]
		public async Task BagRefreshAsync()
		{
			int id = 1;
			int originalCount;
			BasicClass basicClass = await (InsertBasicClassAsync(id));
			originalCount = basicClass.StringBag.Count;
			ISession s = OpenSession();
			// There used to be a transaction started on s and commited before the Close().
			// This transaction was removed since it was causing a deadlock with SQLite.
			// This is a theoretical improvement as well, since the transaction could
			// be in a mode that would prevent non-repeatable reads, hence breaking the test.
			ISession s2 = OpenSession();
			ITransaction t2 = s2.BeginTransaction();
			BasicClass bc = (BasicClass)await (s.LoadAsync(typeof (BasicClass), id));
			BasicClass bc2 = (BasicClass)await (s2.LoadAsync(typeof (BasicClass), id));
			bc2.StringBag.Add("refresh value");
			await (t2.CommitAsync());
			s2.Close();
			await (s.RefreshAsync(bc));
			Assert.AreEqual(originalCount + 1, bc.StringBag.Count, "was refreshed correctly");
			await (s.DeleteAsync(bc));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task TestListCRUDAsync()
		{
			int maxIndex = 5;
			ISession[] s = new ISession[maxIndex];
			ITransaction[] t = new ITransaction[maxIndex];
			BasicClass[] bc = new BasicClass[maxIndex];
			int index = 0;
			int id = 1;
			bc[index] = await (InsertBasicClassAsync(id));
			index++;
			// modify the array so it is updated - should not be recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// remove the last one and update another
			bc[index].StringList.RemoveAt(bc[index].StringList.Count - 1);
			bc[index].StringList[2] = "modified string 2";
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// add an item to the list
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// remove the last one and update another
			bc[index].StringList.Add("inserted into the list");
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// change the List to a new List so it is recreated
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			bc[index].StringList = new List<string>();
			bc[index].StringList.Add("new list zero");
			bc[index].StringList.Add("new list one");
			await (s[index].UpdateAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			// VERIFY PREVIOUS UPDATE & PERFORM DELETE
			s[index] = OpenSession();
			t[index] = s[index].BeginTransaction();
			bc[index] = (BasicClass)await (s[index].LoadAsync(typeof (BasicClass), id));
			AssertPropertiesEqual(bc[index - 1], bc[index]);
			// test the delete method
			await (s[index].DeleteAsync(bc[index]));
			await (t[index].CommitAsync());
			s[index].Close();
			index++;
			await (AssertDeleteAsync(id));
		}

		[Test]
		public async Task TestWrapArrayInListPropertyAsync()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			BasicClass bc = new BasicClass();
			if (!TestDialect.SupportsNullCharactersInUtfStrings)
				bc.CharacterProperty = 'a';
			int id = 1;
			bc.StringList = new string[]{"one", "two"};
			await (s.SaveAsync(bc, id));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			bc = (BasicClass)await (s.LoadAsync(typeof (BasicClass), id));
			Assert.AreEqual(2, bc.StringList.Count, "should have saved to StringList from an array");
			Assert.IsTrue(bc.StringList.Contains("one"), "'one' should be in there");
			Assert.IsTrue(bc.StringList.Contains("two"), "'two' should be in there");
			await (s.DeleteAsync(bc));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task TestLinqWhereOnDictionaryPropertyAsync()
		{
			await (InsertBasicClassAsync(1));
			using (var session = OpenSession())
				using (var trans = session.BeginTransaction())
				{
					IQueryable<BasicClass> bcs = null;
					IList<BasicClass> bcsList = null;
					// IDictionary[]
					bcs = session.Query<BasicClass>().Where(bc => (string)bc.StringMap["keyZero"] == "string zero");
					Assert.That(bcs.Count(), Is.EqualTo(1));
					bcsList = bcs.ToList<BasicClass>();
					Assert.That(bcsList.All(f => f.StringMap != null), Is.True);
					Assert.That(bcsList.All(f => f.StringMap.Count == 3), Is.True);
					Assert.That(bcsList.All(f => ((f.StringMap.ContainsKey("keyZero")) && ((string)f.StringMap["keyZero"] == "string zero"))), Is.True);
					// IDictionary<,>[]
					bcs = session.Query<BasicClass>().Where(bc => bc.StringMapGeneric["keyOne"] == "string one");
					Assert.That(bcs.Count(), Is.EqualTo(1));
					bcsList = bcs.ToList<BasicClass>();
					Assert.That(bcsList.All(f => f.StringMapGeneric != null), Is.True);
					Assert.That(bcsList.All(f => f.StringMapGeneric.Count == 3), Is.True);
					Assert.That(bcsList.All(f => ((f.StringMapGeneric.ContainsKey("keyOne")) && (f.StringMapGeneric["keyOne"] == "string one"))), Is.True);
					// IDictionary.Contains
					bcs = session.Query<BasicClass>().Where(bc => bc.StringMap.ContainsKey("keyZero"));
					Assert.That(bcs.Count(), Is.EqualTo(1));
					bcsList = bcs.ToList<BasicClass>();
					Assert.That(bcsList.All(f => f.StringMap != null), Is.True);
					Assert.That(bcsList.All(f => f.StringMap.Count == 3), Is.True);
					Assert.That(bcsList.All(f => ((f.StringMap.ContainsKey("keyZero")) && ((string)f.StringMap["keyZero"] == "string zero"))), Is.True);
					// IDictionary<,>.ContainsKey
					bcs = session.Query<BasicClass>().Where(bc => bc.StringMapGeneric.ContainsKey("keyZero"));
					Assert.That(bcs.Count(), Is.EqualTo(1));
					bcsList = bcs.ToList<BasicClass>();
					Assert.That(bcsList.All(f => f.StringMapGeneric != null), Is.True);
					Assert.That(bcsList.All(f => f.StringMapGeneric.Count == 3), Is.True);
					Assert.That(bcsList.All(f => ((f.StringMapGeneric.ContainsKey("keyOne")) && (f.StringMapGeneric["keyOne"] == "string one"))), Is.True);
					await (session.DeleteAsync("from BasicClass"));
					await (trans.CommitAsync());
				}
		}

		/// <summary>
		/// Test for NH-2415
		/// </summary>
		[Test]
		public async Task TestHqlParameterizedDictionaryLookupProducesCorrectSqlParameterOrderAsync()
		{
			var bc = await (InsertBasicClassAsync(1));
			using (var session = OpenSession())
				using (var trans = session.BeginTransaction())
				{
					var hql = "from BasicClass bc where (bc.StringProperty = :prop) and (bc.StringMap[:key] = :value)";
					bc = await (session.CreateQuery(hql).SetParameter("prop", "string property").SetParameter("key", "keyZero").SetParameter("value", "string zero").UniqueResultAsync<BasicClass>());
					Assert.NotNull(bc);
					await (session.DeleteAsync(bc));
					await (trans.CommitAsync());
				}
		}

		internal async Task AssertDeleteAsync(int id)
		{
			ISession s = OpenSession();
			try
			{
				await (s.LoadAsync(typeof (BasicClass), id));
			}
			catch (ObjectNotFoundException onfe)
			{
				// I expect this to be thrown because the object no longer exists...
				Assert.IsNotNull(onfe); //getting ride of 'onfe' is never used compile warning
			}

			IList results = await (s.CreateCriteria(typeof (BasicClass)).Add(Expression.Eq("Id", id)).ListAsync());
			Assert.AreEqual(0, results.Count);
			s.Close();
		}

		internal async Task<BasicClass> InsertBasicClassAsync(int id)
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			BasicClass bc = new BasicClass();
			InitializeBasicClass(id, ref bc);
			await (s.SaveAsync(bc));
			await (t.CommitAsync());
			s.Close();
			return bc;
		}

		/// <summary>
		/// Compares the non Collection Properties of the BasicClass
		/// </summary>
		/// <param name = "expected">The expected values.</param>
		/// <param name = "actual">The Actual values.</param>
		/// <remarks>
		/// Passes to the overload with includeCollections=true
		/// </remarks>
		internal void AssertPropertiesEqual(BasicClass expected, BasicClass actual)
		{
			AssertPropertiesEqual(expected, actual, true);
		}

		/// <summary>
		/// Compares the non Collection Properties of the BasicClass
		/// </summary>
		/// <param name = "expected">The expected values.</param>
		/// <param name = "actual">The Actual values.</param>
		/// <param name = "includeCollections"></param>
		internal void AssertPropertiesEqual(BasicClass expected, BasicClass actual, bool includeCollections)
		{
			Assert.AreEqual(expected.Id, actual.Id, "Id");
			Assert.AreEqual(expected.CharacterProperty, actual.CharacterProperty, "CharacterProperty");
			Assert.AreEqual(expected.ClassProperty, actual.ClassProperty, "ClassProperty");
			Assert.AreEqual(expected.CultureInfoProperty, actual.CultureInfoProperty, "CultureInfoProperty");
			Assert.AreEqual(expected.DateTimeProperty, actual.DateTimeProperty, "DateTimeProperty");
			Assert.AreEqual(expected.Int16Property, actual.Int16Property, "Int16Property");
			Assert.AreEqual(expected.Int32Property, actual.Int32Property, "Int32Property");
			Assert.AreEqual(expected.Int64Property, actual.Int64Property, "Int64Property");
			Assert.AreEqual(expected.SingleProperty, actual.SingleProperty, 0, "SingleProperty");
			Assert.AreEqual(expected.StringProperty, actual.StringProperty, "StringProperty");
			Assert.AreEqual(expected.TicksProperty, actual.TicksProperty, "TicksProperty");
			Assert.AreEqual(expected.TrueFalseProperty, actual.TrueFalseProperty, "TrueFalseProperty");
			Assert.AreEqual(expected.YesNoProperty, actual.YesNoProperty, "YesNoProperty");
			if (includeCollections)
			{
				ObjectAssertAsync.AreEqual(expected.StringArray, actual.StringArray);
				ObjectAssertAsync.AreEqual(expected.Int32Array, actual.Int32Array);
				ObjectAssertAsync.AreEqual(expected.StringBag, actual.StringBag, false);
				ObjectAssertAsync.AreEqual(expected.StringList, actual.StringList);
				ObjectAssertAsync.AreEqual(expected.StringMap, actual.StringMap, true);
				ObjectAssertAsync.AreEqual(expected.StringSet, actual.StringSet);
			}
		}

		private void InitializeBasicClass(int id, ref BasicClass basicClass)
		{
			basicClass.Id = id;
			basicClass.CharacterProperty = 'a';
			basicClass.ClassProperty = typeof (object);
			basicClass.CultureInfoProperty = CultureInfo.CurrentCulture;
			basicClass.DateTimeProperty = DateTime.Parse("2003-12-01 10:45:21 AM");
			basicClass.Int16Property = Int16.MaxValue;
			basicClass.Int32Property = Int32.MaxValue;
			basicClass.Int64Property = Int64.MaxValue;
			// more MySql problems - it returns 3.40282E38
			// instead of 3.402823E+38 which is Single.MaxValue
			basicClass.SingleProperty = 3.5F; //Single.MaxValue;
			basicClass.StringProperty = "string property";
			basicClass.TicksProperty = DateTime.Now;
			basicClass.TrueFalseProperty = true;
			basicClass.YesNoProperty = true;
			basicClass.StringArray = new string[]{"3 string", "2 string", "1 string"};
			basicClass.Int32Array = new int[]{5, 4, 3, 2, 1};
			IList<string> stringBag = new List<string>(3);
			stringBag.Add("string 0");
			stringBag.Add("string 1");
			stringBag.Add("string 2");
			basicClass.StringBag = stringBag;
			IList<string> stringList = new List<string>(5);
			stringList.Add("new string zero");
			stringList.Add("new string one");
			stringList.Add("new string two");
			stringList.Add("new string three");
			stringList.Add("new string four");
			basicClass.StringList = stringList;
			IDictionary<string, string> stringMap = new Dictionary<string, string>();
			stringMap.Add("keyOne", "string one");
			stringMap.Add("keyZero", "string zero");
			stringMap.Add("keyTwo", "string two");
			basicClass.StringMap = stringMap;
			basicClass.AddToStringSet("zero");
			basicClass.AddToStringSet("one");
			basicClass.AddToStringSet("zero");
		}
	}
}
#endif
