using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class GameCanvas_Controller : MonoBehaviour
    {
        public GameObject MirrorUI;
        public GameObject Text_Warning;
        public GameObject Text_Grabbed;

        public Image Image_Health;
        public Image Image_Mana;
        public Text Text_Coin;
        public Text Text_InteractionKey;

        public GameObject Text_Level;
        public Image Image_Level;
        public GameObject Text_Name;
        public Image Image_Profile;

        public GameObject GamePlayPanel;
        public GameObject GameOverPanel;
        public GameObject Panel_Warning;

        public AudioSource audioSource;
        public GameObject Panel_EnemyStatusInfo;
        public Image Image_EnemyHealth;
        public Text Text_EnemyName;

        public Image Image_Damage;
        public Color Damage_Effect_Color;

        public GameObject Image_Health_Potion;
        public GameObject Image_Mana_Potion;
        public static GameCanvas_Controller instance;

        public GameObject Joystick;
        public GameObject Touchpad;
        public GameObject Button_Jump;
        public GameObject Button_Interact;
        public GameObject Button_Hit;
        public GameObject Button_Shield;
        public GameObject Button_Sprint;
        public GameObject Button_Pause;


        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            Update_Health_Bar();
            Update_Text_Coin();
            Text_InteractionKey.text = GameManager.Instance.Keycode_Interact.ToString();
        }

        public void Configure_For_PCConsole()
        {
            Joystick.SetActive(false);
            Touchpad.SetActive(false);
            Button_Sprint.SetActive(false);
            Button_Jump.SetActive(false);
            Button_Interact.SetActive(false);
            Button_Pause.SetActive(false);
            Button_Hit.SetActive(false);
            Button_Shield.SetActive(false);
        }

        public void Click_Button_Sprint()
        {
            ThirdPersonController.Instance.Sprint_Now();
        }

        public IEnumerator Show_GameOver_Panel()
        {
            yield return new WaitForSeconds(3);
            GameOverPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
        }

        public void Show_MirrorUI(bool decision)
        {
            if(GameManager.Instance.controllerType == ControllerType.KeyboardMouse)
            {
                MirrorUI.SetActive(decision);
            }
        }

        public void Configure_For_Mobile()
        {
            Joystick.SetActive(true);
            Touchpad.SetActive(true);
            Button_Sprint.SetActive(true);
            Button_Pause.SetActive(true);
            Button_Jump.SetActive(true);
        }

        public void Click_Interact_Button()
        {
            HeroController.instance.CheckInteractionInputsForMobile();
        }

        public void Click_Hit_Button()
        {
            HeroController.instance.Hit();
        }

        public void Click_Button_Pause()
        {
            PauseTheGame();
        }

        public void Click_Defend_Button()
        {
            HeroController.instance.Defend();
        }

        public void Click_Button_Potion(int i)
        {
            HeroController.instance.CheckPotionInputsForMobile(i);
        }

        public void Hide_Button_Shield()
        {
            Button_Shield.SetActive(false);
        }

        public void Show_Button_Shield()
        {
            if (GameManager.Instance.controllerType == ControllerType.Mobile)
            {
                Button_Shield.SetActive(true);
            }
        }

        public void Show_Button_Interact()
        {
            Button_Interact.SetActive(true);
        }

        public void Hide_Button_Interact()
        {
            Button_Interact.SetActive(false);
        }

        public void Hide_Button_Hit()
        {
            Button_Hit.SetActive(false);
        }

        public void Show_Button_Hit()
        {
            if (GameManager.Instance.controllerType == ControllerType.Mobile)
            {
                Button_Hit.SetActive(true);
            }
        }

        public void Click_Button_Jump()
        {
            HeroController.instance.thirdPersonController.JumpNow();
        }

        public void Update_Mana_Bar(int Spended)
        {
            Image_Mana.gameObject.SetActive(true);
            HeroController.instance.Mana = HeroController.instance.Mana - Spended;
            float newResult = (float)(HeroController.instance.Mana / HeroController.instance.TotalMana);
            Image_Mana.fillAmount = newResult;
        }

        public GameObject Panel_Pause;

        public void PauseTheGame()
        {
            if (GameOverPanel.activeSelf) return;
            Panel_Pause.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
            if(GameManager.Instance.controllerType == ControllerType.Mobile)
            {
                Button_Pause.SetActive(false);
            }
        }

        public void Click_Button_Continue()
        {
            if (GameOverPanel.activeSelf) return;
            Panel_Pause.SetActive(false);
            Time.timeScale = 1;
            if (GameManager.Instance.controllerType == ControllerType.Mobile)
            {
                Button_Pause.SetActive(true);
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        public void Click_Button_Quit()
        {
            Application.Quit();
        }

        private void Update()
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                if (Time.timeScale == 0)
                {
                    Click_Button_Continue();
                }
                else
                {
                    PauseTheGame();
                }
            }
        }

        public void Click_Button_RestartGame()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Update_ProfileDetails(string name, Sprite profileImage)
        {
            Text_Name.GetComponentInChildren<Text>().text = name;
            Image_Profile.sprite = profileImage;
        }

        public void Show_Grabbed_Text(string itemName)
        {
            Text_Grabbed.SetActive(false);
            Text_Grabbed.GetComponentInChildren<Text>().text = "- " + itemName.ToUpper();
            Text_Grabbed.SetActive(true);
            StopCoroutine(Hide_Grabbed_TextAsync());
            StartCoroutine(Hide_Grabbed_TextAsync());
        }

        IEnumerator Hide_Grabbed_TextAsync()
        {
            yield return new WaitForSeconds(2);
            Text_Grabbed.SetActive(false);
        }

        public void Update_Text_Coin()
        {
            Text_Coin.text = PlayerPrefs.GetInt("Coin").ToString();
        }

        public void Show_Image_Potion(PotionType potionType)
        {
            switch (potionType)
            {
                case PotionType.Health:
                    Image_Health_Potion.SetActive(true);
                    break;
                case PotionType.Mana:
                    Image_Mana_Potion.SetActive(true);
                    break;
            }
        }

        public void Hide_Image_Potion(PotionType potionType)
        {
            switch (potionType)
            {
                case PotionType.Health:
                    Image_Health_Potion.SetActive(false);
                    break;
                case PotionType.Mana:
                    Image_Mana_Potion.SetActive(false);
                    break;
            }
        }

        public void Update_Health_Bar()
        {
            if (HeroController.instance != null)
            {
                float newResult = (float)(HeroController.instance.Health / HeroController.instance.TotalHealth);
                Image_Health.fillAmount = newResult;
            }
        }

        public void Show_Damage_Effect()
        {
            Image_Damage.color = Color.clear;
            AdvancedTPSCamera.Instance.fCamShakeImpulse = 0.25f;
            Image_Damage.color = Damage_Effect_Color;
            StartCoroutine(ResetDamageImageColor());
        }

        IEnumerator ResetDamageImageColor()
        {
            yield return new WaitForSeconds(0.5f);
            Image_Damage.color = Color.clear;
        }

        public void Show_Warning(String text)
        {
            Panel_Warning.SetActive(true);
            Panel_Warning.transform.GetChild(0).GetComponent<Text>().text = text;
            StopCoroutine(Hide_Warning());
            StartCoroutine(Hide_Warning());
        }

        IEnumerator Hide_Warning()
        {
            yield return new WaitForSeconds(1.5f);
            Panel_Warning.SetActive(false);
        }

        public void Show_EnemyHealthPanel(string name, int health, float totalhealth)
        {
            Panel_EnemyStatusInfo.SetActive(true);
            float newResult = (health > 0 ? (float)(health / totalhealth) : 0);
            Image_EnemyHealth.fillAmount = newResult;
            Text_EnemyName.text = name;
            StopCoroutine(Hide_Panel_EnemyStatus());
            StartCoroutine(Hide_Panel_EnemyStatus());
        }

        IEnumerator Hide_Panel_EnemyStatus()
        {
            yield return new WaitForSeconds(4);
            Panel_EnemyStatusInfo.SetActive(false);
        }
    }
}