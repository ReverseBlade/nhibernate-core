﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;

namespace NHibernate.Impl
{
	using System.Threading.Tasks;
	public partial class FutureCriteriaBatch : FutureBatch<ICriteria, IMultiCriteria>
	{

		protected override Task<IList> GetResultsFromAsync(IMultiCriteria multiApproach)
		{
			return multiApproach.ListAsync();
		}
	}
}
