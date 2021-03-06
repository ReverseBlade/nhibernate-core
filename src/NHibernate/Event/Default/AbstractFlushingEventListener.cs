using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NHibernate.Action;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Persister.Entity;
using NHibernate.Util;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace NHibernate.Event.Default
{
	/// <summary>
	/// A convenience base class for listeners whose functionality results in flushing.
	/// </summary>
	[Serializable]
	public abstract class AbstractFlushingEventListener
	{
		private static readonly IInternalLogger log = LoggerProvider.LoggerFor(typeof (AbstractFlushingEventListener));

		protected virtual object Anything
		{
			get { return null; }
		}

		protected virtual CascadingAction CascadingAction
		{
			get { return CascadingAction.SaveUpdate; }
		}

		/// <summary> 
		/// Coordinates the processing necessary to get things ready for executions
		/// as db calls by preparing the session caches and moving the appropriate
		/// entities and collections to their respective execution queues. 
		/// </summary>
		/// <param name="event">The flush event.</param>
		protected virtual async Task FlushEverythingToExecutions(FlushEvent @event)
		{
			log.Debug("flushing session");

			IEventSource session = @event.Session;
			IPersistenceContext persistenceContext = session.PersistenceContext;

			session.Interceptor.PreFlush((ICollection) persistenceContext.EntitiesByKey.Values);

			await PrepareEntityFlushes(session).ConfigureAwait(false);
			// we could move this inside if we wanted to
			// tolerate collection initializations during
			// collection dirty checking:
			await PrepareCollectionFlushes(session).ConfigureAwait(false);
			// now, any collections that are initialized
			// inside this block do not get updated - they
			// are ignored until the next flush

			persistenceContext.Flushing = true;
			try
			{
				await FlushEntities(@event).ConfigureAwait(false);
				await FlushCollections(session).ConfigureAwait(false);
			}
			finally
			{
				persistenceContext.Flushing = false;
			}

			//some statistics
			if (log.IsDebugEnabled)
			{
				StringBuilder sb = new StringBuilder(100);

				sb.Append("Flushed: ").Append(session.ActionQueue.InsertionsCount).Append(" insertions, ").Append(
					session.ActionQueue.UpdatesCount).Append(" updates, ").Append(session.ActionQueue.DeletionsCount).Append(
					" deletions to ").Append(persistenceContext.EntityEntries.Count).Append(" objects");
				log.Debug(sb.ToString());
				sb = new StringBuilder(100);
				sb.Append("Flushed: ").Append(session.ActionQueue.CollectionCreationsCount).Append(" (re)creations, ").Append(
					session.ActionQueue.CollectionUpdatesCount).Append(" updates, ").Append(session.ActionQueue.CollectionRemovalsCount)
					.Append(" removals to ").Append(persistenceContext.CollectionEntries.Count).Append(" collections");

				log.Debug(sb.ToString());
				new Printer(session.Factory).ToString(persistenceContext.EntitiesByKey.Values.ToArray().GetEnumerator(), session.EntityMode);
			}
		}

		protected virtual async Task FlushCollections(IEventSource session)
		{
			log.Debug("Processing unreferenced collections");

			ICollection list = IdentityMap.Entries(session.PersistenceContext.CollectionEntries);
			foreach (DictionaryEntry me in list)
			{
				CollectionEntry ce = (CollectionEntry) me.Value;
				if (!ce.IsReached && !ce.IsIgnore)
				{
					await Collections.ProcessUnreachableCollection((IPersistentCollection) me.Key, session).ConfigureAwait(false);
				}
			}

			// Schedule updates to collections:

			log.Debug("Scheduling collection removes/(re)creates/updates");

			list = IdentityMap.Entries(session.PersistenceContext.CollectionEntries);
			ActionQueue actionQueue = session.ActionQueue;
			foreach (DictionaryEntry me in list)
			{
				IPersistentCollection coll = (IPersistentCollection) me.Key;
				CollectionEntry ce = (CollectionEntry) me.Value;

				if (ce.IsDorecreate)
				{
					session.Interceptor.OnCollectionRecreate(coll, ce.CurrentKey);
					actionQueue.AddAction(new CollectionRecreateAction(coll, ce.CurrentPersister, ce.CurrentKey, session));
				}
				if (ce.IsDoremove)
				{
					session.Interceptor.OnCollectionRemove(coll, ce.LoadedKey);
					actionQueue.AddAction(
						new CollectionRemoveAction(coll, ce.LoadedPersister, ce.LoadedKey, ce.IsSnapshotEmpty(coll), session));
				}
				if (ce.IsDoupdate)
				{
					session.Interceptor.OnCollectionUpdate(coll, ce.LoadedKey);
					actionQueue.AddAction(
						new CollectionUpdateAction(coll, ce.LoadedPersister, ce.LoadedKey, ce.IsSnapshotEmpty(coll), session));
				}
			}
			actionQueue.SortCollectionActions();
		}

		// 1. detect any dirty entities
		// 2. schedule any entity updates
		// 3. search out any reachable collections
		protected virtual async Task FlushEntities(FlushEvent @event)
		{
			log.Debug("Flushing entities and processing referenced collections");

			// Among other things, updateReachables() will recursively load all
			// collections that are moving roles. This might cause entities to
			// be loaded.

			// So this needs to be safe from concurrent modification problems.
			// It is safe because of how IdentityMap implements entrySet()
			IEventSource source = @event.Session;

			ICollection list = IdentityMap.ConcurrentEntries(source.PersistenceContext.EntityEntries);
			foreach (DictionaryEntry me in list)
			{
				// Update the status of the object and if necessary, schedule an update
				EntityEntry entry = (EntityEntry) me.Value;
				Status status = entry.Status;

				if (status != Status.Loading && status != Status.Gone)
				{
					FlushEntityEvent entityEvent = new FlushEntityEvent(source, me.Key, entry);
					IFlushEntityEventListener[] listeners = source.Listeners.FlushEntityEventListeners;
					foreach (IFlushEntityEventListener listener in listeners)
					{
						await listener.OnFlushEntity(entityEvent).ConfigureAwait(false);
					}
				}
			}
			source.ActionQueue.SortActions();
		}

		// Initialize the flags of the CollectionEntry, including the dirty check.
		protected virtual async Task PrepareCollectionFlushes(ISessionImplementor session)
		{
			// Initialize dirty flags for arrays + collections with composite elements
			// and reset reached, doupdate, etc.
			log.Debug("dirty checking collections");

			ICollection list = IdentityMap.Entries(session.PersistenceContext.CollectionEntries);
			foreach (DictionaryEntry entry in list)
			{
				await ((CollectionEntry) entry.Value).PreFlush((IPersistentCollection) entry.Key).ConfigureAwait(false);
			}
		}

		//process cascade save/update at the start of a flush to discover
		//any newly referenced entity that must be passed to saveOrUpdate(),
		//and also apply orphan delete
		protected virtual async Task PrepareEntityFlushes(IEventSource session)
		{
			log.Debug("processing flush-time cascades");

			ICollection list = IdentityMap.ConcurrentEntries(session.PersistenceContext.EntityEntries);
			//safe from concurrent modification because of how entryList() is implemented on IdentityMap
			foreach (DictionaryEntry me in list)
			{
				EntityEntry entry = (EntityEntry) me.Value;
				Status status = entry.Status;
				if (status == Status.Loaded || status == Status.Saving || status == Status.ReadOnly)
				{
					await CascadeOnFlush(session, entry.Persister, me.Key, Anything).ConfigureAwait(false);
				}
			}
		}

		protected virtual async Task CascadeOnFlush(IEventSource session, IEntityPersister persister, object key, object anything)
		{
			session.PersistenceContext.IncrementCascadeLevel();
			try
			{
				await new Cascade(CascadingAction, CascadePoint.BeforeFlush, session).CascadeOn(persister, key, anything).ConfigureAwait(false);
			}
			finally
			{
				session.PersistenceContext.DecrementCascadeLevel();
			}
		}

		/// <summary> 
		/// Execute all SQL and second-level cache updates, in a
		/// special order so that foreign-key constraints cannot
		/// be violated:
		/// <list type="bullet">
		/// <item> <description>Inserts, in the order they were performed</description> </item>
		/// <item> <description>Updates</description> </item>
		/// <item> <description>Deletion of collection elements</description> </item>
		/// <item> <description>Insertion of collection elements</description> </item>
		/// <item> <description>Deletes, in the order they were performed</description> </item>
		/// </list>
		/// </summary>
		/// <param name="session">The session being flushed</param>
		protected virtual async Task PerformExecutions(IEventSource session)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("executing flush");
			}

			try
			{
				session.ConnectionManager.FlushBeginning();
				// IMPL NOTE : here we alter the flushing flag of the persistence context to allow
				//		during-flush callbacks more leniency in regards to initializing proxies and
				//		lazy collections during their processing.
				// For more information, see HHH-2763 / NH-1882
				session.PersistenceContext.Flushing = true;
				// we need to lock the collection caches before
				// executing entity inserts/updates in order to
				// account for bidi associations
				session.ActionQueue.PrepareActions();
				await session.ActionQueue.ExecuteActions().ConfigureAwait(false);
			}
			catch (HibernateException he)
			{
				if (log.IsErrorEnabled)
				{
					log.Error("Could not synchronize database state with session", he);
				}
				throw;
			}
			finally
			{
				session.PersistenceContext.Flushing = false;
				session.ConnectionManager.FlushEnding();
			}
		}

		/// <summary> 
		/// 1. Recreate the collection key -> collection map
		/// 2. rebuild the collection entries
		/// 3. call Interceptor.postFlush()
		/// </summary>
		protected virtual void PostFlush(ISessionImplementor session)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("post flush");
			}

			IPersistenceContext persistenceContext = session.PersistenceContext;
			persistenceContext.CollectionsByKey.Clear();
			persistenceContext.BatchFetchQueue.ClearSubselects();
			//the database has changed now, so the subselect results need to be invalidated

			// NH Different implementation: In NET an iterator is immutable;
			// we need something to hold the persistent collection to remove, and it must be less intrusive as possible
			IDictionary cEntries = persistenceContext.CollectionEntries;
			List<IPersistentCollection> keysToRemove = new List<IPersistentCollection>(cEntries.Count);
			foreach (DictionaryEntry me in cEntries)
			{
				CollectionEntry collectionEntry = (CollectionEntry) me.Value;
				IPersistentCollection persistentCollection = (IPersistentCollection) me.Key;
				collectionEntry.PostFlush(persistentCollection);
				if (collectionEntry.LoadedPersister == null)
				{
					keysToRemove.Add(persistentCollection);
				}
				else
				{
					//otherwise recreate the mapping between the collection and its key
					CollectionKey collectionKey =
						new CollectionKey(collectionEntry.LoadedPersister, collectionEntry.LoadedKey, session.EntityMode);
					persistenceContext.CollectionsByKey[collectionKey] = persistentCollection;
				}
			}
			foreach (IPersistentCollection key in keysToRemove)
			{
				persistenceContext.CollectionEntries.Remove(key);
			}
			session.Interceptor.PostFlush((ICollection) persistenceContext.EntitiesByKey.Values);
		}
	}
}
