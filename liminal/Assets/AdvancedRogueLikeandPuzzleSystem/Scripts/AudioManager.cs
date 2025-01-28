using System.Collections;
using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        public AudioSource audioSource;
        public AudioClip audioClip_CoinUpdate;
        public AudioClip audioClip_ObjectiveCompleted;
        public AudioClip audioClip_ObjectiveNew;
        public AudioClip audioClip_WoodenDoor;
        public AudioClip audioClip_MetalDoor;
        public AudioClip audioClip_Door_Close;
        public AudioClip audioClip_Grab;
        public AudioClip audioClip_PutOnWeapon;
        public AudioClip audioClip_PutOffWeapon;
        public AudioClip[] audioClips_Hit;
        public AudioClip[] audioClips_Whoosh;
        public AudioSource audioSource_Swimming;
        public AudioClip[] audioClip_ShieldImpact;
        public AudioClip[] audioClip_Die;
        public AudioClip audioClip_Unlock;
        public AudioClip audioClip_ForceLockedDoor;
        public AudioClip audioClip_Potion;
        public AudioClip audioClip_Sprint;
        public AudioClip audioClip_knifeOut;
        public AudioClip audioClip_DoorExplosion;
        public AudioClip audioClip_Chest_Open;
        public AudioClip audioClip_Door_Open_With_Light;
        public AudioClip audioClip_Arm_Pulling;
        public AudioClip audioClip_ButtonPressing;
        public AudioClip[] audioClip_Footsteps;

        private void Awake()
        {
            instance = this;
        }

        public void Play_ButtonPressing()
        {
            audioSource.PlayOneShot(audioClip_ButtonPressing);
        }

        public void Play_Arm_Pulling()
        {
            audioSource.PlayOneShot(audioClip_Arm_Pulling);
        }

        public void Play_Door_Open_With_Light()
        {
            audioSource.PlayOneShot(audioClip_Door_Open_With_Light);
        }

        public void Play_Door_UnLock()
        {
            audioSource.PlayOneShot(audioClip_Unlock);
        }

        public void Play_Chest_Open()
        {
            audioSource.PlayOneShot(audioClip_Chest_Open);
        }

        public void Play_Door_Explosion()
        {
            audioSource.PlayOneShot(audioClip_DoorExplosion);
        }

        public void Play_Door_KnifeOut()
        {
            audioSource.PlayOneShot(audioClip_knifeOut);
        }

        public void Play_Sprint()
        {
            audioSource.PlayOneShot(audioClip_Sprint);
        }

        public void Play_Potion_Dring()
        {
            audioSource.PlayOneShot(audioClip_Potion);
        }

        public void Play_Door_TryOpen()
        {
            audioSource.PlayOneShot(audioClip_ForceLockedDoor);
        }

        public void Play_Door_Open(string material)
        {
            if (material == "Metal")
            {
                audioSource.PlayOneShot(audioClip_MetalDoor);
            }
            else if (material == "Wooden")
            {
                audioSource.PlayOneShot(audioClip_WoodenDoor);
            }
        }

        public void Play_Door_Close()
        {
            audioSource.PlayOneShot(audioClip_Door_Close);
        }

        public void Play_Unlocking()
        {
            audioSource.PlayOneShot(audioClip_Unlock);
        }

        public void Play_Coin()
        {
            audioSource.PlayOneShot(audioClip_CoinUpdate);
        }

        public void Play_ObjectiveCompleted()
        {
            audioSource.PlayOneShot(audioClip_ObjectiveCompleted);
        }

        public void Play_ObjectiveNew()
        {
            audioSource.PlayOneShot(audioClip_ObjectiveNew);
        }

        public void Play_ShieldImpact()
        {
            audioSource.PlayOneShot(audioClip_ShieldImpact[Random.Range(0, audioClip_ShieldImpact.Length)]);
        }

        public void Play_Die()
        {
            audioSource.PlayOneShot(audioClip_Die[Random.Range(0, audioClip_Die.Length)]);
        }


        public void Play_Grab()
        {
            audioSource.PlayOneShot(audioClip_Grab);
        }

        public void Play_PutOnWeapon()
        {
            audioSource.PlayOneShot(audioClip_PutOnWeapon);
        }

        public void Play_PutOffWeapon()
        {
            audioSource.PlayOneShot(audioClip_PutOffWeapon);
        }

        public void Play_WeaponHit()
        {
            audioSource.PlayOneShot(audioClips_Hit[Random.Range(0, audioClips_Hit.Length)], 0.5f);
        }

        public void Play_WeaponWhoosh()
        {
            StartCoroutine(Play_WeaponWhooshAsync());
        }

        IEnumerator Play_WeaponWhooshAsync()
        {
            yield return new WaitForSeconds(0.4f);
            audioSource.PlayOneShot(audioClips_Whoosh[Random.Range(0, audioClips_Whoosh.Length)], 0.5f);
        }

        public void Play_Swimming()
        {
            if (!audioSource_Swimming.isPlaying)
            {
                audioSource_Swimming.loop = true;
                audioSource_Swimming.Play();
            }
        }

        public void Stop_Swimming()
        {
            if (audioSource_Swimming.isPlaying)
            {
                audioSource_Swimming.loop = false;
            }
        }
    }
}