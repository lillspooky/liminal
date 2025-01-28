using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class TestereScript : MonoBehaviour
    {
        public int Damage;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                HeroController.instance.GetDamagePlayer(Damage);
            }
        }
    }
}
