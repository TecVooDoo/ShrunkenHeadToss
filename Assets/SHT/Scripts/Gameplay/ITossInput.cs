using System;
using UnityEngine;

namespace SHT.Gameplay
{
    /// <summary>
    /// Interface for toss input handling.
    /// Allows different input implementations (mouse, touch, controller).
    /// </summary>
    public interface ITossInput
    {
        /// <summary>
        /// Fired when drag starts.
        /// </summary>
        event Action<Vector2> OnDragStarted;

        /// <summary>
        /// Fired each frame during drag with current position.
        /// </summary>
        event Action<Vector2> OnDragUpdated;

        /// <summary>
        /// Fired when drag ends with final position.
        /// </summary>
        event Action<Vector2> OnDragEnded;

        /// <summary>
        /// Fired when toss is cancelled (e.g., right-click or escape).
        /// </summary>
        event Action OnDragCancelled;

        /// <summary>
        /// Whether input is currently enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Enable input processing.
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable input processing.
        /// </summary>
        void Disable();
    }
}
