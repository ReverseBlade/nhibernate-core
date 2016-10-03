﻿#if NET_4_5
using System;
using System.Collections;
using System.Data.Common;
using System.Text;
using System.Xml;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NHibernate.Type
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public abstract partial class EntityType : AbstractType, IAssociationType
	{
		public override Task<object> NullSafeGetAsync(DbDataReader rs, string name, ISessionImplementor session, object owner)
		{
			return NullSafeGetAsync(rs, new string[]{name}, session, owner);
		}

		protected internal async Task<object> GetIdentifierAsync(object value, ISessionImplementor session)
		{
			if (IsNotEmbedded(session))
			{
				return value;
			}

			return await (ForeignKeys.GetEntityIdentifierIfNotUnsavedAsync(GetAssociatedEntityName(), value, session)); //tolerates nulls
		}

		protected internal async Task<object> GetReferenceValueAsync(object value, ISessionImplementor session)
		{
			if (IsNotEmbedded(session))
			{
				return value;
			}

			if (value == null)
			{
				return null;
			}
			else if (IsReferenceToPrimaryKey)
			{
				return await (ForeignKeys.GetEntityIdentifierIfNotUnsavedAsync(GetAssociatedEntityName(), value, session)); //tolerates nulls
			}
			else
			{
				IEntityPersister entityPersister = session.Factory.GetEntityPersister(GetAssociatedEntityName());
				object propertyValue = entityPersister.GetPropertyValue(value, uniqueKeyPropertyName, session.EntityMode);
				// We now have the value of the property-ref we reference.  However,
				// we need to dig a little deeper, as that property might also be
				// an entity type, in which case we need to resolve its identitifier
				IType type = entityPersister.GetPropertyType(uniqueKeyPropertyName);
				if (type.IsEntityType)
				{
					propertyValue = await (((EntityType)type).GetReferenceValueAsync(propertyValue, session));
				}

				return propertyValue;
			}
		}

		public override async Task<object> ReplaceAsync(object original, object target, ISessionImplementor session, object owner, IDictionary copyCache)
		{
			if (original == null)
			{
				return null;
			}

			object cached = copyCache[original];
			if (cached != null)
			{
				return cached;
			}
			else
			{
				if (original == target)
				{
					return target;
				}

				if (session.GetContextEntityIdentifier(original) == null && await (ForeignKeys.IsTransientAsync(associatedEntityName, original, false, session)))
				{
					object copy = session.Factory.GetEntityPersister(associatedEntityName).Instantiate(null, session.EntityMode);
					//TODO: should this be Session.instantiate(Persister, ...)?
					copyCache.Add(original, copy);
					return copy;
				}
				else
				{
					object id = await (GetReferenceValueAsync(original, session));
					if (id == null)
					{
						throw new AssertionFailure("non-transient entity has a null id");
					}

					id = await (GetIdentifierOrUniqueKeyType(session.Factory).ReplaceAsync(id, null, session, owner, copyCache));
					return await (ResolveIdentifierAsync(id, session, owner));
				}
			}
		}

		/// <summary>
		/// Converts the id contained in the <see cref = "DbDataReader"/> to an object.
		/// </summary>
		/// <param name = "rs">The <see cref = "DbDataReader"/> that contains the query results.</param>
		/// <param name = "names">A string array of column names that contain the id.</param>
		/// <param name = "session">The <see cref = "ISessionImplementor"/> this is occurring in.</param>
		/// <param name = "owner">The object that this Entity will be a part of.</param>
		/// <returns>
		/// An instance of the object or <see langword = "null"/> if the identifer was null.
		/// </returns>
		public override sealed async Task<object> NullSafeGetAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			return await (ResolveIdentifierAsync(await (HydrateAsync(rs, names, session, owner)), session, owner));
		}

		public abstract override Task<object> HydrateAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner);
		/// <summary>
		/// Resolves the identifier to the actual object.
		/// </summary>
		protected async Task<object> ResolveIdentifierAsync(object id, ISessionImplementor session)
		{
			string entityName = GetAssociatedEntityName();
			bool isProxyUnwrapEnabled = unwrapProxy && session.Factory.GetEntityPersister(entityName).IsInstrumented(session.EntityMode);
			object proxyOrEntity = await (session.InternalLoadAsync(entityName, id, eager, IsNullable && !isProxyUnwrapEnabled));
			if (proxyOrEntity.IsProxy())
			{
				INHibernateProxy proxy = (INHibernateProxy)proxyOrEntity;
				proxy.HibernateLazyInitializer.Unwrap = isProxyUnwrapEnabled;
			}

			return proxyOrEntity;
		}

		/// <summary>
		/// Resolve an identifier or unique key value
		/// </summary>
		/// <param name = "value"></param>
		/// <param name = "session"></param>
		/// <param name = "owner"></param>
		/// <returns></returns>
		public override async Task<object> ResolveIdentifierAsync(object value, ISessionImplementor session, object owner)
		{
			if (IsNotEmbedded(session))
			{
				return value;
			}

			if (value == null)
			{
				return null;
			}
			else
			{
				if (IsNull(owner, session))
				{
					return null; //EARLY EXIT!
				}

				if (IsReferenceToPrimaryKey)
				{
					return await (ResolveIdentifierAsync(value, session));
				}
				else
				{
					return await (LoadByUniqueKeyAsync(GetAssociatedEntityName(), uniqueKeyPropertyName, value, session));
				}
			}
		}

		/// <summary> 
		/// Load an instance by a unique key that is not the primary key. 
		/// </summary>
		/// <param name = "entityName">The name of the entity to load </param>
		/// <param name = "uniqueKeyPropertyName">The name of the property defining the unique key. </param>
		/// <param name = "key">The unique key property value. </param>
		/// <param name = "session">The originating session. </param>
		/// <returns> The loaded entity </returns>
		public async Task<object> LoadByUniqueKeyAsync(string entityName, string uniqueKeyPropertyName, object key, ISessionImplementor session)
		{
			ISessionFactoryImplementor factory = session.Factory;
			IUniqueKeyLoadable persister = (IUniqueKeyLoadable)factory.GetEntityPersister(entityName);
			//TODO: implement caching?! proxies?!
			EntityUniqueKey euk = new EntityUniqueKey(entityName, uniqueKeyPropertyName, key, GetIdentifierOrUniqueKeyType(factory), session.EntityMode, session.Factory);
			IPersistenceContext persistenceContext = session.PersistenceContext;
			try
			{
				object result = persistenceContext.GetEntity(euk);
				if (result == null)
				{
					result = await (persister.LoadByUniqueKeyAsync(uniqueKeyPropertyName, key, session));
				}

				return result == null ? null : persistenceContext.ProxyFor(result);
			}
			catch (HibernateException)
			{
				// Do not call Convert on HibernateExceptions
				throw;
			}
			catch (Exception sqle)
			{
				throw ADOExceptionHelper.Convert(factory.SQLExceptionConverter, sqle, "Error performing LoadByUniqueKey");
			}
		}
	}
}
#endif
