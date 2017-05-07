﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Engine;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	public partial interface ICacheAssembler
	{

		/// <summary> Reconstruct the object from its cached "disassembled" state.</summary>
		/// <param name="cached">the disassembled state from the cache </param>
		/// <param name="session">the session </param>
		/// <param name="owner">the parent entity object </param>
		/// <returns> the the object </returns>
		Task<object> AssembleAsync(object cached, ISessionImplementor session, object owner);

		/// <summary>
		/// Called before assembling a query result set from the query cache, to allow batch fetching
		/// of entities missing from the second-level cache.
		/// </summary>
		Task BeforeAssembleAsync(object cached, ISessionImplementor session);
	}
}