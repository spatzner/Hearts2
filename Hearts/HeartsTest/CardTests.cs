using Hearts;

namespace HeartsTest;

[TestClass]
public class CardTests
{
    [TestMethod]
    public void Points_ReturnsCorrectPointsForQueenOfSpades()
    {
        // Arrange
        var card = new Card(Suit.Spades, Rank.Queen);

        // Assert
        Assert.AreEqual(13, card.Points);
    }

    [TestMethod]
    public void Points_ReturnsCorrectPointsForHearts()
    {
        // Arrange
        var card = new Card(Suit.Hearts, Rank.Ace);

        // Assert
        Assert.AreEqual(1, card.Points);
    }

    [TestMethod]
    public void Points_ReturnsZeroForNonScoringCards()
    {
        // Arrange
        var card = new Card(Suit.Clubs, Rank.Ace);

        // Assert
        Assert.AreEqual(0, card.Points);
    }

    [TestMethod]
    public void ToString_ReturnsCorrectString()
    {
        // Arrange
        var card = new Card(Suit.Clubs, Rank.Ace);

        // Act
        var result = card.ToString();

        // Assert
        Assert.AreEqual("Ace of Clubs", result);
    }
}