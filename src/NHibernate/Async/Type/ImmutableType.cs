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
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public abstract partial class ImmutableType : NullableType
	{

		public override Task<object> ReplaceAsync(object original, object current, ISessionImplementor session, object owner,
									   IDictionary copiedAlready)
		{
			try
			{
				return Task.FromResult<object>(Replace(original, current, session, owner, copiedAlready));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

	}
}