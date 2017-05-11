﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;

namespace NHibernate.Param
{
	using System.Threading.Tasks;
	using System.Threading;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class VersionTypeSeedParameterSpecification : IParameterSpecification
	{

		#region IParameterSpecification Members

		public async Task BindAsync(DbCommand command, IList<Parameter> sqlQueryParametersList, QueryParameters queryParameters, ISessionImplementor session, CancellationToken cancellationToken = default(CancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			int position = sqlQueryParametersList.GetEffectiveParameterLocations(IdBackTrack).Single(); // version parameter can't appear more than once
			type.NullSafeSet(command, await (type.SeedAsync(session, cancellationToken)).ConfigureAwait(false), position, session);
		}

		#endregion
	}
}