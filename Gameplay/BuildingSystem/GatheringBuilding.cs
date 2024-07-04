using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Petepi.TGA.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay.BuildingSystem
{
    public class GatheringBuilding : Building
    {
        public Resource gatheredResource;
        public float gatherPerMinute = 5f;
        public float gatherRange = 20f;

        private List<Vector2Int> _resourcesInRange;

        public float GatherPerSecond => gatherPerMinute / 60f;

        public override void OnBuildToolSelected(Tile tile, Action<Vector2Int, float> showRange)
        {
            showRange(tile.GridPosition, gatherRange);
        }

        private void FixedUpdate()
        {
            // Placeholder mechanic to test the rest of the system,
            // gathering building are meant to mine a specific amount of individual resources at the same time,
            // currently mine everything in range at the same time. 
            // TODO: implement correct gathering mechanics.
            var totalGathered = 0f;
            _resourcesInRange.ToList().ForEach(position =>
            {
                var tile = Grid.GetTile(position);
                var gatherAmount = GatherPerSecond * Time.deltaTime;
                gatherAmount = Mathf.Min(gatherAmount, tile.ResourceAmount);
                tile.ResourceAmount -= GatherPerSecond * Time.deltaTime;
                if (tile.ResourceAmount <= 0)
                {
                    tile.Resource = Resource.None;
                    tile.ResourceAmount = 0;
                    _resourcesInRange.Remove(position);
                }

                totalGathered += gatherAmount;
                Grid.SetTile(tile);
            });
            Town.AddResource(gatheredResource, totalGathered, ResourceSource.Production);
        }

        protected override void OnInitialized()
        {
            _resourcesInRange = Grid.FindResourcesInRange(gridPosition, gatherRange, gatheredResource);
        }

        public override void StartInspector(StringBuilder tooltipText)
        {
            base.StartInspector(tooltipText);
            var resourceLocalizationKey = Utils.GetResourceLocalizationKey(gatheredResource);
            var localizedResource = LocalizationSettings.StringDatabase
                .GetLocalizedString(resourceLocalizationKey);
            tooltipText.AppendLine(
                LocalizationSettings.StringDatabase
                    .GetLocalizedString("BuildingTooltip_Gathering", 
                        new List<object> {localizedResource, gatheredResource}));
        }
    }
}