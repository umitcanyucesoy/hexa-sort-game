using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class HexStackController : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] private Camera mainCamera;

        [Header("Layers")] 
        [SerializeField] private LayerMask gridLayerMask;
        [SerializeField] private LayerMask groundLayerMask;

        [Header("DOTween Settings")] 
        [SerializeField] private float dragSmoothTime = .1f;
        [SerializeField] private float dropSmoothTime = .25f;
        [SerializeField] private Ease dragEase = Ease.OutQuad;
        [SerializeField] private Ease dropEase = Ease.OutBack;
        
        public static event Action<GridCell> OnStackPlaced; 
        
        private HexagonSelector hexagonSelector;
        private HexStack currentStack;
        private GridCell targetCell;
        private Vector3 initialPosition;
        private Tween currentTween;

        private void Awake()
        {
            hexagonSelector = GetComponent<HexagonSelector>();
            if(!mainCamera) mainCamera = Camera.main;
        }

        private void OnEnable() => hexagonSelector.OnHexagonSelected += OnHexSelected;
        private void OnDisable() => hexagonSelector.OnHexagonSelected -= OnHexSelected;
        private void OnDestroy() { currentTween?.Kill(); }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) hexagonSelector.TrySelectHexagon();
            if (Input.GetMouseButton(0) && currentStack) DragStack();
            if (Input.GetMouseButtonUp(0) && currentStack) DropStack();
        }

        private void OnHexSelected(Hexagon hex)
        {
            currentStack = hex.HexStack;
            initialPosition = currentStack.transform.position;
            
            currentTween?.Kill();
        }

        private void DragStack()
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 500f, gridLayerMask | groundLayerMask))
            {
                if (hit.collider.TryGetComponent(out GridCell cell) && !cell.IsOccupied)
                    targetCell = cell;
                else
                    targetCell = null;

                Vector3 targetPos = hit.point + Vector3.up * 1.2f;
                currentTween?.Kill();

                currentTween = currentStack.transform
                    .DOMove(targetPos, dragSmoothTime)
                    .SetEase(dragEase);
            }
        }

        private void DropStack()
        {
            if (targetCell)
            {
                currentTween?.Kill();
                Vector3 dropTargetPosition = targetCell.transform.position + Vector3.up * 0.2f;
                currentTween = currentStack.transform
                    .DOMove(dropTargetPosition, dropSmoothTime)
                    .SetEase(dropEase)
                    .OnComplete(() =>
                        {
                            currentStack.transform.SetParent(targetCell.transform, true);
                            targetCell.SetHexStack(currentStack);
                            currentStack.PlacedHexagon();
                            OnStackPlaced?.Invoke(targetCell);
                            Cleanup();
                        }
                    );
            }
            else
            {
                currentTween?.Kill();
                currentTween = currentStack.transform
                    .DOMove(initialPosition, dropSmoothTime)
                    .SetEase(dropEase)
                    .OnComplete(Cleanup);
            }
        }

        private void Cleanup()
        {
            currentStack = null;
            targetCell  = null;
            currentTween = null;
        }
        
    }
}