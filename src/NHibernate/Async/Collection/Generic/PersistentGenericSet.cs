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
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Collection.Generic.SetHelpers;
using NHibernate.Collection.Trackers;
using NHibernate.DebugHelpers;
using NHibernate.Engine;
using NHibernate.Linq;
using NHibernate.Loader;
using NHibernate.Persister.Collection;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Collection.Generic
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class PersistentGenericSet<T> : AbstractPersistentCollection, ISet<T>, IReadOnlyCollection<T>, IQueryable<T>
	{

		//Since 5.3
		/// <inheritdoc />
		[Obsolete("This method has no more usages and will be removed in a future version")]
		public override Task<ICollection> GetOrphansAsync(object snapshot, string entityName, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<ICollection>(cancellationToken);
			}
			try
			{
				return Task.FromResult<ICollection>(GetOrphans(snapshot, entityName));
			}
			catch (Exception ex)
			{
				return Task.FromException<ICollection>(ex);
			}
		}

		public override async Task<bool> EqualsSnapshotAsync(ICollectionPersister persister, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var elementType = persister.ElementType;
			var snapshot = (SetSnapShot<T>)GetSnapshot();
			if (((ICollection)snapshot).Count != WrappedSet.Count)
			{
				return false;
			}

			foreach (T obj in WrappedSet)
			{
				T oldValue;
				if (!snapshot.TryGetValue(obj, out oldValue) || await (elementType.IsDirtyAsync(oldValue, obj, Session, cancellationToken)).ConfigureAwait(false))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Initializes this PersistentSet from the cached values.
		/// </summary>
		/// <param name="persister">The CollectionPersister to use to reassemble the PersistentSet.</param>
		/// <param name="disassembled">The disassembled PersistentSet.</param>
		/// <param name="owner">The owner object.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public override async Task InitializeFromCacheAsync(ICollectionPersister persister, object disassembled, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var array = (object[])disassembled;
			int size = array.Length;
			BeforeInitialize(persister, size);

			var elementType = persister.ElementType;
			for (int i = 0; i < size; i++)
			{
				await (elementType.BeforeAssembleAsync(array[i], Session, cancellationToken)).ConfigureAwait(false);
			}

			for (int i = 0; i < size; i++)
			{
				var element = await (elementType.AssembleAsync(array[i], Session, owner, cancellationToken)).ConfigureAwait(false);
				if (element != null)
				{
					WrappedSet.Add((T) element);
				}
			}
			SetInitialized();
		}

		public override async Task<object> ReadFromAsync(DbDataReader rs, ICollectionPersister role, ICollectionAliases descriptor, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var element = await (role.ReadElementAsync(rs, owner, descriptor.SuffixedElementAliases, Session, cancellationToken)).ConfigureAwait(false);
			if (element != null)
			{
				_tempList.Add((T) element);
			}
			return element;
		}

		public override async Task<object> DisassembleAsync(ICollectionPersister persister, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var result = new object[WrappedSet.Count];
			int i = 0;

			foreach (object obj in WrappedSet)
			{
				result[i++] = await (persister.ElementType.DisassembleAsync(obj, Session, null, cancellationToken)).ConfigureAwait(false);
			}
			return result;
		}

		public override async Task<IEnumerable> GetDeletesAsync(ICollectionPersister persister, bool indexIsFormula, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			IType elementType = persister.ElementType;
			var sn = (SetSnapShot<T>)GetSnapshot();
			var deletes = new List<T>(((ICollection<T>)sn).Count);

			deletes.AddRange(sn.Where(obj => !WrappedSet.Contains(obj)));

			foreach (var obj in WrappedSet)
			{
				T oldValue;
				if (sn.TryGetValue(obj, out oldValue) && await (elementType.IsDirtyAsync(obj, oldValue, Session, cancellationToken)).ConfigureAwait(false))
					deletes.Add(oldValue);
			}

			return deletes;
		}

		public override async Task<bool> NeedsInsertingAsync(object entry, int i, IType elemType, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var sn = (SetSnapShot<T>)GetSnapshot();
			T oldKey;

			// note that it might be better to iterate the snapshot but this is safe,
			// assuming the user implements equals() properly, as required by the PersistentSet
			// contract!
			return !sn.TryGetValue((T) entry, out oldKey) || await (elemType.IsDirtyAsync(oldKey, entry, Session, cancellationToken)).ConfigureAwait(false);
		}

		public override Task<bool> NeedsUpdatingAsync(object entry, int i, IType elemType, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<bool>(cancellationToken);
			}
			try
			{
				return Task.FromResult<bool>(NeedsUpdating(entry, i, elemType));
			}
			catch (Exception ex)
			{
				return Task.FromException<bool>(ex);
			}
		}
	}
}
