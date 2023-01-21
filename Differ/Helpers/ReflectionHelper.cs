using Differ.Attributes;
using Differ.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Differ.Extensions;

namespace Differ.Helpers
{
    public static class ReflectionHelper
    {
        public static Type GetCommonBaseType(params Type[] types)
        {
            var typesWithoutAnyNulls = types.Where(type => type != null);
            var currentPotentialCommonBaseType = typesWithoutAnyNulls.FirstOrDefault();
            var remainingTypes = typesWithoutAnyNulls.Skip(1).ToArray();

            while (!remainingTypes.All(type => currentPotentialCommonBaseType.IsAssignableFrom(type)))
                currentPotentialCommonBaseType = currentPotentialCommonBaseType.BaseType;

            return currentPotentialCommonBaseType;
        }

        public static Type GetCollectionType(Type collectionType)
        {
            if (collectionType.IsArray) return collectionType.GetElementType();
            if (collectionType.IsGenericType) return collectionType.GetGenericArguments().FirstOrDefault();
            return collectionType;
        }

        public static IEnumerable<MemberInfo> GetDifferableMembers(Type type, DifferentiationMode mode)
        {
            // Get all public readable instance members that are fields and properties that are not tagged as being [NonDifferable]. 
            var members = GetPotentiallyDiffereableMembers(type, true);

            // If we are in differable only mode, bring through only members tagged with [Differable]
            if (mode == DifferentiationMode.Differable)
                members = members
                    .Where(member => MemberIsTaggedAsDifferable(member));

            // Get the most specific member in the cases where there are multiple members with the name in the inheritance hierarchy
            members = members
                .GroupBy(member => member.Name)
                .Select(memberGroup => memberGroup
                    .Aggregate((currentMostSpecificMember, otherMember) =>
                        currentMostSpecificMember.DeclaringType.IsSubclassOf(otherMember.DeclaringType) ? currentMostSpecificMember : otherMember));

            return members;
        }

        public static IEnumerable<MemberInfo> GetPotentiallyDiffereableMembers(Type type, bool publicAndExplicitOnly)
        {
            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(member =>
                    // Only properties and fields
                    (member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field) &&
                    // Only readable properties
                    ((member as PropertyInfo)?.CanRead ?? true) &&
                    // No [NonDifferable] marked members
                    (!MemberIsTaggedAsNonDifferable(member)));

            if (publicAndExplicitOnly)
            {
                // Filter out non-public properties (except ones marked [Differable])
                members = members.Where(member =>
                {
                    var propertyMember = member as PropertyInfo;
                    return propertyMember == null || (propertyMember.GetMethod?.IsPublic ?? false) ||
                        MemberIsTaggedAsDifferable(member);
                });

                // Filter out non-public fields (except ones marked [Differable])
                members = members?.Where(member =>
                {
                    // Default to true if cast is null as it is not a Field member
                    return ((member as FieldInfo)?.IsPublic ?? true) ||
                        MemberIsTaggedAsDifferable(member);
                });
            }

            return members;
        }

        public static IEnumerable<MemberInfo> GetKeyMembers(Type type)
        {
            return type == null
                ? Array.Empty<MemberInfo>()
                : GetPotentiallyDiffereableMembers(type, false)
                    .Where(member => MemberIsTaggedAsKey(member));
        }

        public static object GetObjectKey(object obj, IEnumerable<MemberInfo> keyMembers = null)
        {
            string key = null;
            if (obj != null)
            {
                foreach (var keyMember in keyMembers ?? GetKeyMembers(obj.GetType()))
                {
                    var keyMemberValue = keyMember.GetValue(obj);
                    // Abort on first null and default to returning the object itself as we consider null
                    // being an unknown key.
                    if (keyMemberValue == null)
                        return obj;
                    // Append string value so as to build up a unique key string
                    key += keyMemberValue.ToString();
                }
            }
            return key ?? obj;
        }

        public static bool MemberIsTaggedAsKey(MemberInfo member)
        {
            return member.IsDefined(typeof(KeyAttribute), true);
        }

        public static bool MemberIsTaggedAsDifferable(MemberInfo member)
        {
            return member.IsDefined(typeof(DifferableAttribute), true);
        }

        public static bool MemberIsTaggedAsNonDifferable(MemberInfo member)
        {
            return member.IsDefined(typeof(NonDifferableAttribute), true);
        }
    }
}
