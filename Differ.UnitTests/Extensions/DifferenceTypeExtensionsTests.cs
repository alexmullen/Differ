using Differ.Enums;
using Differ.Extensions;

namespace Differ.Unit.Extensions
{
    [TestFixture]
    public class DifferenceTypeExtensionsTests
    {
        [Test]
        public void FindAggregatedDifferenceType_ReturnsUpdate_WhenEmptyList()
        {
            var differenceList = new List<DifferenceType>();
            Assert.That(() => differenceList.FindAggregatedDifferenceType() == DifferenceType.Update);
        }

        [Test]
        public void FindAggregatedDifferenceType_ReturnsAdd_WhenListOfAdds()
        {
            var differenceList = new List<DifferenceType>
            {
                DifferenceType.Add,
                DifferenceType.Add,
                DifferenceType.Add,
                DifferenceType.Add,
            };
            Assert.That(() => differenceList.FindAggregatedDifferenceType() == DifferenceType.Add);
        }

        [Test]
        public void FindAggregatedDifferenceType_ReturnsRemove_WhenListOfRemoves()
        {
            var differenceList = new List<DifferenceType>
            {
                DifferenceType.Remove,
                DifferenceType.Remove,
                DifferenceType.Remove,
                DifferenceType.Remove,
            };
            Assert.That(() => differenceList.FindAggregatedDifferenceType() == DifferenceType.Remove);
        }

        [Test]
        public void FindAggregatedDifferenceType_ReturnsAssign_WhenListOfUpdates()
        {
            var differenceList = new List<DifferenceType>
            {
                DifferenceType.Update,
                DifferenceType.Update,
                DifferenceType.Update,
                DifferenceType.Update,
            };
            Assert.That(() => differenceList.FindAggregatedDifferenceType() == DifferenceType.Update);
        }

        [Test]
        public void FindAggregatedDifferenceType_ReturnsAssign_WhenListOfAssigns()
        {
            var differenceList = new List<DifferenceType>
            {
                DifferenceType.Assign,
                DifferenceType.Assign,
                DifferenceType.Assign,
                DifferenceType.Assign,
            };
            Assert.That(() => differenceList.FindAggregatedDifferenceType() == DifferenceType.Assign);
        }

        [Test]
        public void FindAggregatedDifferenceType_ReturnsMove_WhenListOfMoves()
        {
            var differenceList = new List<DifferenceType>
            {
                DifferenceType.Move,
                DifferenceType.Move,
                DifferenceType.Move,
                DifferenceType.Move,
            };
            Assert.That(() => differenceList.FindAggregatedDifferenceType() == DifferenceType.Move);
        }

        [Test]
        public void FindAggregatedDifferenceType_ReturnsUpdate_WhenMixedList()
        {
            var differenceList = new List<DifferenceType>
            {
                DifferenceType.Add,
                DifferenceType.Remove,
                DifferenceType.Update,
                DifferenceType.Assign,
                DifferenceType.Move,
            };
            Assert.That(() => differenceList.FindAggregatedDifferenceType() == DifferenceType.Update);
        }
    }
}
