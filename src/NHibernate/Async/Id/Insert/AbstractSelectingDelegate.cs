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

		public async Task<object> PerformInsertAsync(SqlCommandInfo insertSql, ISessionImplementor session, IBinder binder, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			// NH-2145: Prevent connection releases between insert and select when we cannot perform
			// them as a single statement. Retrieving id most of the time relies on using the same connection.
			session.ConnectionManager.FlushBeginning();
			try
			{
				try
				{
					// prepare and execute the insert
					var insert = await (session.Batcher.PrepareCommandAsync(insertSql.CommandType, insertSql.Text, insertSql.ParameterTypes, cancellationToken)).ConfigureAwait(false);
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
					                                 "could not insert: " + persister.GetInfoString(), insertSql.Text);
				}

				var selectSql = SelectSQL;
				using (new SessionIdLoggingContext(session.SessionId))
				{
					try
					{
						//fetch the generated id in a separate query
						var idSelect = await (session.Batcher.PrepareCommandAsync(CommandType.Text, selectSql, ParametersTypes, cancellationToken)).ConfigureAwait(false);
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
						                                 insertSql.Text);
					}
				}
			}
			finally
			{
				session.ConnectionManager.FlushEnding();
			}
		}

		#endregion

		/// <summary> Extract the generated key value from the given result set. </summary>
		/// <param name="session">The session </param>
		/// <param name="rs">The result set containing the generated primary key values. </param>
		/// <param name="entity">The entity being saved. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns> The generated identifier </returns>
		protected internal abstract Task<object> GetResultAsync(ISessionImplementor session, DbDataReader rs, object entity, CancellationToken cancellationToken);
	}
}