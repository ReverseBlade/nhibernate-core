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
	/// Called after updating the datastore
	/// </summary>
	public partial interface IPostUpdateEventListener
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="event"></param>
		Task OnPostUpdateAsync(PostUpdateEvent @event);
	}
}