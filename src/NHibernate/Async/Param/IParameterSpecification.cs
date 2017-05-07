﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;

namespace NHibernate.Param
{
	using System.Threading.Tasks;
	/// <summary>
	/// Maintains information relating to parameters which need to get bound into a <see cref="DbCommand"/>.
	/// </summary>
	public partial interface IParameterSpecification
	{
		/// <summary>
		/// Bind the appropriate value into the given command.
		/// </summary>
		/// <param name="command">The command into which the value should be bound.</param>
		/// <param name="sqlQueryParametersList">The list of Sql query parameter in the exact sequence they are present in the query.</param>
		/// <param name="queryParameters">The defined values for the current query execution.</param>
		/// <param name="session">The session against which the current execution is occuring.</param>
		Task BindAsync(DbCommand command, IList<Parameter> sqlQueryParametersList, QueryParameters queryParameters, ISessionImplementor session);
	}
}