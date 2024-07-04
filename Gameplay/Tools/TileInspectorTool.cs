using System.Text;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Petepi.TGA.Gameplay.Tools
{
    public class TileInspectorTool : MapTool
    {
        private ITileInspector _inspector = null;
        
        public override void OnActivate()
        {
            
        }

        public override void OnDeactivate()
        {
            
        }

        public override void OnUse()
        {
            var tile = Grid.GetTile(Grid.SelectedTilePosition);
            if (tile.TryGetInspector(out var inspector))
            {
                _inspector?.EndInspector(Toolbar);
                Toolbar.ClearContainers();
                var tooltipText = Instantiate(Toolbar.tooltipTextPrefab, Toolbar.tooltipContainer);
                var stringBuilder = new StringBuilder();
                inspector.StartInspector(stringBuilder);
                tooltipText.SetText(stringBuilder);
                _inspector = inspector;
                OutlineUtility.ClearOutlines();
                OutlineUtility.AddOutline(((MonoBehaviour)_inspector).gameObject, OutlineType.Good);
            }
            else
            {
                _inspector = null;
                Toolbar.ClearContainers();
                OutlineUtility.ClearOutlines();
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(Toolbar.tooltipContainer as RectTransform);
        }

        public override void OnAltUse()
        {
            
        }

        public override string GetDisplayName()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("ToolName_Inspector");
        }
    }
}