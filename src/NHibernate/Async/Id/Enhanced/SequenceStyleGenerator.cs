﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;

using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Id.Enhanced
{
	using System.Threading.Tasks;
	using System.Threading;
	using System;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class SequenceStyleGenerator : IPersistentIdentifierGenerator, IConfigurable
	{

		#region Implementation of IConfigurable


		#endregion

		#region Implementation of IIdentifierGenerator

		public virtual Task<object> GenerateAsync(ISessionImplementor session, object obj, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Optimizer.GenerateAsync(DatabaseStructure.BuildCallback(session), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#endregion
		#region Implementation of IPersistentIdentifierGenerator

		#endregion
	}
}