using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class AudioPlayScript : MonoBehaviour
    {
        public AudioSource audioSource;
        public float PlayingPeriod = 0;
        public float WaitingForStart = 0;

        void Play()
        {
            audioSource.Play();
        }
    }
}