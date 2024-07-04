using System;
using System.Collections.Generic;
using Petepi.TGA.Grid;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay.BuildingSystem.Requirements
{
    [Serializable]
    public class TownDistanceTest : BuildingRequirement
    {
        private Town _nearestTown = null;
        private float _nearestDistance = Mathf.Infinity;
        private bool _inRange;
        
        public override string TooltipCaption()
        {
            return LocalizationSettings.StringDatabase
                .GetLocalizedString("Requirement_TownDistance");
        }

        public override bool TestTileForRequirement(Tile tile, GridSystem grid, Building prefab)
        {
            var inRange = TownSystem.Instance.IsInTownRange(tile.GridPosition, out var town, out var distance);
            _inRange = inRange;
            _nearestTown = town;
            _nearestDistance = distance;
            return inRange;
        }
    }
}