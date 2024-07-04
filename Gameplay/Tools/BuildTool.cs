using System;
using System.Linq;
using System.Text;
using Petepi.TGA.Gameplay.BuildingSystem;
using Petepi.TGA.Gameplay.Tools.ToolSettings;
using Petepi.TGA.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

namespace Petepi.TGA.Gameplay.Tools
{
    public class BuildTool : MapTool
    {
        public BuildingList buildingList;
        public Transform testLight;
        public DecalProjector rangeProjector;
        
        private int _selectedIndex = 0;
        private TextMeshProUGUI _tooltip = null;
        private Town _nearestTown = null;
        
        public override void OnActivate()
        {
            var picker = Instantiate(Toolbar.pickerSetting.gameObject, Toolbar.settingsContainer).GetComponent<PickerSetting>();
            picker.Init("", _selectedIndex);
            picker.SetOptions(buildingList.buildings
                .Select(info => 
                    new PickerSetting.PickerOptionData {Icon = info.icon, Name = info.name})
                .ToArray());
            picker.valueChanged.AddListener(i => _selectedIndex = i);

            _tooltip = Instantiate(Toolbar.tooltipTextPrefab.gameObject, Toolbar.tooltipContainer)
                .GetComponent<TextMeshProUGUI>();
        }

        public override void OnDeactivate()
        {
            testLight.gameObject.SetActive(false);
            rangeProjector.gameObject.SetActive(false);
            _nearestTown?.TownHall.SetShowRange(false);
            _nearestTown = null;
        }

        public override void OnUse()
        {
            var tile = Grid.GetTile(Grid.SelectedTilePosition);
            var buildInfo = buildingList.buildings[_selectedIndex];
            if (!buildInfo.TestAllRequirements(tile, Grid)) return;
                
            var position = Grid.GridToWorld(Grid.SelectedTilePosition);
            var building = Instantiate(
                buildInfo.prefab.gameObject, 
                position, 
                Quaternion.Euler(0, Random.value*360, 0))
                .GetComponent<Building>();
            Town town = null;
            if (TownSystem.Instance.IsInTownRange(tile.GridPosition, out var nearest, out _))
            {
                town = nearest;
            }
            
            building.Initialize(Grid, tile.GridPosition, town);
            town?.RegisterBuilding(building);
            tile.Building = building;
            Grid.SetTile(tile);
        }

        public override void OnAltUse()
        {
            
        }

        private void FixedUpdate()
        {
            if (!isToolActive) return;
            
            var tile = Grid.GetTile(Grid.SelectedTilePosition);

            var buildInfo = buildingList.buildings[_selectedIndex];
            var requirements = buildInfo.GetRequirements();
            
            ResetPreviewTools();
            buildInfo.prefab.OnBuildToolSelected(tile, ShowRange);

            if (buildInfo.requireTown.enabled)
            {
                TownSystem.Instance.IsInTownRange(tile.GridPosition, out var nearestTown, out _);
                if (nearestTown != _nearestTown)
                {
                    _nearestTown?.TownHall.SetShowRange(false);
                    nearestTown?.TownHall.SetShowRange(true);
                    _nearestTown = nearestTown;
                }
            }
            else
            {
                _nearestTown?.TownHall.SetShowRange(false);
                _nearestTown = null;
            }

            var stringBuilder = new StringBuilder();
            var localizedDescription = LocalizationSettings
                .StringDatabase.GetLocalizedString("ToolTooltip_Build");
            stringBuilder.Append(localizedDescription);
            var canBuild = true;
            foreach (var requirement in requirements)
            {
                if(!requirement.enabled) continue;
                var text = requirement.TooltipCaption();
                var pass = requirement.TestTileForRequirement(tile, Grid, buildInfo.prefab);
                canBuild &= pass; // Can't early escape as we still need text from the rest of the tests
                // Can't think of a good way to localize this nicely without smart strings
                // todo: localize this after figuring out localization templates.
                stringBuilder.Append($"<color=\"{(pass ? "green" : "red")}\">{(pass ? "Y" : "N")}</color> {text}");
            }
            
            _tooltip.SetText(stringBuilder);
            
            PhysicalPointer.Instance.SetColor(canBuild ? Color.green : Color.red);

            if (RoadManager.Instance.RoadCount > 0)
            {
                var roadPoint = RoadManager.Instance.FindNearestRoadPoint(tile.GridPosition);
                testLight.gameObject.SetActive(true);
                testLight.position = Grid.GridToWorld(roadPoint.Point);
            }
            else
            {
                testLight.gameObject.SetActive(false);
            }
        }

        private void ResetPreviewTools()
        {
            rangeProjector.gameObject.SetActive(false);
        }

        
        // TODO: Replace this with the generic projector in toolbar
        public void ShowRange(Vector2Int position, float range)
        {
            rangeProjector.gameObject.SetActive(true);
            rangeProjector.transform.position = Grid.GridToWorld(position);
            rangeProjector.ResizeAroundPivot(new Vector3(range*3,(range * 3)/GridSystem.TileWidth, 10));
        }

        public override string GetDisplayName()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("ToolName_Build");
        }
    }
}