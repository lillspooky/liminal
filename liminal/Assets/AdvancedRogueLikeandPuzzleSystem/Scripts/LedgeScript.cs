using UnityEngine;
using System.Collections.Generic;

namespace AdvancedRogueLikeandPuzzleSystem
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class OneWayPlatform3D : MonoBehaviour
    {
        [Tooltip("Extra vertical margin so the platform only becomes solid when the player's feet are clearly above the top.")]
        public float verticalBuffer = 0.1f;

        [Tooltip("Thickness (in world units) of the top collider.")]
        public float topColliderThickness = 0.2f;

        private MeshRenderer meshRenderer;
        private MeshCollider mainMeshCollider;

        // The box collider that acts as the "top" surface.
        private BoxCollider topCollider;
        // The trigger that covers the entire box volume to detect the player's position.
        private BoxCollider detectionTrigger;

        // We store player colliders that are currently inside the detectionTrigger
        // so we can continually toggle collisions as they move.
        private readonly HashSet<Collider> collidersInside = new HashSet<Collider>();

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            mainMeshCollider = GetComponent<MeshCollider>();
        }

        private void Start()
        {
            // 1) Disable or make the main mesh collider a trigger so the underside and sides don't collide.
            mainMeshCollider.convex = false; 
            mainMeshCollider.isTrigger = true; 
            // Or you could just disable mainMeshCollider.enabled = false; 
            // but we'll keep it as a trigger for the player to remain "inside" if you prefer.

            // 2) Create a child object for the top collider (the actual "floor").
            GameObject topObj = new GameObject("TopCollider");
            topObj.transform.SetParent(transform, false);
            topCollider = topObj.AddComponent<BoxCollider>();
            topCollider.isTrigger = false;  // It's a real collider to stand on.

            // Calculate size/center for topCollider from the mesh bounds.
            Bounds b = meshRenderer.bounds;
            float topY = b.max.y;
            Vector3 centerWorld = new Vector3(b.center.x, topY - (topColliderThickness * 0.5f), b.center.z);
            Vector3 size = b.size;
            // We'll cover the full XZ, but only a small thickness in Y.
            topCollider.size = new Vector3(size.x, topColliderThickness, size.z);
            // Convert center from world to local space:
            topCollider.center = transform.InverseTransformPoint(centerWorld);

            // 3) Create a child object for the detection trigger
            GameObject detectObj = new GameObject("DetectionTrigger");
            detectObj.transform.SetParent(transform, false);
            detectionTrigger = detectObj.AddComponent<BoxCollider>();
            detectionTrigger.isTrigger = true;

            // The detection trigger covers the entire bounding box plus some margin in Y so 
            // the player is considered "inside" even if they jump above it a bit.
            // Alternatively, you can just exactly match the bounding box, but let's add some overhead:
            float extraHeight = 2f; 
            detectionTrigger.size = new Vector3(size.x, size.y + extraHeight, size.z);
            // Its center is just the bounding box center
            detectionTrigger.center = transform.InverseTransformPoint(b.center);

            // Initially, we want collisions with the top collider to be ignored 
            // unless the player is actually above it.
            // We'll handle toggling in OnTriggerStay.
        }

        private void OnTriggerEnter(Collider other)
        {
            // If the main mesh collider is a trigger, OnTriggerEnter will be called.
            // Only track the player(s).
            if (!other.CompareTag("Player"))
                return;

            // Add to our set of colliders inside
            collidersInside.Add(other);
        }

        private void OnTriggerStay(Collider other)
        {
            // Only proceed for player
            if (!other.CompareTag("Player"))
                return;

            if (!collidersInside.Contains(other))
                collidersInside.Add(other);

            ToggleCollisionIfNeeded(other);
        }

        private void OnTriggerExit(Collider other)
        {
            // If a player leaves the main trigger, remove them from the set 
            // and force ignoring collisions (so they can pass back under).
            if (!other.CompareTag("Player"))
                return;

            if (collidersInside.Contains(other))
                collidersInside.Remove(other);

            // Ignore collisions again, so from below or the side, they're not blocked.
            Physics.IgnoreCollision(topCollider, other, true);
        }

        /// <summary>
        /// Called each frame to see if we should ignore collisions or not, 
        /// based on the player's bounding box bottom vs. the top face.
        /// </summary>
        private void ToggleCollisionIfNeeded(Collider playerColl)
        {
            // Get the top of this mesh
            float ledgeTop = meshRenderer.bounds.max.y;
            // Player's bottom
            float playerBottom = playerColl.bounds.min.y;

            // If the player's bottom is clearly above the top minus a small buffer,
            // we ALLOW collisions (stop ignoring).
            if (playerBottom > (ledgeTop - verticalBuffer))
            {
                // The player is above or nearly above -> let them land
                Physics.IgnoreCollision(topCollider, playerColl, false);
            }
            else
            {
                // The player is below -> ignore collisions so they can pass from under or sides
                Physics.IgnoreCollision(topCollider, playerColl, true);
            }
        }
    }
}
