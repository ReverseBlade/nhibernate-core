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
	/// <summary> Called after recreating a collection </summary>
	public partial interface IPostCollectionRecreateEventListener
	{
		Task OnPostRecreateCollectionAsync(PostCollectionRecreateEvent @event);
	}
}