using System.Collections.Generic;
using Petepi.TGA.Grid;
using UnityEngine;

namespace Petepi.TGA.Gameplay.BuildingSystem
{
    public class Town
    {
        private List<Building> _buildings = new List<Building>();
        public TownHall TownHall;
        public Vector2Int Position;
        public TownResources Resources = new TownResources();

        public void RegisterBuilding(Building building)
        {
            _buildings.Add(building);
        }

        /// <summary>
        /// Supply a specified amount of resource from a given source.
        /// </summary>
        /// <param name="resource">type fo resource added</param>
        /// <param name="amount">amount of resource added</param>
        /// <param name="source">source category of the resource</param>
        public void AddResource(Resource resource, float amount, ResourceSource source)
        {
            Resources.AddResource(resource, amount, source);
        }
    }
}