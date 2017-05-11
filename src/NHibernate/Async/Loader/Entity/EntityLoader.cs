﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.Type;

namespace NHibernate.Loader.Entity
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class EntityLoader : AbstractEntityLoader
	{

		public Task<object> LoadByUniqueKeyAsync(ISessionImplementor session, object key, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			return LoadAsync(session, key, null, null, cancellationToken);
		}
	}
}