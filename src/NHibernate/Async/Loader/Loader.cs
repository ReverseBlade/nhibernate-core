﻿#if NET_4_5
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NHibernate.AdoNet;
using NHibernate.Cache;
using NHibernate.Collection;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Exceptions;
using NHibernate.Hql;
using NHibernate.Hql.Util;
using NHibernate.Impl;
using NHibernate.Param;
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;
using System.Threading.Tasks;

namespace NHibernate.Loader
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public abstract partial class Loader
	{
		/// <summary>
		/// Execute an SQL query and attempt to instantiate instances of the class mapped by the given
		/// persister from each row of the <c>DataReader</c>. If an object is supplied, will attempt to
		/// initialize that object. If a collection is supplied, attempt to initialize that collection.
		/// </summary>
		private Task<IList> DoQueryAndInitializeNonLazyCollectionsAsync(ISessionImplementor session, QueryParameters queryParameters, bool returnProxies)
		{
			return DoQueryAndInitializeNonLazyCollectionsAsync(session, queryParameters, returnProxies, null);
		}

		private async Task<IList> DoQueryAndInitializeNonLazyCollectionsAsync(ISessionImplementor session, QueryParameters queryParameters, bool returnProxies, IResultTransformer forcedResultTransformer)
		{
			IPersistenceContext persistenceContext = session.PersistenceContext;
			bool defaultReadOnlyOrig = persistenceContext.DefaultReadOnly;
			if (queryParameters.IsReadOnlyInitialized)
				persistenceContext.DefaultReadOnly = queryParameters.ReadOnly;
			else
				queryParameters.ReadOnly = persistenceContext.DefaultReadOnly;
			persistenceContext.BeforeLoad();
			IList result;
			try
			{
				try
				{
					result = await (DoQueryAsync(session, queryParameters, returnProxies, forcedResultTransformer));
				}
				finally
				{
					persistenceContext.AfterLoad();
				}

				await (persistenceContext.InitializeNonLazyCollectionsAsync());
			}
			finally
			{
				persistenceContext.DefaultReadOnly = defaultReadOnlyOrig;
			}

			return result;
		}

		/// <summary>
		/// Loads a single row from the result set.  This is the processing used from the
		/// ScrollableResults where no collection fetches were encountered.
		/// </summary>
		/// <param name = "resultSet">The result set from which to do the load.</param>
		/// <param name = "session">The session from which the request originated.</param>
		/// <param name = "queryParameters">The query parameters specified by the user.</param>
		/// <param name = "returnProxies">Should proxies be generated</param>
		/// <returns>The loaded "row".</returns>
		/// <exception cref = "HibernateException"/>
		protected async Task<object> LoadSingleRowAsync(DbDataReader resultSet, ISessionImplementor session, QueryParameters queryParameters, bool returnProxies)
		{
			int entitySpan = EntityPersisters.Length;
			IList hydratedObjects = entitySpan == 0 ? null : new List<object>(entitySpan);
			object result;
			try
			{
				result = await (GetRowFromResultSetAsync(resultSet, session, queryParameters, GetLockModes(queryParameters.LockModes), null, hydratedObjects, new EntityKey[entitySpan], returnProxies));
			}
			catch (HibernateException)
			{
				throw; // Don't call Convert on HibernateExceptions
			}
			catch (Exception sqle)
			{
				throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, sqle, "could not read next row of results", SqlString, queryParameters.PositionalParameterValues, queryParameters.NamedParameters);
			}

			await (InitializeEntitiesAndCollectionsAsync(hydratedObjects, resultSet, session, queryParameters.IsReadOnly(session)));
			await (session.PersistenceContext.InitializeNonLazyCollectionsAsync());
			return result;
		}

		internal Task<object> GetRowFromResultSetAsync(DbDataReader resultSet, ISessionImplementor session, QueryParameters queryParameters, LockMode[] lockModeArray, EntityKey optionalObjectKey, IList hydratedObjects, EntityKey[] keys, bool returnProxies)
		{
			return GetRowFromResultSetAsync(resultSet, session, queryParameters, lockModeArray, optionalObjectKey, hydratedObjects, keys, returnProxies, null);
		}

		internal async Task<object> GetRowFromResultSetAsync(DbDataReader resultSet, ISessionImplementor session, QueryParameters queryParameters, LockMode[] lockModeArray, EntityKey optionalObjectKey, IList hydratedObjects, EntityKey[] keys, bool returnProxies, IResultTransformer forcedResultTransformer)
		{
			ILoadable[] persisters = EntityPersisters;
			int entitySpan = persisters.Length;
			for (int i = 0; i < entitySpan; i++)
			{
				keys[i] = await (GetKeyFromResultSetAsync(i, persisters[i], i == entitySpan - 1 ? queryParameters.OptionalId : null, resultSet, session));
			//TODO: the i==entitySpan-1 bit depends upon subclass implementation (very bad)
			}

			RegisterNonExists(keys, session);
			// this call is side-effecty
			object[] row = await (GetRowAsync(resultSet, persisters, keys, queryParameters.OptionalObject, optionalObjectKey, lockModeArray, hydratedObjects, session));
			await (ReadCollectionElementsAsync(row, resultSet, session));
			if (returnProxies)
			{
				// now get an existing proxy for each row element (if there is one)
				for (int i = 0; i < entitySpan; i++)
				{
					object entity = row[i];
					object proxy = session.PersistenceContext.ProxyFor(persisters[i], keys[i], entity);
					if (entity != proxy)
					{
						// Force the proxy to resolve itself
						((INHibernateProxy)proxy).HibernateLazyInitializer.SetImplementation(entity);
						row[i] = proxy;
					}
				}
			}

			return forcedResultTransformer == null ? await (GetResultColumnOrRowAsync(row, queryParameters.ResultTransformer, resultSet, session)) : forcedResultTransformer.TransformTuple(await (GetResultRowAsync(row, resultSet, session)), ResultRowAliases);
		}

		/// <summary>
		/// Read any collection elements contained in a single row of the result set
		/// </summary>
		private async Task ReadCollectionElementsAsync(object[] row, DbDataReader resultSet, ISessionImplementor session)
		{
			//TODO: make this handle multiple collection roles!
			ICollectionPersister[] collectionPersisters = CollectionPersisters;
			if (collectionPersisters != null)
			{
				ICollectionAliases[] descriptors = CollectionAliases;
				int[] collectionOwners = CollectionOwners;
				for (int i = 0; i < collectionPersisters.Length; i++)
				{
					bool hasCollectionOwners = collectionOwners != null && collectionOwners[i] > -1;
					//true if this is a query and we are loading multiple instances of the same collection role
					//otherwise this is a CollectionInitializer and we are loading up a single collection or batch
					object owner = hasCollectionOwners ? row[collectionOwners[i]] : null;
					//if null, owner will be retrieved from session
					ICollectionPersister collectionPersister = collectionPersisters[i];
					object key;
					if (owner == null)
					{
						key = null;
					}
					else
					{
						key = collectionPersister.CollectionType.GetKeyOfOwner(owner, session);
					//TODO: old version did not require hashmap lookup:
					//keys[collectionOwner].getIdentifier()
					}

					await (ReadCollectionElementAsync(owner, key, collectionPersister, descriptors[i], resultSet, session));
				}
			}
		}

		private async Task<IList> DoQueryAsync(ISessionImplementor session, QueryParameters queryParameters, bool returnProxies, IResultTransformer forcedResultTransformer)
		{
			using (new SessionIdLoggingContext(session.SessionId))
			{
				RowSelection selection = queryParameters.RowSelection;
				int maxRows = HasMaxRows(selection) ? selection.MaxRows : int.MaxValue;
				int entitySpan = EntityPersisters.Length;
				List<object> hydratedObjects = entitySpan == 0 ? null : new List<object>(entitySpan * 10);
				DbCommand st = await (PrepareQueryCommandAsync(queryParameters, false, session));
				DbDataReader rs = await (GetResultSetAsync(st, queryParameters.HasAutoDiscoverScalarTypes, queryParameters.Callable, selection, session));
				// would be great to move all this below here into another method that could also be used
				// from the new scrolling stuff.
				//
				// Would need to change the way the max-row stuff is handled (i.e. behind an interface) so
				// that I could do the control breaking at the means to know when to stop
				LockMode[] lockModeArray = GetLockModes(queryParameters.LockModes);
				EntityKey optionalObjectKey = GetOptionalObjectKey(queryParameters, session);
				bool createSubselects = IsSubselectLoadingEnabled;
				List<EntityKey[]> subselectResultKeys = createSubselects ? new List<EntityKey[]>() : null;
				IList results = new List<object>();
				try
				{
					HandleEmptyCollections(queryParameters.CollectionKeys, rs, session);
					EntityKey[] keys = new EntityKey[entitySpan]; // we can reuse it each time
					if (Log.IsDebugEnabled)
					{
						Log.Debug("processing result set");
					}

					int count;
					for (count = 0; count < maxRows && await (rs.ReadAsync()); count++)
					{
						if (Log.IsDebugEnabled)
						{
							Log.Debug("result set row: " + count);
						}

						object result = await (GetRowFromResultSetAsync(rs, session, queryParameters, lockModeArray, optionalObjectKey, hydratedObjects, keys, returnProxies, forcedResultTransformer));
						results.Add(result);
						if (createSubselects)
						{
							subselectResultKeys.Add(keys);
							keys = new EntityKey[entitySpan]; //can't reuse in this case
						}
					}

					if (Log.IsDebugEnabled)
					{
						Log.Debug(string.Format("done processing result set ({0} rows)", count));
					}
				}
				catch (Exception e)
				{
					e.Data["actual-sql-query"] = st.CommandText;
					throw;
				}
				finally
				{
					session.Batcher.CloseCommand(st, rs);
				}

				await (InitializeEntitiesAndCollectionsAsync(hydratedObjects, rs, session, queryParameters.IsReadOnly(session)));
				if (createSubselects)
				{
					CreateSubselects(subselectResultKeys, queryParameters, session);
				}

				return results;
			}
		}

		internal async Task InitializeEntitiesAndCollectionsAsync(IList hydratedObjects, object resultSetId, ISessionImplementor session, bool readOnly)
		{
			ICollectionPersister[] collectionPersisters = CollectionPersisters;
			if (collectionPersisters != null)
			{
				for (int i = 0; i < collectionPersisters.Length; i++)
				{
					if (collectionPersisters[i].IsArray)
					{
						//for arrays, we should end the collection load before resolving
						//the entities, since the actual array instances are not instantiated
						//during loading
						//TODO: or we could do this polymorphically, and have two
						//      different operations implemented differently for arrays
						EndCollectionLoad(resultSetId, session, collectionPersisters[i]);
					}
				}
			}

			//important: reuse the same event instances for performance!
			PreLoadEvent pre;
			PostLoadEvent post;
			if (session.IsEventSource)
			{
				var eventSourceSession = (IEventSource)session;
				pre = new PreLoadEvent(eventSourceSession);
				post = new PostLoadEvent(eventSourceSession);
			}
			else
			{
				pre = null;
				post = null;
			}

			if (hydratedObjects != null)
			{
				int hydratedObjectsSize = hydratedObjects.Count;
				if (Log.IsDebugEnabled)
				{
					Log.Debug(string.Format("total objects hydrated: {0}", hydratedObjectsSize));
				}

				for (int i = 0; i < hydratedObjectsSize; i++)
				{
					await (TwoPhaseLoad.InitializeEntityAsync(hydratedObjects[i], readOnly, session, pre, post));
				}
			}

			if (collectionPersisters != null)
			{
				for (int i = 0; i < collectionPersisters.Length; i++)
				{
					if (!collectionPersisters[i].IsArray)
					{
						//for sets, we should end the collection load after resolving
						//the entities, since we might call hashCode() on the elements
						//TODO: or we could do this polymorphically, and have two
						//      different operations implemented differently for arrays
						EndCollectionLoad(resultSetId, session, collectionPersisters[i]);
					}
				}
			}
		}

		/// <summary>
		/// Get the actual object that is returned in the user-visible result list.
		/// </summary>
		/// <remarks>
		/// This empty implementation merely returns its first argument. This is
		/// overridden by some subclasses.
		/// </remarks>
		protected virtual Task<object> GetResultColumnOrRowAsync(object[] row, IResultTransformer resultTransformer, DbDataReader rs, ISessionImplementor session)
		{
			try
			{
				return Task.FromResult<object>(GetResultColumnOrRow(row, resultTransformer, rs, session));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object>(ex);
			}
		}

		protected virtual Task<object[]> GetResultRowAsync(Object[] row, DbDataReader rs, ISessionImplementor session)
		{
			try
			{
				return Task.FromResult<object[]>(GetResultRow(row, rs, session));
			}
			catch (Exception ex)
			{
				return TaskHelper.FromException<object[]>(ex);
			}
		}

		/// <summary>
		/// Read one collection element from the current row of the ADO.NET result set
		/// </summary>
		private static async Task ReadCollectionElementAsync(object optionalOwner, object optionalKey, ICollectionPersister persister, ICollectionAliases descriptor, DbDataReader rs, ISessionImplementor session)
		{
			IPersistenceContext persistenceContext = session.PersistenceContext;
			object collectionRowKey = await (persister.ReadKeyAsync(rs, descriptor.SuffixedKeyAliases, session));
			if (collectionRowKey != null)
			{
				// we found a collection element in the result set
				if (Log.IsDebugEnabled)
				{
					Log.Debug("found row of collection: " + MessageHelper.CollectionInfoString(persister, collectionRowKey));
				}

				object owner = optionalOwner;
				if (owner == null)
				{
					owner = persistenceContext.GetCollectionOwner(collectionRowKey, persister);
					if (owner == null)
					{
					//TODO: This is assertion is disabled because there is a bug that means the
					//      original owner of a transient, uninitialized collection is not known 
					//      if the collection is re-referenced by a different object associated 
					//      with the current Session
					//throw new AssertionFailure("bug loading unowned collection");
					}
				}

				IPersistentCollection rowCollection = persistenceContext.LoadContexts.GetCollectionLoadContext(rs).GetLoadingCollection(persister, collectionRowKey);
				if (rowCollection != null)
				{
					await (rowCollection.ReadFromAsync(rs, persister, descriptor, owner));
				}
			}
			else if (optionalKey != null)
			{
				// we did not find a collection element in the result set, so we
				// ensure that a collection is created with the owner's identifier,
				// since what we have is an empty collection
				if (Log.IsDebugEnabled)
				{
					Log.Debug("result set contains (possibly empty) collection: " + MessageHelper.CollectionInfoString(persister, optionalKey));
				}

				persistenceContext.LoadContexts.GetCollectionLoadContext(rs).GetLoadingCollection(persister, optionalKey);
			// handle empty collection
			}
		// else no collection element, but also no owner
		}

		/// <summary>
		/// Read a row of <c>EntityKey</c>s from the <c>DbDataReader</c> into the given array.
		/// </summary>
		/// <remarks>
		/// Warning: this method is side-effecty. If an <c>id</c> is given, don't bother going
		/// to the <c>DbDataReader</c>
		/// </remarks>
		private async Task<EntityKey> GetKeyFromResultSetAsync(int i, IEntityPersister persister, object id, DbDataReader rs, ISessionImplementor session)
		{
			object resultId;
			// if we know there is exactly 1 row, we can skip.
			// it would be great if we could _always_ skip this;
			// it is a problem for <key-many-to-one>
			if (IsSingleRowLoader && id != null)
			{
				resultId = id;
			}
			else
			{
				IType idType = persister.IdentifierType;
				resultId = await (idType.NullSafeGetAsync(rs, EntityAliases[i].SuffixedKeyAliases, session, null));
				bool idIsResultId = id != null && resultId != null && idType.IsEqual(id, resultId, session.EntityMode, _factory);
				if (idIsResultId)
				{
					resultId = id; //use the id passed in
				}
			}

			return resultId == null ? null : session.GenerateEntityKey(resultId, persister);
		}

		/// <summary>
		/// Check the version of the object in the <c>DbDataReader</c> against
		/// the object version in the session cache, throwing an exception
		/// if the version numbers are different.
		/// </summary>
		/// <exception cref = "StaleObjectStateException"></exception>
		private async Task CheckVersionAsync(int i, IEntityPersister persister, object id, object entity, DbDataReader rs, ISessionImplementor session)
		{
			object version = session.PersistenceContext.GetEntry(entity).Version;
			// null version means the object is in the process of being loaded somewhere else in the ResultSet
			if (version != null)
			{
				IVersionType versionType = persister.VersionType;
				object currentVersion = await (versionType.NullSafeGetAsync(rs, EntityAliases[i].SuffixedVersionAliases, session, null));
				if (!versionType.IsEqual(version, currentVersion))
				{
					if (session.Factory.Statistics.IsStatisticsEnabled)
					{
						session.Factory.StatisticsImplementor.OptimisticFailure(persister.EntityName);
					}

					throw new StaleObjectStateException(persister.EntityName, id);
				}
			}
		}

		/// <summary>
		/// Resolve any ids for currently loaded objects, duplications within the <c>DbDataReader</c>,
		/// etc. Instantiate empty objects to be initialized from the <c>DbDataReader</c>. Return an
		/// array of objects (a row of results) and an array of booleans (by side-effect) that determine
		/// whether the corresponding object should be initialized
		/// </summary>
		private async Task<object[]> GetRowAsync(DbDataReader rs, ILoadable[] persisters, EntityKey[] keys, object optionalObject, EntityKey optionalObjectKey, LockMode[] lockModes, IList hydratedObjects, ISessionImplementor session)
		{
			int cols = persisters.Length;
			IEntityAliases[] descriptors = EntityAliases;
			if (Log.IsDebugEnabled)
			{
				Log.Debug("result row: " + StringHelper.ToString(keys));
			}

			object[] rowResults = new object[cols];
			for (int i = 0; i < cols; i++)
			{
				object obj = null;
				EntityKey key = keys[i];
				if (keys[i] == null)
				{
				// do nothing
				/* TODO NH-1001 : if (persisters[i]...EntityType) is an OneToMany or a ManyToOne and
					 * the keys.length > 1 and the relation IsIgnoreNotFound probably we are in presence of
					 * an load with "outer join" the relation can be considerer loaded even if the key is null (mean not found)
					*/
				}
				else
				{
					//If the object is already loaded, return the loaded one
					obj = await (session.GetEntityUsingInterceptorAsync(key));
					if (obj != null)
					{
						await (InstanceAlreadyLoadedAsync(rs, i, persisters[i], key, obj, lockModes[i], session));
					}
					else
					{
						obj = await (InstanceNotYetLoadedAsync(rs, i, persisters[i], key, lockModes[i], descriptors[i].RowIdAlias, optionalObjectKey, optionalObject, hydratedObjects, session));
					}
				}

				rowResults[i] = obj;
			}

			return rowResults;
		}

		/// <summary>
		/// The entity instance is already in the session cache
		/// </summary>
		private async Task InstanceAlreadyLoadedAsync(DbDataReader rs, int i, IEntityPersister persister, EntityKey key, object obj, LockMode lockMode, ISessionImplementor session)
		{
			if (!persister.IsInstance(obj, session.EntityMode))
			{
				string errorMsg = string.Format("loading object was of wrong class [{0}]", obj.GetType().FullName);
				throw new WrongClassException(errorMsg, key.Identifier, persister.EntityName);
			}

			if (LockMode.None != lockMode && UpgradeLocks())
			{
				EntityEntry entry = session.PersistenceContext.GetEntry(obj);
				bool isVersionCheckNeeded = persister.IsVersioned && entry.LockMode.LessThan(lockMode);
				// we don't need to worry about existing version being uninitialized
				// because this block isn't called by a re-entrant load (re-entrant
				// load _always_ have lock mode NONE
				if (isVersionCheckNeeded)
				{
					await (CheckVersionAsync(i, persister, key.Identifier, obj, rs, session));
					// we need to upgrade the lock mode to the mode requested
					entry.LockMode = lockMode;
				}
			}
		}

		/// <summary>
		/// The entity instance is not in the session cache
		/// </summary>
		private async Task<object> InstanceNotYetLoadedAsync(DbDataReader dr, int i, ILoadable persister, EntityKey key, LockMode lockMode, string rowIdAlias, EntityKey optionalObjectKey, object optionalObject, IList hydratedObjects, ISessionImplementor session)
		{
			object obj;
			string instanceClass = await (GetInstanceClassAsync(dr, i, persister, key.Identifier, session));
			if (optionalObjectKey != null && key.Equals(optionalObjectKey))
			{
				// its the given optional object
				obj = optionalObject;
			}
			else
			{
				obj = session.Instantiate(instanceClass, key.Identifier);
			}

			// need to hydrate it
			// grab its state from the DataReader and keep it in the Session
			// (but don't yet initialize the object itself)
			// note that we acquired LockMode.READ even if it was not requested
			LockMode acquiredLockMode = lockMode == LockMode.None ? LockMode.Read : lockMode;
			await (LoadFromResultSetAsync(dr, i, obj, instanceClass, key, rowIdAlias, acquiredLockMode, persister, session));
			// materialize associations (and initialize the object) later
			hydratedObjects.Add(obj);
			return obj;
		}

		/// <summary>
		/// Hydrate the state of an object from the SQL <c>DbDataReader</c>, into
		/// an array of "hydrated" values (do not resolve associations yet),
		/// and pass the hydrated state to the session.
		/// </summary>
		private async Task LoadFromResultSetAsync(DbDataReader rs, int i, object obj, string instanceClass, EntityKey key, string rowIdAlias, LockMode lockMode, ILoadable rootPersister, ISessionImplementor session)
		{
			object id = key.Identifier;
			// Get the persister for the _subclass_
			ILoadable persister = (ILoadable)Factory.GetEntityPersister(instanceClass);
			if (Log.IsDebugEnabled)
			{
				Log.Debug("Initializing object from DataReader: " + MessageHelper.InfoString(persister, id));
			}

			bool eagerPropertyFetch = IsEagerPropertyFetchEnabled(i);
			// add temp entry so that the next step is circular-reference
			// safe - only needed because some types don't take proper
			// advantage of two-phase-load (esp. components)
			TwoPhaseLoad.AddUninitializedEntity(key, obj, persister, lockMode, !eagerPropertyFetch, session);
			// This is not very nice (and quite slow):
			string[][] cols = persister == rootPersister ? EntityAliases[i].SuffixedPropertyAliases : EntityAliases[i].GetSuffixedPropertyAliases(persister);
			object[] values = await (persister.HydrateAsync(rs, id, obj, rootPersister, cols, eagerPropertyFetch, session));
			object rowId = persister.HasRowId ? rs[rowIdAlias] : null;
			IAssociationType[] ownerAssociationTypes = OwnerAssociationTypes;
			if (ownerAssociationTypes != null && ownerAssociationTypes[i] != null)
			{
				string ukName = ownerAssociationTypes[i].RHSUniqueKeyPropertyName;
				if (ukName != null)
				{
					int index = ((IUniqueKeyLoadable)persister).GetPropertyIndex(ukName);
					IType type = persister.PropertyTypes[index];
					// polymorphism not really handled completely correctly,
					// perhaps...well, actually its ok, assuming that the
					// entity name used in the lookup is the same as the
					// the one used here, which it will be
					EntityUniqueKey euk = new EntityUniqueKey(rootPersister.EntityName, ukName, await (type.SemiResolveAsync(values[index], session, obj)), type, session.EntityMode, session.Factory);
					session.PersistenceContext.AddEntity(euk, obj);
				}
			}

			TwoPhaseLoad.PostHydrate(persister, id, values, rowId, obj, lockMode, !eagerPropertyFetch, session);
		}

		/// <summary>
		/// Determine the concrete class of an instance for the <c>DbDataReader</c>
		/// </summary>
		private async Task<string> GetInstanceClassAsync(DbDataReader rs, int i, ILoadable persister, object id, ISessionImplementor session)
		{
			if (persister.HasSubclasses)
			{
				// code to handle subclasses of topClass
				object discriminatorValue = await (persister.DiscriminatorType.NullSafeGetAsync(rs, EntityAliases[i].SuffixedDiscriminatorAlias, session, null));
				string result = persister.GetSubclassForDiscriminatorValue(discriminatorValue);
				if (result == null)
				{
					// woops we got an instance of another class hierarchy branch.
					throw new WrongClassException(string.Format("Discriminator was: '{0}'", discriminatorValue), id, persister.EntityName);
				}

				return result;
			}

			return persister.EntityName;
		}

		/// <summary>
		/// Advance the cursor to the first required row of the <c>DbDataReader</c>
		/// </summary>
		internal static async Task AdvanceAsync(DbDataReader rs, RowSelection selection)
		{
			int firstRow = GetFirstRow(selection);
			if (firstRow != 0)
			{
				// DataReaders are forward-only, readonly, so we have to step through
				for (int i = 0; i < firstRow; i++)
				{
					await (rs.ReadAsync());
				}
			}
		}

		/// <summary>
		/// Obtain an <c>DbCommand</c> with all parameters pre-bound. Bind positional parameters,
		/// named parameters, and limit parameters.
		/// </summary>
		/// <remarks>
		/// Creates an DbCommand object and populates it with the values necessary to execute it against the 
		/// database to Load an Entity.
		/// </remarks>
		/// <param name = "queryParameters">The <see cref = "QueryParameters"/> to use for the DbCommand.</param>
		/// <param name = "scroll">TODO: find out where this is used...</param>
		/// <param name = "session">The SessionImpl this Command is being prepared in.</param>
		/// <returns>A CommandWrapper wrapping an DbCommand that is ready to be executed.</returns>
		protected internal virtual async Task<DbCommand> PrepareQueryCommandAsync(QueryParameters queryParameters, bool scroll, ISessionImplementor session)
		{
			ISqlCommand sqlCommand = CreateSqlCommand(queryParameters, session);
			SqlString sqlString = sqlCommand.Query;
			sqlCommand.ResetParametersIndexesForTheCommand(0);
			DbCommand command = session.Batcher.PrepareQueryCommand(CommandType.Text, sqlString, sqlCommand.ParameterTypes);
			try
			{
				RowSelection selection = queryParameters.RowSelection;
				if (selection != null && selection.Timeout != RowSelection.NoValue)
				{
					command.CommandTimeout = selection.Timeout;
				}

				await (sqlCommand.BindAsync(command, session));
				IDriver driver = _factory.ConnectionProvider.Driver;
				driver.RemoveUnusedCommandParameters(command, sqlString);
				driver.ExpandQueryParameters(command, sqlString);
			}
			catch (HibernateException)
			{
				session.Batcher.CloseCommand(command, null);
				throw;
			}
			catch (Exception sqle)
			{
				session.Batcher.CloseCommand(command, null);
				ADOExceptionReporter.LogExceptions(sqle);
				throw;
			}

			return command;
		}

		/// <summary>
		/// Fetch a <c>DbCommand</c>, call <c>SetMaxRows</c> and then execute it,
		/// advance to the first result and return an SQL <c>DbDataReader</c>
		/// </summary>
		/// <param name = "st">The <see cref = "DbCommand"/> to execute.</param>
		/// <param name = "selection">The <see cref = "RowSelection"/> to apply to the <see cref = "DbCommand"/> and <see cref = "DbDataReader"/>.</param>
		/// <param name = "autoDiscoverTypes">true if result types need to be auto-discovered by the loader; false otherwise.</param>
		/// <param name = "session">The <see cref = "ISession"/> to load in.</param>
		/// <param name = "callable"></param>
		/// <returns>An DbDataReader advanced to the first record in RowSelection.</returns>
		protected async Task<DbDataReader> GetResultSetAsync(DbCommand st, bool autoDiscoverTypes, bool callable, RowSelection selection, ISessionImplementor session)
		{
			DbDataReader rs = null;
			try
			{
				Log.Info(st.CommandText);
				// TODO NH: Callable
				rs = await (session.Batcher.ExecuteReaderAsync(st));
				//NH: this is checked outside the WrapResultSet because we
				// want to avoid the syncronization overhead in the vast majority
				// of cases where IsWrapResultSetsEnabled is set to false
				if (session.Factory.Settings.IsWrapResultSetsEnabled)
					rs = WrapResultSet(rs);
				Dialect.Dialect dialect = session.Factory.Dialect;
				if (!dialect.SupportsLimitOffset || !UseLimit(selection, dialect))
				{
					await (AdvanceAsync(rs, selection));
				}

				if (autoDiscoverTypes)
				{
					AutoDiscoverTypes(rs);
				}

				return rs;
			}
			catch (Exception sqle)
			{
				ADOExceptionReporter.LogExceptions(sqle);
				session.Batcher.CloseCommand(st, rs);
				throw;
			}
		}

		/// <summary>
		/// Called by subclasses that load entities
		/// </summary>
		protected async Task<IList> LoadEntityAsync(ISessionImplementor session, object id, IType identifierType, object optionalObject, string optionalEntityName, object optionalIdentifier, IEntityPersister persister)
		{
			if (Log.IsDebugEnabled)
			{
				Log.Debug("loading entity: " + MessageHelper.InfoString(persister, id, identifierType, Factory));
			}

			IList result;
			try
			{
				QueryParameters qp = new QueryParameters(new IType[]{identifierType}, new object[]{id}, optionalObject, optionalEntityName, optionalIdentifier);
				result = await (DoQueryAndInitializeNonLazyCollectionsAsync(session, qp, false));
			}
			catch (HibernateException)
			{
				throw;
			}
			catch (Exception sqle)
			{
				ILoadable[] persisters = EntityPersisters;
				throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, sqle, "could not load an entity: " + MessageHelper.InfoString(persisters[persisters.Length - 1], id, identifierType, Factory), SqlString);
			}

			Log.Debug("done entity load");
			return result;
		}

		protected async Task<IList> LoadEntityAsync(ISessionImplementor session, object key, object index, IType keyType, IType indexType, IEntityPersister persister)
		{
			Log.Debug("loading collection element by index");
			IList result;
			try
			{
				result = await (DoQueryAndInitializeNonLazyCollectionsAsync(session, new QueryParameters(new IType[]{keyType, indexType}, new object[]{key, index}), false));
			}
			catch (Exception sqle)
			{
				throw ADOExceptionHelper.Convert(_factory.SQLExceptionConverter, sqle, "could not collection element by index", SqlString);
			}

			Log.Debug("done entity load");
			return result;
		}

		/// <summary>
		/// Called by subclasses that batch load entities
		/// </summary>
		protected internal async Task<IList> LoadEntityBatchAsync(ISessionImplementor session, object[] ids, IType idType, object optionalObject, string optionalEntityName, object optionalId, IEntityPersister persister)
		{
			if (Log.IsDebugEnabled)
			{
				Log.Debug("batch loading entity: " + MessageHelper.InfoString(persister, ids, Factory));
			}

			IType[] types = new IType[ids.Length];
			ArrayHelper.Fill(types, idType);
			IList result;
			try
			{
				result = await (DoQueryAndInitializeNonLazyCollectionsAsync(session, new QueryParameters(types, ids, optionalObject, optionalEntityName, optionalId), false));
			}
			catch (HibernateException)
			{
				throw;
			}
			catch (Exception sqle)
			{
				throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, sqle, "could not load an entity batch: " + MessageHelper.InfoString(persister, ids, Factory), SqlString);
			// NH: Hibernate3 passes EntityPersisters[0] instead of persister, I think it's wrong.
			}

			Log.Debug("done entity batch load");
			return result;
		}

		/// <summary>
		/// Called by subclasses that load collections
		/// </summary>
		public async Task LoadCollectionAsync(ISessionImplementor session, object id, IType type)
		{
			if (Log.IsDebugEnabled)
			{
				Log.Debug("loading collection: " + MessageHelper.CollectionInfoString(CollectionPersisters[0], id));
			}

			object[] ids = new object[]{id};
			try
			{
				await (DoQueryAndInitializeNonLazyCollectionsAsync(session, new QueryParameters(new IType[]{type}, ids, ids), true));
			}
			catch (HibernateException)
			{
				// Do not call Convert on HibernateExceptions
				throw;
			}
			catch (Exception sqle)
			{
				throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, sqle, "could not initialize a collection: " + MessageHelper.CollectionInfoString(CollectionPersisters[0], id), SqlString);
			}

			Log.Debug("done loading collection");
		}

		/// <summary>
		/// Called by wrappers that batch initialize collections
		/// </summary>
		public async Task LoadCollectionBatchAsync(ISessionImplementor session, object[] ids, IType type)
		{
			if (Log.IsDebugEnabled)
			{
				Log.Debug("batch loading collection: " + MessageHelper.CollectionInfoString(CollectionPersisters[0], ids));
			}

			IType[] idTypes = new IType[ids.Length];
			ArrayHelper.Fill(idTypes, type);
			try
			{
				await (DoQueryAndInitializeNonLazyCollectionsAsync(session, new QueryParameters(idTypes, ids, ids), true));
			}
			catch (HibernateException)
			{
				// Do not call Convert on HibernateExceptions
				throw;
			}
			catch (Exception sqle)
			{
				throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, sqle, "could not initialize a collection batch: " + MessageHelper.CollectionInfoString(CollectionPersisters[0], ids), SqlString);
			}

			Log.Debug("done batch load");
		}

		/// <summary>
		/// Called by subclasses that batch initialize collections
		/// </summary>
		protected async Task LoadCollectionSubselectAsync(ISessionImplementor session, object[] ids, object[] parameterValues, IType[] parameterTypes, IDictionary<string, TypedValue> namedParameters, IType type)
		{
			try
			{
				await (DoQueryAndInitializeNonLazyCollectionsAsync(session, new QueryParameters(parameterTypes, parameterValues, namedParameters, ids), true));
			}
			catch (HibernateException)
			{
				// Do not call Convert on HibernateExceptions
				throw;
			}
			catch (Exception sqle)
			{
				throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, sqle, "could not load collection by subselect: " + MessageHelper.CollectionInfoString(CollectionPersisters[0], ids), SqlString, parameterValues, namedParameters);
			}
		}

		/// <summary>
		/// Return the query results, using the query cache, called
		/// by subclasses that implement cacheable queries
		/// </summary>
		/// <param name = "session"></param>
		/// <param name = "queryParameters"></param>
		/// <param name = "querySpaces"></param>
		/// <param name = "resultTypes"></param>
		/// <returns></returns>
		protected async Task<IList> ListAsync(ISessionImplementor session, QueryParameters queryParameters, ISet<string> querySpaces, IType[] resultTypes)
		{
			bool cacheable = _factory.Settings.IsQueryCacheEnabled && queryParameters.Cacheable;
			if (cacheable)
			{
				return await (ListUsingQueryCacheAsync(session, queryParameters, querySpaces, resultTypes));
			}

			return await (ListIgnoreQueryCacheAsync(session, queryParameters));
		}

		private async Task<IList> ListIgnoreQueryCacheAsync(ISessionImplementor session, QueryParameters queryParameters)
		{
			return GetResultList(await (DoListAsync(session, queryParameters)), queryParameters.ResultTransformer);
		}

		private async Task<IList> ListUsingQueryCacheAsync(ISessionImplementor session, QueryParameters queryParameters, ISet<string> querySpaces, IType[] resultTypes)
		{
			IQueryCache queryCache = _factory.GetQueryCache(queryParameters.CacheRegion);
			QueryKey key = GenerateQueryKey(session, queryParameters);
			IList result = await (GetResultFromQueryCacheAsync(session, queryParameters, querySpaces, resultTypes, queryCache, key));
			if (result == null)
			{
				result = await (DoListAsync(session, queryParameters, key.ResultTransformer));
				await (PutResultInQueryCacheAsync(session, queryParameters, resultTypes, queryCache, key, result));
			}

			IResultTransformer resolvedTransformer = ResolveResultTransformer(queryParameters.ResultTransformer);
			if (resolvedTransformer != null)
			{
				result = (AreResultSetRowsTransformedImmediately() ? key.ResultTransformer.RetransformResults(result, ResultRowAliases, queryParameters.ResultTransformer, IncludeInResultRow) : key.ResultTransformer.UntransformToTuples(result));
			}

			return GetResultList(result, queryParameters.ResultTransformer);
		}

		private async Task<IList> GetResultFromQueryCacheAsync(ISessionImplementor session, QueryParameters queryParameters, ISet<string> querySpaces, IType[] resultTypes, IQueryCache queryCache, QueryKey key)
		{
			IList result = null;
			if ((!queryParameters.ForceCacheRefresh) && (session.CacheMode & CacheMode.Get) == CacheMode.Get)
			{
				IPersistenceContext persistenceContext = session.PersistenceContext;
				bool defaultReadOnlyOrig = persistenceContext.DefaultReadOnly;
				if (queryParameters.IsReadOnlyInitialized)
					persistenceContext.DefaultReadOnly = queryParameters.ReadOnly;
				else
					queryParameters.ReadOnly = persistenceContext.DefaultReadOnly;
				try
				{
					result = await (queryCache.GetAsync(key, key.ResultTransformer.GetCachedResultTypes(resultTypes), queryParameters.NaturalKeyLookup, querySpaces, session));
					if (_factory.Statistics.IsStatisticsEnabled)
					{
						if (result == null)
						{
							_factory.StatisticsImplementor.QueryCacheMiss(QueryIdentifier, queryCache.RegionName);
						}
						else
						{
							_factory.StatisticsImplementor.QueryCacheHit(QueryIdentifier, queryCache.RegionName);
						}
					}
				}
				finally
				{
					persistenceContext.DefaultReadOnly = defaultReadOnlyOrig;
				}
			}

			return result;
		}

		private async Task PutResultInQueryCacheAsync(ISessionImplementor session, QueryParameters queryParameters, IType[] resultTypes, IQueryCache queryCache, QueryKey key, IList result)
		{
			if ((session.CacheMode & CacheMode.Put) == CacheMode.Put)
			{
				bool put = await (queryCache.PutAsync(key, key.ResultTransformer.GetCachedResultTypes(resultTypes), result, queryParameters.NaturalKeyLookup, session));
				if (put && _factory.Statistics.IsStatisticsEnabled)
				{
					_factory.StatisticsImplementor.QueryCachePut(QueryIdentifier, queryCache.RegionName);
				}
			}
		}

		/// <summary>
		/// Actually execute a query, ignoring the query cache
		/// </summary>
		/// <param name = "session"></param>
		/// <param name = "queryParameters"></param>
		/// <returns></returns>
		protected Task<IList> DoListAsync(ISessionImplementor session, QueryParameters queryParameters)
		{
			return DoListAsync(session, queryParameters, null);
		}

		protected async Task<IList> DoListAsync(ISessionImplementor session, QueryParameters queryParameters, IResultTransformer forcedResultTransformer)
		{
			bool statsEnabled = Factory.Statistics.IsStatisticsEnabled;
			var stopWatch = new Stopwatch();
			if (statsEnabled)
			{
				stopWatch.Start();
			}

			IList result;
			try
			{
				result = await (DoQueryAndInitializeNonLazyCollectionsAsync(session, queryParameters, true, forcedResultTransformer));
			}
			catch (HibernateException)
			{
				// Do not call Convert on HibernateExceptions
				throw;
			}
			catch (Exception sqle)
			{
				throw ADOExceptionHelper.Convert(Factory.SQLExceptionConverter, sqle, "could not execute query", SqlString, queryParameters.PositionalParameterValues, queryParameters.NamedParameters);
			}

			if (statsEnabled)
			{
				stopWatch.Stop();
				Factory.StatisticsImplementor.QueryExecuted(QueryIdentifier, result.Count, stopWatch.Elapsed);
			}

			return result;
		}
	}
}
#endif
