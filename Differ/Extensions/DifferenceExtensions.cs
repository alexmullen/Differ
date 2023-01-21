using System.Collections.Generic;
using System.Reflection;

namespace Differ.Extensions
{
    internal static class DifferenceExtensions
    {
        public static void SetAllMemberTo(this IEnumerable<Difference> differences, MemberInfo memberInfo)
        {
            foreach (Difference difference in differences)
                difference.Member = memberInfo;
        }
    }
}
