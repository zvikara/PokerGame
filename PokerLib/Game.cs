﻿using System;
using System.Collections.Generic;

namespace PokerLib;

public class Game
{
    public int Id { get; set; }
    public int MaxPlayers { get; set; }
    public int PlayerCount { get; set; }
    public int Pot { get; set; }
    public int CurrentDealerLocation { get; }
    public int CurrentPlayerLocation { get; }
    public int LastBet { get; set; }
    public int GameDeckIndex { get; set; }
    public int CurrentRound { get; set; }
    public Card[] Deck;
    public Card[] TableCards;
    public List<Player> Players;
    public Player CurrentPlayer;
    public int CurrentPlayerIndex;

    public Game()
    {
        PlayerCount = 0;
        GameDeckIndex = 0;
        CurrentRound = 1;
        Pot = 0;
        TableCards = new Card[5];
        CreateDeck();
    }

    public void CreateDeck()
    {
        Deck = new Card[52];
        int n = 0;
        CardSuit s;
        CardValue v;
        for (int i = 2; i < 15; i++)
        {
            v = (CardValue)i;
            for (int j = 0; j < 4; j++)
            {
                s = (CardSuit)j;
                Deck[n] = new Card(s, v);
                n++;
            }
        }
        //ShuffleDeck();
        Deck.Shuffle();
    }

    //public void ShuffleDeck()
    //{
    //    int[] returnArr = new int[52];
    //    List<int> allNumbers = new List<int>();
    //    List<int> allEmptySpaces = new List<int>();
    //    Card[] deckcard = new Card[52];
    //    for (int i = 0; i < 52; i++)
    //    {
    //        deckcard[i] = Deck[i];
    //    }
    //    Random rng = new Random();

    //    for (int i = 0; i < 52; i++)
    //    {
    //        allNumbers.Add(i);
    //        allEmptySpaces.Add(i);
    //    }
    //    while (allNumbers.Count > 0)
    //    {
    //        int randomIndex = allEmptySpaces[rng.Next(allEmptySpaces.Count)];
    //        int number = allNumbers[rng.Next(allNumbers.Count)];

    //        allNumbers.Remove(number);
    //        allEmptySpaces.Remove(randomIndex);

    //        returnArr[randomIndex] = number;
    //    }
    //    for (int i = 0; i < 52; i++)
    //        Deck[returnArr[i]] = deckcard[i];
    //}

    public void AddPlayer(Player player)
    {
        Players.Add(player);
        PlayerCount++;
    }

    public void DealFirstRound()
    {
        foreach (var player in Players)
        {
            player.Cards = TakeCardsFromGameDeck(2);
        }
    }

    public Card[] TakeCardsFromGameDeck(int n)
    {
        var cards = new Card[n];
        for (int i = 0; i < n; i++)
        {
            cards[i] = Deck[i + GameDeckIndex];
        }
        GameDeckIndex += n;
        return cards;
    }

    public void DealCardsToGameCards(int n)
    {
        var cards = new Card[n];
        cards = TakeCardsFromGameDeck(5);
        for (int i = 0; i < cards.Length; i++)
        {
            TableCards[i] = cards[i];
        }
    }

    public void PayUp()
    {
        Player[] p = CheckWinners();
        int c = 0;
        for (int i = 0; i < p.Length; i++)
        {
            if (p[i] != null)
            {
                c++;
            }
        }
        for (int i = 0; i < c - 1; i++)
        {
            p[i].Pot += Pot / c;

        }
        Pot = 0;
    }

    public Player[] CheckWinners()
    {
        int n = 0;
        Player[] p = new Player[Players.Count];
        Player[] winners = new Player[Players.Count];
        for (int i = 0; i < Players.Count; i++)
        {
            p[i] = Players[i];
        }
        Hand h1 = new Hand();
        Hand h2 = new Hand();
        Card[] c1 = new Card[7];
        Card[] c2 = new Card[7];
        for (int j = 1; j < p.Length; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                c1[i] = TableCards[i];
                c2[i] = TableCards[i];
            }
            for (int i = 0; i < 2; i++)
            {
                c1[i + 5] = p[j - 1].Cards[i];
                c2[i + 5] = p[j].Cards[i];
            }

            h1.Cards = c1;
            h2.Cards = c2;
            if (h1.Strength() > h2.Strength())
            {
                winners[n] = p[j - 1];
                n++;
            }
            if (h1.Strength() < h2.Strength())
            {

                winners[n] = p[j];
                n++;
            }
            if (h1.Strength() == h2.Strength())
            {
                winners[n] = p[j - 1];
                winners[n + 1] = p[j];
                n = n + 2;
            }
            p[j] = winners[n - 1];

        }
        return winners;
    }

    public void AllIn()
    {
        var value = CurrentPlayer.Pot;
        BetOld(value);
    }

    public void Check()
    {
        var value = LastBet;
        BetOld(value);
    }

    public void BetPecentage(int percentage)
    {
        var value = Pot * percentage / 100;
        BetOld(value);
    }

    public void BetOld(int sum)
    {
        if (CurrentPlayer.Pot < sum)
            sum = CurrentPlayer.Pot;
        LastBet += sum;
        Pot += sum;
        CurrentPlayer.Pot -= sum;
    }

    public void SetNextPlayerTurn()
    {
        CurrentPlayerIndex++;
        if (CurrentPlayerIndex >= Players.Count)
        {
            CurrentPlayerIndex = 0;
        }
        CurrentPlayer = Players[CurrentPlayerIndex];
    }

    public bool Bet(Player player, BetType betType, int amount)
    {
        if (player != CurrentPlayer)
            return false;

        if (player.HasFolded)
            return false;

        var isValidBet = MakeBet(player, betType, amount);
        if (!isValidBet)
            return false;

        do { SetNextPlayerTurn(); }
        while (!CurrentPlayer.HasFolded);

        return true;
    }

    bool MakeBet(Player player, BetType betType, int amount)
    {
        switch (betType)
        {
            case BetType.Fold:
                player.HasFolded = true;
                return true;
            case BetType.Check:
                if (LastBet > 0)
                    return false;
                return true;
            case BetType.Bet:
                if (player.Pot < amount)
                    return false;
                Pot += amount;
                player.Pot -= amount;
                LastBet = amount;
                return true;
            case BetType.Call:
                return MakeBet(player, BetType.Bet, LastBet);
            case BetType.Raise:
                return MakeBet(player, BetType.Bet, LastBet + amount);
            case BetType.AllIn:
                return MakeBet(player, BetType.Bet, player.Pot);
            default:
                throw new ArgumentOutOfRangeException(nameof(betType));
        }
    }
}
