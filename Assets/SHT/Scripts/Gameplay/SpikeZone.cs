using UnityEngine;
using Sirenix.OdinInspector;

namespace SHT.Gameplay
{
    /// <summary>
    /// Defines a scoring zone on the spike bed.
    /// Attach to collider GameObjects that represent different scoring areas.
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

        [SerializeField, ReadOnly]
        private int _pointValue;

        public int PointValue => _pointValue;
        public ZoneType Type => _zoneType;
        public TargetSide Side => _targetSide;

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

            // Ensure this object is on the Spikes layer
            if (gameObject.layer != LayerMask.NameToLayer("Spikes"))
            {
                Debug.LogWarning($"SpikeZone '{gameObject.name}' should be on the 'Spikes' layer.");
            }
        }
    }
}
