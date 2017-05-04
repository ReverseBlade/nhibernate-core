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
using NHibernate.Engine;
using NHibernate.Engine.Query;
using NHibernate.Hql;
using NHibernate.Properties;
using NHibernate.Proxy;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;
using System.Linq;

namespace NHibernate.Impl
{
	using System.Threading.Tasks;
	/// <summary>
	/// Abstract implementation of the IQuery interface.
	/// </summary>
	public abstract partial class AbstractQueryImpl : IQuery
	{

		#region Parameters

		#endregion
		#region Query properties

		#endregion

		#region Execution methods

		public abstract Task<int> ExecuteUpdateAsync();
		public abstract Task<IEnumerable> EnumerableAsync();
		public abstract Task<IEnumerable<T>> EnumerableAsync<T>();
		public abstract Task<IList> ListAsync();
		public abstract Task ListAsync(IList results);
		public abstract Task<IList<T>> ListAsync<T>();
		public async Task<T> UniqueResultAsync<T>()
		{
			object result = await (UniqueResultAsync()).ConfigureAwait(false);
			if (result == null && typeof(T).IsValueType)
			{
				return default(T);
			}
			else
			{
				return (T)result;
			}
		}

		public async Task<object> UniqueResultAsync()
		{
			return UniqueElement(await (ListAsync()).ConfigureAwait(false));
		}

		#endregion

		protected internal abstract Task<IEnumerable<ITranslator>> GetTranslatorsAsync(ISessionImplementor sessionImplementor, QueryParameters queryParameters);
	}
}
