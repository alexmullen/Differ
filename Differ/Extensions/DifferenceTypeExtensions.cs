using Differ.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Differ.Extensions
{
    public static class DifferenceTypeExtensions
    {
        public static DifferenceType FindAggregatedDifferenceType(this IEnumerable<DifferenceType> differenceTypesEnumerable)
        {
            var aggregatedDifferenceType = DifferenceType.Update;
            if (differenceTypesEnumerable.Any())
            {
                var firstDifferenceType = differenceTypesEnumerable.First();
                aggregatedDifferenceType = differenceTypesEnumerable
                    .Skip(1)
                    .All(differenceType => differenceType == firstDifferenceType) ? firstDifferenceType : DifferenceType.Update;
            }
            return aggregatedDifferenceType;
        }
    }
}
