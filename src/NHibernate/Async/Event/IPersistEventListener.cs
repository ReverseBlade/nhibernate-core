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
	/// Defines the contract for handling of create events generated from a session.
	/// </summary>
	public partial interface IPersistEventListener
	{
		/// <summary> Handle the given create event.</summary>
		/// <param name="event">The create event to be handled.</param>
		Task OnPersistAsync(PersistEvent @event);

		/// <summary> Handle the given create event. </summary>
		/// <param name="event">The create event to be handled.</param>
		/// <param name="createdAlready"></param>
		Task OnPersistAsync(PersistEvent @event, IDictionary createdAlready);
	}
}