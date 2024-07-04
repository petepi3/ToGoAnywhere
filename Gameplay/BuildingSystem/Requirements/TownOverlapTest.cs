using System;
using Petepi.TGA.Grid;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay.BuildingSystem.Requirements
{
    [Serializable]
    public class TownOverlapTest : BuildingRequirement
    {
        public override string TooltipCaption()
        {
            return LocalizationSettings.StringDatabase
                .GetLocalizedString("Requirement_NoTownOverlap");;
        }

        public override bool TestTileForRequirement(Tile tile, GridSystem grid, Building prefab)
        {           
            // ignore test if not town hall
            return prefab is not TownHall hall || TownSystem.Instance.TestNoBorderOverlap(tile.GridPosition, hall.range);
        }
    }
}