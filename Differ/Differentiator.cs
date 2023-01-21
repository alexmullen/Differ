using Differ.Enums;
using Differ.Extensions;
using Differ.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Differ
{
    public static class Differentiator
    {
        /// <summary>
        /// Find the differences between the given two objects
        /// </summary>
        /// <typeparam name="T">The type of both objects</typeparam>
        /// <param name="from">The object considered the original or previous state</param>
        /// <param name="to">The object considered the current or new state</param>
        /// <param name="mode">The mode to run in for the members to compare</param>
        /// <returns>A collection of differences. If there are no differences, an empty collection.</returns>
        public static ICollection<Difference> Differentiate<T>(T from, T to, DifferentiationMode mode = DifferentiationMode.Full)
        {
            var differences = DifferentiateDive(from, to, mode);

            DiscoverMoves(differences, mode);

            return differences;
        }

        private static void DiscoverMoves(ICollection<Difference> differences, DifferentiationMode mode)
        {
            var addAndRemovePaths = FindAddRemoveDifferencePaths(differences);

            var addPathDifferences = addAndRemovePaths
                .Where(path => path.Last().Type == DifferenceType.Add)
                .Select(path => new
                {
                    Difference = path.Last(),
                    Path = path
                });

            var removePathDifferences = addAndRemovePaths
                .Where(path => path.Last().Type == DifferenceType.Remove)
                .Select(path => new
                {
                    Difference = path.Last(),
                    Path = path
                });

            // Pair all Add and Remove differences where they are the same object
            var addedAndRemovedPairs = addPathDifferences
                .Join(
                    removePathDifferences,
                    (added) => ReflectionHelper.GetObjectKey(added.Difference.Item),
                    (removed) => ReflectionHelper.GetObjectKey(removed.Difference.Item),
                    (inner, outer) => new
                    {
                        Added = inner,
                        Removed = outer
                    })
                // Filter out any with same key but different instance type (Should be unlikely but we will handle)
                .Where(arPair => arPair.Added.Difference.Item.GetType().IsAssignableFrom(arPair.Removed.Difference.Item.GetType()));

            // Update all Add and Removes that were actual moves to be Moves and set their most
            // logical containing object
            foreach (var addedRemovedPair in addedAndRemovedPairs)
            {
                var addedDifference = addedRemovedPair.Added.Difference;
                var removedDifference = addedRemovedPair.Removed.Difference;

                addedDifference.Type = DifferenceType.Move;
                //addedDifference.From = FindContainingObjectForMove(addedRemovedPair.Removed.Path);
                addedDifference.From = addedRemovedPair.Removed.Path;   // The difference path the item was moved from
                addedDifference.To = null;
                addedDifference.Differences = DifferentiateDive(removedDifference.Item, addedDifference.Item, mode);

                removedDifference.Type = DifferenceType.Move;
                removedDifference.From = null;
                removedDifference.To = addedRemovedPair.Added.Path;     // The difference path the item was moved to
                //removedDifference.To = FindContainingObjectForMove(addedRemovedPair.Added.Path);
            }
        }

        private static ICollection<List<Difference>> FindAddRemoveDifferencePaths(ICollection<Difference> diffs)
        {
            ICollection<List<Difference>> FindDive(ICollection<Difference> differences, ICollection<Difference> path)
            {
                var addAndRemovePaths = new List<List<Difference>>();
                foreach (var difference in differences)
                {
                    var newPath = new List<Difference>(path) { difference };
                    if (difference.Type == DifferenceType.Add || difference.Type == DifferenceType.Remove)
                        addAndRemovePaths.Add(newPath);
                    if (difference.Differences != null)
                        addAndRemovePaths.AddRange(FindDive(difference.Differences, newPath));
                }
                return addAndRemovePaths;
            }
            return FindDive(diffs, new List<Difference>());
        }

        private static ICollection<Difference> DifferentiateDive<T>(T from, T to, DifferentiationMode mode, int level = 0)
        {
            if (Equals(from, to))
                return Array.Empty<Difference>();

            var commonType = ReflectionHelper.GetCommonBaseType(to?.GetType(), from?.GetType());

            // TODO: Make string and date a special parser object (akin to JsonConverter)
            if (commonType.IsConsideredPrimitive())
                return new List<Difference> { Difference.CreateUpdateDifference(from, to) };
            else if (typeof(ICollection).IsAssignableFrom(commonType))
                return DifferentiateCollectionDive(from as ICollection, to as ICollection, commonType, mode, level + 1);
            else
                return DifferentiateObjectDive(from, to, commonType, mode, level + 1);
        }

        private static ICollection<Difference> DifferentiateCollectionDive(ICollection from, ICollection to, Type commonType, DifferentiationMode mode, int level = 0)
        {
            var differences = new List<Difference>();

            // Array/List/Collection. If one is null, compare with empty collection
            var fromCollection = from ?? Array.Empty<object>();
            var toCollection = to ?? Array.Empty<object>();

            // Still generate an update difference between an empty collection and a null collection
            if ((from == null || to == null) && fromCollection.Count == 0 && toCollection.Count == 0)
            {
                differences.Add(Difference.CreateUpdateDifference(from, to));
            }
            else
            {
                // Get the key members of the items in the collection so we know what was added, removed and updated.
                var collectionItemKeyMembers = ReflectionHelper.GetKeyMembers(ReflectionHelper.GetCollectionType(commonType))
                    .ToArray();
                
                // Find all added, removed and common elements.
                // TODO: Avoid the cast and toarray if possible
                TraverseCollection(fromCollection.Cast<object>().ToArray(), toCollection.Cast<object>().ToArray(),
                    // Item identity selector
                    (item) => ReflectionHelper.GetObjectKey(item, collectionItemKeyMembers),
                    (addedItem) =>
                    {
                        ICollection<Difference> innerAssignments = null;
                        if (addedItem != null && addedItem.GetType().IsConsideredNonPrimitive())
                            innerAssignments = DifferentiateAssignmentsDive(addedItem, mode, level + 1);

                        differences.Add(Difference.CreateAddDifference(from, to, addedItem, innerAssignments));
                    },
                    (removedItem) =>
                    {
                        differences.Add(Difference.CreateRemoveDifference(from, to, removedItem));
                    },
                    (commonFromItem, commonToItem, commonIdentifier) =>
                    {
                        // Go through each common object pair and recurse diff
                        var innerDifferences = DifferentiateDive(commonFromItem, commonToItem, mode, level + 1);
                        if (innerDifferences.Any())
                            differences.Add(Difference.CreateUpdateDifference(from, to, commonToItem, innerDifferences));
                    });
            }

            return differences;
        }

        private static ICollection<Difference> DifferentiateObjectDive(object from, object to, Type commonType, DifferentiationMode mode, int level = 0)
        {
            var differences = new List<Difference>();

            if (from == null || to == null)
            {
                // Only get the assignment differences of the non-null object
                differences.Add(Difference.CreateUpdateDifference(from, to, null, 
                    DifferentiateAssignmentsDive(from ?? to, mode, level + 1)));
            }
            else
            {
                foreach (var member in ReflectionHelper.GetDifferableMembers(commonType, mode))
                {
                    var fromMemberValue = member.GetValue(from);
                    var toMemberValue = member.GetValue(to);
                    var innerDifferences = DifferentiateDive(fromMemberValue, toMemberValue, mode, level + 1);
                    if (innerDifferences.Any())
                    {
                        // Don't bother nesting if the common type is primitive
                        if (ReflectionHelper.GetCommonBaseType(fromMemberValue?.GetType(), toMemberValue?.GetType()).IsConsideredPrimitive())
                        {
                            innerDifferences.SetAllMemberTo(member);
                            differences.AddRange(innerDifferences);
                        }
                        else
                        {
                            differences.Add(Difference.CreateUpdateDifference(from, to, null, innerDifferences, member));
                        }
                    }
                }
            }

            return differences;
        }

        private static ICollection<Difference> DifferentiateAssignmentsDive<T>(T item, DifferentiationMode mode, int level = 0)
        {
            if (item == null)
                return Array.Empty<Difference>();

            var assigmentDifferences = new List<Difference>();

            if (item.GetType().IsConsideredPrimitive()) 
            {
                assigmentDifferences.Add(Difference.CreateAssignDifference(item));
            }
            else if (typeof(ICollection).IsAssignableFrom(item.GetType()))
            {
                var itemCollection = item as ICollection ?? Array.Empty<object>();

                if (ReflectionHelper.GetCollectionType(itemCollection.GetType()).IsConsideredPrimitive())
                {
                    foreach (var primitiveCollectionItem in itemCollection)
                        assigmentDifferences.Add(Difference.CreateAddDifference(null, itemCollection, primitiveCollectionItem));
                }
                else
                {
                    // Get the assignments for all items in the collection and their child items and so on so on...
                    foreach (var objCollectionItem in itemCollection)
                        assigmentDifferences.Add(Difference.CreateAddDifference(null, itemCollection, objCollectionItem, 
                            DifferentiateAssignmentsDive(objCollectionItem, mode, level + 1)));
                }
            }
            else
            {
                foreach (var member in ReflectionHelper.GetDifferableMembers(item.GetType(), mode))
                {
                    var memberValue = member.GetValue(item);
                    if (memberValue == null || memberValue.GetType().IsConsideredPrimitive())
                        assigmentDifferences.Add(Difference.CreateAssignDifference(memberValue, null, member));
                    else
                        assigmentDifferences.Add(Difference.CreateAssignDifference(memberValue, 
                            DifferentiateAssignmentsDive(memberValue, mode, level + 1), member));
                }
            }

            return assigmentDifferences;
        }

        /// <summary>
        /// Finds all added, removed and common items between two collections based on an identifying property.
        /// <para>The order of the items in the collection is not considered.</para>
        /// </summary>
        /// <typeparam name="T">The type of items in the collection</typeparam>
        /// <typeparam name="I">The type to use for the identifier</typeparam>
        /// <param name="from">The previous state of the collection</param>
        /// <param name="to">The next state of the collection</param>
        /// <param name="identitySelector">The function used to select the identifier from an item</param>
        /// <param name="addedAction">The action to invoke for each item in '<paramref name="to"/>' but not in '<paramref name="from"/>'</param>
        /// <param name="removedAction">The action to invoke for each item in '<paramref name="from"/>' but not in '<paramref name="to"/>'</param>
        /// <param name="commonAction">The action to invoke for each item in '<paramref name="from"/>' that is also in '<paramref name="to"/>'</param>
        public static void TraverseCollection<T, I>(ICollection<T> from, ICollection<T> to, Func<T, I> identitySelector, Action<T> addedAction, Action<T> removedAction, Action<T, T, I> commonAction = default)
        {
            // Create a 'identifier -> from' item lookup
            var fromIdentifierGroupLookup = from
                .ToLookup(item => identitySelector(item));

            // Create a 'identifier -> to' item lookup
            var toIdentifiersGroupLookup = to
                .ToLookup(item => identitySelector(item));

            // Get all items that were added
            var addedItems = toIdentifiersGroupLookup
                .GroupJoin(
                    fromIdentifierGroupLookup,
                    (toKvp) => toKvp.Key,
                    (fromKvp) => fromKvp.Key,
                    (toIdentifiersGroup, fromIdentifiersGroups) => new
                    {
                        To = toIdentifiersGroup,
                        From = fromIdentifiersGroups.FirstOrDefault(),
                    })
                .Where(joinedGroup => joinedGroup.To.Count() > (joinedGroup.From?.Count() ?? 0))
                .SelectMany(joinedGroupWithAdditions => {
                    return joinedGroupWithAdditions.From == null
                        ? joinedGroupWithAdditions.To
                        : joinedGroupWithAdditions.To.Take(joinedGroupWithAdditions.To.Count() - joinedGroupWithAdditions.From.Count());
                });

            // Get all items that were removed
            var removedItems = fromIdentifierGroupLookup
                .GroupJoin(
                    toIdentifiersGroupLookup,
                    (fromKvp) => fromKvp.Key,
                    (toKvp) => toKvp.Key,
                    (fromIdentifiersGroup, toIdentifiersGroups) => new
                    {
                        From = fromIdentifiersGroup,
                        To = toIdentifiersGroups.FirstOrDefault(),
                    })
                .Where(joinedGroup => joinedGroup.From?.Count() > (joinedGroup.To?.Count() ?? 0))
                .SelectMany(joinedGroupWithRemovals =>
                {
                    return joinedGroupWithRemovals.To == null
                        ? joinedGroupWithRemovals.From
                        : joinedGroupWithRemovals.From.Take(joinedGroupWithRemovals.From.Count() - joinedGroupWithRemovals.To.Count());
                });

            foreach (var addedItem in addedItems)
                addedAction(addedItem);

            foreach (var removeItem in removedItems)
                removedAction(removeItem);

            if (commonAction != default)
            {
                // Get all common items
                var commonItemGroupIdentifiers = fromIdentifierGroupLookup
                    .Select(group => group.Key)
                    .Union(toIdentifiersGroupLookup
                        .Select(group => group.Key));

                foreach (var commonItemGroupIdentifier in commonItemGroupIdentifiers)
                {
                    var fromCommonItemGroup = fromIdentifierGroupLookup[commonItemGroupIdentifier];
                    var toCommonItemGroup = toIdentifiersGroupLookup[commonItemGroupIdentifier];

                    // Loop through an equal number of the common items and stop when one collection has no more
                    var commonItemPairs = fromCommonItemGroup.Zip(toCommonItemGroup, (fromItem, toItem) => new
                    {
                        FromItem = fromItem,
                        ToItem = toItem
                    });

                    foreach (var commonItemPair in commonItemPairs)
                        commonAction(commonItemPair.FromItem, commonItemPair.ToItem, commonItemGroupIdentifier);
                }
            }
        }
    }
}
