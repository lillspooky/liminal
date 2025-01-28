using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class ExplosionScript : MonoBehaviour
    {
        public AudioSource audioSource;

        void Start()
        {
            AdvancedTPSCamera.Instance.fCamShakeImpulse = 0.55f;
            audioSource.Play();
        }
    }
}