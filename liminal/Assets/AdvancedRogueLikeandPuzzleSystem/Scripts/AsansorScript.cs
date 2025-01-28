using UnityEngine;
using System.Collections;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class AsansorScript : MonoBehaviour
    {
        public GameObject asansor;
        public Transform ustKat;
        public Transform altKat;
        public GameObject leftCark;
        public GameObject rightCark;
        public GameObject arm;
        public bool isInteractable = false;
        public bool isOntheWay = false;
        public bool isDown = true;
        public AudioSource audioSource;


        public void Activate()
        {
            if (!isOntheWay)
            {
                HeroController.instance.transform.parent = asansor.transform;
                HeroController.instance.thirdPersonController._animator.SetFloat(HeroController.instance.thirdPersonController._animIDSpeed, 0);
                HeroController.instance.characterController.enabled = false;
                HeroController.instance.thirdPersonController.enabled = false;
                arm.GetComponent<Animation>().Play();
                HeroController.instance.HideInteractSprite();
                AudioManager.instance.Play_Arm_Pulling();
                StartCoroutine(StartToLiftJourney());
                if (isDown)
                {
                    isDown = false;
                    StartCoroutine(StartAsansorSound());
                }
                else
                {
                    isDown = true;
                    StartCoroutine(StartAsansorSound());
                }
            }
        }

        void Update()
        {
            if (isInteractable)
            {
                if (GameManager.Instance.controllerType == ControllerType.KeyboardMouse)
                {
                    if ((Input.GetKeyUp(GameManager.Instance.Keycode_Interact) || Input.GetButtonUp("Interact")) && !isOntheWay)
                    {
                        HeroController.instance.transform.parent = asansor.transform;
                        HeroController.instance.thirdPersonController._animator.SetFloat(HeroController.instance.thirdPersonController._animIDSpeed, 0);
                        HeroController.instance.characterController.enabled = false;
                        HeroController.instance.thirdPersonController.enabled = false;
                        arm.GetComponent<Animation>().Play();
                        HeroController.instance.HideInteractSprite();
                        AudioManager.instance.Play_Arm_Pulling();
                        StartCoroutine(StartToLiftJourney());
                        if (isDown)
                        {
                            isDown = false;
                            StartCoroutine(StartAsansorSound());
                        }
                        else
                        {
                            isDown = true;
                            StartCoroutine(StartAsansorSound());
                        }
                    }
                }
            }
            if(isOntheWay)
            {
                if(isDown)
                {
                    leftCark.transform.Rotate(Vector3.forward * 1f);
                    rightCark.transform.Rotate(Vector3.forward * 1f);
                    asansor.transform.position = Vector3.MoveTowards(asansor.transform.position, altKat.transform.position, Time.deltaTime * 1.5f);
                    if(asansor.transform.position.y <= altKat.transform.position.y+0.5f)
                    {
                        isOntheWay = false;
                        HeroController.instance.transform.parent = GameManager.Instance.transform;
                        HeroController.instance.characterController.enabled = true;
                        HeroController.instance.thirdPersonController.enabled = true;
                        AdvancedTPSCamera.Instance.fCamShakeImpulse = 0.5f;
                        audioSource.Stop();
                    }
                }
                else
                {
                    leftCark.transform.Rotate(Vector3.forward * -1f);
                    rightCark.transform.Rotate(Vector3.forward * -1f);
                    asansor.transform.position = Vector3.MoveTowards(asansor.transform.position, ustKat.transform.position, Time.deltaTime * 1.5f);
                    if(asansor.transform.position.y >= ustKat.transform.position.y)
                    {
                        isOntheWay = false;
                        HeroController.instance.transform.parent = GameManager.Instance.transform;
                        HeroController.instance.characterController.enabled = true;
                        HeroController.instance.thirdPersonController.enabled = true;
                        AdvancedTPSCamera.Instance.fCamShakeImpulse = 0.5f;
                        audioSource.Stop();
                    }
                }
            }
        }

        IEnumerator StartAsansorSound()
        {
            yield return new WaitForSeconds(1);
            audioSource.Play();
        }

        IEnumerator StartToLiftJourney()
        {
            yield return new WaitForSeconds(1.5f);
            isOntheWay = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isInteractable = true;
                HeroController.instance.ShowInteractSprite();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isInteractable = false;
                HeroController.instance.HideInteractSprite();
            }
        }
    }
}