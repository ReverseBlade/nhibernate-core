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
using System.Data.Common;
using System.Linq;
using NHibernate.Engine;
using NHibernate.Param;
using NHibernate.SqlTypes;

namespace NHibernate.SqlCommand
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial interface ISqlCommand
	{

		/// <summary>
		/// Bind the appropriate value into the given command.
		/// </summary>
		/// <param name="command">The command into which the value should be bound.</param>
		/// <param name="session">The session against which the current execution is occurring.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <remarks>
		/// Use this method when the <paramref name="command"/> contains just 'this' instance of <see cref="ISqlCommand"/>.
		/// Use the overload <see cref="Bind(DbCommand, IList{Parameter}, int, ISessionImplementor)"/> when the <paramref name="command"/> contains more instances of <see cref="ISqlCommand"/>.
		/// </remarks>
		Task BindAsync(DbCommand command, ISessionImplementor session, CancellationToken cancellationToken = default(CancellationToken));
	}

	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class SqlCommandImpl : ISqlCommand
	{

		/// <summary>
		/// Bind the appropriate value into the given command.
		/// </summary>
		/// <param name="command">The command into which the value should be bound.</param>
		/// <param name="session">The session against which the current execution is occuring.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <remarks>
		/// Use this method when the <paramref name="command"/> contains just 'this' instance of <see cref="ISqlCommand"/>.
		/// Use the overload <see cref="Bind(DbCommand, IList{Parameter}, int, ISessionImplementor)"/> when the <paramref name="command"/> contains more instances of <see cref="ISqlCommand"/>.
		/// </remarks>
		public async Task BindAsync(DbCommand command, ISessionImplementor session, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (IParameterSpecification parameterSpecification in Specifications)
			{
				await (parameterSpecification.BindAsync(command, SqlQueryParametersList, QueryParameters, session, cancellationToken)).ConfigureAwait(false);
			}
		}
	}
}