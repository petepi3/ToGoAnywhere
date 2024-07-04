using System;
using System.Collections.Generic;
using Petepi.TGA.Gameplay.BuildingSystem;
using Petepi.TGA.Grid;
using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    public class TownWidgetController : MonoBehaviour
    {
        public Transform resourceContainer;
        public ResourceWidget resourceWidgetPrefab;
        private Town _town;
        private readonly Dictionary<Resource, ResourceWidget> _resourceWidgets = new Dictionary<Resource, ResourceWidget>();
        
        public void Initialize(Town town)
        {
            _town = town;
            transform.position = town.TownHall.transform.position;
            foreach (var resource in Utils.AllResources)
            {
                AddResourceWidget(resource, resourceWidgetPrefab);
            }
        }

        private void Update()
        {
            foreach (var kvp in _town.Resources.Resources)
            {
                _resourceWidgets[kvp.Key].SetAmount(kvp.Value);
            }
        }

        private void Awake()
        {
            var mainCamera = Camera.main;
            transform.rotation = mainCamera.transform.rotation;
        }

        private ResourceWidget AddResourceWidget(Resource resource, ResourceWidget prefab)
        {
            var widget = Instantiate(prefab.gameObject, resourceContainer).GetComponent<ResourceWidget>();
            widget.Initialize(resource);
            _resourceWidgets.Add(resource, widget);
            return widget;
        }
    }
}