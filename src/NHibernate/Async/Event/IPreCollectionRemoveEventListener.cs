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
	/// <summary> Called before removing a collection </summary>
	public partial interface IPreCollectionRemoveEventListener
	{
		Task OnPreRemoveCollectionAsync(PreCollectionRemoveEvent @event);
	}
}