using System;

namespace SHT.Core
{
    /// <summary>
    /// Interface for player abstraction. Supports both local and network players.
    /// Local players respond immediately; network players wait for server confirmation.
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// Unique identifier for this player (useful for networking).
        /// </summary>
        string PlayerId { get; }

        /// <summary>
        /// Display name for UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Player index (0 = Player 1, 1 = Player 2).
        /// Determines spawn position and target spike bed.
        /// </summary>
        int PlayerIndex { get; }

        /// <summary>
        /// Whether this player is controlled locally (vs network remote).
        /// </summary>
        bool IsLocal { get; }

        /// <summary>
        /// Whether this player is ready to take their turn.
        /// For local: always true when it's their turn.
        /// For network: true after connection confirmed.
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Current score this round.
        /// </summary>
        int RoundScore { get; }

        /// <summary>
        /// Total rounds won this match.
        /// </summary>
        int RoundsWon { get; }

        /// <summary>
        /// Heads remaining this round.
        /// </summary>
        int HeadsRemaining { get; }

        /// <summary>
        /// Called when this player's turn begins.
        /// </summary>
        event Action OnTurnStarted;

        /// <summary>
        /// Called when this player's turn ends (after head lands).
        /// </summary>
        event Action OnTurnEnded;

        /// <summary>
        /// Called when player scores points. Passes points earned.
        /// </summary>
        event Action<int> OnScored;

        /// <summary>
        /// Start this player's turn. Enables input for local, waits for network.
        /// </summary>
        void BeginTurn();

        /// <summary>
        /// End this player's turn. Called after head lands.
        /// </summary>
        void EndTurn();

        /// <summary>
        /// Add points to this player's round score.
        /// </summary>
        void AddScore(int points);

        /// <summary>
        /// Consume one head (after toss).
        /// </summary>
        void UseHead();

        /// <summary>
        /// Reset for new round (score to 0, heads to max).
        /// </summary>
        void ResetForRound(int headsPerRound);

        /// <summary>
        /// Award a round win.
        /// </summary>
        void WinRound();

        /// <summary>
        /// Full reset for new match.
        /// </summary>
        void ResetForMatch();
    }
}
