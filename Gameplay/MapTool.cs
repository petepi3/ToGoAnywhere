using System;
using Petepi.TGA.Grid;
using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    public abstract class MapTool : MonoBehaviour
    {
        protected GridSystem Grid;
        protected ToolbarController Toolbar;
        public bool isToolActive;

        public void Init(ToolbarController toolbar)
        {
            Toolbar = toolbar;
        }

        private void Awake()
        {
            Grid = FindAnyObjectByType<GridSystem>();
        }
        
        public abstract void OnActivate();
        public abstract void OnDeactivate();
        public abstract void OnUse();
        public abstract void OnAltUse();
        public abstract string GetDisplayName();
    }
}