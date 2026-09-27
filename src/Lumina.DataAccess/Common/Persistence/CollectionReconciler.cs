#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Lumina.DataAccess.Common.Persistence;

/// <summary>
/// Reconciles the children of a tracked entity against a desired set, preserving the identity of the children that already exist.
/// </summary>
internal static class CollectionReconciler
{
    /// <summary>
    /// Applies the desired <paramref name="incomingItems"/> onto the tracked <paramref name="existingItems"/>, by matching both sides with the provided key selectors.
    /// </summary>
    /// <remarks>
    /// Existing items that are absent from the desired set are removed, desired items that are absent are added, and matched items are left untouched, unless
    /// <paramref name="shouldReplace"/> reports that their non-key values changed, in which case only that item is replaced. Because the tracked instances are
    /// never rebuilt as a whole, the rows that did not change keep their identity and their audit columns.
    /// </remarks>
    /// <typeparam name="TExisting">The type of the tracked items.</typeparam>
    /// <typeparam name="TIncoming">The type of the desired items.</typeparam>
    /// <typeparam name="TKey">The type of the key used to match the two sides.</typeparam>
    /// <param name="existingItems">The tracked collection that is reconciled in place.</param>
    /// <param name="incomingItems">The desired items.</param>
    /// <param name="existingKeySelector">Selects the matching key of a tracked item.</param>
    /// <param name="incomingKeySelector">Selects the matching key of a desired item.</param>
    /// <param name="shouldReplace">Determines whether a matched tracked item must be replaced, because a non-key value changed.</param>
    /// <param name="createNew">Creates the entity used for a desired item that is not matched by any tracked item.</param>
    public static void Reconcile<TExisting, TIncoming, TKey>(
        ICollection<TExisting> existingItems,
        IEnumerable<TIncoming> incomingItems,
        Func<TExisting, TKey> existingKeySelector,
        Func<TIncoming, TKey> incomingKeySelector,
        Func<TExisting, TIncoming, bool> shouldReplace,
        Func<TIncoming, TExisting> createNew)
        where TExisting : class
        where TKey : notnull
    {
        // Index the tracked children by their key, so that each desired child can be matched in constant time; the first item wins when duplicate keys exist.
        Dictionary<TKey, TExisting> existingByKey = [];
        foreach (TExisting existingItem in existingItems)
            existingByKey.TryAdd(existingKeySelector(existingItem), existingItem);

        // Materialize the desired children once, so the source is not enumerated repeatedly, and collect their keys for the removal pass.
        List<TIncoming> incomingList = [.. incomingItems];
        HashSet<TKey> incomingKeys = [.. incomingList.Select(incomingKeySelector)];

        // Remove the tracked children that are absent from the desired set, so that deletions are applied.
        foreach (TExisting existingItem in existingItems.ToList())
            if (!incomingKeys.Contains(existingKeySelector(existingItem)))
                existingItems.Remove(existingItem);

        // Reconcile each desired child, replacing a matched one only when its non-key values changed, so unchanged rows keep their identity and audit columns.
        foreach (TIncoming incomingItem in incomingList)
        {
            TKey incomingKey = incomingKeySelector(incomingItem);
            if (existingByKey.TryGetValue(incomingKey, out TExisting? matchedItem) && matchedItem is not null)
            {
                if (shouldReplace(matchedItem, incomingItem))
                {
                    TExisting replacementItem = createNew(incomingItem);
                    existingItems.Remove(matchedItem);
                    existingItems.Add(replacementItem);
                    existingByKey[incomingKey] = replacementItem;
                }
            }
            else
            {
                // The desired child has no tracked counterpart, so it is added as a new child and indexed, so that a later desired child
                // that reuses the same key is matched against it instead of adding a second row that would break the unique index.
                TExisting addedItem = createNew(incomingItem);
                existingItems.Add(addedItem);
                existingByKey[incomingKey] = addedItem;
            }
        }
    }
}
