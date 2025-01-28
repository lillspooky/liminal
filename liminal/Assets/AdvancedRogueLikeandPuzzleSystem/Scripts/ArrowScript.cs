using System.Collections;
using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class ArrowScript : MonoBehaviour
    {
        private int Damage;
        private bool isHit = false;
        private bool Sent = false;
        public TrailRenderer trailRenderer;
        public Collider cold;

        public void Shooted(int DamageAmount)
        {
            Damage = DamageAmount;
            GetComponent<AudioSource>().Play();
            trailRenderer.enabled = true;
            Sent = true;
            StartCoroutine(ActivateCollider());
            Destroy(gameObject, 5);
        }

        IEnumerator ActivateCollider()
        {
            yield return new WaitForSeconds(0.1f);
            cold.isTrigger = false;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (Sent && !isHit)
            {
                if (collision.collider.CompareTag("Player"))
                {
                    isHit = true;
                    trailRenderer.enabled = false;
                    HeroController.instance.GetDamagePlayer(Damage);
                    Destroy(gameObject);
                }
            }
        }
    }
}