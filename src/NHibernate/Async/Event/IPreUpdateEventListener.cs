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
	/// <summary>
	/// Called before updating the datastore
	/// </summary>
	public partial interface IPreUpdateEventListener
	{
		/// <summary> Return true if the operation should be vetoed</summary>
		/// <param name="event"></param>
		Task<bool> OnPreUpdateAsync(PreUpdateEvent @event);
	}
}