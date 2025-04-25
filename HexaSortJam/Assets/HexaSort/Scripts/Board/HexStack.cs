using System.Collections.Generic;
using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class HexStack : MonoBehaviour
    {
        public List<Hexagon> Hexagons { get; private set; }

        public void AddHexagon(Hexagon hexagon)
        {
            Hexagons ??= new List<Hexagon>();
            Hexagons.Add(hexagon);
        }
    }
}