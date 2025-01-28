using System.Collections.Generic;
using UnityEngine;

namespace AdvancedRogueLikeandPuzzleSystem
{
    public class GainScript : MonoBehaviour
    {
        public List<GameObject> prefabs;

        public void Spread()
        {
            int a = Random.Range(0, 3);
            if (a == 1)
            {
                GameObject o = Instantiate(prefabs[Random.Range(0, prefabs.Count)], transform.position, Quaternion.identity);
                o.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1, 1), Random.Range(1, 3), Random.Range(-1, 1)) * 4, ForceMode.Impulse);
            }
            else if (a == 2)
            {
                int i = Random.Range(0, prefabs.Count);
                GameObject o = Instantiate(prefabs[i], transform.position, Quaternion.identity);
                o.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1, 1), Random.Range(1, 3), Random.Range(-1, 1)) * 4, ForceMode.Impulse);
                prefabs.RemoveAt(i);
                GameObject j = Instantiate(prefabs[Random.Range(0, prefabs.Count)], transform.position, Quaternion.identity);
                j.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1, 1), Random.Range(1, 3), Random.Range(-1, 1)) * 4, ForceMode.Impulse);
            }
        }
    }
}
