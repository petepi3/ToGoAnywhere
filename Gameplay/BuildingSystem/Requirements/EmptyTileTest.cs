using System;
using Petepi.TGA.Grid;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay.BuildingSystem.Requirements
{
    /// <summary>
    /// Test if any structure or resource exists on the tile.
    /// </summary>
    [Serializable]
    public class EmptyTileTest : BuildingRequirement
    {
        public override string TooltipCaption()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("Requirement_EmptyTile");
        }

        public override bool TestTileForRequirement(Tile tile, GridSystem grid, Building prefab)
        {
            // this does not take intersections into account
            // TODO: Rework this test after reworking intersections
            return tile.Resource == Resource.None && !tile.Building && !tile.Road;
        }
    }
}