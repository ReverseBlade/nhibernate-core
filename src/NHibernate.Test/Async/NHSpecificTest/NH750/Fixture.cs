#if NET_4_5
using System;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH750
{
	[TestFixture]
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class FixtureAsync : BugTestCaseAsync
	{
		protected override async Task OnTearDownAsync()
		{
			using (ISession s = sessions.OpenSession())
			{
				await (s.DeleteAsync("from Device"));
				await (s.DeleteAsync("from Drive"));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task DeviceOfDriveAsync()
		{
			int[] dvSavedId = new int[2];
			Drive dr1 = new Drive("Drive 1");
			Drive dr2 = new Drive("Drive 2");
			Drive dr3 = new Drive("Drive 3");
			Device dv1 = new Device("Device 1");
			Device dv2 = new Device("Device 2");
			using (ISession s = sessions.OpenSession())
			{
				await (s.SaveAsync(dr1));
				await (s.SaveAsync(dr2));
				await (s.SaveAsync(dr3));
				dvSavedId[0] = (int)await (s.SaveAsync(dv1));
				dvSavedId[1] = (int)await (s.SaveAsync(dv2));
				await (s.FlushAsync());
			}

			dv1.Drives.Add(dr1);
			dv1.Drives.Add(dr2);
			dv2.Drives.Add(dr1);
			dv2.Drives.Add(dr3);
			using (ISession s = sessions.OpenSession())
			{
				dvSavedId[0] = (int)await (s.SaveAsync(dv1));
				dvSavedId[1] = (int)await (s.SaveAsync(dv2));
				await (s.FlushAsync());
			}

			dv1 = null;
			dv2 = null;
			using (ISession s = sessions.OpenSession())
			{
				await (s.DeleteAsync(dr3));
				await (s.FlushAsync());
				dv1 = (Device)await (s.LoadAsync(typeof (Device), dvSavedId[0]));
				dv2 = (Device)await (s.LoadAsync(typeof (Device), dvSavedId[1]));
			}

			Assert.AreEqual(2, dv1.Drives.Count);
			Assert.AreEqual(2, dv2.Drives.Count);
			// Verify dv1 unchanged
			Assert.IsTrue(dv1.Drives.Contains(dr1));
			Assert.IsTrue(dv1.Drives.Contains(dr2));
			// Verify dv2
			Assert.IsTrue(dv2.Drives.Contains(dr1));
			Assert.IsFalse(dv2.Drives.Contains(dr3));
			// Verify one null
			int nullCount = 0;
			for (int i = 0; i < dv2.Drives.Count; i++)
			{
				if (dv2.Drives[i] == null)
					nullCount++;
			}

			Assert.AreEqual(1, nullCount);
		}
	}
}
#endif
