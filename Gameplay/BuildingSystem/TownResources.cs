using System;
using System.Collections.Generic;
using System.Linq;
using Petepi.TGA.Grid;

namespace Petepi.TGA.Gameplay.BuildingSystem
{
    /// <summary>
    /// Information about resources stored in a town.
    /// </summary>
    public class TownResources
    {
        public Dictionary<Resource, float> Resources;

        public TownResources()
        {
            Resources = new Dictionary<Resource, float>();
            foreach (var resource in Utils.AllResources)
            {
                Resources.Add(resource, 0);
            }
        }

        public void AddResource(Resource resource, float amount, ResourceSource source)
        {
            Resources[resource] += amount;
        }
    }
}