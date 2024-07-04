using System.Collections.Generic;
using UnityEngine;

namespace Petepi.TGA.Grid
{
    public class Path
    {
        public Vector2Int StartPoint;
        public Vector2Int EndPoint;
        public List<Vector2Int> Points;
        public float Slope;
        public float Length;
    }
}