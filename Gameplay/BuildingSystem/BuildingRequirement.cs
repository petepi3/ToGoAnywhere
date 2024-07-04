using System;
using Petepi.TGA.Grid;

namespace Petepi.TGA.Gameplay.BuildingSystem
{
    [Serializable]
    public abstract class BuildingRequirement
    {
        /// <summary>
        /// disabled requirements are not taken into account when placing buildings
        /// </summary>
        public bool enabled = false;
        
        /// <returns>Text to be displayed in a tooltip for this requirement</returns>
        public abstract string TooltipCaption();
        
        /// <summary>
        /// Test if the tile passes the requirements for the prefab we are trying to place.
        /// </summary>
        /// <param name="tile">Tested tile</param>
        /// <param name="grid">Reference to grid system</param>
        /// <param name="prefab">Prefab of the building that's being placed</param>
        /// <returns>True if the requirements are met</returns>
        public abstract bool TestTileForRequirement(Tile tile, GridSystem grid, Building prefab);
    }
}