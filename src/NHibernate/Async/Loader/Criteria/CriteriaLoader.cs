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
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Param;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Loader.Criteria
{
	using System.Threading.Tasks;
	/// <summary>
	/// A <c>Loader</c> for <see cref="ICriteria"/> queries. 
	/// </summary>
	/// <remarks>
	/// Note that criteria
	/// queries are more like multi-object <c>Load()</c>s than like HQL queries.
	/// </remarks>
	public partial class CriteriaLoader : OuterJoinLoader
	{

		public Task<IList> ListAsync(ISessionImplementor session)
		{
			try
			{
				return ListAsync(session, translator.GetQueryParameters(), querySpaces, resultTypes);
			}
			catch (Exception ex)
			{
				return Task.FromException<IList>(ex);
			}
		}

		protected override async Task<object> GetResultColumnOrRowAsync(object[] row, IResultTransformer customResultTransformer, DbDataReader rs,
													   ISessionImplementor session)
		{
			return ResolveResultTransformer(customResultTransformer)
				.TransformTuple(await (GetResultRowAsync(row, rs, session)).ConfigureAwait(false), ResultRowAliases);
		}


		protected override async Task<object[]> GetResultRowAsync(object[] row, DbDataReader rs, ISessionImplementor session)
		{
			object[] result;

			if (translator.HasProjection)
			{
				result = new object[ResultTypes.Length];

				for (int i = 0, position = 0; i < result.Length; i++)
				{
					int numColumns = ResultTypes[i].GetColumnSpan(session.Factory);

					if (numColumns > 1)
					{
						string[] typeColumnAliases = ArrayHelper.Slice(cachedProjectedColumnAliases, position, numColumns);
						result[i] = await (ResultTypes[i].NullSafeGetAsync(rs, typeColumnAliases, session, null)).ConfigureAwait(false);
					}
					else
					{
						result[i] = await (ResultTypes[i].NullSafeGetAsync(rs, cachedProjectedColumnAliases[position], session, null)).ConfigureAwait(false);
					}
					position += numColumns;
				}
			}
			else
			{
				result = ToResultRow(row);
			}
			return result;
		}
	}
}
