using UnityEngine;

namespace SHT.Gameplay
{
    /// <summary>
    /// Temporary test script to enable TossController on Start.
    /// Remove once GameManager handles this.
    /// </summary>
    public class TossTestEnabler : MonoBehaviour
    {
        [SerializeField]
        private TossController _tossController;

        private void Start()
        {
            if (_tossController == null)
            {
                _tossController = GetComponent<TossController>();
            }

            if (_tossController != null)
            {
                _tossController.EnableInput();
                Debug.Log("TossController input enabled.");
            }
            else
            {
                Debug.LogError("TossTestEnabler: No TossController found!");
            }
        }
    }
}
