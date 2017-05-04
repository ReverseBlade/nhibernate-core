﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;

using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.Util;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace NHibernate.Id
{
	using System.Threading.Tasks;
	/// <summary>
	/// An <see cref="IIdentifierGenerator" /> that generates <c>Int64</c> values using an 
	/// oracle-style sequence. A higher performance algorithm is 
	/// <see cref="SequenceHiLoGenerator"/>.
	/// </summary>
	/// <remarks>
	/// <p>
	///	This id generation strategy is specified in the mapping file as 
	///	<code>
	///	&lt;generator class="sequence"&gt;
	///		&lt;param name="sequence"&gt;uid_sequence&lt;/param&gt;
	///		&lt;param name="schema"&gt;db_schema&lt;/param&gt;
	///	&lt;/generator&gt;
	///	</code>
	/// </p>
	/// <p>
	/// The <c>sequence</c> parameter is required while the <c>schema</c> is optional.
	/// </p>
	/// </remarks>
	public partial class SequenceGenerator : IPersistentIdentifierGenerator, IConfigurable
	{

		#region IConfigurable Members

		#endregion

		#region IIdentifierGenerator Members

		/// <summary>
		/// Generate an <see cref="Int16"/>, <see cref="Int32"/>, or <see cref="Int64"/> 
		/// for the identifier by using a database sequence.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> this id is being generated in.</param>
		/// <param name="obj">The entity for which the id is being generated.</param>
		/// <returns>The new identifier as a <see cref="Int16"/>, <see cref="Int32"/>, or <see cref="Int64"/>.</returns>
		public virtual async Task<object> GenerateAsync(ISessionImplementor session, object obj)
		{
			try
			{
				var cmd = await (session.Batcher.PrepareCommandAsync(CommandType.Text, sql, SqlTypeFactory.NoTypes)).ConfigureAwait(false);
				DbDataReader reader = null;
				try
				{
					reader = await (session.Batcher.ExecuteReaderAsync(cmd)).ConfigureAwait(false);
					try
					{
						await (reader.ReadAsync()).ConfigureAwait(false);
						object result = await (IdentifierGeneratorFactory.GetAsync(reader, identifierType, session)).ConfigureAwait(false);
						if (log.IsDebugEnabled)
						{
							log.Debug("Sequence identifier generated: " + result);
						}
						return result;
					}
					finally
					{
						reader.Close();
					}
				}
				finally
				{
					session.Batcher.CloseCommand(cmd, reader);
				}
			}
			catch (DbException sqle)
			{
				log.Error("error generating sequence", sqle);
				throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, sqle, "could not get next sequence value");
			}
		}

		#endregion
		#region IPersistentIdentifierGenerator Members

		#endregion
	}
}
