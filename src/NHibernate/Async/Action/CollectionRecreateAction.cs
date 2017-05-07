﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Diagnostics;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Persister.Collection;

namespace NHibernate.Action
{
	using System.Threading.Tasks;
	public sealed partial class CollectionRecreateAction : CollectionAction
	{

		/// <summary> Execute this action</summary>
		/// <remarks>
		/// This method is called when a new non-null collection is persisted
		/// or when an existing (non-null) collection is moved to a new owner
		/// </remarks>
		public override async Task ExecuteAsync()
		{
			bool statsEnabled = Session.Factory.Statistics.IsStatisticsEnabled;
			Stopwatch stopwatch = null;
			if (statsEnabled)
			{
				stopwatch = Stopwatch.StartNew();
			}
			IPersistentCollection collection = Collection;

			await (PreRecreateAsync()).ConfigureAwait(false);

			await (Persister.RecreateAsync(collection, Key, Session)).ConfigureAwait(false);

			Session.PersistenceContext.GetCollectionEntry(collection).AfterAction(collection);

			Evict();

			await (PostRecreateAsync()).ConfigureAwait(false);
			if (statsEnabled)
			{
				stopwatch.Stop();
				Session.Factory.StatisticsImplementor.RecreateCollection(Persister.Role, stopwatch.Elapsed);
			}
		}

		private async Task PreRecreateAsync()
		{
			IPreCollectionRecreateEventListener[] preListeners = Session.Listeners.PreCollectionRecreateEventListeners;
			if (preListeners.Length > 0)
			{
				PreCollectionRecreateEvent preEvent = new PreCollectionRecreateEvent(Persister, Collection, (IEventSource)Session);
				for (int i = 0; i < preListeners.Length; i++)
				{
					await (preListeners[i].OnPreRecreateCollectionAsync(preEvent)).ConfigureAwait(false);
				}
			}
		}

		private async Task PostRecreateAsync()
		{
			IPostCollectionRecreateEventListener[] postListeners = Session.Listeners.PostCollectionRecreateEventListeners;
			if (postListeners.Length > 0)
			{
				PostCollectionRecreateEvent postEvent = new PostCollectionRecreateEvent(Persister, Collection, (IEventSource)Session);
				for (int i = 0; i < postListeners.Length; i++)
				{
					await (postListeners[i].OnPostRecreateCollectionAsync(postEvent)).ConfigureAwait(false);
				}
			}
		}
	}
}