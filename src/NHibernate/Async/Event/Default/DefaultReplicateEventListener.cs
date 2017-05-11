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
using NHibernate.Impl;
using NHibernate.Persister.Entity;

namespace NHibernate.Event.Default
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class DefaultReplicateEventListener : AbstractSaveEventListener, IReplicateEventListener
	{

		public virtual async Task OnReplicateAsync(ReplicateEvent @event, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			IEventSource source = @event.Session;
			if (source.PersistenceContext.ReassociateIfUninitializedProxy(@event.Entity))
			{
				log.Debug("uninitialized proxy passed to replicate()");
				return;
			}

			object entity = await (source.PersistenceContext.UnproxyAndReassociateAsync(@event.Entity, cancellationToken)).ConfigureAwait(false);

			if (source.PersistenceContext.IsEntryFor(entity))
			{
				log.Debug("ignoring persistent instance passed to replicate()");
				//hum ... should we cascade anyway? throw an exception? fine like it is?
				return;
			}

			IEntityPersister persister = source.GetEntityPersister(@event.EntityName, entity);

			// get the id from the object
			/*if ( persister.isUnsaved(entity, source) ) {
			throw new TransientObjectException("transient instance passed to replicate()");
			}*/
			object id = persister.GetIdentifier(entity);
			if (id == null)
			{
				throw new TransientObjectException("instance with null id passed to replicate()");
			}

			ReplicationMode replicationMode = @event.ReplicationMode;
			object oldVersion;
			if (replicationMode == ReplicationMode.Exception)
			{
				//always do an INSERT, and let it fail by constraint violation
				oldVersion = null;
			}
			else
			{
				//what is the version on the database?
				oldVersion = await (persister.GetCurrentVersionAsync(id, source, cancellationToken)).ConfigureAwait(false);
			}

			if (oldVersion != null)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("found existing row for " + MessageHelper.InfoString(persister, id, source.Factory));
				}

				// HHH-2378
				object realOldVersion = persister.IsVersioned ? oldVersion : null;

				bool canReplicate =
					replicationMode.ShouldOverwriteCurrentVersion(entity, realOldVersion,
					                                              persister.GetVersion(entity),
					                                              persister.VersionType);

				if (canReplicate)
				{
					//will result in a SQL UPDATE:
					await (PerformReplicationAsync(entity, id, realOldVersion, persister, replicationMode, source, cancellationToken)).ConfigureAwait(false);
				}
				else
				{
					//else do nothing (don't even reassociate object!)
					log.Debug("no need to replicate");
				}

				//TODO: would it be better to do a refresh from db?
			}
			else
			{
				// no existing row - do an insert
				if (log.IsDebugEnabled)
				{
					log.Debug("no existing row, replicating new instance " + MessageHelper.InfoString(persister, id, source.Factory));
				}

				bool regenerate = persister.IsIdentifierAssignedByInsert; // prefer re-generation of identity!
				EntityKey key = regenerate ? null : source.GenerateEntityKey(id, persister);

				await (PerformSaveOrReplicateAsync(entity, key, persister, regenerate, replicationMode, source, true, cancellationToken)).ConfigureAwait(false);
			}
		}

		private async Task PerformReplicationAsync(object entity, object id, object version, IEntityPersister persister, ReplicationMode replicationMode, IEventSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (log.IsDebugEnabled)
			{
				log.Debug("replicating changes to " + MessageHelper.InfoString(persister, id, source.Factory));
			}

			await (new OnReplicateVisitor(source, id, entity, true).ProcessAsync(entity, persister, cancellationToken)).ConfigureAwait(false);

			source.PersistenceContext.AddEntity(
				entity, 
				persister.IsMutable ? Status.Loaded : Status.ReadOnly,
				null,
				source.GenerateEntityKey(id, persister),
				version, 
				LockMode.None, 
				true, 
				persister,
				true, 
				false);

			await (CascadeAfterReplicateAsync(entity, persister, replicationMode, source, cancellationToken)).ConfigureAwait(false);
		}

		private async Task CascadeAfterReplicateAsync(object entity, IEntityPersister persister, ReplicationMode replicationMode, IEventSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			source.PersistenceContext.IncrementCascadeLevel();
			try
			{
				await (new Cascade(CascadingAction.Replicate, CascadePoint.AfterUpdate, source).CascadeOnAsync(persister, entity, replicationMode, cancellationToken)).ConfigureAwait(false);
			}
			finally
			{
				source.PersistenceContext.DecrementCascadeLevel();
			}
		}

		protected override Task<bool> SubstituteValuesIfNecessaryAsync(object entity, object id, object[] values, IEntityPersister persister, ISessionImplementor source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<bool>(cancellationToken);
			}
			try
			{
				return Task.FromResult<bool>(SubstituteValuesIfNecessary(entity, id, values, persister, source));
			}
			catch (Exception ex)
			{
				return Task.FromException<bool>(ex);
			}
		}

		protected override async Task<bool> VisitCollectionsBeforeSaveAsync(object entity, object id, object[] values, Type.IType[] types, IEventSource source, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			//TODO: we use two visitors here, inefficient!
			OnReplicateVisitor visitor = new OnReplicateVisitor(source, id, entity, false);
			await (visitor.ProcessEntityPropertyValuesAsync(values, types, cancellationToken)).ConfigureAwait(false);
			return await (base.VisitCollectionsBeforeSaveAsync(entity, id, values, types, source, cancellationToken)).ConfigureAwait(false);
		}
	}
}
