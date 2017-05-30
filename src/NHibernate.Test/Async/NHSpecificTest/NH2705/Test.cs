﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace NHibernate.Test.NHSpecificTest.NH2705
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class TestAsync : BugTestCase
	{
		private static async Task<IEnumerable<T>> GetAndFetchAsync<T>(string name, ISession session, CancellationToken cancellationToken = default(CancellationToken)) where T : ItemBase
		{
			// this is a valid abstraction, the calling code should be able to ask that a property is eagerly loaded/available
			// without having to know how it is mapped
			return await (session.Query<T>()
				.Fetch(p => p.SubItem).ThenFetch(p => p.Details) // should be able to fetch .Details when used with components (NH2615)
				.Where(p => p.SubItem.Name == name).ToListAsync(cancellationToken));
		}
	}
}