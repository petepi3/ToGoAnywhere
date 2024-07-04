using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Petepi.TGA.Gameplay.BuildingSystem
{
    public class TownSystem : MonoBehaviour
    {
        /// <summary>
        /// prefab to be spawned with a town that displays information about it
        /// </summary>
        /// TODO: consider removing this and displaying this information on tile inspector tool.
        public TownWidgetController widgetPrefab;
        
        // all towns placed on the map
        private List<Town> _towns = new List<Town>();
        
        private static TownSystem _instance;
        public static TownSystem Instance 
        {
            get
            {
                _instance ??= FindAnyObjectByType<TownSystem>();
                return _instance;
            }
        }

        public bool IsInTownRange(Vector2Int position, out Town closestTown, out float distanceToClosestBorder)
        {
            closestTown = null;
            distanceToClosestBorder = Mathf.Infinity;
            
            if (!_towns.Any())
            {
                return false;
            }
            
            for (int i = 0; i < _towns.Count; i++)
            {
                var town = _towns[i];
                // Tiles are not a uniform shape, TODO: replace with a function that takes that into account.
                var distanceToBorder = Vector2Int.Distance(position, town.Position) - town.TownHall.range;
                if (distanceToBorder < distanceToClosestBorder)
                {
                    distanceToClosestBorder = distanceToBorder;
                    closestTown = town;
                    
                    if (distanceToBorder < 0)
                    {
                        // by design town borders should never overlap,
                        // we can return the first town that meets the criteria
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryFindNearestTown(Vector2Int tilePosition, out Town town)
        {
            if (!_towns.Any())
            {
                town = null;
                return false;
            }
            
            // Tiles are not a uniform shape,
            // TODO: replace with a function that takes that into account when measuring distance.
            _towns.Sort((town, town1) =>
            {
                var distance1 = Vector2Int.Distance(tilePosition, town.Position);
                var distance2 = Vector2Int.Distance(tilePosition, town1.Position);
                return distance2.CompareTo(distance1);
            });

            town = _towns[0];
            return true;
        }

        /// <summary>
        /// Test if a town placed at a given tile position doesn't overlap borders of any other town.
        /// </summary>
        /// <param name="tilePosition">tested position</param>
        /// <param name="myTownRange">range of the border for that town that will be placed</param>
        /// <returns>True if no borders are overlapping</returns>
        public bool TestNoBorderOverlap(Vector2Int tilePosition, float myTownRange)
        {
            // Tiles are not a uniform shape,
            // TODO: replace with a function that takes that into account when measuring distance.
            return _towns.All(town =>
                Vector2Int.Distance(tilePosition, town.Position) > town.TownHall.range + myTownRange);
        }

        /// <summary>
        /// Call after placing a townhall to create a town around it.
        /// The townhall will be registered in the new town when it's created.
        /// </summary>
        /// <param name="townHall">townhall that was placed</param>
        /// <returns>The town that was created</returns>
        public Town CreateTown(TownHall townHall)
        {
            var town = new Town
            {
                Position = townHall.gridPosition,
                TownHall = townHall
            };
            town.RegisterBuilding(townHall);
            
            _towns.Add(town);

            var widget = Instantiate(widgetPrefab.gameObject).GetComponent<TownWidgetController>();
            widget.Initialize(town);
            
            return town;
        }
    }
}