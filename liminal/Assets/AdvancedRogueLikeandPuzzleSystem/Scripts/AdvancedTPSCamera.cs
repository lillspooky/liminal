using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class AdvancedTPSCamera : MonoBehaviour
    {
        [Header("Camera Targets & Layers")]
        public Transform viewTarget;
        public Transform playerCameraRoot; 
        public LayerMask collisionLayers;

        [Header("Distance & Height Settings")]
        public float distance = 10.0f;
        public float distanceSpeed = 150.0f;
        public float collisionOffset = 0.3f;
        public float minDistance = 8.0f;
        public float maxDistance = 15.0f;
        public float height = 1.5f;

        [Header("Rotation Options")]
        public Vector3 cameraRotationEuler = new Vector3(10f, 180f, 0f);

        [Header("Misc")]
        public float MoveDamping = 0.1f;
        public float fCamShakeImpulse = 0.0f;

        public static AdvancedTPSCamera Instance;

        private Transform camTransform;
        private float smoothDistance;
        private Vector3 newPosition;

        // --------------------------------------------------
        // Simple camera state machine
        // --------------------------------------------------
        private enum CameraState
        {
            Normal,                 // normal user-driven camera
            TransitioningToOverride,
            Overridden,            // pinned at override
            TransitioningToNormal
        }

        private CameraState cameraState = CameraState.Normal;

        // "Normal" parameters (editor & runtime input). We store them 
        // so we can revert to these if we were overridden
        private float normalDistance;
        private float normalHeight;
        private Vector3 normalRotationEuler;

        // The override parameters
        private float overrideDistance;
        private float overrideHeight;
        private Quaternion overrideRotation;

        // Transition fields
        private float transitionStartTime;
        private float transitionDuration;

        // Start of transition state (distance, height, rotation)
        private float startDistance;
        private float startHeight;
        private Quaternion startRotation;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            camTransform = transform;
            smoothDistance = distance;

            // Initialize "normal" parameters
            normalDistance = distance;
            normalHeight = height;
            normalRotationEuler = cameraRotationEuler;
        }

        private void LateUpdate()
        {
            if (!viewTarget) return;

            // Evaluate the cameraState
            switch (cameraState)
            {
                case CameraState.Normal:
                    DoNormalUpdate();
                    break;
                case CameraState.TransitioningToOverride:
                    DoTransitionUpdate(toOverride: true);
                    break;
                case CameraState.Overridden:
                    DoOverriddenUpdate();
                    break;
                case CameraState.TransitioningToNormal:
                    DoTransitionUpdate(toOverride: false);
                    break;
            }
        }

        #region State Updates

        // Normal user-driven logic (mouse zoom, etc.)
        private void DoNormalUpdate()
        {
            // Zoom logic
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 2f,
                                   minDistance, maxDistance);
            smoothDistance = Mathf.Lerp(smoothDistance, distance, TimeSignature(distanceSpeed));

            // The pivot is either playerCameraRoot or viewTarget
            Vector3 pivotPos = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            Quaternion currentRot = Quaternion.Euler(cameraRotationEuler);

            // Calculate position
            newPosition = pivotPos
                          + currentRot * (Vector3.back * smoothDistance)
                          + Vector3.up * height;

            // Collision check
            newPosition = CheckCollisions(pivotPos, newPosition);

            // Move camera
            camTransform.position = Vector3.Lerp(camTransform.position, newPosition, MoveDamping);
            camTransform.rotation = currentRot;

            // Shake
            if (fCamShakeImpulse > 0f)
                shakeCamera();

            // Continuously update "normal" fields so if we override & revert, we come back to the *latest* normal
            normalDistance = distance;
            normalHeight = height;
            normalRotationEuler = cameraRotationEuler;
        }

        // Overridden: we stay pinned at the final override parameters (no user input).
        private void DoOverriddenUpdate()
        {
            // We remain locked in the override parameters
            Vector3 pivotPos = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;

            // Just place camera exactly at the override
            var rot = overrideRotation;
            newPosition = pivotPos
                          + rot * (Vector3.back * overrideDistance)
                          + Vector3.up * overrideHeight;

            newPosition = CheckCollisions(pivotPos, newPosition);

            camTransform.position = Vector3.Lerp(camTransform.position, newPosition, MoveDamping);
            camTransform.rotation = rot;

            if (fCamShakeImpulse > 0f) shakeCamera();
        }

        // Transition (lerp) either to override or back to normal
        private void DoTransitionUpdate(bool toOverride)
        {
            float elapsed = Time.time - transitionStartTime;
            float t = (transitionDuration > 0f) ? Mathf.Clamp01(elapsed / transitionDuration) : 1f;

            // Interpolate distance, height, rotation
            float curDist = Mathf.Lerp(startDistance, (toOverride ? overrideDistance : normalDistance), t);
            float curHeight = Mathf.Lerp(startHeight, (toOverride ? overrideHeight : normalHeight), t);
            Quaternion curRot = Quaternion.Slerp(startRotation, (toOverride ? overrideRotation : Quaternion.Euler(normalRotationEuler)), t);

            // Position
            Vector3 pivotPos = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            Vector3 desiredPos = pivotPos
                                 + curRot * (Vector3.back * curDist)
                                 + Vector3.up * curHeight;

            desiredPos = CheckCollisions(pivotPos, desiredPos);

            camTransform.position = Vector3.Lerp(camTransform.position, desiredPos, MoveDamping);
            camTransform.rotation = curRot;

            if (fCamShakeImpulse > 0f) shakeCamera();

            // If we've hit t = 1, transition is done
            if (t >= 1f)
            {
                if (toOverride)
                {
                    // Now pinned in override
                    cameraState = CameraState.Overridden;
                }
                else
                {
                    // Now fully normal
                    cameraState = CameraState.Normal;
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Start a smooth transition from the current camera state 
        /// to the given override (distance, height, rotation).
        /// </summary>
        public void StartTransitionToOverride(float newDist, float newHeight, Vector3 newRotEuler, float duration)
        {
            // If we are already "Overridden" or "TransitioningToOverride", we’ll reset the transition with new info
            // If we are Normal or TransitioningToNormal, we just begin override now

            // Capture the camera’s current "live" state as the start
            startDistance = GetCurrentDistance();
            startHeight = GetCurrentHeight();
            startRotation = camTransform.rotation;

            overrideDistance = newDist;
            overrideHeight = newHeight;
            overrideRotation = Quaternion.Euler(newRotEuler);

            transitionStartTime = Time.time;
            transitionDuration = duration;

            cameraState = CameraState.TransitioningToOverride;
        }

        /// <summary>
        /// Start a smooth revert from the current camera state back to normal player-controlled camera.
        /// </summary>
        public void StartRevertToNormal(float duration)
        {
            // If we are already normal, do nothing
            if (cameraState == CameraState.Normal) return;

            // The start of revert is the camera’s current live state
            startDistance = GetCurrentDistance();
            startHeight = GetCurrentHeight();
            startRotation = camTransform.rotation;

            transitionStartTime = Time.time;
            transitionDuration = duration;

            cameraState = CameraState.TransitioningToNormal;
        }

        #endregion

        #region Helpers

        // Actually measure how far the camera is from pivot right now (since we might be mid-lerp)
        private float GetCurrentDistance()
        {
            if (!viewTarget && !playerCameraRoot) return distance;
            Vector3 pivot = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            return Vector3.Distance(pivot, camTransform.position);
        }

        private float GetCurrentHeight()
        {
            if (!viewTarget && !playerCameraRoot) return height;
            Vector3 pivot = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            return (camTransform.position.y - pivot.y);
        }

        // Basic sphere cast for collisions
        private Vector3 CheckCollisions(Vector3 pivotPos, Vector3 desiredPos)
        {
            Vector3 dir = (desiredPos - pivotPos).normalized;
            float dist = Vector3.Distance(pivotPos, desiredPos);

            RaycastHit hit;
            if (Physics.SphereCast(pivotPos, 0.3f, dir, out hit, dist, collisionLayers))
            {
                return hit.point + (hit.normal * collisionOffset);
            }
            return desiredPos;
        }

        // Your smoothing function
        private float TimeSignature(float speed)
        {
            return 1.0f / (1.0f + 80.0f * Mathf.Exp(-speed * 0.02f));
        }

        public void shakeCamera()
        {
            camTransform.position += new Vector3(
                Random.Range(-fCamShakeImpulse, fCamShakeImpulse) / 4,
                Random.Range(-fCamShakeImpulse, fCamShakeImpulse) / 4,
                Random.Range(-fCamShakeImpulse, fCamShakeImpulse) / 4
            );

            fCamShakeImpulse -= Time.deltaTime * fCamShakeImpulse * 4.0f;
            if (fCamShakeImpulse < 0.01f)
            {
                fCamShakeImpulse = 0.0f;
            }
        }

        #endregion
    }
}
