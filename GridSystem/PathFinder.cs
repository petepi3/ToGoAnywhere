using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Petepi.TGA.Grid
{
    public static class PathFinder
    {
        private static GridSystem _grid;

        private class Point
        {
            public Point(Vector2Int position, float totalCost, Point parent, float hueristicCost, float distance, float heightDifference)
            {
                Position = position;
                TotalCost = totalCost;
                Parent = parent;
                HueristicCost = hueristicCost;
                Distance = distance;
                HeightDifference = heightDifference;
            }

            public readonly Vector2Int Position;
            public float TotalCost;
            public float HueristicCost;
            public float Distance;
            public Point Parent;
            public float HeightDifference;
        }

        private static bool RoadTest(Tile from, Tile to)
        {
            if (from.Road)
            {
                if (to.Road && to.Road == from.Road) return false;
                if (to.Intersection != null && to.Intersection.ContainsRoad(from.Road)) return false;
            }

            if (from.Intersection != null)
            {
                if (to.Road && from.Intersection.ContainsRoad(to.Road)) return false;
                if (to.Intersection != null &&
                    (to.Intersection.ContainsRoad(from.Intersection.Road1) ||
                     to.Intersection.ContainsRoad(from.Intersection.Road2) ||
                     to.Intersection.ContainsRoad(from.Intersection.Road3))) return false;
            }
            return true;
        }
        

        public static async void FindPath(Vector2Int start, Vector2Int end, float slopePenalty, Action<Path> callback, Func<float, float> sqrt)
        {
            // reference to grid is lazy-loaded and cached
            // TODO: consider implementing a singleton in the grid.
            _grid ??= Object.FindAnyObjectByType<GridSystem>();

            await Task.Run(() =>
            {
                var open = new List<Point>();
                var openDict = new Dictionary<Vector2Int, Point>();
                var closed = new List<Vector2Int>();

                void AddOpen(Point point)
                {
                    open.Add(point);
                    openDict.Add(point.Position, point);
                }
                
                var startPoint = new Point(start, 0, default, 0, 0, 0);
                AddOpen(startPoint);

                while (open.Any())
                {
                    open.Sort((point, point1) => point.TotalCost.CompareTo(point1.TotalCost));
                    var current = open[0];
                    open.RemoveAt(0);
                    var currentTile = _grid.GetTile(current.Position);
                    openDict.Remove(current.Position);
                    closed.Add(current.Position);

                    var neighbourPositions = GridSystem.GetNeighbours(current.Position);
                    foreach (var neighbourPosition in neighbourPositions)
                    {
                        var neighbourTile = _grid.GetTile(neighbourPosition);
                        if (closed.Contains(neighbourPosition) ||
                            !_grid.DoesTileExist(neighbourPosition) ||
                            !neighbourTile.IsPassable) // is on closed list or unwalkable
                        {
                            continue;
                        }

                        if(!RoadTest(currentTile, neighbourTile)) continue;

                        if (neighbourPosition == end)
                        {
                            // found target, backtrack
                            var points = new List<Point>();
                            // we are no longer path finding so the values do not matter
                            points.Add(new Point(end, 0, null, 0, 0, 0)); 
                            var c = current;

                            while (c.Position != start)
                            {
                                points.Add(c);
                                c = c.Parent;
                            }
                            // we are no longer path finding so the values do not matter
                            points.Add(new Point(start, 0, null, 0, 0, 0)); 

                            points.Reverse();

                            var path = new Path
                            {
                                StartPoint = start,
                                EndPoint = end,
                                Points = points.Select(p => p.Position).ToList(),
                                Slope = points.Select(p => p.HeightDifference).Aggregate((h1, h2) => h1 + h2),
                                Length = points.Count
                            };

                            callback(path);
                            return;
                        }

                        var heightDifference = _grid.GetHeightDifference(current.Position, neighbourPosition);
                        var h = GetHCost(current.Position, neighbourPosition, end, sqrt, slopePenalty, heightDifference);
                        var dist = current.Distance + 1;
                        var cost = dist + h;

                        if (!openDict.TryGetValue(neighbourPosition, out var neighbour)) // is not on open list
                        {
                            AddOpen(new Point(neighbourPosition, cost, current, h, dist, heightDifference));
                        }
                        else if (cost < neighbour.TotalCost)
                        {
                            neighbour.HueristicCost = h;
                            neighbour.Distance = dist;
                            neighbour.TotalCost = cost;
                            neighbour.Parent = current;
                            neighbour.HeightDifference = heightDifference;
                        }
                    }
                }
            });
        }

        public static float GetHCost(Vector2Int from, Vector2Int to, Vector2Int target, Func<float, float> sqrt, float slopePenalty, float heightDifference)
        {
            var distance = sqrt(Mathf.Pow(target.x - to.x, 2) + Mathf.Pow(target.y - to.y, 2));
            var heightPenalty = heightDifference*slopePenalty;
            return distance + heightPenalty;
        }
    }
}