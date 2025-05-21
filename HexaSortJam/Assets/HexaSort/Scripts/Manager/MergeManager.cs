using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HexaSort.Scripts.Board;
using UnityEngine;

namespace HexaSort.Scripts.Manager
{
    public class MergeManager : MonoBehaviour
    {
        private void Awake()
        {
            HexStackController.OnStackPlaced += StackPlacedCallback;
        }

        private void OnDestroy()
        {
            HexStackController.OnStackPlaced -= StackPlacedCallback;
        }

        private void StackPlacedCallback(GridCell gridCell)
        {
            StartCoroutine(StackPlacedCoroutine(gridCell));
        }

        private IEnumerator StackPlacedCoroutine(GridCell gridCell)
        {
            yield return CheckForMerge(gridCell);
        }

        private IEnumerator CheckForMerge(GridCell gridCell)
        {
            List<GridCell> neighborGridCells = GetNeighborGridCells(gridCell);
            
            var targetStack = gridCell.Stack;
            if (!targetStack || targetStack.Hexagons.Count == 0)
                yield break;  
            
            Color gridCellTopHexColor = targetStack.GetTopHexagonColor();
                
            List<GridCell> similarNeighborGridCells = GetSimilarNeighborGridCells(gridCellTopHexColor, neighborGridCells.ToArray());

            if (similarNeighborGridCells.Count <= 0)
                yield break;
            
            List<Hexagon> hexagonsToAdd = GetHexagonsToAdd(gridCellTopHexColor, similarNeighborGridCells.ToArray());
            
            RemoveHexagonsFromStacks(hexagonsToAdd, similarNeighborGridCells.ToArray());
            
            foreach (var neighbor in similarNeighborGridCells)
            {
                var stack = neighbor.Stack;
                if (stack && stack.Hexagons.Count == 0)
                {
                    Destroy(stack.gameObject);
                    neighbor.ClearHexStack();
                }
            }
            
            MoveHexagons(hexagonsToAdd, gridCell);
            
            yield return new WaitForSeconds(0.2f + (hexagonsToAdd.Count + 1) * .05f);
            
            yield return CheckForCompleteStack(gridCell, gridCellTopHexColor);
        }

        private List<GridCell> GetNeighborGridCells(GridCell gridCell)
        {
            LayerMask gridCellMask = 1 << gridCell.gameObject.layer;
            
            List<GridCell> neighborGridCells = new List<GridCell>();
            
            Collider[] neighborGridCellColliders = Physics.OverlapSphere(
                gridCell.transform.position,
                2,
                gridCellMask);

            foreach (var gridCellCollider in neighborGridCellColliders)
            {
                if (gridCellCollider.TryGetComponent<GridCell>(out var neighborGridCell))
                {
                    if (!neighborGridCell.IsOccupied || neighborGridCell == gridCell) 
                        continue;
                    
                    neighborGridCells.Add(neighborGridCell);
                }
            }
            
            return neighborGridCells;
        }

        private List<GridCell> GetSimilarNeighborGridCells(Color gridCellTopHexColor, GridCell[] neighborGridCells)
        {
            List<GridCell> similarNeighborGridCells = new List<GridCell>();

            foreach (var neighborGridCell in neighborGridCells)
            {
                var neighborStack = neighborGridCell.Stack;
                if (!neighborStack || neighborStack.Hexagons.Count == 0)
                    continue;
                
                Color neighborGridCellTopHexColor = neighborGridCell.Stack.GetTopHexagonColor();
                
                if (gridCellTopHexColor == neighborGridCellTopHexColor)
                    similarNeighborGridCells.Add(neighborGridCell);
            }
            
            return similarNeighborGridCells;
        }

        private List<Hexagon> GetHexagonsToAdd(Color gridCellTopHexColor, GridCell[] similarNeighborGridCells)
        {
            List<Hexagon> hexagonsToAdd = new List<Hexagon>();

            foreach (var neighborGridCell in similarNeighborGridCells)
            {
                HexStack neighborCellHexStack = neighborGridCell.Stack;

                for (int i = neighborCellHexStack.Hexagons.Count - 1; i >= 0; i--)
                {
                    Hexagon hexagon = neighborCellHexStack.Hexagons[i];

                    if (!hexagon.Color.Equals(gridCellTopHexColor))
                        break;
                    
                    hexagonsToAdd.Add(hexagon);
                    hexagon.transform.SetParent(null);
                }
            }
            
            return hexagonsToAdd;
        }

        private void RemoveHexagonsFromStacks(List<Hexagon> hexagonsToAdd, GridCell[] similarNeighborGridCells)
        {
            foreach (var neighborGridCell in similarNeighborGridCells)
            {
                HexStack hexStack = neighborGridCell.Stack;

                foreach (var hexagon in hexagonsToAdd)
                {
                    if (hexStack.ContainsHexagon(hexagon))
                        hexStack.RemoveHexagon(hexagon);
                }
            }
        }

        private void MoveHexagons(List<Hexagon> hexagonsToAdd, GridCell gridCell)
        {
            float initialY = gridCell.Stack.Hexagons.Count * .2f;

            for (int i = 0; i < hexagonsToAdd.Count; i++)
            {
                Hexagon hexagon = hexagonsToAdd[i];

                float targetY = initialY + i * .2f;
                Vector3 targetLocalPos = Vector3.up * targetY;
                
                gridCell.Stack.AddHexagon(hexagon);
                hexagon.MoveToLocal(targetLocalPos);
            }
        }

        private IEnumerator CheckForCompleteStack(GridCell gridCell, Color topColor)
        {
            if (gridCell.Stack.Hexagons.Count < 10)
                yield break;
            
            List<Hexagon> similarHexagons = new List<Hexagon>();

            for (int i = gridCell.Stack.Hexagons.Count - 1; i >= 0; i--)
            {
                Hexagon hexagon = gridCell.Stack.Hexagons[i];

                if (hexagon.Color != topColor)
                    break;
                
                similarHexagons.Add(hexagon);
            }

            if (similarHexagons.Count < 10)
                yield break;

            float delay = 0;
            
            foreach (var hex in similarHexagons)
            {
                hex.transform.SetParent(null);
                hex.Vanish(delay);
                delay += .01f;
                
                gridCell.Stack.RemoveHexagon(hex);
            }
            similarHexagons.Clear();

            yield return new WaitForSeconds(0.2f + (similarHexagons.Count +1) * .1f);
            
            if (gridCell.Stack.Hexagons.Count == 0)
            {
                Destroy(gridCell.Stack.gameObject);
                gridCell.ClearHexStack();
            }
        }
    }
}