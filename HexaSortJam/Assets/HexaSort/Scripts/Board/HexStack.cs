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
        
        public Material GetTopHexagonMaterial()
        {
            if (Hexagons == null || Hexagons.Count == 0)
                throw new InvalidOperationException(
                    $"[{name}] stack has no hexagons to get color from."
                );
            return Hexagons[^1].Material;
        }

        public void Initialize()
        {
            Hexagons.Clear();
            
            for (int i = 0; i < transform.childCount; i++)
                AddHexagon(transform.GetChild(i).GetComponent<Hexagon>());
            
            PlacedHexagon();
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