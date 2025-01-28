using UnityEngine;
using AdvancedRogueLikeandPuzzleSystem; // Match the namespace of your camera script

[RequireComponent(typeof(BoxCollider))]
public class RoomVisibilityTrigger : MonoBehaviour
{
    [Header("Room Geometry to Toggle")]
    public GameObject[] geometryToHideInThisRoom;

    [Header("Camera Overrides")]
    [Tooltip("Enable camera override for this room.")]
    public bool useCameraOverride = false;

    [Tooltip("Distance to override when entering (e.g. 7 for close-up, 15 for wide).")]
    public float cameraDistance = 10f;

    [Tooltip("Vertical offset for the camera (e.g. 1.5).")]
    public float cameraHeight = 1.5f;

    [Tooltip("Euler angles for the camera when overridden (e.g., (10,180,0)).")]
    public Vector3 cameraRotationEuler = new Vector3(10f, 180f, 0f);

    [Header("Transition Durations")]
    [Tooltip("Seconds to smoothly transition to these camera values on enter.")]
    public float cameraEnterTime = 1.5f;

    [Tooltip("Seconds to smoothly revert to normal camera on exit.")]
    public float cameraExitTime = 1.5f;

    [Tooltip("If true, revert to normal camera settings when leaving this trigger.")]
    public bool revertCameraOnExit = true;

    // Tracks which room is currently active
    private static RoomVisibilityTrigger _currentRoom;

    private void Awake()
    {
        var col = GetComponent<BoxCollider>();
        if (col && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 1) Re-enable geometry of the previous room
        if (_currentRoom != null && _currentRoom != this)
        {
            _currentRoom.SetGeometryActive(true);

            // Smoothly revert the old room if it was overriding camera
            if (_currentRoom.revertCameraOnExit && AdvancedTPSCamera.Instance != null)
            {
                // Revert from old override to normal (using the old room's exit time)
                AdvancedTPSCamera.Instance.StartRevertToNormal(_currentRoom.cameraExitTime);
            }
        }

        // 2) Hide THIS room’s geometry
        SetGeometryActive(false);

        // 3) Apply camera override (smoothly transitions in)
        if (useCameraOverride && AdvancedTPSCamera.Instance != null)
        {
            AdvancedTPSCamera.Instance.StartTransitionToOverride(
                cameraDistance,
                cameraHeight,
                cameraRotationEuler,
                cameraEnterTime
            );
        }

        // 4) Mark this as the current room
        _currentRoom = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (_currentRoom == this)
        {
            // Re-enable geometry because we’re leaving
            SetGeometryActive(true);

            // Smoothly revert camera to normal
            if (revertCameraOnExit && AdvancedTPSCamera.Instance != null)
            {
                AdvancedTPSCamera.Instance.StartRevertToNormal(cameraExitTime);
            }

            _currentRoom = null;
        }
    }

    private void SetGeometryActive(bool state)
    {
        if (geometryToHideInThisRoom == null) return;
        foreach (var obj in geometryToHideInThisRoom)
        {
            if (obj != null) obj.SetActive(state);
        }
    }
}
