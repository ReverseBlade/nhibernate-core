using System;
using System.Collections.Generic;
using System.Data;

using NHibernate.Engine;
using NHibernate.Impl;
using System.Threading.Tasks;

namespace NHibernate.Transaction
{
	/// <summary>
	/// Wraps an ADO.NET <see cref="IDbTransaction"/> to implement
	/// the <see cref="ITransaction" /> interface.
	/// </summary>
	public class AdoTransaction : ITransaction
	{
		private static readonly IInternalLogger log = LoggerProvider.LoggerFor(typeof(AdoTransaction));
		private ISessionImplementor session;
		private IDbTransaction trans;
		private bool begun;
		private bool committed;
		private bool rolledBack;
		private bool commitFailed;
		private IList<ISynchronization> synchronizations;

		/// <summary>
		/// Initializes a new instance of the <see cref="AdoTransaction"/> class.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> the Transaction is for.</param>
		public AdoTransaction(ISessionImplementor session)
		{
			this.session = session;
			sessionId = this.session.SessionId;
		}

		/// <summary>
		/// Enlist the <see cref="IDbCommand"/> in the current <see cref="ITransaction"/>.
		/// </summary>
		/// <param name="command">The <see cref="IDbCommand"/> to enlist in this Transaction.</param>
		/// <remarks>
		/// <para>
		/// This takes care of making sure the <see cref="IDbCommand"/>'s Transaction property 
		/// contains the correct <see cref="IDbTransaction"/> or <see langword="null" /> if there is no
		/// Transaction for the ISession - ie <c>BeginTransaction()</c> not called.
		/// </para>
		/// <para>
		/// This method may be called even when the transaction is disposed.
		/// </para>
		/// </remarks>
		public void Enlist(IDbCommand command)
		{
			if (trans == null)
			{
				if (log.IsWarnEnabled)
				{
					if (command.Transaction != null)
					{
						log.Warn("set a nonnull IDbCommand.Transaction to null because the Session had no Transaction");
					}
				}

				command.Transaction = null;
				return;
			}
			else
			{
				if (log.IsWarnEnabled)
				{
					// got into here because the command was being initialized and had a null Transaction - probably
					// don't need to be confused by that - just a normal part of initialization...
					if (command.Transaction != null && command.Transaction != trans)
					{
						log.Warn("The IDbCommand had a different Transaction than the Session.  This can occur when " +
								 "Disconnecting and Reconnecting Sessions because the PreparedCommand Cache is Session specific.");
					}
				}
				log.Debug("Enlist Command");

				// If you try to assign a disposed transaction to a command with MSSQL, it will leave the command's
				// transaction as null and not throw an error.  With SQLite, for example, it will throw an exception
				// here instead.  Because of this, we set the trans field to null in when Dispose is called.
				command.Transaction = trans;
			}
		}

		public void RegisterSynchronization(ISynchronization sync)
		{
			if (sync == null) throw new ArgumentNullException("sync");
			if (synchronizations == null)
			{
				synchronizations = new List<ISynchronization>();
			}
			synchronizations.Add(sync);
		}

		public void Begin()
		{
			BeginAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public Task BeginAsync()
		{
			return BeginAsync(IsolationLevel.Unspecified);
		}

		/// <summary>
		/// Begins the <see cref="IDbTransaction"/> on the <see cref="IDbConnection"/>
		/// used by the <see cref="ISession"/>.
		/// </summary>
		/// <exception cref="TransactionException">
		/// Thrown if there is any problems encountered while trying to create
		/// the <see cref="IDbTransaction"/>.
		/// </exception>
		public void Begin(IsolationLevel isolationLevel)
		{
			BeginAsync(isolationLevel).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Begins the <see cref="IDbTransaction"/> on the <see cref="IDbConnection"/>
		/// used by the <see cref="ISession"/>.
		/// </summary>
		/// <exception cref="TransactionException">
		/// Thrown if there is any problems encountered while trying to create
		/// the <see cref="IDbTransaction"/>.
		/// </exception>
		public async Task BeginAsync(IsolationLevel isolationLevel)
		{
			using (new SessionIdLoggingContext(sessionId))
			{
				if (begun)
				{
					return;
				}

				if (commitFailed)
				{
					throw new TransactionException("Cannot restart transaction after failed commit");
				}

				if (isolationLevel == IsolationLevel.Unspecified)
				{
					isolationLevel = session.Factory.Settings.IsolationLevel;
				}

				log.Debug(string.Format("Begin ({0})", isolationLevel));

				try
				{
					if (isolationLevel == IsolationLevel.Unspecified)
					{
						trans = (await session.GetConnection().ConfigureAwait(false)).BeginTransaction();
					}
					else
					{
						trans = (await session.GetConnection().ConfigureAwait(false)).BeginTransaction(isolationLevel);
					}
				}
				catch (HibernateException)
				{
					// Don't wrap HibernateExceptions
					throw;
				}
				catch (Exception e)
				{
					log.Error("Begin transaction failed", e);
					throw new TransactionException("Begin failed with SQL exception", e);
				}

				begun = true;
				committed = false;
				rolledBack = false;

				session.AfterTransactionBegin(this);
			}
		}

		private async Task AfterTransactionCompletion(bool successful)
		{
			using (new SessionIdLoggingContext(sessionId))
			{
				await session.AfterTransactionCompletion(successful, this).ConfigureAwait(false);
				await NotifyLocalSynchsAfterTransactionCompletion(successful).ConfigureAwait(false);
				session = null;
				begun = false;
			}
		}

		/// <summary>
		/// Commits the <see cref="ITransaction"/> by flushing the <see cref="ISession"/>
		/// and committing the <see cref="IDbTransaction"/>.
		/// </summary>
		/// <exception cref="TransactionException">
		/// Thrown if there is any exception while trying to call <c>Commit()</c> on 
		/// the underlying <see cref="IDbTransaction"/>.
		/// </exception>
		public void Commit()
		{
			this.CommitAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Commits the <see cref="ITransaction"/> by flushing the <see cref="ISession"/>
		/// and committing the <see cref="IDbTransaction"/>.
		/// </summary>
		/// <exception cref="TransactionException">
		/// Thrown if there is any exception while trying to call <c>Commit()</c> on 
		/// the underlying <see cref="IDbTransaction"/>.
		/// </exception>
		public async Task CommitAsync()
		{
			using (new SessionIdLoggingContext(sessionId))
			{
				CheckNotDisposed();
				CheckBegun();
				CheckNotZombied();

				log.Debug("Start Commit");

				if (session.FlushMode != FlushMode.Never)
				{
					await session.FlushAsync().ConfigureAwait(false);
				}

				await NotifyLocalSynchsBeforeTransactionCompletion().ConfigureAwait(false);
				await session.BeforeTransactionCompletion(this).ConfigureAwait(false);

				try
				{
					trans.Commit();
					log.Debug("IDbTransaction Committed");

					committed = true;
					await AfterTransactionCompletion(true).ConfigureAwait(false);
					Dispose();
				}
				catch (HibernateException e)
				{
					log.Error("Commit failed", e);
					await AfterTransactionCompletion(false).ConfigureAwait(false);
					commitFailed = true;
					// Don't wrap HibernateExceptions
					throw;
				}
				catch (Exception e)
				{
					log.Error("Commit failed", e);
					await AfterTransactionCompletion(false).ConfigureAwait(false);
					commitFailed = true;
					throw new TransactionException("Commit failed with SQL exception", e);
				}
				finally
				{
					CloseIfRequired();
				}
			}
		}

		/// <summary>
		/// Rolls back the <see cref="ITransaction"/> by calling the method <c>Rollback</c> 
		/// on the underlying <see cref="IDbTransaction"/>.
		/// </summary>
		/// <exception cref="TransactionException">
		/// Thrown if there is any exception while trying to call <c>Rollback()</c> on 
		/// the underlying <see cref="IDbTransaction"/>.
		/// </exception>
		public void Rollback()
		{
			RollbackAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		/// <summary>
		/// Rolls back the <see cref="ITransaction"/> by calling the method <c>Rollback</c> 
		/// on the underlying <see cref="IDbTransaction"/>.
		/// </summary>
		/// <exception cref="TransactionException">
		/// Thrown if there is any exception while trying to call <c>Rollback()</c> on 
		/// the underlying <see cref="IDbTransaction"/>.
		/// </exception>
		public async Task RollbackAsync()
		{
			using (new SessionIdLoggingContext(sessionId))
			{
				CheckNotDisposed();
				CheckBegun();
				CheckNotZombied();

				log.Debug("Rollback");

				if (!commitFailed)
				{
					try
					{
						trans.Rollback();
						log.Debug("IDbTransaction RolledBack");
						rolledBack = true;
						Dispose();
					}
					catch (HibernateException e)
					{
						log.Error("Rollback failed", e);
						// Don't wrap HibernateExceptions
						throw;
					}
					catch (Exception e)
					{
						log.Error("Rollback failed", e);
						throw new TransactionException("Rollback failed with SQL Exception", e);
					}
					finally
					{
						await AfterTransactionCompletion(false).ConfigureAwait(false);
						CloseIfRequired();
					}
				}
			}
		}

		/// <summary>
		/// Gets a <see cref="Boolean"/> indicating if the transaction was rolled back.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if the <see cref="IDbTransaction"/> had <c>Rollback</c> called
		/// without any exceptions.
		/// </value>
		public bool WasRolledBack
		{
			get { return rolledBack; }
		}

		/// <summary>
		/// Gets a <see cref="Boolean"/> indicating if the transaction was committed.
		/// </summary>
		/// <value>
		/// <see langword="true" /> if the <see cref="IDbTransaction"/> had <c>Commit</c> called
		/// without any exceptions.
		/// </value>
		public bool WasCommitted
		{
			get { return committed; }
		}

		public bool IsActive
		{
			get { return begun && !rolledBack && !committed; }
		}

		public IsolationLevel IsolationLevel
		{
			get { return trans.IsolationLevel; }
		}

		void CloseIfRequired()
		{
			//bool close = session.ShouldAutoClose() && !transactionContext.isClosed();
			//if (close)
			//{
			//    transactionContext.managedClose();
			//}
		}

		#region System.IDisposable Members

		/// <summary>
		/// A flag to indicate if <c>Disose()</c> has been called.
		/// </summary>
		private bool _isAlreadyDisposed;

		private Guid sessionId;

		/// <summary>
		/// Finalizer that ensures the object is correctly disposed of.
		/// </summary>
		~AdoTransaction()
		{
			Dispose(false);
		}

		/// <summary>
		/// Takes care of freeing the managed and unmanaged resources that 
		/// this class is responsible for.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Takes care of freeing the managed and unmanaged resources that 
		/// this class is responsible for.
		/// </summary>
		/// <param name="isDisposing">Indicates if this AdoTransaction is being Disposed of or Finalized.</param>
		/// <remarks>
		/// If this AdoTransaction is being Finalized (<c>isDisposing==false</c>) then make sure not
		/// to call any methods that could potentially bring this AdoTransaction back to life.
		/// </remarks>
		protected virtual void Dispose(bool isDisposing)
		{
			using (new SessionIdLoggingContext(sessionId))
			{
				if (_isAlreadyDisposed)
				{
					// don't dispose of multiple times.
					return;
				}

				// free managed resources that are being managed by the AdoTransaction if we
				// know this call came through Dispose()
				if (isDisposing)
				{
					if (trans != null)
					{
						trans.Dispose();
						trans = null;
						log.Debug("IDbTransaction disposed.");
					}

					if (IsActive && session != null)
					{
						// Assume we are rolled back
						AfterTransactionCompletion(false).ConfigureAwait(false).GetAwaiter().GetResult();
					}
				}

				// free unmanaged resources here

				_isAlreadyDisposed = true;
				// nothing for Finalizer to do - so tell the GC to ignore it
				GC.SuppressFinalize(this);
			}
		}

		#endregion

		private void CheckNotDisposed()
		{
			if (_isAlreadyDisposed)
			{
				throw new ObjectDisposedException("AdoTransaction");
			}
		}

		private void CheckBegun()
		{
			if (!begun)
			{
				throw new TransactionException("Transaction not successfully started");
			}
		}

		private void CheckNotZombied()
		{
			if (trans != null && trans.Connection == null)
			{
				throw new TransactionException("Transaction not connected, or was disconnected");
			}
		}

		private async Task NotifyLocalSynchsBeforeTransactionCompletion()
		{
			if (synchronizations != null)
			{
				for (int i = 0; i < synchronizations.Count; i++)
				{
					ISynchronization sync = synchronizations[i];
					try
					{
						await sync.BeforeCompletion().ConfigureAwait(false);
					}
					catch (Exception e)
					{
						log.Error("exception calling user Synchronization", e);
#pragma warning disable 618
						if (!session.Factory.Settings.IsInterceptorsBeforeTransactionCompletionIgnoreExceptionsEnabled)
							throw;
#pragma warning restore 618
					}
				}
			}
		}

		private async Task NotifyLocalSynchsAfterTransactionCompletion(bool success)
		{
			begun = false;
			if (synchronizations != null)
			{
				for (int i = 0; i < synchronizations.Count; i++)
				{
					ISynchronization sync = synchronizations[i];
					try
					{
						await sync.AfterCompletion(success).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						log.Error("exception calling user Synchronization", e);
					}
				}
			}
		}
	}
}
