using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class GridCell : MonoBehaviour
    {
        public HexStack Stack { get; private set; }
        
        public bool IsOccupied => Stack;

        public void SetHexStack(HexStack hexStack)
        {
            Stack = hexStack;
        }

        public void ClearHexStack()
        {
            Stack = null;
        }
    }
}