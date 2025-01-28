using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class DoorScript : MonoBehaviour
    {
        public GameObject ObjectWillBeActivatedWhenDoorisOpened;
        public bool isLocked = false;
        public bool isOpened = false;
        public bool isCloseable = true;
        public int KeyID_ToOpen = 0;
        private Animation anim;
        public string doorType = "";
        public string doorMaterial = "";
        public NavMeshObstacle obstacle;
        public BoxCollider MainCollider;
        private bool openedByLight = false;

        private void Start()
        {
            anim = GetComponent<Animation>();
            if(!isLocked && MainCollider != null)
            {
                MainCollider.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Light"))
            {
                openedByLight = true;
                UnlockAndOpen();
            }
        }

        public void Explode()
        {
            gameObject.tag = "Untagged";
            if (MainCollider != null)
            {
                MainCollider.enabled = false;
            }
            AudioManager.instance.Play_Door_Explosion();
            Destroy(GetComponent<DoorScript>());
            HeroController.instance.HideInteractSprite();
            if (ObjectWillBeActivatedWhenDoorisOpened != null && !ObjectWillBeActivatedWhenDoorisOpened.activeSelf)
            {
                ObjectWillBeActivatedWhenDoorisOpened.SetActive(true);
            }
        }

        

        IEnumerator OpenTheDoor()
        {
            LastTimeTry = LastTimeTry - 1;
            yield return new WaitForSeconds(0.5f);
            TryToOpen();
        }

        public void UnlockAndOpen()
        {
            isLocked = false;
            TryToOpen();
        }
        

        private float LastTimeTry = 0;
        public void TryToOpen()
        {
            if (Time.time > LastTimeTry + 1)
            {
                LastTimeTry = Time.time;
                if (isLocked)
                {
                    if (HeroController.instance.Keys_Grabbed.Contains(KeyID_ToOpen))
                    {
                        isLocked = false;
                        AudioManager.instance.Play_Door_UnLock();
                        StartCoroutine(OpenTheDoor());
                    }
                    else
                    {
                        AudioManager.instance.Play_Door_TryOpen();
                        GameCanvas_Controller.instance.Show_Warning("LOCKED!");
                        anim.Play("TryToOpen_" + doorType);
                        return;
                    }
                }
                else
                {
                    if (isOpened == false)
                    {
                        isOpened = true;
                        if(obstacle != null)
                        {
                            obstacle.enabled = false;
                        }
                        anim.Play("Open_Door_" + doorType);
                        if(ObjectWillBeActivatedWhenDoorisOpened != null)
                        {
                            ObjectWillBeActivatedWhenDoorisOpened.SetActive(true);
                        }
                        if (!openedByLight) HeroController.instance.HideInteractSprite();
                        if (MainCollider != null)
                        {
                            MainCollider.enabled = false;
                        }
                        AudioManager.instance.Play_Door_Open(doorMaterial);
                        if(!isCloseable)
                        {
                            gameObject.tag = "Untagged";
                            Destroy(GetComponent<DoorScript>());
                        }
                    }
                    else
                    {
                        if(isCloseable)
                        {
                            isOpened = false;
                            if (obstacle != null)
                            {
                                obstacle.enabled = true;
                            }
                            HeroController.instance.HideInteractSprite();
                            AudioManager.instance.Play_Door_Close();
                            anim.Play("Close_Door_" + doorType);
                        }
                    }
                }
            }
        }
    }
}