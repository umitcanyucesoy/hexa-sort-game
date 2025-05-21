using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class HexStack : MonoBehaviour
    {
        public List<Hexagon> Hexagons { get; private set; } = new List<Hexagon>();
        
        public bool ContainsHexagon(Hexagon hexagon) => Hexagons.Contains(hexagon);
        
        public Color GetTopHexagonColor()
        {
            if (Hexagons == null || Hexagons.Count == 0)
                throw new InvalidOperationException(
                    $"[{name}] stack has no hexagons to get color from."
                );
            return Hexagons[^1].Color;
        }

        public void AddHexagon(Hexagon hexagon)
        {
            Hexagons ??= new List<Hexagon>();
            Hexagons.Add(hexagon);
            
            hexagon.transform.SetParent(transform);
        }

        public void PlacedHexagon()
        {
            foreach (var hexagon in Hexagons)
                hexagon.DisableCollider();
        }

        public void RemoveHexagon(Hexagon hexagon)
        {
            Hexagons.Remove(hexagon);
        } 
            
    }
}