﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Engine;
using NHibernate.Id.Insert;

namespace NHibernate.Id
{
	using System.Threading.Tasks;
	using System.Threading;
	using System;
	/// <content>
	/// Contains generated async methods
	/// </content>
	public abstract partial class AbstractPostInsertGenerator : IPostInsertIdentifierGenerator
	{
		/// <summary>
		/// The IdentityGenerator for autoincrement/identity key generation. 
		/// </summary>
		/// <param name="s">The <see cref="ISessionImplementor"/> this id is being generated in.</param>
		/// <param name="obj">The entity the id is being generated for.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>
		/// <c>IdentityColumnIndicator</c> Indicates to the Session that identity (i.e. identity/autoincrement column)
		/// key generation should be used.
		/// </returns>
		public Task<object> GenerateAsync(ISessionImplementor s, object obj, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Task.FromResult<object>(Generate(s, obj));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#region IPostInsertIdentifierGenerator Members

		#endregion
	}
}
