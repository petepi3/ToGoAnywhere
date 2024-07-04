using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Petepi.TGA.Gameplay.Tools.ToolSettings
{
    /// <summary>
    /// UI controller for a single option in the PickerSetting
    /// <seealso cref="PickerSetting"/>
    /// </summary>
    public class PickerOption : MonoBehaviour
    {
        public TextMeshProUGUI label;
        public Image iconView;

        /// <summary>
        /// Called after widget was spawned inside pickers' container.
        /// </summary>
        /// <param name="optionName">Name of the option</param>
        /// <param name="iconSprite">Sprite for preview icon</param>
        /// <param name="onClick">Callback for when widget was clicked</param>
        public void Init(string optionName, Sprite iconSprite, Action onClick)
        {
            label.SetText(optionName);
            iconView.sprite = iconSprite;
            GetComponent<Button>().onClick.AddListener(() => onClick());
        }
    }
}