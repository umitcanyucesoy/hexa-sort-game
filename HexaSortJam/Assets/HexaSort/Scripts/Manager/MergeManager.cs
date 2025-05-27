using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
            
            yield return MoveHexagons(hexagonsToAdd, gridCell);
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

        private IEnumerator MoveHexagons(List<Hexagon> toAdd, GridCell cell)
        {
            float startY = cell.Stack.Hexagons.Count * .2f;
            Sequence allMoves = DOTween.Sequence();         

            for (int i = 0; i < toAdd.Count; i++)
            {
                var hex = toAdd[i];
                Vector3 localPos = Vector3.up * (startY + i * .2f);

                cell.Stack.AddHexagon(hex);

                float delay   = hex.transform.GetSiblingIndex() * .05f;
                float dur     = .2f;

                allMoves.Insert(delay, hex.transform.DOLocalMove(localPos, dur).SetEase(Ease.InOutSine));
                allMoves.Insert(delay, hex.transform.DOLocalRotate(new Vector3(180,0,0), dur, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));
            }

            allMoves.SetLink(cell.gameObject, LinkBehaviour.KillOnDestroy);
            yield return allMoves.WaitForCompletion();  
        }

        private IEnumerator CheckForCompleteStack(GridCell cell, Color topColor)
        {
            if (cell.Stack.Hexagons.Count < 10) yield break;

            var vanishing = new List<Hexagon>();
            for (int i = cell.Stack.Hexagons.Count - 1; i >= 0; i--)
            {
                var h = cell.Stack.Hexagons[i];
                if (h.Color != topColor) break;
                vanishing.Add(h);
            }
            if (vanishing.Count < 10) yield break;

            float delayStep = .01f;
            float delay     = 0f;
            foreach (var h in vanishing)
            {
                h.Vanish(delay);
                cell.Stack.RemoveHexagon(h); 
                delay += delayStep;
            }

            yield return new WaitForSeconds(delay + 0.2f);

            if (cell.Stack.Hexagons.Count == 0)
            {
                Destroy(cell.Stack.gameObject);
                cell.ClearHexStack();
            }
        }
    }
}