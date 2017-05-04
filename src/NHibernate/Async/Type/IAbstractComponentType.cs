﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Reflection;
using NHibernate.Engine;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	/// <summary>
	/// Enables other Component-like types to hold collections and have cascades, etc.
	/// </summary>
	public partial interface IAbstractComponentType : IType
	{

		/// <summary>
		/// Get the values of the component properties of 
		/// a component instance
		/// </summary>
		Task<object[]> GetPropertyValuesAsync(object component, ISessionImplementor session);

		Task<object> GetPropertyValueAsync(object component, int i, ISessionImplementor session);
	}
}