﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;

namespace NHibernate.Event
{
	using System.Threading.Tasks;
	/// <summary>
	/// Defines the contract for handling of merge events generated from a session.
	/// </summary>
	public partial interface IMergeEventListener
	{
		/// <summary> Handle the given merge event. </summary>
		/// <param name="event">The merge event to be handled. </param>
		Task OnMergeAsync(MergeEvent @event);

		/// <summary> Handle the given merge event. </summary>
		/// <param name="event">The merge event to be handled. </param>
		/// <param name="copiedAlready"></param>
		Task OnMergeAsync(MergeEvent @event, IDictionary copiedAlready);
	}
}