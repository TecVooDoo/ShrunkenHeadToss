using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SHT.Gameplay
{
    /// <summary>
    /// Controls individual shrunken head behavior after launch.
    ///
    /// Scoring Rules:
    /// - Hit correct target FIRST = score points, head sticks (impaled)
    /// - Hit anything else first (wrong target, ground, bounds) = no points, turn ends immediately
    /// - Once scored, points are locked in
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class HeadController : MonoBehaviour
    {
        public enum HeadState
        {
            Idle,
            Flying,
            Landed,     // Scored and impaled on target
            Missed      // Hit something other than target first
        }

        [Title("Settings")]
        [SerializeField]
        private float _rotationSpeed = 360f;

        [SerializeField, Tooltip("How far down the spike the head settles when impaled")]
        private float _impaleDepth = 0.3f;

        [Title("Debug")]
        [SerializeField, ReadOnly]
        private HeadState _currentState = HeadState.Idle;

        [SerializeField, ReadOnly]
        private int _landedZonePoints;

        [SerializeField, ReadOnly]
        private bool _turnEnded;

        // Events
        public event Action<HeadController> OnHeadLanded;
        public event Action<HeadController, int> OnHeadScored;

        public HeadState CurrentState => _currentState;
        public int LandedZonePoints => _landedZonePoints;
        public int OwnerPlayerIndex => _ownerPlayerIndex;
        public SpikeZone.TargetSide? ImpaledOnSide => _impaledOnSide;

        private Rigidbody2D _rb;
        private int _ownerPlayerIndex = -1;
        private SpikeZone.TargetSide? _impaledOnSide = null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Launch the head with the given velocity.
        /// </summary>
        /// <param name="velocity">Initial velocity</param>
        /// <param name="playerIndex">Index of the player who threw this head (0 or 1)</param>
        public void Launch(Vector2 velocity, int playerIndex = -1)
        {
            _currentState = HeadState.Flying;
            _rb.linearVelocity = velocity;
            _landedZonePoints = 0;
            _turnEnded = false;
            _ownerPlayerIndex = playerIndex;
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Only process first collision while flying
            if (_currentState != HeadState.Flying)
                return;

            string layerName = LayerMask.LayerToName(collision.gameObject.layer);
            Debug.Log($"Head collision with: {collision.gameObject.name} (Layer: {layerName})");

            // Check if hit spikes (potential target)
            if (collision.gameObject.layer == LayerMask.NameToLayer("Spikes"))
            {
                HandleSpikeCollision(collision);
            }
            // Check if hit another head
            else if (collision.gameObject.layer == LayerMask.NameToLayer("Heads"))
            {
                HandleHeadCollision(collision);
            }
            // Hit anything else = miss, turn ends immediately
            else
            {
                Debug.Log($"MISS! Hit {layerName} before target.");
                EndTurnAsMiss();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only process while flying
            if (_currentState != HeadState.Flying)
                return;

            // Out of bounds = miss
            if (other.gameObject.layer == LayerMask.NameToLayer("Bounds"))
            {
                Debug.Log("MISS! Head went OUT OF BOUNDS!");
                EndTurnAsMiss();
            }
        }

        private void HandleSpikeCollision(Collision2D collision)
        {
            var spikeZone = collision.gameObject.GetComponent<SpikeZone>();

            if (spikeZone == null)
            {
                Debug.LogWarning("Hit Spikes layer but no SpikeZone component found!");
                EndTurnAsMiss();
                return;
            }

            // Check if this is the correct target for this player
            if (_ownerPlayerIndex >= 0 && !spikeZone.IsValidTargetForPlayer(_ownerPlayerIndex))
            {
                Debug.Log($"MISS! Player {_ownerPlayerIndex + 1} hit WRONG target ({spikeZone.Side} zone).");
                EndTurnAsMiss();
                return;
            }

            // SCORED! Head impales on target
            ScoreAndImpale(spikeZone.PointValue, spikeZone.Side, spikeZone.Type.ToString());
        }

        private void HandleHeadCollision(Collision2D collision)
        {
            var otherHead = collision.gameObject.GetComponent<HeadController>();

            if (otherHead == null)
            {
                Debug.Log("MISS! Hit object on Heads layer but no HeadController found.");
                EndTurnAsMiss();
                return;
            }

            // Check if the other head is impaled on a valid target for us
            if (otherHead.CurrentState == HeadState.Landed && otherHead.ImpaledOnSide.HasValue)
            {
                // The other head is impaled - check if it's on our valid target
                bool isValidTarget = (_ownerPlayerIndex == 0 && otherHead.ImpaledOnSide == SpikeZone.TargetSide.Right) ||
                                     (_ownerPlayerIndex == 1 && otherHead.ImpaledOnSide == SpikeZone.TargetSide.Left);

                if (isValidTarget)
                {
                    // Stacking on valid target! Award points (same as outer zone for stacking)
                    int stackPoints = 5; // Could make this configurable or add bonus
                    ScoreAndImpale(stackPoints, otherHead.ImpaledOnSide.Value, "STACKED on head");
                    return;
                }
                else
                {
                    // Hit opponent's impaled head - ignore collision, let head pass through
                    // This prevents stacked heads from blocking opponent's throws
                    Debug.Log($"Passed through opponent's impaled head on {otherHead.ImpaledOnSide} target.");
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider);
                    return;
                }
            }

            // Other head is not impaled (on ground or still flying) = miss
            Debug.Log("MISS! Hit a head that is NOT impaled on a target.");
            EndTurnAsMiss();
        }

        private void ScoreAndImpale(int points, SpikeZone.TargetSide side, string description)
        {
            _landedZonePoints = points;
            _currentState = HeadState.Landed;
            _impaledOnSide = side;

            // Stop the head (impaled)
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic; // Freeze in place

            // Settle the head down onto the spike
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y - _impaleDepth,
                transform.position.z
            );

            Debug.Log($"SCORED! {_landedZonePoints} points - {description} - HEAD IMPALED!");

            OnHeadScored?.Invoke(this, _landedZonePoints);
            EndTurn();
        }

        private void EndTurnAsMiss()
        {
            _currentState = HeadState.Missed;
            _landedZonePoints = 0;
            EndTurn();
        }

        private void EndTurn()
        {
            if (_turnEnded)
                return;

            _turnEnded = true;
            OnHeadLanded?.Invoke(this);
        }

        /// <summary>
        /// Reset head for reuse (object pooling).
        /// </summary>
        public void Reset()
        {
            _currentState = HeadState.Idle;
            _landedZonePoints = 0;
            _turnEnded = false;
            _ownerPlayerIndex = -1;
            _impaledOnSide = null;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            transform.rotation = Quaternion.identity;
        }
    }
}
