﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Engine;

namespace NHibernate.Loader.Entity
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial interface IUniqueEntityLoader
	{
		/// <summary>
		/// Load an entity instance. If <c>OptionalObject</c> is supplied, load the entity
		/// state into the given (uninitialized) object
		/// </summary>
		Task<object> LoadAsync(object id, object optionalObject, ISessionImplementor session, CancellationToken cancellationToken = default(CancellationToken));
	}
}