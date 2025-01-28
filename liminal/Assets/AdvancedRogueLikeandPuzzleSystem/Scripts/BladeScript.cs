using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class BladeScript : MonoBehaviour
    {
        public int Damage;
        public bool byTrigger = false;
        public bool alttakiCube = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (!byTrigger)
            {
                if (collision.collider.CompareTag("Player"))
                {
                    HeroController.instance.GetDamagePlayer(Damage);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (byTrigger)
            {
                if (other.CompareTag("Player"))
                {
                    if(!alttakiCube)
                    {
                        GetComponent<Collider>().isTrigger = false;
                    }
                    HeroController.instance.GetDamagePlayer(Damage);
                }
            }
        }
    }
}
