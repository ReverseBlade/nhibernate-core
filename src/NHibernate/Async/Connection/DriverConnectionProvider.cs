﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Data.Common;

namespace NHibernate.Connection
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class DriverConnectionProvider : ConnectionProvider
	{

		/// <summary>
		/// Gets a new open <see cref="DbConnection"/> through 
		/// the <see cref="NHibernate.Driver.IDriver"/>.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>
		/// An Open <see cref="DbConnection"/>.
		/// </returns>
		/// <exception cref="Exception">
		/// If there is any problem creating or opening the <see cref="DbConnection"/>.
		/// </exception>
		public override async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			log.Debug("Obtaining DbConnection from Driver");
			var conn = Driver.CreateConnection();
			try
			{
				conn.ConnectionString = ConnectionString;
				await (conn.OpenAsync(cancellationToken)).ConfigureAwait(false);
			}
			catch (Exception)
			{
				conn.Dispose();
				throw;
			}
			
			return conn;
		}
	}
}