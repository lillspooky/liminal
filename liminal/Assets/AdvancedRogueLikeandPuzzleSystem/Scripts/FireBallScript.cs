using UnityEngine;
using System.Linq;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class FireBallScript : MonoBehaviour
    {
        public GameObject explosionPrefab;
        public int damage;
        private bool crashed = false;
        public Rigidbody rigidBody;
        public Collider cold;
        public float damageRange;
        public LayerMask collisions;
        public LayerMask damageLayerMask;
        public ParticleSystem trailerInside;

        void Start()
        {
            Destroy(gameObject, 10);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Contains(collision.gameObject.layer))
            {
                if (!crashed)
                {
                    GameObject explosion;
                    explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                    Destroy(gameObject, 5f);
                    crashed = true;
                    Collider[] colliders = Physics.OverlapSphere(transform.position, damageRange, damageLayerMask);
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        colliders[i].SendMessage("GetDamage", damage);
                    }

                    colliders = Physics.OverlapSphere(transform.position, 5, collisions).Where(x => x.CompareTag("Collapsable")).ToArray();
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        if (colliders[i].GetComponent<Rigidbody>() != null)
                        {
                            colliders[i].GetComponent<Rigidbody>().isKinematic = false;
                            colliders[i].GetComponent<Rigidbody>().AddExplosionForce(500, new Vector3(transform.position.x, transform.position.y - 3, transform.position.z), 50);
                        }
                    }

                    Destroy(rigidBody);
                    GetComponent<MeshRenderer>().enabled = false;
                    Destroy(cold);
                    trailerInside.Stop();
                }
            }
        }

        public bool Contains(int layer)
        {
            return collisions == (collisions | (1 << layer));
        }
    }
}