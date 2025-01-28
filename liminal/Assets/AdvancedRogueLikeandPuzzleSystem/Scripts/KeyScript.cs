using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class KeyScript : MonoBehaviour
    {
        public int Key_ID = 0;
        public string Name;

        public void GrabIt()
        {
            HeroController.instance.Keys_Grabbed.Add(Key_ID);
            GameCanvas_Controller.instance.Show_Grabbed_Text(Name);
            AudioManager.instance.Play_Grab();
            Destroy(gameObject);
        }
    }
}