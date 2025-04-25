using UnityEngine;

namespace HexaSort.Scripts.Extensions
{
    public static class TransformExtensions
    {
        public static void Clear(this Transform transform)
        {
            while (transform.childCount > 0)
            {
                Transform child = transform.transform.GetChild(0);
                child.SetParent(null);
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }
}