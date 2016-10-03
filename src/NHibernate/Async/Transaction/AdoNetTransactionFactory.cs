﻿#if NET_4_5
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Engine.Transaction;
using NHibernate.Exceptions;
using NHibernate.Impl;
using System.Threading.Tasks;

namespace NHibernate.Transaction
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class AdoNetTransactionFactory : ITransactionFactory
	{
		public async Task ExecuteWorkInIsolationAsync(ISessionImplementor session, IIsolatedWork work, bool transacted)
		{
			DbConnection connection = null;
			DbTransaction trans = null;
			// bool wasAutoCommit = false;
			try
			{
				// We make an exception for SQLite and use the session's connection,
				// since SQLite only allows one connection to the database.
				if (session.Factory.Dialect is SQLiteDialect)
					connection = session.Connection;
				else
					connection = await (session.Factory.ConnectionProvider.GetConnectionAsync());
				if (transacted)
				{
					trans = connection.BeginTransaction();
				// TODO NH: a way to read the autocommit state is needed
				//if (TransactionManager.GetAutoCommit(connection))
				//{
				//  wasAutoCommit = true;
				//  TransactionManager.SetAutoCommit(connection, false);
				//}
				}

				await (work.DoWorkAsync(connection, trans));
				if (transacted)
				{
					trans.Commit();
				//TransactionManager.Commit(connection);
				}
			}
			catch (Exception t)
			{
				using (new SessionIdLoggingContext(session.SessionId))
				{
					try
					{
						if (trans != null && connection.State != ConnectionState.Closed)
						{
							trans.Rollback();
						}
					}
					catch (Exception ignore)
					{
						isolaterLog.Debug("unable to release connection on exception [" + ignore + "]");
					}

					if (t is HibernateException)
					{
						throw;
					}
					else if (t is DbException)
					{
						throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, t, "error performing isolated work");
					}
					else
					{
						throw new HibernateException("error performing isolated work", t);
					}
				}
			}
			finally
			{
				//if (transacted && wasAutoCommit)
				//{
				//  try
				//  {
				//    // TODO NH: reset autocommit
				//    // TransactionManager.SetAutoCommit(connection, true);
				//  }
				//  catch (Exception)
				//  {
				//    log.Debug("was unable to reset connection back to auto-commit");
				//  }
				//}
				if (session.Factory.Dialect is SQLiteDialect == false)
					session.Factory.ConnectionProvider.CloseConnection(connection);
			}
		}
	}
}
#endif
