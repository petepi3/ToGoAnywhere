using TMPro;
using UnityEngine.UI;

namespace Petepi.TGA.Gameplay.Tools.ToolSettings
{
    public class SliderSetting : ToolbarSetting<float>
    {
        public Slider slider;
        public TextMeshProUGUI label;
        
        public override void Init(string optionName, float initialValue)
        {
            label.SetText(optionName);
            slider.value = initialValue;
            slider.onValueChanged.AddListener(valueChanged.Invoke);
        }
    }
}