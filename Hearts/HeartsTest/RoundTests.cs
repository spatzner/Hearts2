using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Hearts;
using Moq;

namespace HeartsTest;

[TestClass]
public class RoundTests
{
    private ITrickFactory _trickFactory = null!;
    private Mock<IDeckFactory> _mockDeckFactory = null!;
    private Mock<IDeck> _mockDeck = null!;

    [TestInitialize]
    public void SetUp()
    {
        _trickFactory = new TrickFactory();
        _mockDeckFactory = new Mock<IDeckFactory>();
        _mockDeck = new Mock<IDeck>();
        _mockDeckFactory.Setup(x => x.CreateDeck(It.IsAny<int>())).Returns(_mockDeck.Object);
    }

    [TestMethod]
    public void StartRound_RaisesActionRequested()
    {
        //Arrange
        bool eventRaised = false;
        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args) => eventRaised = true;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].DealHand([new Card(Suit.Clubs, (Rank)i)]);
                }
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        //Act
        sut.StartRound();

        //Assert
        Assert.IsTrue(eventRaised);
    }

    [TestMethod]
    public void PlayCard_RaisesActionRequestedWithCorrectArgs()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].DealHand([new Card(Suit.Clubs, (Rank)i)]);
                }
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //has to be wired up after StartRound to only trigger after the first card is played
        sut.ActionRequested += (sender, args1) => args = args1;

        //Act
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));

        //Assert
        CollectionAssert.AreEqual(new List<Card> { new(Suit.Clubs, Rank.Two) }, args.CardsPlayed);
        Assert.AreEqual(Suit.Clubs, args.LeadingSuit);
        Assert.AreEqual(players[1], args.Player);
        CollectionAssert.AreEqual(new List<Card> { new(Suit.Clubs, Rank.Three) }, args.ValidCards);
    }

    [TestMethod]
    public void WhenPlayerLeadingRound_MustPlayLeadingCard()
    {
        //Arrange

        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].DealHand([new Card(Suit.Clubs, (Rank)i)]);
                }
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        //Act
        sut.StartRound();

        //Assert
        CollectionAssert.AreEqual(new List<Card> { new(Suit.Clubs, Rank.Two) }, args!.ValidCards);
    }

    [TestMethod]
    public void WhenPlayerOnlyHasHearts_CanPlayHearts()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two)]);
                players[1].DealHand([new Card(Suit.Hearts, Rank.Two)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));

        //Assert
        Assert.IsTrue(args!.ValidCards.All(c => c.Suit == Suit.Hearts));
    }

    [TestMethod]
    public void WhenPlayerLeadingTrickButNotRound_CanPlayNonHeartsIfHeartsNotBrokenAndHasNonHearts()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1]
                   .DealHand([
                        new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two),
                        new Card(Suit.Hearts, Rank.Three)
                    ]);
                players[2].DealHand([new Card(Suit.Clubs, Rank.Three), new Card(Suit.Hearts, Rank.Two)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Diamonds, Rank.Seven)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        //player1 wins the trick and hearts are broken
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Clubs, Rank.Three));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        //Assert
        Assert.IsFalse(args!.ValidCards.Any(c => c.Suit == Suit.Hearts));
    }

    [TestMethod]
    public void WhenPlayerLeadingTrickButNotRound_CanPlayAllCardsIfHeartsBroken()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1]
                   .DealHand([
                        new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two),
                        new Card(Suit.Hearts, Rank.Three)
                    ]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Diamonds, Rank.Seven)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        //player1 wins the trick and hearts are broken
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        //Assert
        CollectionAssert.AreEqual(players[1].Hand.ToList(), args.ValidCards);
    }

    [TestMethod]
    public void WhenPlayerHasLeadingSuit_CanOnlyPlayLeadingSuitCards()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Clubs, Rank.Three)]);
                players[1].DealHand([new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));

        //Assert
        Assert.IsTrue(args!.ValidCards.All(c => c.Suit == Suit.Clubs));
    }

    [TestMethod]
    public void WhenHeartsHasBeenBroken_LeadingPlayerCanPlayHearts()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1]
                   .DealHand([
                        new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two),
                        new Card(Suit.Hearts, Rank.Three)
                    ]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Diamonds, Rank.Seven)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        //player1 wins the trick and hearts are broken
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        //Assert
        Assert.IsTrue(args!.ValidCards.Any(c => c.Suit == Suit.Hearts));
    }

    [TestMethod]
    public void WhenHeartsHasBeenBroken_NonLeadingPlayerCanPlayHearts()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1]
                   .DealHand([
                        new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two),
                        new Card(Suit.Hearts, Rank.Three)
                    ]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3]
                   .DealHand([
                        new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Hearts, Rank.Seven),
                        new Card(Suit.Spades, Rank.Jack)
                    ]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        //player1 wins the trick and hearts are broken
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        //trick2
        sut.PlayCard(players[1], new Card(Suit.Diamonds, Rank.Two));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Two));

        //Assert
        Assert.IsTrue(args!.ValidCards.Any(c => c.Suit == Suit.Hearts));
    }

    [TestMethod]
    public void WhenHeartsHasBeenBrokenAndNonLeadingPlayerDoesNotHaveLeadingSuit_LeadingPlayerCanPlayAllCards()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1]
                   .DealHand([
                        new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two),
                        new Card(Suit.Hearts, Rank.Three)
                    ]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Hearts, Rank.Seven)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        //player1 wins the trick and hearts are broken
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        //Hearts broken
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));

        //Assert
        Assert.IsTrue(players[3].Hand.Any(c => c.Suit == Suit.Hearts));
        Assert.IsFalse(players[3].Hand.Any(c => c.Suit == Suit.Clubs));
        CollectionAssert.AreEqual(players[3].Hand.ToList(), args.ValidCards);
    }

    [TestMethod]
    public void WhenPlayerWinsTrick_LeadsNextTrick()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1]
                   .DealHand([
                        new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two),
                        new Card(Suit.Hearts, Rank.Three)
                    ]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3]
                   .DealHand([
                        new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Hearts, Rank.Seven),
                        new Card(Suit.Spades, Rank.Jack)
                    ]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        //player1 wins the trick and hearts are broken
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        //Assert
        Assert.AreEqual(sut.Tricks.First().Winner, args.Player);
    }

    [TestMethod]
    public void WhenTrickCreated_IsAddedToTricksList()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1]
                   .DealHand([
                        new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two),
                        new Card(Suit.Hearts, Rank.Three)
                    ]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3]
                   .DealHand([
                        new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Hearts, Rank.Seven),
                        new Card(Suit.Spades, Rank.Jack)
                    ]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        //player1 wins the trick and hearts are broken
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        //Assert
        Assert.AreEqual(2, sut.Tricks.Count);
    }

    [TestMethod]
    public void WhenAccessingCurrentTrick_ReturnsLatestTrick()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1].DealHand([new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two)]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Hearts, Rank.Seven)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        //player1 wins the trick and hearts are broken
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        //Assert
        Assert.AreEqual(sut.CurrentTrick, sut.Tricks.Last());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void WhenCardPlayed_IfNotCurrentPlayer_Throws()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1].DealHand([new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two)]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Hearts, Rank.Seven)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void WhenCardPlayed_IfCurrentPlayerDoesNotHaveCard_Throws()
    {
        //Arrange
        ActionRequestArgs args = null!;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.ActionRequested += (sender, args1) => args = args1;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1].DealHand([new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two)]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Hearts, Rank.Seven)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Hearts, Rank.Ace));
    }

    [TestMethod]
    public void WhenPlayersAreOutOfCards_RoundIsCompleted()
    {
        //Arrange
        bool eventRaised = false;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.RoundCompleted += (sender, args1) => eventRaised = true;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two)]);
                players[1].DealHand([new Card(Suit.Clubs, Rank.Four)]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        Assert.IsTrue(eventRaised);
    }

    [TestMethod]
    public void WhenTrickCompletedButRoundIsNotCompleted_RaisesTrickCompletedEvent()
    {
        //Arrange
        bool eventRaised = false;

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        sut.TrickCompleted += (sender, args1) => eventRaised = true;

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                players[0].DealHand([new Card(Suit.Clubs, Rank.Two), new Card(Suit.Spades, Rank.Three)]);
                players[1].DealHand([new Card(Suit.Clubs, Rank.Four), new Card(Suit.Diamonds, Rank.Two)]);
                players[2].DealHand([new Card(Suit.Hearts, Rank.Four), new Card(Suit.Hearts, Rank.Two)]);
                players[3].DealHand([new Card(Suit.Diamonds, Rank.Four), new Card(Suit.Hearts, Rank.Seven)]);
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        sut.PlayCard(players[0], new Card(Suit.Clubs, Rank.Two));
        sut.PlayCard(players[1], new Card(Suit.Clubs, Rank.Four));
        sut.PlayCard(players[2], new Card(Suit.Hearts, Rank.Four));
        sut.PlayCard(players[3], new Card(Suit.Diamonds, Rank.Four));

        Assert.IsTrue(eventRaised);
    }

    [TestMethod]
    public void WhenPlayerShootsTheMoon_AllOtherPlayersGetThePoints()
    {
        //Arrange

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                //player0 will win all the rounds because they lead all rounds with clubs and no one else can play Clubs to beat the leading card
                players[0].DealHand(Enumerable.Range(0, 13).Select(rank => new Card(Suit.Clubs, (Rank)rank)));
                players[1].DealHand(Enumerable.Range(0, 13).Select(rank => new Card(Suit.Hearts, (Rank)rank)));
                players[2].DealHand(Enumerable.Range(0, 13).Select(rank => new Card(Suit.Spades, (Rank)rank)));
                players[3].DealHand(Enumerable.Range(0, 13).Select(rank => new Card(Suit.Diamonds, (Rank)rank)));
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        sut.StartRound();

        //Act
        for (int i = 0; i < 13; i++)
        {
            sut.PlayCard(players[0], players[0].Hand.First());
            sut.PlayCard(players[1], players[1].Hand.First());
            sut.PlayCard(players[2], players[2].Hand.First());
            sut.PlayCard(players[3], players[3].Hand.First());
        }

        Assert.AreEqual(0, players[0].Score);
        Assert.AreEqual(26, players[1].Score);
        Assert.AreEqual(26, players[2].Score);
        Assert.AreEqual(26, players[3].Score);
    }

    [TestMethod]
    public void WhenPlayerDoesNotShootTheMoon_PointsAreCorrectlyAllocated()
    {
        //Arrange

        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        Round sut = new Round(players, _trickFactory, _mockDeckFactory.Object);
        
        sut.ActionRequested += (sender, args) => { sut.PlayCard(args.Player, args.ValidCards[0]); };

        _mockDeck
           .Setup(x => x.DealShuffled(players))
           .Callback(() =>
            {
                //example of random hands
                players[0].DealHand(new List<Card>{ new(Suit.Diamonds, Rank.Three), new(Suit.Spades, Rank.Eight), new(Suit.Clubs, Rank.Ten), new(Suit.Diamonds, Rank.Eight), new(Suit.Hearts, Rank.Two), new(Suit.Diamonds, Rank.Six), new(Suit.Spades, Rank.Four), new(Suit.Diamonds, Rank.Jack), new(Suit.Clubs, Rank.Ace), new(Suit.Clubs, Rank.Four), new(Suit.Clubs, Rank.Five), new(Suit.Hearts, Rank.Three), new(Suit.Diamonds, Rank.Five) });
                players[1].DealHand(new List<Card> { new(Suit.Spades, Rank.Nine), new(Suit.Diamonds, Rank.Ten), new(Suit.Hearts, Rank.Eight), new(Suit.Diamonds, Rank.Four), new(Suit.Diamonds, Rank.Two), new(Suit.Hearts, Rank.Nine), new(Suit.Spades, Rank.Two), new(Suit.Spades, Rank.Three), new(Suit.Hearts, Rank.Four), new(Suit.Spades, Rank.Six), new(Suit.Hearts, Rank.Jack), new(Suit.Spades, Rank.Seven), new(Suit.Diamonds, Rank.Nine) });
                players[2].DealHand(new List<Card> { new(Suit.Clubs, Rank.Eight), new(Suit.Hearts, Rank.Five), new(Suit.Hearts, Rank.Ten), new(Suit.Diamonds, Rank.Queen), new(Suit.Spades, Rank.Ace), new(Suit.Spades, Rank.Five), new(Suit.Hearts, Rank.Six), new(Suit.Clubs, Rank.Queen), new(Suit.Diamonds, Rank.King), new(Suit.Hearts, Rank.Seven), new(Suit.Hearts, Rank.King), new(Suit.Diamonds, Rank.Seven), new(Suit.Spades, Rank.Jack) });
                players[3].DealHand(new List<Card> { new(Suit.Spades, Rank.Ten), new(Suit.Spades, Rank.Queen), new(Suit.Clubs, Rank.Two), new(Suit.Clubs, Rank.Jack), new(Suit.Clubs, Rank.Six), new(Suit.Clubs, Rank.Seven), new(Suit.Diamonds, Rank.Ace), new(Suit.Clubs, Rank.Nine), new(Suit.Clubs, Rank.King), new(Suit.Spades, Rank.King), new(Suit.Clubs, Rank.Three), new(Suit.Hearts, Rank.Queen), new(Suit.Hearts, Rank.Ace) });
            });
        _mockDeck.Setup(x => x.StartingCard).Returns(new Card(Suit.Clubs, Rank.Two));

        //Act
        sut.StartRound();

        Assert.AreEqual(sut.Tricks.Where(t => t.Winner == players[0]).Sum(t => t.Points), players[0].Score);
        Assert.AreEqual(sut.Tricks.Where(t => t.Winner == players[1]).Sum(t => t.Points), players[1].Score);
        Assert.AreEqual(sut.Tricks.Where(t => t.Winner == players[2]).Sum(t => t.Points), players[2].Score);
        Assert.AreEqual(sut.Tricks.Where(t => t.Winner == players[3]).Sum(t => t.Points), players[3].Score);
        Assert.AreEqual(26, players.Sum(x => x.Score));
    }

    public void CreateRandomDeal()
    {
        List<Player> players =
            [new Player("Player0"), new Player("Player1"), new Player("Player2"), new Player("Player3")];
        
        Deck deck = new Deck(4);
        
        deck.DealShuffled(players);

        string p0hand = string.Join(", ", players[0].Hand.Select(c => $"new Card(Suit.{c.Suit}, Rank.{c.Rank})"));
        string p1hand = string.Join(", ", players[1].Hand.Select(c => $"new Card(Suit.{c.Suit}, Rank.{c.Rank})"));
        string p2hand = string.Join(", ", players[2].Hand.Select(c => $"new Card(Suit.{c.Suit}, Rank.{c.Rank})"));
        string p3hand = string.Join(", ", players[3].Hand.Select(c => $"new Card(Suit.{c.Suit}, Rank.{c.Rank})"));
        int x = 0;
    }

    //round scoring tests
}