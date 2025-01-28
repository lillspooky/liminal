using System.Collections;
using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class PotionScript : MonoBehaviour
    {
        public int Extra_Health = 25;
        public int Extra_Mana = 50;
        public int Extra_Speed = 2;
        public int Invisibilty_Duration = 10;
        public int Shield_Duration = 10;
        public string Name;
        public PotionType potionType;
        bool grabbale = false;
        float lastTime = 0;

        private void Start()
        {
            StartCoroutine(Baslat());
        }

        IEnumerator Baslat()
        {
            yield return new WaitForSeconds(1.5f);
            grabbale = true;
        }

        public void GrabIt()
        {
            int parameter = 0;
            switch (potionType)
            {
                case PotionType.Health:
                    parameter = Extra_Health;
                    break;
                case PotionType.Invisibility:
                    parameter = Invisibilty_Duration;
                    break;
                case PotionType.Shield:
                    parameter = Shield_Duration;
                    break;
                case PotionType.Speed:
                    parameter = Extra_Speed;
                    break;
                case PotionType.Mana:
                    parameter = Extra_Mana;
                    break;
            }
            if (HeroController.instance.AddPotion(potionType, parameter))
            {
                GameCanvas_Controller.instance.Show_Image_Potion(potionType);
                AudioManager.instance.Play_Grab();
                GameCanvas_Controller.instance.Show_Grabbed_Text(Name);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && grabbale && Time.time > lastTime + 0.5f)
            {
                lastTime = Time.time;
                GrabIt();
            }
        }
    }

    public enum PotionType
    {
        Health,
        Speed,
        Invisibility,
        Shield,
        Mana
    }
}