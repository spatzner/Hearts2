using Hearts;

namespace HeartsTest;

[TestClass]
public class PlayerTests
{
    [TestMethod]
    public void DealHand_SetsHandCorrectly()
    {
        // Arrange
        var player = new Player("Test");
        var hand = new List<Card> { new(Suit.Clubs, Rank.Ace), new(Suit.Diamonds, Rank.Two) };

        // Act
        player.DealHand(hand);

        // Assert
        Assert.IsTrue(player.Hand!.SequenceEqual(hand));
    }

    [TestMethod]
    public void PlayCard_RemovesCardFromHand()
    {
        // Arrange
        var player = new Player("Test");
        var hand = new List<Card> { new(Suit.Clubs, Rank.Ace), new(Suit.Diamonds, Rank.Two) };
        player.DealHand(hand);

        // Act
        player.PlayCard(hand[0]);

        // Assert
        Assert.IsFalse(player.Hand!.Contains(hand[0]));
    }

    [TestMethod]
    public void HasFullHand_ReturnsTrueWhenHandHas13Cards()
    {
        // Arrange
        var player = new Player("Test");
        var hand = Enumerable.Range(1, 13).Select(i => new Card(Suit.Clubs, (Rank)i)).ToList();
        player.DealHand(hand);

        // Act
        var result = player.HasFullHand();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasRoundStartCard_ReturnsTrueWhenHandContainsTwoOfClubs()
    {
        // Arrange
        var player = new Player("Test");
        var hand = new List<Card> { new(Suit.Clubs, Rank.Two), new(Suit.Diamonds, Rank.Three) };
        player.DealHand(hand);

        // Act
        var result = player.HasRoundStartCard();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void TakePoints_AddsPointsToScore()
    {
        // Arrange
        var player = new Player("Test");

        // Act
        player.TakePoints(14);

        // Assert
        Assert.AreEqual(14, player.Score);
    }
}