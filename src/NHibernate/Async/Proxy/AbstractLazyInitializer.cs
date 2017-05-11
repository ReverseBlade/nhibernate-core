﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Engine;
using NHibernate.Persister.Entity;

namespace NHibernate.Proxy
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public abstract partial class AbstractLazyInitializer : ILazyInitializer
	{
		
		/// <summary>
		/// Perform an ImmediateLoad of the actual object for the Proxy.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <exception cref="HibernateException">
		/// Thrown when the Proxy has no Session or the Session is closed or disconnected.
		/// </exception>
		public virtual async Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!initialized)
			{
				if (_session == null)
				{
					throw new LazyInitializationException(_entityName, _id, "Could not initialize proxy - no Session.");
				}
				else if (!_session.IsOpen)
				{
					throw new LazyInitializationException(_entityName, _id, "Could not initialize proxy - the owning Session was closed.");
				}
				else if (!_session.IsConnected)
				{
					throw new LazyInitializationException(_entityName, _id, "Could not initialize proxy - the owning Session is disconnected.");
				}
				else
				{
					_target = await (_session.ImmediateLoadAsync(_entityName, _id, cancellationToken)).ConfigureAwait(false);
					initialized = true;
					CheckTargetState();
				}
			}
			else
			{
				CheckTargetState();
			}
		}

		/// <summary>
		/// Return the Underlying Persistent Object, initializing if necessary.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>The Persistent Object this proxy is Proxying.</returns>
		public async Task<object> GetImplementationAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			await (InitializeAsync(cancellationToken)).ConfigureAwait(false);
			return _target;
		}
	}
}
