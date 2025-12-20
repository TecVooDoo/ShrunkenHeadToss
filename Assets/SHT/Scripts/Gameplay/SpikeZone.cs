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

        [Title("Zone Settings")]
        [SerializeField]
        private ZoneType _zoneType = ZoneType.Outer;

        [SerializeField, ReadOnly]
        private int _pointValue;

        public int PointValue => _pointValue;
        public ZoneType Type => _zoneType;

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
