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
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using System.Collections.Generic;
using System.Data;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class TimeSpanType : PrimitiveType, IVersionType, ILiteralType
	{

		#region IVersionType Members

		public Task<object> NextAsync(object current, ISessionImplementor session)
		{
			return SeedAsync(session);
		}

		/// <summary></summary>
		public virtual Task<object> SeedAsync(ISessionImplementor session)
		{
			try
			{
				return Task.FromResult<object>(Seed(session));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#endregion
	}
}