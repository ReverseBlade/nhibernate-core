﻿using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3570
{
	[TestFixture]
	public partial class UniFixture : BugTestCase
	{
		private Guid id;

		[Test]
		public void ShouldNotSaveRemoveChild()
		{
			Assert.Throws<Exception>(
				() =>
				{
					var parent = new UniParent();
					parent.Children.Add(new UniChild());
					using (var s = OpenSession())
					{
						using (var tx = s.BeginTransaction())
						{
							id = (Guid)s.Save(parent);
							parent.Children.Clear();
							parent.Children.Add(new UniChild());
							tx.Commit();
						}
					}
					using (var s = OpenSession())
					{
						using (s.BeginTransaction())
						{
							Assert.That(s.Get<UniParent>(id).Children.Count, Is.EqualTo(1));
							Assert.That(s.CreateCriteria<UniChild>().List().Count, Is.EqualTo(1));
						}
					}
				}, KnownBug.Issue("NH-3570"));
		}

		protected override void OnTearDown()
		{
			using (var s = OpenSession())
			{
				using (var tx = s.BeginTransaction())
				{
					s.CreateQuery("delete from UniChild").ExecuteUpdate();
					s.CreateQuery("delete from UniParent").ExecuteUpdate();
					tx.Commit();
				}
			}
		}
	}
}