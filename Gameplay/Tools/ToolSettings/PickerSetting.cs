using UnityEngine;

namespace Petepi.TGA.Gameplay.Tools.ToolSettings
{
    public class PickerSetting : ToolbarSetting<int>
    {
        /// <summary>
        /// Container in this widget that options will be spawned in
        /// </summary>
        public Transform containerTransform;
        /// <summary>
        /// Prefab of a widget for a single option
        /// </summary>
        public PickerOption optionPrefab;
        
        public override void Init(string optionName, int initialValue)
        {
            
        }

        public void SetOptions(PickerOptionData[] options)
        {
            for (int i = 0; i < options.Length; i++)
            {
                SpawnOption(options[i], i);
            }
        }

        private void SpawnOption(PickerOptionData optionData, int index)
        {
            var option = Instantiate(optionPrefab, containerTransform);
            option.Init(optionData.Name, optionData.Icon, () => valueChanged.Invoke(index));
        }

        public struct PickerOptionData
        {
            public string Name;
            public Sprite Icon;
        }
    }
}