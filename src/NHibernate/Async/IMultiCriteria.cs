﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace NHibernate
{
	using System.Threading.Tasks;
	/// <summary>
	/// Combines several queries into a single DB call
	/// </summary>
	public partial interface IMultiCriteria
	{
		/// <summary>
		/// Get all the results
		/// </summary>
		Task<IList> ListAsync();

		/// <summary>
		/// Returns the result of one of the Criteria based on the key
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns></returns>
		Task<object> GetResultAsync(string key);
	}
}