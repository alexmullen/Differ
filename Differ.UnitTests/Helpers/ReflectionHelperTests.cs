using Differ.Attributes;
using Differ.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Differ.UnitTests.Helpers
{
    public class ReflectionHelperTests
    {
        public class ParentClassMock
        {
            [Key]
            public int ParentKey { get; set; }
            [Differable]
            public string ParentName { get; set; }
            public string ParentMetadata { get; set; }

            public ParentClassMock()
            {

            }
        }

        public class ChildClassMock : ParentClassMock
        {
            [Differable]
            public string ChildName { get; set; }
            [NonDifferable]
            public string ChildPassword { get; set; }
            private int PrivateHashcodeField;

            public ChildClassMock()
            {

            }

            public void SomeFunction()
            {

            }
        }

        [Test]
        public void GetCommonBaseType_ReturnsExpectedBaseType()
        {
            Assert.That(ReflectionHelper.GetCommonBaseType(typeof(ChildClassMock), typeof(ParentClassMock)), Is.SameAs(typeof(ParentClassMock)));
        }

        [Test]
        [TestCase(typeof(int[]), typeof(int))]
        [TestCase(typeof(string[]), typeof(string))]
        [TestCase(typeof(List<int>), typeof(int))]
        [TestCase(typeof(List<string>), typeof(string))]
        public void GetCollectionType_ReturnsExpectedType(Type collectionType, Type expectedCollectionType)
        {
            Assert.That(ReflectionHelper.GetCollectionType(collectionType), Is.SameAs(expectedCollectionType));
        }

        [Test]
        public void GetDifferableMembers_ReturnsExpectedMembers()
        {
            var obj = typeof(ChildClassMock);
            var differableMemberNames = ReflectionHelper.GetDifferableMembers(obj, Enums.DifferentiationMode.Differable)
                .Select(member => member.Name);
            var expectedDifferableNames = new List<string>()
            {
                "ParentName",
                "ChildName"
            };
            Assert.That(differableMemberNames, Is.EquivalentTo(expectedDifferableNames));
        }

        [Test]
        public void GetPotentiallyDiffereableMembers_ReturnsExpectedMembers()
        {
            var obj = typeof(ChildClassMock);
            var potentiallyDifferableMemberNames = ReflectionHelper.GetPotentiallyDiffereableMembers(obj, true)
                .Select(member => member.Name);
            var expectedPotentiallyDifferableNames = new List<string>()
            {
                "ParentKey",
                "ParentName",
                "ParentMetadata",
                "ChildName"
            };
            Assert.That(potentiallyDifferableMemberNames, Is.EquivalentTo(expectedPotentiallyDifferableNames));
        }

        [Test]
        public void GetKeyMembers_ReturnsExpectedMembers()
        {
            var obj = typeof(ChildClassMock);
            var keyMemberNames = ReflectionHelper.GetKeyMembers(obj)
                .Select(member => member.Name);
            var expectedKeyMemberNames = new List<string>()
            {
                "ParentKey"
            };
            Assert.That(keyMemberNames, Is.EquivalentTo(expectedKeyMemberNames));
        }

        [Test]
        public void GetObjectKey_ReturnsExpectedKey()
        {
            var obj = new ChildClassMock
            {
                ParentKey = 42
            };
            var sutKeyMembers = ReflectionHelper.GetKeyMembers(obj.GetType());
            var objectKey = ReflectionHelper.GetObjectKey(obj, sutKeyMembers);
            Assert.That(objectKey, Is.EqualTo("42"));
        }

        [Test]
        public void MemberIsTaggedAsKey_ReturnsExpectedResult()
        {
            Assert.That(ReflectionHelper.MemberIsTaggedAsKey(typeof(ChildClassMock).GetMember("ParentKey").First()), Is.True);
        }

        [Test]
        public void MemberIsTaggedAsDifferable_ReturnsExpectedResult()
        {
            Assert.That(ReflectionHelper.MemberIsTaggedAsDifferable(typeof(ChildClassMock).GetMember("ParentName").First()), Is.True);
        }

        [Test]
        public void MemberIsTaggedAsNonDifferable_ReturnsExpectedResult()
        {
            Assert.That(ReflectionHelper.MemberIsTaggedAsNonDifferable(typeof(ChildClassMock).GetMember("ChildPassword").First()), Is.True);
        }
    }
}
