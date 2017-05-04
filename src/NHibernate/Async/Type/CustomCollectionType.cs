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
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.UserTypes;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	public partial class CustomCollectionType : CollectionType
	{

		public override Task<object> ReplaceElementsAsync(object original, object target, object owner, IDictionary copyCache,
		                                       ISessionImplementor session)
		{
			try
			{
				ICollectionPersister cp = session.Factory.GetCollectionPersister(Role);
				return Task.FromResult<object>(userType.ReplaceElements(original, target, cp, owner, copyCache, session));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}
	}
}