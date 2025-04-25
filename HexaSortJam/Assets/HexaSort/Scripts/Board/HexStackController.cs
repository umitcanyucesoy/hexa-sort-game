using System;
using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class HexStackController : MonoBehaviour
    {
        [Header("Settings")]
        private HexagonSelector hexagonSelector;
        private HexStack currentStack;
        private Vector3 currentStackInitialPosition;

        private void Awake()
        {
            hexagonSelector = GetComponent<HexagonSelector>();
        }

        private void OnEnable()
        {
            hexagonSelector.OnHexagonSelected += HandleHexagonSelected;
        }

        private void OnDisable()
        {
            hexagonSelector.OnHexagonSelected -= HandleHexagonSelected;
        }

        private void Update()
        {
            ManageControl();
        }

        private void HandleHexagonSelected(Hexagon hexagon)
        {
            currentStack = hexagon.HexStack;
            currentStackInitialPosition = currentStack.transform.position;
        }

        private void ManageControl()
        {
            if (Input.GetMouseButtonDown(0))
               ManageMouseDown();
            if (Input.GetMouseButton(0))
                ManageMouseDrag();
            if (Input.GetMouseButtonUp(0))
                ManageMouseUp();
        }

        private void ManageMouseDown()
        {
            hexagonSelector.TrySelectHexagon();
        }
        
        private void ManageMouseDrag()
        {
        }

        private void ManageMouseUp()
        {
        }
        
    }
}