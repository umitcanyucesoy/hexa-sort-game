using DG.Tweening;
using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class Hexagon : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] private new Renderer renderer;
        [SerializeField] private Collider hexagonCollider;
        
        public HexStack HexStack { get; private set; }
        public Color Color { get => renderer.material.color; set => renderer.material.color = value; }

        public void Configure(HexStack hexStack)
        {
            HexStack = hexStack;
        }

        public void DisableCollider()
        {
            hexagonCollider.enabled = false;
        }

        public void MoveToLocal(Vector3 targetLocalPos)
        {
            transform.DOLocalMove(targetLocalPos, .2f)
                .SetEase(Ease.InOutSine)
                .SetDelay(transform.GetSiblingIndex() * .01f);
        }

        public void Vanish(float delay)
        {
            transform.SetParent(null, true);
            
            transform.DOKill();
            transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetDelay(delay)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}