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
using System.Collections.Generic;
using System.Diagnostics;
using NHibernate.Cache;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Engine.Query.Sql;
using NHibernate.Exceptions;
using NHibernate.Hql;
using NHibernate.Loader.Custom;
using NHibernate.Loader.Custom.Sql;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Impl
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class MultiQueryImpl : IMultiQuery
	{

		#region Parameters setting

		#endregion

		/// <summary>
		/// Return the query results of all the queries
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public async Task<IList> ListAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			using (new SessionIdLoggingContext(session.SessionId))
			{
				bool cacheable = session.Factory.Settings.IsQueryCacheEnabled && isCacheable;
				combinedParameters = CreateCombinedQueryParameters();

				if (log.IsDebugEnabled)
				{
					log.DebugFormat("Multi query with {0} queries.", queries.Count);
					for (int i = 0; i < queries.Count; i++)
					{
						log.DebugFormat("Query #{0}: {1}", i, queries[i]);
					}
				}

				try
				{
					Before();
					return cacheable ? await (ListUsingQueryCacheAsync(cancellationToken)) .ConfigureAwait(false): await (ListIgnoreQueryCacheAsync(cancellationToken)).ConfigureAwait(false);
				}
				finally
				{
					After();
				}
			}
		}

		protected async Task<List<object>> DoListAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			bool statsEnabled = session.Factory.Statistics.IsStatisticsEnabled;
			var stopWatch = new Stopwatch();
			if (statsEnabled)
			{
				stopWatch.Start();
			}
			int rowCount = 0;

			var results = new List<object>();

			var hydratedObjects = new List<object>[Translators.Count];
			List<EntityKey[]>[] subselectResultKeys = new List<EntityKey[]>[Translators.Count];
			bool[] createSubselects = new bool[Translators.Count];

			try
			{
				using (var reader = await (resultSetsCommand.GetReaderAsync(commandTimeout != RowSelection.NoValue ? commandTimeout : (int?)null, cancellationToken)).ConfigureAwait(false))
				{
					if (log.IsDebugEnabled)
					{
						log.DebugFormat("Executing {0} queries", translators.Count);
					}
					for (int i = 0; i < translators.Count; i++)
					{
						ITranslator translator = Translators[i];
						QueryParameters parameter = Parameters[i];

						int entitySpan = translator.Loader.EntityPersisters.Length;
						hydratedObjects[i] = entitySpan > 0 ? new List<object>() : null;
						RowSelection selection = parameter.RowSelection;
						int maxRows = Loader.Loader.HasMaxRows(selection) ? selection.MaxRows : int.MaxValue;
						if (!dialect.SupportsLimitOffset || !translator.Loader.UseLimit(selection, dialect))
						{
							await (Loader.Loader.AdvanceAsync(reader, selection, cancellationToken)).ConfigureAwait(false);
						}

						if (parameter.HasAutoDiscoverScalarTypes)
						{
							translator.Loader.AutoDiscoverTypes(reader);
						}

						LockMode[] lockModeArray = translator.Loader.GetLockModes(parameter.LockModes);
						EntityKey optionalObjectKey = Loader.Loader.GetOptionalObjectKey(parameter, session);

						createSubselects[i] = translator.Loader.IsSubselectLoadingEnabled;
						subselectResultKeys[i] = createSubselects[i] ? new List<EntityKey[]>() : null;

						translator.Loader.HandleEmptyCollections(parameter.CollectionKeys, reader, session);
						EntityKey[] keys = new EntityKey[entitySpan]; // we can reuse it each time

						if (log.IsDebugEnabled)
						{
							log.Debug("processing result set");
						}

						IList tempResults = new List<object>();
						int count;
						for (count = 0; count < maxRows && await (reader.ReadAsync(cancellationToken)).ConfigureAwait(false); count++)
						{
							if (log.IsDebugEnabled)
							{
								log.Debug("result set row: " + count);
							}

							rowCount++;
							object result = await (translator.Loader.GetRowFromResultSetAsync(
								reader, session, parameter, lockModeArray, optionalObjectKey, hydratedObjects[i], keys, true, cancellationToken)).ConfigureAwait(false);
							tempResults.Add(result);

							if (createSubselects[i])
							{
								subselectResultKeys[i].Add(keys);
								keys = new EntityKey[entitySpan]; //can't reuse in this case
							}
						}

						if (log.IsDebugEnabled)
						{
							log.Debug(string.Format("done processing result set ({0} rows)", count));
						}

						results.Add(tempResults);

						if (log.IsDebugEnabled)
						{
							log.DebugFormat("Query {0} returned {1} results", i, tempResults.Count);
						}

						await (reader.NextResultAsync(cancellationToken)).ConfigureAwait(false);
					}

					for (int i = 0; i < translators.Count; i++)
					{
						ITranslator translator = translators[i];
						QueryParameters parameter = parameters[i];

						await (translator.Loader.InitializeEntitiesAndCollectionsAsync(hydratedObjects[i], reader, session, false, cancellationToken)).ConfigureAwait(false);

						if (createSubselects[i])
						{
							translator.Loader.CreateSubselects(subselectResultKeys[i], parameter, session);
						}
					}
				}
			}
			catch (Exception sqle)
			{
				var message = string.Format("Failed to execute multi query: [{0}]", resultSetsCommand.Sql);
				log.Error(message, sqle);
				throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, sqle, "Failed to execute multi query", resultSetsCommand.Sql);
			}

			if (statsEnabled)
			{
				stopWatch.Stop();
				session.Factory.StatisticsImplementor.QueryExecuted(string.Format("{0} queries (MultiQuery)", translators.Count), rowCount, stopWatch.Elapsed);
			}
			return results;
		}

		private async Task AggregateQueriesInformationAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int queryIndex = 0;
			foreach (AbstractQueryImpl query in queries)
			{
				query.VerifyParameters();
				QueryParameters queryParameters = query.GetQueryParameters();
				queryParameters.ValidateParameters();
				foreach (var translator in await (query.GetTranslatorsAsync(session, queryParameters, cancellationToken)).ConfigureAwait(false))
				{
					translators.Add(translator);
					translatorQueryMap.Add(queryIndex);
					parameters.Add(queryParameters);
					ISqlCommand singleCommand = translator.Loader.CreateSqlCommand(queryParameters, session);
					resultSetsCommand.Append(singleCommand);
				}
				queryIndex++;
			}
		}

		public async Task<object> GetResultAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (queryResults == null)
			{
				queryResults = await (ListAsync(cancellationToken)).ConfigureAwait(false);
			}

			int queryResultPosition;
			if (!queryResultPositions.TryGetValue(key, out queryResultPosition))
				throw new InvalidOperationException(String.Format("The key '{0}' is unknown", key));

			return queryResults[queryResultPosition];
		}

		#region Implementation

		private async Task<IList> ListIgnoreQueryCacheAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return GetResultList(await (DoListAsync(cancellationToken)).ConfigureAwait(false));
		}

		private async Task<IList> ListUsingQueryCacheAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			IQueryCache queryCache = session.Factory.GetQueryCache(cacheRegion);

			ISet<FilterKey> filterKeys = FilterKey.CreateFilterKeys(session.EnabledFilters);

			ISet<string> querySpaces = new HashSet<string>();
			List<IType[]> resultTypesList = new List<IType[]>(Translators.Count);
			for (int i = 0; i < Translators.Count; i++)
			{
				ITranslator queryTranslator = Translators[i];
				querySpaces.UnionWith(queryTranslator.QuerySpaces);
				resultTypesList.Add(queryTranslator.ReturnTypes);
			}
			int[] firstRows = new int[Parameters.Count];
			int[] maxRows = new int[Parameters.Count];
			for (int i = 0; i < Parameters.Count; i++)
			{
				RowSelection rowSelection = Parameters[i].RowSelection;
				firstRows[i] = rowSelection.FirstRow;
				maxRows[i] = rowSelection.MaxRows;
			}

			MultipleQueriesCacheAssembler assembler = new MultipleQueriesCacheAssembler(resultTypesList);

			QueryKey key = new QueryKey(session.Factory, SqlString, combinedParameters, filterKeys, null)
				.SetFirstRows(firstRows)
				.SetMaxRows(maxRows);

			IList result = await (assembler.GetResultFromQueryCacheAsync(session, combinedParameters, querySpaces, queryCache, key, cancellationToken)).ConfigureAwait(false);

			if (result == null)
			{
				log.Debug("Cache miss for multi query");
				var list = await (DoListAsync(cancellationToken)).ConfigureAwait(false);
				queryCache.Put(key, new ICacheAssembler[] { assembler }, new object[] { list }, false, session);
				result = list;
			}

			return GetResultList(result);
		}

		#endregion
	}
}