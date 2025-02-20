using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace LiminalEditor
{
    public static class StripCollidersUtility
    {
        static readonly List<Collider> k_Colliders = new();
        static readonly HashSet<Scene> k_Scenes = new();

        [MenuItem("Tools/Strip Colliders")]
        public static void StripColliders()
        {
            k_Scenes.Clear();
            foreach (var gameObject in Selection.gameObjects)
            {
                gameObject.GetComponents<Collider>(k_Colliders);
                foreach (var collider in k_Colliders)
                {
                    if (collider.isTrigger)
                        continue;

                    UnityObject.DestroyImmediate(collider);
                }

                k_Scenes.Add(gameObject.scene);
            }

            foreach (var scene in k_Scenes)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }
    }
}
