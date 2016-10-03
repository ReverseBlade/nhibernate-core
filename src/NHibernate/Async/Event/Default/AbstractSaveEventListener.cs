﻿#if NET_4_5
using System;
using System.Collections;
using NHibernate.Action;
using NHibernate.Classic;
using NHibernate.Engine;
using NHibernate.Id;
using NHibernate.Impl;
using NHibernate.Intercept;
using NHibernate.Persister.Entity;
using NHibernate.Type;
using Status = NHibernate.Engine.Status;
using System.Threading.Tasks;

namespace NHibernate.Event.Default
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public abstract partial class AbstractSaveEventListener : AbstractReassociateEventListener
	{
		/// <summary> 
		/// Prepares the save call using the given requested id. 
		/// </summary>
		/// <param name = "entity">The entity to be saved. </param>
		/// <param name = "requestedId">The id to which to associate the entity. </param>
		/// <param name = "entityName">The name of the entity being saved. </param>
		/// <param name = "anything">Generally cascade-specific information. </param>
		/// <param name = "source">The session which is the source of this save event. </param>
		/// <returns> The id used to save the entity. </returns>
		protected virtual async Task<object> SaveWithRequestedIdAsync(object entity, object requestedId, string entityName, object anything, IEventSource source)
		{
			return await (PerformSaveAsync(entity, requestedId, source.GetEntityPersister(entityName, entity), false, anything, source, true));
		}

		/// <summary> 
		/// Prepares the save call using a newly generated id. 
		/// </summary>
		/// <param name = "entity">The entity to be saved </param>
		/// <param name = "entityName">The entity-name for the entity to be saved </param>
		/// <param name = "anything">Generally cascade-specific information. </param>
		/// <param name = "source">The session which is the source of this save event. </param>
		/// <param name = "requiresImmediateIdAccess">
		/// does the event context require
		/// access to the identifier immediately after execution of this method (if
		/// not, post-insert style id generators may be postponed if we are outside
		/// a transaction). 
		/// </param>
		/// <returns> 
		/// The id used to save the entity; may be null depending on the
		/// type of id generator used and the requiresImmediateIdAccess value
		/// </returns>
		protected virtual async Task<object> SaveWithGeneratedIdAsync(object entity, string entityName, object anything, IEventSource source, bool requiresImmediateIdAccess)
		{
			IEntityPersister persister = source.GetEntityPersister(entityName, entity);
			object generatedId = await (persister.IdentifierGenerator.GenerateAsync(source, entity));
			if (generatedId == null)
			{
				throw new IdentifierGenerationException("null id generated for:" + entity.GetType());
			}
			else if (generatedId == IdentifierGeneratorFactory.ShortCircuitIndicator)
			{
				return source.GetIdentifier(entity);
			}
			else if (generatedId == IdentifierGeneratorFactory.PostInsertIndicator)
			{
				return await (PerformSaveAsync(entity, null, persister, true, anything, source, requiresImmediateIdAccess));
			}
			else
			{
				if (log.IsDebugEnabled)
				{
					log.Debug(string.Format("generated identifier: {0}, using strategy: {1}", persister.IdentifierType.ToLoggableString(generatedId, source.Factory), persister.IdentifierGenerator.GetType().FullName));
				}

				return await (PerformSaveAsync(entity, generatedId, persister, false, anything, source, true));
			}
		}

		/// <summary> 
		/// Prepares the save call by checking the session caches for a pre-existing
		/// entity and performing any lifecycle callbacks. 
		/// </summary>
		/// <param name = "entity">The entity to be saved. </param>
		/// <param name = "id">The id by which to save the entity. </param>
		/// <param name = "persister">The entity's persister instance. </param>
		/// <param name = "useIdentityColumn">Is an identity column being used? </param>
		/// <param name = "anything">Generally cascade-specific information. </param>
		/// <param name = "source">The session from which the event originated. </param>
		/// <param name = "requiresImmediateIdAccess">
		/// does the event context require
		/// access to the identifier immediately after execution of this method (if
		/// not, post-insert style id generators may be postponed if we are outside
		/// a transaction). 
		/// </param>
		/// <returns> 
		/// The id used to save the entity; may be null depending on the
		/// type of id generator used and the requiresImmediateIdAccess value
		/// </returns>
		protected virtual async Task<object> PerformSaveAsync(object entity, object id, IEntityPersister persister, bool useIdentityColumn, object anything, IEventSource source, bool requiresImmediateIdAccess)
		{
			if (log.IsDebugEnabled)
			{
				log.Debug("saving " + MessageHelper.InfoString(persister, id, source.Factory));
			}

			EntityKey key;
			if (!useIdentityColumn)
			{
				key = source.GenerateEntityKey(id, persister);
				object old = source.PersistenceContext.GetEntity(key);
				if (old != null)
				{
					if (source.PersistenceContext.GetEntry(old).Status == Status.Deleted)
					{
						await (source.ForceFlushAsync(source.PersistenceContext.GetEntry(old)));
					}
					else
					{
						throw new NonUniqueObjectException(id, persister.EntityName);
					}
				}

				persister.SetIdentifier(entity, id, source.EntityMode);
			}
			else
			{
				key = null;
			}

			if (InvokeSaveLifecycle(entity, persister, source))
			{
				return id; //EARLY EXIT
			}

			return await (PerformSaveOrReplicateAsync(entity, key, persister, useIdentityColumn, anything, source, requiresImmediateIdAccess));
		}

		/// <summary> 
		/// Performs all the actual work needed to save an entity (well to get the save moved to
		/// the execution queue). 
		/// </summary>
		/// <param name = "entity">The entity to be saved </param>
		/// <param name = "key">The id to be used for saving the entity (or null, in the case of identity columns) </param>
		/// <param name = "persister">The entity's persister instance. </param>
		/// <param name = "useIdentityColumn">Should an identity column be used for id generation? </param>
		/// <param name = "anything">Generally cascade-specific information. </param>
		/// <param name = "source">The session which is the source of the current event. </param>
		/// <param name = "requiresImmediateIdAccess">
		/// Is access to the identifier required immediately
		/// after the completion of the save?  persist(), for example, does not require this... 
		/// </param>
		/// <returns> 
		/// The id used to save the entity; may be null depending on the
		/// type of id generator used and the requiresImmediateIdAccess value
		/// </returns>
		protected virtual async Task<object> PerformSaveOrReplicateAsync(object entity, EntityKey key, IEntityPersister persister, bool useIdentityColumn, object anything, IEventSource source, bool requiresImmediateIdAccess)
		{
			Validate(entity, persister, source);
			object id = key == null ? null : key.Identifier;
			// NH Different behavior (shouldDelayIdentityInserts=false anyway)
			//bool inTxn = source.ConnectionManager.IsInActiveTransaction;
			//bool shouldDelayIdentityInserts = !inTxn && !requiresImmediateIdAccess;
			bool shouldDelayIdentityInserts = false;
			// Put a placeholder in entries, so we don't recurse back and try to save() the
			// same object again. QUESTION: should this be done before onSave() is called?
			// likewise, should it be done before onUpdate()?
			source.PersistenceContext.AddEntry(entity, Status.Saving, null, null, id, null, LockMode.Write, useIdentityColumn, persister, false, false);
			await (CascadeBeforeSaveAsync(source, persister, entity, anything));
			// NH-962: This was originally done before many-to-one cascades.
			if (useIdentityColumn && !shouldDelayIdentityInserts)
			{
				log.Debug("executing insertions");
				await (source.ActionQueue.ExecuteInsertsAsync());
			}

			object[] values = await (persister.GetPropertyValuesToInsertAsync(entity, GetMergeMap(anything), source));
			IType[] types = persister.PropertyTypes;
			bool substitute = await (SubstituteValuesIfNecessaryAsync(entity, id, values, persister, source));
			if (persister.HasCollections)
			{
				substitute = substitute || await (VisitCollectionsBeforeSaveAsync(entity, id, values, types, source));
			}

			if (substitute)
			{
				persister.SetPropertyValues(entity, values, source.EntityMode);
			}

			TypeHelper.DeepCopy(values, types, persister.PropertyUpdateability, values, source);
			await (new ForeignKeys.Nullifier(entity, false, useIdentityColumn, source).NullifyTransientReferencesAsync(values, types));
			new Nullability(source).CheckNullability(values, persister, false);
			if (useIdentityColumn)
			{
				EntityIdentityInsertAction insert = new EntityIdentityInsertAction(values, entity, persister, source, shouldDelayIdentityInserts);
				if (!shouldDelayIdentityInserts)
				{
					log.Debug("executing identity-insert immediately");
					await (source.ActionQueue.ExecuteAsync(insert));
					id = insert.GeneratedId;
					//now done in EntityIdentityInsertAction
					//persister.setIdentifier( entity, id, source.getEntityMode() );
					key = source.GenerateEntityKey(id, persister);
					source.PersistenceContext.CheckUniqueness(key, entity);
				//source.getBatcher().executeBatch(); //found another way to ensure that all batched joined inserts have been executed
				}
				else
				{
					log.Debug("delaying identity-insert due to no transaction in progress");
					source.ActionQueue.AddAction(insert);
					key = insert.DelayedEntityKey;
				}
			}

			object version = Versioning.GetVersion(values, persister);
			source.PersistenceContext.AddEntity(entity, persister.IsMutable ? Status.Loaded : Status.ReadOnly, values, key, version, LockMode.Write, useIdentityColumn, persister, VersionIncrementDisabled, false);
			//source.getPersistenceContext().removeNonExist( new EntityKey( id, persister, source.getEntityMode() ) );
			if (!useIdentityColumn)
			{
				source.ActionQueue.AddAction(new EntityInsertAction(id, values, entity, version, persister, source));
			}

			await (CascadeAfterSaveAsync(source, persister, entity, anything));
			MarkInterceptorDirty(entity, persister, source);
			return id;
		}

		protected virtual async Task<bool> VisitCollectionsBeforeSaveAsync(object entity, object id, object[] values, IType[] types, IEventSource source)
		{
			WrapVisitor visitor = new WrapVisitor(source);
			// substitutes into values by side-effect
			await (visitor.ProcessEntityPropertyValuesAsync(values, types));
			return visitor.SubstitutionRequired;
		}

		/// <summary> 
		/// Perform any property value substitution that is necessary
		/// (interceptor callback, version initialization...) 
		/// </summary>
		/// <param name = "entity">The entity </param>
		/// <param name = "id">The entity identifier </param>
		/// <param name = "values">The snapshot entity state </param>
		/// <param name = "persister">The entity persister </param>
		/// <param name = "source">The originating session </param>
		/// <returns> 
		/// True if the snapshot state changed such that
		/// reinjection of the values into the entity is required.
		/// </returns>
		protected virtual async Task<bool> SubstituteValuesIfNecessaryAsync(object entity, object id, object[] values, IEntityPersister persister, ISessionImplementor source)
		{
			bool substitute = source.Interceptor.OnSave(entity, id, values, persister.PropertyNames, persister.PropertyTypes);
			//keep the existing version number in the case of replicate!
			if (persister.IsVersioned)
			{
				// NH Specific feature (H3.2 use null value for versionProperty; NH ask to persister to know if a valueType mean unversioned)
				object versionValue = values[persister.VersionProperty];
				substitute |= await (Versioning.SeedVersionAsync(values, persister.VersionProperty, persister.VersionType, persister.IsUnsavedVersion(versionValue), source));
			}

			return substitute;
		}

		/// <summary> Handles the calls needed to perform pre-save cascades for the given entity. </summary>
		/// <param name = "source">The session from which the save event originated.</param>
		/// <param name = "persister">The entity's persister instance. </param>
		/// <param name = "entity">The entity to be saved. </param>
		/// <param name = "anything">Generally cascade-specific data </param>
		protected virtual async Task CascadeBeforeSaveAsync(IEventSource source, IEntityPersister persister, object entity, object anything)
		{
			// cascade-save to many-to-one BEFORE the parent is saved
			source.PersistenceContext.IncrementCascadeLevel();
			try
			{
				await (new Cascade(CascadeAction, CascadePoint.BeforeInsertAfterDelete, source).CascadeOnAsync(persister, entity, anything));
			}
			finally
			{
				source.PersistenceContext.DecrementCascadeLevel();
			}
		}

		/// <summary> Handles to calls needed to perform post-save cascades. </summary>
		/// <param name = "source">The session from which the event originated. </param>
		/// <param name = "persister">The entity's persister instance. </param>
		/// <param name = "entity">The entity being saved. </param>
		/// <param name = "anything">Generally cascade-specific data </param>
		protected virtual async Task CascadeAfterSaveAsync(IEventSource source, IEntityPersister persister, object entity, object anything)
		{
			// cascade-save to collections AFTER the collection owner was saved
			source.PersistenceContext.IncrementCascadeLevel();
			try
			{
				await (new Cascade(CascadeAction, CascadePoint.AfterInsertBeforeDelete, source).CascadeOnAsync(persister, entity, anything));
			}
			finally
			{
				source.PersistenceContext.DecrementCascadeLevel();
			}
		}

		/// <summary> 
		/// Determine whether the entity is persistent, detached, or transient 
		/// </summary>
		/// <param name = "entity">The entity to check </param>
		/// <param name = "entityName">The name of the entity </param>
		/// <param name = "entry">The entity's entry in the persistence context </param>
		/// <param name = "source">The originating session. </param>
		/// <returns> The state. </returns>
		protected virtual async Task<EntityState> GetEntityStateAsync(object entity, string entityName, EntityEntry entry, ISessionImplementor source)
		{
			if (entry != null)
			{
				// the object is persistent
				//the entity is associated with the session, so check its status
				if (entry.Status != Status.Deleted)
				{
					// do nothing for persistent instances
					if (log.IsDebugEnabled)
					{
						log.Debug("persistent instance of: " + GetLoggableName(entityName, entity));
					}

					return EntityState.Persistent;
				}
				else
				{
					//ie. e.status==DELETED
					if (log.IsDebugEnabled)
					{
						log.Debug("deleted instance of: " + GetLoggableName(entityName, entity));
					}

					return EntityState.Deleted;
				}
			}
			else
			{
				//the object is transient or detached
				//the entity is not associated with the session, so
				//try interceptor and unsaved-value
				if (await (ForeignKeys.IsTransientAsync(entityName, entity, AssumedUnsaved, source)))
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("transient instance of: " + GetLoggableName(entityName, entity));
					}

					return EntityState.Transient;
				}
				else
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("detached instance of: " + GetLoggableName(entityName, entity));
					}

					return EntityState.Detached;
				}
			}
		}
	}
}
#endif
