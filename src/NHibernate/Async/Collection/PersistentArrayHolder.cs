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

using NHibernate.DebugHelpers;
using NHibernate.Engine;
using NHibernate.Loader;
using NHibernate.Persister.Collection;
using NHibernate.Type;

namespace NHibernate.Collection
{
	using System.Threading.Tasks;
	public partial class PersistentArrayHolder : AbstractPersistentCollection, ICollection
	{

		public override async Task<ICollection> GetOrphansAsync(object snapshot, string entityName)
		{
			object[] sn = (object[]) snapshot;
			object[] arr = (object[]) array;
			List<object> result = new List<object>(sn);
			for (int i = 0; i < sn.Length; i++)
			{
				await (IdentityRemoveAsync(result, arr[i], entityName, Session)).ConfigureAwait(false);
			}
			return result;
		}

		public override async Task<object> ReadFromAsync(DbDataReader rs, ICollectionPersister role, ICollectionAliases descriptor, object owner)
		{
			object element = await (role.ReadElementAsync(rs, owner, descriptor.SuffixedElementAliases, Session)).ConfigureAwait(false);
			int index = (int) await (role.ReadIndexAsync(rs, descriptor.SuffixedIndexAliases, Session)).ConfigureAwait(false);
			for (int i = tempList.Count; i <= index; i++)
			{
				tempList.Add(null);
			}
			tempList[index] = element;
			return element;
		}

		/// <summary>
		/// Initializes this array holder from the cached values.
		/// </summary>
		/// <param name="persister">The CollectionPersister to use to reassemble the Array.</param>
		/// <param name="disassembled">The disassembled Array.</param>
		/// <param name="owner">The owner object.</param>
		public override async Task InitializeFromCacheAsync(ICollectionPersister persister, object disassembled, object owner)
		{
			object[] cached = (object[]) disassembled;

			array = System.Array.CreateInstance(persister.ElementClass, cached.Length);

			for (int i = 0; i < cached.Length; i++)
			{
				array.SetValue(await (persister.ElementType.AssembleAsync(cached[i], Session, owner)).ConfigureAwait(false), i);
			}
		}

		#region ICollection Members

		#endregion
		#region IEnumerable Members

		#endregion
	}
}