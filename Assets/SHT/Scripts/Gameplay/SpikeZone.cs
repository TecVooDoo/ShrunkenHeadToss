using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SHT.Gameplay
{
    /// <summary>
    /// Defines a scoring zone on the spike bed.
    /// Attach to collider GameObjects that represent different scoring areas.
    /// Tracks impaled heads and calculates stacking positions.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SpikeZone : MonoBehaviour
    {
        public enum ZoneType
        {
            Center = 10,
            Inner = 7,
            Outer = 5,
            Edge = 3
        }

        public enum TargetSide
        {
            Left,   // Valid target for Player 2 (throws from right)
            Right   // Valid target for Player 1 (throws from left)
        }

        [Title("Zone Settings")]
        [SerializeField]
        private ZoneType _zoneType = ZoneType.Outer;

        [SerializeField, Tooltip("Which side is this spike bed on? Player 1 targets Right, Player 2 targets Left.")]
        private TargetSide _targetSide = TargetSide.Right;

        [Title("Impale Position")]
        [SerializeField, Tooltip("Reference to the Spike_Base child. Head will move to this position when impaled.")]
        private Transform _baseTransform;

        [SerializeField, Tooltip("Vertical offset from base position for first head placement")]
        private float _impaleOffset = 0.1f;

        [SerializeField, Tooltip("Vertical spacing between stacked heads")]
        private float _headStackSpacing = 0.8f;

        [SerializeField, Tooltip("Maximum number of heads this spike can hold")]
        private int _maxHeadCapacity = 4;

        [Title("Debug")]
        [SerializeField, ReadOnly]
        private int _pointValue;

        [SerializeField, ReadOnly]
        private List<HeadController> _impaledHeads = new List<HeadController>();

        private Collider2D _tipCollider;

        public int PointValue => _pointValue;
        public ZoneType Type => _zoneType;
        public TargetSide Side => _targetSide;
        public int ImpaledHeadCount => _impaledHeads.Count;
        public int MaxCapacity => _maxHeadCapacity;

        /// <summary>
        /// Returns the LOCAL position where the next head should settle when impaled on this spike.
        /// Head should be parented to this spike tip first, then positioned using this local position.
        /// </summary>
        public Vector3 GetLocalImpalePosition()
        {
            if (_baseTransform == null)
            {
                // Fallback if no base assigned
                return Vector3.zero;
            }

            // Get base's local Y position relative to this tip
            float baseLocalY = _baseTransform.localPosition.y + _impaleOffset;

            // Stack additional heads on top (moving UP from base, so ADD spacing)
            float localY = baseLocalY + (_headStackSpacing * _impaledHeads.Count);

            // X and Z are 0 (centered on spike tip)
            return new Vector3(0f, localY, 0f);
        }

        /// <summary>
        /// Returns this spike's transform for parenting heads.
        /// </summary>
        public Transform GetSpikeTransform()
        {
            return transform;
        }

        /// <summary>
        /// Returns true if this spike has room for another head.
        /// </summary>
        public bool HasRoom()
        {
            return _impaledHeads.Count < _maxHeadCapacity;
        }

        /// <summary>
        /// Register a head as impaled on this spike.
        /// Call this after the head has been positioned.
        /// Disables tip collider when spike is full.
        /// </summary>
        public void RegisterImpaledHead(HeadController head)
        {
            if (head != null && !_impaledHeads.Contains(head))
            {
                _impaledHeads.Add(head);
                Debug.Log($"SpikeZone '{name}': Registered head. Total: {_impaledHeads.Count}/{_maxHeadCapacity}");

                // Disable tip trigger when full so heads bounce off the top head
                if (!HasRoom() && _tipCollider != null)
                {
                    _tipCollider.enabled = false;
                    Debug.Log($"SpikeZone '{name}': FULL - Tip collider disabled.");
                }
            }
        }

        /// <summary>
        /// Unregister a head from this spike (for round reset).
        /// </summary>
        public void UnregisterImpaledHead(HeadController head)
        {
            if (_impaledHeads.Remove(head))
            {
                Debug.Log($"SpikeZone '{name}': Unregistered head. Total: {_impaledHeads.Count}/{_maxHeadCapacity}");
            }
        }

        /// <summary>
        /// Clear all impaled heads (for round/match reset).
        /// Re-enables tip collider.
        /// </summary>
        public void ClearImpaledHeads()
        {
            _impaledHeads.Clear();

            // Re-enable tip collider
            if (_tipCollider != null)
            {
                _tipCollider.enabled = true;
            }

            Debug.Log($"SpikeZone '{name}': Cleared all heads. Tip collider re-enabled.");
        }

        /// <summary>
        /// Check if this zone is a valid target for the given player.
        /// Player 0 (left side) targets Right zones.
        /// Player 1 (right side) targets Left zones.
        /// </summary>
        public bool IsValidTargetForPlayer(int playerIndex)
        {
            // Player 0 throws from left, targets right spike bed
            // Player 1 throws from right, targets left spike bed
            if (playerIndex == 0)
                return _targetSide == TargetSide.Right;
            else
                return _targetSide == TargetSide.Left;
        }

        private void OnValidate()
        {
            _pointValue = (int)_zoneType;
        }

        private void Awake()
        {
            _pointValue = (int)_zoneType;
            _tipCollider = GetComponent<Collider2D>();

            // Ensure this object is on the Spikes layer
            if (gameObject.layer != LayerMask.NameToLayer("Spikes"))
            {
                Debug.LogWarning($"SpikeZone '{gameObject.name}' should be on the 'Spikes' layer.");
            }
        }
    }
}
