﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.Id.Insert;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace NHibernate.Id
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class SelectGenerator : AbstractPostInsertGenerator, IConfigurable
	{

		#region Overrides of AbstractPostInsertGenerator

		#endregion
		#region Implementation of IConfigurable

		#endregion

		#region Nested type: SelectGeneratorDelegate

		/// <content>
		/// Contains generated async methods
		/// </content>
		public partial class SelectGeneratorDelegate : AbstractSelectingDelegate
		{

			protected internal override async Task<object> GetResultAsync(ISessionImplementor session, DbDataReader rs, object entity, CancellationToken cancellationToken = default(CancellationToken))
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (!await (rs.ReadAsync(cancellationToken)).ConfigureAwait(false))
				{
					throw new IdentifierGenerationException("the inserted row could not be located by the unique key: "
					                                        + uniqueKeyPropertyName);
				}
				return await (idType.NullSafeGetAsync(rs, persister.RootTableKeyColumnNames, session, entity, cancellationToken)).ConfigureAwait(false);
			}
		}

		#endregion
	}
}