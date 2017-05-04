﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Mapping;
using System.Collections.Generic;

namespace NHibernate.Tool.hbm2ddl
{
	using System.Threading.Tasks;
	using System;
	// Candidate to be exstensions of ISessionFactory and Configuration
	public static partial class SchemaMetadataUpdater
	{
		public static async Task UpdateAsync(ISessionFactory sessionFactory)
		{
			var factory = (ISessionFactoryImplementor) sessionFactory;
			var dialect = factory.Dialect;
			var connectionHelper = new SuppliedConnectionProviderConnectionHelper(factory.ConnectionProvider);
			factory.Dialect.Keywords.UnionWith(await (GetReservedWordsAsync(dialect, connectionHelper)).ConfigureAwait(false));
		}

		public static async Task QuoteTableAndColumnsAsync(Configuration configuration)
		{
			ISet<string> reservedDb = await (GetReservedWordsAsync(configuration.GetDerivedProperties())).ConfigureAwait(false);
			foreach (var cm in configuration.ClassMappings)
			{
				QuoteTable(cm.Table, reservedDb);
			}
			foreach (var cm in configuration.CollectionMappings)
			{
				QuoteTable(cm.Table, reservedDb);
			}
		}

		private static Task<ISet<string>> GetReservedWordsAsync(IDictionary<string, string> cfgProperties)
		{
			try
			{
				var dialect = Dialect.Dialect.GetDialect(cfgProperties);
				var connectionHelper = new ManagedProviderConnectionHelper(cfgProperties);
				return GetReservedWordsAsync(dialect, connectionHelper);
			}
			catch (Exception ex)
			{
				return Task.FromException<ISet<string>>(ex);
			}
		}

		private static async Task<ISet<string>> GetReservedWordsAsync(Dialect.Dialect dialect, IConnectionHelper connectionHelper)
		{
			ISet<string> reservedDb = new HashSet<string>();
			await (connectionHelper.PrepareAsync()).ConfigureAwait(false);
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				foreach (var rw in metaData.GetReservedWords())
				{
					reservedDb.Add(rw.ToLowerInvariant());
				}
			}
			finally
			{
				connectionHelper.Release();
			}
			return reservedDb;
		}
	}
}