﻿#if NET_4_5
using System;
using NHibernate.Cache;
using NHibernate.Cache.Access;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Persister.Entity;
using Status = NHibernate.Engine.Status;
using System.Threading.Tasks;

namespace NHibernate.Event.Default
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class AbstractLockUpgradeEventListener : AbstractReassociateEventListener
	{
		/// <summary> 
		/// Performs a pessimistic lock upgrade on a given entity, if needed. 
		/// </summary>
		/// <param name = "entity">The entity for which to upgrade the lock.</param>
		/// <param name = "entry">The entity's EntityEntry instance.</param>
		/// <param name = "requestedLockMode">The lock mode being requested for locking. </param>
		/// <param name = "source">The session which is the source of the event being processed.</param>
		protected virtual async Task UpgradeLockAsync(object entity, EntityEntry entry, LockMode requestedLockMode, ISessionImplementor source)
		{
			if (requestedLockMode.GreaterThan(entry.LockMode))
			{
				// The user requested a "greater" (i.e. more restrictive) form of
				// pessimistic lock
				if (entry.Status != Status.Loaded)
				{
					throw new ObjectDeletedException("attempted to lock a deleted instance", entry.Id, entry.EntityName);
				}

				IEntityPersister persister = entry.Persister;
				if (log.IsDebugEnabled)
				{
					log.Debug(string.Format("locking {0} in mode: {1}", MessageHelper.InfoString(persister, entry.Id, source.Factory), requestedLockMode));
				}

				ISoftLock slock;
				CacheKey ck;
				if (persister.HasCache)
				{
					ck = source.GenerateCacheKey(entry.Id, persister.IdentifierType, persister.RootEntityName);
					slock = persister.Cache.Lock(ck, entry.Version);
				}
				else
				{
					ck = null;
					slock = null;
				}

				try
				{
					if (persister.IsVersioned && requestedLockMode == LockMode.Force)
					{
						// todo : should we check the current isolation mode explicitly?
						object nextVersion = await (persister.ForceVersionIncrementAsync(entry.Id, entry.Version, source));
						entry.ForceLocked(entity, nextVersion);
					}
					else
					{
						await (persister.LockAsync(entry.Id, entry.Version, entity, requestedLockMode, source));
					}

					entry.LockMode = requestedLockMode;
				}
				finally
				{
					// the database now holds a lock + the object is flushed from the cache,
					// so release the soft lock
					if (persister.HasCache)
					{
						persister.Cache.Release(ck, slock);
					}
				}
			}
		}
	}
}
#endif
