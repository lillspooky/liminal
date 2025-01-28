using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class AynaCarkScript : MonoBehaviour
    {
        public GameObject mirror;
        public bool isInteractable = false;
        public GameObject cark;
        bool interacted = false;
        public AudioSource audioSource;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }


        public void Activate()
        {
            if (!interacted)
            {
                Debug.Log("g2");
                interacted = true;
                AdvancedTPSCamera.Instance.distance = AdvancedTPSCamera.Instance.distance + 4;
                AdvancedTPSCamera.Instance.maxDistance = AdvancedTPSCamera.Instance.maxDistance + 4;
                AdvancedTPSCamera.Instance.minDistance = AdvancedTPSCamera.Instance.minDistance + 4;
                HeroController.instance.characterController.enabled = false;
                HeroController.instance.thirdPersonController._animator.SetFloat(HeroController.instance.thirdPersonController._animIDSpeed, 0);
                HeroController.instance.thirdPersonController.enabled = false;
                GameCanvas_Controller.instance.Show_MirrorUI(true);
            }
            else
            {
                interacted = false;
                GameCanvas_Controller.instance.Show_MirrorUI(false);
                AdvancedTPSCamera.Instance.distance = AdvancedTPSCamera.Instance.distance - 4;
                AdvancedTPSCamera.Instance.maxDistance = AdvancedTPSCamera.Instance.maxDistance - 4;
                AdvancedTPSCamera.Instance.minDistance = AdvancedTPSCamera.Instance.minDistance - 4;
                HeroController.instance.characterController.enabled = true;
                HeroController.instance.thirdPersonController.enabled = true;
                audioSource.Stop();
            }
        }

        void Update()
        {
            if (isInteractable)
            {
                if(GameManager.Instance.controllerType == ControllerType.KeyboardMouse)
                {
                    if (Input.GetKeyUp(GameManager.Instance.Keycode_Interact) || Input.GetButtonUp("Interact"))
                    {
                        if (!interacted)
                        {
                            interacted = true;
                            AdvancedTPSCamera.Instance.distance = AdvancedTPSCamera.Instance.distance + 4;
                            AdvancedTPSCamera.Instance.maxDistance = AdvancedTPSCamera.Instance.maxDistance + 4;
                            AdvancedTPSCamera.Instance.minDistance = AdvancedTPSCamera.Instance.minDistance + 4;
                            HeroController.instance.characterController.enabled = false;
                            HeroController.instance.thirdPersonController._animator.SetFloat(HeroController.instance.thirdPersonController._animIDSpeed, 0);
                            HeroController.instance.thirdPersonController.enabled = false;
                            GameCanvas_Controller.instance.Show_MirrorUI(true);
                        }
                        else
                        {
                            interacted = false;
                            GameCanvas_Controller.instance.Show_MirrorUI(false);
                            AdvancedTPSCamera.Instance.distance = AdvancedTPSCamera.Instance.distance - 4;
                            AdvancedTPSCamera.Instance.maxDistance = AdvancedTPSCamera.Instance.maxDistance - 4;
                            AdvancedTPSCamera.Instance.minDistance = AdvancedTPSCamera.Instance.minDistance - 4;
                            HeroController.instance.characterController.enabled = true;
                            HeroController.instance.thirdPersonController.enabled = true;
                            audioSource.Stop();
                        }
                    }
                }
                
                if(interacted)
                {
                    float h = 0;
                    if(GameManager.Instance.controllerType == ControllerType.KeyboardMouse)
                    {
                        h = Input.GetAxis("Horizontal") * Time.deltaTime * 3;
                    }
                    else
                    {
                        h = SimpleJoystick.Instance.HorizontalValue * Time.deltaTime * 3;
                    }
                    mirror.transform.Rotate(new Vector3(0, h, 0));
                    HeroController.instance.thirdPersonController._animator.SetFloat(HeroController.instance.thirdPersonController._animIDSpeed, 0);
                    cark.transform.Rotate(new Vector3(0, 0, h * 5));
                    if(Mathf.Abs(h) > 0)
                    {
                        if (!audioSource.isPlaying)
                        {
                            audioSource.Play();
                        }
                    }
                    else
                    {
                        audioSource.Stop();
                    }
                }
            }
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
                interacted = false;
            }
        }
    }
}