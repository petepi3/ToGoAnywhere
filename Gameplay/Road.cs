using System.Collections.Generic;
using System.Linq;
using System.Text;
using Petepi.TGA.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Petepi.TGA.Gameplay
{
    public class Road : MonoBehaviour, ITileInspector
    {
        private Vector2Int[] _points;
        private Vector3[] _worldSpacePoints;
        private LineRenderer _lineRenderer;
        private GridSystem _grid;
        public Path Path;
        public List<RoadIntersection> Intersections = new List<RoadIntersection>();

        private Vector3[] PointsToVertices(Vector2Int[] points)
        {
            var count = (points.Length*2) - 1;
            var verts = new Vector3[count];
            var j = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (i == 0)
                {
                    verts[j] = _grid.GridToWorld(points[i]);
                    j++;
                    continue;
                }

                var current = _grid.GridToWorld(points[i]);
                var prev = verts[j - 1];
                var backwards = (prev - current);
                backwards.y = 0;
                var bias = current.y > prev.y ? 0.03f : -0.03f;
                backwards = backwards.normalized * ((GridSystem.TileWidth * 0.5f)+bias);
                var middle = current + backwards;
                middle.y = Mathf.Max(prev.y, current.y);
                verts[j] = middle;
                verts[j + 1] = current;
                j += 2;
            }

            return verts;
        }

        public Vector2Int FindNearestPoint(Vector2Int point)
        {
            var points = _points.Select(p => (point: p, distance: Vector2Int.Distance(point, p))).ToList();
            points.Sort((p1, p2) => p1.distance.CompareTo(p2.distance)); // ascending
            return points[0].point;
        }
        
        public void Initialize(Path path, GridSystem grid, bool preview = false)
        {
            _grid = grid;
            _lineRenderer = GetComponent<LineRenderer>();
            _points = path.Points.ToArray();
            Path = path;
            if (!preview)
            {
                foreach (var point in _points)
                {
                    var tile = _grid.GetTile(point);
                    tile.Road = this;
                    _grid.SetTile(tile);
                }
            }
            _worldSpacePoints = PointsToVertices(_points);
            _lineRenderer.positionCount = _worldSpacePoints.Length;
            _lineRenderer.SetPositions(_worldSpacePoints.Select(p => p+(Vector3.up*0.01f)).ToArray());
        }

        public void StartInspector(StringBuilder tooltipText)
        {
            var localizedDescription = LocalizationSettings.StringDatabase
                .GetLocalizedString("BuildingTooltip_Road");
            tooltipText.AppendLine(localizedDescription);
        }

        public void EndInspector(ToolbarController toolbar)
        {
            
        }
    }
}