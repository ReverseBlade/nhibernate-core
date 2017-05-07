﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Engine;
using NHibernate.SqlCommand;

namespace NHibernate.Id.Insert
{
	using System.Threading.Tasks;
	/// <summary> 
	/// Responsible for handling delegation relating to variants in how
	/// insert-generated-identifier generator strategies dictate processing:
	/// <ul>
	/// <li>building the sql insert statement</li>
	/// <li>determination of the generated identifier value</li>
	/// </ul> 
	/// </summary>
	public partial interface IInsertGeneratedIdentifierDelegate
	{

		/// <summary> 
		/// Perform the indicated insert SQL statement and determine the identifier value generated. 
		/// </summary>
		/// <param name="insertSQL"> </param>
		/// <param name="session"> </param>
		/// <param name="binder"> </param>
		/// <returns> The generated identifier value. </returns>
		Task<object> PerformInsertAsync(SqlCommandInfo insertSQL, ISessionImplementor session, IBinder binder);
	}
}