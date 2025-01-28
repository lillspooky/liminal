using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class CameraLooker : MonoBehaviour
    {
        void Update()
        {
            transform.LookAt(AdvancedTPSCamera.Instance.transform);
        }
    }
}
