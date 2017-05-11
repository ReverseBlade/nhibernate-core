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
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.Type;

namespace NHibernate.Event.Default
{
	using System.Threading.Tasks;
	using System.Threading;
	using System;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class WrapVisitor : ProxyVisitor
	{

		internal override async Task ProcessAsync(object obj, IEntityPersister persister, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] values = persister.GetPropertyValues(obj);
			IType[] types = persister.PropertyTypes;
			await (ProcessEntityPropertyValuesAsync(values, types, cancellationToken)).ConfigureAwait(false);
			if (SubstitutionRequired)
			{
				persister.SetPropertyValues(obj, values);
			}
		}

		internal override Task<object> ProcessCollectionAsync(object collection, CollectionType collectionType, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Task.FromResult<object>(ProcessCollection(collection, collectionType));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		internal override async Task ProcessValueAsync(int i, object[] values, IType[] types, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			object result = await (ProcessValueAsync(values[i], types[i], cancellationToken)).ConfigureAwait(false);
			if (result != null)
			{
				substitute = true;
				values[i] = result;
			}
		}

		internal override async Task<object> ProcessComponentAsync(object component, IAbstractComponentType componentType, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (component != null)
			{
				object[] values = componentType.GetPropertyValues(component, Session);
				IType[] types = componentType.Subtypes;
				bool substituteComponent = false;
				for (int i = 0; i < types.Length; i++)
				{
					object result = await (ProcessValueAsync(values[i], types[i], cancellationToken)).ConfigureAwait(false);
					if (result != null)
					{
						values[i] = result;
						substituteComponent = true;
					}
				}
				if (substituteComponent)
				{
					componentType.SetPropertyValues(component, values);
				}
			}

			return null;
		}
	}
}
