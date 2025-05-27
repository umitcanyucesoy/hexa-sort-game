using System;
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
        public static bool IsMerging { get; private set; } = false;
        private List<GridCell> updatedCells = new List<GridCell>();
        public static event Action<bool> OnMergeStateChanged;
        
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
            if (!IsMerging)
            {
                IsMerging = true;
                OnMergeStateChanged?.Invoke(true);
            }
            
            updatedCells.Add(gridCell);

            while (updatedCells.Count > 0)
                yield return CheckForMerge(updatedCells[0]);
            
            IsMerging = false;
            OnMergeStateChanged?.Invoke(false);
        }

        private IEnumerator CheckForMerge(GridCell gridCell)
        {
            updatedCells.Remove(gridCell);
            
            if (!gridCell.IsOccupied)
                yield break;
            
            var targetStack = gridCell.Stack;
            if (!targetStack || targetStack.Hexagons.Count == 0)
                yield break;

            Material topMat = targetStack.GetTopHexagonMaterial();

            var neighbors = GetNeighborGridCells(gridCell);
            var sameNeighbors = GetSimilarNeighborGridCells(topMat, neighbors);

            if (sameNeighbors.Count == 0) yield break;
            
            updatedCells.AddRange(sameNeighbors);

            var toMove = GetHexagonsToAdd(topMat, sameNeighbors);
            RemoveHexagonsFromStacks(toMove, sameNeighbors);

            foreach (var n in sameNeighbors)
            {
                if (n.Stack && n.Stack.Hexagons.Count == 0)
                {
                    Destroy(n.Stack.gameObject);
                    n.ClearHexStack();
                }
            }

            yield return MoveHexagons(toMove, gridCell);
            yield return CheckForCompleteStack(gridCell, topMat);
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

        private List<GridCell> GetSimilarNeighborGridCells(Material mat, List<GridCell> neighbors)
        {
            var list = new List<GridCell>();
            foreach (var n in neighbors)
            {
                if (n.Stack &&
                    n.Stack.Hexagons.Count > 0 &&
                    n.Stack.GetTopHexagonMaterial() == mat)
                {
                    list.Add(n);
                }
            }
            return list;
        }

        private List<Hexagon> GetHexagonsToAdd(Material mat, List<GridCell> sameNeighbors)
        {
            var outList = new List<Hexagon>();
            foreach (var n in sameNeighbors)
            {
                var stack = n.Stack;
                for (int i = stack.Hexagons.Count - 1; i >= 0; i--)
                {
                    var h = stack.Hexagons[i];
                    if (h.Material != mat) break;
                    outList.Add(h);
                    h.transform.SetParent(null);
                }
            }
            return outList;
        }

        private void RemoveHexagonsFromStacks(List<Hexagon> hexagonsToAdd, List<GridCell> similarNeighborGridCells)
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

        private IEnumerator CheckForCompleteStack(GridCell cell, Material mat)
        {
            if (!cell.Stack || cell.Stack.Hexagons == null || cell.Stack.Hexagons.Count < 10)
                yield break;
            
            if (!cell.Stack)
            {
                updatedCells.Remove(cell);   
                yield break;
            }

            var vanishing = new List<Hexagon>();
            for (int i = cell.Stack.Hexagons.Count - 1; i >= 0; i--)
            {
                var h = cell.Stack.Hexagons[i];
                if (h.Material != mat) break;
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
            
            updatedCells.Add(cell);

            yield return new WaitForSeconds(delay + 0.2f);

            if (cell.Stack.Hexagons.Count == 0)
            {
                Destroy(cell.Stack.gameObject);
                cell.ClearHexStack();
            }
        }
    }
}