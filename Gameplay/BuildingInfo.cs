using System;
using System.Linq;
using Petepi.TGA.Gameplay.BuildingSystem;
using Petepi.TGA.Gameplay.BuildingSystem.Requirements;
using Petepi.TGA.Grid;
using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    /// <summary>
    /// Used in building list, holds additional information for build tool needed to place buildings
    /// </summary>
    [Serializable]
    public class BuildingInfo
    {
        public string name;
        public Sprite icon;
        public Building prefab;

        // All requirements are listed for every building, we only enable the ones we need in inspector.
        // Decided not to make a custom inspector for this as my time is too limited and there
        // aren't that many requirements in total
        public NotOnWaterTest requireNoWater;
        public EmptyTileTest requireEmpty;
        public RoadDistanceTest requireRoad;
        public TownDistanceTest requireTown;
        public TownOverlapTest requireNoTownOverlap;

        public BuildingRequirement[] GetRequirements()
        {
            return new BuildingRequirement[]
            {
                requireEmpty,
                requireNoWater,
                requireRoad,
                requireTown,
                requireNoTownOverlap,
            };
        }

        public bool TestAllRequirements(Tile tile, GridSystem grid)
        {
            return GetRequirements()
                .All(requirement => 
                    !requirement.enabled 
                    || requirement.TestTileForRequirement(tile, grid, prefab));
        }
    }
}