using System;
using System.Collections.Generic;
using Petepi.TGA.Grid;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay.BuildingSystem.Requirements
{
    /// <summary>
    /// Test if a tile is within a specified distance of a road.
    /// </summary>
    [Serializable]
    public class RoadDistanceTest : BuildingRequirement
    {
        public float minimumDistance = 5f;
        
        public override string TooltipCaption()
        {
            return LocalizationSettings.StringDatabase
                .GetLocalizedString("Requirement_RoadDistance", new List<object>{minimumDistance});
        }

        public override bool TestTileForRequirement(Tile tile, GridSystem grid, Building prefab)
        {
            if (RoadManager.Instance.RoadCount <= 0) return false;
            var nearest = RoadManager.Instance.FindNearestRoadPoint(tile.GridPosition);
            // Distance measured in world units but given grid positions as input
            // this biases the distance horizontally.
            // TODO: implement distance measuring function that takes that into account.
            return Vector2Int.Distance(tile.GridPosition, nearest.Point) <= minimumDistance;
        }
    }
}