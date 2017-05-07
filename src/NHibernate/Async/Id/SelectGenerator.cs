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
	/// <summary> 
	/// A generator that selects the just inserted row to determine the identifier
	/// value assigned by the database. The correct row is located using a unique key.
	/// </summary>
	/// <remarks>One mapping parameter is required: key (unless a natural-id is defined in the mapping).</remarks>
	public partial class SelectGenerator : AbstractPostInsertGenerator, IConfigurable
	{

		#region Overrides of AbstractPostInsertGenerator

		#endregion
		#region Implementation of IConfigurable

		#endregion

		#region Nested type: SelectGeneratorDelegate

		/// <summary> The delegate for the select generation strategy.</summary>
		public partial class SelectGeneratorDelegate : AbstractSelectingDelegate
		{

			protected internal override async Task<object> GetResultAsync(ISessionImplementor session, DbDataReader rs, object entity)
			{
				if (!await (rs.ReadAsync()).ConfigureAwait(false))
				{
					throw new IdentifierGenerationException("the inserted row could not be located by the unique key: "
					                                        + uniqueKeyPropertyName);
				}
				return await (idType.NullSafeGetAsync(rs, persister.RootTableKeyColumnNames, session, entity)).ConfigureAwait(false);
			}
		}

		#endregion
	}
}