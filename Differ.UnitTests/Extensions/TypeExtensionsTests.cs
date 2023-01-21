using Differ.Enums;
using Differ.Extensions;

namespace Differ.Unit.Extensions
{
    [TestFixture]
    public class TypeExtensionsTests
    {
        [Test]
        [TestCase(typeof(char))]
        [TestCase(typeof(int))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(string))]
        [TestCase(typeof(DifferenceType))]  // Enum
        [TestCase(typeof(DateTime))]
        public void IsConsideredPrimitive_ReturnsTrue_ForExpectedPrimitiveTypes(Type type)
        {
            Assert.That(type.IsConsideredPrimitive());
        }

        [Test]
        [TestCase(typeof(TypeExtensionsTests))]
        public void IsConsideredNonPrimitive_ReturnsTrue_ForExpectedNonPrimitiveTypes(Type type)
        {
            Assert.That(type.IsConsideredNonPrimitive());
        }
    }
}
