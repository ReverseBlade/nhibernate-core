﻿#if NET_4_5
using System.Collections;
using NHibernate.Criterion;
using NHibernate.DomainModel.NHSpecific;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class UnsavedValueFixtureAsync : TestCaseAsync
	{
		public static int newId = 0;
		protected override IList Mappings
		{
			get
			{
				return new string[]{"NHSpecific.UnsavedType.hbm.xml"};
			}
		}

		[Test]
		public async Task TestCRUDAsync()
		{
			// make a new object outside of the Session
			UnsavedType unsavedToSave = new UnsavedType();
			unsavedToSave.TypeName = "Simple UnsavedValue";
			// open the first session to SaveOrUpdate it - should be Save
			ISession s1 = OpenSession();
			ITransaction t1 = s1.BeginTransaction();
			await (s1.SaveOrUpdateAsync(unsavedToSave));
			await (t1.CommitAsync());
			s1.Close();
			// simple should have been inserted - generating a new key for it
			Assert.IsTrue(unsavedToSave.Id != 0, "Id should not be zero");
			// use the ICriteria interface to get another instance in a different
			// session
			ISession s2 = OpenSession();
			ITransaction t2 = s2.BeginTransaction();
			IList results2 = await (s2.CreateCriteria(typeof (UnsavedType)).Add(Expression.Eq("Id", unsavedToSave.Id)).ListAsync());
			Assert.AreEqual(1, results2.Count, "Should have found a match for the new Id");
			UnsavedType unsavedToUpdate = (UnsavedType)results2[0];
			// make sure it has the same Id
			Assert.AreEqual(unsavedToSave.Id, unsavedToUpdate.Id, "Should have the same Id");
			await (t2.CommitAsync());
			s2.Close();
			// passing it to the UI for modification
			unsavedToUpdate.TypeName = "ui changed it";
			// create a new session for the Update
			ISession s3 = OpenSession();
			ITransaction t3 = s3.BeginTransaction();
			await (s3.SaveOrUpdateAsync(unsavedToUpdate));
			await (t3.CommitAsync());
			s3.Close();
			// make sure it has the same Id - if the Id has changed then that means it
			// was inserted.
			Assert.AreEqual(unsavedToSave.Id, unsavedToUpdate.Id, "Should have the same Id");
			// lets get a list of all the rows in the table to make sure 
			// that there has not been any extra inserts
			ISession s4 = OpenSession();
			ITransaction t4 = s4.BeginTransaction();
			IList results4 = await (s4.CreateCriteria(typeof (UnsavedType)).ListAsync());
			Assert.AreEqual(1, results4.Count, "Should only be one item");
			// lets make sure the object was updated
			UnsavedType unsavedToDelete = (UnsavedType)results4[0];
			Assert.AreEqual(unsavedToUpdate.TypeName, unsavedToDelete.TypeName);
			await (s4.DeleteAsync(unsavedToDelete));
			await (t4.CommitAsync());
			s4.Close();
			// lets make sure the object was deleted
			ISession s5 = OpenSession();
			try
			{
				UnsavedType unsavedNull = (UnsavedType)await (s5.LoadAsync(typeof (UnsavedType), unsavedToDelete.Id));
				Assert.IsNull(unsavedNull);
			}
			catch (ObjectNotFoundException onfe)
			{
				// do nothing it was expected
				Assert.IsNotNull(onfe); //getting ride of 'onfe' is never used compile warning
			}

			s5.Close();
		}
	}
}
#endif
