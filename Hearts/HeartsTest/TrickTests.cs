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
        var trick = new Trick([player], false);

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
        var trick = new Trick([player], false);
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
        var trick = new Trick([player1, player2], false);

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

        var trick = new Trick([player1, player2], false);
        var trickCompletedCount = 0;

        trick.TrickCompleted += (_, _) => trickCompletedCount++;

        player1.DealHand(new List<Card> { new(Suit.Clubs, Rank.Ace) });
        player2.DealHand(new List<Card> { new(Suit.Clubs, Rank.Two) });

        // Act
        trick.PlayCard(player1, player1.Hand.First());
        trick.PlayCard(player2, player2.Hand.First());

        // Assert
        Assert.IsTrue(trick.TrickComplete);
        Assert.AreEqual(player1, trick.Winner);
        Assert.AreEqual(1, trickCompletedCount);
    }

    [TestMethod]
    public void ActionRequested_IsRaisedCorrectly()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");
        var trick = new Trick([player1, player2], false);
        var actionRequestedCount = 0;

        trick.ActionRequested += (_, _) => actionRequestedCount++;

        player1.DealHand(new List<Card> { new(Suit.Clubs, Rank.Ace) });
        player2.DealHand(new List<Card> { new(Suit.Clubs, Rank.Two) });

        // Act
        trick.PlayCard(player1, player1.Hand.First());

        // Assert
        Assert.AreEqual(1, actionRequestedCount);
    }

    [TestMethod]
    public void ActionRequested_ReturnsCorrectActionRequestArgs()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");

        var trick = new Trick([player1, player2], false);

        ActionRequestArgs actionRequestArgs = null!;
        trick.ActionRequested += (_, args) => actionRequestArgs = args;

        var card = new Card(Suit.Clubs, Rank.Ace);
        player1.DealHand(new List<Card> { card });
        player2.DealHand(new List<Card> { new(Suit.Clubs, Rank.Two) });

        // Act
        trick.PlayCard(player1, card);

        // Assert
        Assert.AreEqual(player2, actionRequestArgs.Player);
        Assert.AreEqual(Suit.Clubs, actionRequestArgs.LeadingSuit);
        Assert.IsTrue(actionRequestArgs.CardsPlayed.Contains(card));
        Assert.IsTrue(actionRequestArgs.ValidCards.SequenceEqual(player2.Hand));
    }

    [TestMethod]
    public void GetValidCardsToPlay_ReturnsCorrectCards()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");
        var trick = new Trick([player1, player2], false);
        var card1 = new Card(Suit.Clubs, Rank.Ace);
        var card2 = new Card(Suit.Hearts, Rank.Two);
        player1.DealHand(new List<Card> { card1 });
        player2.DealHand(new List<Card> { card2 });

        ActionRequestArgs actionRequestArgs = null!;
        trick.ActionRequested += (_, args) => actionRequestArgs = args;

        // Act
        trick.PlayCard(player1, card1);

        // Assert
        Assert.IsTrue(actionRequestArgs.ValidCards.Contains(card2));
    }

    [TestMethod]
    public void GetValidCardsToPlay_ReturnsTwoOfClubs_WhenPlayerHasFullHandAndNoCardsPlayed()
    {
        // Arrange
        var player = new Player("Test");
        var trick = new Trick([player], false);

        ActionRequestArgs actionRequestArgs = null!;
        trick.ActionRequested += (_, args) => actionRequestArgs = args;

        var hand = Enumerable.Range(0, 13).Select(i => new Card(Suit.Clubs, (Rank)i)).ToList();
        player.DealHand(hand);

        // Act
        trick.StartTrick();

        // Assert
        Assert.AreEqual(1, actionRequestArgs.ValidCards.Count);
        Assert.IsTrue(actionRequestArgs.ValidCards.Single() is { Rank: Rank.Two, Suit: Suit.Clubs });
    }

    [TestMethod]
    public void GetValidCardsToPlay_ReturnsAllCards_WhenPlayerHasOnlyHeartsAndHeartsNotBroken()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");
        var trick = new Trick([player1, player2], false);

        ActionRequestArgs actionRequestArgs = null!;
        trick.ActionRequested += (_, args) => actionRequestArgs = args;

        var startCard = new Card(Suit.Clubs, Rank.Two);
        player1.DealHand([startCard]);

        var hand = Enumerable.Range(1, 13).Select(i => new Card(Suit.Hearts, (Rank)i)).ToList();
        player2.DealHand(hand);

        trick.StartTrick();

        // Act
        trick.PlayCard(player1, startCard);

        // Assert
        CollectionAssert.AreEquivalent(hand, actionRequestArgs.ValidCards.ToList());
    }

    [TestMethod]
    public void GetValidCardsToPlay_ReturnsNonHeartCards_WhenHeartsNotBrokenAndNoCardsPlayed()
    {
        // Arrange
        var player = new Player("Test");
        var trick = new Trick([player], false);

        ActionRequestArgs actionRequestArgs = null!;
        trick.ActionRequested += (_, args) => actionRequestArgs = args;

        var hand = new List<Card>
        {
            new(Suit.Clubs, Rank.Ace), new(Suit.Hearts, Rank.Two), new(Suit.Diamonds, Rank.Ace)
        };

        player.DealHand(hand);

        // Act
        trick.StartTrick();

        // Assert
        Assert.AreEqual(2, actionRequestArgs.ValidCards.Count);
        var expectedHand = new List<Card> { new(Suit.Clubs, Rank.Ace), new(Suit.Diamonds, Rank.Ace) };
        CollectionAssert.AreEquivalent(expectedHand, actionRequestArgs.ValidCards);
    }

    [TestMethod]
    public void GetValidCardsToPlay_ReturnsLeadingSuitCards_WhenPlayerHasLeadingSuitCards()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");
        var trick = new Trick([player1, player2], false);

        ActionRequestArgs actionRequestArgs = null!;
        trick.ActionRequested += (_, args) => actionRequestArgs = args;

        var card1 = new Card(Suit.Clubs, Rank.Ace);
        var card2 = new Card(Suit.Clubs, Rank.Two);
        player1.DealHand(new List<Card> { card1 });
        player2.DealHand(new List<Card> { card2, new(Suit.Diamonds, Rank.Three) });

        // Act

        trick.PlayCard(player1, card1);

        // Assert
        Assert.IsTrue(actionRequestArgs.ValidCards.Single().Suit == Suit.Clubs);
    }

    [TestMethod]
    public void GetValidCardsToPlay_ReturnsNonHeartCards_WhenHeartsNotBrokenAndPlayerHasNoLeadingSuitCards()
    {
        // Arrange
        var player1 = new Player("Test1");
        var player2 = new Player("Test2");
        var trick = new Trick([player1, player2], false);

        ActionRequestArgs actionRequestArgs = null!;
        trick.ActionRequested += (_, args) => actionRequestArgs = args;

        var card1 = new Card(Suit.Clubs, Rank.Ace);
        var card2 = new Card(Suit.Diamonds, Rank.Two);
        player1.DealHand(new List<Card> { card1 });
        player2.DealHand(new List<Card> { card2, new(Suit.Hearts, Rank.Three) });

        // Act
        trick.PlayCard(player1, card1);

        // Assert
        Assert.IsTrue(actionRequestArgs.ValidCards.Single().Suit == Suit.Diamonds);
    }

    [TestMethod]
    public void PlayCard_ThrowsException_WhenPlayerHasNoCards()
    {
        // Arrange
        var player = new Player("Test");
        var trick = new Trick([player], false);
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
        var trick = new Trick([player1, player2], false);
        trick.PlayCard(player1, card1);

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() =>
            trick.PlayCard(player2, new Card(Suit.Hearts, Rank.Three)));
    }
}