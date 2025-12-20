using System;
using UnityEngine;

namespace SHT.Core
{
    /// <summary>
    /// Local player implementation. Used for same-device multiplayer.
    /// For network play, create a NetworkPlayer that implements IPlayer
    /// and syncs state with the server.
    /// </summary>
    public class LocalPlayer : IPlayer
    {
        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; }
        public int PlayerIndex { get; private set; }
        public bool IsLocal => true;
        public bool IsReady => true;
        public int RoundScore { get; private set; }
        public int RoundsWon { get; private set; }
        public int HeadsRemaining { get; private set; }

        public event Action OnTurnStarted;
        public event Action OnTurnEnded;
        public event Action<int> OnScored;

        public LocalPlayer(int playerIndex, string displayName = null)
        {
            PlayerIndex = playerIndex;
            PlayerId = Guid.NewGuid().ToString();
            DisplayName = displayName ?? $"Player {playerIndex + 1}";
        }

        public void BeginTurn()
        {
            Debug.Log($"{DisplayName} turn started. Heads remaining: {HeadsRemaining}");
            OnTurnStarted?.Invoke();
        }

        public void EndTurn()
        {
            Debug.Log($"{DisplayName} turn ended. Score: {RoundScore}");
            OnTurnEnded?.Invoke();
        }

        public void AddScore(int points)
        {
            RoundScore += points;
            Debug.Log($"{DisplayName} scored {points} points. Total: {RoundScore}");
            OnScored?.Invoke(points);
        }

        public void UseHead()
        {
            if (HeadsRemaining > 0)
            {
                HeadsRemaining--;
                Debug.Log($"{DisplayName} used a head. Remaining: {HeadsRemaining}");
            }
        }

        public void ResetForRound(int headsPerRound)
        {
            RoundScore = 0;
            HeadsRemaining = headsPerRound;
            Debug.Log($"{DisplayName} reset for round. Heads: {HeadsRemaining}");
        }

        public void WinRound()
        {
            RoundsWon++;
            Debug.Log($"{DisplayName} won the round! Total rounds won: {RoundsWon}");
        }

        public void ResetForMatch()
        {
            RoundScore = 0;
            RoundsWon = 0;
            HeadsRemaining = 0;
            Debug.Log($"{DisplayName} reset for new match.");
        }
    }
}
