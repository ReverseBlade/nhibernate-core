﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public partial class ClassMetaType : AbstractType
	{

		public override Task<object> NullSafeGetAsync(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			return NullSafeGetAsync(rs, names[0], session, owner);
		}

		public override async Task<object> NullSafeGetAsync(DbDataReader rs,string name,ISessionImplementor session,object owner)
		{
			int index = rs.GetOrdinal(name);

			if (await (rs.IsDBNullAsync(index)).ConfigureAwait(false))
			{
				return null;
			}
			else
			{
				string str = (string) NHibernateUtil.String.Get(rs, index);
				return string.IsNullOrEmpty(str) ? null : str;
			}
		}

		public override Task<object> ReplaceAsync(object original, object current, ISessionImplementor session, object owner, System.Collections.IDictionary copiedAlready)
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
