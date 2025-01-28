using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    [RequireComponent(typeof(Collider))]
    public class TunnelScript : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            ThirdPersonController controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.SetInTunnel(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            ThirdPersonController controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.SetInTunnel(false);
            }
        }
    }
}

