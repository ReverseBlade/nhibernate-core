﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;

using NHibernate.Collection;
using NHibernate.Impl;
using NHibernate.Persister.Collection;

namespace NHibernate.Engine
{
	using System.Threading.Tasks;
	public partial class CollectionEntry
	{

		public Task<ICollection> GetOrphansAsync(string entityName, IPersistentCollection collection)
		{
			if (snapshot == null)
			{
				throw new AssertionFailure("no collection snapshot for orphan delete");
			}
			return collection.GetOrphansAsync(snapshot, entityName);
		}
	}
}