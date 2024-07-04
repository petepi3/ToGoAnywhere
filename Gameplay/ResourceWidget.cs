using System;
using Petepi.TGA.Grid;
using TMPro;
using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    public class ResourceWidget : MonoBehaviour
    {
        private Resource _resource;
        public TextMeshProUGUI label;
        
        public void Initialize(Resource resource)
        {
            _resource = resource;
        }
        
        public void SetAmount(float amount)
        {
            label.SetText($"{Enum.GetName(typeof(Resource), _resource)}: \n{Mathf.RoundToInt(amount)}");
        }
    }
}