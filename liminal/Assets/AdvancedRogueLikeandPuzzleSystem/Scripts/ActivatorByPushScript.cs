using UnityEngine;
using UnityEngine.Events;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class ActivatorByPushScript : MonoBehaviour
    {
        public bool isInteractable = false;
        private bool isDoneCorrectly = false;
        public UnityEvent eventstoInvoke;

        public void Activate()
        {
            Material[] mymat = transform.GetComponent<Renderer>().materials;
            mymat[1].SetColor("_EmissionColor", Color.green * 5);
            transform.GetComponent<Renderer>().materials = mymat;
            isInteractable = true;
        }

        public void Kapan()
        {
            Material[] mymat = transform.GetComponent<Renderer>().materials;
            mymat[1].SetColor("_EmissionColor", Color.red * 5);
            transform.GetComponent<Renderer>().materials = mymat;
            isDoneCorrectly = false;
        }

        public void Open()
        {
            if (eventstoInvoke != null)
            {
                eventstoInvoke.Invoke();
                eventstoInvoke = null;
            }
            AudioManager.instance.Play_ButtonPressing();
        }



        private void OnTriggerEnter(Collider other)
        {
            if (!isDoneCorrectly)
            {
                if (isInteractable && other.CompareTag("Player"))
                {
                    Open();
                    this.enabled = false;
                }
            }
        }
    }
}