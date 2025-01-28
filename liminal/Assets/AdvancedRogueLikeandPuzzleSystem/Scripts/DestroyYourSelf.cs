using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class DestroyYourSelf : MonoBehaviour
    {
        public float DestroyTime = 3;
        void Start()
        {
            Destroy(gameObject, DestroyTime);
        }
    }
}