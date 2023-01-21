using System;

namespace Differ.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = false,
        Inherited = true
    )]
    public class NonDifferableAttribute : Attribute
    {
        public NonDifferableAttribute() { }
    }
}
