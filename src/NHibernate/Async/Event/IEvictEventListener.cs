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
	/// <summary> Defines the contract for handling of evict events generated from a session. </summary>
	public partial interface IEvictEventListener
	{
		/// <summary> Handle the given evict event. </summary>
		/// <param name="event">The evict event to be handled.</param>
		Task OnEvictAsync(EvictEvent @event);
	}
}