﻿using Hearts;

namespace HeartsTest;

[TestClass]
public class DeckTests
{
    [TestMethod]
    public void Deck_Constructor_CreatesFullDeck()
    {
        // Arrange & Act
        var deck = new Deck(4);
        var player = new Player("Test");
        deck.DealShuffled([player]);

        // Assert
        Assert.AreEqual(52, player.Hand.Count);
    }

    [TestMethod]
    public void Deck_Constructor_RemovesCardsToMakeEvenHands()
    {
        // Arrange & Act
        var deck = new Deck(3);
        List<Player> players = [new Player("Test1"), new Player("Test2"), new Player("Test3")];
        deck.DealShuffled(players);

        // Assert
        Assert.AreEqual(players[0].Hand.Count, players[1].Hand.Count);
        Assert.AreEqual(players[1].Hand.Count, players[2].Hand.Count);
    }

    [TestMethod]
    public void Deck_Constructor_CreatesAllUniqueCards()
    {
        // Arrange & Act
        var deck = new Deck(4);
        var player = new Player("Test");
        deck.DealShuffled([player]);

        // Assert
        Assert.AreEqual(player.Hand.Count, player.Hand.Distinct().Count());
    }

    [TestMethod]
    public void DealShuffled_GivesEachPlayerUniqueCards()
    {
        // Arrange
        var deck = new Deck(4);
        var players = new List<Player>
        {
            new("Bob"),
            new("Steve"),
            new("Jane"),
            new("Beth")
        };

        // Act
        deck.DealShuffled(players);

        // Assert
        var allCards = players.SelectMany(p => p.Hand).ToList();
        Assert.AreEqual(allCards.Count, allCards.Distinct().Count());
    }

    [TestMethod]
    public void DealShuffled_ShufflesCardsForEachCall()
    {
        // Arrange
        var deck = new Deck(4);
        var players1 = new List<Player>
        {
            new("Bob"),
            new("Steve"),
            new("Jane"),
            new("Beth")
        };
        var players2 = new List<Player>
        {
            new("Alice"),
            new("Charlie"),
            new("Dave"),
            new("Eve")
        };

        // Act
        deck.DealShuffled(players1);
        var hands1 = players1.SelectMany(p => p.Hand).ToList();

        deck.DealShuffled(players2);
        var hands2 = players2.SelectMany(p => p.Hand).ToList();

        // Assert
        Assert.IsFalse(hands1.SequenceEqual(hands2));
    }
}