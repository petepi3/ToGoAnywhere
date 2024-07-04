using System;
using System.Linq;
using System.Text;
using Petepi.TGA.Gameplay.BuildingSystem.Requirements;
using Petepi.TGA.Gameplay.Tools.ToolSettings;
using Petepi.TGA.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Petepi.TGA.Gameplay.Tools
{
    public class RoadTool : MapTool
    {
        private Vector2Int _startPostion;
        private bool _startSet;
        private Vector2Int _endPosition;
        private bool _endSet;
        private float _slopePenalty = 50;
        public float maxSlopePenalty = 10;
        public Road roadPrefab;

        public EmptyTileTest emptyTest;
        public NotOnWaterTest waterTest;

        private Road _previewRoad;
        private bool _pathFindingOngoing;
        private bool _pointsUpdated;
        private bool _paramsChanged;
        private float _averageSlope;
        private Path _path;

        private TextMeshProUGUI inspectorTextElement;

        private string GenerationText => LocalizationSettings
            .StringDatabase.GetLocalizedString(_pathFindingOngoing ? "Generating":"GenerationDone");
        private string SlopeText => _pathFindingOngoing ? $"Average Slope:\n  Not generated\n " : $"Total height difference:\n  {_averageSlope}\n ";
        
        public override void OnActivate()
        {
            _startSet = false;
            _endSet = false;
            _pathFindingOngoing = false;
            _paramsChanged = false;
            _pointsUpdated = false;

            inspectorTextElement = Instantiate(Toolbar.tooltipTextPrefab, Toolbar.tooltipContainer);
            UpdateTooltipText();
            // hack to fix an issue with text mesh pro containers only updating layout after a frame
            // somehow the rebuild after the tool is activated in toolbar was insufficient to avoid UI jitter.
            LayoutRebuilder.ForceRebuildLayoutImmediate(Toolbar.tooltipContainer as RectTransform);
            var slopeSlider = Instantiate(Toolbar.sliderPrefab.gameObject, Toolbar.settingsContainer).GetComponent<SliderSetting>();
            slopeSlider.Init("Slope penalty", Mathf.InverseLerp(1, maxSlopePenalty, _slopePenalty));
            slopeSlider.valueChanged.AddListener(f =>
            {
                _slopePenalty = Mathf.Lerp(1, maxSlopePenalty, f);
                _paramsChanged = true;
            });
        }

        private void UpdateTooltipText()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(LocalizationSettings
                .StringDatabase.GetLocalizedString("ToolTooltip_Road"));
            stringBuilder.AppendLine(SlopeText);
            stringBuilder.AppendLine(GenerationText);
            inspectorTextElement.SetText(stringBuilder);
        }

        public override void OnDeactivate()
        {
            if (_previewRoad)
            {
                Destroy(_previewRoad.gameObject);
            }
        }

        public override void OnUse()
        {
            var position = Grid.SelectedTilePosition;
            var tile = Grid.GetTile(position);
            if ((_endSet && position == _endPosition) || !TestTile(tile)) return;
            _startPostion = position;
            _startSet = true;
            _paramsChanged = true;
        }
        
        public override void OnAltUse()
        {
            var position = Grid.SelectedTilePosition;
            var tile = Grid.GetTile(position);
            if ((_startSet && position == _startPostion) || !TestTile(tile)) return;
            _endPosition = position;
            _endSet = true;
            _paramsChanged = true;
        }

        private bool TestTile(Tile tile)
        {
            return tile.IsPassable 
                   && emptyTest.TestTileForRequirement(tile, Grid, null) 
                   && waterTest.TestTileForRequirement(tile, Grid, null);
        }

        private void PreviewRoad()
        {
            if (!(_startSet && _endSet) || _pathFindingOngoing)
            {
                return;
            }
            
            if (!_previewRoad)
            {
                _previewRoad = Instantiate(roadPrefab.gameObject).GetComponent<Road>();
            }

            _paramsChanged = false;
            _pathFindingOngoing = true;
            PathFinder.FindPath(_startPostion, _endPosition, _slopePenalty, path =>
            {
                _path = path;
                _pathFindingOngoing = false;
                _pointsUpdated = true;
                _averageSlope = path.Slope / path.Length;
            }, Utils.AprSqrt2);
        }
        
        private void Update()
        {
            if (!isToolActive) return;
            if (Input.GetKeyDown(KeyCode.Return) && _path != null && !_pathFindingOngoing)
            {
                RoadManager.Instance.CreateRoad(roadPrefab, _path);
                _startSet = false;
                _endSet = false;
                _pathFindingOngoing = false;
                _paramsChanged = false;
                _pointsUpdated = false;
                Destroy(_previewRoad.gameObject);
                Toolbar.ClearSpotlights();
            }
        }

        private void FixedUpdate()
        {
            if (!isToolActive) return;
            
            if (_pointsUpdated)
            {
                _previewRoad.Initialize(_path, Grid, true);
                Toolbar.ClearSpotlights();
                var intersections = RoadManager.Instance.FindRoadCrossings(_path);
                foreach (var intersection in intersections)
                {
                    Toolbar.AddSpotlight(intersection.GridPosition);
                }
                _pointsUpdated = false;
            }
            if (_paramsChanged)
            {
                PreviewRoad();
            }
            
            
            UpdateTooltipText();
        }

        public override string GetDisplayName()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("ToolName_Road");
        }
    }
}