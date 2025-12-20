using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Heathen.UnityPhysics.API;

namespace SHT.Gameplay
{
    /// <summary>
    /// Controls the toss mechanic - handles drag input and launches heads.
    /// </summary>
    public class TossController : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required]
        private Transform _launchPoint;

        [SerializeField, Required]
        private LineRenderer _trajectoryLine;

        [SerializeField, Required]
        private GameObject _headPrefab;

        [Title("Toss Settings")]
        [SerializeField, MinValue(1f)]
        private float _minPower = 10f;

        [SerializeField, MinValue(1f)]
        private float _maxPower = 50f;

        [SerializeField, MinValue(0.1f)]
        private float _dragSensitivity = 10f;

        [SerializeField]
        private float _gravity = 20f;

        [Title("Trajectory Preview")]
        [SerializeField, MinValue(5)]
        private int _trajectoryResolution = 30;

        [SerializeField, MinValue(1f)]
        private float _trajectoryMaxDistance = 20f;

        [Title("Debug")]
        [SerializeField, ReadOnly]
        private bool _isAiming;

        [SerializeField, ReadOnly]
        private float _currentPower;

        [SerializeField, ReadOnly]
        private float _currentAngle;

        // Events
        public event Action OnTossStarted;
        public event Action<Vector2> OnTossReleased;
        public event Action OnTossCancelled;

        private ITossInput _input;
        private Vector2 _dragStartPos;
        private bool _inputEnabled;

        private void Awake()
        {
            // Get or add input component
            _input = GetComponent<ITossInput>();
            if (_input == null)
            {
                _input = gameObject.AddComponent<MouseTossInput>();
            }

            HideTrajectory();
        }

        private void OnEnable()
        {
            _input.OnDragStarted += HandleDragStarted;
            _input.OnDragUpdated += HandleDragUpdated;
            _input.OnDragEnded += HandleDragEnded;
            _input.OnDragCancelled += HandleDragCancelled;
        }

        private void OnDisable()
        {
            _input.OnDragStarted -= HandleDragStarted;
            _input.OnDragUpdated -= HandleDragUpdated;
            _input.OnDragEnded -= HandleDragEnded;
            _input.OnDragCancelled -= HandleDragCancelled;
        }

        /// <summary>
        /// Enable toss input for this controller.
        /// </summary>
        public void EnableInput()
        {
            _inputEnabled = true;
            _input.Enable();
        }

        /// <summary>
        /// Disable toss input for this controller.
        /// </summary>
        public void DisableInput()
        {
            _inputEnabled = false;
            _input.Disable();
            CancelAiming();
        }

        /// <summary>
        /// Set the launch point transform (player position).
        /// </summary>
        public void SetLaunchPoint(Transform launchPoint)
        {
            _launchPoint = launchPoint;
        }

        private void HandleDragStarted(Vector2 worldPos)
        {
            if (!_inputEnabled)
                return;

            _isAiming = true;
            _dragStartPos = worldPos;
            _currentPower = 0f;
            _currentAngle = 0f;

            OnTossStarted?.Invoke();
        }

        private void HandleDragUpdated(Vector2 worldPos)
        {
            if (!_isAiming)
                return;

            CalculateTossParameters(worldPos);
            UpdateTrajectoryPreview();
        }

        private void HandleDragEnded(Vector2 worldPos)
        {
            if (!_isAiming)
                return;

            CalculateTossParameters(worldPos);

            if (_currentPower > 0.1f)
            {
                LaunchHead();
            }

            CancelAiming();
        }

        private void HandleDragCancelled()
        {
            CancelAiming();
            OnTossCancelled?.Invoke();
        }

        private void CalculateTossParameters(Vector2 currentPos)
        {
            // Calculate drag vector (slingshot style - pull back to throw forward)
            Vector2 launchPos = _launchPoint.position;
            Vector2 pullVector = currentPos - launchPos; // Direction we're pulling
            Vector2 launchVector = -pullVector; // Launch goes opposite of pull

            // Power based on drag distance from launch point
            float dragMagnitude = pullVector.magnitude * _dragSensitivity;
            _currentPower = Mathf.Clamp(dragMagnitude, 0f, _maxPower);

            // Angle for launch direction (opposite of pull)
            if (launchVector.sqrMagnitude > 0.01f)
            {
                _currentAngle = Mathf.Atan2(launchVector.y, launchVector.x) * Mathf.Rad2Deg;
            }
        }

        private void UpdateTrajectoryPreview()
        {
            if (_trajectoryLine == null || _launchPoint == null)
                return;

            if (_currentPower < 0.1f)
            {
                HideTrajectory();
                return;
            }

            Vector2 launchPos = _launchPoint.position;
            Vector2 velocity = CalculateLaunchVelocity();

            // Generate trajectory points using fixed time step
            Vector3[] points = new Vector3[_trajectoryResolution];
            float totalTime = 2f; // seconds of trajectory to show
            float timeStep = totalTime / _trajectoryResolution;

            for (int i = 0; i < _trajectoryResolution; i++)
            {
                float t = i * timeStep;
                Vector2 point = CalculateTrajectoryPoint(launchPos, velocity, t);
                points[i] = new Vector3(point.x, point.y, 0f);

                // Stop if trajectory goes below ground level
                if (point.y < -5f)
                {
                    _trajectoryLine.positionCount = i + 1;
                    _trajectoryLine.SetPositions(points);
                    _trajectoryLine.enabled = true;
                    _trajectoryLine.useWorldSpace = true;
                    return;
                }
            }

            _trajectoryLine.positionCount = _trajectoryResolution;
            _trajectoryLine.SetPositions(points);
            _trajectoryLine.enabled = true;
            _trajectoryLine.useWorldSpace = true;
        }

        private Vector2 CalculateLaunchVelocity()
        {
            float angleRad = _currentAngle * Mathf.Deg2Rad;
            return new Vector2(
                Mathf.Cos(angleRad) * _currentPower,
                Mathf.Sin(angleRad) * _currentPower
            );
        }

        private Vector2 CalculateTrajectoryPoint(Vector2 start, Vector2 velocity, float time)
        {
            // Standard projectile motion: p = p0 + v*t + 0.5*a*t^2
            float x = start.x + velocity.x * time;
            float y = start.y + velocity.y * time - 0.5f * _gravity * time * time;
            return new Vector2(x, y);
        }

        private void LaunchHead()
        {
            if (_headPrefab == null || _launchPoint == null)
            {
                Debug.LogWarning("TossController: Missing headPrefab or launchPoint reference.");
                return;
            }

            Vector2 launchPos = _launchPoint.position;
            Vector2 velocity = CalculateLaunchVelocity();

            // Spawn head
            GameObject headObj = Instantiate(_headPrefab, launchPos, Quaternion.identity);

            // Apply velocity via HeadController or Rigidbody2D
            var headController = headObj.GetComponent<HeadController>();
            if (headController != null)
            {
                headController.Launch(velocity);
            }
            else
            {
                var rb = headObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = velocity;
                }
            }

            OnTossReleased?.Invoke(velocity);
        }

        private void CancelAiming()
        {
            _isAiming = false;
            _currentPower = 0f;
            _currentAngle = 0f;
            HideTrajectory();
        }

        private void HideTrajectory()
        {
            if (_trajectoryLine != null)
            {
                _trajectoryLine.enabled = false;
            }
        }
    }
}
