using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class JumpThroughTrigger : MonoBehaviour
    {
        [Tooltip("Z position to shift the player behind the ledge when a jump-through is successful.")]
        public float behindZDepth = -1f;

        [Tooltip("Extra vertical margin to ensure the player has fully passed the trigger.")]
        [SerializeField] private float verticalBuffer = 0.05f;

        private BoxCollider triggerCollider;

        private void Awake()
        {
            triggerCollider = GetComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            ThirdPersonController controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                // When entering, allow z-axis movement (simulate tunnel mode)
                controller.SetInTunnel(true);
                Debug.Log($"[JumpThroughTrigger] Player entered jump-through zone: {name}");
            }
        }

        private void OnTriggerStay(Collider other)
        {
            Debug.Log($"Trigger Stay: {other.gameObject.name}");
            
            // Check if 'other' is the player
            if (!other.CompareTag("Player")) return;

            // Get the player's ThirdPersonController
            ThirdPersonController playerController = other.GetComponent<ThirdPersonController>();
            if (playerController == null) return;

            // 1) Get the bounding box of the trigger
            Bounds triggerBounds = triggerCollider.bounds;

            // 2) Get the bounding box of the player
            Bounds playerBounds = other.bounds;

            // 3) Let's say Y is "vertical" in your game
            float triggerTop = triggerBounds.max.y;
            float playerBottom = playerBounds.min.y;

            // 4) Once the player's bottom passes fully above the trigger's top
            if (playerBottom > triggerTop + verticalBuffer)
            {
                // Move the player behind in Z (or whichever axis is your 'depth')
                var playerPosition = other.transform.position;
                playerPosition.z = behindZDepth;
                other.transform.position = playerPosition;
                playerController.SetInTunnel(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"Trigger Exit: {other.gameObject.name}");
            
            if (other.CompareTag("Player"))
            {
                ThirdPersonController playerController = other.GetComponent<ThirdPersonController>();
                if (playerController != null)
                {
                    playerController.SetInTunnel(false);
                    Debug.Log("Player exited trigger - Tunnel mode disabled");
                }
            }
        }
    }
}
