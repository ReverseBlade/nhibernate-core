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
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using NHibernate.Collection.Generic.SetHelpers;
using NHibernate.DebugHelpers;
using NHibernate.Engine;
using NHibernate.Loader;
using NHibernate.Persister.Collection;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Collection.Generic
{
	using System.Threading.Tasks;
	public partial class PersistentGenericSet<T> : AbstractPersistentCollection, ISet<T>
	{

		public override Task<ICollection> GetOrphansAsync(object snapshot, string entityName)
		{
			try
			{
				var sn = new SetSnapShot<T>((IEnumerable<T>)snapshot);
				// TODO: Avoid duplicating shortcuts and array copy, by making base class GetOrphans() more flexible
				if (WrappedSet.Count == 0)
					return Task.FromResult<ICollection>(sn);
				if (((ICollection)sn).Count == 0)
					return Task.FromResult<ICollection>(sn);
				return GetOrphansAsync(sn, WrappedSet.ToArray(), entityName, Session);
			}
			catch (Exception ex)
			{
				return Task.FromException<ICollection>(ex);
			}
		}

		/// <summary>
		/// Initializes this PersistentSet from the cached values.
		/// </summary>
		/// <param name="persister">The CollectionPersister to use to reassemble the PersistentSet.</param>
		/// <param name="disassembled">The disassembled PersistentSet.</param>
		/// <param name="owner">The owner object.</param>
		public override async Task InitializeFromCacheAsync(ICollectionPersister persister, object disassembled, object owner)
		{
			var array = (object[])disassembled;
			int size = array.Length;
			BeforeInitialize(persister, size);
			for (int i = 0; i < size; i++)
			{
				var element = await (persister.ElementType.AssembleAsync(array[i], Session, owner)).ConfigureAwait(false);
				if (element != null)
				{
					WrappedSet.Add((T) element);
				}
			}
			SetInitialized();
		}

		public override async Task<object> ReadFromAsync(DbDataReader rs, ICollectionPersister role, ICollectionAliases descriptor, object owner)
		{
			var element = await (role.ReadElementAsync(rs, owner, descriptor.SuffixedElementAliases, Session)).ConfigureAwait(false);
			if (element != null)
			{
				_tempList.Add((T) element);
			}
			return element;
		}

		#region ISet<T> Members

		#endregion
		#region ICollection<T> Members

		#endregion
		#region IEnumerable Members

		#endregion
		#region IEnumerable<T> Members

		#endregion
		#region DelayedOperations

		#endregion
	}
}