﻿using Hearts;

namespace HeartsTest;

[TestClass]
public class DeckTests
{
    [TestMethod]
    public void Deal_DistributesCardsEvenlyAmongPlayers()
    {
        // Arrange
        var deck = new Deck();
        var players = new List<Player>
        {
            new("Bob"),
            new("Steve"),
            new("Jane"),
            new("Beth")
        };

        // Act
        deck.Deal(players);

        // Assert
        foreach (Player player in players)
            Assert.AreEqual(13, player.Hand?.Count);
    }

    [TestMethod]
    public void Shuffle_RandomizesCardOrder()
    {
        // Arrange
        var deck1 = new Deck();
        var deck2 = new Deck();

        // Act
        deck1.Shuffle();
        deck2.Shuffle();

        // Assert
        Assert.IsFalse(deck1.Cards.SequenceEqual(deck2.Cards));
    }
}