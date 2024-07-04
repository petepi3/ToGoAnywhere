using System;
using System.Collections.Generic;
using System.Linq;
using Petepi.TGA.Grid;
using Unity.VisualScripting;
using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    public class RoadManager : MonoBehaviour
    {
        private static RoadManager _instance;
        private GridSystem _grid;
        private Queue<RoadBuildData> _buildQueue = new Queue<RoadBuildData>();

        private void Awake()
        {
            _instance = FindAnyObjectByType<RoadManager>();
            _grid = FindAnyObjectByType<GridSystem>();
        }

        public static RoadManager Instance => _instance;

        private List<Road> _roads = new List<Road>();
        private List<RoadIntersection> _roadIntersections = new List<RoadIntersection>();

        public int RoadCount => _roads.Count;
        
        public void CreateRoad(Road prefab, Path path)
        {
            var data = new RoadBuildData()
            {
                Prefab = prefab,
                Path = path
            };
            _buildQueue.Enqueue(data);
        }

        public List<RoadIntersection> FindRoadCrossings(Path path)
        {
            var intersections = new List<RoadIntersection>();
            for (int i = 0; i < path.Points.Count; i++)
            {
                foreach (var road in _roads)
                {
                    for (int j = 0; j < road.Path.Length; j++)
                    {
                        if (path.Points[i] == road.Path.Points[j])
                        {
                            intersections.Add(new RoadIntersection
                            {
                                Road1 = road,
                                Road1Position = j,
                                Road2 = null,
                                Road2Position = i,
                                GridPosition = path.Points[i]
                            });
                        }
                    }
                }
            }

            return intersections;
        }
        

        private void FixedUpdate()
        {
            while (_buildQueue.Any())
            {
                var data = _buildQueue.Dequeue();
                var road = Instantiate(data.Prefab.gameObject).GetComponent<Road>();
                road.Initialize(data.Path, _grid);
                var intersections = FindRoadCrossings(data.Path);
                foreach (var intersection in intersections)
                {
                    var match = _roadIntersections
                        .Find(roadIntersection => roadIntersection.GridPosition == intersection.GridPosition);
                    if (match != null)
                    {
                        // intersection already exists, meaning our road is third
                        match.Road3 = road;
                        match.Road3Position = intersection.Road2Position;
                        var tile = _grid.GetTile(intersection.GridPosition);
                        tile.Road = null;
                        _grid.SetTile(tile);
                        road.Intersections.Add(match);
                    }
                    else
                    {
                        intersection.Road2 = road;
                        var tile = _grid.GetTile(intersection.GridPosition);
                        tile.Intersection = intersection;
                        tile.Road = null;
                        _grid.SetTile(tile);
                        _roadIntersections.Add(intersection);
                        road.Intersections.Add(intersection);
                    }
                }
                _roads.Add(road);
            }
        }

        public RoadAndPoint FindNearestRoadPoint(Vector2Int point)
        {
            var points = 
                _roads.Select(road =>
                    {
                        var gridPosition = road.FindNearestPoint(point);
                        return (Road: road, Point: gridPosition,
                            Distance: Vector2Int.Distance(point, gridPosition));
                    }).ToList();
            points.Sort((p1, p2) => p1.Distance.CompareTo(p2.Distance));
            return new RoadAndPoint
            {
                Point = points[0].Point,
                Road = points[0].Road
            };
        }

        private struct RoadBuildData
        {
            public Road Prefab;
            public Path Path;
        }

        public struct RoadAndPoint
        {
            public Road Road;
            public Vector2Int Point;
        }
    }
}