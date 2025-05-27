using DG.Tweening;
using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class Hexagon : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] private new Renderer renderer;
        [SerializeField] private Collider hexagonCollider;
        
        private bool _isVanishing;
        private Material _material;
        
        public HexStack HexStack { get; private set; }
        public Material Material => _material;
        
        public void Configure(HexStack hexStack)
        {
            HexStack = hexStack;
        }

        public void DisableCollider()
        {
            hexagonCollider.enabled = false;
        }
        
        public void Vanish(float delay)
        {
            if (_isVanishing) return;      
            _isVanishing = true;
    
            transform.SetParent(null, true);

            DOTween.Sequence()
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .AppendInterval(delay)
                .Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack))
                .AppendCallback(() => Destroy(gameObject));
        }

        public void SetMaterial(Material matToUse)
        {
            _material = matToUse;
            if(renderer) renderer.material = matToUse;
        }
    }
}