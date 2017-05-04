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
using System.Configuration;
using System.Data.Common;

using NHibernate.Driver;
using NHibernate.Util;
using Environment=NHibernate.Cfg.Environment;
using System.Collections.Generic;

namespace NHibernate.Connection
{
	using System.Threading.Tasks;
	/// <summary>
	/// The base class for the ConnectionProvider.
	/// </summary>
	public abstract partial class ConnectionProvider : IConnectionProvider
	{

		/// <summary>
		/// Get an open <see cref="DbConnection"/>.
		/// </summary>
		/// <returns>An open <see cref="DbConnection"/>.</returns>
		public abstract Task<DbConnection> GetConnectionAsync();

		#region IDisposable Members

		#endregion
	}
}
