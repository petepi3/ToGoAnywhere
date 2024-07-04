using System;
using Petepi.TGA.Grid;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay.BuildingSystem.Requirements
{
    /// <summary>
    /// Test if the tile has water.
    /// </summary>
    [Serializable]
    public class NotOnWaterTest : BuildingRequirement
    {
        public override string TooltipCaption()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Requirement_NoWater");
        }

        public override bool TestTileForRequirement(Tile tile, GridSystem grid, Building prefab)
        {
            return tile.WaterDepth == 0;
        }
    }
}