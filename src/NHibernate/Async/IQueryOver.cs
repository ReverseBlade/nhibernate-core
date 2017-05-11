﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------



using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using NHibernate.Criterion;
using NHibernate.Criterion.Lambda;
using NHibernate.SqlCommand;
using NHibernate.Transform;

namespace NHibernate
{
	using System.Threading.Tasks;
	using System.Threading;

	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial interface IQueryOver<TRoot> : IQueryOver
	{
		/// <summary>
		/// Get the results of the root type and fill the <see cref="IList&lt;T&gt;"/>
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>The list filled with the results.</returns>
		Task<IList<TRoot>> ListAsync(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Get the results of the root type and fill the <see cref="IList&lt;T&gt;"/>
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>The list filled with the results.</returns>
		Task<IList<U>> ListAsync<U>(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Short for ToRowCountQuery().SingleOrDefault&lt;int&gt;()
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task<int> RowCountAsync(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Short for ToRowCountInt64Query().SingleOrDefault&lt;long&gt;()
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task<long> RowCountInt64Async(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Convenience method to return a single instance that matches
		/// the query, or null if the query returns no results.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>the single result or <see langword="null" /></returns>
		/// <exception cref="HibernateException">
		/// If there is more than one matching result
		/// </exception>
		Task<TRoot> SingleOrDefaultAsync(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Override type of <see cref="SingleOrDefault()" />.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		Task<U> SingleOrDefaultAsync<U>(CancellationToken cancellationToken = default(CancellationToken));

	}

}
