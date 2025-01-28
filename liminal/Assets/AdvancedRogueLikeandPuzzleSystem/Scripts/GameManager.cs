using UnityEngine;
using UnityEngine.UI;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class GameManager : MonoBehaviour
    {
        public ControllerType controllerType;
        public static GameManager Instance;

        [Header("PC Control Settings: (Ignore here if you select Mobile Controller Type)")]
        public KeyCode Keycode_Interact;
        public KeyCode Keycode_Sprint;
        public KeyCode Keycode_Jump;
        public KeyCode Keycode_Using_Potion_Heal;
        public KeyCode Keycode_Using_Potion_Mana;
        public GameObject MainCamera;

        public void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Time.timeScale = 1;

            if (controllerType == ControllerType.KeyboardMouse)
            {
                GameCanvas_Controller.instance.Configure_For_PCConsole();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (controllerType == ControllerType.Mobile)
            {
                GameCanvas_Controller.instance.Configure_For_Mobile();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }

    public enum ControllerType
    {
        KeyboardMouse,
        Mobile
    }
}