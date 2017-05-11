﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Impl;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace NHibernate.Id.Insert
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public abstract partial class AbstractSelectingDelegate : IInsertGeneratedIdentifierDelegate
	{

		#region IInsertGeneratedIdentifierDelegate Members

		public async Task<object> PerformInsertAsync(SqlCommandInfo insertSQL, ISessionImplementor session, IBinder binder, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				// prepare and execute the insert
				var insert = await (session.Batcher.PrepareCommandAsync(insertSQL.CommandType, insertSQL.Text, insertSQL.ParameterTypes, cancellationToken)).ConfigureAwait(false);
				try
				{
					binder.BindValues(insert);
					await (session.Batcher.ExecuteNonQueryAsync(insert, cancellationToken)).ConfigureAwait(false);
				}
				finally
				{
					session.Batcher.CloseCommand(insert, null);
				}
			}
			catch (DbException sqle)
			{
				throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, sqle,
				                                 "could not insert: " + persister.GetInfoString(), insertSQL.Text);
			}

			SqlString selectSQL = SelectSQL;
			using (new SessionIdLoggingContext(session.SessionId)) 
			try
			{
				//fetch the generated id in a separate query
				var idSelect = await (session.Batcher.PrepareCommandAsync(CommandType.Text, selectSQL, ParametersTypes, cancellationToken)).ConfigureAwait(false);
				try
				{
					BindParameters(session, idSelect, binder.Entity);
					var rs = await (session.Batcher.ExecuteReaderAsync(idSelect, cancellationToken)).ConfigureAwait(false);
					try
					{
						return await (GetResultAsync(session, rs, binder.Entity, cancellationToken)).ConfigureAwait(false);
					}
					finally
					{
						session.Batcher.CloseReader(rs);
					}
				}
				finally
				{
					session.Batcher.CloseCommand(idSelect, null);
				}
			}
			catch (DbException sqle)
			{
				throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, sqle,
				                                 "could not retrieve generated id after insert: " + persister.GetInfoString(),
				                                 insertSQL.Text);
			}
		}

		#endregion

		/// <summary> Extract the generated key value from the given result set. </summary>
		/// <param name="session">The session </param>
		/// <param name="rs">The result set containing the generated primary key values. </param>
		/// <param name="entity">The entity being saved. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns> The generated identifier </returns>
		protected internal abstract Task<object> GetResultAsync(ISessionImplementor session, DbDataReader rs, object entity, CancellationToken cancellationToken = default(CancellationToken));

		#region NH Specific

		#endregion
	}
}