using System;
using UnityEngine;

namespace HexaSort.Scripts.Board
{
    public class Tower : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float fillIncrement;

        private float fillPercent;
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            UpdateMaterials();
        }

        private void Update()
        {
            Fill();
        }

        private void Fill()
        {
            if (fillPercent >= 87)
                return;
            
            fillPercent += fillIncrement;
            UpdateMaterials();
            
            animator.Play("Tower Bump");
        }

        private void UpdateMaterials()
        {
            foreach (var material in _renderer.materials)
            {
                material.SetFloat("_Fill_Percent", fillPercent);   
            }
        }
    }
}
