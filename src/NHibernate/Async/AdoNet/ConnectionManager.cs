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
using System.Security.Permissions;

using NHibernate.Engine;

namespace NHibernate.AdoNet
{
	using System.Threading.Tasks;
	public partial class ConnectionManager : ISerializable, IDeserializationCallback
	{

		public async Task<DbConnection> GetConnectionAsync()
		{
			if (connection == null)
			{
				if (ownConnection)
				{
					connection = await (Factory.ConnectionProvider.GetConnectionAsync()).ConfigureAwait(false);
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

		#region Serialization

		#endregion
		#region IDeserializationCallback Members

		#endregion

		public async Task<DbCommand> CreateCommandAsync()
		{
			var result = (await (GetConnectionAsync()).ConfigureAwait(false)).CreateCommand();
			Transaction.Enlist(result);
			return result;
		}
	}
}