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
	public partial interface IPostDeleteEventListener
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="event"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken = default(CancellationToken));
	}
}