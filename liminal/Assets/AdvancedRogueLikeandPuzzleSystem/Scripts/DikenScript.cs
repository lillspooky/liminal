using System.Collections;
using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class DikenScript : MonoBehaviour
    {
        public int Damage;
        bool userTouched = false;

        void Start()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                userTouched = true;
                StartCoroutine(BicaklarCiksin());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                userTouched = false;
            }
        }


        IEnumerator BicaklarCiksin()
        {
            yield return new WaitForSeconds(0.5f);
            GetComponent<Animation>().Play();
            AudioManager.instance.Play_Door_KnifeOut();
            yield return new WaitForSeconds(0.5f);
            if (userTouched)
            {
                HeroController.instance.GetDamagePlayer(Damage);
            }
        }
    }
}