using Hearts;

namespace HeartsTest;

[TestClass]
public class TrickTests
{
    [TestMethod]
    public void PlayCard_AddsCardToTrick()
    {
        // Arrange
        var player = new Player("Test");
        var card = new Card(Suit.Clubs, Rank.Ace);
        player.DealHand(new List<Card> { card });
        var trick = new Trick([player]);

        // Act
        trick.PlayCard(player, card);

        // Assert
        Assert.IsTrue(trick.Cards.ContainsValue(card));
    }

    [TestMethod]
    public void GetPoints_ReturnsCorrectPoints()
    {
        // Arrange
        var player = new Player("Test");
        var card = new Card(Suit.Hearts, Rank.Ace);
        player.DealHand(new List<Card> { card });
        var trick = new Trick([player]);
        trick.PlayCard(player, card);

        // Act
        var points = trick.Points;

        // Assert
        Assert.AreEqual(1, points);
    }

    [TestMethod]
    public void GetNextPlayer_ReturnsCorrectPlayer()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");
        var trick = new Trick([player1, player2]);

        // Act
        Player? nextPlayer = trick.CurrentPlayer;

        // Assert
        Assert.AreEqual(player1, nextPlayer);
    }

    [TestMethod]
    public void OnTrickCompleted_SetsTrickCompleteToTrue()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");

        var trick = new Trick([player1, player2]);

        player1.DealHand(new List<Card> { new(Suit.Clubs, Rank.Ace) });
        player2.DealHand(new List<Card> { new(Suit.Clubs, Rank.Two) });

        // Act
        trick.PlayCard(player1, player1.Hand.First());
        trick.PlayCard(player2, player2.Hand.First());

        // Assert
        Assert.IsTrue(trick.TrickComplete);
        Assert.AreEqual(player1, trick.Winner);
    }

    [TestMethod]
    public void PlayCard_ThrowsException_WhenPlayerHasNoCards()
    {
        // Arrange
        var player = new Player("Test");
        var trick = new Trick([player]);
        player.DealHand(new List<Card>());

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => trick.PlayCard(player, new Card(Suit.Clubs, Rank.Ace)));
    }

    [TestMethod]
    public void PlayCard_ThrowsException_WhenCardIsNotValid()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");
        var card1 = new Card(Suit.Clubs, Rank.Ace);
        var card2 = new Card(Suit.Hearts, Rank.Two);
        player1.DealHand(new List<Card> { card1 });
        player2.DealHand(new List<Card> { card2 });
        var trick = new Trick([player1, player2]);
        trick.PlayCard(player1, card1);

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() =>
            trick.PlayCard(player2, new Card(Suit.Hearts, Rank.Three)));
    }
}