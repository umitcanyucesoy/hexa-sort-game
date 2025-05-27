using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
        [SerializeField] private Material[] materials;
        
        [Header("Spawn Animation")]
        [SerializeField] private float entryDistance = 10f;    
        [SerializeField] private float entryDuration = .6f;
        [SerializeField] private Ease  entryEase     = Ease.OutBack;
        [SerializeField] private float siblingDelay  = .05f; 
        
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
            Vector3 targetPos = parent.position;
            Vector3 startPos  = targetPos + Vector3.right * entryDistance;
            
            HexStack hexStack = Instantiate(hexagonStackPrefab, startPos, Quaternion.identity, parent);
            hexStack.name = $"Stack { parent.GetSiblingIndex() }";

            Material[] chosenMats = GetRandomMaterials();
            
            int amount = Random.Range(minMaxHexCount.x, minMaxHexCount.y);
            int firstMatCount = Random.Range(0, amount);
            
            for (int i = 0; i < amount; i++)
            {
                Vector3 hexagonLocalPos = Vector3.up * (i * .2f);
                Vector3 spawnPos = hexStack.transform.TransformPoint(hexagonLocalPos);
                
                Hexagon hexagonInstance = Instantiate(hexagonPrefab, spawnPos, Quaternion.identity, hexStack.transform);
                
                var matToUse = i < firstMatCount ? chosenMats[0] : chosenMats[1];
                hexagonInstance.SetMaterial(matToUse);
                hexagonInstance.Configure(hexStack);
                hexStack.AddHexagon(hexagonInstance);   
            }
            
            float delay = parent.GetSiblingIndex() * siblingDelay;
            hexStack.transform
                .DOMove(targetPos, entryDuration)
                .SetEase(entryEase)
                .SetDelay(delay);
        }

        private Material[] GetRandomMaterials()
        {
            if (materials == null && materials.Length < 2)
                throw new InvalidOperationException("Need at least two materials in the inspector!");
            
            var list = new List<Material>(materials);
            var first = list.OrderBy(_ => Random.value).First();
            list.Remove(first);
            var second = list.OrderBy(_ => Random.value).First();
            
            return new[] { first, second };
        }
    }
}