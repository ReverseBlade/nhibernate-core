﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	public abstract partial class EnumStringType : AbstractEnumType
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cached"></param>
		/// <param name="session"></param>
		/// <param name="owner"></param>
		/// <returns></returns>
		public override Task<object> AssembleAsync(object cached, ISessionImplementor session, object owner)
		{
			try
			{
				if (cached == null)
				{
					return Task.FromResult<object>(null);
				}
				else
				{
					return Task.FromResult<object>(GetInstance(cached));
				}
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}
	}
}