#if NET_4_5
using System.Collections;
using NUnit.Framework;
using NHibernate.Type;
using System.Threading.Tasks;

namespace NHibernate.Test.Interceptor
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class InterceptorFixtureAsync : TestCaseAsync
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
				return new string[]{"Interceptor.User.hbm.xml", "Interceptor.Image.hbm.xml"};
			}
		}

		[Test]
		public async Task CollectionInterceptAsync()
		{
			ISession s = OpenSession(new CollectionInterceptor());
			ITransaction t = s.BeginTransaction();
			User u = new User("Gavin", "nivag");
			await (s.PersistAsync(u));
			u.Password = "vagni";
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			u = await (s.GetAsync<User>("Gavin"));
			Assert.AreEqual(2, u.Actions.Count);
			await (s.DeleteAsync(u));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task PropertyInterceptAsync()
		{
			ISession s = OpenSession(new PropertyInterceptor());
			ITransaction t = s.BeginTransaction();
			User u = new User("Gavin", "nivag");
			await (s.PersistAsync(u));
			u.Password = "vagni";
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			u = await (s.GetAsync<User>("Gavin"));
			Assert.IsTrue(u.Created.HasValue);
			Assert.IsTrue(u.LastUpdated.HasValue);
			await (s.DeleteAsync(u));
			await (t.CommitAsync());
			s.Close();
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		private class HHH1921Interceptor : EmptyInterceptor
		{
			public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames, IType[] types)
			{
				currentState[0] = "test";
				return true;
			}
		}

		///
		///Here the interceptor resets the
		///current-state to the same thing as the current db state; this
		///causes EntityPersister.FindDirty() to return no dirty properties.
		///
		[Test]
		public async Task PropertyIntercept2Async()
		{
			ISession s = OpenSession();
			ITransaction t = s.BeginTransaction();
			User u = new User("Josh", "test");
			await (s.PersistAsync(u));
			await (t.CommitAsync());
			s.Close();
			s = OpenSession(new HHH1921Interceptor());
			t = s.BeginTransaction();
			u = await (s.GetAsync<User>(u.Name));
			u.Password = "nottest";
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			u = await (s.GetAsync<User>("Josh"));
			Assert.AreEqual("test", u.Password);
			await (s.DeleteAsync(u));
			await (t.CommitAsync());
			s.Close();
		}

		[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
		private class MyComponentInterceptor : EmptyInterceptor
		{
			readonly int checkPerm;
			readonly string checkComment;
			public MyComponentInterceptor(int checkPerm, string checkComment)
			{
				this.checkPerm = checkPerm;
				this.checkComment = checkComment;
			}

			public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
			{
				if (state[0] == null)
				{
					Image.Detail detail = new Image.Detail();
					detail.Perm1 = checkPerm;
					detail.Comment = checkComment;
					state[0] = detail;
				}

				return true;
			}
		}

		[Test]
		public async Task ComponentInterceptorAsync()
		{
			const int checkPerm = 500;
			const string checkComment = "generated from interceptor";
			ISession s = OpenSession(new MyComponentInterceptor(checkPerm, checkComment));
			ITransaction t = s.BeginTransaction();
			Image i = new Image();
			i.Name = "compincomp";
			i = (Image)s.Merge(i);
			Assert.IsNotNull(i.Details);
			Assert.AreEqual(checkPerm, i.Details.Perm1);
			Assert.AreEqual(checkComment, i.Details.Comment);
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			i = await (s.GetAsync<Image>(i.Id));
			Assert.IsNotNull(i.Details);
			Assert.AreEqual(checkPerm, i.Details.Perm1);
			Assert.AreEqual(checkComment, i.Details.Comment);
			await (s.DeleteAsync(i));
			await (t.CommitAsync());
			s.Close();
		}

		[Test]
		public async Task StatefulInterceptAsync()
		{
			StatefulInterceptor statefulInterceptor = new StatefulInterceptor();
			ISession s = OpenSession(statefulInterceptor);
			Assert.IsNotNull(statefulInterceptor.Session);
			ITransaction t = s.BeginTransaction();
			User u = new User("Gavin", "nivag");
			await (s.PersistAsync(u));
			u.Password = "vagni";
			await (t.CommitAsync());
			s.Close();
			s = OpenSession();
			t = s.BeginTransaction();
			IList logs = await (s.CreateCriteria(typeof (Log)).ListAsync());
			Assert.AreEqual(2, logs.Count);
			await (s.DeleteAsync(u));
			await (s.DeleteAsync("from Log"));
			await (t.CommitAsync());
			s.Close();
		}
	}
}
#endif
