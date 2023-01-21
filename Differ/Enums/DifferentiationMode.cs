using Differ.Attributes;

namespace Differ.Enums
{
    /// <summary>
    /// Specifies the different modes of differentiation.
    /// </summary>
    public enum DifferentiationMode
    {
        /// <summary>
        /// Differentiate all public members.
        /// <para>Members tagged with <see cref="NonDifferableAttribute"/> will still be excluded.</para>
        /// </summary>
        Full,
        /// <summary>
        /// Only differentiate members tagged with <see cref="DifferableAttribute"/>.
        /// </summary>
        Differable
    }
}
