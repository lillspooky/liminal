using System.Collections;
using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class AnimationStart : MonoBehaviour
    {
        public float starttime = 0;

        void Start()
        {
            StartCoroutine(StartAn());
        }

        IEnumerator StartAn()
        {
            yield return new WaitForSeconds(starttime);
            GetComponent<Animation>().Play();
        }
    }
}