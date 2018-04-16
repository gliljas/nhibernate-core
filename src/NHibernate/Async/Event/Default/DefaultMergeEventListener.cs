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
using System.Linq;
using NHibernate.Classic;
using NHibernate.Engine;
using NHibernate.Intercept;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.Type;


namespace NHibernate.Event.Default
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class DefaultMergeEventListener : AbstractSaveEventListener, IMergeEventListener
	{

		public virtual async Task OnMergeAsync(MergeEvent @event, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			EventCache copyCache = new EventCache();
			
			await (OnMergeAsync(@event, copyCache, cancellationToken)).ConfigureAwait(false);

			// transientCopyCache may contain parent and child entities in random order.
			// Child entities occurring ahead of their respective transient parents may fail 
			// to get merged in one iteration.
			// Retries are necessary as more and more children may be able to merge on subsequent iterations.
			// Iteratively get transient entities and retry merge until one of the following conditions is true:
			//   1) transientCopyCache.size() == 0
			//   2) transientCopyCache.size() is not decreasing
			
			// TODO: find out if retrying can add entities to copyCache (don't think it can...)
			// For now, just retry once; throw TransientObjectException if there are still any transient entities
			
			IDictionary transientCopyCache = await (this.GetTransientCopyCacheAsync(@event, copyCache, cancellationToken)).ConfigureAwait(false);
			
			while (transientCopyCache.Count > 0)
			{
				var initialTransientCount = transientCopyCache.Count;

				await (RetryMergeTransientEntitiesAsync(@event, transientCopyCache, copyCache, cancellationToken)).ConfigureAwait(false);
				
				// find any entities that are still transient after retry
				transientCopyCache = await (this.GetTransientCopyCacheAsync(@event, copyCache, cancellationToken)).ConfigureAwait(false);

				// if a retry did nothing, the remaining transient entities 
				// cannot be merged due to references to other transient entities 
				// that are not part of the merge
				if (transientCopyCache.Count == initialTransientCount)
				{
					ISet<string> transientEntityNames = new HashSet<string>();
					
					foreach (object transientEntity in transientCopyCache.Keys)
					{
						string transientEntityName = @event.Session.GuessEntityName(transientEntity);
						
						transientEntityNames.Add(transientEntityName);
						
						log.Info(
							"transient instance could not be processed by merge: {0} [{1}]",
							transientEntityName,
							transientEntity.ToString());
					}

					throw new TransientObjectException("one or more objects is an unsaved transient instance - save transient instance(s) before merging: " + String.Join(",",  transientEntityNames.ToArray()));
				}
			}

			copyCache.Clear();
		}
		
		public virtual async Task OnMergeAsync(MergeEvent @event, IDictionary copiedAlready, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			EventCache copyCache = (EventCache)copiedAlready;
			IEventSource source = @event.Session;
			object original = @event.Original;

			if (original != null)
			{
				object entity;
				if (original.IsProxy())
				{
					ILazyInitializer li = ((INHibernateProxy)original).HibernateLazyInitializer;
					if (li.IsUninitialized)
					{
						log.Debug("ignoring uninitialized proxy");
						@event.Result = await (source.LoadAsync(li.EntityName, li.Identifier, cancellationToken)).ConfigureAwait(false);
						return; //EARLY EXIT!
					}
					else
					{
						entity = await (li.GetImplementationAsync(cancellationToken)).ConfigureAwait(false);
					}
				}
				else
				{
					entity = original;
				}
				
				if (copyCache.Contains(entity) && copyCache.IsOperatedOn(entity))
				{
					log.Debug("already in merge process");
					@event.Result = entity;
				}
				else
				{
					if (copyCache.Contains(entity))
					{
						log.Info("already in copyCache; setting in merge process");
						copyCache.SetOperatedOn(entity, true);
					}
					
					@event.Entity = entity;
					EntityState entityState = EntityState.Undefined;
					if (ReferenceEquals(null, @event.EntityName))
					{
						@event.EntityName = source.BestGuessEntityName(entity);
					}

					// Check the persistence context for an entry relating to this
					// entity to be merged...
					EntityEntry entry = source.PersistenceContext.GetEntry(entity);
					if (entry == null)
					{
						IEntityPersister persister = source.GetEntityPersister(@event.EntityName, entity);
						object id = persister.GetIdentifier(entity);
						if (id != null)
						{
							EntityKey key = source.GenerateEntityKey(id, persister);
							object managedEntity = source.PersistenceContext.GetEntity(key);
							entry = source.PersistenceContext.GetEntry(managedEntity);
							if (entry != null)
							{
								// we have specialized case of a detached entity from the
								// perspective of the merge operation.  Specifically, we
								// have an incoming entity instance which has a corresponding
								// entry in the current persistence context, but registered
								// under a different entity instance
								entityState = EntityState.Detached;
							}
						}
					}

					if (entityState == EntityState.Undefined)
					{
						entityState = await (GetEntityStateAsync(entity, @event.EntityName, entry, source, cancellationToken)).ConfigureAwait(false);
					}

					switch (entityState)
					{
						case EntityState.Persistent:
							await (EntityIsPersistentAsync(@event, copyCache, cancellationToken)).ConfigureAwait(false);
							break;
						case EntityState.Transient:
							await (EntityIsTransientAsync(@event, copyCache, cancellationToken)).ConfigureAwait(false);
							break;
						case EntityState.Detached:
							await (EntityIsDetachedAsync(@event, copyCache, cancellationToken)).ConfigureAwait(false);
							break;
						default:
							throw new ObjectDeletedException("deleted instance passed to merge", null, GetLoggableName(@event.EntityName, entity));
					}
				}
			}
		}

		protected virtual async Task EntityIsPersistentAsync(MergeEvent @event, IDictionary copyCache, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			log.Debug("ignoring persistent instance");

			//TODO: check that entry.getIdentifier().equals(requestedId)
			
			object entity = @event.Entity;
			IEventSource source = @event.Session;
			IEntityPersister persister = source.GetEntityPersister(@event.EntityName, entity);

			((EventCache)copyCache).Add(entity, entity, true); //before cascade!

			await (CascadeOnMergeAsync(source, persister, entity, copyCache, cancellationToken)).ConfigureAwait(false);
			await (CopyValuesAsync(persister, entity, entity, source, copyCache, cancellationToken)).ConfigureAwait(false);

			@event.Result = entity;
		}

		protected virtual async Task EntityIsTransientAsync(MergeEvent @event, IDictionary copyCache, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			log.Info("merging transient instance");

			object entity = @event.Entity;
			IEventSource source = @event.Session;

			IEntityPersister persister = source.GetEntityPersister(@event.EntityName, entity);
			string entityName = persister.EntityName;
			
			@event.Result = await (this.MergeTransientEntityAsync(entity, entityName, @event.RequestedId, source, copyCache, cancellationToken)).ConfigureAwait(false);
		}
	
		private async Task<object> MergeTransientEntityAsync(object entity, string entityName, object requestedId, IEventSource source, IDictionary copyCache, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			IEntityPersister persister = source.GetEntityPersister(entityName, entity);

			object id = persister.HasIdentifierProperty ? persister.GetIdentifier(entity) : null;
			object copy = null;
			
			if (copyCache.Contains(entity))
			{
				copy = copyCache[entity];
				persister.SetIdentifier(copy, id);
			}
			else
			{
				copy = source.Instantiate(persister, id);
				((EventCache)copyCache).Add(entity, copy, true); // before cascade!
			}

			// cascade first, so that all unsaved objects get their
			// copy created before we actually copy
			//cascadeOnMerge(event, persister, entity, copyCache, Cascades.CASCADE_BEFORE_MERGE);
			await (base.CascadeBeforeSaveAsync(source, persister, entity, copyCache, cancellationToken)).ConfigureAwait(false);
			await (CopyValuesAsync(persister, entity, copy, source, copyCache, ForeignKeyDirection.ForeignKeyFromParent, cancellationToken)).ConfigureAwait(false);

			try
			{
				// try saving; check for non-nullable properties that are null or transient entities before saving
				await (this.SaveTransientEntityAsync(copy, entityName, requestedId, source, copyCache, cancellationToken)).ConfigureAwait(false);
			}
			catch (PropertyValueException ex)
			{
				string propertyName = ex.PropertyName;
				object propertyFromCopy = persister.GetPropertyValue(copy, propertyName);
				object propertyFromEntity = persister.GetPropertyValue(entity, propertyName);
				IType propertyType = persister.GetPropertyType(propertyName);
				EntityEntry copyEntry = source.PersistenceContext.GetEntry(copy);

				if (propertyFromCopy == null || !propertyType.IsEntityType)
				{
					log.Info("property '{0}.{1}' is null or not an entity; {1} =[{2}]", copyEntry.EntityName, propertyName, propertyFromCopy);
					throw;
				}

				if (!copyCache.Contains(propertyFromEntity))
				{
					log.Info("property '{0}.{1}' from original entity is not in copyCache; {1} =[{2}]", copyEntry.EntityName, propertyName, propertyFromEntity);
					throw;
				}
				
				if (((EventCache)copyCache).IsOperatedOn(propertyFromEntity))
				{
					log.Info(ex, "property '{0}.{1}' from original entity is in copyCache and is in the process of being merged; {1} =[{2}]", copyEntry.EntityName, propertyName, propertyFromEntity);
				}
				else
				{
					log.Info(ex, "property '{0}.{1}' from original entity is in copyCache and is not in the process of being merged; {1} =[{2}]", copyEntry.EntityName, propertyName, propertyFromEntity);
				}
				
				// continue...; we'll find out if it ends up not getting saved later
			}
			
			// cascade first, so that all unsaved objects get their
			// copy created before we actually copy
			await (base.CascadeAfterSaveAsync(source, persister, entity, copyCache, cancellationToken)).ConfigureAwait(false);
			await (CopyValuesAsync(persister, entity, copy, source, copyCache, ForeignKeyDirection.ForeignKeyToParent, cancellationToken)).ConfigureAwait(false);

			return copy;
		}
	
		private Task SaveTransientEntityAsync(object entity, string entityName, object requestedId, IEventSource source, IDictionary copyCache, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				// this bit is only *really* absolutely necessary for handling
				// requestedId, but is also good if we merge multiple object
				// graphs, since it helps ensure uniqueness
				if (requestedId == null)
				{
					return SaveWithGeneratedIdAsync(entity, entityName, copyCache, source, false, cancellationToken);
				}
				else
				{
					return SaveWithRequestedIdAsync(entity, requestedId, entityName, copyCache, source, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		protected virtual async Task EntityIsDetachedAsync(MergeEvent @event, IDictionary copyCache, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			log.Debug("merging detached instance");

			object entity = @event.Entity;
			IEventSource source = @event.Session;

			IEntityPersister persister = source.GetEntityPersister(@event.EntityName, entity);
			string entityName = persister.EntityName;

			object id = @event.RequestedId;
			if (id == null)
			{
				id = persister.GetIdentifier(entity);
			}
			else
			{
				// check that entity id = requestedId
				object entityId = persister.GetIdentifier(entity);
				if (!persister.IdentifierType.IsEqual(id, entityId, source.Factory))
				{
					throw new HibernateException("merge requested with id not matching id of passed entity");
				}
			}

			string previousFetchProfile = source.FetchProfile;
			source.FetchProfile = "merge";

			//we must clone embedded composite identifiers, or
			//we will get back the same instance that we pass in
			object clonedIdentifier = persister.IdentifierType.DeepCopy(id, source.Factory);
			object result = await (source.GetAsync(persister.EntityName, clonedIdentifier, cancellationToken)).ConfigureAwait(false);

			source.FetchProfile = previousFetchProfile;

			if (result == null)
			{
				//TODO: we should throw an exception if we really *know* for sure
				//      that this is a detached instance, rather than just assuming
				//throw new StaleObjectStateException(entityName, id);

				// we got here because we assumed that an instance
				// with an assigned id was detached, when it was
				// really persistent
				await (EntityIsTransientAsync(@event, copyCache, cancellationToken)).ConfigureAwait(false);
			}
			else
			{
				// NH different behavior : NH-1517
				if (InvokeUpdateLifecycle(entity, persister, source))
				{
					return;
				}

				((EventCache)copyCache).Add(entity, result, true); //before cascade!

				object target = source.PersistenceContext.Unproxy(result);
				if (target == entity)
				{
					throw new AssertionFailure("entity was not detached");
				}
				else if (!(await (source.GetEntityNameAsync(target, cancellationToken)).ConfigureAwait(false)).Equals(entityName))
				{
					throw new WrongClassException("class of the given object did not match class of persistent copy",
					                              @event.RequestedId, persister.EntityName);
				}
				else if (IsVersionChanged(entity, source, persister, target))
				{
					if (source.Factory.Statistics.IsStatisticsEnabled)
					{
						source.Factory.StatisticsImplementor.OptimisticFailure(entityName);
					}
					throw new StaleObjectStateException(persister.EntityName, id);
				}

				// cascade first, so that all unsaved objects get their
				// copy created before we actually copy
				await (CascadeOnMergeAsync(source, persister, entity, copyCache, cancellationToken)).ConfigureAwait(false);
				await (CopyValuesAsync(persister, entity, target, source, copyCache, cancellationToken)).ConfigureAwait(false);

				//copyValues works by reflection, so explicitly mark the entity instance dirty
				MarkInterceptorDirty(entity, target);

				@event.Result = result;
			}
		}

		protected virtual async Task CopyValuesAsync(IEntityPersister persister, object entity, object target, ISessionImplementor source, IDictionary copyCache, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] copiedValues =
				await (TypeHelper.ReplaceAsync(persister.GetPropertyValues(entity),
				                    persister.GetPropertyValues(target), persister.PropertyTypes, source, target,
				                    copyCache, cancellationToken)).ConfigureAwait(false);

			persister.SetPropertyValues(target, copiedValues);
		}

		protected virtual async Task CopyValuesAsync(IEntityPersister persister, object entity, object target, ISessionImplementor source, IDictionary copyCache, ForeignKeyDirection foreignKeyDirection, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] copiedValues;

			if (foreignKeyDirection.Equals( ForeignKeyDirection.ForeignKeyToParent))
			{
				// this is the second pass through on a merge op, so here we limit the
				// replacement to associations types (value types were already replaced
				// during the first pass)
				copiedValues =
					await (TypeHelper.ReplaceAssociationsAsync(persister.GetPropertyValues(entity),
					                                persister.GetPropertyValues(target), persister.PropertyTypes,
					                                source, target, copyCache, foreignKeyDirection, cancellationToken)).ConfigureAwait(false);
			}
			else
			{
				copiedValues =
					await (TypeHelper.ReplaceAsync(persister.GetPropertyValues(entity),
					                    persister.GetPropertyValues(target), persister.PropertyTypes, source, target,
					                    copyCache, foreignKeyDirection, cancellationToken)).ConfigureAwait(false);
			}

			persister.SetPropertyValues(target, copiedValues);
		}

		/// <summary>
		/// Perform any cascades needed as part of this copy event.
		/// </summary>
		/// <param name="source">The merge event being processed. </param>
		/// <param name="persister">The persister of the entity being copied. </param>
		/// <param name="entity">The entity being copied. </param>
		/// <param name="copyCache">A cache of already copied instance. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		protected virtual async Task CascadeOnMergeAsync(IEventSource source, IEntityPersister persister, object entity, IDictionary copyCache, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			source.PersistenceContext.IncrementCascadeLevel();
			try
			{
				await (new Cascade(CascadeAction, CascadePoint.BeforeMerge, source).CascadeOnAsync(persister, entity, copyCache, cancellationToken)).ConfigureAwait(false);
			}
			finally
			{
				source.PersistenceContext.DecrementCascadeLevel();
			}
		}
		
		/// <summary>
		/// Determine which merged entities in the copyCache are transient.
		/// </summary>
		/// <param name="event"></param>
		/// <param name="copyCache"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns></returns>
		/// <remarks>Should this method be on the EventCache class?</remarks>
		protected async Task<EventCache> GetTransientCopyCacheAsync(MergeEvent @event, EventCache copyCache, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			EventCache transientCopyCache = new EventCache();

			foreach(object entity in copyCache.Keys)
			{
				object entityCopy = copyCache[entity];
				
				if (entityCopy.IsProxy())
					entityCopy = await (((INHibernateProxy)entityCopy).HibernateLazyInitializer.GetImplementationAsync(cancellationToken)).ConfigureAwait(false);
				
				// NH-specific: Disregard entities that implement ILifecycle and manage their own state - they 
				// don't have an EntityEntry, and we can't determine if they are transient or not
				if (entityCopy is ILifecycle)
					continue;
			
				EntityEntry copyEntry = @event.Session.PersistenceContext.GetEntry(entityCopy);

				if (copyEntry == null)
				{
					// entity name will not be available for non-POJO entities
					// TODO: cache the entity name somewhere so that it is available to this exception
					log.Info(
						"transient instance could not be processed by merge: {0} [{1}]",
						@event.Session.GuessEntityName(entityCopy),
						entity);
					
					// merge did not cascade to this entity; it's in copyCache because a
					// different entity has a non-nullable reference to it;
					// this entity should not be put in transientCopyCache, because it was
					// not included in the merge;
					
					throw new TransientObjectException(
						"object is an unsaved transient instance - save the transient instance before merging: " + @event.Session.GuessEntityName(entityCopy));
				}
				else if (copyEntry.Status == Status.Saving)
				{
					transientCopyCache.Add(entity, entityCopy, copyCache.IsOperatedOn(entity));
				}
				else if (copyEntry.Status != Status.Loaded && copyEntry.Status != Status.ReadOnly)
				{
					throw new AssertionFailure(
						String.Format(
							"Merged entity does not have status set to MANAGED or READ_ONLY; {0} status = {1}",
							entityCopy,
							copyEntry.Status));
				}
			}
			return transientCopyCache;
		}
		
		/// <summary>
		/// Retry merging transient entities
		/// </summary>
		/// <param name="event"></param>
		/// <param name="transientCopyCache"></param>
		/// <param name="copyCache"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		protected async Task RetryMergeTransientEntitiesAsync(MergeEvent @event, IDictionary transientCopyCache, EventCache copyCache, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			// TODO: The order in which entities are saved may matter (e.g., a particular
			// transient entity may need to be saved before other transient entities can
			// be saved).
			// Keep retrying the batch of transient entities until either:
			// 1) there are no transient entities left in transientCopyCache
			// or 2) no transient entities were saved in the last batch.
			// For now, just run through the transient entities and retry the merge
			
			foreach(object entity in transientCopyCache.Keys)
			{
				object copy = transientCopyCache[entity];
				EntityEntry copyEntry = @event.Session.PersistenceContext.GetEntry(copy);
				
				if (entity == @event.Entity)
					await (MergeTransientEntityAsync(entity, copyEntry.EntityName, @event.RequestedId, @event.Session, copyCache, cancellationToken)).ConfigureAwait(false);
				else
					await (MergeTransientEntityAsync(entity, copyEntry.EntityName, copyEntry.Id, @event.Session, copyCache, cancellationToken)).ConfigureAwait(false);
			}
		}
		
		/// <summary> Cascade behavior is redefined by this subclass, disable superclass behavior</summary>
		protected override Task CascadeAfterSaveAsync(IEventSource source, IEntityPersister persister, object entity, object anything, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				CascadeAfterSave(source, persister, entity, anything);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		/// <summary> Cascade behavior is redefined by this subclass, disable superclass behavior</summary>
		protected override Task CascadeBeforeSaveAsync(IEventSource source, IEntityPersister persister, object entity, object anything, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				CascadeBeforeSave(source, persister, entity, anything);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}
	}
}
