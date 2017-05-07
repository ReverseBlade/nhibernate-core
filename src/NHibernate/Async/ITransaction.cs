﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Data;
using System.Data.Common;
using NHibernate.Transaction;

namespace NHibernate
{
	using System.Threading.Tasks;
	/// <summary>
	/// Allows the application to define units of work, while maintaining abstraction from the
	/// underlying transaction implementation
	/// </summary>
	/// <remarks>
	/// A transaction is associated with a <c>ISession</c> and is usually instantiated by a call to
	/// <c>ISession.BeginTransaction()</c>. A single session might span multiple transactions since 
	/// the notion of a session (a conversation between the application and the datastore) is of
	/// coarser granularity than the notion of a transaction. However, it is intended that there be
	/// at most one uncommitted <c>ITransaction</c> associated with a particular <c>ISession</c>
	/// at a time. Implementors are not intended to be threadsafe.
	/// </remarks>
	public partial interface ITransaction : IDisposable
	{

		/// <summary>
		/// Flush the associated <c>ISession</c> and end the unit of work.
		/// </summary>
		/// <remarks>
		/// This method will commit the underlying transaction if and only if the transaction
		/// was initiated by this object.
		/// </remarks>
		Task CommitAsync();
	}
}