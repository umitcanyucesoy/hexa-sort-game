using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class Hexagon : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] private new Renderer renderer;
        
        public HexStack HexStack { get; private set; }
        public Color Color { get => renderer.material.color; set => renderer.material.color = value; }

        public void Configure(HexStack hexStack)
        {
            HexStack = hexStack;
        }
    }
}