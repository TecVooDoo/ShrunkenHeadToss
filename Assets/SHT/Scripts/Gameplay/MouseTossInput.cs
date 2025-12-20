using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SHT.Gameplay
{
    /// <summary>
    /// Mouse/touch implementation of ITossInput using New Input System.
    /// </summary>
    public class MouseTossInput : MonoBehaviour, ITossInput
    {
        public event Action<Vector2> OnDragStarted;
        public event Action<Vector2> OnDragUpdated;
        public event Action<Vector2> OnDragEnded;
        public event Action OnDragCancelled;

        public bool IsEnabled { get; private set; }

        private bool _isDragging;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;

            if (_isDragging)
            {
                _isDragging = false;
                OnDragCancelled?.Invoke();
            }
        }

        private void Update()
        {
            if (!IsEnabled)
                return;

            var mouse = Mouse.current;
            if (mouse == null)
                return;

            Vector2 screenPos = mouse.position.ReadValue();
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);

            // Check for cancel (right-click or escape)
            if (_isDragging)
            {
                bool cancelRequested = mouse.rightButton.wasPressedThisFrame;

                var keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    cancelRequested |= keyboard.escapeKey.wasPressedThisFrame;
                }

                if (cancelRequested)
                {
                    _isDragging = false;
                    OnDragCancelled?.Invoke();
                    return;
                }
            }

            // Drag start
            if (mouse.leftButton.wasPressedThisFrame && !_isDragging)
            {
                _isDragging = true;
                OnDragStarted?.Invoke(worldPos);
            }
            // Drag update
            else if (mouse.leftButton.isPressed && _isDragging)
            {
                OnDragUpdated?.Invoke(worldPos);
            }
            // Drag end
            else if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
                OnDragEnded?.Invoke(worldPos);
            }
        }
    }
}
