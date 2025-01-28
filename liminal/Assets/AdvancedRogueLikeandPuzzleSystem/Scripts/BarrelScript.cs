using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class BarrelScript : MonoBehaviour
    {
        public int Damage;
        bool isExploded = false;
        public ParticleSystem funyeParticle;
        public GameObject explosionParticle;
        public int ExplosionEffectRange = 10;
        IEnumerator Explode()
        {
            if (!isExploded)
            {
                isExploded = true;
                funyeParticle.Play();
                GetComponent<AudioSource>().Play();
                yield return new WaitForSeconds(Random.Range(2.5f, 3.5f));
                Instantiate(explosionParticle, transform.position, new Quaternion(0, 0, 0, 0));
                if (Vector3.Distance(transform.position, HeroController.instance.transform.position) <= ExplosionEffectRange)
                {
                    HeroController.instance.GetDamagePlayer(Damage);
                }
                Rigidbody rigidbody = GetComponent<Rigidbody>();
                rigidbody.useGravity = true;
                rigidbody.constraints = RigidbodyConstraints.None;
                AdvancedTPSCamera.Instance.fCamShakeImpulse = 0.5f;
                funyeParticle.Stop();
                GetComponent<AudioSource>().Stop();
                Vector3 firlatmaYonu = (new Vector3(Camera.main.transform.position.x + Random.Range(-3, 3), Camera.main.transform.position.y + Random.Range(-1, 3), Camera.main.transform.position.z + Random.Range(-3, 3)) - transform.position).normalized;
                rigidbody.AddForce(firlatmaYonu * 15, ForceMode.Impulse);
                rigidbody.AddTorque(Vector3.right * 5, ForceMode.Impulse);
                Destroy(gameObject, 3f);


                List<GameObject> transforms = Physics.OverlapSphere(transform.position, ExplosionEffectRange).Where(x => x.CompareTag("DoorItem")).Select(x => x.gameObject).ToList();
                foreach (var item in transforms)
                {
                    Rigidbody rigidbody1 = item.GetComponent<Rigidbody>();
                    if (rigidbody1 != null && !rigidbody1.useGravity)
                    {
                        rigidbody1.useGravity = true;
                        if (rigidbody1.GetComponentInParent<DoorScript>() != null)
                        {
                            rigidbody1.GetComponentInParent<DoorScript>().Explode();
                        }
                        rigidbody1.isKinematic = false;
                        rigidbody1.constraints = RigidbodyConstraints.None;
                        // Simdi de fırlatalım bir yere dogru!
                        rigidbody1.AddExplosionForce(Random.Range(5, 20), rigidbody.transform.position, Random.Range(5, 15), Random.Range(2, 4), ForceMode.Impulse);
                        rigidbody1.AddRelativeTorque(new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), Random.Range(-5, 5)), ForceMode.Impulse);
                    }
                }

            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(Explode());
            }
        }
    }
}