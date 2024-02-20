using Hearts;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartsTest
{
    [TestClass]
    public class RoundTests
    {
        [TestMethod]
        public void StartTrick_AddsNewTrickToTricks()
        {
            // Arrange
            var player = new Player("Test");
            var trickFactory = new Mock<ITrickFactory>();
            trickFactory
               .Setup(tf => tf.CreateTrick(It.IsAny<List<Player>>(), It.IsAny<bool>()))
               .Returns(new Trick([player], false));
            player.DealHand(Enumerable.Range(0, 13).Select(i => new Card(Suit.Clubs, (Rank)i)));
            var round = new Round([player], trickFactory.Object);

            // Act
            round.StartTrick();

            // Assert
            Assert.AreEqual(1, round.Tricks.Count);
        }

        [TestMethod]
        public void StartTrick_SetsCurrentTrickCorrectly()
        {
            // Arrange
            var player = new Player("Test");
            var trickFactory = new Mock<ITrickFactory>();
            trickFactory
               .Setup(tf => tf.CreateTrick(It.IsAny<List<Player>>(), It.IsAny<bool>()))
               .Returns(new Trick([player], false));
            player.DealHand(Enumerable.Range(0, 13).Select(i => new Card(Suit.Clubs, (Rank)i)));
            var round = new Round([player], trickFactory.Object);

            // Act
            round.StartTrick();

            // Assert
            Assert.AreEqual(round.Tricks.Last(), round.CurrentTrick);
        }

        [TestMethod]
        public void StartTrick_WiresUpEventsCorrectly()
        {
            // Arrange
            var trickFactory = new Mock<ITrickFactory>();
            var trick = new Mock<ITrick>();
            trickFactory.Setup(tf => tf.CreateTrick(It.IsAny<List<Player>>(), It.IsAny<bool>())).Returns(trick.Object);
            var player = new Player("Test");
            player.DealHand(Enumerable.Range(0, 13).Select(i => new Card(Suit.Clubs, (Rank)i)));
            var round = new Round([player], trickFactory.Object);

            // Act
            round.StartTrick();

            // Assert
            trick.VerifyAdd(t => t.ActionRequested += It.IsAny<ActionRequestedEventHandler>(), Times.Once);
            trick.VerifyAdd(t => t.TrickCompleted += It.IsAny<EventHandler>(), Times.Once);
        }

        [TestMethod]
        public void StartTrick_CallsStartTrickOnTrick()
        {
            // Arrange
            var trickFactory = new Mock<ITrickFactory>();
            var trick = new Mock<ITrick>();
            trickFactory.Setup(tf => tf.CreateTrick(It.IsAny<List<Player>>(), It.IsAny<bool>())).Returns(trick.Object);
            var player = new Player("Test");
            player.DealHand(Enumerable.Range(0, 13).Select(i => new Card(Suit.Clubs, (Rank)i)));
            var round = new Round([player], trickFactory.Object);

            // Act
            round.StartTrick();

            // Assert
            trick.Verify(t => t.StartTrick(), Times.Once);
        }
    }
}