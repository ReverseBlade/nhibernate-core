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
	/// Called before deleting an item from the datastore
	/// </summary>
	public partial interface IPreDeleteEventListener
	{
		/// <summary> Return true if the operation should be vetoed</summary>
		/// <param name="event"></param>
		Task<bool> OnPreDeleteAsync(PreDeleteEvent @event);
	}
}