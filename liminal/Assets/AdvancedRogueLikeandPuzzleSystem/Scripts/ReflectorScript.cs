using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class ReflectorScript : MonoBehaviour
    {
        public GameObject reflectedLight;
        public LayerMask layers;
        public float raycastDistance = 20f;

        void Update()
        {
            if(reflectedLight.activeSelf)
            {
                Vector3 raycastDirection = reflectedLight.transform.forward * -1;

                Ray ray = new Ray(transform.position, raycastDirection);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, raycastDistance, layers))
                {
                    Transform targetObject = hit.transform;
                    float distance = hit.distance;

                    Vector3 scale = reflectedLight.transform.localScale;
                    scale.z = distance+ 0.1f;
                    reflectedLight.transform.localScale = scale;
                }
                else
                {
                    Vector3 scale = reflectedLight.transform.localScale;
                    scale.z = 100;
                    reflectedLight.transform.localScale = scale;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Light"))
            {
                reflectedLight.SetActive(true);
                AudioManager.instance.Play_Door_Open_With_Light();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Light"))
            {
                reflectedLight.SetActive(false);
            }
        }
    }
}
