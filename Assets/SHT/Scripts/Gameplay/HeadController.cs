using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SHT.Gameplay
{
    /// <summary>
    /// Controls individual shrunken head behavior after launch.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class HeadController : MonoBehaviour 
    {
        public enum HeadState
        {
            Idle,
            Flying,
            Landed,
            Stacked
        }

        [Title("Settings")]
        [SerializeField]
        private float _rotationSpeed = 360f;

        [SerializeField]
        private float _settleVelocityThreshold = 0.1f;

        [SerializeField]
        private float _settleTimeRequired = 0.5f;

        [Title("Debug")]
        [SerializeField, ReadOnly]
        private HeadState _currentState = HeadState.Idle;

        [SerializeField, ReadOnly]
        private int _landedZonePoints;

        // Events
        public event Action<HeadController> OnHeadLanded;
        public event Action<HeadController, int> OnHeadScored;

        public HeadState CurrentState => _currentState;
        public int LandedZonePoints => _landedZonePoints;

        private Rigidbody2D _rb;
        private float _settleTimer;
        private bool _hasScored;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Launch the head with the given velocity.
        /// </summary>
        public void Launch(Vector2 velocity)
        {
            _currentState = HeadState.Flying;
            _rb.linearVelocity = velocity;
            _hasScored = false;
            _settleTimer = 0f;
        }

        private void Update()
        {
            if (_currentState == HeadState.Flying)
            {
                // Rotate while flying
                float rotationDirection = _rb.linearVelocity.x > 0 ? -1f : 1f;
                transform.Rotate(0f, 0f, rotationDirection * _rotationSpeed * Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (_currentState == HeadState.Flying)
            {
                CheckForSettled();
            }
        }

        private void CheckForSettled()
        {
            if (_rb.linearVelocity.magnitude < _settleVelocityThreshold)
            {
                _settleTimer += Time.fixedDeltaTime;

                if (_settleTimer >= _settleTimeRequired)
                {
                    SetLanded();
                }
            }
            else
            {
                _settleTimer = 0f;
            }
        }

        private void SetLanded()
        {
            _currentState = HeadState.Landed;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            OnHeadLanded?.Invoke(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log($"Head collision with: {collision.gameObject.name} (Layer: {LayerMask.LayerToName(collision.gameObject.layer)}) - State: {_currentState}, HasScored: {_hasScored}");

            if (_currentState != HeadState.Flying)
                return;

            // Check if landed on spikes
            if (collision.gameObject.layer == LayerMask.NameToLayer("Spikes"))
            {
                Debug.Log("Hit spikes!");
                HandleSpikeCollision(collision);
            }
            // Check if landed on another head
            else if (collision.gameObject.TryGetComponent<HeadController>(out var otherHead))
            {
                Debug.Log("Hit another head!");
                HandleHeadStackCollision(otherHead);
            }
            // Check if hit ground (miss) - only count as miss if we haven't already scored
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !_hasScored)
            {
                Debug.Log("Hit ground - miss!");
                HandleGroundCollision();
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && _hasScored)
            {
                Debug.Log("Hit ground after scoring - points retained!");
            }
        }

        private void HandleSpikeCollision(Collision2D collision)
        {
            // Get zone points from the spike zone if available
            var spikeZone = collision.gameObject.GetComponent<SpikeZone>();
            if (spikeZone != null && !_hasScored)
            {
                _landedZonePoints = spikeZone.PointValue;
                _hasScored = true;
                Debug.Log($"SCORED! {_landedZonePoints} points on {spikeZone.Type} zone");
                OnHeadScored?.Invoke(this, _landedZonePoints);
            }
            else if (spikeZone == null)
            {
                Debug.LogWarning("Hit Spikes layer but no SpikeZone component found!");
            }
        }

        private void HandleHeadStackCollision(HeadController otherHead)
        {
            // Only stack if the other head has already landed
            if (otherHead.CurrentState == HeadState.Landed ||
                otherHead.CurrentState == HeadState.Stacked)
            {
                _currentState = HeadState.Stacked;
                // Stacking bonus handled by ScoreManager listening to events
            }
        }

        private void HandleGroundCollision()
        {
            // Missed the target - no points
            _landedZonePoints = 0;
        }

        /// <summary>
        /// Reset head for reuse (object pooling).
        /// </summary>
        public void Reset()
        {
            _currentState = HeadState.Idle;
            _landedZonePoints = 0;
            _hasScored = false;
            _settleTimer = 0f;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            transform.rotation = Quaternion.identity;
        }
    }
}
