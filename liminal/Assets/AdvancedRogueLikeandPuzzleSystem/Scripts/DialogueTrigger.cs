using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class DialogueTrigger : MonoBehaviour
    {
        public string Speaker;
        public string Speech;

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                SpeechManager.instance.Show_Speach(Speech, Speaker);
                Destroy(gameObject);
            }
        }
    }
}
