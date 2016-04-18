using System;

using NHibernate.Engine;
using System.Threading.Tasks;

namespace NHibernate.Event.Default
{
	/// <summary>
	/// Defines the default flush event listeners used by hibernate for
	/// flushing session state in response to generated auto-flush events.
	/// </summary>
	[Serializable]
	public class DefaultAutoFlushEventListener : AbstractFlushingEventListener, IAutoFlushEventListener
	{
		private static readonly IInternalLogger log = LoggerProvider.LoggerFor(typeof(DefaultAutoFlushEventListener));

		#region IAutoFlushEventListener Members

		/// <summary>
		/// Handle the given auto-flush event.
		/// </summary>
		/// <param name="event">The auto-flush event to be handled.</param>
		/// <param name="async"></param>
		public virtual async Task OnAutoFlush(AutoFlushEvent @event, bool async)
		{
			IEventSource source = @event.Session;

			if (FlushMightBeNeeded(source))
			{
				int oldSize = source.ActionQueue.CollectionRemovalsCount;

				FlushEverythingToExecutions(@event);

				if (FlushIsReallyNeeded(@event, source))
				{
					if (log.IsDebugEnabled)
						log.Debug("Need to execute flush");

					await PerformExecutions(source, async).ConfigureAwait(false);
					PostFlush(source);
					// note: performExecutions() clears all collectionXxxxtion
					// collections (the collection actions) in the session

					if (source.Factory.Statistics.IsStatisticsEnabled)
					{
						source.Factory.StatisticsImplementor.Flush();
					}
				}
				else
				{

					if (log.IsDebugEnabled)
						log.Debug("Dont need to execute flush");
					source.ActionQueue.ClearFromFlushNeededCheck(oldSize);
				}

				@event.FlushRequired = FlushIsReallyNeeded(@event, source);
			}
		}

		#endregion

		private bool FlushIsReallyNeeded(AutoFlushEvent @event, IEventSource source)
		{
			return source.ActionQueue.AreTablesToBeUpdated(@event.QuerySpaces) || ((ISessionImplementor)source).FlushMode == FlushMode.Always;
		}

		private bool FlushMightBeNeeded(ISessionImplementor source)
		{
			return
				!(source.FlushMode < FlushMode.Auto)
				&& source.DontFlushFromFind == 0
				&& ((source.PersistenceContext.EntityEntries.Count > 0) || (source.PersistenceContext.CollectionEntries.Count > 0));
		}
	}
}
