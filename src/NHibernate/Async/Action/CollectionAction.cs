﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Runtime.Serialization;
using NHibernate.Cache;
using NHibernate.Cache.Access;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Persister.Collection;
using NHibernate.Util;

namespace NHibernate.Action
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public abstract partial class CollectionAction : IExecutable, IComparable<CollectionAction>, IDeserializationCallback
	{

		#region IExecutable Members

		/// <summary>Execute this action</summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public abstract Task ExecuteAsync(CancellationToken cancellationToken);

		#endregion
	}
}
