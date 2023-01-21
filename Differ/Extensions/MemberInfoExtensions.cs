using System;
using System.Reflection;

namespace Differ.Extensions
{
    public static class MemberInfoExtensions
    {
        public static object GetValue(this MemberInfo memberInfo, object forObject)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.GetValue(forObject);
                case PropertyInfo propertyInfo:
                    // Ignore indexed parameters.
                    return propertyInfo.GetIndexParameters().Length > 0 ? null : propertyInfo.GetValue(forObject);
                default:
                    throw new NotSupportedException("Only fields and properties supported.");
            }
        }
    }
}
