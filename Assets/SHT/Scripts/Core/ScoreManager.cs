using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SHT.Core
{
    /// <summary>
    /// Tracks and displays scores. Subscribes to GameManager events.
    /// Provides score data for UI systems to display.
    ///
    /// For network play: this would sync scores from server authoritative state.
    /// Currently tracks local state only.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Title("Score Display (Debug)")]
        [SerializeField, ReadOnly]
        private int _player1RoundScore;

        [SerializeField, ReadOnly]
        private int _player2RoundScore;

        [SerializeField, ReadOnly]
        private int _player1RoundsWon;

        [SerializeField, ReadOnly]
        private int _player2RoundsWon;

        [SerializeField, ReadOnly]
        private int _currentRound;

        // Events for UI
        public event Action<int, int> OnScoreUpdated; // player1Score, player2Score
        public event Action<int, int> OnRoundsWonUpdated; // player1Rounds, player2Rounds
        public event Action<int> OnRoundChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to GameManager events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerScored += HandlePlayerScored;
                GameManager.Instance.OnRoundStarted += HandleRoundStarted;
                GameManager.Instance.OnRoundEnded += HandleRoundEnded;
                GameManager.Instance.OnMatchEnded += HandleMatchEnded;
            }
        }

        private void HandlePlayerScored(IPlayer player, int points)
        {
            UpdateScoreDisplay();
            Debug.Log($"[ScoreManager] {player.DisplayName} scored {points}. P1: {_player1RoundScore}, P2: {_player2RoundScore}");
        }

        private void HandleRoundStarted(int roundNumber)
        {
            _currentRound = roundNumber;
            UpdateScoreDisplay();
            OnRoundChanged?.Invoke(roundNumber);
            Debug.Log($"[ScoreManager] Round {roundNumber} started.");
        }

        private void HandleRoundEnded(IPlayer winner)
        {
            UpdateRoundsDisplay();
            if (winner != null)
            {
                Debug.Log($"[ScoreManager] Round ended. Winner: {winner.DisplayName}");
            }
            else
            {
                Debug.Log("[ScoreManager] Round ended in a tie.");
            }
        }

        private void HandleMatchEnded(IPlayer winner)
        {
            Debug.Log($"[ScoreManager] Match ended! Winner: {winner.DisplayName}");
        }

        private void UpdateScoreDisplay()
        {
            if (GameManager.Instance == null) return;

            var players = GameManager.Instance.Players;
            if (players.Count >= 1)
            {
                _player1RoundScore = players[0].RoundScore;
            }
            if (players.Count >= 2)
            {
                _player2RoundScore = players[1].RoundScore;
            }

            OnScoreUpdated?.Invoke(_player1RoundScore, _player2RoundScore);
        }

        private void UpdateRoundsDisplay()
        {
            if (GameManager.Instance == null) return;

            var players = GameManager.Instance.Players;
            if (players.Count >= 1)
            {
                _player1RoundsWon = players[0].RoundsWon;
            }
            if (players.Count >= 2)
            {
                _player2RoundsWon = players[1].RoundsWon;
            }

            OnRoundsWonUpdated?.Invoke(_player1RoundsWon, _player2RoundsWon);
        }

        /// <summary>
        /// Get current scores for UI display.
        /// </summary>
        public (int player1Score, int player2Score) GetRoundScores()
        {
            return (_player1RoundScore, _player2RoundScore);
        }

        /// <summary>
        /// Get rounds won for UI display.
        /// </summary>
        public (int player1Rounds, int player2Rounds) GetRoundsWon()
        {
            return (_player1RoundsWon, _player2RoundsWon);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerScored -= HandlePlayerScored;
                GameManager.Instance.OnRoundStarted -= HandleRoundStarted;
                GameManager.Instance.OnRoundEnded -= HandleRoundEnded;
                GameManager.Instance.OnMatchEnded -= HandleMatchEnded;
            }
        }
    }
}
