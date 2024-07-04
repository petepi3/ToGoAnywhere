using System;
using System.Collections.Generic;
using Petepi.TGA.Gameplay.Tools.ToolSettings;
using Petepi.TGA.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Petepi.TGA.Gameplay
{
    public class ToolbarController : MonoBehaviour
    {
        public Button buttonPrefab;
        public Transform settingsContainer;
        public Transform tooltipContainer;

        public SliderSetting sliderPrefab;
        public PickerSetting pickerSetting;

        public TextMeshProUGUI tooltipTextPrefab;
        public GameObject spotlightPrefab;
        private List<GameObject> _spotlights = new List<GameObject>();

        private List<MapTool> _tools = new List<MapTool>();
        private List<Button> _buttons = new List<Button>();
        private Dictionary<MapTool, Button> _toolButtons = new Dictionary<MapTool, Button>();
        private MapTool _activeTool;
        private GridSystem _grid;

        private void Awake()
        {
            Init(GetComponents<MapTool>());
            _grid = FindAnyObjectByType<GridSystem>();
        }

        private void Update()
        {
            CollectInput();
        }

        private void CollectInput()
        {
            if (!_activeTool)
            {
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                _activeTool.OnUse();
            }
            
            if (Input.GetMouseButtonDown(1))
            {
                _activeTool.OnAltUse();
            }
        }

        public void Init(MapTool[] tools)
        {
            foreach (var tool in tools)
            {
                _tools.Add(tool);
                var button = AddButton(tool);
                _buttons.Add(button);
                _toolButtons.Add(tool, button);
                tool.Init(this);
            }
        }

        public void RemoveSpotlight(GameObject spotlight)
        {
            _spotlights.Remove(spotlight);
            Destroy(spotlight);
        }

        public GameObject AddSpotlight(Vector2Int gridPosition)
        {
            var spotlightGameObject = Instantiate(spotlightPrefab);
            var tile = _grid.GetTile(gridPosition);
            spotlightGameObject.transform.position = _grid.GridToWorld(tile.GridPosition);
            _spotlights.Add(spotlightGameObject);
            return spotlightGameObject;
        }

        public void ClearSpotlights()
        {
            foreach (var spotlight in _spotlights)
            {
                Destroy(spotlight);
            }
            _spotlights.Clear();
        }

        public void ResetPreviewTools()
        {
            ClearContainers();
            PhysicalPointer.Instance.SetColor(Color.white);
            OutlineUtility.ClearOutlines();
            ClearSpotlights();
        }

        public void SetActiveTool(MapTool tool)
        {
            if (_activeTool)
            {
                _activeTool.OnDeactivate();
                _activeTool.isToolActive = false;
            }
            ResetPreviewTools();
            _activeTool = tool;
            _activeTool.OnActivate();
            _activeTool.isToolActive = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(settingsContainer as RectTransform);
            LayoutRebuilder.MarkLayoutForRebuild(tooltipContainer as RectTransform);
        }

        public void ClearContainers()
        {
            var childCount = settingsContainer.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(settingsContainer.GetChild(i).gameObject);
            }
            
            childCount = tooltipContainer.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(tooltipContainer.GetChild(i).gameObject);
            }
        }
        
        private Button AddButton(MapTool tool)
        {
            var go = Instantiate(buttonPrefab.gameObject, transform);
            var btn = go.GetComponent<Button>();
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            text.text = tool.GetDisplayName();
            btn.onClick.AddListener(() =>
            {
                SetActiveTool(tool);
            });

            return btn;
        }
    }
}