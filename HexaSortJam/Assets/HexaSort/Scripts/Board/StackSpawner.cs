using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HexaSort.Scripts.Board
{
    public class StackSpawner : MonoBehaviour
    {
        [Header("Elements")] 
        [SerializeField] private Transform stackPositionsParent;
        [SerializeField] private Hexagon hexagonPrefab;
        [SerializeField] private HexStack hexagonStackPrefab;

        [Header("Settings")]
        [NaughtyAttributes.MinMaxSlider(2, 8)]
        [SerializeField] private Vector2Int minMaxHexCount;
        [SerializeField] private Color[] colors;
        
        private int currentStackCounter;

        private void Awake()
        {
            HexStackController.OnStackPlaced += OnStackPlacedCallback;
        }

        private void OnDestroy()
        {
            HexStackController.OnStackPlaced -= OnStackPlacedCallback;
        }

        private void Start()
        {
            GenerateStacks();
        }

        private void OnStackPlacedCallback(GridCell gridCell)
        {
            currentStackCounter++;

            if (currentStackCounter >= 3)
            {
                currentStackCounter = 0;
                GenerateStacks();
            }
        }

        private void GenerateStacks()
        {
            for (int i = 0; i < stackPositionsParent.childCount; i++)
                GenerateStack(stackPositionsParent.GetChild(i));
        }

        private void GenerateStack(Transform parent)
        {
            HexStack hexStack = Instantiate(hexagonStackPrefab, parent.position, Quaternion.identity, parent);
            hexStack.name = $"Stack { parent.GetSiblingIndex() }";

            Color[] colorArray = GetRandomColors();
            
            int amount = Random.Range(minMaxHexCount.x, minMaxHexCount.y);
            
            int firstColorHexagonCount = Random.Range(0, amount);
            
            for (int i = 0; i < amount; i++)
            {
                Vector3 hexagonLocalPos = Vector3.up * (i * .2f);
                Vector3 spawnPos = hexStack.transform.TransformPoint(hexagonLocalPos);
                
                Hexagon hexagonInstance = Instantiate(hexagonPrefab, spawnPos, Quaternion.identity, hexStack.transform);
                hexagonInstance.Color = i < firstColorHexagonCount ? colorArray[0] : colorArray[1];

                hexagonInstance.Configure(hexStack);
                
                hexStack.AddHexagon(hexagonInstance);   
            }
        }

        private Color[] GetRandomColors()
        {
            List<Color> colorList = new List<Color>();
            colorList.AddRange(colors);

            if (colorList.Count <= 0)
            {
                return null;
            }
            
            Color firstColor = colorList.OrderBy(x => Random.value).First();
            colorList.Remove(firstColor);
            
            if (colorList.Count <= 0)
            {
                return null;
            }
            
            Color secondColor = colorList.OrderBy(x => Random.value).First();
            
            return new Color[] { firstColor, secondColor };
        }
    }
}