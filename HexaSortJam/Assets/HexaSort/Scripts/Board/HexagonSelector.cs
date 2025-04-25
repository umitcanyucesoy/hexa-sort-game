using System;
using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class HexagonSelector : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask hexagonLayerMask;
        
        public event Action<Hexagon> OnHexagonSelected; 
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (!mainCamera)
                Debug.Log("No main camera!");
        }
        
        public void TrySelectHexagon()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hexagonLayerMask)
                && hit.collider.TryGetComponent(out Hexagon hexagon))
            {
                OnHexagonSelected?.Invoke(hexagon);
            }
        }
    }
}