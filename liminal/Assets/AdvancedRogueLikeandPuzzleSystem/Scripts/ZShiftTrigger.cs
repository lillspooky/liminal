using UnityEngine;
using System.Collections;

namespace AdvancedRogueLikeandPuzzleSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class ZShiftTrigger : MonoBehaviour
    {
        [Header("Player & Controller")]
        [Tooltip("Reference to the player's ThirdPersonController for toggling tunnel mode.")]
        public ThirdPersonController playerController;

        [Header("Target Object")]
        [Tooltip("The object (plane/mesh) that will glow and whose Z-center the player will jump to.")]
        public Transform targetObject;

        [Header("Highlight Settings")]
        [Tooltip("If you use a standard material, set this to true to adjust its emission color.")]
        public bool useEmissionHighlight = true;
        [Tooltip("Color used for emission highlight.")]
        public Color highlightEmissionColor = Color.yellow;
        
        [Header("Z Shift Mechanics")]
        [Tooltip("How long (seconds) we stay in tunnel mode after the jump key is pressed.")]
        public float tunnelDuration = 1.0f;

        [Tooltip("Horizontal speed (units/sec) used to move from oldZ to targetZ over time.")]
        public float zShiftSpeed = 5f;

        [Tooltip("Additional upward velocity to apply for the jump.")]
        public float extraJumpVelocity = 5f;

        [Header("Keys")]
        [Tooltip("Key for 'jump' (space by default).")]
        public KeyCode jumpKey = KeyCode.Space;
        [Tooltip("Key for 'up' movement (W key or UpArrow).")]
        public KeyCode upKey = KeyCode.W;

        // Private references
        private BoxCollider triggerCollider;
        private Renderer targetRenderer;
        private Material[] originalMaterials; // store original materials if we want to restore them
        private Material[] glowMaterials;     // cloned materials with emission color

        private bool playerInside = false;
        private bool isShifting = false;

        private void Awake()
        {
            triggerCollider = GetComponent<BoxCollider>();
            if (!triggerCollider.isTrigger)
            {
                Debug.LogWarning($"[{name}] Forcing BoxCollider to be 'Is Trigger'");
                triggerCollider.isTrigger = true;
            }
            if (targetObject != null)
            {
                targetRenderer = targetObject.GetComponent<Renderer>();
            }
        }

        private void Start()
        {
            if (useEmissionHighlight && targetRenderer != null)
            {
                originalMaterials = targetRenderer.sharedMaterials;
                glowMaterials = new Material[originalMaterials.Length];
                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    Material matCopy = new Material(originalMaterials[i]);
                    matCopy.EnableKeyword("_EMISSION");
                    matCopy.SetColor("_EmissionColor", Color.black);
                    glowMaterials[i] = matCopy;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerInside = true;
            SetGlowEnabled(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerInside = false;
            SetGlowEnabled(false);
        }

        private void Update()
        {
            // If player is inside, check for jump+up input.
            if (playerInside && !isShifting)
            {
                if (Input.GetKeyDown(jumpKey) && Input.GetKey(upKey))
                {
                    StartCoroutine(PerformZShiftJump());
                }
            }
        }

        /// <summary>
        /// Executes the "special jump" by enabling tunnel mode, applying upward velocity
        /// and smoothly shifting the player's z position to the target's z center.
        /// </summary>
        private IEnumerator PerformZShiftJump()
        {
            if (!playerController || !playerInside || !targetObject)
                yield break;

            isShifting = true;

            // 1) Enable tunnel mode to allow z movement.
            playerController.SetInTunnel(true);

            // 2) Apply upward velocity if a Rigidbody exists.
            Rigidbody rb = playerController.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 vel = rb.linearVelocity;
                vel.y = extraJumpVelocity;
                rb.linearVelocity = vel;
            }
            // Otherwise, if no Rigidbody is available, skip applying upward velocity.

            // 3) Smoothly shift the player's z position over time.
            float elapsed = 0f;
            Vector3 startPos = playerController.transform.position;

            float targetZ = targetRenderer != null ? targetRenderer.bounds.center.z : targetObject.position.z;

            while (elapsed < tunnelDuration)
            {
                elapsed += Time.deltaTime;
                float newZ = Mathf.MoveTowards(playerController.transform.position.z, targetZ, zShiftSpeed * Time.deltaTime);
                Vector3 pos = playerController.transform.position;
                pos.z = newZ;
                playerController.transform.position = pos;
                yield return null;
            }

            // 4) After the duration, revert tunnel mode.
            playerController.SetInTunnel(false);
            isShifting = false;
        }

        /// <summary>
        /// Enables or disables the glow effect on the target object.
        /// </summary>
        private void SetGlowEnabled(bool enable)
        {
            if (!useEmissionHighlight || targetRenderer == null || glowMaterials == null)
                return;

            if (enable)
            {
                targetRenderer.materials = glowMaterials;
                for (int i = 0; i < glowMaterials.Length; i++)
                {
                    glowMaterials[i].SetColor("_EmissionColor", highlightEmissionColor);
                }
            }
            else
            {
                targetRenderer.materials = originalMaterials;
            }
        }
    }
}
