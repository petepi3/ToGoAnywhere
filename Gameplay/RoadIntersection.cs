using UnityEngine;

namespace Petepi.TGA.Gameplay
{
    public class RoadIntersection
    {
        public Road Road1;
        public int Road1Position;
        public Road Road2;
        public int Road2Position;
        public Road Road3;
        public int Road3Position;
        public Vector2Int GridPosition;

        public bool ContainsRoad(Road road)
        {
            return (Road1 != null && Road1 == road) || 
                   (Road2 != null && Road2 == road) || 
                   (Road3 != null && Road3 == road);
        }
    }
}