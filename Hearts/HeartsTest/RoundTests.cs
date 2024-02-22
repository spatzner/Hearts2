using Hearts;
using Moq;

namespace HeartsTest;

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

    //[TestMethod]
    //public void ScoreRound_GivesPointsToNonWinners_WhenPlayerShotTheMoon()
    //{
    //    // Arrange
    //    var trickFactory = new Mock<ITrickFactory>();
    //    var players = new List<Player>
    //    {
    //        new("Bob"),
    //        new("Steve"),
    //        new("Jane"),
    //        new("Beth")
    //    };
    //    var round = new Round(players, trickFactory.Object);

    //    // Simulate a round where one player won all the tricks
    //    var playerWhoShotTheMoon = players[0];
    //    for (int i = 0; i < 13; i++)
    //    {
    //        var trick = new Trick(players, false) { Winner = playerWhoShotTheMoon };
    //        round.Tricks.Add(trick);
    //    }

    //    // Act
    //    round.OnTrickCompleted();

    //    // Assert
    //    foreach (var player in players.Where(p => p != playerWhoShotTheMoon))
    //    {
    //        Assert.AreEqual(26, player.Score);
    //    }
    //    Assert.AreEqual(0, playerWhoShotTheMoon.Score);
    //}

    //[TestMethod]
    //public void ScoreRound_GivesPointsToWinners_WhenNoPlayerShotTheMoon()
    //{
    //    // Arrange
    //    var trickFactory = new Mock<ITrickFactory>();
    //    var players = new List<Player>
    //    {
    //        new("Bob"),
    //        new("Steve"),
    //        new("Jane"),
    //        new("Beth")
    //    };
    //    var round = new Round(players, trickFactory.Object);

    //    // Simulate a round where each player won an equal number of tricks
    //    for (int i = 0; i < 13; i++)
    //    {
    //        var trick = new Trick(players, false) { Winner = players[i % players.Count] };
    //        round.Tricks.Add(trick);
    //    }

    //    // Act
    //    round.ScoreRound();

    //    // Assert
    //    foreach (var player in players)
    //    {
    //        Assert.AreEqual(13, player.Score);
    //    }
    //}
}