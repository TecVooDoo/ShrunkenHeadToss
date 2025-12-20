using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SHT.Gameplay; 

namespace SHT.Core
{
    /// <summary>
    /// Central game state manager. Controls match flow, turns, and scoring.
    /// Designed for local play now, but IPlayer abstraction supports future networking.
    ///
    /// Match Flow:
    /// 1. StartMatch() - initializes players, resets scores
    /// 2. StartRound() - gives each player their heads
    /// 3. Players alternate turns until all heads thrown
    /// 4. EndRound() - determines round winner
    /// 5. Repeat until match winner (best of N rounds)
    /// 6. EndMatch() - declare winner
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Title("Match Settings")]
        [SerializeField, Range(1, 5)]
        private int _roundsToWin = 2;

        [SerializeField, Range(1, 8)]
        private int _headsPerRound = 4;

        [Title("Player Positions")]
        [SerializeField, Required]
        private Transform _player1Position;

        [SerializeField, Required]
        private Transform _player2Position;

        [Title("Toss Controllers")]
        [SerializeField, Required]
        private TossController _player1TossController;

        [SerializeField]
        private TossController _player2TossController;

        [Title("Debug")]
        [SerializeField, ReadOnly]
        private GameState _currentState = GameState.WaitingToStart;

        [SerializeField, ReadOnly]
        private int _currentPlayerIndex = 0;

        [SerializeField, ReadOnly]
        private int _currentRound = 0;

        // Players
        private List<IPlayer> _players = new List<IPlayer>();
        public IReadOnlyList<IPlayer> Players => _players;
        public IPlayer CurrentPlayer => _players.Count > _currentPlayerIndex ? _players[_currentPlayerIndex] : null;
        public int CurrentRound => _currentRound;
        public GameState CurrentState => _currentState;

        // Events for UI and other systems
        public event Action<GameState> OnStateChanged;
        public event Action<IPlayer> OnTurnChanged;
        public event Action<IPlayer, int> OnPlayerScored;
        public event Action<int> OnRoundStarted;
        public event Action<IPlayer> OnRoundEnded;
        public event Action<IPlayer> OnMatchEnded;

        private HeadController _activeHead;

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
            // Auto-start for testing. Remove when proper menu flow exists.
            InitializeLocalMatch();
        }

        /// <summary>
        /// Initialize a local 2-player match. Call this from menu or auto-start.
        /// </summary>
        [Button("Start Local Match")]
        public void InitializeLocalMatch()
        {
            _players.Clear();
            _players.Add(new LocalPlayer(0, "Player 1"));
            _players.Add(new LocalPlayer(1, "Player 2"));

            StartMatch();
        }

        /// <summary>
        /// Start the match. Resets all scores and begins first round.
        /// </summary>
        public void StartMatch()
        {
            foreach (var player in _players)
            {
                player.ResetForMatch();
            }

            _currentRound = 0;
            SetState(GameState.MatchStarting);

            StartRound();
        }

        /// <summary>
        /// Start a new round. Gives each player their heads.
        /// </summary>
        public void StartRound()
        {
            _currentRound++;
            Debug.Log($"=== Round {_currentRound} Starting ===");

            foreach (var player in _players)
            {
                player.ResetForRound(_headsPerRound);
            }

            // Alternate who goes first each round
            _currentPlayerIndex = (_currentRound - 1) % _players.Count;

            SetState(GameState.RoundInProgress);
            OnRoundStarted?.Invoke(_currentRound);

            StartTurn();
        }

        /// <summary>
        /// Start the current player's turn.
        /// </summary>
        private void StartTurn()
        {
            var player = CurrentPlayer;
            if (player == null) return;

            Debug.Log($"--- {player.DisplayName}'s Turn ---");
            player.BeginTurn();
            OnTurnChanged?.Invoke(player);

            // Enable the correct TossController based on player
            EnableTossController(player.PlayerIndex);
        }

        /// <summary>
        /// Enable the toss controller for the given player index.
        /// </summary>
        private void EnableTossController(int playerIndex)
        {
            // Disable all first
            if (_player1TossController != null)
                _player1TossController.DisableInput();
            if (_player2TossController != null)
                _player2TossController.DisableInput();

            // Enable the active player's controller
            TossController activeController = playerIndex == 0 ? _player1TossController : _player2TossController;
            if (activeController != null)
            {
                activeController.EnableInput();
            }
            else
            {
                Debug.LogWarning($"No TossController assigned for Player {playerIndex + 1}");
            }
        }

        /// <summary>
        /// Called by TossController when a head is launched.
        /// </summary>
        public void OnHeadLaunched(HeadController head)
        {
            _activeHead = head;
            head.OnHeadLanded += HandleHeadLanded;
            head.OnHeadScored += HandleHeadScored;

            CurrentPlayer?.UseHead();
        }

        /// <summary>
        /// Handle head scoring.
        /// </summary>
        private void HandleHeadScored(HeadController head, int points)
        {
            var player = CurrentPlayer;
            if (player != null)
            {
                player.AddScore(points);
                OnPlayerScored?.Invoke(player, points);
            }
        }

        /// <summary>
        /// Handle head landing (end of toss).
        /// </summary>
        private void HandleHeadLanded(HeadController head)
        {
            // Unsubscribe
            head.OnHeadLanded -= HandleHeadLanded;
            head.OnHeadScored -= HandleHeadScored;
            _activeHead = null;

            CurrentPlayer?.EndTurn();

            // Check if round is over
            if (IsRoundOver())
            {
                EndRound();
            }
            else
            {
                // Switch to next player
                NextPlayer();
                StartTurn();
            }
        }

        /// <summary>
        /// Move to next player.
        /// </summary>
        private void NextPlayer()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;

            // Skip players with no heads remaining
            int safetyCounter = 0;
            while (CurrentPlayer.HeadsRemaining <= 0 && safetyCounter < _players.Count)
            {
                _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
                safetyCounter++;
            }
        }

        /// <summary>
        /// Check if all players have used all heads.
        /// </summary>
        private bool IsRoundOver()
        {
            foreach (var player in _players)
            {
                if (player.HeadsRemaining > 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// End the current round, determine winner.
        /// </summary>
        private void EndRound()
        {
            SetState(GameState.RoundEnding);

            // Find round winner
            IPlayer roundWinner = null;
            int highScore = -1;
            bool isTie = false;

            foreach (var player in _players)
            {
                if (player.RoundScore > highScore)
                {
                    highScore = player.RoundScore;
                    roundWinner = player;
                    isTie = false;
                }
                else if (player.RoundScore == highScore)
                {
                    isTie = true;
                }
            }

            if (isTie)
            {
                Debug.Log($"Round {_currentRound} is a TIE! Score: {highScore}");
                // TODO: Handle tie (sudden death?)
            }
            else if (roundWinner != null)
            {
                roundWinner.WinRound();
                Debug.Log($"Round {_currentRound} winner: {roundWinner.DisplayName} with {highScore} points!");
            }

            OnRoundEnded?.Invoke(roundWinner);

            // Check for match winner
            if (CheckMatchWinner(out IPlayer matchWinner))
            {
                EndMatch(matchWinner);
            }
            else
            {
                // Start next round after brief delay
                // For now, just start immediately. Add delay/UI later.
                StartRound();
            }
        }

        /// <summary>
        /// Check if any player has won enough rounds.
        /// </summary>
        private bool CheckMatchWinner(out IPlayer winner)
        {
            winner = null;
            foreach (var player in _players)
            {
                if (player.RoundsWon >= _roundsToWin)
                {
                    winner = player;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// End the match.
        /// </summary>
        private void EndMatch(IPlayer winner)
        {
            SetState(GameState.MatchEnded);
            Debug.Log($"=== MATCH OVER === {winner.DisplayName} WINS!");
            OnMatchEnded?.Invoke(winner);
        }

        private void SetState(GameState newState)
        {
            _currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }

    public enum GameState
    {
        WaitingToStart,
        MatchStarting,
        RoundInProgress,
        RoundEnding,
        MatchEnded
    }
}
