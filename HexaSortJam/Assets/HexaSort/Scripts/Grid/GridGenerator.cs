
using HexaSort.Scripts.Extensions;
using NaughtyAttributes;
using UnityEngine;

namespace HexaSort.Scripts.Grid
{
    public class GridGenerator : MonoBehaviour
    {
        [Header("Elements")] 
        [SerializeField] private UnityEngine.Grid grid;
        [SerializeField] private GameObject hexagon;
        
        [Header("Settings")]
        [OnValueChanged("GenerateGrid")]
        [SerializeField] private int gridSize;

        private void GenerateGrid()
        {
            transform.Clear();

            for (int x = -gridSize; x <= gridSize; x++)
            {
                for (int y = -gridSize; y <= gridSize; y++)
                {
                    Vector3 spawnPosition = grid.CellToWorld(new Vector3Int(x, y, 0));

                    if (spawnPosition.magnitude > grid.CellToWorld(new Vector3Int(1, 0, 0)).magnitude * gridSize)
                        continue;
                    
                    Instantiate(hexagon, spawnPosition, Quaternion.identity, transform);
                }
            }
        }
        
        
    }
}