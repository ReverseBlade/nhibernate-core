﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Event.Default
{
	using System.Threading.Tasks;
	/// <summary> 
	/// Process collections reachable from an entity. 
	/// This visitor assumes that wrap was already performed for the entity.
	/// </summary>
	public partial class FlushVisitor : AbstractVisitor
	{

		internal override async Task<object> ProcessCollectionAsync(object collection, CollectionType type)
		{
			if (collection == CollectionType.UnfetchedCollection)
			{
				return null;
			}

			if (collection != null)
			{
				IPersistentCollection coll;
				if (type.IsArrayType)
				{
					coll = Session.PersistenceContext.GetCollectionHolder(collection);
				}
				else
				{
					coll = (IPersistentCollection)collection;
				}

				await (Collections.ProcessReachableCollectionAsync(coll, type, owner, Session)).ConfigureAwait(false);
			}
			return null;
		}
	}
}