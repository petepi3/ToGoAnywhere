using Petepi.TGA.Gameplay.Tools.ToolSettings;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay.Tools
{
    public class DayTimeTool : MapTool
    {
        private float _dayTime = 0.25f;
        public Transform lightTransform;
        public float startRotation = 0;
        public float endRotation = 180;
        
        public override void OnActivate()
        {
            var slider = Instantiate(Toolbar.sliderPrefab.gameObject, Toolbar.settingsContainer).GetComponent<SliderSetting>();
            slider.Init("Time", Mathf.InverseLerp(0, 1, _dayTime));
            slider.valueChanged.AddListener(f =>
            {
                _dayTime = Mathf.Lerp(0, 1, f);
                SetTime(_dayTime);
            });
        }

        private void SetTime(float time)
        {
            lightTransform.localRotation = Quaternion.Euler(Mathf.Lerp(startRotation, endRotation, time), 0, 0);
        }

        public override void OnDeactivate()
        {
            
        }

        public override void OnUse()
        {
            
        }

        public override void OnAltUse()
        {
            
        }

        public override string GetDisplayName()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString("ToolName_DayTime");
        }
    }
}