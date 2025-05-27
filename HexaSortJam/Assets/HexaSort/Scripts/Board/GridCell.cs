using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace HexaSort.Scripts.Board
{
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private Hexagon hexagonPrefab;
        [OnValueChanged("GenerateInitialHexagons")] 
        [SerializeField] private Material[] hexagonMaterials;
        
        public HexStack Stack { get; private set; }
        
        public bool IsOccupied =>
            Stack   != null &&
            Stack.Hexagons != null &&
            Stack.Hexagons.Count > 0;

        private void Awake()
        {
            GenerateInitialHexagons();
        }

        private void Start()
        {
            if (transform.childCount > 1)
            {
                Stack = transform.GetChild(1).GetComponent<HexStack>();
                Stack.Initialize();
            }
        }

        public void SetHexStack(HexStack hexStack)
        {
            Stack = hexStack;
        }

        public void ClearHexStack()
        {
            Stack = null;
        }

        private void GenerateInitialHexagons()
        {
            while (transform.childCount > 1)
            {
                Transform t = transform.GetChild(1);
                t.SetParent(null);
                DestroyImmediate(t.gameObject);
            }
            
            Stack = new GameObject("Initial Stack").AddComponent<HexStack>();
            Stack.transform.SetParent(transform);
            Stack.transform.localPosition = Vector3.up * 0.2f;

            for (int i = 0; i < hexagonMaterials.Length; i++)
            {
                Vector3 spawnPosition = Stack.transform.TransformPoint(Vector3.up * i * 0.2f);
                
                Hexagon hexagon = Instantiate(hexagonPrefab, spawnPosition, Quaternion.identity);
                hexagon.SetMaterial(hexagonMaterials[i]);
                hexagon.Configure(Stack);
                Stack.AddHexagon(hexagon);
            }
        }
    }
}