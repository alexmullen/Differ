using Differ.Enums;
using System.Collections.Generic;
using System.Reflection;

namespace Differ
{
    public class Difference
    {
        public MemberInfo Member { get; set; }
        public DifferenceType Type { get; set; }
        public object From { get; set; }
        public object To { get; set; }
        public object Item { get; set; }
        public ICollection<Difference> Differences { get; set; }

        internal static Difference CreateAddDifference(object from, object to, object addedItem, ICollection<Difference> innerAssignments = null)
        {
            return new Difference
            {
                Type = DifferenceType.Add,
                From = from,
                To = to,
                Differences = innerAssignments,
                Item = addedItem,
            };
        }
        internal static Difference CreateRemoveDifference(object from, object to, object removedItem)
        {
            return new Difference
            {
                Type = DifferenceType.Remove,
                From = from,
                To = to,
                Item = removedItem,
            };
        }
        internal static Difference CreateUpdateDifference(object from, object to, object item = null, ICollection<Difference> innerDifferences = null, MemberInfo member = null)
        {
            return new Difference
            {
                Type = DifferenceType.Update,
                Differences = innerDifferences,
                From = from,
                To = to,
                Item = item,
                Member = member
            };
        }
        internal static Difference CreateAssignDifference(object to, ICollection<Difference> innerAssignments = null, MemberInfo member = null)
        {
            return new Difference
            {
                Type = DifferenceType.Assign,
                Differences = innerAssignments,
                To = to,
                Member = member
            };
        }
    }
}
