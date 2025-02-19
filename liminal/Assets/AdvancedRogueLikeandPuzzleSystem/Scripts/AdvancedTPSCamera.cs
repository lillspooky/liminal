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
        // Fixed distance from the tracking target (no smooth zooming).
        public float distance = 10.0f;
        public float collisionOffset = 0.3f;
        public float minDistance = 8.0f;
        public float maxDistance = 15.0f;
        public float height = 1.5f;

        [Header("Rotation Options")]
        public Vector3 cameraRotationEuler = new Vector3(10f, 180f, 0f);

        [Header("Camera Movement")]
        public float MoveDamping = 0.1f;
        public float fCamShakeImpulse = 0.0f;

        [Header("Dead Zone (Player) Settings")]
        [Tooltip("Normalized viewport coordinates for the dead zone's bottom-left corner.")]
        public Vector2 playerDeadZoneMin = new Vector2(0.35f, 0.35f);
        [Tooltip("Normalized viewport coordinates for the dead zone's top-right corner.")]
        public Vector2 playerDeadZoneMax = new Vector2(0.65f, 0.65f);
        [Tooltip("How quickly the camera tracking target adjusts when the player leaves the dead zone.")]
        public float trackingSmooth = 5f;

        [Header("Mouse Panning Settings")]
        [Tooltip("Normalized viewport coordinates for the mouse pan zone's bottom-left corner.")]
        public Vector2 mousePanZoneMin = new Vector2(0.05f, 0.05f);
        [Tooltip("Normalized viewport coordinates for the mouse pan zone's top-right corner.")]
        public Vector2 mousePanZoneMax = new Vector2(0.95f, 0.95f);
        [Tooltip("Camera pan speed when the mouse is near the edge.")]
        public float mousePanSpeed = 5f;

        [Header("Ground Floor Constraint")]
        [Tooltip("The minimum y value for the camera position. The camera will not go below this level.")]
        public float groundFloorY = 0f;

        public static AdvancedTPSCamera Instance;

        private Transform camTransform;
        private Vector3 newPosition;

        // Internal tracking target used for dead zone and mouse panning adjustments.
        private Vector3 trackingTarget;
        // The z value of the tracking target is fixed.
        private float fixedTrackingZ;

        // Cached Camera component (used for viewport conversions).
        private Camera cam;

        // --------------------------------------------------
        // Camera state machine definitions
        // --------------------------------------------------
        private enum CameraState
        {
            Normal,
            TransitioningToOverride,
            Overridden,
            TransitioningToNormal
        }
        private CameraState cameraState = CameraState.Normal;

        // "Normal" parameters (the baseline for user input).
        private float normalDistance;
        private float normalHeight;
        private Vector3 normalRotationEuler;

        // The override parameters.
        private float overrideDistance;
        private float overrideHeight;
        private Quaternion overrideRotation;

        // Transition fields.
        private float transitionStartTime;
        private float transitionDuration;
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

            // Initialize "normal" parameters.
            normalDistance = distance;
            normalHeight = height;
            normalRotationEuler = cameraRotationEuler;

            // Get the Camera component (or fallback to Camera.main).
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }
            if (cam == null)
            {
                Debug.LogError("AdvancedTPSCamera: No Camera component found!");
                enabled = false;
                return;
            }

            // Set the tracking target to the current pivot.
            Vector3 pivotPos = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            trackingTarget = pivotPos;
            // Record the fixed z value.
            fixedTrackingZ = trackingTarget.z;
            ClampTrackingTargetXZ();

            // Snap the camera immediately.
            ForceImmediateCameraPosition();
        }

        private void LateUpdate()
        {
            if (!viewTarget) return;

            switch (cameraState)
            {
                case CameraState.Normal:
                    DoNormalUpdate();
                    break;
                case CameraState.TransitioningToOverride:
                    DoTransitionUpdate(true);
                    break;
                case CameraState.Overridden:
                    DoOverriddenUpdate();
                    break;
                case CameraState.TransitioningToNormal:
                    DoTransitionUpdate(false);
                    break;
            }
        }

        #region Immediate Position Snap

        /// <summary>
        /// Immediately places the camera in the desired position at scene start.
        /// </summary>
        private void ForceImmediateCameraPosition()
        {
            if (!viewTarget) return;

            Quaternion initRot = Quaternion.Euler(cameraRotationEuler);
            Vector3 desiredPos = trackingTarget + initRot * (Vector3.back * distance) + Vector3.up * height;
            desiredPos = CheckCollisions(trackingTarget, desiredPos);
            desiredPos.y = Mathf.Max(desiredPos.y, groundFloorY);

            camTransform.position = desiredPos;
            camTransform.rotation = initRot;
        }

        #endregion

        #region Camera State Updates

        /// <summary>
        /// Normal state: update the tracking target (via dead zone and mouse panning),
        /// then compute and apply the camera position relative to the tracking target.
        /// </summary>
        private void DoNormalUpdate()
        {
            // (Zoom input has been removed so that the distance remains fixed.)

            // Update tracking target using player dead zone logic.
            UpdateTrackingTarget();
            // Apply mouse panning adjustments.
            ApplyMousePanning();

            // Compute the camera position from the tracking target.
            Quaternion currentRot = Quaternion.Euler(cameraRotationEuler);
            newPosition = trackingTarget + currentRot * (Vector3.back * distance) + Vector3.up * height;
            newPosition = CheckCollisions(trackingTarget, newPosition);
            newPosition.y = Mathf.Max(newPosition.y, groundFloorY);

            camTransform.position = Vector3.Lerp(camTransform.position, newPosition, MoveDamping);
            camTransform.rotation = currentRot;

            if (fCamShakeImpulse > 0f)
                shakeCamera();

            // Update the normal parameters.
            normalDistance = distance;
            normalHeight = height;
            normalRotationEuler = cameraRotationEuler;
        }

        /// <summary>
        /// Overridden state: camera is pinned to override parameters.
        /// </summary>
        private void DoOverriddenUpdate()
        {
            Vector3 pivotPos = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            Quaternion rot = overrideRotation;
            newPosition = pivotPos + rot * (Vector3.back * overrideDistance) + Vector3.up * overrideHeight;
            newPosition = CheckCollisions(pivotPos, newPosition);
            newPosition.y = Mathf.Max(newPosition.y, groundFloorY);

            camTransform.position = Vector3.Lerp(camTransform.position, newPosition, MoveDamping);
            camTransform.rotation = rot;

            if (fCamShakeImpulse > 0f)
                shakeCamera();
        }

        /// <summary>
        /// Handles transitions to/from override states.
        /// </summary>
        /// <param name="toOverride">True if transitioning to override, false if reverting to normal.</param>
        private void DoTransitionUpdate(bool toOverride)
        {
            float elapsed = Time.time - transitionStartTime;
            float t = (transitionDuration > 0f) ? Mathf.Clamp01(elapsed / transitionDuration) : 1f;

            float curDist = Mathf.Lerp(startDistance, (toOverride ? overrideDistance : normalDistance), t);
            float curHeight = Mathf.Lerp(startHeight, (toOverride ? overrideHeight : normalHeight), t);
            Quaternion curRot = Quaternion.Slerp(startRotation, (toOverride ? overrideRotation : Quaternion.Euler(normalRotationEuler)), t);

            Vector3 pivotPos = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            Vector3 desiredPos = pivotPos + curRot * (Vector3.back * curDist) + Vector3.up * curHeight;
            desiredPos = CheckCollisions(pivotPos, desiredPos);
            desiredPos.y = Mathf.Max(desiredPos.y, groundFloorY);

            camTransform.position = Vector3.Lerp(camTransform.position, desiredPos, MoveDamping);
            camTransform.rotation = curRot;

            if (fCamShakeImpulse > 0f)
                shakeCamera();

            if (t >= 1f)
            {
                cameraState = toOverride ? CameraState.Overridden : CameraState.Normal;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Starts a smooth transition from the current state to an override camera state.
        /// </summary>
        public void StartTransitionToOverride(float newDist, float newHeight, Vector3 newRotEuler, float duration)
        {
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
        /// Starts a smooth transition reverting the camera back to normal (player-driven) state.
        /// </summary>
        public void StartRevertToNormal(float duration)
        {
            if (cameraState == CameraState.Normal) return;

            startDistance = GetCurrentDistance();
            startHeight = GetCurrentHeight();
            startRotation = camTransform.rotation;

            transitionStartTime = Time.time;
            transitionDuration = duration;

            cameraState = CameraState.TransitioningToNormal;
        }

        #endregion

        #region Helpers

        private float GetCurrentDistance()
        {
            Vector3 pivot = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            return Vector3.Distance(pivot, camTransform.position);
        }

        private float GetCurrentHeight()
        {
            Vector3 pivot = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            return (camTransform.position.y - pivot.y);
        }

        /// <summary>
        /// Checks for collisions along the path between the pivot and the desired camera position.
        /// </summary>
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

        /// <summary>
        /// Updates the internal tracking target based on the player's position relative to the dead zone.
        /// Only the x and y coordinates are adjusted.
        /// </summary>
        private void UpdateTrackingTarget()
        {
            Vector3 pivotPos = (playerCameraRoot != null) ? playerCameraRoot.position : viewTarget.position;
            // Convert the player's world position to viewport coordinates.
            Vector3 playerViewportPos = cam.WorldToViewportPoint(pivotPos);

            // Clamp the viewport position to remain inside the dead zone.
            float clampedX = Mathf.Clamp(playerViewportPos.x, playerDeadZoneMin.x, playerDeadZoneMax.x);
            float clampedY = Mathf.Clamp(playerViewportPos.y, playerDeadZoneMin.y, playerDeadZoneMax.y);
            Vector3 clampedViewportPos = new Vector3(clampedX, clampedY, playerViewportPos.z);

            // Convert the clamped viewport position back to world space.
            Vector3 worldPosAtDeadZone = cam.ViewportToWorldPoint(clampedViewportPos);

            // Determine the offset required to keep the player within the dead zone.
            Vector3 offsetWorld = pivotPos - worldPosAtDeadZone;
            Vector3 desiredTrackingTarget = trackingTarget + offsetWorld;

            // Smoothly update the tracking target.
            trackingTarget = Vector3.Lerp(trackingTarget, desiredTrackingTarget, trackingSmooth * Time.deltaTime);
            ClampTrackingTargetXZ();
        }

        /// <summary>
        /// Nudges the tracking target if the mouse cursor is near the edge of the screen.
        /// Only x and y adjustments are applied.
        /// </summary>
        private void ApplyMousePanning()
        {
            Vector3 mousePos = Input.mousePosition;
            Vector2 mouseViewport = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);
            Vector3 panOffset = Vector3.zero;

            if (mouseViewport.x < mousePanZoneMin.x)
            {
                panOffset += -camTransform.right * mousePanSpeed * Time.deltaTime;
            }
            else if (mouseViewport.x > mousePanZoneMax.x)
            {
                panOffset += camTransform.right * mousePanSpeed * Time.deltaTime;
            }

            if (mouseViewport.y < mousePanZoneMin.y)
            {
                panOffset += -camTransform.up * mousePanSpeed * Time.deltaTime;
            }
            else if (mouseViewport.y > mousePanZoneMax.y)
            {
                panOffset += camTransform.up * mousePanSpeed * Time.deltaTime;
            }

            trackingTarget += panOffset;
            ClampTrackingTargetXZ();
        }

        /// <summary>
        /// Clamps the tracking target's y value so it doesn't drop below the ground floor,
        /// and fixes the z value to the initial setting.
        /// </summary>
        private void ClampTrackingTargetXZ()
        {
            if (trackingTarget.y < groundFloorY)
                trackingTarget.y = groundFloorY;
            trackingTarget.z = fixedTrackingZ;
        }

        #endregion
    }
}
