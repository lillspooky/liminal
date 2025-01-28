using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class WeaponEffectScript : MonoBehaviour
    {
        public List<GameObject> list = new List<GameObject>();
        public static WeaponEffectScript Instance;

        private void Awake()
        {
            Instance = this;    
        }

        public void Attack()
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                HeroController.instance.WeaponAttack.GetComponent<WeaponScript>().MakeDamage(list[i]);
            }
            if(list.Count > 0)
            {
                AdvancedTPSCamera.Instance.fCamShakeImpulse = 0.5f;
            }
        }

        IEnumerator TimeScaleEffect()
        {
            Time.timeScale = 0.3f;
            yield return new WaitForSeconds(0.15f);
            Time.timeScale = 1;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Soldier"))
            {
                if(!list.Contains(other.gameObject))
                {
                    list.Add(other.gameObject);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Soldier"))
            {
                if (list.Contains(other.gameObject))
                {
                    list.Remove(other.gameObject);
                }
            }
        }
    }
}