using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using NHibernate.DebugHelpers;
using NHibernate.Engine;
using NHibernate.Loader;
using NHibernate.Persister.Collection;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Collection.Generic
{
	/// <summary>
	/// A persistent wrapper for a <see cref="IDictionary{TKey,TValue}"/>.  Underlying
	/// collection is a <see cref="Dictionary{TKey,TValue}"/>
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the IDictionary.</typeparam>
	/// <typeparam name="TValue">The type of the elements in the IDictionary.</typeparam>
	[Serializable]
	[DebuggerTypeProxy(typeof(DictionaryProxy<,>))]
	public class PersistentGenericMap<TKey, TValue> : AbstractPersistentCollection, IDictionary<TKey, TValue>, ICollection
	{
		protected IDictionary<TKey, TValue> WrappedMap;

		public PersistentGenericMap() { }

		/// <summary>
		/// Construct an uninitialized PersistentGenericMap.
		/// </summary>
		/// <param name="session">The ISession the PersistentGenericMap should be a part of.</param>
		public PersistentGenericMap(ISessionImplementor session) : base(session) { }

		/// <summary>
		/// Construct an initialized PersistentGenericMap based off the values from the existing IDictionary.
		/// </summary>
		/// <param name="session">The ISession the PersistentGenericMap should be a part of.</param>
		/// <param name="map">The IDictionary that contains the initial values.</param>
		public PersistentGenericMap(ISessionImplementor session, IDictionary<TKey, TValue> map)
			: base(session)
		{
			WrappedMap = map;
			SetInitialized();
			IsDirectlyAccessible = true;
		}

		public override object GetSnapshot(ICollectionPersister persister)
		{
			EntityMode entityMode = Session.EntityMode;
			Dictionary<TKey, TValue> clonedMap = new Dictionary<TKey, TValue>(WrappedMap.Count);
			foreach (KeyValuePair<TKey, TValue> e in WrappedMap)
			{
				object copy = persister.ElementType.DeepCopy(e.Value, entityMode, persister.Factory);
				clonedMap[e.Key] = (TValue)copy;
			}
			return clonedMap;
		}

		public override Task<ICollection> GetOrphans(object snapshot, string entityName)
		{
			var sn = (IDictionary<TKey, TValue>)snapshot;
			return GetOrphans((ICollection)sn.Values, (ICollection)WrappedMap.Values, entityName, Session);
		}

		public override async Task<bool> EqualsSnapshot(ICollectionPersister persister)
		{
			IType elementType = persister.ElementType;
			var xmap = (IDictionary<TKey, TValue>)GetSnapshot();
			if (xmap.Count != WrappedMap.Count)
			{
				return false;
			}
			foreach (KeyValuePair<TKey, TValue> entry in WrappedMap)
			{
				if (await elementType.IsDirty(entry.Value, xmap[entry.Key], Session).ConfigureAwait(false))
				{
					return false;
				}
			}
			return true;
		}

		public override bool IsSnapshotEmpty(object snapshot)
		{
			return ((IDictionary)snapshot).Count == 0;
		}

		public override bool IsWrapper(object collection)
		{
			return WrappedMap == collection;
		}

		public override void BeforeInitialize(ICollectionPersister persister, int anticipatedSize)
		{
			WrappedMap = (IDictionary<TKey, TValue>)persister.CollectionType.Instantiate(anticipatedSize);
		}

		public override bool Empty
		{
			get { return (WrappedMap.Count == 0); }
		}

		public override string ToString()
		{
			Read();
			return StringHelper.CollectionToString(WrappedMap);
		}

		public override async Task<object> ReadFrom(IDataReader rs, ICollectionPersister role, ICollectionAliases descriptor, object owner)
		{
			object element = await role.ReadElement(rs, owner, descriptor.SuffixedElementAliases, Session).ConfigureAwait(false);
			object index = await role.ReadIndex(rs, descriptor.SuffixedIndexAliases, Session).ConfigureAwait(false);

			AddDuringInitialize(index, element);
			return element;
		}

		protected virtual void AddDuringInitialize(object index, object element)
		{
			WrappedMap[(TKey)index] = (TValue)element;
		}

		public override IEnumerable Entries(ICollectionPersister persister)
		{
			return WrappedMap;
		}

		/// <summary>
		/// Initializes this PersistentGenericMap from the cached values.
		/// </summary>
		/// <param name="persister">The CollectionPersister to use to reassemble the PersistentGenericMap.</param>
		/// <param name="disassembled">The disassembled PersistentGenericMap.</param>
		/// <param name="owner">The owner object.</param>
		public override async Task InitializeFromCache(ICollectionPersister persister, object disassembled, object owner)
		{
			object[] array = (object[])disassembled;
			int size = array.Length;
			BeforeInitialize(persister, size);
			for (int i = 0; i < size; i += 2)
			{
				WrappedMap[(TKey)await persister.IndexType.Assemble(array[i], Session, owner).ConfigureAwait(false)] =
					(TValue)await persister.ElementType.Assemble(array[i + 1], Session, owner).ConfigureAwait(false);
			}
		}

		public override async Task<object> Disassemble(ICollectionPersister persister)
		{
			object[] result = new object[WrappedMap.Count * 2];
			int i = 0;
			foreach (KeyValuePair<TKey, TValue> e in WrappedMap)
			{
				result[i++] = await persister.IndexType.Disassemble(e.Key, Session, null).ConfigureAwait(false);
				result[i++] = await persister.ElementType.Disassemble(e.Value, Session, null).ConfigureAwait(false);
			}
			return result;
		}

		public override Task<IEnumerable> GetDeletes(ICollectionPersister persister, bool indexIsFormula)
		{
			IList deletes = new List<object>();
			var sn = (IDictionary<TKey, TValue>)GetSnapshot();
			foreach (var e in sn)
			{
				if (!WrappedMap.ContainsKey(e.Key))
				{
					object key = e.Key;
					deletes.Add(indexIsFormula ? e.Value : key);
				}
			}
			return Task.FromResult<IEnumerable>(deletes);
		}

		public override Task<bool> NeedsInserting(object entry, int i, IType elemType)
		{
			var sn = (IDictionary)GetSnapshot();
			var e = (KeyValuePair<TKey, TValue>)entry;
			return Task.FromResult(!sn.Contains(e.Key));
		}

		public override async Task<bool> NeedsUpdating(object entry, int i, IType elemType)
		{
			var sn = (IDictionary)GetSnapshot();
			var e = (KeyValuePair<TKey, TValue>)entry;
			var snValue = sn[e.Key];
			var isNew = !sn.Contains(e.Key);
			return e.Value != null && snValue != null && await elemType.IsDirty(snValue, e.Value, Session).ConfigureAwait(false)
				|| (!isNew && ((e.Value == null) != (snValue == null)));
		}

		public override object GetIndex(object entry, int i, ICollectionPersister persister)
		{
			return ((KeyValuePair<TKey, TValue>)entry).Key;
		}

		public override object GetElement(object entry)
		{
			return ((KeyValuePair<TKey, TValue>)entry).Value;
		}

		public override object GetSnapshotElement(object entry, int i)
		{
			var sn = (IDictionary)GetSnapshot();
			return sn[((KeyValuePair<TKey, TValue>)entry).Key];
		}

		public override bool Equals(object other)
		{
			var that = other as IDictionary<TKey, TValue>;
			if (that == null)
			{
				return false;
			}
			Read();
			return CollectionHelper.DictionaryEquals(WrappedMap, that);
		}

		public override int GetHashCode()
		{
			Read();
			return WrappedMap.GetHashCode();
		}

		public override bool EntryExists(object entry, int i)
		{
			return WrappedMap.ContainsKey(((KeyValuePair<TKey, TValue>)entry).Key);
		}


		#region IDictionary<TKey,TValue> Members

		public bool ContainsKey(TKey key)
		{
			bool? exists = ReadIndexExistence(key).ConfigureAwait(false).GetAwaiter().GetResult();
			return !exists.HasValue ? WrappedMap.ContainsKey(key) : exists.Value;
		}

		public void Add(TKey key, TValue value)
		{
			AddAsync(key, value).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public async Task AddAsync(TKey key, TValue value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (PutQueueEnabled)
			{
				object old = await ReadElementByIndex(key).ConfigureAwait(false);
				if (old != Unknown)
				{
					QueueOperation(new PutDelayedOperation(this, key, value, old == NotFound ? null : old));
					return;
				}
			}
			await Initialize(true).ConfigureAwait(false);
			WrappedMap.Add(key, value);
			Dirty();
		}

		public bool Remove(TKey key)
		{
			return RemoveAsync(key).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public async Task<bool> RemoveAsync(TKey key)
		{
			object old = PutQueueEnabled ? await ReadElementByIndex(key).ConfigureAwait(false) : Unknown;
			if (old == Unknown) // queue is not enabled for 'puts', or element not found
			{
				await Initialize(true).ConfigureAwait(false);
				bool contained = WrappedMap.Remove(key);
				if (contained)
				{
					Dirty();
				}
				return contained;
			}

			QueueOperation(new RemoveDelayedOperation(this, key, old == NotFound ? null : old));
			return true;
		}


		public bool TryGetValue(TKey key, out TValue value)
		{
			object result = ReadElementByIndex(key).ConfigureAwait(false).GetAwaiter().GetResult();
			if (result == Unknown)
			{
				return WrappedMap.TryGetValue(key, out value);
			}
			if (result == NotFound)
			{
				value = default(TValue);
				return false;
			}
			value = (TValue)result;
			return true;
		}

		public TValue this[TKey key]
		{
			get
			{
				object result = ReadElementByIndex(key).ConfigureAwait(false).GetAwaiter().GetResult();
				if (result == Unknown)
				{
					return WrappedMap[key];
				}
				if (result == NotFound)
				{
					throw new KeyNotFoundException();
				}
				return (TValue)result;
			}
			set
			{
				// NH Note: the assignment in NET work like the put method in JAVA (mean assign or add)
				if (PutQueueEnabled)
				{
					object old = ReadElementByIndex(key).ConfigureAwait(false).GetAwaiter().GetResult();
					if (old != Unknown)
					{
						QueueOperation(new PutDelayedOperation(this, key, value, old == NotFound ? null : old));
						return;
					}
				}
				Initialize(true).ConfigureAwait(false).GetAwaiter().GetResult();
				TValue tempObject;
				WrappedMap.TryGetValue(key, out tempObject);
				WrappedMap[key] = value;
				TValue old2 = tempObject;
				// would be better to use the element-type to determine
				// whether the old and the new are equal here; the problem being
				// we do not necessarily have access to the element type in all
				// cases
				if (!ReferenceEquals(value, old2))
				{
					Dirty();
				}
			}
		}

		public ICollection<TKey> Keys
		{
			get
			{
				Read();
				return WrappedMap.Keys;
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				Read();
				return WrappedMap.Values;
			}
		}

		#endregion

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public override async Task<object> AddAsync(object obj)
		{
			var item = (KeyValuePair<TKey, TValue>) obj;
			await AddAsync(item.Key, item.Value).ConfigureAwait(false);
			return null;
		}

		public void Clear()
		{
			ClearAsync().ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public override async Task ClearAsync()
		{
			if (ClearQueueEnabled)
			{
				QueueOperation(new ClearDelayedOperation(this));
			}
			else
			{
				await Initialize(true).ConfigureAwait(false);
				if (WrappedMap.Count != 0)
				{
					Dirty();
					WrappedMap.Clear();
				}
			}
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ContainsAsync(item).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public override async Task<bool> ContainsAsync(object obj)
		{
			var item = (KeyValuePair<TKey, TValue>) obj;
			bool? exists = await ReadIndexExistence(item.Key).ConfigureAwait(false);
			if (!exists.HasValue)
			{
				return WrappedMap.Contains(item);
			}

			if (exists.Value)
			{
				TValue x = ((IDictionary<TKey, TValue>)this)[item.Key];
				TValue y = item.Value;
				return EqualityComparer<TValue>.Default.Equals(x, y);
			}

			return false;
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			int c = Count;
			var keys = new TKey[c];
			var values = new TValue[c];
			if (Keys != null)
			{
				Keys.CopyTo(keys, arrayIndex);
			}
			if (Values != null)
			{
				Values.CopyTo(values, arrayIndex);
			}
			for (int i = arrayIndex; i < c; i++)
			{
				if (keys[i] != null || values[i] != null)
				{
					array.SetValue(new KeyValuePair<TKey, TValue>(keys[i], values[i]), i);
				}
			}
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return RemoveAsync(item).ConfigureAwait(false).GetAwaiter().GetResult();
		}

		public override async Task<bool> RemoveAsync(object obj)
		{
			var item = (KeyValuePair<TKey, TValue>) obj;
			if(await ContainsAsync(item).ConfigureAwait(false))
			{
				await RemoveAsync(item.Key);
				return true;
			}

			return false;
		}

		public int Count
		{
			get
			{
				return CountAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}

		public override async Task<int> CountAsync()
		{
			return await ReadSize().ConfigureAwait(false) ? CachedSize : WrappedMap.Count;
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region ICollection Members

		public void CopyTo(Array array, int index)
		{
			CopyTo((KeyValuePair<TKey, TValue>[])array, index);
		}

		public object SyncRoot
		{
			get { return this; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		#endregion

		#region IEnumerable<KeyValuePair<TKey,TValue>> Members

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			Read();
			return WrappedMap.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			Read();
			return WrappedMap.GetEnumerator();
		}

		#endregion

		#region DelayedOperations

		protected sealed class ClearDelayedOperation : IDelayedOperation
		{
			private readonly PersistentGenericMap<TKey, TValue> _enclosingInstance;

			public ClearDelayedOperation(PersistentGenericMap<TKey, TValue> enclosingInstance)
			{
				_enclosingInstance = enclosingInstance;
			}

			public object AddedInstance
			{
				get { return null; }
			}

			public object Orphan
			{
				get { throw new NotSupportedException("queued clear cannot be used with orphan delete"); }
			}

			public void Operate()
			{
				_enclosingInstance.WrappedMap.Clear();
			}
		}

		protected sealed class PutDelayedOperation : IDelayedOperation
		{
			private readonly PersistentGenericMap<TKey, TValue> _enclosingInstance;
			private readonly TKey _index;
			private readonly TValue _value;
			private readonly object _old;

			public PutDelayedOperation(PersistentGenericMap<TKey, TValue> enclosingInstance, TKey index, TValue value, object old)
			{
				_enclosingInstance = enclosingInstance;
				_index = index;
				_value = value;
				_old = old;
			}

			public object AddedInstance
			{
				get { return _value; }
			}

			public object Orphan
			{
				get { return _old; }
			}

			public void Operate()
			{
				_enclosingInstance.WrappedMap[_index] = _value;
			}
		}

		protected sealed class RemoveDelayedOperation : IDelayedOperation
		{
			private readonly PersistentGenericMap<TKey, TValue> _enclosingInstance;
			private readonly TKey _index;
			private readonly object _old;

			public RemoveDelayedOperation(PersistentGenericMap<TKey, TValue> enclosingInstance, TKey index, object old)
			{
				_enclosingInstance = enclosingInstance;
				_index = index;
				_old = old;
			}

			public object AddedInstance
			{
				get { return null; }
			}

			public object Orphan
			{
				get { return _old; }
			}

			public void Operate()
			{
				_enclosingInstance.WrappedMap.Remove(_index);
			}
		}

		#endregion
	}
}