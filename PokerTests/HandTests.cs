﻿namespace PokerTests;

[TestClass]
public class HandTests
{
    [TestMethod]
    public void Pair()
    {
        var hand = new Hand(new Card[]
        {
            new Card(CardSuit.Spades, CardValue.Two),
            new Card(CardSuit.Hearts, CardValue.Two),
            new Card(CardSuit.Hearts, CardValue.Three),
            new Card(CardSuit.Spades, CardValue.Five),
            new Card(CardSuit.Diamonds, CardValue.Seven),
            new Card(CardSuit.Clubs, CardValue.Ten),
            new Card(CardSuit.Spades, CardValue.King)
        });
        Assert.IsTrue(hand.IsPair);
    }
}