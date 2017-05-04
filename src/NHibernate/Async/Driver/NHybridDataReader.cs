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
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace NHibernate.Driver
{
	/// <summary>
	/// An implementation of <see cref="DbDataReader"/> that will work with either an 
	/// <see cref="DbDataReader"/> returned by Execute or with an <see cref="DbDataReader"/>
	/// whose contents have been read into a <see cref="NDataReader"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This allows NHibernate to use the underlying <see cref="DbDataReader"/> for as long as
	/// possible without the need to read everything into the <see cref="NDataReader"/>.
	/// </para>
	/// <para>
	/// The consumer of the <see cref="DbDataReader"/> returned from <see cref="Engine.IBatcher"/> does
	/// not need to know the underlying reader and can use it the same even if it switches from an
	/// <see cref="DbDataReader"/> to <see cref="NDataReader"/> in the middle of its use.
	/// </para>
	/// </remarks>
	public partial class NHybridDataReader : DbDataReader
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="NHybridDataReader"/> class.
		/// </summary>
		/// <param name="reader">The underlying DbDataReader to use.</param>
		public static Task<NHybridDataReader> CreateAsync(DbDataReader reader)
		{
			return CreateAsync(reader, false);
		}

		/// <summary>
		/// Initializes a new instance of the NHybridDataReader class.
		/// </summary>
		/// <param name="reader">The underlying DbDataReader to use.</param>
		/// <param name="inMemory"><see langword="true" /> if the contents of the DbDataReader should be read into memory right away.</param>
		public static async Task<NHybridDataReader> CreateAsync(DbDataReader reader, bool inMemory)
		{
			var dataReader = new NHybridDataReader();
			if (inMemory)
			{
				dataReader._reader = await (NDataReader.CreateAsync(reader, false)).ConfigureAwait(false);
			}
			else
			{
				dataReader._reader = reader;
			}
			return dataReader;
		}

		/// <summary>
		/// Reads all of the contents into memory because another <see cref="DbDataReader"/>
		/// needs to be opened.
		/// </summary>
		/// <remarks>
		/// This will result in a no op if the reader is closed or is already in memory.
		/// </remarks>
		public async Task ReadIntoMemoryAsync()
		{
			if (_reader.IsClosed == false && _reader.GetType() != typeof(NDataReader))
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("Moving DbDataReader into an NDataReader.  It was converted in midstream " + _isMidstream.ToString());
				}
				_reader = await (NDataReader.CreateAsync(_reader, _isMidstream)).ConfigureAwait(false);
			}
		}
	}
}