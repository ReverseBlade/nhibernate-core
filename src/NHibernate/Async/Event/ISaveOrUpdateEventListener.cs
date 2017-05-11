﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace NHibernate.Event
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial interface ISaveOrUpdateEventListener
	{
		/// <summary> Handle the given update event. </summary>
		/// <param name="event">The update event to be handled.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task OnSaveOrUpdateAsync(SaveOrUpdateEvent @event, CancellationToken cancellationToken = default(CancellationToken));
	}
}