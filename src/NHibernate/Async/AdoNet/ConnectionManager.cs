﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Security;

using NHibernate.Engine;

namespace NHibernate.AdoNet
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class ConnectionManager : ISerializable, IDeserializationCallback
	{

		public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (connection == null)
			{
				if (ownConnection)
				{
					connection = await (Factory.ConnectionProvider.GetConnectionAsync(cancellationToken)).ConfigureAwait(false);
					if (Factory.Statistics.IsStatisticsEnabled)
					{
						Factory.StatisticsImplementor.Connect();
					}
				}
				else if (session.IsOpen)
				{
					throw new HibernateException("Session is currently disconnected");
				}
				else
				{
					throw new HibernateException("Session is closed");
				}
			}
			return connection;
		}

		public async Task<DbCommand> CreateCommandAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var result = (await (GetConnectionAsync(cancellationToken)).ConfigureAwait(false)).CreateCommand();
			Transaction.Enlist(result);
			return result;
		}
	}
}
