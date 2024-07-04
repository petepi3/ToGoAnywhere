using System;
using System.Collections.Generic;
using System.Text;
using Petepi.TGA.Grid;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay.BuildingSystem
{
    public class Building : MonoBehaviour, ITileInspector
    {
        public string buildingName;
        [HideInInspector] public Vector2Int gridPosition;

        protected GridSystem Grid;
        protected Town Town;

        public virtual void OnBuildToolSelected(Tile tile, Action<Vector2Int, float> showRange)
        {
            
        }

        protected virtual void OnInitialized()
        {
        }

        public virtual void Initialize(GridSystem grid, Vector2Int position, Town town)
        {
            Grid = grid;
            gridPosition = position;
            Town = town;
            OnInitialized();
        }

        public virtual void StartInspector(StringBuilder tooltipText)
        {
            var localizedName = LocalizationSettings.StringDatabase
                .GetLocalizedString(buildingName);
            var localizedHeader = LocalizationSettings.StringDatabase
                .GetLocalizedString("Building_NameHeader", localizedName);
            tooltipText.AppendLine(localizedHeader);
        }

        public virtual void EndInspector(ToolbarController toolbar)
        {
            
        }
    }
}