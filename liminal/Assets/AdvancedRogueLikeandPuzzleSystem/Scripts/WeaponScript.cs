using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class WeaponScript : MonoBehaviour
    {
        public int ID;
        public Collider colliderGrab;
        public string Name;
        public Collider colliderHit;
        public WeaponType weaponType;
        public bool isTaken = false;
        public TrailRenderer trailRenderer;
        public int DamagePower = 20;

        public Vector3 direction;
        public GameObject[] bloodParticles;

        public void MakeDamage(GameObject obje)
        {
            if (obje.CompareTag("Soldier"))
            {
                if (obje.GetComponent<SoldierController>() != null && obje.GetComponent<SoldierController>().health > 0)
                {
                    obje.GetComponent<SoldierController>().GetDamage(DamagePower);
                    Instantiate(bloodParticles[Random.Range(0, bloodParticles.Length)], new Vector3(obje.transform.position.x, obje.transform.position.y + 1, obje.transform.position.z), Quaternion.identity);
                    Vector3 targetPosition = obje.transform.position - obje.transform.forward/6f;
                    obje.transform.position = targetPosition;
                }
                else if (obje.GetComponent<ArcherController>() != null && obje.GetComponent<ArcherController>().health > 0)
                {
                    obje.GetComponent<ArcherController>().GetDamage(DamagePower);
                    Instantiate(bloodParticles[Random.Range(0, bloodParticles.Length)], new Vector3(obje.transform.position.x, obje.transform.position.y + 1, obje.transform.position.z), Quaternion.identity);
                    Vector3 targetPosition = obje.transform.position - obje.transform.forward / 6;
                    obje.transform.position = targetPosition;
                }
                AudioManager.instance.Play_WeaponHit();
            }
        }
    }

    public enum WeaponType
    {
        Shield,
        Sword,
        Tourch,
        Axe
    }
}